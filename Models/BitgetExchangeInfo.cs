using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class BitgetExchangeInfo
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
        [JsonPropertyName("requestTime")]
        public long RequestTime { get; set; }
        [JsonPropertyName("data")]
        public BitgetData[]? Data { get; set; }
    }
    public class BitgetData
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("baseCoin")]
        public required string BaseCoin { get; set; }
        [JsonPropertyName("quoteCoin")]
        public required string QuoteCoin { get; set; }
        [JsonPropertyName("minTradeAmount")]
        public string? MinTradeAmount { get; set; }
        [JsonPropertyName("maxTradeAmount")]
        public string? MaxTradeAmount { get; set; }
        [JsonPropertyName("takerFeeRate")]
        public required string TakerFeeRate { get; set; }
        [JsonPropertyName("makerFeeRate")]
        public string? MakerFeeRate { get; set; }
        [JsonPropertyName("pricePrecision")]
        public string? PricePrecision { get; set; }
        [JsonPropertyName("quantityPrecision")]
        public string? QuantityPrecision { get; set; }
        [JsonPropertyName("quotePrecision")]
        public string? QuotePrecision { get; set; }
        [JsonPropertyName("status")]
        public required string Status { get; set; }
        [JsonPropertyName("minTradeUSDT")]
        public string? MinTradeUSDT { get; set; }
        [JsonPropertyName("buyLimitPriceRatio")]
        public string? BuyLimitPriceRatio { get; set; }
        [JsonPropertyName("sellLimitPriceRatio")]
        public string? SellLimitPriceRatio { get; set; }
        [JsonPropertyName("areaSymbol")]
        public string? AreaSymbol { get; set; }
    }


    public class BitgetContractAddresses
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
        [JsonPropertyName("requestTime")]
        public long RequestTime { get; set; }
        [JsonPropertyName("data")]
        public BitgetDatum[]? Data { get; set; }
    }
    public class BitgetDatum
    {
        [JsonPropertyName("coinId")]
        public string? CoinId { get; set; }
        [JsonPropertyName("coin")]
        public required string Coin { get; set; }
        [JsonPropertyName("transfer")]
        public string? Transfer { get; set; }
        [JsonPropertyName("chains")]
        public required BitgetChain[] Chains { get; set; }
        [JsonPropertyName("areaCoin")]
        public string? AreaCoin { get; set; }
    }
    public class BitgetChain
    {
        [JsonPropertyName("chain")]
        public required string Chain { get; set; }
        [JsonPropertyName("needTag")]
        public string? NeedTag { get; set; }
        [JsonPropertyName("withdrawable")]
        public required string Withdrawable { get; set; }
        [JsonPropertyName("rechargeable")]
        public required string Rechargeable { get; set; }
        [JsonPropertyName("withdrawFee")]
        public string? WithdrawFee { get; set; }
        [JsonPropertyName("extraWithdrawFee")]
        public string? ExtraWithdrawFee { get; set; }
        [JsonPropertyName("depositConfirm")]
        public string? DepositConfirm { get; set; }
        [JsonPropertyName("withdrawConfirm")]
        public string? WithdrawConfirm { get; set; }
        [JsonPropertyName("minDepositAmount")]
        public string? MinDepositAmount { get; set; }
        [JsonPropertyName("minWithdrawAmount")]
        public string? MinWithdrawAmount { get; set; }
        [JsonPropertyName("browserUrl")]
        public string? BrowserUrl { get; set; }
        [JsonPropertyName("contractAddress")]
        public string? ContractAddress { get; set; }
        [JsonPropertyName("withdrawStep")]
        public string? WithdrawStep { get; set; }
        [JsonPropertyName("withdrawMinScale")]
        public string? WithdrawMinScale { get; set; }
        [JsonPropertyName("congestion")]
        public string? Congestion { get; set; }
    }


    public class BitgetPrice
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
        [JsonPropertyName("requestTime")]
        public long RequestTime { get; set; }
        [JsonPropertyName("data")]
        public BitgetPriceDatum[]? Data { get; set; }
    }
    public class BitgetPriceDatum
    {
        [JsonPropertyName("open")]
        public string? Open { get; set; }
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("high24h")]
        public string? High24h { get; set; }
        [JsonPropertyName("low24h")]
        public string? Low24h { get; set; }
        [JsonPropertyName("lastPr")]
        public string? LastPr { get; set; }
        [JsonPropertyName("quoteVolume")]
        public string? QuoteVolume { get; set; }
        [JsonPropertyName("baseVolume")]
        public string? BaseVolume { get; set; }
        [JsonPropertyName("usdtVolume")]
        public string? UsdtVolume { get; set; }
        [JsonPropertyName("ts")]
        public string? Ts { get; set; }
        [JsonPropertyName("bidPr")]
        public required string BidPr { get; set; }
        [JsonPropertyName("askPr")]
        public required string AskPr { get; set; }
        [JsonPropertyName("bidSz")]
        public string? BidSz { get; set; }
        [JsonPropertyName("askSz")]
        public string? AskSz { get; set; }
        [JsonPropertyName("openUtc")]
        public string? OpenUtc { get; set; }
        [JsonPropertyName("changeUtc24h")]
        public string? ChangeUtc24h { get; set; }
        [JsonPropertyName("change24h")]
        public string? Change24h { get; set; }
    }


    public class BitgetOrderBook
    {
        [JsonPropertyName("code")]
        public required string Code { get; set; }
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
        [JsonPropertyName("requestTime")]
        public long RequestTime { get; set; }
        [JsonPropertyName("data")]
        public BitgetOrderBookData? Data { get; set; }
    }
    public class BitgetOrderBookData
    {
        [JsonPropertyName("asks")]
        public required string[][] Asks { get; set; }
        [JsonPropertyName("bids")]
        public required string[][] Bids { get; set; }
        [JsonPropertyName("ts")]
        public string? Ts { get; set; }
    }
}
