﻿using CardMakerGachaSqlGenerator.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CardMakerGachaSqlGenerator
{
    public static class CardManager
    {
        public static ImmutableDictionary<int, string> CardImageFileMap { get; private set; }
        public static ImmutableDictionary<int, CardInfo> CardInfoMap { get; private set; }

        public static CardInfo GetCardInfo(int id)
        {
            if (CardInfoMap?.TryGetValue(id, out var info) ?? false)
                return info;

            return default;
        }

        public static string GetCardName(int id)
        {
            if (CardInfoMap?.TryGetValue(id, out var info) ?? false)
                return info.Name;

            return "<ID-NOT-EXIST>";
        }

        public static async Task Reload(string packagePath)
        {
            var cardXmlFiles = Directory.GetFiles(packagePath, "Card.xml", SearchOption.AllDirectories);
            var cardImageFiles = Directory.GetFiles(packagePath, "ui_card_*_s", SearchOption.AllDirectories);

            var result = new ConcurrentBag<CardInfo>();
            var imageResult = new ConcurrentDictionary<int, string>();

            async ValueTask parseCardXml(string file)
            {
                var r = await ParseCardXmlFile(file);
                result.Add(r);
            }
            var reg = new Regex(@"ui_card_(\d+)_s");
            ValueTask parseImageFileName(string file)
            {
                var fileName = Path.GetFileName(file);
                var match = reg.Match(fileName);
                if (!match.Success)
                    return ValueTask.CompletedTask;
                var id = int.Parse(match.Groups[1].Value);
                imageResult[id] = file;
                return ValueTask.CompletedTask;
            }

            var task1 = Parallel.ForEachAsync(cardXmlFiles, (file, ct) => parseCardXml(file));
            var task2 = Parallel.ForEachAsync(cardImageFiles, (file, ct) => parseImageFileName(file));
            await Task.WhenAll([task1, task2]);

            CardImageFileMap = imageResult.ToImmutableDictionary();
            CardInfoMap = result.ToImmutableDictionary(x => x.Id, x => x);
        }

        public static async ValueTask<CardInfo> ParseCardXmlFile(string filePath)
        {
            using var fs = File.OpenRead(filePath);
            var cardXml = await XDocument.LoadAsync(fs, LoadOptions.None, default);

            var str = cardXml.XPathSelectElement($@"//Name[1]/str[1]").Value;
            var id = int.Parse(cardXml.XPathSelectElement($@"//Name[1]/id[1]").Value);
            var rarityStr = cardXml.XPathSelectElement($@"//Rarity[1]").Value;

            var rarity = rarityStr.ToLower() switch
            {
                "n" => 1,
                "r" => 2,
                "sr" => 3,
                "ssr" => 4,
                _ => 0,
            };

            var card = new CardInfo(str, id, rarity);
            return card;
        }

        public static string GetCardAssetFilePath(int cardId)
        {
            if (CardImageFileMap?.TryGetValue(cardId, out var filePath) ?? false)
                return filePath;

            return default;
        }
    }
}
