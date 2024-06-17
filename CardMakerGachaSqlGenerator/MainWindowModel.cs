using CardMakerGachaSqlGenerator.Base;
using CardMakerGachaSqlGenerator.Base.Resp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using static CardMakerGachaSqlGenerator.Base.Gacha;

namespace CardMakerGachaSqlGenerator
{
    public partial class MainWindowModel : ObservableObject
    {
        public Gacha.GachaKind[] GachaKindEnums { get; } = Enum.GetValues<Gacha.GachaKind>();
        public Gacha.GachaType[] GachaTypeEnums { get; } = Enum.GetValues<Gacha.GachaType>();

        [ObservableProperty]
        private bool isEnable = true;

        [ObservableProperty]
        private int inputCardId;

        [ObservableProperty]
        private ObservableCollection<Gacha> gachaList = new();

        [ObservableProperty]
        private Gacha currentSelectedGacha;

        [ObservableProperty]
        private Config config = new();

        [ObservableProperty]
        private ObservableCollection<GachaCard> currentSelectedGachaCards = new();

        [ObservableProperty]
        private GachaCard currentSelectedGachaCard;

        private string GetApiEndPointUrl(string apiName)
        {
            var url = Config.AquaURL;
            if (!url.EndsWith("/"))
                url += "/";
            url += $"OngekiServlet/{Config.RomVersion}/{Config.ClientId}/{apiName}";
            //url += $"OngekiServlet/{apiName}";
            return url;
        }

        public string ConfigFilePath => Path.Combine(Path.GetDirectoryName(typeof(MainWindowModel).Assembly.Location), "config.json");

        public MainWindowModel()
        {
            try
            {
                var configStr = File.ReadAllText(ConfigFilePath);
                Config = JsonSerializer.Deserialize<Config>(configStr) ?? new();
                IsEnable = false;
                CardManager.Reload(Config.PackagePath).ContinueWith(x => IsEnable = true).ConfigureAwait(false);
            }
            catch
            {

            }
        }

        [RelayCommand]
        private async Task OnLoadCardInfo()
        {
            if (!Directory.Exists(Config.PackagePath))
            {
                MessageBox.Show("PackagePath没填写或者不存在");
                return;
            }

            IsEnable = false;
            await CardManager.Reload(Config.PackagePath);
            IsEnable = true;
            MessageBox.Show("读取成功");
        }

        [RelayCommand]
        private void OnSaveConfig()
        {
            try
            {
                var configStr = JsonSerializer.Serialize(Config, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                });
                File.WriteAllText(ConfigFilePath, configStr);
                MessageBox.Show("保存成功");

            }
            catch (Exception e)
            {
                MessageBox.Show($"保存失败: {e.Message}");
            }
        }

        [RelayCommand]
        private async Task OnCleanAndFetchGachaFromServer()
        {
            //清空并从服务器获取卡池
            GachaList.Clear();

            var gachaUrl = GetApiEndPointUrl("GetGameGachaApi");

            var resp = await ApiCaller.PostJsonAsync<GetGameGachaApiResp>(gachaUrl, new { });
            var gachaList = resp.GameGachaList ?? Enumerable.Empty<Gacha>();

            var cardUrl = GetApiEndPointUrl("GetGameGachaCardByIdApi");
            var respArray = gachaList.Select(x => ApiCaller.PostJsonAsync<GetGachaCardApiResp>(cardUrl, new
            {
                gachaId = x.GachaId,
                placeId = 100,
                clientId = "",
                isDetail = false,
                regionId = 2,
            }).AsTask()).ToArray();

            await Task.WhenAll(respArray);

            var gachaCardList = respArray.Select(x => x.Result).ToList();

            foreach ((var gacha, var idx) in gachaList.Select((x, i) => (x, i)))
            {
                GachaList.Add(gacha);
                if (gachaCardList.ElementAtOrDefault(idx)?.GameGachaCardList is GachaCard[] cardList)
                {
                    foreach (var card in cardList)
                        gacha.Cards.Add(card);
                }
            }
        }

        [RelayCommand]
        private void OnCreateNewGacha()
        {
            //新建卡池
            var newGacha = new Gacha()
            {
                GachaName = "新的卡池"
            };
            GachaList.Add(newGacha);
        }

        [RelayCommand]
        private void OnGenerateGachaSql()
        {
            //生成卡池sql
            if (CurrentSelectedGacha is null)
            {
                MessageBox.Show("请先选择卡池");
                return;
            }

            var sql = SqlGenerator.GenerateInsert("ongeki_game_gacha", CurrentSelectedGacha);

            ShowContentAsNotepad(sql);
        }

        [RelayCommand]
        private void OnGenerateGachaCardSql()
        {
            //生成卡池卡片sql
            if (CurrentSelectedGacha is null)
            {
                MessageBox.Show("请先选择卡池");
                return;
            }

            var sb = new StringBuilder();

            //打印相关信息
            sb.AppendLine($"-- Created for gacha id: {CurrentSelectedGacha.GachaId}, gacha name: {CurrentSelectedGacha.GachaName}");
            sb.AppendLine();

            foreach (var r in CurrentSelectedGacha.Cards
                .DistinctBy(x => x.CardId)
                .AsParallel()
                .Select(x => (SqlGenerator.GenerateInsert("ongeki_game_gacha_card", x), x.CardId))
                .OrderBy(x => x.CardId))
            {
                var cardInfo = CardManager.GetCardInfo(r.CardId);
                if (cardInfo is not null)
                    sb.AppendLine($"-- {cardInfo.Name}");
                sb.AppendLine(r.Item1);
                sb.AppendLine();
            }

            ShowContentAsNotepad(sb.ToString());
        }

        [RelayCommand]
        private void OnDeleteSelectedGachaCards()
        {
            foreach (var item in CurrentSelectedGachaCards.ToArray())
                CurrentSelectedGacha.Cards.Remove(item);
        }

        [RelayCommand]
        private void OnAddGachaCardManually()
        {
            if (CardManager.GetCardInfo(InputCardId) is not CardInfo cardInfo)
            {
                MessageBox.Show("找不到此卡");
                return;
            }

            var gachaCard = new GachaCard()
            {
                CardId = cardInfo.Id,
                GachaId = CurrentSelectedGacha.GachaId,
                Rarity = cardInfo.Rarity,
                Weight = 1
            };
            CurrentSelectedGacha.Cards.Insert(0, gachaCard);
        }

        [RelayCommand]
        private async Task OnDragEnd(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var file in files)
            {
                try
                {
                    var cardInfo = await CardManager.ParseCardXmlFile(file);
                    var gachaCard = new GachaCard()
                    {
                        CardId = cardInfo.Id,
                        GachaId = CurrentSelectedGacha.GachaId,
                        Rarity = cardInfo.Rarity,
                        Weight = 1
                    };

                    CurrentSelectedGacha.Cards.Insert(0, gachaCard);
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        private void ShowContentAsNotepad(string content)
        {
            content = $"""
                -- AUTO GENERATED BY CardMakerGachaSqlGenerator 
                -- DATE {DateTime.Now}
                {Environment.NewLine}
                """ + content;
            var filePath = Path.GetTempFileName() + ".txt";
            File.WriteAllText(filePath, content);
            Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            });
        }
    }
}
