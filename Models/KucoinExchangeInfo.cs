using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class KucoinExchangeInfo
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("data")]
        public KucoinDatum[]? Data { get; set; }
    }
    public class KucoinDatum
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("baseCurrency")]
        public required string BaseCurrency { get; set; }
        [JsonPropertyName("quoteCurrency")]
        public required string QuoteCurrency { get; set; }
        [JsonPropertyName("feeCurrency")]
        public string? FeeCurrency { get; set; }
        [JsonPropertyName("market")]
        public string? Market { get; set; }
        [JsonPropertyName("baseMinSize")]
        public string? BaseMinSize { get; set; }
        [JsonPropertyName("quoteMinSize")]
        public string? QuoteMinSize { get; set; }
        [JsonPropertyName("baseMaxSize")]
        public string? BaseMaxSize { get; set; }
        [JsonPropertyName("quoteMaxSize")]
        public string? QuoteMaxSize { get; set; }
        [JsonPropertyName("baseIncrement")]
        public string? BaseIncrement { get; set; }
        [JsonPropertyName("quoteIncrement")]
        public string? QuoteIncrement { get; set; }
        [JsonPropertyName("priceIncrement")]
        public string? PriceIncrement { get; set; }
        [JsonPropertyName("priceLimitRate")]
        public string? PriceLimitRate { get; set; }
        [JsonPropertyName("minFunds")]
        public string? MinFunds { get; set; }
        [JsonPropertyName("isMarginEnabled")]
        public bool IsMarginEnabled { get; set; }
        [JsonPropertyName("enableTrading")]
        public bool EnableTrading { get; set; }
    }


    public class KucoinContractAddresses
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("data")]
        public KucoinContractDatum[]? Data { get; set; }
    }
    public class KucoinContractDatum
    {
        [JsonPropertyName("currency")]
        public required string Currency { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }
        [JsonPropertyName("precision")]
        public int Precision { get; set; }
        [JsonPropertyName("confirms")]
        public object? Confirms { get; set; }
        [JsonPropertyName("contractAddress")]
        public object? ContractAddress { get; set; }
        [JsonPropertyName("isMarginEnabled")]
        public bool IsMarginEnabled { get; set; }
        [JsonPropertyName("isDebitEnabled")]
        public bool IsDebitEnabled { get; set; }
        [JsonPropertyName("chains")]
        public required KucoinChain[] Chains { get; set; }
    }
    public class KucoinChain
    {
        [JsonPropertyName("chainName")]
        public required string ChainName { get; set; }
        [JsonPropertyName("withdrawalMinSize")]
        public string? WithdrawalMinSize { get; set; }
        [JsonPropertyName("depositMinSize")]
        public string? DepositMinSize { get; set; }
        [JsonPropertyName("withdrawFeeRate")]
        public string? WithdrawFeeRate { get; set; }
        [JsonPropertyName("withdrawalMinFee")]
        public string? WithdrawalMinFee { get; set; }
        [JsonPropertyName("isWithdrawEnabled")]
        public bool IsWithdrawEnabled { get; set; }
        [JsonPropertyName("isDepositEnabled")]
        public bool IsDepositEnabled { get; set; }
        [JsonPropertyName("confirms")]
        public int Confirms { get; set; }
        [JsonPropertyName("preConfirms")]
        public int PreConfirms { get; set; }
        [JsonPropertyName("contractAddress")]
        public string? ContractAddress { get; set; }
        [JsonPropertyName("chainId")]
        public string? ChainId { get; set; }
        [JsonPropertyName("depositFeeRate")]
        public string? DepositFeeRate { get; set; }
        [JsonPropertyName("withdrawMaxFee")]
        public string? WithdrawMaxFee { get; set; }
        [JsonPropertyName("depositTierFee")]
        public string? DepositTierFee { get; set; }
    }


    public class KucoinPrices
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("data")]
        public KucoinData? Data { get; set; }
    }
    public class KucoinData
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }
        [JsonPropertyName("ticker")]
        public required KucoinTicker[] Ticker { get; set; }
    }
    public class KucoinTicker
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("symbolName")]
        public string? SymbolName { get; set; }
        [JsonPropertyName("buy")]
        public required string Buy { get; set; }
        [JsonPropertyName("bestBidSize")]
        public string? BestBidSize { get; set; }
        [JsonPropertyName("sell")]
        public required string Sell { get; set; }
        [JsonPropertyName("bestAskSize")]
        public string? BestAskSize { get; set; }
        [JsonPropertyName("changeRate")]
        public string? ChangeRate { get; set; }
        [JsonPropertyName("changePrice")]
        public string? ChangePrice { get; set; }
        [JsonPropertyName("high")]
        public string? High { get; set; }
        [JsonPropertyName("low")]
        public string? Low { get; set; }
        [JsonPropertyName("vol")]
        public string? Vol { get; set; }
        [JsonPropertyName("volValue")]
        public string? VolValue { get; set; }
        [JsonPropertyName("last")]
        public string? Last { get; set; }
        [JsonPropertyName("averagePrice")]
        public string? AveragePrice { get; set; }
        [JsonPropertyName("takerFeeRate")]
        public string? TakerFeeRate { get; set; }
        [JsonPropertyName("makerFeeRate")]
        public string? MakerFeeRate { get; set; }
        [JsonPropertyName("takerCoefficient")]
        public string? TakerCoefficient { get; set; }
        [JsonPropertyName("makerCoefficient")]
        public string? MakerCoefficient { get; set; }
    }


    public class KucoinOrderBook
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("data")]
        public KucoinOrderData? Data { get; set; }
    }
    public class KucoinOrderData
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }
        [JsonPropertyName("sequence")]
        public string? Sequence { get; set; }
        [JsonPropertyName("bids")]
        public required string[][] Bids { get; set; }
        [JsonPropertyName("asks")]
        public required string[][] Asks { get; set; }
    }
}
