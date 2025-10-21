using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class GateExchangeInfo
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }
        [JsonPropertyName("base")]
        public required string Base { get; set; }
        [JsonPropertyName("quote")]
        public required string Quote { get; set; }
        [JsonPropertyName("fee")]
        public required string Fee { get; set; }
        [JsonPropertyName("min_base_amount")]
        public string? Min_base_amount { get; set; }
        [JsonPropertyName("min_quote_amount")]
        public string? Min_quote_amount { get; set; }
        [JsonPropertyName("max_quote_amount")]
        public string? Max_quote_amount { get; set; }
        [JsonPropertyName("amount_precision")]
        public int Amount_precision { get; set; }
        [JsonPropertyName("precision")]
        public int Precision { get; set; }
        [JsonPropertyName("trade_status")]
        public required string Trade_status { get; set; }
        [JsonPropertyName("sell_start")]
        public int Sell_start { get; set; }
        [JsonPropertyName("buy_start")]
        public int Buy_start { get; set; }
        [JsonPropertyName("max_base_amount")]
        public string? Max_base_amount { get; set; }
    }


    public class GateContractAddresses
    {
        public required long Timestamp { get; set; }
        public required GateCurrency[] Currencies { get; set; }
    }
    public class GateCurrency
    {
        public required string Currency { get; set; }
        public required AssetNetwork[] Networks { get; set; }
    }


    public class GateCurrencyChains
    {
        [JsonPropertyName("chain")]
        public required string Chain { get; set; }

        [JsonPropertyName("name_cn")]
        public string? NameCn { get; set; }

        [JsonPropertyName("name_en")]
        public required string NameEn { get; set; }

        [JsonPropertyName("is_disabled")]
        public int IsDisabled { get; set; }

        [JsonPropertyName("contract_address")]
        public string? ContractAddress { get; set; }

        [JsonPropertyName("decimal")]
        public string? Decimal { get; set; }

        [JsonPropertyName("is_tag")]
        public int IsTag { get; set; }

        [JsonPropertyName("is_withdraw_disabled")]
        public int IsWithdrawDisabled { get; set; }

        [JsonPropertyName("is_deposit_disabled")]
        public int IsDepositDisabled { get; set; }
    }


    public class GatePrice
    {
        [JsonPropertyName("currency_pair")]
        public required string Currency_pair { get; set; }
        [JsonPropertyName("last")]
        public string? Last { get; set; }
        [JsonPropertyName("lowest_ask")]
        public required string Lowest_ask { get; set; }
        [JsonPropertyName("highest_bid")]
        public required string Highest_bid { get; set; }
        [JsonPropertyName("change_percentage")]
        public string? Change_percentage { get; set; }
        [JsonPropertyName("change_utc0")]
        public string? Change_utc0 { get; set; }
        [JsonPropertyName("change_utc8")]
        public string? Change_utc8 { get; set; }
        [JsonPropertyName("base_volume")]
        public string? Base_volume { get; set; }
        [JsonPropertyName("quote_volume")]
        public string? Quote_volume { get; set; }
        [JsonPropertyName("high_24h")]
        public string? High_24h { get; set; }
        [JsonPropertyName("low_24h")]
        public string? Low_24h { get; set; }
        [JsonPropertyName("etf_net_value")]
        public string? Etf_net_value { get; set; }
        [JsonPropertyName("etf_pre_net_value")]
        public string? Etf_pre_net_value { get; set; }
        [JsonPropertyName("etf_pre_timestamp")]
        public int Etf_pre_timestamp { get; set; }
        [JsonPropertyName("etf_leverage")]
        public string? Etf_leverage { get; set; }
    }


    public class GateOrderBook
    {
        [JsonPropertyName("current")]
        public long Current { get; set; }
        [JsonPropertyName("update")]
        public long Update { get; set; }
        [JsonPropertyName("asks")]
        public required string[][] Asks { get; set; }
        [JsonPropertyName("bids")]
        public required string[][] Bids { get; set; }
    }
}
