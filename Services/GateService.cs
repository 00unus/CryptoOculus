using CryptoOculus.Models;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class GateService(IHttpClientFactory httpClientFactory, ILogger<GateService> logger, IWebHostEnvironment env) : IExchange, IDnsUpdate
    {
        public int ExchangeId { get; } = 3;
        public string ExchangeName { get; } = "Gate";
        public string[] Hosts { get; } = ["api.gateio.ws", "www.gate.com"];
        public string[] Ips { get; set; } = [];

        private void ValidateGateExchangeInfo(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<GateExchangeInfo[]>(request);
        }
        private void ValidateGatePrice(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<GatePrice[]>(request);
        }
        private void ValidateGateCurrencyChains(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<GateCurrencyChains[]>(request);
        }
        private void ValidateGateOrderBook(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<GateOrderBook>(request);
        }
        private void ValidateGateCoinsDepositWithdrawFeeList(HttpRequestMessage request)
        {
            GateCoinsDepositWithdrawFee model = ClientService.Deserialize<GateCoinsDepositWithdrawFee>(request);

            if (model.Data is null || model.Code != 0 || model.Message != "success")
            {
                throw new HttpRequestException("Incorrect response or Ex error code");
            }
        }

        private async Task<GateContractAddresses> ContractAddressesUpdateAsync(GateExchangeInfo[] exchangeInfo)
        {
            List<Task> tasks = [];
            Lock locker = new();

            //Selecting tradable currencies and сhecking for repeats
            List<string> bases = [];
            for (int i = 0; i < exchangeInfo.Length; i++)
            {
                if (exchangeInfo[i].Trade_status == "tradable")
                {
                    bool isExist = false;
                    for (int a = 0; a < bases.Count; a++)
                    {
                        if (exchangeInfo[i].Base == bases[a])
                        {
                            isExist = true;
                            break;
                        }

                    }

                    if (!isExist)
                    {
                        bases.Add(exchangeInfo[i].Base);
                    }
                }
            }

            //Getting fee list
            List<GateCoinsDepositWithdrawFeeList> coinsDepositWithdrawFeeList = [];
            async Task<int> GetCoinsDepositWithdrawFeeList(int index)
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[1]}/api/web/v1/withdraw/depositwithdraw/getCoinsDepositWithdrawFee?page={index}&page_size=200").WithVersion();
                request.Headers.Host = Hosts[1];
                request.Headers.Add("sentry-trace", "66ebe57632294f0c831a743e6ce1c0a3-8cff7f9a105e1209-0");
                request.Headers.Add("sec-ch-ua-platform", "Windows");
                request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");
                request.Headers.Add("accept", "*/*");
                request.Headers.Add("sec-ch-ua", "\"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"138\", \"Google Chrome\";v=\"138\"");
                request.Headers.Add("sec-ch-ua-mobile", "?0");
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateGateCoinsDepositWithdrawFeeList);
                string clientName = ClientService.RotatingProxyClient("gate2");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);
                GateCoinsDepositWithdrawFee coinsDepositWithdrawFee = JsonSerializer.Deserialize<GateCoinsDepositWithdrawFee>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                lock (locker)
                {
                    coinsDepositWithdrawFeeList.AddRange(coinsDepositWithdrawFee.Data!.List);
                }

                return coinsDepositWithdrawFee.Data!.PageCount;
            }
            int length = await GetCoinsDepositWithdrawFeeList(1);
            for (int i = 2; i < length; i++)
            {
                int buffer = i;
                tasks.Add(GetCoinsDepositWithdrawFeeList(buffer));
            }

            await Task.WhenAll(tasks);

            tasks = [];

            List<GateCurrency> currencies = [];

            for (int i = 0; i < bases.Count; i++)
            {
                int buffer = i;

                tasks.Add(GetCurrencyAsync(buffer));
            }

            async Task GetCurrencyAsync(int index)
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v4/wallet/currency_chains?currency={bases[index].ToUpper()}").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateGateCurrencyChains);
                string clientName = ClientService.RotatingProxyClient("gate");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                GateCurrencyChains[] currencyChains = JsonSerializer.Deserialize<GateCurrencyChains[]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                List<AssetNetwork> assetNetworkList = [];

                for (int i = 0; i < currencyChains.Length; i++)
                {
                    if (currencyChains[i].IsDisabled == 0 &&
                        (currencyChains[i].IsDepositDisabled == 0 || currencyChains[i].IsWithdrawDisabled == 0))
                    {
                        AssetNetwork assetNetwork = new()
                        {
                            NetworkName = currencyChains[i].Chain
                        };

                        if (!String.IsNullOrWhiteSpace(currencyChains[i].ContractAddress))
                        {
                            assetNetwork.Address = currencyChains[i].ContractAddress;
                        }

                        if (currencyChains[i].IsDepositDisabled == 0)
                        {
                            assetNetwork.DepositEnable = true;
                        }

                        if (currencyChains[i].IsWithdrawDisabled == 0)
                        {
                            assetNetwork.WithdrawEnable = true;
                        }

                        for (int a = 0; a < coinsDepositWithdrawFeeList.Count; a++)
                        {
                            if (coinsDepositWithdrawFeeList[a].Coin.Equals(bases[index], StringComparison.CurrentCultureIgnoreCase))
                            {
                                for (int b = 0; b < coinsDepositWithdrawFeeList[a].Chains.Length; b++)
                                {
                                    if (coinsDepositWithdrawFeeList[a].Chains[b].Chain.Equals(currencyChains[i].Chain, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        assetNetwork.WithdrawFee = double.TryParse(coinsDepositWithdrawFeeList[a].Chains[b].WithdrawTxfee, out double withdrawFee) ? withdrawFee : null;
                                        assetNetwork.TransferTax = double.TryParse(coinsDepositWithdrawFeeList[a].Chains[b].WithdrawFeePercent, out double transferTax) ? transferTax / 100 : null;
                                        assetNetwork.DepositTax = coinsDepositWithdrawFeeList[a].Chains[b].DepositTxfee;
                                    }
                                }
                            }
                        }

                        assetNetworkList.Add(assetNetwork);
                    }
                }

                if (assetNetworkList.Count > 0)
                {
                    GateCurrency currency = new()
                    {
                        Currency = bases[index].ToUpper(),
                        Networks = [.. assetNetworkList]
                    };

                    lock (locker)
                    {
                        currencies.Add(currency);
                    }
                }
            }

            await Task.WhenAll(tasks);

            GateContractAddresses contractAddresses = new()
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Currencies = [.. currencies]
            };

            using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Gate/contractAddresses.json"));
            sw.Write(JsonSerializer.Serialize(contractAddresses, Helper.serializeOptions));

            return contractAddresses;
        }

        public async Task<Pair[]> GetPairs()
        {
            /* GATE AUTHENTIFICATION FOR FUTURE
            byte[] hashBytes = SHA512.HashData(Encoding.UTF8.GetBytes(""));
            var bodyHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            HMACSHA512 hmac = new(Encoding.UTF8.GetBytes("8eb777ea0b0cfd9950486ee8b4423c396848f7ac3fc7e2f19d25793b7ffc6079"));
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string data = $"GET\n/api/v4/wallet/withdrawals\n\n{bodyHash}\n{now}";
            byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

            WebRequest request = WebRequest.Create("https://api.gateio.ws/api/v4/wallet/withdrawals");
            request.Headers.Add("KEY", "446260df2c51768f7c37fd7f7477d344");
            request.Headers.Add("Timestamp", now.ToString());
            request.Headers.Add("SIGN", BitConverter.ToString(signature).Replace("-", "").ToLower());

            StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream());
            StreamWriter sw = new StreamWriter("cache/gatecontract.json");
            sw.Write(sr.ReadToEnd());
            sw.Close();
            sr.Close();*/
            //Getting Current exchange trading rules and symbol information (Exchange info)
            async Task<(GateExchangeInfo[], GateContractAddresses)> ExInfoContract()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v4/spot/currency_pairs").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateGateExchangeInfo);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                GateExchangeInfo[] exchangeInfo = JsonSerializer.Deserialize<GateExchangeInfo[]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                //Query currency details and the smart contract address
                string path = Path.Combine(env.ContentRootPath, "Cache/Gate/contractAddresses.json");

                if (File.Exists(path))
                {
                    GateContractAddresses? contractAddresses = JsonSerializer.Deserialize<GateContractAddresses>(await File.ReadAllTextAsync(path), Helper.deserializeOptions);

                    if (contractAddresses is not null && TimeSpan.FromSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - contractAddresses.Timestamp).TotalHours < 1)
                    {
                        return (exchangeInfo, contractAddresses);
                    }
                }

                logger.LogInformation("Gate: Calling ContractAddressesUpdate");
                return (exchangeInfo, await ContractAddressesUpdateAsync(exchangeInfo));
            }

            //Query currect prices of pairs
            async Task<GatePrice[]> Prices()
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v4/spot/tickers").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateGatePrice);
                HttpResponseMessage response = await httpClientFactory.CreateClient("Standard").SendAsync(request);

                return JsonSerializer.Deserialize<GatePrice[]>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;
            }

            try
            {
                Task<(GateExchangeInfo[], GateContractAddresses)> exInfoContractTask = ExInfoContract();
                Task<GatePrice[]> pricesTask = Prices();

                await Task.WhenAll([exInfoContractTask, pricesTask]);

                (GateExchangeInfo[] exchangeInfo, GateContractAddresses contractAddresses) = await exInfoContractTask;
                GatePrice[] prices = await pricesTask;

                List<Pair> pairs = [];

                for (int i = 0; i < exchangeInfo.Length; i++)
                {
                    if (exchangeInfo[i].Trade_status == "tradable" &&
                       (exchangeInfo[i].Quote == "USDT" || exchangeInfo[i].Quote == "USDC" || exchangeInfo[i].Quote == "TUSD"))
                    {
                        //Adding basic info of pair
                        Pair pair = new()
                        {
                            ExchangeId = ExchangeId,
                            ExchangeName = ExchangeName,
                            BaseAsset = exchangeInfo[i].Base.ToUpper(),
                            QuoteAsset = exchangeInfo[i].Quote.ToUpper()
                        };

                        //adding price of pair
                        for (int a = 0; a < prices.Length; a++)
                        {
                            if (prices[a].Currency_pair == exchangeInfo[i].Id)
                            {
                                if (double.TryParse(prices[a].Lowest_ask, out double askPrice))
                                {
                                    pair.AskPrice = askPrice;
                                }

                                if (double.TryParse(prices[a].Highest_bid, out double bidPrice))
                                {
                                    pair.BidPrice = bidPrice;
                                }

                                break;
                            }
                        }

                        //adding supported networks of base asset
                        for (int b = 0; b < contractAddresses.Currencies.Length; b++)
                        {
                            if (contractAddresses.Currencies[b].Currency.Equals(exchangeInfo[i].Base, StringComparison.CurrentCultureIgnoreCase))
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
                                            WithdrawEnable = contractAddresses.Currencies[b].Networks[c].WithdrawEnable
                                        };

                                        if (contractAddresses.Currencies[b].Networks[c].Address is not null)
                                        {
                                            assetNetwork.Address = contractAddresses.Currencies[b].Networks[c].Address;
                                        }

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

                using StreamWriter sw = new(Path.Combine(env.ContentRootPath, "Cache/Gate/firstStepPairs.json"));
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/api/v4/spot/order_book?currency_pair={baseAsset.ToUpper()}_{quoteAsset.ToUpper()}&limit=100").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateGateOrderBook);
                string clientName = ClientService.RotatingProxyClient("gate");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                GateOrderBook orderBook = JsonSerializer.Deserialize<GateOrderBook>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (askOrBid)
                {
                    return Array.ConvertAll(orderBook.Asks, innerArray => Array.ConvertAll(innerArray, double.Parse));
                }

                else if (!askOrBid)
                {
                    return Array.ConvertAll(orderBook.Bids, innerArray => Array.ConvertAll(innerArray, double.Parse));
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
