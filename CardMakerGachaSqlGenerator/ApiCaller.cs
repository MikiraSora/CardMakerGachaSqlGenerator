using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CardMakerGachaSqlGenerator
{
    public static class ApiCaller
    {
        static HttpClient client = new HttpClient();
        static JsonSerializerOptions opt;

        private class TitleString2DateTimeConverter : JsonConverter<DateTime>
        {
            public const string Format = "yyyy-MM-dd HH:mm:ss";
            public static Regex Regex = new Regex(@"\d+-\d+-\d+\s+\d+:\d+:\d+");

            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var dateString = reader.GetString();
                    return ParseStr(dateString);
                }

                return JsonSerializer.Deserialize<DateTime>(ref reader, options);
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(Format));
            }

            public static DateTime ParseStr(string dateString)
            {
                var sliceStr = Regex.Match(dateString).Value;
                if (DateTime.TryParseExact(sliceStr, Format, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    return result;
                return default;
            }
        }


        static ApiCaller()
        {
            opt = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
            opt.Converters.Add(new TitleString2DateTimeConverter());
        }

        public static async ValueTask<T> PostJsonAsync<T>(string url, object body)
        {
            var respMsg = await client.PostAsJsonAsync(url, body);

            var resp2 = await respMsg.Content.ReadAsByteArrayAsync();
            using var deflatStream = new ZLibStream(new MemoryStream(resp2), CompressionMode.Decompress);
            var ms = new MemoryStream();
            await deflatStream.CopyToAsync(ms);
            var str = Encoding.UTF8.GetString(ms.ToArray());
            var ret = JsonSerializer.Deserialize<T>(str, opt);
            return ret;
        }
    }
}
