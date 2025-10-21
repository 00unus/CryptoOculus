using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace CryptoOculus.Models
{
    public class Spread
    {
        public required string Id { get; set; }
        public DateTimeOffset LastProfitableDate { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset FindingDate { get; set; } = DateTimeOffset.UtcNow;
        public required int BuyExchangeId { get; set; }
        public required int SellExchangeId { get; set; }
        public required Pair BuyExchangePair { get; set; }
        public required Pair SellExchangePair { get; set; }
        public double Profit { get; set; }
        public double SpreadPercent { get; set; }

        public Spread() { }

        [SetsRequiredMembers]
        public Spread(Spread spread)
        {
            Id = spread.Id;
            LastProfitableDate = spread.LastProfitableDate;
            FindingDate = spread.FindingDate;
            BuyExchangeId = spread.BuyExchangeId;
            SellExchangeId = spread.SellExchangeId;
            Profit = spread.Profit;
            SpreadPercent = spread.SpreadPercent;

            BuyExchangePair = new Pair(spread.BuyExchangePair);
            SellExchangePair = new Pair(spread.SellExchangePair);
        }

        public static Dictionary<string, Spread> CloneDictionary(ConcurrentDictionary<string, Spread> spreads)
        {
            Dictionary<string, Spread> newSpreads = [];

            foreach (var spread in spreads)
            {
                newSpreads.Add(spread.Key, new(spread.Value));
            }

            return newSpreads;
        }
    }
}