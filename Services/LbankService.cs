using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class LbankService(IHttpClientFactory httpClientFactory, ILogger<LbankService> logger, ApiKeysService apiKeys, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 10;
        public string ExchangeName { get; } = "LBank";
        public string[] Hosts { get; } = ["api.lbank.info", "ccapi.ierpifvid.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateLbankExchangeInfo(HttpRequestMessage request)
        {
            LbankExchangeInfo model = ClientService.Deserialize<LbankExchangeInfo>(request);

            if (model.Data is null || model.Msg != "Success" || model.Result != "true")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateLbankConfigs(HttpRequestMessage request)
        {
            LbankConfigs model = ClientService.Deserialize<LbankConfigs>(request);

            if (model.Data is null || model.Result == "false")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateLbankDepositDetail(HttpRequestMessage request)
        {
            LbankDepositDetail model = ClientService.Deserialize<LbankDepositDetail>(request);

            if (model.Data is null || model.Code != 200 || model.Message != "Success!")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateLbankRate(HttpRequestMessage request)
        {
            LbankRate model = ClientService.Deserialize<LbankRate>(request);

            if (model.Data is null || model.Code != 200 || model.Message != "Success!")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateLbankPrices(HttpRequestMessage request)
        {
            LbankPrices model = ClientService.Deserialize<LbankPrices>(request);

            if (model.Data is null || model.Msg != "Success" || model.Result != "true")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateLbankOrderBook(HttpRequestMessage request)
        {
            LbankOrderBook model = ClientService.Deserialize<LbankOrderBook>(request);

            if (model.Data is null || model.Msg != "Success" || model.Result != "true")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }


        public async Task<LbankContractAddresses> ContractAddressesUpdateAsync(LbankExchangeInfo exchangeInfo)
        {
            try
            {
                List<string> bases = [];
                for (int i = 0; i < exchangeInfo.Data!.Length; i++)
                {
                    string[] split = exchangeInfo.Data[i].Split("_");
                    bases.Add(split[0]);
                }

                List<Task> tasks = [];
                Lock locker = new();
                CancellationTokenSource cts = new();

                List<LbankCurrency> currencies = [];

                for (int i = 0; i < bases.Count; i++)
                {
                    int buffer = i;

                    tasks.Add(GetCurrencyAsync(buffer, cts.Token));
                }

                async Task GetCurrencyAsync(int index, CancellationToken cancellationToken)
                {
                    try
                    {
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/assetConfigs.do?assetCode={bases[index]}").WithVersion();
                        request.Headers.Host = Hosts[0];
                        request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateLbankConfigs);
                        string clientName = ClientService.RotatingProxyClient("lbank-1");
                        request.Options.Set(HttpOptionKeys.ClientName, clientName);
                        HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request, cancellationToken);

                        LbankConfigs lbankConfigs = JsonSerializer.Deserialize<LbankConfigs>(await response.Content.ReadAsStringAsync(cancellationToken), Helper.deserializeOptions)!;

                        List<LbankDepositDetail> lbankDepositDetails = [];

                        if (lbankConfigs.Data is not null)
                        {
                            List<Task> tasks = [];

                            for (int i = 0; i < lbankConfigs.Data.Length; i++)
                            {
                                if (lbankConfigs.Data[i].CanDeposit)
                                {
                                    string buffer = lbankConfigs.Data[i].ChainName;
                                    tasks.Add(Details(buffer, cancellationToken));
                                }
                            }

                            await Task.WhenAll([.. tasks]);
                        }

                        async Task Details(string networkName, CancellationToken cancellationToken)
                        {
                            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[1]}/coin-wallet-center/wallet/address/deposit/detail/v1?assetCode={bases[index]}&assetFlag={networkName}").WithVersion();
                            request.Headers.Host = Hosts[1];
                            request.Headers.Add("ex-token", apiKeys.GetSingle("LbankExToken"));
                            request.Headers.Add("ex-client-type", "WEB");
                            request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateLbankDepositDetail);
                            string clientName = ClientService.RotatingProxyClient("lbank-2");
                            request.Options.Set(HttpOptionKeys.ClientName, clientName);
                            HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request, cancellationToken);

                            lbankDepositDetails.Add(JsonSerializer.Deserialize<LbankDepositDetail>(await response.Content.ReadAsStringAsync(cancellationToken), Helper.deserializeOptions)!);
                        }

                        List<AssetNetwork> assetNetworks = [];

                        if (lbankConfigs.Data is not null)
                        {
                            for (int i = 0; i < lbankConfigs.Data.Length; i++)
                            {
                                AssetNetwork assetNetwork = new()
                                {
                                    NetworkName = lbankConfigs.Data[i].ChainName,
                                    DepositEnable = lbankConfigs.Data[i].CanDeposit,
                                    WithdrawEnable = lbankConfigs.Data[i].CanDraw,
                                    WithdrawFee = double.TryParse(lbankConfigs.Data[i].AssetFee?.FeeAmt, out double withdrawFee) ? withdrawFee : null,
                                    TransferTax = double.TryParse(lbankConfigs.Data[i].AssetFee?.FeeRate, out double transferTax) ? transferTax : null
                                };

                                for (int a = 0; a < lbankDepositDetails.Count; a++)
                                {
                                    if (lbankDepositDetails[a].Data is not null && lbankDepositDetails[a].Data!.DepositEnabled && lbankDepositDetails[a].Data?.AddressData?.WalletAddress?.AssetFlag == lbankConfigs.Data[i].ChainName)
                                    {
                                        if (!String.IsNullOrWhiteSpace(lbankDepositDetails[a].Data?.AddressData?.ContractInfo))
                                        {
                                            assetNetwork.Address = lbankDepositDetails[a].Data?.AddressData?.ContractInfo;
                                        }

                                        break;
                                    }
                                }

                                assetNetworks.Add(assetNetwork);
                            }
                        }

                        if (assetNetworks.Count > 0)
                        {
                            LbankCurrency currency = new()
                            {
                                Currency = bases[index],
                                Networks = [.. assetNetworks]
                            };

                            lock (locker)
                            {
                                currencies.Add(currency);
                            }
                        }
                    }
                    catch
                    {
                        cts.Cancel();
                        throw;
                    }
                }

                await Task.WhenAll(tasks);

                LbankContractAddresses contractAddresses = new()
                {
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Currencies = [.. currencies]
                };

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Lbank/contractAddresses.json"), JsonSerializer.Serialize(contractAddresses, Helper.serializeOptions));

                return contractAddresses;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ContractAddressesUpdate disabled!");
                throw;
            }
        }

        public async Task<LbankComissions> CommissionsUpdateAsync(LbankExchangeInfo exchangeInfo)
        {

            List<Task> tasks = [];
            Lock locker = new();

            List<LbankComission> comissions = [];

            for (int i = 0; i < exchangeInfo.Data!.Length; i++)
            {
                int buffer = i;

                tasks.Add(GetComissionAsync(buffer));
            }

            async Task GetComissionAsync(int index)
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[1]}/spot-trade-center/order/feeRate?category={exchangeInfo.Data[index]}").WithVersion();
                request.Headers.Host = Hosts[1];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateLbankRate);
                string clientName = ClientService.RotatingProxyClient("lbank-3");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                LbankRate rate = JsonSerializer.Deserialize<LbankRate>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (double.TryParse(rate.Data?.TakerRate, out double takerComission))
                {
                    lock (locker)
                    {
                        comissions.Add(new()
                        {
                            Symbol = exchangeInfo.Data[index],
                            TakerComission = takerComission / 100
                        });
                    }
                }
            }

            await Task.WhenAll(tasks);

            LbankComissions lbankComissions = new()
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Comissions = [.. comissions]
            };

            using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Lbank/comissions.json"));
            sw.Write(JsonSerializer.Serialize<LbankComissions>(lbankComissions, Helper.serializeOptions));

            return lbankComissions;
        }

        public async Task<Pair[]> GetPairs()
        {
            /*AUTHENTICATION
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string echostr = GenerateRandomString(35);
            string parameters = $"api_key=a0bcb893-a292-4731-8c4b-d0ad8443ffeb&echostr={echostr}&signature_method=HmacSHA256&timestamp={now}";
            string md5Hash = string.Concat(MD5.HashData(Encoding.UTF8.GetBytes(parameters)).Select(b => b.ToString("X2")));
            using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes("67B13D4D4E2550A0E50EF5DBB74A6846"));
            string computedHash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(md5Hash))).Replace("-", "").ToLower();
            StringContent postData = new($"api_key=a0bcb893-a292-4731-8c4b-d0ad8443ffeb&echostr={echostr}&signature_method=HmacSHA256&timestamp={now}&sign={computedHash}", Encoding.UTF8, "application/x-www-form-urlencoded");

            try
            {
                HttpResponseMessage response = await client.PostAsync($"https://{ip}/v2/supplement/asset_detail.do", postData);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Status code: {response.StatusCode} | Content: {await response.Content.ReadAsStringAsync()}");
                }

                string content = await response.Content.ReadAsStringAsync();
                contractAddresses = JsonSerializer.Deserialize<LbankContractAddresses?>(content, Helper.deserializeOptions);

                if (contractAddresses is null || contractAddresses.Data is null || contractAddresses.Result != "true")
                {
                    throw new Exception($"Empty response or Ex error code | Content: {content}");
                }
            }

            catch (Exception ex)
            {
                logger.LogWarning(ex, "{Message}", ex.Message);
                await Task.Delay(300);
                continue;
            }

            isSuccess = true;
            break;
        }*/
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<(LbankExchangeInfo, LbankContractAddresses, LbankComissions)> ExInfoContractComissions()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/currencyPairs.do").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateLbankExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                LbankExchangeInfo exchangeInfo = JsonSerializer.Deserialize<LbankExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                //Query currency details and the smart contract address
                async Task<LbankContractAddresses> ContractAddresses()
                {
                    string path = Path.Combine(env.ContentRootPath, "Cache/Lbank/contractAddresses.json");

                    if (File.Exists(path))
                    {
                        LbankContractAddresses? contractAddresses = JsonSerializer.Deserialize<LbankContractAddresses>(await File.ReadAllTextAsync(path), Helper.deserializeOptions);

                        if (contractAddresses is not null && TimeSpan.FromSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - contractAddresses.Timestamp).TotalHours < 1)
                        {
                            return contractAddresses;
                        }
                    }

                    logger.LogInformation("LBank: Calling ContractAddressesUpdate");
                    return await ContractAddressesUpdateAsync(exchangeInfo);
                }

                //Query spot comissions
                async Task<LbankComissions> Comissions()
                {
                    string path = Path.Combine(env.ContentRootPath, "Cache/Lbank/comissions.json");

                    if (File.Exists(path))
                    {
                        LbankComissions? comissions = JsonSerializer.Deserialize<LbankComissions>(await File.ReadAllTextAsync(path), Helper.deserializeOptions);

                        if (comissions is not null && TimeSpan.FromSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - comissions.Timestamp).TotalHours < 1)
                        {
                            return comissions;
                        }
                    }

                    logger.LogInformation("LBank: Calling CommissionsUpdateAsync");
                    return await CommissionsUpdateAsync(exchangeInfo);
                }

                Task<LbankContractAddresses> contractAddresses = ContractAddresses();
                Task<LbankComissions> comissions = Comissions();

                await Task.WhenAll([contractAddresses, comissions]);

                return (exchangeInfo, await contractAddresses, await comissions);
            }

            //Query currect prices of pairs
            async Task<LbankPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/ticker/24hr.do?symbol=all").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateLbankPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<LbankPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<(LbankExchangeInfo, LbankContractAddresses, LbankComissions)> exInfoContractComissionsTask = ExInfoContractComissions();
                Task<LbankPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoContractComissionsTask, pricesTask]);

                (LbankExchangeInfo exchangeInfo, LbankContractAddresses contractAddresses, LbankComissions comissions) = await exInfoContractComissionsTask;
                LbankPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Length; i++)
                    {
                        string[] split = exchangeInfo.Data[i].Split('_');
                        if (split[1] == "usdt" || split[1] == "usdc" || split[1] == "tusd")
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = split[0].ToUpper(),
                                QuoteAsset = split[1].ToUpper()
                            };

                            //adding price of pair
                            if (prices.Data is not null)
                            {
                                for (int a = 0; a < prices.Data.Length; a++)
                                {
                                    if (prices.Data[a].Symbol == exchangeInfo.Data[i])
                                    {
                                        if (double.TryParse(prices.Data[a].Ticker.Latest, out double lastPrice))
                                        {
                                            pair.AskPrice = lastPrice;
                                            pair.BidPrice = lastPrice;
                                        }

                                        break;
                                    }
                                }
                            }

                            //adding supported networks of base asset
                            for (int b = 0; b < contractAddresses.Currencies.Length; b++)
                            {
                                if (contractAddresses.Currencies[b].Currency.Equals(split[0], StringComparison.CurrentCultureIgnoreCase))
                                {
                                    List<AssetNetwork> baseAssetNetworks = [];

                                    for (int c = 0; c < contractAddresses.Currencies[b].Networks.Length; c++)
                                    {
                                        if (contractAddresses.Currencies[b].Networks[c].DepositEnable || contractAddresses.Currencies[b].Networks[c].WithdrawEnable)
                                        {
                                            AssetNetwork assetNetwork = new()
                                            {
                                                NetworkName = contractAddresses.Currencies[b].Networks[c].NetworkName,
                                                Address = contractAddresses.Currencies[b].Networks[c].Address,
                                                DepositEnable = contractAddresses.Currencies[b].Networks[c].DepositEnable,
                                                WithdrawEnable = contractAddresses.Currencies[b].Networks[c].WithdrawEnable,
                                                TransferTax = contractAddresses.Currencies[b].Networks[c].TransferTax
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

                            //adding spot comission
                            for (int a = 0; a < comissions.Comissions.Length; a++)
                            {
                                if (comissions.Comissions[a].Symbol.Equals(exchangeInfo.Data[i], StringComparison.CurrentCultureIgnoreCase))
                                {
                                    pair.SpotComission = comissions.Comissions[a].TakerComission;
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

                using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Lbank/firstStepPairs.json"));
                sw.Write(JsonSerializer.Serialize<List<Pair>>(pairs, Helper.serializeOptions));

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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/depth.do?symbol={baseAsset.ToLower()}_{quoteAsset.ToLower()}&size=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateLbankOrderBook);
                string clientName = ClientService.RotatingProxyClient("lbank");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                LbankOrderBook orderBook = JsonSerializer.Deserialize<LbankOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Data is not null)
                {
                    return Array.ConvertAll(orderBook.Data.Asks, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid && orderBook.Data is not null)
                {
                    return Array.ConvertAll(orderBook.Data.Bids, innerArray => Array.ConvertAll(innerArray, double.Parse));
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