using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class LbankExchangeInfo
    {
        public string? Msg { get; set; }
        public string? Result { get; set; }
        public string[]? Data { get; set; }
        public int ErrorCode { get; set; }
        public long Ts { get; set; }
    }

    public class LbankContractAddresses
    {
        public required long Timestamp { get; set; }
        public required LbankCurrency[] Currencies { get; set; }
    }
    public class LbankCurrency
    {
        public required string Currency { get; set; }
        public required AssetNetwork[] Networks { get; set; }
    }


    public class LbankComissions
    {
        public required long Timestamp { get; set; }
        public required LbankComission[] Comissions { get; set; }
    }
    public class LbankComission
    {
        public required string Symbol { get; set; }
        public required double TakerComission {  get; set; }
    }


    public class LbankConfigs
    {
        [JsonPropertyName("result")]
        public required string Result { get; set; }

        [JsonPropertyName("data")]
        public LbankConfigsData[]? Data { get; set; }
    }
    public class LbankConfigsData
    {
        [JsonPropertyName("assetCode")]
        public required string AssetCode { get; set; }

        [JsonPropertyName("chainName")]
        public required string ChainName { get; set; }

        [JsonPropertyName("canDraw")]
        public bool CanDraw { get; set; }

        [JsonPropertyName("canStationDraw")]
        public bool CanStationDraw { get; set; }

        [JsonPropertyName("canDeposit")]
        public bool CanDeposit { get; set; }

        [JsonPropertyName("hasMemo")]
        public bool HasMemo { get; set; }

        [JsonPropertyName("assetFee")]
        public required LbankConfigsAssetFee AssetFee { get; set; }
    }
    public class LbankConfigsAssetFee
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("feeCode")]
        public required string FeeCode { get; set; }

        [JsonPropertyName("scale")]
        public int Scale { get; set; }

        [JsonPropertyName("minAmt")]
        public required string MinAmt { get; set; }

        [JsonPropertyName("feeAmt")]
        public required string FeeAmt { get; set; }

        [JsonPropertyName("feeRate")]
        public required string FeeRate { get; set; }

        [JsonPropertyName("stationFeeAmt")]
        public required string StationFeeAmt { get; set; }

        [JsonPropertyName("stationScale")]
        public int StationScale { get; set; }

        [JsonPropertyName("stationMinAmt")]
        public required string StationMinAmt { get; set; }

        [JsonPropertyName("minDepositAmt")]
        public required string MinDepositAmt { get; set; }

        [JsonPropertyName("depositFee")]
        public required string DepositFee { get; set; }
    }


    public class LbankRate
    {
        [JsonPropertyName("data")]
        public LbankRateData? Data { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
    public class LbankRateData
    {
        [JsonPropertyName("makerRate")]
        public string? MakerRate { get; set; }

        [JsonPropertyName("takerRate")]
        public string? TakerRate { get; set; }
    }


    public class LbankDepositDetail
    {
        [JsonPropertyName("data")]
        public LbankDepositDetailData? Data { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
    public class LbankDepositDetailData
    {
        [JsonPropertyName("addressData")]
        public LbankDepositDetailAddressData? AddressData { get; set; }

        [JsonPropertyName("depositEnabled")]
        public bool DepositEnabled { get; set; }
    }
    public class LbankDepositDetailAddressData
    {
        [JsonPropertyName("walletAddress")]
        public LbankDepositDetailWalletAddress? WalletAddress { get; set; }

        [JsonPropertyName("contractSwitch")]
        public bool ContractSwitch { get; set; }

        [JsonPropertyName("contractInfo")]
        public string? ContractInfo { get; set; }

        [JsonPropertyName("minQty")]
        public string? MinQty { get; set; }

        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("confirmation")]
        public int Confirmation { get; set; }

        [JsonPropertyName("depositRemark")]
        public string? DepositRemark { get; set; }
    }
    public class LbankDepositDetailWalletAddress
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("assetCode")]
        public string? AssetCode { get; set; }

        [JsonPropertyName("digitAssetAddress")]
        public string? DigitAssetAddress { get; set; }

        [JsonPropertyName("drawWhiteListStatus")]
        public int DrawWhiteListStatus { get; set; }

        [JsonPropertyName("assetFlag")]
        public string? AssetFlag { get; set; }
    }


    public class LbankPrices
    {
        public string? Msg { get; set; }
        public string? Result { get; set; }
        public LbankPricesData[]? Data { get; set; }
        public int ErrorCode { get; set; }
        public long Ts { get; set; }
    }
    public class LbankPricesData
    {
        public required string Symbol { get; set; }
        public required LbankPricesDataTicker Ticker { get; set; }
        public object? Timestamp { get; set; }
    }
    public class LbankPricesDataTicker
    {
        public string? High { get; set; }
        public string? Vol { get; set; }
        public string? Low { get; set; }
        public string? Change { get; set; }
        public string? Turnover { get; set; }
        public required string Latest { get; set; }
    }


    public class LbankOrderBook
    {
        public string? Result { get; set; }
        public string? Msg { get; set; }
        public LbankOrderBookData? Data { get; set; }
        public int ErrorCode { get; set; }
        public long Ts { get; set; }
    }
    public class LbankOrderBookData
    {
        public required string[][] Asks { get; set; }
        public required string[][] Bids { get; set; }
        public long Timestamp { get; set; }
    }
}
