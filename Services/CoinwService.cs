using CryptoOculus.Models;
using System.Text;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class CoinwService(IHttpClientFactory httpClientFactory, ILogger<CoinwService> logger, ApiKeysService apiKeys, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 11;
        public string ExchangeName { get; } = "CoinW";
        public string[] Hosts { get; } = ["api.coinw.com", "spotapi.coinw.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateCoinwExchangeInfo(HttpRequestMessage request)
        {
            CoinwExchangeInfo model = ClientService.Deserialize<CoinwExchangeInfo>(request);

            if (model.Data is null || model.Code != "200")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateCoinwPrices(HttpRequestMessage request)
        {
            CoinwPrices model = ClientService.Deserialize<CoinwPrices>(request);

            if (model.Data is null || model.Code != "200")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateCoinwCoinOption(HttpRequestMessage request)
        {
            CoinwCoinOption model = ClientService.Deserialize<CoinwCoinOption>(request);

            if (model.Data is null || model.Code != 200 || model.Msg != "success")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateCoinwAddressList(HttpRequestMessage request)
        {
            CoinwAddressList model = ClientService.Deserialize<CoinwAddressList>(request);

            if (model.Data is null || model.Code != 200 || model.Msg != "success")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateCoinwWalletCoin(HttpRequestMessage request)
        {
            CoinwWalletCoin model = ClientService.Deserialize<CoinwWalletCoin>(request);

            if (model.Data is null || model.Code != 200 || model.Msg != "success")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateCoinwOrderBook(HttpRequestMessage request)
        {
            CoinwOrderBook model = ClientService.Deserialize<CoinwOrderBook>(request);

            if (model.Data is null || model.Code != "200" || model.Msg != "SUCCESS" || !model.Success || model.Failed)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<CoinwContractAddresses> ContractAddressesUpdateAsync()
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://{Ips[1]}/asset/v1/api/coin/option").WithVersion();
                request.Headers.Host = Hosts[1];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateCoinwCoinOption);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                CoinwCoinOption coinOption = JsonSerializer.Deserialize<CoinwCoinOption>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                List<Task> tasks = [];
                Lock locker = new();
                CancellationTokenSource cts = new();

                List<CoinwCurrency> currencies = [];

                string[] loginTokens = apiKeys.GetMultiple("CoinwLoginTokens");

                int apiIndex = 0;

                if (coinOption.Data is not null)
                {
                    for (int i = 0; i < coinOption.Data.Length; i++)
                    {
                        if (coinOption.Data[i].RechargeStatus == 1 || coinOption.Data[i].WithdrawStatus == 1)
                        {
                            int buffer = i;
                            int apiIndexBuffer = apiIndex;
                            apiIndex = (apiIndex + 1) % loginTokens.Length;

                            tasks.Add(GetContractAddress(buffer, apiIndexBuffer, cts.Token));
                        }
                    }
                }

                async Task GetContractAddress(int index, int apiIndex, CancellationToken cancellationToken)
                {
                    try
                    {
                        CoinwAddressList? addressList = null;
                        CoinwWalletCoin? walletCoin = null;

                        async Task AddressList()
                        {
                            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://{Ips[1]}/asset/v1/api/address/list").WithVersion();
                            request.Content = new StringContent(JsonSerializer.Serialize(new { coinId = coinOption.Data[index].Id }), Encoding.UTF8, "application/json");

                            request.Headers.Host = Hosts[1];
                            request.Headers.Add("logintoken", loginTokens[apiIndex]);
                            request.Options.Set(HttpOptionKeys.KeyedConcurrencyLimitName, "CoinwLoginTokenLimit");
                            request.Options.Set(HttpOptionKeys.KeyedConcurrencyLimitKey, $"{loginTokens[apiIndex]}AddressList");
                            request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateCoinwAddressList);
                            string clientName = ClientService.RotatingProxyClient("coinw");
                            request.Options.Set(HttpOptionKeys.ClientName, clientName);
                            HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request, cancellationToken);

                            addressList = JsonSerializer.Deserialize<CoinwAddressList>(await response.Content.ReadAsStringAsync(cancellationToken), Helper.deserializeOptions)!;
                        }

                        async Task WalletCoin()
                        {
                            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://{Ips[1]}/asset/v1/api/wallet/coin").WithVersion();
                            request.Content = new StringContent(JsonSerializer.Serialize(new { id = coinOption.Data[index].Id }), Encoding.UTF8, "application/json");

                            request.Headers.Host = Hosts[1];
                            request.Headers.Add("logintoken", loginTokens[apiIndex]);
                            request.Options.Set(HttpOptionKeys.KeyedConcurrencyLimitName, "CoinwLoginTokenLimit");
                            request.Options.Set(HttpOptionKeys.KeyedConcurrencyLimitKey, $"{loginTokens[apiIndex]}WalletCoin");
                            request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateCoinwWalletCoin);
                            string clientName = ClientService.RotatingProxyClient("coinw");
                            request.Options.Set(HttpOptionKeys.ClientName, clientName);
                            HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request, cancellationToken);

                            walletCoin = JsonSerializer.Deserialize<CoinwWalletCoin>(await response.Content.ReadAsStringAsync(cancellationToken), Helper.deserializeOptions)!;
                        }

                        await Task.WhenAll([AddressList(), WalletCoin()]);

                        if (coinOption.Data is not null)
                        {
                            List<AssetNetwork> assetNetworks = [];

                            for (int a = 0; a < coinOption.Data[index].MemoChainNames.Length; a++)
                            {
                                AssetNetwork assetNetwork = new()
                                {
                                    NetworkName = coinOption.Data[index].MemoChainNames[a].ChainName,
                                    DepositEnable = coinOption.Data[index].MemoChainNames[a].RechargeStatus == 1,
                                    WithdrawEnable = coinOption.Data[index].MemoChainNames[a].WithdrawStatus == 1
                                };

                                if (addressList is not null && addressList.Data is not null)
                                {
                                    for (int b = 0; b < addressList.Data.Length; b++)
                                    {
                                        if (coinOption.Data[index].MemoChainNames[a].ChainName == addressList.Data[b].ChainName && !String.IsNullOrWhiteSpace(addressList.Data[b].ContractAddress))
                                        {
                                            assetNetwork.Address = addressList.Data[b].ContractAddress;
                                            break;
                                        }
                                    }
                                }

                                if (walletCoin is not null && walletCoin.Data is not null)
                                {
                                    for (int b = 0; b < walletCoin.Data.ItemList[0].WithdrawFeeList.Length; b++)
                                    {
                                        if (walletCoin.Data.ItemList[0].WithdrawFeeList[b].ChainName == coinOption.Data[index].MemoChainNames[a].ChainName)
                                        {
                                            assetNetwork.TransferTax = double.TryParse(walletCoin.Data.ItemList[0].WithdrawFeeList[b].Fee, out double fee) ? fee : null;
                                            assetNetwork.WithdrawFee = double.TryParse(walletCoin.Data.ItemList[0].WithdrawFeeList[b].FixedFee, out double fixedFee) ? fixedFee : null;
                                            break;
                                        }
                                    }
                                }

                                assetNetworks.Add(assetNetwork);
                            }

                            if (assetNetworks.Count > 0)
                            {
                                CoinwCurrency currency = new()
                                {
                                    Currency = coinOption.Data[index].Name,
                                    Networks = [.. assetNetworks]
                                };

                                lock (locker)
                                {
                                    currencies.Add(currency);
                                }
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

                CoinwContractAddresses contractAddresses = new()
                {
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Currencies = [.. currencies]
                };

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Coinw/contractAddresses.json"), JsonSerializer.Serialize(contractAddresses, Helper.serializeOptions));

                return contractAddresses;
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "ContractAddressesUpdate disabled!");
                throw;
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<CoinwExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v1/public?command=returnSymbol").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateCoinwExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<CoinwExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<CoinwContractAddresses> Contract()
            {
                string path = Path.Combine(env.ContentRootPath, "Cache/Coinw/contractAddresses.json");

                if (File.Exists(path))
                {
                    CoinwContractAddresses? contractAddresses = JsonSerializer.Deserialize<CoinwContractAddresses>(await File.ReadAllTextAsync(path), Helper.deserializeOptions);

                    if (contractAddresses is not null && TimeSpan.FromSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - contractAddresses.Timestamp).TotalHours < 1)
                    {
                        return contractAddresses;
                    }
                }

                logger.LogInformation("Coinw: Calling ContractAddressesUpdate");
                return await ContractAddressesUpdateAsync();
            }

            //Query currect prices of pairs
            async Task<CoinwPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v1/public?command=returnTicker").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateCoinwPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<CoinwPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<CoinwExchangeInfo> exInfoTask = ExInfo();
                Task<CoinwContractAddresses> contractTask = Contract();
                Task<CoinwPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                CoinwExchangeInfo exchangeInfo = await exInfoTask;
                CoinwContractAddresses contractAddresses = await contractTask;
                CoinwPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Length; i++)
                    {
                        if (exchangeInfo.Data[i].State == 1 &&
                           (exchangeInfo.Data[i].CurrencyQuote == "USDT" || exchangeInfo.Data[i].CurrencyQuote == "USDC" || exchangeInfo.Data[i].CurrencyQuote == "TUSD"))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = exchangeInfo.Data[i].CurrencyBase.ToUpper(),
                                QuoteAsset = exchangeInfo.Data[i].CurrencyQuote.ToUpper()
                            };

                            //adding price of pair
                            if (prices.Data is not null)
                            {
                                foreach (var price in prices.Data)
                                {
                                    if (price.Key == exchangeInfo.Data[i].CurrencyPair)
                                    {
                                        if (double.TryParse(price.Value.LowestAsk, out double askPrice))
                                        {
                                            pair.AskPrice = askPrice;
                                        }

                                        if (double.TryParse(price.Value.HighestBid, out double bidPrice))
                                        {
                                            pair.BidPrice = bidPrice;
                                        }

                                        break;
                                    }
                                }
                            }

                            //adding supported networks of base asset
                            for (int b = 0; b < contractAddresses.Currencies.Length; b++)
                            {
                                if (contractAddresses.Currencies[b].Currency.Equals(exchangeInfo.Data[i].CurrencyBase, StringComparison.CurrentCultureIgnoreCase))
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

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Coinw/firstStepPairs.json"), JsonSerializer.Serialize(pairs, Helper.serializeOptions));

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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v1/public?command=returnOrderBook&symbol={baseAsset.ToUpper()}_{quoteAsset.ToUpper()}&size=20").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateCoinwOrderBook);
                string clientName = ClientService.RotatingProxyClient("coinw");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                CoinwOrderBook orderBook = JsonSerializer.Deserialize<CoinwOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Data is not null && orderBook.Data.Asks is not null)
                {
                    return Array.ConvertAll(orderBook.Data.Asks, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid && orderBook.Data is not null && orderBook.Data.Bids is not null)
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
