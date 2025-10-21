using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class TapbitExchangeInfo
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public object? Message { get; set; }

        [JsonPropertyName("data")]
        public TapbitExchangeInfoDatum[]? Data { get; set; }
    }
    public class TapbitExchangeInfoDatum
    {
        [JsonPropertyName("trade_pair_name")]
        public required string TradePairName { get; set; }

        [JsonPropertyName("base_asset")]
        public required string BaseAsset { get; set; }

        [JsonPropertyName("quote_asset")]
        public required string QuoteAsset { get; set; }

        [JsonPropertyName("price_precision")]
        public string? PricePrecision { get; set; }

        [JsonPropertyName("amount_precision")]
        public string? AmountPrecision { get; set; }

        [JsonPropertyName("taker_fee_rate")]
        public string? TakerFeeRate { get; set; }

        [JsonPropertyName("maker_fee_rate")]
        public string? MakerFeeRate { get; set; }

        [JsonPropertyName("min_amount")]
        public string? MinAmount { get; set; }

        [JsonPropertyName("price_fluctuation")]
        public string? PriceFluctuation { get; set; }

        [JsonPropertyName("min_notional")]
        public string? MinNotional { get; set; }
    }


    public class TapbitContractAddresses
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public object? Message { get; set; }

        [JsonPropertyName("data")]
        public TapbitContractDatum[]? Data { get; set; }
    }
    public class TapbitContractDatum
    {
        [JsonPropertyName("currency")]
        public required string Currency { get; set; }

        [JsonPropertyName("full_name")]
        public required string? FullName { get; set; }

        [JsonPropertyName("chains")]
        public required TapbitContractChain[] Chains { get; set; }
    }
    public class TapbitContractChain
    {
        [JsonPropertyName("chain")]
        public required string Chain { get; set; }

        [JsonPropertyName("precision")]
        public int Precision { get; set; }

        [JsonPropertyName("fee")]
        public string? Fee { get; set; }

        [JsonPropertyName("is_withdraw_enabled")]
        public bool IsWithdrawEnabled { get; set; }

        [JsonPropertyName("is_deposit_enabled")]
        public bool IsDepositEnabled { get; set; }

        [JsonPropertyName("deposit_min_confirm")]
        public int DepositMinConfirm { get; set; }

        [JsonPropertyName("withdraw_limit_min")]
        public string? WithdrawLimitMin { get; set; }
    }


    public class TapbitPrices
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public object? Message { get; set; }

        [JsonPropertyName("data")]
        public TapbitPricesDatum[]? Data { get; set; }
    }
    public class TapbitPricesDatum
    {
        [JsonPropertyName("trade_pair_name")]
        public required string TradePairName { get; set; }

        [JsonPropertyName("last_price")]
        public string? LastPrice { get; set; }

        [JsonPropertyName("highest_bid")]
        public required string HighestBid { get; set; }

        [JsonPropertyName("lowest_ask")]
        public required string LowestAsk { get; set; }

        [JsonPropertyName("highest_price_24h")]
        public string? HighestPrice24h { get; set; }

        [JsonPropertyName("lowest_price_24h")]
        public string? LowestPrice24h { get; set; }

        [JsonPropertyName("volume24h")]
        public string? Volume24h { get; set; }

        [JsonPropertyName("chg24h")]
        public string? Chg24h { get; set; }

        [JsonPropertyName("chg0h")]
        public string? Chg0h { get; set; }

        [JsonPropertyName("amount24h")]
        public string? Amount24h { get; set; }
    }


    public class TapbitOrderBook
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public object? Message { get; set; }

        [JsonPropertyName("data")]
        public TapbitOrderBookData? Data { get; set; }
    }
    public class TapbitOrderBookData
    {
        [JsonPropertyName("asks")]
        public string[][]? Asks { get; set; }

        [JsonPropertyName("bids")]
        public string[][]? Bids { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }
}
