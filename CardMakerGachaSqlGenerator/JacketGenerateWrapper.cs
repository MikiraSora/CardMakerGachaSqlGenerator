using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TexturePlugin;

namespace CardMakerGachaSqlGenerator
{
    public static class JacketGenerateWrapper
    {
        public class ImageData
        {
            public ImageData(int width, int height, byte[] data)
            {
                Width = width;
                Height = height;
                Data = data;
            }

            public int Width { get; }
            public int Height { get; }
            public string Name { get; }

            /// <summary>
            /// Pure RGBA32 array
            /// </summary>
            public byte[] Data { get; }
        }

        public static Task<ImageData> GetMainImageDataAsync(byte[] abFileData, string filePath)
        {
            return Task.Run(async () =>
            {
                var assetManager = new AssetsManager();

                if (abFileData is null)
                    abFileData = await File.ReadAllBytesAsync(filePath);
                using var ms = new MemoryStream(abFileData);

                var assetBundleFile = assetManager.LoadBundleFile(ms, filePath);
                var assetsFile = assetManager.LoadAssetsFileFromBundle(assetBundleFile, 0);
                var assetsTable = assetsFile.table;

                var assetInfos = assetsTable.GetAssetsOfType(0x1C);
                foreach (var assetInfo in assetInfos)
                {
                    var baseField = assetManager.GetTypeInstance(assetsFile.file, assetInfo).GetBaseField();

                    var width = baseField["m_Width"].GetValue().AsInt();
                    var height = baseField["m_Height"].GetValue().AsInt();
                    var format = (TextureFormat)baseField["m_TextureFormat"].GetValue().AsInt();

                    var picData = default(byte[]);
                    var beforePath = baseField["m_StreamData"]["path"].GetValue().AsString();

                    //try get texture data from stream data
                    if (!string.IsNullOrWhiteSpace(beforePath))
                    {
                        string searchPath = beforePath;
                        var offset = baseField["m_StreamData"]["offset"].GetValue().AsUInt();
                        var size = baseField["m_StreamData"]["size"].GetValue().AsUInt();

                        if (searchPath.StartsWith("archive:/"))
                            searchPath = searchPath.Substring("archive:/".Length);

                        searchPath = Path.GetFileName(searchPath);
                        var reader = assetBundleFile.file.reader;
                        var dirInf = assetBundleFile.file.bundleInf6.dirInf;

                        for (int i = 0; i < dirInf.Length; i++)
                        {
                            var info = dirInf[i];
                            if (info.name == searchPath)
                            {
                                reader.Position = assetBundleFile.file.bundleHeader6.GetFileDataOffset() + info.offset + offset;
                                picData = reader.ReadBytes((int)size);
                                break;
                            }
                        }
                    }

                    //try get texture data from image data field
                    if ((picData?.Length ?? 0) == 0)
                    {
                        var imageDataField = baseField["image data"];
                        var arr = imageDataField.GetValue().value.asByteArray;
                        picData = new byte[arr.size];
                        Array.Copy(arr.data, picData, arr.size);
                    }

                    if ((picData?.Length ?? 0) == 0)
                        continue;

                    byte[] decData = TextureEncoderDecoder.Decode(picData, width, height, format);

                    if (decData == null)
                        continue;

                    return new ImageData(width, height, decData);
                }

                return default;
            });
        }
    }
}
