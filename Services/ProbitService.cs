using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class ProbitService(IHttpClientFactory httpClientFactory, ILogger<ProbitService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 16;
        public string ExchangeName { get; } = "ProBit Global";
        public string[] Hosts { get; } = ["api.probit.com", "www.probit.com", "static.probit.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateProbitExchangeInfo(HttpRequestMessage request)
        {
            ProbitExchangeInfo model = ClientService.Deserialize<ProbitExchangeInfo>(request);

            if (model.Data is null)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateProbitContractAddresses(HttpRequestMessage request)
        {
            ProbitContractAddresses model = ClientService.Deserialize<ProbitContractAddresses>(request);

            if (model.Data is null)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateProbitContracts(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<ProbitContracts>(request);
        }
        private void ValidateProbitPrices(HttpRequestMessage request)
        {
            ProbitPrices model = ClientService.Deserialize<ProbitPrices>(request);

            if (model.Data is null)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateProbitOrderBook(HttpRequestMessage request)
        {
            ProbitOrderBook model = ClientService.Deserialize<ProbitOrderBook>(request);

            if (model.Data is null)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<ProbitExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/exchange/v1/market").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateProbitExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<ProbitExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details
            async Task<ProbitContractAddresses> Contract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[1]}/api/exchange/v1/currency_with_platform").WithVersion();
                request.Headers.Host = Hosts[1];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateProbitContractAddresses);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<ProbitContractAddresses>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query the smart contract address
            async Task<ProbitContracts> Contracts()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[2]}/ua-cfg/wdwarn.en-us.json").WithVersion();
                request.Headers.Host = Hosts[2];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateProbitContracts);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<ProbitContracts>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<ProbitPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/exchange/v1/ticker").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateProbitPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<ProbitPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<ProbitExchangeInfo> exInfoTask = ExInfo();
                Task<ProbitContractAddresses> contractTask = Contract();
                Task<ProbitContracts> contractsTask = Contracts();
                Task<ProbitPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, contractsTask, pricesTask]);

                ProbitExchangeInfo exchangeInfo = await exInfoTask;
                ProbitContractAddresses contractAddresses = await contractTask;
                ProbitContracts contracts = await contractsTask;
                ProbitPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Length; i++)
                    {
                        if (!exchangeInfo.Data[i].Closed &&
                           (exchangeInfo.Data[i].QuoteCurrencyId == "USDT" || exchangeInfo.Data[i].QuoteCurrencyId == "USDC" || exchangeInfo.Data[i].QuoteCurrencyId == "TUSD"))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = exchangeInfo.Data[i].BaseCurrencyId.ToUpper(),
                                QuoteAsset = exchangeInfo.Data[i].QuoteCurrencyId.ToUpper()
                            };

                            //adding price of pair
                            if (prices.Data is not null)
                            {
                                for (int a = 0; a < prices.Data.Length; a++)
                                {
                                    if (prices.Data[a].MarketId.Equals(exchangeInfo.Data[i].Id, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (double.TryParse(prices.Data[a].Last, out double lastPrice))
                                        {
                                            pair.AskPrice = lastPrice;
                                            pair.BidPrice = lastPrice;
                                        }

                                        break;
                                    }
                                }
                            }

                            //adding supported networks of base asset
                            if (contractAddresses.Data is not null)
                            {
                                for (int b = 0; b < contractAddresses.Data.Length; b++)
                                {
                                    if (contractAddresses.Data[b].Id.Equals(exchangeInfo.Data[i].BaseCurrencyId, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        List<AssetNetwork> baseAssetNetworks = [];

                                        for (int c = 0; c < contractAddresses.Data[b].Platform.Length; c++)
                                        {
                                            if (!contractAddresses.Data[b].Platform[c].DepositSuspended || !contractAddresses.Data[b].Platform[c].WithdrawalSuspended)
                                            {
                                                AssetNetwork assetNetwork = new()
                                                {
                                                    NetworkName = contractAddresses.Data[b].Platform[c].Id,
                                                    DepositEnable = !contractAddresses.Data[b].Platform[c].DepositSuspended,
                                                    WithdrawEnable = !contractAddresses.Data[b].Platform[c].WithdrawalSuspended,
                                                    DepositTax = double.TryParse(contractAddresses.Data[b].Platform[c].DepositFee?.Proportional, out double proportional) ? proportional : null
                                                };

                                                //Withraw fee
                                                if (contractAddresses.Data[b].Platform[c].WithdrawalFee.Length > 0)
                                                {
                                                    if (contractAddresses.Data[b].Platform[c].WithdrawalFee[0].CurrencyId.Equals(exchangeInfo.Data[i].BaseCurrencyId, StringComparison.CurrentCultureIgnoreCase))
                                                    {
                                                        if (double.TryParse(contractAddresses.Data[b].Platform[c].WithdrawalFee[0].Amount, out double withdrawFee))
                                                        {
                                                            if (pair.AskPrice != 0)
                                                            {
                                                                assetNetwork.WithdrawFee = pair.AskPrice * withdrawFee;
                                                            }

                                                            else if (pair.BidPrice != 0)
                                                            {
                                                                assetNetwork.WithdrawFee = pair.BidPrice * withdrawFee;
                                                            }
                                                        }
                                                    }

                                                    else if (contractAddresses.Data[b].Platform[c].WithdrawalFee[0].CurrencyId == "USDT" ||
                                                        contractAddresses.Data[b].Platform[c].WithdrawalFee[0].CurrencyId == "USDC")
                                                    {
                                                        if (double.TryParse(contractAddresses.Data[b].Platform[c].WithdrawalFee[0].Amount, out double amount))
                                                        {
                                                            assetNetwork.WithdrawFee = amount;
                                                        }
                                                    }

                                                    else
                                                    {
                                                        //Find price
                                                        if (prices.Data is not null)
                                                        {
                                                            for (int a = 0; a < prices.Data.Length; a++)
                                                            {
                                                                if (prices.Data[a].MarketId.Equals(contractAddresses.Data[b].Platform[c].WithdrawalFee[0].CurrencyId + "-USDT", StringComparison.CurrentCultureIgnoreCase))
                                                                {
                                                                    if (double.TryParse(prices.Data[a].Last, out double lastPrice) && double.TryParse(contractAddresses.Data[b].Platform[c].WithdrawalFee[0].Amount, out double amount))
                                                                    {
                                                                        assetNetwork.WithdrawFee = lastPrice * amount;
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                //Adding contract address
                                                for (int d = 0; d < contracts.Withdrawal.Length; d++)
                                                {
                                                    if (contracts.Withdrawal[d].CurrencyId.Equals(exchangeInfo.Data[i].BaseCurrencyId, StringComparison.CurrentCultureIgnoreCase) &&
                                                        contracts.Withdrawal[d].PlatformId.Equals(contractAddresses.Data[b].Platform[c].Id, StringComparison.CurrentCultureIgnoreCase) &&
                                                        contracts.Withdrawal[d].Notice[0].Type.Equals("smart_contract", StringComparison.CurrentCultureIgnoreCase) && !String.IsNullOrWhiteSpace(contracts.Withdrawal[d].Notice[0].Data))
                                                    {
                                                        assetNetwork.Address = contracts.Withdrawal[d].Notice[0].Data;
                                                        break;
                                                    }
                                                }

                                                if (assetNetwork.Address is null)
                                                {
                                                    for (int d = 0; d < contracts.Deposit.Length; d++)
                                                    {
                                                        if (contracts.Deposit[d].CurrencyId.Equals(exchangeInfo.Data[i].BaseCurrencyId, StringComparison.CurrentCultureIgnoreCase) &&
                                                            contracts.Deposit[d].PlatformId.Equals(contractAddresses.Data[b].Platform[c].Id, StringComparison.CurrentCultureIgnoreCase) &&
                                                            contracts.Deposit[d].Notice[0].Type.Equals("smart_contract", StringComparison.CurrentCultureIgnoreCase) && !String.IsNullOrWhiteSpace(contracts.Deposit[d].Notice[0].Data))
                                                        {
                                                            assetNetwork.Address = contracts.Deposit[d].Notice[0].Data;
                                                            break;
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
                            }

                            if ((pair.BidPrice != 0 || pair.AskPrice != 0) && pair.BaseAssetNetworks.Length > 0)
                            {
                                pairs.Add(pair);
                            }
                        }
                    }
                }

                using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Probit/firstStepPairs.json"));
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/exchange/v1/order_book?market_id={baseAsset.ToUpper()}-{quoteAsset.ToUpper()}").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.LimitRuleName, "ProbitRatelimitGroup3");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateProbitOrderBook);
                string clientName = ClientService.RotatingProxyClient("probit");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                ProbitOrderBook orderBook = JsonSerializer.Deserialize<ProbitOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                List<string[]> asks = [];
                List<string[]> bids = [];

                //Sort and fill asks and bids
                if (orderBook.Data is not null)
                {
                    for (int i = 0; i < orderBook.Data.Length; i++)
                    {
                        if (orderBook.Data[i].Side == "sell")
                        {
                            asks.Add([orderBook.Data[i].Price, orderBook.Data[i].Quantity]);
                        }

                        else if (orderBook.Data[i].Side == "buy")
                        {
                            bids.Add([orderBook.Data[i].Price, orderBook.Data[i].Quantity]);
                        }
                    }
                }

                for (int i = 0; i < asks.Count; i++)
                {
                    for (int a = 0; a < asks.Count; a++)
                    {
                        if (double.Parse(asks[i][0]) < double.Parse(asks[a][0]))
                        {
                            asks.Insert(a, asks[i]);
                            asks.RemoveAt(i + 1);
                            break;
                        }
                    }
                }
                for (int i = 0; i < bids.Count; i++)
                {
                    for (int a = 0; a < bids.Count; a++)
                    {
                        if (double.Parse(bids[i][0]) > double.Parse(bids[a][0]))
                        {
                            bids.Insert(a, bids[i]);
                            bids.RemoveAt(i + 1);
                            break;
                        }
                    }
                }

                if (askOrBid)
                {
                    return Array.ConvertAll([.. asks], innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid)
                {
                    return Array.ConvertAll([.. bids], innerArray => Array.ConvertAll(innerArray, double.Parse));
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
