using CryptoOculus.Services;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoOculus
{
    public class Testing(IHttpClientFactory httpClientFactory, ILogger<Testing> logger)
    {
        public async Task Request()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://spotapi.coinw.com/asset/v1/api/address/list").WithVersion();
            request.Content = new StringContent(JsonSerializer.Serialize(new { coinId = 1739 }), Encoding.UTF8, "application/json");
            request.Headers.Add("logintoken", "71BF1655423FD27D59962AE975E7F4CCweb_1759665547836_26344426");
            request.Options.Set(HttpOptionKeys.KeyedConcurrencyLimitName, "CoinwLoginTokenLimit");
            request.Options.Set(HttpOptionKeys.KeyedConcurrencyLimitKey, "71BF1655423FD27D59962AE975E7F4CCweb_1759665547836_26344426");
            HttpResponseMessage response = await httpClientFactory.CreateClient(ClientService.RotatingProxyClient("testt")).SendAsync(request);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        public async Task Main()
        {
            while (true)
            {
                await Task.Delay(200);
                DateTime start = DateTime.Now;
                List<Task> tasks = [];

                for (int a = 0; a < 100; a++)
                {
                    tasks.Add(Request());
                }

                await Task.WhenAll(tasks);

                logger.LogInformation($"End in {(DateTime.Now - start).TotalSeconds}");
                //await Task.Delay(20000);
                Console.ReadLine();
            }
        }
    }
    public class Helper(BitfinexService bitfinexService)
    {
        public readonly static JsonSerializerOptions telegramSerializeOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        public readonly static JsonSerializerOptions serializeOptions = new() { WriteIndented = true };
        public readonly static JsonSerializerOptions deserializeOptions = new() { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
        public readonly static string choices = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string GetExchangeName1(int id)
        {
            return id switch
            {
                0 => "Binance",
                1 => "Mexc",
                2 => "Bybit",
                3 => "Gate",
                4 => "Bitget",
                5 => "Bitmart",
                6 => "BingX",
                7 => "Kucoin",
                8 => "HTX",
                9 => "OKX",
                10 => "LBank",
                11 => "CoinW",
                12 => "CoinEx",
                13 => "Bitfinex",
                14 => "XT․com",
                15 => "Digifinex",
                16 => "Probit",
                17 => "Phemex",
                18 => "Tapbit",
                19 => "AscendEX",
                20 => "Poloniex",
                21 => "Kraken",
                _ => $"Exchange name error (id {id} not found)"
            };
        }

        public string GetExchangePairLink(int id, string baseAsset, string quoteAsset)
        {
            string bitfinexPair = "";

            if (id == 13)
            {
                string? bitfinexPairNull = bitfinexService.GetSymbol(baseAsset, quoteAsset);
                if (bitfinexPairNull is not null)
                {
                    bitfinexPair = bitfinexPairNull[..];
                }
            }

            return id switch
            {
                0 => $"https://www.binance.com/trade/{baseAsset}_{quoteAsset}?type=spot",
                1 => $"https://www.mexc.com/exchange/{baseAsset}_{quoteAsset}",
                2 => $"https://www.bybit.com/trade/spot/{baseAsset}/{quoteAsset}",
                3 => $"https://www.gate.io/trade/{baseAsset}_{quoteAsset}",
                4 => $"https://www.bitget.com/spot/{baseAsset}{quoteAsset}",
                5 => $"https://www.bitmart.com/trade?type=spot&symbol={baseAsset}_{quoteAsset}",
                6 => $"https://bingx.com/spot/{baseAsset}{quoteAsset}",
                7 => $"https://www.kucoin.com/trade/{baseAsset}-{quoteAsset}",
                8 => $"https://www.htx.com/trade/{baseAsset.ToLower()}_{quoteAsset.ToLower()}?type=spot",
                9 => $"https://www.okx.com/trade-spot/{baseAsset.ToLower()}-{quoteAsset.ToLower()}",
                10 => $"https://www.lbank.com/trade/{baseAsset.ToLower()}_{quoteAsset.ToLower()}",
                11 => $"https://www.coinw.com/spot/{baseAsset.ToLower()}{quoteAsset.ToLower()}",
                12 => $"https://www.coinex.com/exchange/{baseAsset.ToLower()}-{quoteAsset.ToLower()}#spot",
                13 => $"https://trading.bitfinex.com/t/{bitfinexPair}",
                14 => $"https://www.xt.com/trade/{baseAsset.ToLower()}_{quoteAsset.ToLower()}",
                15 => $"https://www.digifinex.com/en-ww/trade/{quoteAsset.ToUpper()}/{baseAsset.ToUpper()}",
                16 => $"https://www.probit.com/app/exchange/{baseAsset.ToUpper()}-{quoteAsset.ToUpper()}",
                17 => $"https://phemex.com/spot/trade/{baseAsset.ToUpper()}{quoteAsset.ToUpper()}",
                18 => $"https://www.tapbit.com/spot/exchange/{baseAsset.ToUpper()}_{quoteAsset.ToUpper()}",
                19 => $"https://ascendex.com/cashtrade-spottrading/{quoteAsset.ToLower()}/{baseAsset.ToLower()}",
                20 => $"https://poloniex.com/trade/{baseAsset.ToUpper()}_{quoteAsset.ToUpper()}",
                21 => $"https://pro.kraken.com/app/trade/{baseAsset.ToLower()}-{quoteAsset.ToLower()}",
                _ => "Exchange pair link get error (id not found)"
            };
        }

        public static double GetExchangeFee(int id)
        {
            return id switch
            {
                0 => 0.001,
                1 => 0.0005,
                2 => 0.001,
                3 => 0.001,
                4 => 0.001,
                5 => 0.001,
                6 => 0.001,
                7 => 0.001,
                8 => 0.002,
                9 => 0.0023,
                10 => 0.002,
                11 => 0.002,
                12 => 0.002,
                13 => 0.002,
                14 => 0.002,
                15 => 0.002,
                16 => 0.002,
                17 => 0.001,
                18 => 0.001,
                19 => 0.002,
                20 => 0.002,
                21 => 0.004,
                _ => 0
            };
        }

        public static string AesEncryptToBase64URL(string text, string key)
        {
            using Aes aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] textBytes = Compress(Encoding.UTF8.GetBytes(text));
            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] cipherBytes = encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);

            byte[] combined = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(combined).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public static string AesDecryptFromBase64URL(string base64Url, string key)
        {
            string base64 = base64Url.Replace('-', '+').Replace('_', '/');
            while (base64.Length % 4 != 0)
                base64 += '=';

            byte[] combined = Convert.FromBase64String(base64);
            byte[] iv = new byte[16];
            byte[] cipherBytes = new byte[combined.Length - 16];

            Buffer.BlockCopy(combined, 0, iv, 0, 16);
            Buffer.BlockCopy(combined, 16, cipherBytes, 0, cipherBytes.Length);

            using Aes aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] plainBytes = Decompress(decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length));

            return Encoding.UTF8.GetString(plainBytes);
        }

        public static byte[] Compress(byte[] data)
        {
            using MemoryStream memoryStream = new();

            using (BrotliStream brotliStream = new(memoryStream, CompressionLevel.Optimal, true))
            {
                brotliStream.Write(data, 0, data.Length);
            }

            return memoryStream.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            using MemoryStream memoryStream = new(data);
            using MemoryStream outputStream = new();

            using (BrotliStream brotliStream = new(memoryStream, CompressionMode.Decompress))
            {
                brotliStream.CopyTo(outputStream);
            }

            return outputStream.ToArray();
        }
    }

    public class FlexibleStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? String.Empty;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int intValue))
                {
                    return intValue.ToString();
                }
                else if (reader.TryGetDouble(out double doubleValue))
                {
                    return doubleValue.ToString();
                }
            }
            throw new JsonException("Unexpected token type for string conversion");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}