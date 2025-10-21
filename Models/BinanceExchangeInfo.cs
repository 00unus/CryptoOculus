using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class BinanceExchangeInfo
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
        public required BinanceSymbol[] Symbols { get; set; }
    }
    public class BinanceSymbol
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("status")]
        public required string Status { get; set; }
        [JsonPropertyName("baseAsset")]
        public required string BaseAsset { get; set; }
        public int BaseAssetPrecision { get; set; }
        [JsonPropertyName("quoteAsset")]
        public required string QuoteAsset { get; set; }
        public int QuotePrecision { get; set; }
        public int QuoteAssetPrecision { get; set; }
        public int BaseCommissionPrecision { get; set; }
        public int QuoteCommissionPrecision { get; set; }
        public string[]? OrderTypes { get; set; }
        public bool IcebergAllowed { get; set; }
        public bool OcoAllowed { get; set; }
        public bool OtoAllowed { get; set; }
        public bool QuoteOrderQtyMarketAllowed { get; set; }
        public bool AllowTrailingStop { get; set; }
        public bool CancelReplaceAllowed { get; set; }
        public bool IsSpotTradingAllowed { get; set; }
        public bool IsMarginTradingAllowed { get; set; }
        public object[]? Filters { get; set; }
        public object[]? Permissions { get; set; }
        public string[][]? PermissionSets { get; set; }
        public string? DefaultSelfTradePreventionMode { get; set; }
        public string[]? AllowedSelfTradePreventionModes { get; set; }
    }


    public class BinanceContractAddresses
    {
        [JsonPropertyName("coin")]
        public required string Coin { get; set; }

        [JsonPropertyName("depositAllEnable")]
        public bool? DepositAllEnable { get; set; }

        [JsonPropertyName("withdrawAllEnable")]
        public bool? WithdrawAllEnable { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("free")]
        public string? Free { get; set; }

        [JsonPropertyName("locked")]
        public string? Locked { get; set; }

        [JsonPropertyName("freeze")]
        public string? Freeze { get; set; }

        [JsonPropertyName("withdrawing")]
        public string? Withdrawing { get; set; }

        [JsonPropertyName("ipoing")]
        public string? Ipoing { get; set; }

        [JsonPropertyName("ipoable")]
        public string? Ipoable { get; set; }

        [JsonPropertyName("storage")]
        public string? Storage { get; set; }

        [JsonPropertyName("isLegalMoney")]
        public bool? IsLegalMoney { get; set; }

        [JsonPropertyName("trading")]
        public bool? Trading { get; set; }

        [JsonPropertyName("networkList")]
        public required BinanceNetworkList[] NetworkList { get; set; }
    }
    public class BinanceNetworkList
    {
        [JsonPropertyName("network")]
        public required string Network { get; set; }

        [JsonPropertyName("coin")]
        public string? Coin { get; set; }

        [JsonPropertyName("withdrawIntegerMultiple")]
        public string? WithdrawIntegerMultiple { get; set; }

        [JsonPropertyName("isDefault")]
        public bool? IsDefault { get; set; }

        [JsonPropertyName("depositEnable")]
        public required bool DepositEnable { get; set; }

        [JsonPropertyName("withdrawEnable")]
        public required bool WithdrawEnable { get; set; }

        [JsonPropertyName("depositDesc")]
        public string? DepositDesc { get; set; }

        [JsonPropertyName("withdrawDesc")]
        public string? WithdrawDesc { get; set; }

        [JsonPropertyName("specialTips")]
        public string? SpecialTips { get; set; }

        [JsonPropertyName("specialWithdrawTips")]
        public string? SpecialWithdrawTips { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("resetAddressStatus")]
        public bool? ResetAddressStatus { get; set; }

        [JsonPropertyName("addressRegex")]
        public string? AddressRegex { get; set; }

        [JsonPropertyName("memoRegex")]
        public string? MemoRegex { get; set; }

        [JsonPropertyName("withdrawFee")]
        public string? WithdrawFee { get; set; }

        [JsonPropertyName("withdrawMin")]
        public string? WithdrawMin { get; set; }

        [JsonPropertyName("withdrawMax")]
        public string? WithdrawMax { get; set; }

        [JsonPropertyName("depositDust")]
        public string? DepositDust { get; set; }

        [JsonPropertyName("minConfirm")]
        public int? MinConfirm { get; set; }

        [JsonPropertyName("unLockConfirm")]
        public int? UnLockConfirm { get; set; }

        [JsonPropertyName("sameAddress")]
        public bool? SameAddress { get; set; }

        [JsonPropertyName("estimatedArrivalTime")]
        public int? EstimatedArrivalTime { get; set; }

        [JsonPropertyName("busy")]
        public bool? Busy { get; set; }

        [JsonPropertyName("contractAddressUrl")]
        public string? ContractAddressUrl { get; set; }

        [JsonPropertyName("contractAddress")]
        public string? ContractAddress { get; set; }
    }


    public class BinancePrice
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


    public class BinanceOrderBook
    {
        public long? LastUpdateId { get; set; }
        [JsonPropertyName("bids")]
        public required string[][] Bids { get; set; }
        [JsonPropertyName("asks")]
        public required string[][] Asks { get; set; }
    }
}
