using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class AscendexExchangeInfo
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public AscendexExchangeDatum[]? Data { get; set; }
    }
    public class AscendexExchangeDatum
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("tradingStartTime")]
        public object? TradingStartTime { get; set; }

        [JsonPropertyName("collapseDecimals")]
        public string? CollapseDecimals { get; set; }

        [JsonPropertyName("minQty")]
        public string? MinQty { get; set; }

        [JsonPropertyName("maxQty")]
        public string? MaxQty { get; set; }

        [JsonPropertyName("minNotional")]
        public string? MinNotional { get; set; }

        [JsonPropertyName("maxNotional")]
        public string? MaxNotional { get; set; }

        [JsonPropertyName("statusCode")]
        public required string StatusCode { get; set; }

        [JsonPropertyName("statusMessage")]
        public string? StatusMessage { get; set; }

        [JsonPropertyName("tickSize")]
        public string? TickSize { get; set; }

        [JsonPropertyName("useTick")]
        public bool UseTick { get; set; }

        [JsonPropertyName("lotSize")]
        public string? LotSize { get; set; }

        [JsonPropertyName("useLot")]
        public bool UseLot { get; set; }

        [JsonPropertyName("commissionType")]
        public string? CommissionType { get; set; }

        [JsonPropertyName("commissionReserveRate")]
        public string? CommissionReserveRate { get; set; }

        [JsonPropertyName("qtyScale")]
        public int QtyScale { get; set; }

        [JsonPropertyName("priceScale")]
        public int PriceScale { get; set; }

        [JsonPropertyName("notionalScale")]
        public int NotionalScale { get; set; }
    }


    public class AscendexContractAddresses
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public AscendexContractDatum[]? Data { get; set; }
    }
    public class AscendexContractDatum
    {
        [JsonPropertyName("assetCode")]
        public required string AssetCode { get; set; }

        [JsonPropertyName("assetName")]
        public string? AssetName { get; set; }

        [JsonPropertyName("precisionScale")]
        public int PrecisionScale { get; set; }

        [JsonPropertyName("nativeScale")]
        public int NativeScale { get; set; }

        [JsonPropertyName("blockChain")]
        public required AscendexContractBlockChain[] BlockChain { get; set; }
    }
    public class AscendexContractBlockChain
    {
        [JsonPropertyName("chainName")]
        public required string ChainName { get; set; }

        [JsonPropertyName("withdrawFee")]
        public string? WithdrawFee { get; set; }

        [JsonPropertyName("allowDeposit")]
        public bool AllowDeposit { get; set; }

        [JsonPropertyName("allowWithdraw")]
        public bool AllowWithdraw { get; set; }

        [JsonPropertyName("minDepositAmt")]
        public string? MinDepositAmt { get; set; }

        [JsonPropertyName("minWithdrawal")]
        public string? MinWithdrawal { get; set; }

        [JsonPropertyName("numConfirmations")]
        public int NumConfirmations { get; set; }
    }


    public class AscendexPrices
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public AscendexPricesDatum[]? Data { get; set; }
    }
    public class AscendexPricesDatum
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }

        [JsonPropertyName("open")]
        public string? Open { get; set; }

        [JsonPropertyName("close")]
        public string? Close { get; set; }

        [JsonPropertyName("high")]
        public string? High { get; set; }

        [JsonPropertyName("low")]
        public string? Low { get; set; }

        [JsonPropertyName("volume")]
        public string? Volume { get; set; }

        [JsonPropertyName("ask")]
        public required string[] Ask { get; set; }

        [JsonPropertyName("bid")]
        public required string[] Bid { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }


    public class AscendexOrderBook
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public AscendexOrderBookData? Data { get; set; }
    }
    public class AscendexOrderBookData
    {
        [JsonPropertyName("m")]
        public string? M { get; set; }

        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("data")]
        public required AscendexOrderBookData2 Data { get; set; }
    }
    public class AscendexOrderBookData2
    {
        [JsonPropertyName("ts")]
        public long Ts { get; set; }

        [JsonPropertyName("seqnum")]
        public long Seqnum { get; set; }

        [JsonPropertyName("asks")]
        public string[][]? Asks { get; set; }

        [JsonPropertyName("bids")]
        public string[][]? Bids { get; set; }
    }
}
