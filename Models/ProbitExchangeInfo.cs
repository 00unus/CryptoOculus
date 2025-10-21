using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class ProbitExchangeInfo
    {
        [JsonPropertyName("data")]
        public ProbitExchangeInfoDatum[]? Data { get; set; }
    }
    public class ProbitExchangeInfoDatum
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("base_currency_id")]
        public required string BaseCurrencyId { get; set; }

        [JsonPropertyName("quote_currency_id")]
        public required string QuoteCurrencyId { get; set; }

        [JsonPropertyName("min_price")]
        public string? MinPrice { get; set; }

        [JsonPropertyName("max_price")]
        public string? MaxPrice { get; set; }

        [JsonPropertyName("price_increment")]
        public string? PriceIncrement { get; set; }

        [JsonPropertyName("min_quantity")]
        public string? MinQuantity { get; set; }

        [JsonPropertyName("max_quantity")]
        public string? MaxQuantity { get; set; }

        [JsonPropertyName("quantity_precision")]
        public int QuantityPrecision { get; set; }

        [JsonPropertyName("min_cost")]
        public string? MinCost { get; set; }

        [JsonPropertyName("max_cost")]
        public string? MaxCost { get; set; }

        [JsonPropertyName("cost_precision")]
        public int CostPrecision { get; set; }

        [JsonPropertyName("maker_fee_rate")]
        public string? MakerFeeRate { get; set; }

        [JsonPropertyName("taker_fee_rate")]
        public string? TakerFeeRate { get; set; }

        [JsonPropertyName("show_in_ui")]
        public bool ShowInUi { get; set; }

        [JsonPropertyName("closed")]
        public bool Closed { get; set; }
    }


    public class ProbitContractAddresses
    {
        [JsonPropertyName("data")]
        public ProbitContractDatum[]? Data { get; set; }
    }
    public class ProbitContractDatum
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("display_name")]
        public object? DisplayName { get; set; }

        [JsonPropertyName("show_in_ui")]
        public bool ShowInUi { get; set; }

        [JsonPropertyName("platform")]
        public required ProbitContractPlatform[] Platform { get; set; }

        [JsonPropertyName("stakeable")]
        public bool Stakeable { get; set; }

        [JsonPropertyName("unstakeable")]
        public bool Unstakeable { get; set; }

        [JsonPropertyName("auto_stake")]
        public bool AutoStake { get; set; }

        [JsonPropertyName("auto_stake_amount")]
        public string? AutoStakeAmount { get; set; }

        [JsonPropertyName("shutdown")]
        public bool Shutdown { get; set; }

        [JsonPropertyName("internal_transfer")]
        public object? InternalTransfer { get; set; }
    }
    public class ProbitContractPlatform
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("deposit")]
        public bool Deposit { get; set; }

        [JsonPropertyName("withdrawal")]
        public bool Withdrawal { get; set; }

        [JsonPropertyName("currency_id")]
        public string? CurrencyId { get; set; }

        [JsonPropertyName("precision")]
        public int Precision { get; set; }

        [JsonPropertyName("min_confirmation_count")]
        public int MinConfirmationCount { get; set; }

        [JsonPropertyName("require_destination_tag")]
        public bool RequireDestinationTag { get; set; }

        [JsonPropertyName("allow_withdrawal_destination_tag")]
        public bool AllowWithdrawalDestinationTag { get; set; }

        [JsonPropertyName("display_name")]
        public required ProbitContractDisplayName DisplayName { get; set; }

        [JsonPropertyName("min_deposit_amount")]
        public string? MinDepositAmount { get; set; }

        [JsonPropertyName("min_withdrawal_amount")]
        public string? MinWithdrawalAmount { get; set; }

        [JsonPropertyName("withdrawal_fee")]
        public required ProbitContractWithdrawalFee[] WithdrawalFee { get; set; }

        [JsonPropertyName("deposit_fee")]
        public ProbitContractDepositFee? DepositFee { get; set; }

        [JsonPropertyName("suspended_reason")]
        public string? SuspendedReason { get; set; }

        [JsonPropertyName("deposit_suspended")]
        public bool DepositSuspended { get; set; }

        [JsonPropertyName("withdrawal_suspended")]
        public bool WithdrawalSuspended { get; set; }

        [JsonPropertyName("platform_currency_display_name")]
        public object? PlatformCurrencyDisplayName { get; set; }
    }
    public class ProbitContractDisplayName
    {
        [JsonPropertyName("name")]
        public required ProbitContractName Name { get; set; }

        [JsonPropertyName("destinationTag")]
        public object? DestinationTag { get; set; }
    }
    public class ProbitContractName
    {
        [JsonPropertyName("en-us")]
        public required string EnUs { get; set; }

        [JsonPropertyName("ko-kr")]
        public string? KoKr { get; set; }
    }
    public class ProbitContractWithdrawalFee
    {
        [JsonPropertyName("currency_id")]
        public required string CurrencyId { get; set; }

        [JsonPropertyName("amount")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public required string Amount { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }
    }
    public class ProbitContractDepositFee
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("proportional")]
        public string? Proportional { get; set; }

        [JsonPropertyName("proportional_currency_id")]
        public string? ProportionalCurrencyId { get; set; }
    }


    public class ProbitContracts
    {
        [JsonPropertyName("withdrawal")]
        public required ProbitContractsWithdrawal[] Withdrawal { get; set; }

        [JsonPropertyName("deposit")]
        public required ProbitContractsDeposit[] Deposit { get; set; }

        [JsonPropertyName("dictionary")]
        public object? Dictionary { get; set; }
    }
    public class ProbitContractsWithdrawal
    {
        [JsonPropertyName("currency_id")]
        public required string CurrencyId { get; set; }

        [JsonPropertyName("platform_id")]
        public required string PlatformId { get; set; }

        [JsonPropertyName("notice")]
        public required ProbitContractsNotice[] Notice { get; set; }
    }
    public class ProbitContractsDeposit
    {
        [JsonPropertyName("currency_id")]
        public required string CurrencyId { get; set; }

        [JsonPropertyName("platform_id")]
        public required string PlatformId { get; set; }

        [JsonPropertyName("notice")]
        public required ProbitContractsNotice[] Notice { get; set; }
    }
    public class ProbitContractsNotice
    {
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("text")]
        public object? Text { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("article_id")]
        public string? ArticleId { get; set; }
    }


    public class ProbitPrices
    {
        [JsonPropertyName("data")]
        public ProbitPricesDatum[]? Data { get; set; }
    }
    public class ProbitPricesDatum
    {
        [JsonPropertyName("last")]
        public required string Last { get; set; }

        [JsonPropertyName("low")]
        public string? Low { get; set; }

        [JsonPropertyName("high")]
        public string? High { get; set; }

        [JsonPropertyName("change")]
        public string? Change { get; set; }

        [JsonPropertyName("base_volume")]
        public string? BaseVolume { get; set; }

        [JsonPropertyName("quote_volume")]
        public string? QuoteVolume { get; set; }

        [JsonPropertyName("market_id")]
        public required string MarketId { get; set; }

        [JsonPropertyName("time")]
        public string? Time { get; set; }
    }


    public class ProbitOrderBook
    {
        [JsonPropertyName("data")]
        public ProbitOrderBookDatum[]? Data { get; set; }
    }
    public class ProbitOrderBookDatum
    {
        [JsonPropertyName("side")]
        public required string Side { get; set; }

        [JsonPropertyName("price")]
        public required string Price { get; set; }

        [JsonPropertyName("quantity")]
        public required string Quantity { get; set; }
    }
}
