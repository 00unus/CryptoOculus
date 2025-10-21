using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class BitfinexService(IHttpClientFactory httpClientFactory, ILogger<BitfinexService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 13;
        public string ExchangeName { get; } = "Bitfinex";
        public string[] Hosts { get; } = ["api-pub.bitfinex.com"];
        public string[] Ips { get; set; } = [];

        private static BitfinexSymbol[]? _bitfinexSymbols;

        private void ValidateBitfinexPrices(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<object[][]>(request);
        }
        private void ValidateBitfinexChains(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<object[][][]>(request);
        }
        private void ValidateBitfinexStatus(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<object[][][]>(request);
        }
        private void ValidateBitfinexOrderBook(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<object[][]>(request);
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Prices
            async Task<object[][]> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/tickers?symbols=ALL").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitfinexPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<object[][]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Getting chains
            async Task<object[][][]> Chains()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/conf/pub:map:tx:method").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitfinexChains);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<object[][][]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Getting chain statuses
            async Task<object[][][]> Status()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/conf/pub:info:tx:status").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitfinexStatus);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<object[][][]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<object[][]> pricesTask = Prices();
                Task<object[][][]> chainsTask = Chains();
                Task<object[][][]> statusesTask = Status();

                await Task.WhenAll([pricesTask, chainsTask, statusesTask]);

                object[][] prices = await pricesTask;
                object[][][] chains = await chainsTask;
                object[][][] statuses = await statusesTask;

                List<Pair> pairs = [];

                for (int i = 0; i < prices.Length; i++)
                {
                    string quoteAsset = "UST";
                    string? symbol = prices[i][0].ToString();
                    if (symbol is not null && symbol.EndsWith(quoteAsset) && symbol.Length > 4)
                    {
                        string baseAsset = symbol[1..^quoteAsset.Length].Replace(":", "");

                        //Adding basic info and prices of pair
                        Pair pair = new()
                        {
                            ExchangeId = ExchangeId,
                            ExchangeName = ExchangeName,
                            BaseAsset = baseAsset.ToUpper(),
                            QuoteAsset = "USDT"
                        };

                        if (double.TryParse(prices[i][3].ToString(), out double askPrice))
                        {
                            pair.AskPrice = askPrice;
                        }

                        if (double.TryParse(prices[i][1].ToString(), out double bidPrice))
                        {
                            pair.BidPrice = bidPrice;
                        }

                        //Adding withdraw and deposit status
                        List<AssetNetwork> baseAssetNetworks = [];

                        for (int a = 0; a < chains[0].Length; a++)
                        {
                            string? jsonCurrency = chains[0][a][1].ToString();
                            if (!string.IsNullOrWhiteSpace(jsonCurrency))
                            {
                                string[]? currency = JsonSerializer.Deserialize<string[]>(jsonCurrency);
                                if (currency is not null && currency[0].Equals(baseAsset, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    //Find With and Dep status
                                    for (int b = 0; b < statuses[0].Length; b++)
                                    {
                                        if (statuses[0][b][0].ToString()!.Equals(chains[0][a][0].ToString(), StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            if (statuses[0][b][1].ToString() == "1" || statuses[0][b][2].ToString() == "1")
                                            {
                                                AssetNetwork assetNetwork = new() { NetworkName = statuses[0][b][0].ToString() };

                                                if (statuses[0][b][1].ToString() == "1")
                                                {
                                                    assetNetwork.DepositEnable = true;
                                                }

                                                if (statuses[0][b][2].ToString() == "1")
                                                {
                                                    assetNetwork.WithdrawEnable = true;
                                                }

                                                baseAssetNetworks.Add(assetNetwork);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (baseAssetNetworks.Count > 0)
                        {
                            pair.BaseAssetNetworks = [.. baseAssetNetworks];
                        }

                        if ((pair.BidPrice != 0 || pair.AskPrice != 0) && pair.BaseAssetNetworks.Length > 0)
                        {
                            pairs.Add(pair);
                        }
                    }
                }

                //Saving symbols
                List<BitfinexSymbol> symbols = [];

                for (int i = 0; i < prices.Length; i++)
                {
                    string quoteAsset = "UST";
                    string? symbol = prices[i][0].ToString();
                    if (symbol is not null && symbol.EndsWith(quoteAsset) && symbol.Length > 4)
                    {
                        string baseAsset = symbol[1..^quoteAsset.Length].Replace(":", "");
                        BitfinexSymbol bitfinexSymbol = new()
                        {
                            Symbol = symbol,
                            BaseAsset = baseAsset.ToUpper(),
                            QuoteAsset = "USDT"
                        };

                        symbols.Add(bitfinexSymbol);
                    }
                }

                _bitfinexSymbols = [.. symbols];

                using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Bitfinex/symbols.json"));
                sw.Write(JsonSerializer.Serialize<List<BitfinexSymbol>>(symbols, Helper.serializeOptions));

                using StreamWriter sww = new(Path.Combine(env.ContentRootPath, "Cache/Bitfinex/firstStepPairs.json"));
                sww.Write(JsonSerializer.Serialize<List<Pair>>(pairs, Helper.serializeOptions));

                return [.. pairs];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Service disabled!");
                return [];
            }
        }

        public string? GetSymbol(string baseAsset, string quoteAsset)
        {
            if (_bitfinexSymbols is null)
            {
                string path = Path.Combine(env.ContentRootPath, "Cache/Bitfinex/symbols.json");

                if (File.Exists(path))
                {
                    _bitfinexSymbols = JsonSerializer.Deserialize<BitfinexSymbol[]>(File.ReadAllText(path), Helper.deserializeOptions);
                }
            }

            if (_bitfinexSymbols is not null)
            {
                for (int a = 0; a < _bitfinexSymbols.Length; a++)
                {
                    if (_bitfinexSymbols[a].BaseAsset.Equals(baseAsset, StringComparison.CurrentCultureIgnoreCase) && _bitfinexSymbols[a].QuoteAsset.Equals(quoteAsset, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return _bitfinexSymbols[a].Symbol;
                    }
                }
            }

            return null;
        }

        public async Task<double[][]?> OrderBook(string baseAsset, string quoteAsset, bool askOrBid)
        {
            try
            {
                string? symbol = GetSymbol(baseAsset, quoteAsset);
                if (symbol is null)
                {
                    return null;
                }

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/book/{symbol}/P0?len=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateBitfinexOrderBook);
                string clientName = ClientService.RotatingProxyClient("bitfinex");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                double[][] orderBook = JsonSerializer.Deserialize<double[][]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid)
                {
                    List<double[]> returnOrderBook = [];

                    for (int a = 0; a < orderBook.Length; a++)
                    {
                        if (orderBook[a][2] < 0)
                        {
                            returnOrderBook.Add([orderBook[a][0], Math.Abs(orderBook[a][2])]);
                        }
                    }

                    return [.. returnOrderBook];
                }

                else if (!askOrBid)
                {
                    List<double[]> returnOrderBook = [];

                    for (int a = 0; a < orderBook.Length; a++)
                    {
                        if (orderBook[a][2] > 0)
                        {
                            returnOrderBook.Add([orderBook[a][0], orderBook[a][2]]);
                        }
                    }

                    return [.. returnOrderBook];
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
