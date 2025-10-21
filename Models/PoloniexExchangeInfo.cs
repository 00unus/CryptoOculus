using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class PoloniexExchangeInfo
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }

        [JsonPropertyName("baseCurrencyName")]
        public required string BaseCurrencyName { get; set; }

        [JsonPropertyName("quoteCurrencyName")]
        public required string QuoteCurrencyName { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("state")]
        public required string State { get; set; }

        [JsonPropertyName("visibleStartTime")]
        public object? VisibleStartTime { get; set; }

        [JsonPropertyName("tradableStartTime")]
        public object? TradableStartTime { get; set; }

        [JsonPropertyName("symbolTradeLimit")]
        public object? SymbolTradeLimit { get; set; }

        [JsonPropertyName("crossMargin")]
        public object? CrossMargin { get; set; }
    }


    public class PoloniexContractAddress
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("coin")]
        public required string Coin { get; set; }

        [JsonPropertyName("delisted")]
        public bool Delisted { get; set; }

        [JsonPropertyName("tradeEnable")]
        public bool TradeEnable { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("networkList")]
        public required PoloniexContractNetworkList[] NetworkList { get; set; }

        [JsonPropertyName("supportCollateral")]
        public bool? SupportCollateral { get; set; }

        [JsonPropertyName("supportBorrow")]
        public bool? SupportBorrow { get; set; }
    }
    public class PoloniexContractNetworkList
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("coin")]
        public string? Coin { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("currencyType")]
        public string? CurrencyType { get; set; }

        [JsonPropertyName("blockchain")]
        public required string Blockchain { get; set; }

        [JsonPropertyName("withdrawalEnable")]
        public bool WithdrawalEnable { get; set; }

        [JsonPropertyName("depositEnable")]
        public bool DepositEnable { get; set; }

        [JsonPropertyName("depositAddress")]
        public string? DepositAddress { get; set; }

        [JsonPropertyName("withdrawMin")]
        public string? WithdrawMin { get; set; }

        [JsonPropertyName("decimals")]
        public int Decimals { get; set; }

        [JsonPropertyName("withdrawFee")]
        public string? WithdrawFee { get; set; }

        [JsonPropertyName("minConfirm")]
        public int MinConfirm { get; set; }

        [JsonPropertyName("contractAddress")]
        public string? ContractAddress { get; set; }
    }


    public class PoloniexPrice
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }

        [JsonPropertyName("open")]
        public string? Open { get; set; }

        [JsonPropertyName("low")]
        public string? Low { get; set; }

        [JsonPropertyName("high")]
        public string? High { get; set; }

        [JsonPropertyName("close")]
        public string? Close { get; set; }

        [JsonPropertyName("quantity")]
        public string? Quantity { get; set; }

        [JsonPropertyName("amount")]
        public string? Amount { get; set; }

        [JsonPropertyName("tradeCount")]
        public int TradeCount { get; set; }

        [JsonPropertyName("startTime")]
        public object? StartTime { get; set; }

        [JsonPropertyName("closeTime")]
        public object? CloseTime { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("dailyChange")]
        public string? DailyChange { get; set; }

        [JsonPropertyName("bid")]
        public required string Bid { get; set; }

        [JsonPropertyName("bidQuantity")]
        public string? BidQuantity { get; set; }

        [JsonPropertyName("ask")]
        public required string Ask { get; set; }

        [JsonPropertyName("askQuantity")]
        public string? AskQuantity { get; set; }

        [JsonPropertyName("ts")]
        public object? Ts { get; set; }

        [JsonPropertyName("markPrice")]
        public string? MarkPrice { get; set; }
    }


    public class PoloniexOrderBook
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("scale")]
        public string? Scale { get; set; }

        [JsonPropertyName("asks")]
        public string[]? Asks { get; set; }

        [JsonPropertyName("bids")]
        public string[]? Bids { get; set; }

        [JsonPropertyName("ts")]
        public long Ts { get; set; }
    }
}
