using CryptoOculus.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class DigifinexService(IHttpClientFactory httpClientFactory, ILogger<DigifinexService> logger, ApiKeysService apiKeys, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 15;
        public string ExchangeName { get; } = "DigiFinex";
        public string[] Hosts { get; } = ["openapi.digifinex.io", "api.digifinex.io"];
        public string[] Ips { get; set; } = [];

        private readonly Dictionary<int, string> currencyTypes = new()
        {
            [1] = "ERC20",
            [2] = "OMNI",
            [3] = "TRC20",
            [4] = "ETZ",
            [5] = "XGP",
            [6] = "BEP20",
            [7] = "LUK",
            [8] = "ETH-TSS",
            [9] = "SOL",
            [10] = "XLM",
            [11] = "AOK",
            [12] = "VITE",
            [13] = "NEO",
            [14] = "XRP",
            [15] = "Cardano",
            [16] = "Moonriver",
            [17] = "SORA",
            [18] = "LUNC",
            [19] = "EVMOSCOSMOS",
            [20] = "APTOS",
            [21] = "BRC20",
            [22] = "ONT"
        };

        private void ValidateDigifinexExchangeInfo(HttpRequestMessage request)
        {
            DigifinexExchangeInfo model = ClientService.Deserialize<DigifinexExchangeInfo>(request);

            if (model.SymbolList is null || model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateDigifinexCurrencySpot(HttpRequestMessage request)
        {
            DigifinexCurrencySpot model = ClientService.Deserialize<DigifinexCurrencySpot>(request);

            if (model.Data is null || model.Errcode != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateDigifinexDepositDetail(HttpRequestMessage request)
        {
            DigifinexDepositDetail model = ClientService.Deserialize<DigifinexDepositDetail>(request);

            if (model.Data is null || model.Errcode != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateDigifinexWithdrawDetail(HttpRequestMessage request)
        {
            DigifinexWithdrawDetail model = ClientService.Deserialize<DigifinexWithdrawDetail>(request);

            if (model.Data is null || model.Errcode != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateDigifinexPrices(HttpRequestMessage request)
        {
            DigifinexPrices model = ClientService.Deserialize<DigifinexPrices>(request);

            if (model.Ticker is null || model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateDigifinexRefreshToken(HttpRequestMessage request)
        {
            DigifinexRefreshToken model = ClientService.Deserialize<DigifinexRefreshToken>(request);

            if (model.Data is null || model.Errcode != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateDigifinexOrderBook(HttpRequestMessage request)
        {
            DigifinexOrderBook model = ClientService.Deserialize<DigifinexOrderBook>(request);

            if (model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public static string DecryptMAccess(string encryptedHexString)
        {
            byte[] cipherTextBytes = Convert.FromHexString(encryptedHexString);

            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes("tUYoAGpaX8K7GKdR");
            aesAlg.IV = Encoding.UTF8.GetBytes("8741750136967789");
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherTextBytes, 0, cipherTextBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private static string GenerateDcscForMobile(string deviceUuid, long timestamp)
        {
            byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(deviceUuid + timestamp + "hGYu9DVs9ivkP1T+rdb4e9+8QclvYRBRkP/7Wu9eYz6kHkNJGZm0mevkFqaWbzyH"));

            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }

        private static string GenerateDcscForWeb(string deviceUuid, long timestamp)
        {
            byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(deviceUuid + timestamp + "UKIH2uxyXZDbZYbF9jFq4KSrei9cxEGV"));

            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }

        private static string GenerateTraceParent()
        {
            byte[] traceIdBytes = new byte[16];
            RandomNumberGenerator.Fill(traceIdBytes);
            string traceId = Convert.ToHexString(traceIdBytes).ToLowerInvariant();

            byte[] parentIdBytes = new byte[8];
            RandomNumberGenerator.Fill(parentIdBytes);
            string parentId = Convert.ToHexString(parentIdBytes).ToLowerInvariant();

            return $"00-{traceId}-{parentId}-01";
        }

        private static string Signature(Dictionary<string, object> parameters, string accessKey, string nonce, long timestamp, string showUid)
        {
            Dictionary<string, object> paramsToSign = new(parameters)
            {
                ["nonce"] = nonce,
                ["timestamp"] = timestamp,
                ["show_uid"] = showUid
            };

            string stringToSign = string.Join("&", paramsToSign.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));

            return string.Concat(new HMACSHA1(Encoding.UTF8.GetBytes(accessKey)).ComputeHash(Encoding.UTF8.GetBytes(stringToSign)).Select(b => b.ToString("x2")));
        }

        private static string GenerateRandomString(int size)
        {
            Random rnd = new();
            string str_random = "abcdefghijklmnopqrstuvwxyz0123456789";
            string result = "";

            for (int i = 0; i < size; i++)
            {
                int a = rnd.Next(str_random.Length);
                result += str_random[a];
            }

            return result;
        }

        private async Task UpdateAccessToken()
        {
            string accessToken = apiKeys.GetSingle("DigifinexAccessToken");
            string accessKey = apiKeys.GetSingle("DigifinexAccessKey");
            string deviceUuid = apiKeys.GetSingle("DigifinexDeviceUuid");
            string showUid = apiKeys.GetSingle("DigifinexShowUid");

            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string nonce = GenerateRandomString(11);

            Dictionary<string, object> requestParams = new()
            {
                { "show_uid", showUid }
            };

            string sign = Signature(requestParams, accessKey, nonce, timestamp, showUid);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://{Ips[1]}/token/refresh").WithVersion();
            request.Content = new StringContent(JsonSerializer.Serialize(requestParams), Encoding.UTF8, "application/json");

            request.Headers.Host = Hosts[1];
            request.Headers.Add("timestamp", timestamp.ToString());
            request.Headers.Add("nonce", nonce);
            request.Headers.Add("device-uuid", deviceUuid);
            request.Headers.Add("access-token", accessToken);
            request.Headers.Add("show-uid", showUid);
            request.Headers.Add("sign", sign);
            request.Headers.Add("os-type", "ANDROID");
            request.Headers.Add("lang", "en-ww");

            string clientName = ClientService.RotatingProxyClient("digifinex");
            request.Options.Set(HttpOptionKeys.ClientName, clientName);
            request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateDigifinexRefreshToken);
            HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

            DigifinexRefreshToken digifinexRefreshToken = JsonSerializer.Deserialize<DigifinexRefreshToken>(await response.Content.ReadAsStringAsync())!;

            if (digifinexRefreshToken.Data is not null)
            {
                apiKeys.SetSingle("DigifinexAccessToken", digifinexRefreshToken.Data.AccessToken);
            }
        }

        private async Task<DigifinexContractAddresses> ContractAddressesUpdateAsync()
        {
            await UpdateAccessToken();

            string accessToken = apiKeys.GetSingle("DigifinexAccessToken");
            string accessKey = apiKeys.GetSingle("DigifinexAccessKey");
            string deviceUuid = apiKeys.GetSingle("DigifinexDeviceUuid");
            string showUid = apiKeys.GetSingle("DigifinexShowUid");

            List<(string, bool, bool)> bases = [];

            try
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string nonce = GenerateRandomString(11);
                string sign = Signature([], accessKey, nonce, timestamp, showUid);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://{Ips[1]}/currency/v1/spot").WithVersion();
                
                request.Headers.Host = Hosts[1];
                request.Headers.Add("timestamp", timestamp.ToString());
                request.Headers.Add("nonce", nonce);
                request.Headers.Add("access-token", accessToken);
                request.Headers.Add("show-uid", showUid);
                request.Headers.Add("device-uuid", deviceUuid);
                request.Headers.Add("sign", sign);

                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateDigifinexCurrencySpot);
                string clientName = ClientService.RotatingProxyClient("digifinex");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                DigifinexCurrencySpot currencySpot = JsonSerializer.Deserialize<DigifinexCurrencySpot>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                for (int i = 0; i < currencySpot.Data!.Blist!.Length; i++)
                {
                    if (currencySpot.Data!.Blist![i].IsRecharge == 1 || currencySpot.Data!.Blist![i].IsWithdraw == 1)
                    {
                        bases.Add((currencySpot.Data!.Blist![i].CurrencyMark, currencySpot.Data!.Blist![i].IsRecharge == 1, currencySpot.Data!.Blist![i].IsWithdraw == 1));
                    }
                }

                for (int i = 0; i < currencySpot.Data!.Plist!.Length; i++)
                {
                    if (currencySpot.Data!.Plist![i].IsRecharge == 1 || currencySpot.Data!.Plist![i].IsWithdraw == 1)
                    {
                        bases.Add((currencySpot.Data!.Plist![i].CurrencyMark, currencySpot.Data!.Plist![i].IsRecharge == 1, currencySpot.Data!.Plist![i].IsWithdraw == 1));
                    }
                }
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "ContractAddressesUpdate disabled!");
                throw;
            }

            List<Task> tasks = [];
            Lock locker = new();
            List<DigifinexCurrency> currencies = [];

            for (int i = 0; i < bases.Count; i++)
            {
                int buffer = i;

                tasks.Add(GetContractAddress(buffer));
            }

            async Task GetContractAddress(int index)
            {
                (string baseAsset, bool depositEnabled, bool withdrawEnabled) = bases[index];

                async Task<(string addressType, bool status, string? contractAddress)[]> Deposit()
                {
                    if (!depositEnabled)
                    {
                        return [];
                    }

                    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string nonce = GenerateRandomString(11);

                    Dictionary<string, object> requestParams = new()
                    {
                        { "currency_mark", baseAsset.ToUpper() },
                        { "address_type", String.Empty }
                    };

                    string sign = Signature(requestParams, accessKey, nonce, timestamp, showUid);

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://{Ips[1]}/deposit/currency/detail").WithVersion();
                    request.Content = new StringContent(JsonSerializer.Serialize(requestParams), Encoding.UTF8, "application/json");

                    request.Headers.Host = Hosts[1];
                    request.Headers.Add("timestamp", timestamp.ToString());
                    request.Headers.Add("nonce", nonce);
                    request.Headers.Add("access-token", accessToken);
                    request.Headers.Add("show-uid", showUid);
                    request.Headers.Add("device-uuid", deviceUuid);
                    request.Headers.Add("sign", sign);

                    string clientName = ClientService.RotatingProxyClient("digifinex-1");
                    request.Options.Set(HttpOptionKeys.ClientName, clientName);
                    request.Options.Set(HttpOptionKeys.KeyedLimitRuleName, "DigifinexDepositDetails");
                    request.Options.Set(HttpOptionKeys.KeyedLimitRuleKey, "DigifinexDepositDetails");
                    request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateDigifinexDepositDetail);

                    HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                    DigifinexDepositDetail detail = JsonSerializer.Deserialize<DigifinexDepositDetail>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                    List<Task> tasks = [];
                    List<(string, bool, string?)> depositDetails = [];

                    if (detail.Data is not null)
                    {
                        if (detail.Data.AddressTypeConf.Length > 0)
                        {
                            for (int i = 0; i < detail.Data.AddressTypeConf.Length; i++)
                            {
                                if (detail.Data.AddressTypeConf[i].IsEnabled == 1)
                                {
                                    string buffer = detail.Data.AddressTypeConf[i].AddressType;
                                    tasks.Add(DetailAsync(buffer));
                                }

                                else
                                {
                                    depositDetails.Add((detail.Data.AddressTypeConf[i].AddressType, false, null));
                                }
                            }
                        }

                        else
                        {
                            tasks.Add(DetailAsync(detail.Data.CurrencyType == 0 ? baseAsset.ToUpper() : currencyTypes[detail.Data.CurrencyType]));
                        }
                    }

                    async Task DetailAsync(string addressType)
                    {
                        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        string nonce = GenerateRandomString(11);

                        Dictionary<string, object> requestParams = new()
                        {
                            { "currency_mark", baseAsset.ToUpper() },
                            { "address_type", addressType.ToUpper() }
                        };

                        string sign = Signature(requestParams, accessKey, nonce, timestamp, showUid);

                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://{Ips[1]}/deposit/currency/detail").WithVersion();
                        request.Content = new StringContent(JsonSerializer.Serialize<Dictionary<string, object>>(requestParams), Encoding.UTF8, "application/json");

                        request.Headers.Host = Hosts[1];
                        request.Headers.Add("timestamp", timestamp.ToString());
                        request.Headers.Add("nonce", nonce);
                        request.Headers.Add("access-token", accessToken);
                        request.Headers.Add("show-uid", showUid);
                        request.Headers.Add("device-uuid", deviceUuid);
                        request.Headers.Add("sign", sign);

                        string clientName = ClientService.RotatingProxyClient("digifinex-1");
                        request.Options.Set(HttpOptionKeys.ClientName, clientName);
                        request.Options.Set(HttpOptionKeys.KeyedLimitRuleName, "DigifinexDepositDetails");
                        request.Options.Set(HttpOptionKeys.KeyedLimitRuleKey, "DigifinexDepositDetails");
                        request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateDigifinexDepositDetail);

                        HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                        DigifinexDepositDetail detail = JsonSerializer.Deserialize<DigifinexDepositDetail>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                        depositDetails.Add((addressType, true, !String.IsNullOrWhiteSpace(detail.Data?.ContractAddress) ? detail.Data.ContractAddress : null));
                    }

                    await Task.WhenAll(tasks);

                    return [.. depositDetails];
                }

                async Task<(string addressType, bool status, string fee, string feeCurrencyMark)[]> Withdraw()
                {
                    if (!withdrawEnabled)
                    {
                        return [];
                    }

                    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string nonce = GenerateRandomString(11);

                    Dictionary<string, object> requestParams = new()
                    {
                        { "currency_mark", baseAsset.ToUpper() },
                        { "address_type", String.Empty },
                        { "transfer_type", "0" }
                    };

                    string sign = Signature(requestParams, accessKey, nonce, timestamp, showUid);

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://{Ips[1]}/withdraw/currency/detail").WithVersion();
                    request.Content = new StringContent(JsonSerializer.Serialize<Dictionary<string, object>>(requestParams), Encoding.UTF8, "application/json");

                    request.Headers.Host = Hosts[1];
                    request.Headers.Add("timestamp", timestamp.ToString());
                    request.Headers.Add("nonce", nonce);
                    request.Headers.Add("access-token", accessToken);
                    request.Headers.Add("show-uid", showUid);
                    request.Headers.Add("device-uuid", deviceUuid);
                    request.Headers.Add("sign", sign);

                    /*request.Headers.Add("lang", "en-ww");
                    request.Headers.Add("traceparent", GenerateTraceParent());
                    request.Headers.Add("os-type", "PCWEB");
                    request.Headers.Add("dcsc", GenerateDcscForWeb(deviceUuid, timestamp));
                    request.Headers.Add("dcts", timestamp.ToString());
                    request.Headers.Add("dcver", "1.0");*/

                    string clientName = ClientService.RotatingProxyClient("digifinex-1");
                    request.Options.Set(HttpOptionKeys.ClientName, clientName);
                    request.Options.Set(HttpOptionKeys.KeyedLimitRuleName, "DigifinexWithdrawDetails");
                    request.Options.Set(HttpOptionKeys.KeyedLimitRuleKey, "DigifinexWithdrawDetails");
                    request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateDigifinexWithdrawDetail);

                    HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                    DigifinexWithdrawDetail detail = JsonSerializer.Deserialize<DigifinexWithdrawDetail>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                    List<(string, bool, string, string)> withdrawDetails = [];

                    if (detail.Data is not null)
                    {
                        if (detail.Data.AddressTypeConf.Length > 0)
                        {
                            for (int i = 0; i < detail.Data.AddressTypeConf.Length; i++)
                            {
                                withdrawDetails.Add((detail.Data.AddressTypeConf[i].AddressType, detail.Data.AddressTypeConf[i].IsEnabled == 1, detail.Data.AddressTypeConf[i].Fee, detail.Data.AddressTypeConf[i].FeeCurrencyMark));
                            }
                        }

                        else
                        {
                            withdrawDetails.Add((detail.Data.CurrencyType == 0 ? baseAsset.ToUpper() : currencyTypes[detail.Data.CurrencyType], detail.Data.IsWithdraw == 1, detail.Data.Fee, detail.Data.FeeCurrencyMark));
                        }
                    }

                    return [.. withdrawDetails];
                }

                try
                {
                    Task<(string, bool, string?)[]>? depositTask = Deposit();
                    Task<(string, bool, string, string)[]>? withdrawTask = Withdraw();

                    await Task.WhenAll([depositTask, withdrawTask]);

                    (string addressType, bool status, string? contractAddress)[] deposit = await depositTask;
                    (string addressType, bool status, string fee, string feeCurrencyMark)[] withdraw = await withdrawTask;

                    List<DigifinexAssetNetwork> digifinexAssetNetworks = [];

                    for (int i = 0; i < deposit.Length; i++)
                    {
                        digifinexAssetNetworks.Add(new()
                        {
                            NetworkName = deposit[i].addressType,
                            DepositEnable = deposit[i].status,
                            Address = deposit[i].contractAddress
                        });
                    }

                    for (int i = 0; i < withdraw.Length; i++)
                    {
                        bool add = true;

                        for (int a = 0; a < digifinexAssetNetworks.Count; a++)
                        {
                            if (withdraw[i].addressType.Equals(digifinexAssetNetworks[a].NetworkName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                digifinexAssetNetworks[a].WithdrawEnable = withdraw[i].status;
                                digifinexAssetNetworks[a].WithdrawFee = double.Parse(withdraw[i].fee);
                                digifinexAssetNetworks[a].WithdrawFeeCurrency = withdraw[i].feeCurrencyMark;
                                add = false;
                                break;
                            }
                        }

                        if (add)
                        {
                            digifinexAssetNetworks.Add(new()
                            {
                                NetworkName = withdraw[i].addressType,
                                WithdrawEnable = withdraw[i].status,
                                WithdrawFee = double.Parse(withdraw[i].fee),
                                WithdrawFeeCurrency = withdraw[i].feeCurrencyMark
                            });
                        }
                    }

                    if (digifinexAssetNetworks.Count > 0)
                    {
                        DigifinexCurrency currency = new()
                        {
                            Currency = baseAsset.ToUpper(),
                            Networks = [.. digifinexAssetNetworks]
                        };

                        lock (locker)
                        {
                            currencies.Add(currency);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to fully retrieve information about the coin!");
                    return;
                }
            }

            await Task.WhenAll(tasks);

            DigifinexContractAddresses contractAddresses = new()
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Currencies = [.. currencies]
            };

            await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Digifinex/contractAddresses.json"), JsonSerializer.Serialize(contractAddresses, Helper.serializeOptions));

            return contractAddresses;
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<DigifinexExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v3/spot/symbols").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateDigifinexExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<DigifinexExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<DigifinexContractAddresses> Contract()
            {
                string path = Path.Combine(env.ContentRootPath, "Cache/Digifinex/contractAddresses.json");

                if (File.Exists(path))
                {
                    DigifinexContractAddresses? contractAddresses = JsonSerializer.Deserialize<DigifinexContractAddresses>(await File.ReadAllTextAsync(path), Helper.deserializeOptions);

                    if (contractAddresses is not null && TimeSpan.FromSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - contractAddresses.Timestamp).TotalHours < 1)
                    {
                        return contractAddresses;
                    }
                }

                logger.LogInformation("Digifinex: Calling ContractAddressesUpdate");
                return await ContractAddressesUpdateAsync();
            }

            //Query currect prices of pairs
            async Task<DigifinexPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v3/ticker").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateDigifinexPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<DigifinexPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<DigifinexExchangeInfo> exInfoTask = ExInfo();
                Task<DigifinexContractAddresses> contractTask = Contract();
                Task<DigifinexPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                DigifinexExchangeInfo exchangeInfo = await exInfoTask;
                DigifinexContractAddresses contractAddresses = await contractTask;
                DigifinexPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.SymbolList is not null)
                {
                    for (int i = 0; i < exchangeInfo.SymbolList.Length; i++)
                    {
                        if (exchangeInfo.SymbolList[i].Status == "TRADING" &&
                           (exchangeInfo.SymbolList[i].QuoteAsset == "USDT" || exchangeInfo.SymbolList[i].QuoteAsset == "USDC" || exchangeInfo.SymbolList[i].QuoteAsset == "TUSD"))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = exchangeInfo.SymbolList[i].BaseAsset.ToUpper(),
                                QuoteAsset = exchangeInfo.SymbolList[i].QuoteAsset.ToUpper()
                            };

                            //adding price of pair
                            if (prices.Ticker is not null)
                            {
                                for (int a = 0; a < prices.Ticker.Length; a++)
                                {
                                    if (prices.Ticker[a].Symbol.Equals(exchangeInfo.SymbolList[i].Symbol, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        pair.AskPrice = prices.Ticker[a].Sell;
                                        pair.BidPrice = prices.Ticker[a].Buy;

                                        break;
                                    }
                                }
                            }

                            //adding supported networks of base asset
                            for (int b = 0; b < contractAddresses.Currencies.Length; b++)
                            {
                                if (contractAddresses.Currencies[b].Currency.Equals(exchangeInfo.SymbolList[i].BaseAsset, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    List<AssetNetwork> baseAssetNetworks = [];

                                    for (int c = 0; c < contractAddresses.Currencies[b].Networks.Length; c++)
                                    {
                                        if (contractAddresses.Currencies[b].Networks[c].DepositEnable || contractAddresses.Currencies[b].Networks[c].WithdrawEnable)
                                        {
                                            AssetNetwork assetNetwork = new()
                                            {
                                                NetworkName = contractAddresses.Currencies[b].Networks[c].NetworkName,
                                                DepositEnable = contractAddresses.Currencies[b].Networks[c].DepositEnable,
                                                WithdrawEnable = contractAddresses.Currencies[b].Networks[c].WithdrawEnable,
                                                Address = contractAddresses.Currencies[b].Networks[c].Address
                                            };

                                            if (contractAddresses.Currencies[b].Networks[c].WithdrawFee is not null)
                                            {
                                                if (pair.AskPrice != 0)
                                                {
                                                    assetNetwork.WithdrawFee = pair.AskPrice * contractAddresses.Currencies[b].Networks[c].WithdrawFee;
                                                }

                                                else if (pair.BidPrice != 0)
                                                {
                                                    assetNetwork.WithdrawFee = pair.BidPrice * contractAddresses.Currencies[b].Networks[c].WithdrawFee;
                                                }
                                            }

                                            string? withdrawFeeCurrency = contractAddresses.Currencies[b].Networks[c].WithdrawFeeCurrency;

                                            if (withdrawFeeCurrency is not null)
                                            {
                                                if (withdrawFeeCurrency.Equals(exchangeInfo.SymbolList[i].BaseAsset, StringComparison.CurrentCultureIgnoreCase))
                                                {
                                                    if (pair.AskPrice != 0)
                                                    {
                                                        assetNetwork.WithdrawFee = pair.AskPrice * contractAddresses.Currencies[b].Networks[c].WithdrawFee;
                                                    }

                                                    else if (pair.BidPrice != 0)
                                                    {
                                                        assetNetwork.WithdrawFee = pair.BidPrice * contractAddresses.Currencies[b].Networks[c].WithdrawFee;
                                                    }
                                                }

                                                else if (withdrawFeeCurrency == "USDT" || withdrawFeeCurrency == "USDC")
                                                {
                                                    assetNetwork.WithdrawFee = contractAddresses.Currencies[b].Networks[c].WithdrawFee;
                                                }

                                                else
                                                {
                                                    //Find price
                                                    if (prices.Ticker is not null)
                                                    {
                                                        for (int d = 0; d < prices.Ticker.Length; d++)
                                                        {
                                                            if (prices.Ticker[d].Symbol == $"{withdrawFeeCurrency.ToLower()}_usdt")
                                                            {
                                                                assetNetwork.WithdrawFee = prices.Ticker[d].Last * contractAddresses.Currencies[b].Networks[c].WithdrawFee;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            baseAssetNetworks.Add(assetNetwork);
                                        }
                                    }

                                    if (baseAssetNetworks.Count > 0)
                                    {
                                        pair.BaseAssetNetworks = [.. baseAssetNetworks];
                                    }

                                    break;
                                }
                            }

                            if ((pair.BidPrice != 0 || pair.AskPrice != 0) && pair.BaseAssetNetworks.Length > 0)
                            {
                                pairs.Add(pair);
                            }
                        }
                    }
                }

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Digifinex/firstStepPairs.json"), JsonSerializer.Serialize(pairs, Helper.serializeOptions));

                return [.. pairs];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Service disabled!");
                return [];
            }
        }

        public async Task<double[][]?> OrderBook(string baseAsset, string quoteAsset, bool askOrBid)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v3/order_book?symbol={baseAsset.ToLower()}_{quoteAsset.ToLower()}&limit=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateDigifinexOrderBook);
                string clientName = ClientService.RotatingProxyClient("digifinex");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                DigifinexOrderBook orderBook = JsonSerializer.Deserialize<DigifinexOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Asks is not null)
                {
                    return orderBook.Asks;
                }

                else if (!askOrBid && orderBook.Bids is not null)
                {
                    return orderBook.Bids;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OrderBook disabled!");
            }

            return null;
        }
    }
}
