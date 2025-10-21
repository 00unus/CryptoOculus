using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class AscendexService(IHttpClientFactory httpClientFactory, ILogger<AscendexService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 19;
        public string ExchangeName { get; } = "AscendEX";
        public string[] Hosts { get; } = ["ascendex.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateAscendexExchangeInfo(HttpRequestMessage request)
        {
            AscendexExchangeInfo model = ClientService.Deserialize<AscendexExchangeInfo>(request);

            if (model.Data is null || model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateAscendexContractAddresses(HttpRequestMessage request)
        {
            AscendexContractAddresses model = ClientService.Deserialize<AscendexContractAddresses>(request);

            if (model.Data is null || model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateAscendexPrices(HttpRequestMessage request)
        {
            AscendexPrices model = ClientService.Deserialize<AscendexPrices>(request);

            if (model.Data is null || model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }
        private void ValidateAscendexOrderBook(HttpRequestMessage request)
        {
            AscendexOrderBook model = ClientService.Deserialize<AscendexOrderBook>(request);

            if (model.Data is null || model.Code != 0)
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        public async Task<Pair[]> GetPairs()
        {
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<AscendexExchangeInfo> ExInfo()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/pro/v1/cash/products").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateAscendexExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<AscendexExchangeInfo>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currency details and the smart contract address
            async Task<AscendexContractAddresses> Contract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/pro/v2/assets").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateAscendexContractAddresses);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<AscendexContractAddresses>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            //Query currect prices of pairs
            async Task<AscendexPrices> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/pro/v1/spot/ticker").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateAscendexPrices);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<AscendexPrices>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<AscendexExchangeInfo> exInfoTask = ExInfo();
                Task<AscendexContractAddresses> contractTask = Contract();
                Task<AscendexPrices> pricesTask = Prices();

                await Task.WhenAll([exInfoTask, contractTask, pricesTask]);

                AscendexExchangeInfo exchangeInfo = await exInfoTask;
                AscendexContractAddresses contractAddresses = await contractTask;
                AscendexPrices prices = await pricesTask;

                List<Pair> pairs = [];

                if (exchangeInfo.Data is not null)
                {
                    for (int i = 0; i < exchangeInfo.Data.Length; i++)
                    {
                        string[] assets = exchangeInfo.Data[i].Symbol.Split("/");
                        if (assets[1].Equals("USDT", StringComparison.CurrentCultureIgnoreCase) || assets[1].Equals("USDC", StringComparison.CurrentCultureIgnoreCase) || assets[1].Equals("TUSD", StringComparison.CurrentCultureIgnoreCase))
                        {
                            //Adding basic info of pair
                            Pair pair = new()
                            {
                                ExchangeId = ExchangeId,
                                ExchangeName = ExchangeName,
                                BaseAsset = assets[0].ToUpper(),
                                QuoteAsset = assets[1].ToUpper()
                            };

                            //adding price of pair
                            if (prices.Data is not null)
                            {
                                for (int a = 0; a < prices.Data.Length; a++)
                                {
                                    if (prices.Data[a].Symbol.Equals(exchangeInfo.Data[i].Symbol, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (double.TryParse(prices.Data[a].Ask[0], out double askPrice))
                                        {
                                            pair.AskPrice = askPrice;
                                        }

                                        if (double.TryParse(prices.Data[a].Bid[0], out double bidPrice))
                                        {
                                            pair.BidPrice = bidPrice;
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
                                    if (contractAddresses.Data[b].AssetCode.Equals(assets[0], StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        List<AssetNetwork> baseAssetNetworks = [];

                                        for (int c = 0; c < contractAddresses.Data[b].BlockChain.Length; c++)
                                        {
                                            if (contractAddresses.Data[b].BlockChain[c].AllowDeposit || contractAddresses.Data[b].BlockChain[c].AllowWithdraw)
                                            {
                                                AssetNetwork assetNetwork = new()
                                                {
                                                    NetworkName = contractAddresses.Data[b].BlockChain[c].ChainName,
                                                    DepositEnable = contractAddresses.Data[b].BlockChain[c].AllowDeposit,
                                                    WithdrawEnable = contractAddresses.Data[b].BlockChain[c].AllowWithdraw
                                                };

                                                //Withraw fee
                                                if (double.TryParse(contractAddresses.Data[b].BlockChain[c].WithdrawFee, out double withdrawFee))
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
                            }

                            if ((pair.AskPrice != 0 || pair.BidPrice != 0) && pair.BaseAssetNetworks.Length > 0)
                            {
                                pairs.Add(pair);
                            }
                        }
                    }
                }

                await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Ascendex/firstStepPairs.json"), JsonSerializer.Serialize(pairs, Helper.serializeOptions));

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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/pro/v1/depth?symbol={baseAsset.ToUpper()}/{quoteAsset.ToUpper()}").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateAscendexOrderBook);
                string clientName = ClientService.RotatingProxyClient("ascendex");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                AscendexOrderBook orderBook = JsonSerializer.Deserialize<AscendexOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid && orderBook.Data is not null && orderBook.Data.Data.Asks is not null)
                {
                    return Array.ConvertAll(orderBook.Data.Data.Asks, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid && orderBook.Data is not null && orderBook.Data.Data.Bids is not null)
                {
                    return Array.ConvertAll(orderBook.Data.Data.Bids, innerArray => Array.ConvertAll(innerArray, double.Parse));
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
