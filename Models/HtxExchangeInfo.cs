using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class HtxExchangeInfo
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("data")]
        public HtxExchangeInfoData[]? Data { get; set; }
    }
    public class HtxExchangeInfoData
    {
        [JsonPropertyName("base-currency")]
        public required string Basecurrency { get; set; }
        [JsonPropertyName("quote-currency")]
        public required string Quotecurrency { get; set; }
        [JsonPropertyName("price-precision")]
        public int Priceprecision { get; set; }
        [JsonPropertyName("amount-precision")]
        public int Amountprecision { get; set; }
        [JsonPropertyName("symbol-partition")]
        public string? Symbolpartition { get; set; }
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("state")]
        public required string State { get; set; }
        [JsonPropertyName("value-precision")]
        public int Valueprecision { get; set; }
        [JsonPropertyName("min-order-amt")]
        public float Minorderamt { get; set; }
        [JsonPropertyName("max-order-amt")]
        public float Maxorderamt { get; set; }
        [JsonPropertyName("min-order-value")]
        public float Minordervalue { get; set; }
        [JsonPropertyName("limit-order-min-order-amt")]
        public float Limitorderminorderamt { get; set; }
        [JsonPropertyName("limit-order-max-order-amt")]
        public float Limitordermaxorderamt { get; set; }
        [JsonPropertyName("limit-order-max-buy-amt")]
        public float Limitordermaxbuyamt { get; set; }
        [JsonPropertyName("limit-order-max-sell-amt")]
        public float Limitordermaxsellamt { get; set; }
        [JsonPropertyName("buy-limit-must-less-than")]
        public float Buylimitmustlessthan { get; set; }
        [JsonPropertyName("sell-limit-must-greater-than")]
        public float Selllimitmustgreaterthan { get; set; }
        [JsonPropertyName("sell-market-min-order-amt")]
        public float Sellmarketminorderamt { get; set; }
        [JsonPropertyName("sell-market-max-order-amt")]
        public float Sellmarketmaxorderamt { get; set; }
        [JsonPropertyName("buy-market-max-order-value")]
        public float Buymarketmaxordervalue { get; set; }
        [JsonPropertyName("market-sell-order-rate-must-less-than")]
        public float Marketsellorderratemustlessthan { get; set; }
        [JsonPropertyName("market-buy-order-rate-must-less-than")]
        public float Marketbuyorderratemustlessthan { get; set; }
        [JsonPropertyName("api-trading")]
        public string? Apitrading { get; set; }
        [JsonPropertyName("tags")]
        public string? Tags { get; set; }
        [JsonPropertyName("max-order-value")]
        public int Maxordervalue { get; set; }
        [JsonPropertyName("leverage-ratio")]
        public int Leverageratio { get; set; }
        [JsonPropertyName("super-margin-leverage-ratio")]
        public int Supermarginleverageratio { get; set; }
        [JsonPropertyName("funding-leverage-ratio")]
        public int Fundingleverageratio { get; set; }
    }


    public class HtxContractAddresses
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public HtxContractAddressesData[]? Data { get; set; }
    }
    public class HtxContractAddressesData
    {
        [JsonPropertyName("currency")]
        public required string Currency { get; set; }

        [JsonPropertyName("assetType")]
        public int AssetType { get; set; }

        [JsonPropertyName("chains")]
        public required HtxContractAddressesChain[] Chains { get; set; }

        [JsonPropertyName("instStatus")]
        public string? InstStatus { get; set; }
    }
    public class HtxContractAddressesChain
    {
        [JsonPropertyName("chain")]
        public string? Chain { get; set; }

        [JsonPropertyName("displayName")]
        public required string DisplayName { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("baseChain")]
        public string? BaseChain { get; set; }

        [JsonPropertyName("baseChainProtocol")]
        public string? BaseChainProtocol { get; set; }

        [JsonPropertyName("isDynamic")]
        public bool IsDynamic { get; set; }

        [JsonPropertyName("numOfConfirmations")]
        public int NumOfConfirmations { get; set; }

        [JsonPropertyName("numOfFastConfirmations")]
        public int NumOfFastConfirmations { get; set; }

        [JsonPropertyName("depositStatus")]
        public required string DepositStatus { get; set; }

        [JsonPropertyName("minDepositAmt")]
        public string? MinDepositAmt { get; set; }

        [JsonPropertyName("withdrawStatus")]
        public required string WithdrawStatus { get; set; }

        [JsonPropertyName("minWithdrawAmt")]
        public string? MinWithdrawAmt { get; set; }

        [JsonPropertyName("withdrawPrecision")]
        public int WithdrawPrecision { get; set; }

        [JsonPropertyName("maxWithdrawAmt")]
        public string? MaxWithdrawAmt { get; set; }

        [JsonPropertyName("withdrawQuotaPerDay")]
        public string? WithdrawQuotaPerDay { get; set; }

        [JsonPropertyName("withdrawQuotaPerYear")]
        public object? WithdrawQuotaPerYear { get; set; }

        [JsonPropertyName("withdrawQuotaTotal")]
        public object? WithdrawQuotaTotal { get; set; }

        [JsonPropertyName("withdrawFeeType")]
        public string? WithdrawFeeType { get; set; }

        [JsonPropertyName("withdrawFeeExpandType")]
        public string? WithdrawFeeExpandType { get; set; }

        [JsonPropertyName("withdrawFeeExpandData")]
        public HtxContractAddressesWithdrawFeeExpandData? WithdrawFeeExpandData { get; set; }

        [JsonPropertyName("transactFeeWithdraw")]
        public string? TransactFeeWithdraw { get; set; }

        [JsonPropertyName("addrWithTag")]
        public bool AddrWithTag { get; set; }

        [JsonPropertyName("addrDepositTag")]
        public bool AddrDepositTag { get; set; }

        [JsonPropertyName("minTransactFeeWithdraw")]
        public string? MinTransactFeeWithdraw { get; set; }

        [JsonPropertyName("transactFeeRateWithdraw")]
        public string? TransactFeeRateWithdraw { get; set; }

        [JsonPropertyName("contractAddress")]
        public string? ContractAddress { get; set; }
    }
    public class HtxContractAddressesStepFixedStepList
    {
        [JsonPropertyName("max")]
        public long Max { get; set; }

        [JsonPropertyName("fee")]
        public double Fee { get; set; }
    }
    public class HtxContractAddressesWithdrawFeeExpandData
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("step-fixed-step-list")]
        public HtxContractAddressesStepFixedStepList[]? StepFixedStepList { get; set; }
    }


    public class HtxPrices
    {
        [JsonPropertyName("data")]
        public HtxPricesDatum[]? Data { get; set; }
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("ts")]
        public long Ts { get; set; }
    }
    public class HtxPricesDatum
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("open")]
        public float Open { get; set; }
        [JsonPropertyName("high")]
        public float High { get; set; }
        [JsonPropertyName("low")]
        public float Low { get; set; }
        [JsonPropertyName("close")]
        public float Close { get; set; }
        [JsonPropertyName("amount")]
        public float Amount { get; set; }
        [JsonPropertyName("vol")]
        public float Vol { get; set; }
        [JsonPropertyName("count")]
        public int Count { get; set; }
        [JsonPropertyName("bid")]
        public float Bid { get; set; }
        [JsonPropertyName("bidSize")]
        public float BidSize { get; set; }
        [JsonPropertyName("ask")]
        public float Ask { get; set; }
        [JsonPropertyName("askSize")]
        public float AskSize { get; set; }
    }


    public class HtxOrderBook
    {
        [JsonPropertyName("ch")]
        public string? Ch { get; set; }
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("ts")]
        public long Ts { get; set; }
        [JsonPropertyName("tick")]
        public HtxOrderBookTick? Tick { get; set; }
    }
    public class HtxOrderBookTick
    {
        [JsonPropertyName("ts")]
        public long Ts { get; set; }
        [JsonPropertyName("version")]
        public long Version { get; set; }
        [JsonPropertyName("bids")]
        public required double[][] Bids { get; set; }
        [JsonPropertyName("asks")]
        public required double[][] Asks { get; set; }
    }
}