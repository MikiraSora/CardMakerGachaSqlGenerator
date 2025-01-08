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
        private ObservableCollection<Theater> theaterList = new();

        [ObservableProperty]
        private Theater currentSelectedTheater;

        [ObservableProperty]
        private SubTheater currentSelectedSubTheater;

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

        [ObservableProperty]
        private Supply currentSupply = new();

        [ObservableProperty]
        private int inputUserAimeId;

        [ObservableProperty]
        private int inputSubTheaterId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSSR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSRPlus))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardN))]
        private float chanceCardSSR;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSSR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSRPlus))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardN))]
        private float chanceCardSRPlus;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSSR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSRPlus))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardN))]
        private float chanceCardSR;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSSR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSRPlus))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardN))]
        private float chanceCardR;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSSR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSRPlus))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardSR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardR))]
        [NotifyPropertyChangedFor(nameof(AcutalChanceCardN))]
        private float chanceCardN;

        public float chanceTotal => ChanceCardSSR + ChanceCardSRPlus + ChanceCardSR + ChanceCardR + ChanceCardN;

        public float AcutalChanceCardSSR => ChanceCardSSR * 100f / chanceTotal;
        public float AcutalChanceCardSRPlus => ChanceCardSRPlus * 100f / chanceTotal;
        public float AcutalChanceCardSR => ChanceCardSR * 100f / chanceTotal;
        public float AcutalChanceCardR => ChanceCardR * 100f / chanceTotal;
        public float AcutalChanceCardN => ChanceCardN * 100f / chanceTotal;


        private string GetApiEndPointUrl(string apiName)
        {
            var url = Config.AquaURL;
            if (!url.EndsWith("/"))
                url += "/";
            //url += $"OngekiServlet/{Config.RomVersion}/{Config.ClientId}/{apiName}";
            url += $"OngekiServlet/{apiName}";
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

                Task.WhenAll([
                    GameResourceManager.ReloadSDDTResource(Config.PackagePath),
                    GameResourceManager.ReloadSDEDResource(Config.SdedPackagePath)])
                    .ContinueWith(x => IsEnable = true)
                    .ConfigureAwait(false);
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
            await Task.WhenAll([GameResourceManager.ReloadSDDTResource(Config.PackagePath), GameResourceManager.ReloadSDEDResource(Config.SdedPackagePath)]);
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
                var cardInfo = GameResourceManager.GetCardInfo(r.CardId);
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
            if (GameResourceManager.GetCardInfo(InputCardId) is not CardInfo cardInfo)
            {
                MessageBox.Show("找不到此卡");
                return;
            }

            var gachaCard = new GachaCard()
            {
                CardId = cardInfo.Id,
                GachaId = CurrentSelectedGacha.GachaId,
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
                    var cardInfo = await GameResourceManager.ParseCardXmlFile(file);
                    var gachaCard = new GachaCard()
                    {
                        CardId = cardInfo.Id,
                        GachaId = CurrentSelectedGacha.GachaId,
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

        [RelayCommand]
        private async Task OnSupplyCardListDragEnd(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var file in files)
            {
                try
                {
                    var cardInfo = await GameResourceManager.ParseCardXmlFile(file);
                    var gachaCard = new SupplyGachaCard()
                    {
                        CardId = cardInfo.Id,
                        SupplyId = CurrentSupply?.SupplyId ?? 0
                    };

                    CurrentSupply.Cards.Insert(0, gachaCard);
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        [RelayCommand]
        private async Task OnFetchSupplyFromServer()
        {
            if (InputUserAimeId == 0)
            {
                MessageBox.Show("请先填写用户AimeId");
                return;
            }

            var supplyUrl = GetApiEndPointUrl("CMGetUserGachaSupplyApi");

            var resp = await ApiCaller.PostJsonAsync<CMGetUserGachaSupplyApiResp>(supplyUrl, new
            {
                userId = InputUserAimeId
            });
            var supply = new Supply()
            {
                SupplyId = resp.SupplyId
            };
            foreach (var cardId in resp.SupplyCardList)
            {
                var supplyCard = new SupplyGachaCard()
                {
                    CardId = cardId,
                    SupplyId = supply.SupplyId
                };
                supply.Cards.Add(supplyCard);
            }

            CurrentSupply = supply;
        }

        [RelayCommand]
        private void OnCreateNewSupply()
        {
            CurrentSupply = new()
            {
                CreateDate = DateTime.Now,
            };
        }

        [RelayCommand]
        private async Task OnGenerateSupplySql()
        {
            if (CurrentSupply.UserId == 0)
            {
                MessageBox.Show("请先填写用户UserId");
                return;
            }
            if (CurrentSupply.Cards.Count == 0)
            {
                MessageBox.Show("卡池为空");
                return;
            }

            var sb = new StringBuilder();

            //打印相关信息
            sb.AppendLine($"-- Created for supply id: {CurrentSupply.SupplyId}");
            sb.AppendLine(SqlGenerator.GenerateInsert("ongeki_user_gacha_supply", CurrentSupply));
            sb.AppendLine();

            foreach (var r in CurrentSupply.Cards
                .DistinctBy(x => x.CardId)
                .AsParallel()
                .Select(x =>
                {
                    x.SupplyId = CurrentSupply.SupplyId;
                    return (SqlGenerator.GenerateInsert("ongeki_user_gacha_card_supply", x), x.CardId);
                })
                .OrderBy(x => x.CardId))
            {
                var cardInfo = GameResourceManager.GetCardInfo(r.CardId);
                if (cardInfo is not null)
                    sb.AppendLine($"-- {cardInfo.Name}");
                sb.AppendLine(r.Item1);
                sb.AppendLine();
            }

            ShowContentAsNotepad(sb.ToString());
        }


        [RelayCommand]
        private async Task OnCleanAndFetchTheaterFromServer()
        {
            TheaterList.Clear();

            var theaterUrl = GetApiEndPointUrl("GetGameTheaterApi");
            var resp = await ApiCaller.PostJsonAsync<GetGameTheaterApiResp>(theaterUrl, new
            {
                isAllTheater = true
            });

            var theaterList = resp.GameTheaterList ?? Enumerable.Empty<Theater>();

            foreach (var theater in theaterList)
                TheaterList.Add(theater);
        }

        [RelayCommand]
        private void OnCreateNewTheater()
        {
            var newTheater = new Theater()
            {
                TheaterName = "新的漫画"
            };
            TheaterList.Add(newTheater);
        }

        [RelayCommand]
        private void OnGenerateTheaterSql()
        {
            if (CurrentSelectedTheater is null)
            {
                MessageBox.Show("请先新建或选择Theater");
                return;
            }
            if (CurrentSelectedTheater.GameSubTheaterList is null)
            {
                MessageBox.Show("漫画页面列表为空");
                return;
            }

            var sb = new StringBuilder();

            CurrentSelectedTheater.SerializedGameSubTheaterList = string.Join(",", CurrentSelectedTheater.GameSubTheaterList.Select(x => x.Id));
            sb.AppendLine(SqlGenerator.GenerateInsert("ongeki_game_theater", CurrentSelectedTheater));

            ShowContentAsNotepad(sb.ToString());
        }


        [RelayCommand]
        private void OnSubTheaterListDragEnd(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var reg = new Regex(@"ui_manga_(\d+)");

            var addList = new List<SubTheater>();
            foreach (var file in files)
            {
                try
                {
                    var fileName = Path.GetFileName(file);
                    var match = reg.Match(fileName);
                    if (!match.Success)
                        continue;
                    var id = int.Parse(match.Groups[1].Value);

                    var subTheater = new SubTheater()
                    {
                        TheaterId = CurrentSelectedTheater.TheaterId,
                        Id = id,
                        No = CurrentSelectedTheater.GameSubTheaterList.Count
                    };

                    addList.Add(subTheater);
                }
                catch
                {

                }
            }

            var copyList = CurrentSelectedTheater.GameSubTheaterList.Concat(addList);
            var chars = string.Join(",", copyList.Select(x => x.Id));

            if (chars.Length > 255)
            {
                MessageBox.Show($"要添加的页面太多(idStr.Length({chars.Length}) > 255)");
                return;
            }

            foreach (var add in addList)
                CurrentSelectedTheater.GameSubTheaterList.Add(add);
        }

        [RelayCommand]
        private void OnApplyChangeToWeight()
        {
            if (MessageBox.Show("是否要重新计算所有卡片weight值成符合概率的", "提醒", button: MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            var countMap = CurrentSelectedGacha.Cards.GroupBy(x => x.Rarity).ToDictionary(x => x.Key, x => x.Count());

            var totalWeight = 100 * countMap.Values.Max();

            var ssrCount = countMap.GetValueOrDefault(4, 0);
            var srCount = countMap.GetValueOrDefault(3, 0);
            var rCount = countMap.GetValueOrDefault(2, 0);
            var nCount = countMap.GetValueOrDefault(1, 0);
            var srpCount = countMap.GetValueOrDefault(0, 0);

            var ssrChance = (int)(AcutalChanceCardSSR * totalWeight / ssrCount);
            var srpChance = (int)(AcutalChanceCardSRPlus * totalWeight / srpCount);
            var srChance = (int)(AcutalChanceCardSR * totalWeight / srCount);
            var rChance = (int)(AcutalChanceCardR * totalWeight / rCount);
            var nChance = (int)(AcutalChanceCardN * totalWeight / nCount);

            foreach (var card in CurrentSelectedGacha.Cards)
            {
                switch (card.Rarity)
                {
                    case 4:
                        card.Weight = ssrChance;
                        break;
                    case 3:
                        card.Weight = srChance;
                        break;
                    case 2:
                        card.Weight = rChance;
                        break;
                    case 1:
                        card.Weight = nChance;
                        break;
                    case 0:
                        card.Weight = srpChance;
                        break;
                }
            }

            MessageBox.Show("计算完成");
        }
    }
}
