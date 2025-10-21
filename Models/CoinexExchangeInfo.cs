using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class CoinexExchangeInfo
    {
        public int Code { get; set; }
        public CoinexData[]? Data { get; set; }
        public string? Message { get; set; }
    }
    public class CoinexData
    {
        [JsonPropertyName("base_ccy")]
        public required string BaseCcy { get; set; }
        public int BaseCcyPrecision { get; set; }
        public bool IsAmmAvailable { get; set; }
        public bool IsMarginAvailable { get; set; }
        public string? MakerFeeRate { get; set; }
        [JsonPropertyName("market")]
        public required string Market { get; set; }
        public string? MinAmount { get; set; }
        [JsonPropertyName("quote_ccy")]
        public required string QuoteCcy { get; set; }
        public int QuoteCcyPrecision { get; set; }
        public string? TakerFeeRate { get; set; }
    }


    public class CoinexContractAddresses
    {
        public int Code { get; set; }
        public CoinexDatum[]? Data { get; set; }
        public string? Message { get; set; }
    }
    public class CoinexDatum
    {
        [JsonPropertyName("short_name")]
        public required string ShortName { get; set; }
        public string? FullName { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? WhitePaperUrl { get; set; }
        [JsonPropertyName("chain_info")]
        public required CoinexChainInfo[] ChainInfo { get; set; }
    }
    public class CoinexChainInfo
    {
        [JsonPropertyName("chain_name")]
        public required string ChainName { get; set; }
        [JsonPropertyName("identity")]
        public string? Identity { get; set; }
        public string? ExplorerUrl { get; set; }
    }


    public class CoinexCurrencyDetails
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public CoinexCurrencyData[]? Data { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
    public class CoinexCurrencyData
    {
        [JsonPropertyName("asset")]
        public required CoinexCurrencyAsset Asset { get; set; }

        [JsonPropertyName("chains")]
        public required CoinexCurrencyChain[] Chains { get; set; }
    }
    public class CoinexCurrencyAsset
    {
        [JsonPropertyName("ccy")]
        public required string Ccy { get; set; }

        [JsonPropertyName("deposit_enabled")]
        public bool DepositEnabled { get; set; }

        [JsonPropertyName("withdraw_enabled")]
        public bool WithdrawEnabled { get; set; }

        [JsonPropertyName("inter_transfer_enabled")]
        public bool InterTransferEnabled { get; set; }

        [JsonPropertyName("is_st")]
        public bool IsSt { get; set; }
    }
    public class CoinexCurrencyChain
    {
        [JsonPropertyName("chain")]
        public required string Chain { get; set; }

        [JsonPropertyName("min_deposit_amount")]
        public string? MinDepositAmount { get; set; }

        [JsonPropertyName("min_withdraw_amount")]
        public string? MinWithdrawAmount { get; set; }

        [JsonPropertyName("deposit_enabled")]
        public bool DepositEnabled { get; set; }

        [JsonPropertyName("withdraw_enabled")]
        public bool WithdrawEnabled { get; set; }

        [JsonPropertyName("deposit_delay_minutes")]
        public int DepositDelayMinutes { get; set; }

        [JsonPropertyName("safe_confirmations")]
        public int SafeConfirmations { get; set; }

        [JsonPropertyName("irreversible_confirmations")]
        public int IrreversibleConfirmations { get; set; }

        [JsonPropertyName("deflation_rate")]
        public string? DeflationRate { get; set; }

        [JsonPropertyName("withdrawal_fee")]
        public string? WithdrawalFee { get; set; }

        [JsonPropertyName("withdrawal_precision")]
        public int WithdrawalPrecision { get; set; }

        [JsonPropertyName("memo")]
        public string? Memo { get; set; }

        [JsonPropertyName("is_memo_required_for_deposit")]
        public bool IsMemoRequiredForDeposit { get; set; }

        [JsonPropertyName("explorer_asset_url")]
        public string? ExplorerAssetUrl { get; set; }
    }


    public class CoinexPrices
    {
        public int Code { get; set; }
        public CoinexPricesDatum[]? Data { get; set; }
        public string? Message { get; set; }
    }
    public class CoinexPricesDatum
    {
        public string? Close { get; set; }
        public string? High { get; set; }
        public required string Last { get; set; }
        public string? Low { get; set; }
        public required string Market { get; set; }
        public string? Open { get; set; }
        public int Period { get; set; }
        public string? Value { get; set; }
        public string? Volume { get; set; }
        public string? VolumeBuy { get; set; }
        public string? VolumeSell { get; set; }
    }


    public class CoinexOrderBook
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public CoinexOrderBookData? Data { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
    public class CoinexOrderBookData
    {
        [JsonPropertyName("depth")]
        public required CoinexOrderBookDepth Depth { get; set; }

        [JsonPropertyName("is_full")]
        public bool IsFull { get; set; }

        [JsonPropertyName("market")]
        public string? Market { get; set; }
    }
    public class CoinexOrderBookDepth
    {
        [JsonPropertyName("asks")]
        public string[][]? Asks { get; set; }

        [JsonPropertyName("bids")]
        public string[][]? Bids { get; set; }

        [JsonPropertyName("checksum")]
        public long Checksum { get; set; }

        [JsonPropertyName("last")]
        public string? Last { get; set; }

        [JsonPropertyName("updated_at")]
        public long UpdatedAt { get; set; }
    }
}
