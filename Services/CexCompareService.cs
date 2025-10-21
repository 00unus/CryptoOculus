using CryptoOculus.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class CexCompareService(ILogger<CexCompareService> logger, IWebHostEnvironment env, CexsLoadService cexsLoad, DnsUpdateService dnsUpdate, LocalCexCompareService localCexCompare, HoneypotIsService honeypotIs, DataService dataService, TelegramService telegram) : BackgroundService
    {
        private readonly double _profit = 3;
        private readonly double _maxAmount = 10000;
        private readonly double _maxNonProfitableUpdateTime = 5;

        private readonly ConcurrentDictionary<int, IExchange> _exchangeServices = cexsLoad.GetCexs();
        private readonly List<IDnsUpdate> _dnsUpdates = [honeypotIs];

        private readonly static ConcurrentDictionary<string, ConcurrentDictionary<string, Spread>> _spreads = [];
        //private readonly static List<CompareResult> _results = JsonSerializer.Deserialize<List<CompareResult>>(File.ReadAllText("Cache/ResultsLast.json"))!;
        private readonly static Lock _spreadsLocker = new();

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _dnsUpdates.AddRange(_exchangeServices.Values.OfType<IDnsUpdate>());
            await dnsUpdate.UpdateAsync([.. _dnsUpdates], cancellationToken);

            List<Task> tasks = [];
            tasks.Add(Compare(cancellationToken));
            tasks.Add(UpdateCompareResults(cancellationToken));
            tasks.Add(telegram.UpdateSubscribedUsers(cancellationToken));
            tasks.Add(dnsUpdate.UpdateService([.. _dnsUpdates], cancellationToken));
            await Task.WhenAll(tasks);
        }

        public async Task Compare(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    DateTime now = DateTime.Now;

                    logger.LogInformation("Starting Exchanges");

                    List<Task<Pair[]>> pairsTasks = [];

                    foreach (IExchange exchange in _exchangeServices.Values)
                    {
                        pairsTasks.Add(exchange.GetPairs());
                    }

                    Pair[][] pairs = await Task.WhenAll(pairsTasks);

                    /*pairsTasks[0] = binance.GetPairs();
                    pairsTasks[1] = mexc.GetPairs();
                    pairsTasks[2] = bybit.GetPairs();
                    pairsTasks[3] = gate.GetPairs();
                    pairsTasks[4] = bitget.GetPairs();
                    pairsTasks[5] = bitmart.GetPairs();
                    pairsTasks[6] = bingx.GetPairs();
                    pairsTasks[7] = kucoin.GetPairs();
                    pairsTasks[8] = htx.GetPairs();
                    pairsTasks[9] = okx.GetPairs();
                    pairsTasks[10] = lbank.GetPairs();
                    pairsTasks[11] = coinw.GetPairs();
                    pairsTasks[12] = coinex.GetPairs();
                    pairsTasks[13] = bitfinex.GetPairs();
                    pairsTasks[14] = xtcom.GetPairs();
                    pairsTasks[15] = digifinex.GetPairs();
                    pairsTasks[16] = probit.GetPairs();
                    pairsTasks[17] = phemex.GetPairs();
                    ///pairsTasks[18] = tapbit.GetPairs();
                    pairsTasks[19] = ascendex.GetPairs();
                    pairsTasks[20] = poloniex.GetPairs();
                    pairsTasks[21] = kraken.GetPairs();*/

                    /*await NetworksDebug(pairs);

                    Console.WriteLine("Ok");
                    Console.ReadLine();*/

                    logger.LogInformation("Starting local compare");

                    List<Spread> spreads = await localCexCompare.Compare(pairs, 0.005);

                    logger.LogInformation("Starting compare...");

                    List<Task> tasks = [];
                    Lock resultsLocker = new();

                    HashSet<string> idsToRemove = [];
                    Lock idsToRemoveLocker = new();

                    for (int i = 0; i < spreads.Count; i++)
                    {
                        int buffer = i;
                        tasks.Add(GetOrderBookAsync(buffer));
                    }

                    async Task GetOrderBookAsync(int index)
                    {
                        Task<double[][]?> buyOrderBook = OrderBook(spreads[index].BuyExchangeId, spreads[index].BuyExchangePair.BaseAsset, spreads[index].BuyExchangePair.QuoteAsset, true);
                        Task<double[][]?> sellOrderBook = OrderBook(spreads[index].SellExchangeId, spreads[index].SellExchangePair.BaseAsset, spreads[index].SellExchangePair.QuoteAsset, false);

                        await Task.WhenAll(buyOrderBook, sellOrderBook);

                        if (await buyOrderBook is not null && await sellOrderBook is not null)
                        {
                            //Adding tax
                            for (int i = 0; i < spreads[index].BuyExchangePair.BaseAssetNetworks.Length; i++)
                            {
                                string? address = spreads[index].BuyExchangePair.BaseAssetNetworks[i].Address;
                                if (!String.IsNullOrWhiteSpace(address))
                                {
                                    double? tax = await honeypotIs.GetTransferTax(address);

                                    if (tax is not null)
                                    {
                                        spreads[index].BuyExchangePair.BaseAssetNetworks[i].TransferTax = tax;
                                    }
                                }
                            }

                            Spread newResult = GoldenSectionSearch(spreads[index], _maxAmount, await buyOrderBook, await sellOrderBook);

                            if (newResult.Profit > _profit)
                            {
                                newResult.LastProfitableDate = DateTimeOffset.UtcNow;

                                lock (resultsLocker)
                                {
                                    spreads[index] = newResult;
                                }
                            }

                            else
                            {
                                lock (idsToRemoveLocker)
                                {
                                    idsToRemove.Add(spreads[index].Id);
                                }
                            }
                        }

                        else
                        {
                            lock (idsToRemoveLocker)
                            {
                                idsToRemove.Add(spreads[index].Id);
                            }
                        }
                    }

                    await Task.WhenAll(tasks);

                    spreads.RemoveAll(spread => idsToRemove.Contains(spread.Id));

                    logger.LogInformation("Adding new spreads... Completed in {seconds}s", (DateTime.Now - now).TotalSeconds);

                    await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/Spreads.json"), JsonSerializer.Serialize(spreads, Helper.serializeOptions), cancellationToken);

                    lock (_spreadsLocker)
                    {
                        foreach (Spread newSpread in spreads)
                        {
                            if (_spreads.TryGetValue(newSpread.BuyExchangePair.BaseAsset, out ConcurrentDictionary<string, Spread>? innerSpreads) &&
                                innerSpreads.TryGetValue(newSpread.Id, out Spread? spread))
                            {
                                spread.BuyExchangePair.BaseAssetNetworks = newSpread.BuyExchangePair.BaseAssetNetworks;
                                spread.SellExchangePair.BaseAssetNetworks = newSpread.SellExchangePair.BaseAssetNetworks;

                                idsToRemove.Add(newSpread.Id);
                            }
                        }
                    }

                    spreads.RemoveAll(spread => idsToRemove.Contains(spread.Id));

                    if (spreads.Count > 0)
                    {
                        List<string> newSpreads = [];

                        lock (_spreadsLocker)
                        {
                            foreach (Spread newSpread in spreads)
                            {
                                if (_spreads.TryGetValue(newSpread.BuyExchangePair.BaseAsset, out ConcurrentDictionary<string, Spread>? innerSpreads))
                                {
                                    innerSpreads.TryAdd(newSpread.Id, newSpread);
                                }

                                else
                                {
                                    _spreads.TryAdd(newSpread.BuyExchangePair.BaseAsset, new ConcurrentDictionary<string, Spread> { [newSpread.Id] = newSpread });
                                    newSpreads.Add(newSpread.BuyExchangePair.BaseAsset);
                                }
                            }
                        }

                        telegram.SendNewSpreads([.. newSpreads], cancellationToken);
                    }

                    await Task.Delay(20000, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Compare disabled!");
                throw;
            }
        }

        private async Task UpdateCompareResults(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    DateTime now = DateTime.Now;

                    List<Task> tasks = [];
                    List<Spread> spreadsToRemove = [];
                    Lock idsToRemoveLocker = new();

                    Dictionary<string, Spread> spreads = [];

                    lock (_spreadsLocker)
                    {
                        foreach (var innerSpreads in _spreads)
                        {
                            foreach (var spread in innerSpreads.Value)
                            {
                                spreads[spread.Key] = new(spread.Value);
                            }
                        }
                    }

                    foreach (var spread in spreads)
                    {
                        tasks.Add(GetOrderBookAsync(spread.Value));
                    }

                    async Task GetOrderBookAsync(Spread spread)
                    {
                        Task<double[][]?> buyOrderBook = OrderBook(spread.BuyExchangeId, spread.BuyExchangePair.BaseAsset, spread.BuyExchangePair.QuoteAsset, true);
                        Task<double[][]?> sellOrderBook = OrderBook(spread.SellExchangeId, spread.SellExchangePair.BaseAsset, spread.SellExchangePair.QuoteAsset, false);

                        await Task.WhenAll(buyOrderBook, sellOrderBook);

                        if (await buyOrderBook is not null && await sellOrderBook is not null)
                        {
                            Spread newSpread = GoldenSectionSearch(spread, _maxAmount, await buyOrderBook, await sellOrderBook);

                            if (newSpread.Profit > _profit || (DateTimeOffset.UtcNow - newSpread.LastProfitableDate).TotalMinutes < _maxNonProfitableUpdateTime)
                            {
                                lock (_spreadsLocker)
                                {
                                    if (_spreads.TryGetValue(newSpread.BuyExchangePair.BaseAsset, out ConcurrentDictionary<string, Spread>? innerSpreads) && innerSpreads.TryGetValue(newSpread.Id, out Spread? _spread))
                                    {
                                        if (newSpread.Profit > _profit)
                                        {
                                            _spread.LastProfitableDate = DateTimeOffset.UtcNow;
                                        }

                                        _spread.BuyExchangePair.AskPrice = newSpread.BuyExchangePair.AskPrice;
                                        _spread.BuyExchangePair.AskQuantity = newSpread.BuyExchangePair.AskQuantity;
                                        _spread.BuyExchangePair.AskAmount = newSpread.BuyExchangePair.AskAmount;
                                        _spread.BuyExchangePair.AskPriceRange = newSpread.BuyExchangePair.AskPriceRange;
                                        _spread.BuyExchangePair.Asks = newSpread.BuyExchangePair.Asks;

                                        _spread.SellExchangePair.BidPrice = newSpread.SellExchangePair.BidPrice;
                                        _spread.SellExchangePair.BidQuantity = newSpread.SellExchangePair.BidQuantity;
                                        _spread.SellExchangePair.BidAmount = newSpread.SellExchangePair.BidAmount;
                                        _spread.SellExchangePair.BidPriceRange = newSpread.SellExchangePair.BidPriceRange;
                                        _spread.SellExchangePair.Bids = newSpread.SellExchangePair.Bids;

                                        _spread.Profit = newSpread.Profit;
                                        _spread.SpreadPercent = newSpread.SpreadPercent;
                                    }
                                }
                            }

                            else
                            {
                                lock (idsToRemoveLocker)
                                {
                                    spreadsToRemove.Add(spread);
                                }
                            }
                        }

                        else
                        {
                            lock (idsToRemoveLocker)
                            {
                                spreadsToRemove.Add(spread);
                            }
                        }
                    }

                    await Task.WhenAll([.. tasks]);

                    int resultsCount = 0;

                    lock (_spreadsLocker)
                    {
                        foreach (Spread spread in spreadsToRemove)
                        {
                            if (_spreads.TryGetValue(spread.BuyExchangePair.BaseAsset, out ConcurrentDictionary<string, Spread>? innerSpreads))
                            {
                                innerSpreads.TryRemove(spread.Id, out _);
                            }
                        }

                        foreach (var innerSpreads in _spreads)
                        {
                            resultsCount += innerSpreads.Value.Count;
                        }
                    }

                    logger.LogInformation("Updating results({count})... Completed in {seconds}s", [resultsCount, (DateTime.Now - now).TotalSeconds]);
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "UpdateCompareResults disabled!");
                throw;
            }
        }

        private async Task<double[][]?> OrderBook(int exchangeId, string baseAsset, string quoteAsset, bool askOrBid)
        {
            if (_exchangeServices.TryGetValue(exchangeId, out IExchange? exchange))
            {
                return await exchange.OrderBook(baseAsset, quoteAsset, askOrBid);
            }

            Exception ex = new($"exchangeId: {exchangeId} not found!");
            logger.LogError(ex, "{ex.Message}", ex.Message);
            throw ex;
        }

        private static Spread CalculateProfit(Spread result, double[][] buyOrderBook, double[][] sellOrderBook, double amount)
        {
            //Calculating trading fee and order
            Order? buyOrder = CalculateOrder(buyOrderBook, amount * (1 - Helper.GetExchangeFee(result.BuyExchangeId)));

            if (buyOrder is not null)
            {
                double buyOrderQuantity = buyOrder.Quantity;

                //Calculating withdraw fee
                double? withdrawFee = result.BuyExchangePair.BaseAssetNetworks[0].WithdrawFee;
                if (withdrawFee is not null)
                {
                    buyOrderQuantity -= (double)withdrawFee / buyOrder.Price;
                }

                //Calculating blockchain transfer tax
                double? transferTax = result.BuyExchangePair.BaseAssetNetworks[0].TransferTax;
                if (transferTax is not null)
                {
                    buyOrderQuantity -= buyOrder.Quantity * (double)transferTax;
                }

                //Calculating deposit tax
                double? depositTax = result.SellExchangePair.BaseAssetNetworks[0].DepositTax;
                if (depositTax is not null)
                {
                    buyOrderQuantity -= buyOrderQuantity * (double)depositTax;
                }

                //Calculating trading fee and order
                Order? sellOrder = CalculateOrder(sellOrderBook, tradeQuantity: buyOrderQuantity * (1 - Helper.GetExchangeFee(result.SellExchangeId)));

                if (sellOrder is not null)//&& sellOrder.Amount - amount > 0
                {
                    result.Profit = sellOrder.Amount - amount;
                    result.SpreadPercent = (sellOrder.Amount / amount) - 1;

                    result.BuyExchangePair.AskPrice = buyOrder.Price;
                    result.BuyExchangePair.AskQuantity = buyOrder.Quantity * (1 + Helper.GetExchangeFee(result.BuyExchangeId));
                    result.BuyExchangePair.AskAmount = amount;
                    result.BuyExchangePair.AskPriceRange = buyOrder.PriceRange;
                    result.BuyExchangePair.Asks = buyOrderBook;

                    result.SellExchangePair.BidPrice = sellOrder.Price;
                    result.SellExchangePair.BidQuantity = sellOrder.Quantity;
                    result.SellExchangePair.BidAmount = sellOrder.Amount;
                    result.SellExchangePair.BidPriceRange = sellOrder.PriceRange;
                    result.SellExchangePair.Bids = sellOrderBook;

                    return result;
                }
            }

            result.Profit = 0;
            result.SpreadPercent = 0;

            return result;
        }

        private static Order? CalculateOrder(double[][] orderBook, double? tradeAmount = null, double? tradeQuantity = null)
        {
            double price = 0;
            double quantity = 0;
            double amount = 0;

            double? reqQuantity = tradeQuantity;
            double? reqAmount = tradeAmount;

            for (int i = 0; i < orderBook.Length; i++)
            {
                if ((tradeAmount is not null && amount >= tradeAmount * 0.99999) || (tradeQuantity is not null && quantity >= tradeQuantity * 0.99999))
                {
                    return new Order() { Price = price, Quantity = quantity, Amount = amount, PriceRange = [orderBook[0][0], orderBook[i][0]] };
                }

                if (reqQuantity is not null)
                {
                    if (tradeQuantity > quantity + orderBook[i][1])
                    {
                        price = (price * quantity) + (orderBook[i][0] * orderBook[i][1]);
                        quantity += orderBook[i][1];
                        price /= quantity;
                        reqQuantity -= orderBook[i][1];
                    }

                    else
                    {
                        price = (price * quantity) + (orderBook[i][0] * (double)reqQuantity);
                        quantity += (double)reqQuantity;
                        price /= quantity;
                        reqQuantity -= (double)reqQuantity;
                        i--;
                    }
                }

                else if (reqAmount is not null)
                {
                    if (tradeAmount > amount + (orderBook[i][0] * orderBook[i][1]))
                    {
                        price = (price * quantity) + (orderBook[i][0] * orderBook[i][1]);
                        quantity += orderBook[i][1];
                        price /= quantity;
                        reqAmount -= orderBook[i][0] * orderBook[i][1];
                    }

                    else
                    {
                        price = (price * quantity) + (orderBook[i][0] * ((double)reqAmount / orderBook[i][0]));
                        quantity += (double)reqAmount / orderBook[i][0];
                        price /= quantity;
                        reqAmount -= orderBook[i][0] * ((double)reqAmount / orderBook[i][0]);
                        i--;
                    }
                }

                amount = price * quantity;
            }

            return null;
        }

        private static bool IsInBlackList(string id, Spread result, Dictionary<string, SpreadsBlacklistItem> spreadsBlacklist, Dictionary<string, CoinsBlacklistItem> coinsBlacklist)
        {
            if (spreadsBlacklist.TryGetValue(id, out _) ||
                coinsBlacklist.TryGetValue($"{result.BuyExchangePair.BaseAsset}%||%ALL", out _) ||
                coinsBlacklist.TryGetValue($"{result.SellExchangePair.BaseAsset}%||%ALL", out _) ||
                coinsBlacklist.TryGetValue($"{result.BuyExchangePair.BaseAsset}%||%{result.BuyExchangeId}", out _) ||
                coinsBlacklist.TryGetValue($"{result.SellExchangePair.BaseAsset}%||%{result.SellExchangeId}", out _))
            {
                return true;
            }

            return false;
        }

        private static Spread GoldenSectionSearch(Spread compareResult, double amount, double[][]? buyOrderBook = null, double[][]? sellOrderBook = null)
        {
            Spread result = new(compareResult);
            buyOrderBook ??= result.BuyExchangePair.Asks;
            sellOrderBook ??= result.SellExchangePair.Bids;

            if (buyOrderBook is not null && sellOrderBook is not null)
            {
                double min = result.BuyExchangePair.BaseAssetNetworks[0].WithdrawFee ?? 0;
                double max = amount;
                double phi = (1 + Math.Sqrt(5)) / 2;

                double x1 = max - (max - min) / phi;
                double x2 = min + (max - min) / phi;

                Spread spreadX1 = CalculateProfit(new(result), buyOrderBook, sellOrderBook, x1);
                Spread spreadX2 = CalculateProfit(new(result), buyOrderBook, sellOrderBook, x2);

                while ((max - min) > 0.1)
                {
                    if (spreadX1.Profit > spreadX2.Profit)
                    {
                        max = x2;
                        x2 = x1;
                        spreadX2 = new(spreadX1);
                        x1 = max - (max - min) / phi;
                        spreadX1 = CalculateProfit(spreadX1, buyOrderBook, sellOrderBook, x1);
                    }

                    /*else if (resultX1.Profit == 0)
                    {
                        max = x1;
                        x1 = max - (max - min) / phi;
                        x2 = min + (max - min) / phi;
                        resultX1 = CalculateProfit(new(result), buyOrderBook, sellOrderBook, x1); //resultX1
                        resultX2 = CalculateProfit(new(result), buyOrderBook, sellOrderBook, x2); //resultX2
                    }

                    else if (resultX2.Profit == 0)
                    {
                        max = x2;
                        x1 = max - (max - min) / phi;
                        x2 = min + (max - min) / phi;
                        resultX1 = CalculateProfit(new(result), buyOrderBook, sellOrderBook, x1); //resultX1
                        resultX2 = CalculateProfit(new(result), buyOrderBook, sellOrderBook, x2); //resultX2
                    }*/

                    else
                    {
                        min = x1;
                        x1 = x2;
                        spreadX1 = new(spreadX2);
                        x2 = min + (max - min) / phi;
                        spreadX2 = CalculateProfit(spreadX2, buyOrderBook, sellOrderBook, x2);
                    }
                }

                return spreadX2.Profit >= spreadX1.Profit ? spreadX2 : spreadX1;
            }

            return compareResult;
        }

        public static Spread? GetSpread(string id, double profit, double amount, Dictionary<string, SpreadsBlacklistItem> spreadsBlacklist, Dictionary<string, CoinsBlacklistItem> coinsBlacklist, HashSet<int> buyExBlacklist, HashSet<int> sellExBlacklist)
        {
            Spread? spread = null;

            lock (_spreadsLocker)
            {
                string[] idSplit = id.Split("%||%");

                if (_spreads.TryGetValue(idSplit[2], out ConcurrentDictionary<string, Spread>? innerSpreads) &&
                    innerSpreads.TryGetValue(id, out Spread? _spread))
                {
                    spread = new(_spread);
                }
            }

            if (spread is not null && !IsInBlackList(id, spread, spreadsBlacklist, coinsBlacklist) && !buyExBlacklist.Contains(spread.BuyExchangeId) && !sellExBlacklist.Contains(spread.SellExchangeId))
            {
                Spread newSpread = GoldenSectionSearch(spread, amount);

                if (newSpread.Profit > profit)
                {
                    return newSpread;
                }
            }

            return null;
        }

        public static OrderedDictionary<string, Spread> GetSpreads(string id, double profit, double amount, Dictionary<string, SpreadsBlacklistItem> spreadsBlacklist, Dictionary<string, CoinsBlacklistItem> coinsBlacklist, HashSet<int> buyExBlacklist, HashSet<int> sellExBlacklist)
        {
            OrderedDictionary<string, Spread> spreads = [];

            lock (_spreadsLocker)
            {
                if (_spreads.TryGetValue(id, out ConcurrentDictionary<string, Spread>? innerSpreads))
                {
                    foreach (var spread in innerSpreads)
                    {
                        spreads[spread.Key] = spread.Value;
                    }
                }
            }

            List<string> spreadsToRemove = [];

            foreach (var spread in spreads)
            {
                if (!IsInBlackList(spread.Key, spread.Value, spreadsBlacklist, coinsBlacklist) && !buyExBlacklist.Contains(spread.Value.BuyExchangeId) && !sellExBlacklist.Contains(spread.Value.SellExchangeId))
                {
                    Spread newResult = GoldenSectionSearch(spread.Value, amount);

                    if (newResult.Profit >= profit)
                    {
                        spreads[spread.Key] = newResult;
                    }

                    else
                    {
                        spreadsToRemove.Add(spread.Key);
                    }
                }
            }

            foreach (var spreadId in spreadsToRemove)
            {
                spreads.Remove(spreadId);
            }

            spreads = new(spreads.OrderByDescending(s => s.Value.Profit));

            return spreads;
        }

        public static OrderedDictionary<string, OrderedDictionary<string, Spread>> GetInnerSpreads(double profit, double amount, Dictionary<string, SpreadsBlacklistItem> spreadsBlacklist, Dictionary<string, CoinsBlacklistItem> coinsBlacklist, HashSet<int> buyExBlacklist, HashSet<int> sellExBlacklist)
        {
            OrderedDictionary<string, OrderedDictionary<string, Spread>> spreads = [];

            lock (_spreadsLocker)
            {
                foreach (var innerSpreads in _spreads)
                {
                    OrderedDictionary<string, Spread> newInnerSpreads = [];

                    foreach (var spread in innerSpreads.Value)
                    {
                        newInnerSpreads[spread.Key] = new(spread.Value);
                    }

                    spreads[innerSpreads.Key] = newInnerSpreads;
                }
            }

            List<(string id, string spreadId)> spreadsToRemove = [];

            foreach (var innerSpreads in spreads)
            {
                foreach (var spread in innerSpreads.Value)
                {
                    if (!IsInBlackList(spread.Key, spread.Value, spreadsBlacklist, coinsBlacklist) && !buyExBlacklist.Contains(spread.Value.BuyExchangeId) && !sellExBlacklist.Contains(spread.Value.SellExchangeId))
                    {
                        Spread newResult = GoldenSectionSearch(spread.Value, amount);

                        if (newResult.Profit >= profit)
                        {
                            innerSpreads.Value[spread.Key] = newResult;
                        }

                        else
                        {
                            spreadsToRemove.Add((innerSpreads.Key, spread.Key));
                        }
                    }
                }
            }

            foreach ((string id, string spreadId) in spreadsToRemove)
            {
                spreads[id].Remove(spreadId);
            }

            List<string> innerSpreadsToRemove = [];

            foreach (var innerSpreads in spreads)
            {
                if (innerSpreads.Value.Count < 1)
                {
                    innerSpreadsToRemove.Add(innerSpreads.Key);
                }
            }

            foreach (string innerSpreads in innerSpreadsToRemove)
            {
                spreads.Remove(innerSpreads);
            }

            foreach (var innerSpreads in spreads)
            {
                spreads[innerSpreads.Key] = new(innerSpreads.Value.OrderByDescending(s => s.Value.Profit));
            }

            spreads = new(spreads.OrderByDescending(s => s.Value.ElementAt(0).Value.Profit));

            return spreads;
        }

        public static bool IsSpreadActual(string id)
        {
            lock (_spreadsLocker)
            {
                string[] idSplit = id.Split("%||%");

                if (_spreads.TryGetValue(idSplit[2], out ConcurrentDictionary<string, Spread>? innerSpreads) &&
                    innerSpreads.TryGetValue(id, out _))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task NetworksDebug(Pair[][] pairs)
        {
            List<string>[] networkNames = new List<string>[pairs.Length];
            for (int i = 0; i < networkNames.Length; i++)
            {
                networkNames[i] = [];
            }

            for (int i = 0; i < pairs.Length; i++)
            {
                for (int a = 0; a < pairs[i].Length; a++)
                {
                    for (int b = 0; b < pairs[i][a].BaseAssetNetworks.Length; b++)
                    {
                        string? networkName = pairs[i][a].BaseAssetNetworks[b].NetworkName;

                        if (!String.IsNullOrWhiteSpace(networkName) && dataService.GetNetworkId(networkName) is null)
                        {
                            bool isAdded = false;

                            for (int c = 0; c < networkNames[i].Count; c++)
                            {
                                if (networkNames[i][c] == networkName)
                                {
                                    isAdded = true;
                                    break;
                                }
                            }

                            if (!isAdded)
                            {
                                networkNames[i].Add(networkName);
                            }
                        }
                    }
                }
            }

            await File.WriteAllTextAsync(Path.Combine(env.ContentRootPath, "Cache/NetworksDebug.json"), JsonSerializer.Serialize(networkNames, Helper.serializeOptions));
        }
    }
}