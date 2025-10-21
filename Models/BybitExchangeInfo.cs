using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class BybitExchangeInfo
    {
        [JsonPropertyName("retCode")]
        public int RetCode { get; set; }

        [JsonPropertyName("retMsg")]
        public string? RetMsg { get; set; }

        [JsonPropertyName("result")]
        public BybitInfoResult? Result { get; set; }

        [JsonPropertyName("retExtInfo")]
        public object? RetExtInfo { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }
    }
    public class BybitInfoResult
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("list")]
        public required BybitInfoList[] List { get; set; }
    }
    public class BybitInfoList
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }

        [JsonPropertyName("baseCoin")]
        public required string BaseCoin { get; set; }

        [JsonPropertyName("quoteCoin")]
        public required string QuoteCoin { get; set; }

        [JsonPropertyName("innovation")]
        public string? Innovation { get; set; }

        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("marginTrading")]
        public string? MarginTrading { get; set; }

        [JsonPropertyName("lotSizeFilter")]
        public object? LotSizeFilter { get; set; }

        [JsonPropertyName("priceFilter")]
        public object? PriceFilter { get; set; }

        [JsonPropertyName("riskParameters")]
        public object? RiskParameters { get; set; }
    }


    public class BybitContractAddress
    {
        [JsonPropertyName("retCode")]
        public int RetCode { get; set; }
        [JsonPropertyName("retMsg")]
        public string? RetMsg { get; set; }
        [JsonPropertyName("result")]
        public BybitContractResult? Result { get; set; }
        [JsonPropertyName("retExtInfo")]
        public object? RetExtInfo { get; set; }
        [JsonPropertyName("time")]
        public long Time { get; set; }
    }
    public class BybitContractResult
    {
        [JsonPropertyName("rows")]
        public BybitContractRow[]? Rows { get; set; }
    }
    public class BybitContractRow
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("coin")]
        public required string Coin { get; set; }
        [JsonPropertyName("remainAmount")]
        public string? RemainAmount { get; set; }
        [JsonPropertyName("chains")]
        public required BybitContractChain[] Chains { get; set; }
    }
    public class BybitContractChain
    {
        [JsonPropertyName("chainType")]
        public string? ChainType { get; set; }
        [JsonPropertyName("confirmation")]
        public string? Confirmation { get; set; }
        [JsonPropertyName("withdrawFee")]
        public string? WithdrawFee { get; set; }
        [JsonPropertyName("depositMin")]
        public string? DepositMin { get; set; }
        [JsonPropertyName("withdrawMin")]
        public string? WithdrawMin { get; set; }
        [JsonPropertyName("chain")]
        public required string Chain { get; set; }
        [JsonPropertyName("chainDeposit")]
        public required string ChainDeposit { get; set; }
        [JsonPropertyName("chainWithdraw")]
        public required string ChainWithdraw { get; set; }
        [JsonPropertyName("minAccuracy")]
        public string? MinAccuracy { get; set; }
        [JsonPropertyName("withdrawPercentageFee")]
        public string? WithdrawPercentageFee { get; set; }
        [JsonPropertyName("contractAddress")]
        public string? ContractAddress { get; set; }
        [JsonPropertyName("safeConfirmNumber")]
        public string? SafeConfirmNumber { get; set; }
    }


    public class BybitPrice
    {
        [JsonPropertyName("retCode")]
        public int RetCode { get; set; }

        [JsonPropertyName("retMsg")]
        public string? RetMsg { get; set; }

        [JsonPropertyName("result")]
        public BybitPriceResult? Result { get; set; }

        [JsonPropertyName("retExtInfo")]
        public object? RetExtInfo { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }
    }
    public class BybitPriceResult
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("list")]
        public required BybitPriceList[] List { get; set; }
    }
    public class BybitPriceList
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }

        [JsonPropertyName("bid1Price")]
        public required string Bid1Price { get; set; }

        [JsonPropertyName("bid1Size")]
        public string? Bid1Size { get; set; }

        [JsonPropertyName("ask1Price")]
        public required string Ask1Price { get; set; }

        [JsonPropertyName("ask1Size")]
        public string? Ask1Size { get; set; }

        [JsonPropertyName("lastPrice")]
        public string? LastPrice { get; set; }

        [JsonPropertyName("prevPrice24h")]
        public string? PrevPrice24h { get; set; }

        [JsonPropertyName("price24hPcnt")]
        public string? Price24hPcnt { get; set; }

        [JsonPropertyName("highPrice24h")]
        public string? HighPrice24h { get; set; }

        [JsonPropertyName("lowPrice24h")]
        public string? LowPrice24h { get; set; }

        [JsonPropertyName("turnover24h")]
        public string? Turnover24h { get; set; }

        [JsonPropertyName("volume24h")]
        public string? Volume24h { get; set; }

        [JsonPropertyName("usdIndexPrice")]
        public string? UsdIndexPrice { get; set; }
    }


    public class BybitOrderBook
    {
        [JsonPropertyName("retCode")]
        public int RetCode { get; set; }
        [JsonPropertyName("retMsg")]
        public string? RetMsg { get; set; }
        [JsonPropertyName("result")]
        public BybitOrderBookResult? Result { get; set; }
        [JsonPropertyName("retExtInfo")]
        public object? RetExtInfo { get; set; }
        [JsonPropertyName("time")]
        public long Time { get; set; }
    }
    public class BybitOrderBookResult
    {
        [JsonPropertyName("s")]
        public string? S { get; set; }
        [JsonPropertyName("a")]
        public required string[][] A { get; set; }
        [JsonPropertyName("b")]
        public required string[][] B { get; set; }
        [JsonPropertyName("ts")]
        public long Ts { get; set; }
        [JsonPropertyName("u")]
        public int U { get; set; }
        [JsonPropertyName("seq")]
        public long Seq { get; set; }
        [JsonPropertyName("cts")]
        public long Cts { get; set; }
    }
}
