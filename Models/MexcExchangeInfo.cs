using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class MexcExchangeInfo
    {
        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("serverTime")]
        public long? ServerTime { get; set; }

        [JsonPropertyName("rateLimits")]
        public object[]? RateLimits { get; set; }

        [JsonPropertyName("exchangeFilters")]
        public object[]? ExchangeFilters { get; set; }

        [JsonPropertyName("symbols")]
        public required MexcSymbol[] Symbols { get; set; }
    }
    public class MexcSymbol
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }

        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("baseAsset")]
        public required string BaseAsset { get; set; }

        [JsonPropertyName("baseAssetPrecision")]
        public int? BaseAssetPrecision { get; set; }

        [JsonPropertyName("quoteAsset")]
        public required string QuoteAsset { get; set; }

        [JsonPropertyName("quotePrecision")]
        public int? QuotePrecision { get; set; }

        [JsonPropertyName("quoteAssetPrecision")]
        public int? QuoteAssetPrecision { get; set; }

        [JsonPropertyName("baseCommissionPrecision")]
        public int? BaseCommissionPrecision { get; set; }

        [JsonPropertyName("quoteCommissionPrecision")]
        public int? QuoteCommissionPrecision { get; set; }

        [JsonPropertyName("orderTypes")]
        public string[]? OrderTypes { get; set; }

        [JsonPropertyName("isSpotTradingAllowed")]
        public bool? IsSpotTradingAllowed { get; set; }

        [JsonPropertyName("isMarginTradingAllowed")]
        public bool? IsMarginTradingAllowed { get; set; }

        [JsonPropertyName("quoteAmountPrecision")]
        public string? QuoteAmountPrecision { get; set; }

        [JsonPropertyName("baseSizePrecision")]
        public string? BaseSizePrecision { get; set; }

        [JsonPropertyName("permissions")]
        public string[]? Permissions { get; set; }

        [JsonPropertyName("filters")]
        public object[]? Filters { get; set; }

        [JsonPropertyName("maxQuoteAmount")]
        public string? MaxQuoteAmount { get; set; }

        [JsonPropertyName("makerCommission")]
        public string? MakerCommission { get; set; }

        [JsonPropertyName("takerCommission")]
        public required string TakerCommission { get; set; }

        [JsonPropertyName("quoteAmountPrecisionMarket")]
        public string? QuoteAmountPrecisionMarket { get; set; }

        [JsonPropertyName("maxQuoteAmountMarket")]
        public string? MaxQuoteAmountMarket { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("tradeSideType")]
        public int? TradeSideType { get; set; }
    }


    public class MexcContractAddress
    {
        [JsonPropertyName("coin")]
        public required string Coin { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("networkList")]
        public required MexcNetworklist[] NetworkList { get; set; }
    }
    public class MexcNetworklist
    {
        [JsonPropertyName("coin")]
        public string? Coin { get; set; }

        [JsonPropertyName("depositDesc")]
        public string? DepositDesc { get; set; }

        [JsonPropertyName("depositEnable")]
        public required bool DepositEnable { get; set; }

        [JsonPropertyName("minConfirm")]
        public int? MinConfirm { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("netWork")]
        public required string Network { get; set; }

        [JsonPropertyName("withdrawEnable")]
        public required bool WithdrawEnable { get; set; }

        [JsonPropertyName("withdrawFee")]
        public string? WithdrawFee { get; set; }

        [JsonPropertyName("withdrawIntegerMultiple")]
        public object? WithdrawIntegerMultiple { get; set; }

        [JsonPropertyName("withdrawMax")]
        public string? WithdrawMax { get; set; }

        [JsonPropertyName("withdrawMin")]
        public string? WithdrawMin { get; set; }

        [JsonPropertyName("sameAddress")]
        public bool? SameAddress { get; set; }

        [JsonPropertyName("contract")]
        public required string Contract { get; set; }

        [JsonPropertyName("withdrawTips")]
        public string? WithdrawTips { get; set; }

        [JsonPropertyName("depositTips")]
        public string? DepositTips { get; set; }
    }
    

    public class MexcPrice
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("bidPrice")]
        public required string BidPrice { get; set; }
        [JsonPropertyName("bidQty")]
        public required string BidQty { get; set; }
        [JsonPropertyName("askPrice")]
        public required string AskPrice { get; set; }
        [JsonPropertyName("askQty")]
        public required string AskQty { get; set; }
    }


    public class MexcOrderBook
    {
        [JsonPropertyName("lastUpdateId")]
        public long? LastUpdateId { get; set; }
        [JsonPropertyName("bids")]
        public required string[][] Bids { get; set; }
        [JsonPropertyName("asks")]
        public required string[][] Asks { get; set; }
        [JsonPropertyName("timestamp")]
        public long? Timestamp { get; set; }
    }
}
