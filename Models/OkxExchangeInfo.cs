using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class OkxExchangeInfo
    {
        public string? Code { get; set; }
        public OkxDatum[]? Data { get; set; }
        public string? Msg { get; set; }
    }
    public class OkxDatum
    {
        public string? Alias { get; set; }
        public required string BaseCcy { get; set; }
        public string? Category { get; set; }
        public string? CtMult { get; set; }
        public string? CtType { get; set; }
        public string? CtVal { get; set; }
        public string? CtValCcy { get; set; }
        public string? ExpTime { get; set; }
        public string? InstFamily { get; set; }
        public string? InstId { get; set; }
        public string? InstType { get; set; }
        public string? Lever { get; set; }
        public string? ListTime { get; set; }
        public string? LotSz { get; set; }
        public string? MaxIcebergSz { get; set; }
        public string? MaxLmtAmt { get; set; }
        public string? MaxLmtSz { get; set; }
        public string? MaxMktAmt { get; set; }
        public string? MaxMktSz { get; set; }
        public string? MaxStopSz { get; set; }
        public string? MaxTriggerSz { get; set; }
        public string? MaxTwapSz { get; set; }
        public string? MinSz { get; set; }
        public string? OptType { get; set; }
        public required string QuoteCcy { get; set; }
        public string? RuleType { get; set; }
        public string? SettleCcy { get; set; }
        public required string State { get; set; }
        public string? Stk { get; set; }
        public string? TickSz { get; set; }
        public string? Uly { get; set; }
    }


    public class OkxContractAddresses
    {
        public string? Code { get; set; }
        public OkxContractDatum[]? Data { get; set; }
        public string? Msg { get; set; }
    }
    public class OkxContractDatum
    {
        [JsonPropertyName("burningFeeRate")]
        public string? BurningFeeRate { get; set; }

        [JsonPropertyName("canDep")]
        public bool CanDep { get; set; }

        [JsonPropertyName("canInternal")]
        public bool CanInternal { get; set; }

        [JsonPropertyName("canWd")]
        public bool CanWd { get; set; }

        [JsonPropertyName("ccy")]
        public required string Ccy { get; set; }

        [JsonPropertyName("chain")]
        public required string Chain { get; set; }

        [JsonPropertyName("ctAddr")]
        public required string CtAddr { get; set; }

        [JsonPropertyName("depEstOpenTime")]
        public string? DepEstOpenTime { get; set; }

        [JsonPropertyName("depQuotaFixed")]
        public string? DepQuotaFixed { get; set; }

        [JsonPropertyName("depQuoteDailyLayer2")]
        public string? DepQuoteDailyLayer2 { get; set; }

        [JsonPropertyName("fee")]
        public required string Fee { get; set; }

        [JsonPropertyName("logoLink")]
        public string? LogoLink { get; set; }

        [JsonPropertyName("mainNet")]
        public bool MainNet { get; set; }

        [JsonPropertyName("maxFee")]
        public string? MaxFee { get; set; }

        [JsonPropertyName("maxFeeForCtAddr")]
        public string? MaxFeeForCtAddr { get; set; }

        [JsonPropertyName("maxWd")]
        public string? MaxWd { get; set; }

        [JsonPropertyName("minDep")]
        public string? MinDep { get; set; }

        [JsonPropertyName("minDepArrivalConfirm")]
        public string? MinDepArrivalConfirm { get; set; }

        [JsonPropertyName("minFee")]
        public string? MinFee { get; set; }

        [JsonPropertyName("minFeeForCtAddr")]
        public string? MinFeeForCtAddr { get; set; }

        [JsonPropertyName("minInternal")]
        public string? MinInternal { get; set; }

        [JsonPropertyName("minWd")]
        public string? MinWd { get; set; }

        [JsonPropertyName("minWdUnlockConfirm")]
        public string? MinWdUnlockConfirm { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("needTag")]
        public bool NeedTag { get; set; }

        [JsonPropertyName("usedDepQuotaFixed")]
        public string? UsedDepQuotaFixed { get; set; }

        [JsonPropertyName("usedWdQuota")]
        public string? UsedWdQuota { get; set; }

        [JsonPropertyName("wdEstOpenTime")]
        public string? WdEstOpenTime { get; set; }

        [JsonPropertyName("wdQuota")]
        public string? WdQuota { get; set; }

        [JsonPropertyName("wdTickSz")]
        public string? WdTickSz { get; set; }
    }


    public class OkxPrices
    {
        public string? Code { get; set; }
        public string? Msg { get; set; }
        public OkxPricesDatum[]? Data { get; set; }
    }
    public class OkxPricesDatum
    {
        public string? OnstType { get; set; }
        public required string InstId { get; set; }
        public string? Last { get; set; }
        public string? LastSz { get; set; }
        public required string AskPx { get; set; }
        public string? AskSz { get; set; }
        public required string BidPx { get; set; }
        public string? BidSz { get; set; }
        public string? Open24h { get; set; }
        public string? High24h { get; set; }
        public string? Low24h { get; set; }
        public string? VolCcy24h { get; set; }
        public string? Vol24h { get; set; }
        public string? Ts { get; set; }
        public string? SodUtc0 { get; set; }
        public string? SodUtc8 { get; set; }
    }


    public class OkxTradeFee
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("data")]
        public required OkxTradeFeeData[] Data { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
    }
    public class OkxTradeFeeData
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("delivery")]
        public string? Delivery { get; set; }

        [JsonPropertyName("exercise")]
        public string? Exercise { get; set; }

        [JsonPropertyName("fiat")]
        public object[]? Fiat { get; set; }

        [JsonPropertyName("instType")]
        public string? InstType { get; set; }

        [JsonPropertyName("level")]
        public string? Level { get; set; }

        [JsonPropertyName("maker")]
        public string? Maker { get; set; }

        [JsonPropertyName("makerU")]
        public string? MakerU { get; set; }

        [JsonPropertyName("makerUSDC")]
        public string? MakerUSDC { get; set; }

        [JsonPropertyName("ruleType")]
        public string? RuleType { get; set; }

        [JsonPropertyName("taker")]
        public required string Taker { get; set; }

        [JsonPropertyName("takerU")]
        public string? TakerU { get; set; }

        [JsonPropertyName("takerUSDC")]
        public string? TakerUSDC { get; set; }

        [JsonPropertyName("ts")]
        public string? Ts { get; set; }
    }


    public class OkxOrderBook
    {
        public string? Code { get; set; }
        public string? Msg { get; set; }
        public OkxOrderBookDatum[]? Data { get; set; }
    }
    public class OkxOrderBookDatum
    {
        public required string[][] Asks { get; set; }
        public required string[][] Bids { get; set; }
        public string? Ts { get; set; }
    }
}
