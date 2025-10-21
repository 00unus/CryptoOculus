using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class BitmartExchangeInfo
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("trace")]
        public string? Trace { get; set; }
        [JsonPropertyName("data")]
        public BitmartData? Data { get; set; }
    }
    public class BitmartData
    {
        [JsonPropertyName("symbols")]
        public required BitmartSymbol[] Symbols { get; set; }
    }
    public class BitmartSymbol
    {
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("symbol_id")]
        public int Symbol_id { get; set; }
        [JsonPropertyName("base_currency")]
        public required string Base_currency { get; set; }
        [JsonPropertyName("quote_currency")]
        public required string Quote_currency { get; set; }
        [JsonPropertyName("quote_increment")]
        public string? Quote_increment { get; set; }
        [JsonPropertyName("base_min_size")]
        public string? Base_min_size { get; set; }
        [JsonPropertyName("price_min_precision")]
        public int Price_min_precision { get; set; }
        [JsonPropertyName("price_max_precision")]
        public int Price_max_precision { get; set; }
        [JsonPropertyName("expiration")]
        public string? Expiration { get; set; }
        [JsonPropertyName("min_buy_amount")]
        public string? Min_buy_amount { get; set; }
        [JsonPropertyName("min_sell_amount")]
        public string? Min_sell_amount { get; set; }
        [JsonPropertyName("trade_status")]
        public required string Trade_status { get; set; }
    }


    public class BitmartContractAddress
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("trace")]
        public string? Trace { get; set; }
        [JsonPropertyName("data")]
        public BitmartCurrencyData? Data { get; set; }
    }
    public class BitmartCurrencyData
    {
        [JsonPropertyName("currencies")]
        public required BitmartContractCurrency[] Currencies { get; set; }
    }
    public class BitmartContractCurrency
    {
        [JsonPropertyName("currency")]
        public required string Currency { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("contract_address")]
        public string? Contract_address { get; set; }
        [JsonPropertyName("network")]
        public string? Network { get; set; }
        [JsonPropertyName("withdraw_enabled")]
        public bool Withdraw_enabled { get; set; }
        [JsonPropertyName("deposit_enabled")]
        public bool Deposit_enabled { get; set; }
        [JsonPropertyName("withdraw_minsize")]
        public string? Withdraw_minsize { get; set; }
        [JsonPropertyName("withdraw_minfee")]
        public string? Withdraw_minfee { get; set; }
    }


    public class BitmartPrices
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("trace")]
        public string? Trace { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("data")]
        public string[][]? Data { get; set; }
    }


    public class BitmartOrderBook
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("trace")]
        public string? Trace { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("data")]
        public BitmartOrderBookData? Data { get; set; }
    }
    public class BitmartOrderBookData
    {
        [JsonPropertyName("ts")]
        public string? Ts { get; set; }
        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }
        [JsonPropertyName("asks")]
        public required string[][] Asks { get; set; }
        [JsonPropertyName("bids")]
        public required string[][] Bids { get; set; }
    }
}
