using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class PoloniexService(IHttpClientFactory httpClientFactory, ILogger<PoloniexService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 20;
        public string ExchangeName { get; } = "Poloniex";
        public string[] Hosts { get; } = ["api.poloniex.com"];
        public string[] Ips { get; set; } = [];

        private void ValidatePoloniexExchangeInfo(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<PoloniexExchangeInfo[]>(request);
        }
        private void ValidatePoloniexContractAddress(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<PoloniexContractAddress[]>(request);
        }
        private void ValidatePoloniexPrice(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<PoloniexPrice[]>(request);
        }
        private void ValidatePoloniexOrderBook(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<PoloniexOrderBook>(request);
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<PoloniexExchangeInfo[]> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/markets").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.LimitRuleName, "PoloniexPublicData1");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidatePoloniexExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<PoloniexExchangeInfo[]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<PoloniexContractAddress[]> Contract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/currencies").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.LimitRuleName, "PoloniexPublicData1");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidatePoloniexContractAddress);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<PoloniexContractAddress[]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<PoloniexPrice[]> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/markets/ticker24h").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.LimitRuleName, "PoloniexPublicData1");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidatePoloniexPrice);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<PoloniexPrice[]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<PoloniexExchangeInfo[]> exInfoTask = ExInfo();
                Task<PoloniexContractAddress[]> contractTask = Contract();
                Task<PoloniexPrice[]> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                PoloniexExchangeInfo[] exchangeInfo = await exInfoTask;
                PoloniexContractAddress[] contractAddresses = await contractTask;
                PoloniexPrice[] prices = await pricesTask;

                List<Pair> pairs = [];

                for (int i = 0; i < exchangeInfo.Length; i++)
                {
                    if (exchangeInfo[i].State.Equals("NORMAL", StringComparison.CurrentCultureIgnoreCase) &&
                        (exchangeInfo[i].QuoteCurrencyName.Equals("USDT", StringComparison.CurrentCultureIgnoreCase) || exchangeInfo[i].QuoteCurrencyName.Equals("USDC", StringComparison.CurrentCultureIgnoreCase) || exchangeInfo[i].QuoteCurrencyName.Equals("TUSD", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        //Adding basic info of pair
                        Pair pair = new()
                        {
                            ExchangeId = ExchangeId,
                            ExchangeName = ExchangeName,
                            BaseAsset = exchangeInfo[i].BaseCurrencyName.ToUpper(),
                            QuoteAsset = exchangeInfo[i].QuoteCurrencyName.ToUpper()
                        };

                        //adding price of pair
                        for (int a = 0; a < prices.Length; a++)
                        {
                            if (prices[a].Symbol.Equals(exchangeInfo[i].Symbol, StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (double.TryParse(prices[a].Ask, out double askPrice))
                                {
                                    pair.AskPrice = askPrice;
                                }

                                if (double.TryParse(prices[a].Bid, out double bidPrice))
                                {
                                    pair.BidPrice = bidPrice;
                                }

                                break;
                            }
                        }

                        //adding supported networks of base asset
                        for (int b = 0; b < contractAddresses.Length; b++)
                        {
                            if (contractAddresses[b].Coin.Equals(exchangeInfo[i].BaseCurrencyName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                List<AssetNetwork> baseAssetNetworks = [];
                                for (int c = 0; c < contractAddresses[b].NetworkList.Length; c++)
                                {
                                    if (contractAddresses[b].NetworkList[c].DepositEnable || contractAddresses[b].NetworkList[c].WithdrawalEnable)
                                    {
                                        AssetNetwork assetNetwork = new()
                                        {
                                            NetworkName = contractAddresses[b].NetworkList[c].Blockchain,
                                            DepositEnable = contractAddresses[b].NetworkList[c].DepositEnable,
                                            WithdrawEnable = contractAddresses[b].NetworkList[c].WithdrawalEnable
                                        };

                                        if (!String.IsNullOrWhiteSpace(contractAddresses[b].NetworkList[c].ContractAddress))
                                        {
                                            assetNetwork.Address = contractAddresses[b].NetworkList[c].ContractAddress;
                                        }

                                        //Withraw fee
                                        if (double.TryParse(contractAddresses[b].NetworkList[c].WithdrawFee, out double withdrawFee))
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

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Poloniex/firstStepPairs.json"), JsonSerializer.Serialize(pairs, Helper.serializeOptions));

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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/markets/{baseAsset.ToUpper()}_{quoteAsset.ToUpper()}/orderBook?limit=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.LimitRuleName, "PoloniexPublicData2");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidatePoloniexOrderBook);
                string clientName = ClientService.RotatingProxyClient("poloniex");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                PoloniexOrderBook orderBook = JsonSerializer.Deserialize<PoloniexOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                //Changing response structure
                List<string[]> asks = [];
                List<string[]> bids = [];

                if (orderBook.Asks is not null)
                {
                    for (int i = 0; i < orderBook.Asks.Length; i += 2)
                    {
                        asks.Add([orderBook.Asks[i], orderBook.Asks[i + 1]]);
                    }
                }

                if (orderBook.Bids is not null)
                {
                    for (int i = 0; i < orderBook.Bids.Length; i += 2)
                    {
                        bids.Add([orderBook.Bids[i], orderBook.Bids[i + 1]]);
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
