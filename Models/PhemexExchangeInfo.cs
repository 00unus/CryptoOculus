using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class PhemexExchangeInfo
    {
        public int Code { get; set; }
        public string? Msg { get; set; }
        public PhemexExchangeInfoData? Data { get; set; }
    }
    public class PhemexExchangeInfoData
    {
        public object[]? Currencies { get; set; }
        public required PhemexExchangeInfoProduct[] Products { get; set; }
        public object[]? PerpProductsV2 { get; set; }
        public object[]? RiskLimits { get; set; }
        public object[]? Leverages { get; set; }
        public object[]? RiskLimitsV2 { get; set; }
        public object[]? LeveragesV2 { get; set; }
        public object[]? LeverageMargins { get; set; }
        public int RatioScale { get; set; }
        public string? Md5Checksum { get; set; }
    }
    public class PhemexExchangeInfoProduct
    {
        public required string Symbol { get; set; }
        public int Code { get; set; }
        public required string Type { get; set; }
        public string? DisplaySymbol { get; set; }
        public string? IndexSymbol { get; set; }
        public string? MarkSymbol { get; set; }
        public string? FundingRateSymbol { get; set; }
        public string? FundingRate8hSymbol { get; set; }
        public string? ContractUnderlyingAssets { get; set; }
        public string? SettleCurrency { get; set; }
        public string? BaseCurrency { get; set; }
        public required string QuoteCurrency { get; set; }
        public double ContractSize { get; set; }
        public int LotSize { get; set; }
        public double TickSize { get; set; }
        public int PriceScale { get; set; }
        public int RatioScale { get; set; }
        public int PricePrecision { get; set; }
        public int MinPriceEp { get; set; }
        public object? MaxPriceEp { get; set; }
        public int MaxOrderQty { get; set; }
        public string? Description { get; set; }
        public required string Status { get; set; }
        public double TipOrderQty { get; set; }
        public object? ListTime { get; set; }
        public bool MajorSymbol { get; set; }
        public string? DefaultLeverage { get; set; }
        public int FundingInterval { get; set; }
        public int MaxLeverage { get; set; }
        public int LeverageMargin { get; set; }
    }


    public class PhemexDeposit
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("data")]
        public PhemexDepositDatum[]? Data { get; set; }
    }
    public class PhemexDepositDatum
    {
        [JsonPropertyName("currency")]
        public required string Currency { get; set; }

        [JsonPropertyName("currencyCode")]
        public int CurrencyCode { get; set; }

        [JsonPropertyName("minAmountRv")]
        public string? MinAmountRv { get; set; }

        [JsonPropertyName("confirmations")]
        public int Confirmations { get; set; }

        [JsonPropertyName("chainCode")]
        public int ChainCode { get; set; }

        [JsonPropertyName("chainName")]
        public required string ChainName { get; set; }

        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("contractAddress")]
        public string? ContractAddress { get; set; }
    }


    public class PhemexWithdraw
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("data")]
        public PhemexWithdrawData? Data { get; set; }
    }
    public class PhemexWithdrawData
    {
        [JsonPropertyName("currency")]
        public required string Currency { get; set; }

        [JsonPropertyName("currencyCode")]
        public int CurrencyCode { get; set; }

        [JsonPropertyName("balanceRv")]
        public string? BalanceRv { get; set; }

        [JsonPropertyName("confirmAmountRv")]
        public string? ConfirmAmountRv { get; set; }

        [JsonPropertyName("allAvailableBalanceRv")]
        public string? AllAvailableBalanceRv { get; set; }

        [JsonPropertyName("chainInfos")]
        public PhemexWithdrawChainInfo[]? ChainInfos { get; set; }
    }
    public class PhemexWithdrawChainInfo
    {
        [JsonPropertyName("chainCode")]
        public int ChainCode { get; set; }

        [JsonPropertyName("chainName")]
        public required string ChainName { get; set; }

        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("contractAddress")]
        public string? ContractAddress { get; set; }

        [JsonPropertyName("minWithdrawAmountRv")]
        public string? MinWithdrawAmountRv { get; set; }

        [JsonPropertyName("minWithdrawAmountWithFeeRv")]
        public string? MinWithdrawAmountWithFeeRv { get; set; }

        [JsonPropertyName("withdrawFeeRv")]
        public string? WithdrawFeeRv { get; set; }

        [JsonPropertyName("receiveAmountRv")]
        public string? ReceiveAmountRv { get; set; }
    }


    public class PhemexContractAddresses
    {
        public required long Timestamp { get; set; }
        public required PhemexCurrency[] Currencies { get; set; }
    }
    public class PhemexCurrency
    {
        public required string Currency { get; set; }
        public required AssetNetwork[] Networks { get; set; }
    }


    public class PhemexPrices
    {
        public object? Error { get; set; }
        public int Id { get; set; }
        public PhemexPricesResult[]? Result { get; set; }
    }
    public class PhemexPricesResult
    {
        public double AskEp { get; set; }
        public double BidEp { get; set; }
        public long HighEp { get; set; }
        public long IndexEp { get; set; }
        public long LastEp { get; set; }
        public long LowEp { get; set; }
        public long OpenEp { get; set; }
        public required string Symbol { get; set; }
        public object? Timestamp { get; set; }
        public object? TurnoverEv { get; set; }
        public object? VolumeEv { get; set; }
    }


    public class PhemexOrderBook
    {
        public object? Error { get; set; }
        public int Id { get; set; }
        public PhemexOrderBookResult? Result { get; set; }
    }
    public class PhemexOrderBookResult
    {
        public required PhemexOrderBookBook Book { get; set; }
        public int Depth { get; set; }
        public long Sequence { get; set; }
        public string? Symbol { get; set; }
        public long Timestamp { get; set; }
        public string? Type { get; set; }
    }
    public class PhemexOrderBookBook
    {
        public double[][]? Asks { get; set; }
        public double[][]? Bids { get; set; }
    }
}
