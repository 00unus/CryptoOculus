using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class BingxExchangeInfo
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
        [JsonPropertyName("debugMsg")]
        public string? DebugMsg { get; set; }
        [JsonPropertyName("data")]
        public BingxData? Data { get; set; }
    }
    public class BingxData
    {
        [JsonPropertyName("symbols")]
        public required BingxSymbol[] Symbols { get; set; }
    }
    public class BingxSymbol
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("minQty")]
        public float MinQty { get; set; }
        [JsonPropertyName("maxQty")]
        public float MaxQty { get; set; }
        [JsonPropertyName("minNotional")]
        public float MinNotional { get; set; }
        [JsonPropertyName("maxNotional")]
        public float MaxNotional { get; set; }
        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("tickSize")]
        public float TickSize { get; set; }
        [JsonPropertyName("stepSize")]
        public float StepSize { get; set; }
        [JsonPropertyName("apiStateSell")]
        public bool ApiStateSell { get; set; }
        [JsonPropertyName("apiStateBuy")]
        public bool ApiStateBuy { get; set; }
        [JsonPropertyName("timeOnline")]
        public long TimeOnline { get; set; }
        [JsonPropertyName("offTime")]
        public long OffTime { get; set; }
        [JsonPropertyName("maintainTime")]
        public long MaintainTime { get; set; }
    }


    public class BingxContractAddresses
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("data")]
        public BingxContractAddressesDatum[]? Data { get; set; }
    }
    public class BingxContractAddressesDatum
    {
        [JsonPropertyName("coin")]
        public required string Coin { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("networkList")]
        public required BingxContractAddressesNetworkList[] NetworkList { get; set; }
    }
    public class BingxContractAddressesNetworkList
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("network")]
        public required string Network { get; set; }

        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }

        [JsonPropertyName("minConfirm")]
        public int MinConfirm { get; set; }

        [JsonPropertyName("withdrawEnable")]
        public bool WithdrawEnable { get; set; }

        [JsonPropertyName("depositEnable")]
        public bool DepositEnable { get; set; }

        [JsonPropertyName("withdrawFee")]
        public string? WithdrawFee { get; set; }

        [JsonPropertyName("withdrawMax")]
        public string? WithdrawMax { get; set; }

        [JsonPropertyName("withdrawMin")]
        public string? WithdrawMin { get; set; }

        [JsonPropertyName("depositMin")]
        public string? DepositMin { get; set; }

        [JsonPropertyName("withdrawPrecision")]
        public int WithdrawPrecision { get; set; }

        [JsonPropertyName("depositPrecision")]
        public int DepositPrecision { get; set; }

        [JsonPropertyName("contractAddress")]
        public string? ContractAddress { get; set; }
    }


    public class BingxPrices
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
        [JsonPropertyName("data")]
        public BingxDatum[]? Data { get; set; }
    }
    public class BingxDatum
    {
        [JsonPropertyName("eventType")]
        public string? EventType { get; set; }
        [JsonPropertyName("time")]
        public long Time { get; set; }
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("bidPrice")]
        public string? BidPrice { get; set; }
        [JsonPropertyName("bidVolume")]
        public string? BidVolume { get; set; }
        [JsonPropertyName("askPrice")]
        public string? AskPrice { get; set; }
        [JsonPropertyName("askVolume")]
        public string? AskVolume { get; set; }
    }


    public class BingxCommissions
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("data")]
        public BingxCommissionsData? Data { get; set; }
    }
    public class BingxCommissionsData
    {
        [JsonPropertyName("myUpgradeInfo")]
        public object? MyUpgradeInfo { get; set; }

        [JsonPropertyName("contractLevelConfigList")]
        public object[]? ContractLevelConfigList { get; set; }

        [JsonPropertyName("swapLevelConfigList")]
        public object[]? SwapLevelConfigList { get; set; }

        [JsonPropertyName("spotLevelConfigList")]
        public required BingxCommissionsSpotLevelConfigList[] SpotLevelConfigList { get; set; }

        [JsonPropertyName("coinSwapLevelConfigList")]
        public object[]? CoinSwapLevelConfigList { get; set; }

        [JsonPropertyName("isNeverUpgrade")]
        public int IsNeverUpgrade { get; set; }

        [JsonPropertyName("enableTgVipService")]
        public bool EnableTgVipService { get; set; }
    }
    public class BingxCommissionsSpotLevelConfigList
    {
        [JsonPropertyName("vipLevel")]
        public int VipLevel { get; set; }

        [JsonPropertyName("highestVipLevel")]
        public bool HighestVipLevel { get; set; }

        [JsonPropertyName("minAssetAmount")]
        public string? MinAssetAmount { get; set; }

        [JsonPropertyName("maxSpotApiTradePercentage")]
        public string? MaxSpotApiTradePercentage { get; set; }

        [JsonPropertyName("minSpotTradeAmount")]
        public string? MinSpotTradeAmount { get; set; }

        [JsonPropertyName("spotMakerCommission")]
        public string? SpotMakerCommission { get; set; }

        [JsonPropertyName("spotTakerCommission")]
        public required string SpotTakerCommission { get; set; }
    }


    public class BingxOrderBook
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
        [JsonPropertyName("data")]
        public BingxOrderBookData? Data { get; set; }
    }
    public class BingxOrderBookData
    {
        [JsonPropertyName("bids")]
        public required string[][] Bids { get; set; }
        [JsonPropertyName("asks")]
        public required string[][] Asks { get; set; }
        [JsonPropertyName("ts")]
        public long Ts { get; set; }
        [JsonPropertyName("lastUpdateId")]
        public long LastUpdateId { get; set; }
    }
}
