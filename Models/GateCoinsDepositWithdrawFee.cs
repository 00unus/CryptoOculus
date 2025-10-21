using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class GateCoinsDepositWithdrawFee
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public GateCoinsDepositWithdrawFeeData? Data { get; set; }

        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; }
    }
    public class GateCoinsDepositWithdrawFeeData
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("page_count")]
        public int PageCount { get; set; }

        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        [JsonPropertyName("list")]
        public GateCoinsDepositWithdrawFeeList[] List { get; set; } = [];
    }
    public class GateCoinsDepositWithdrawFeeList
    {
        [JsonPropertyName("name_en")]
        public string? NameEn { get; set; }

        [JsonPropertyName("name_cn")]
        public string? NameCn { get; set; }

        [JsonPropertyName("coin")]
        public required string Coin { get; set; }

        [JsonPropertyName("chains")]
        public GateCoinsDepositWithdrawFeeChain[] Chains { get; set; } = [];
    }
    public class GateCoinsDepositWithdrawFeeChain
    {
        [JsonPropertyName("chain")]
        public required string Chain { get; set; }

        [JsonPropertyName("name_cn")]
        public string? NameCn { get; set; }

        [JsonPropertyName("name_en")]
        public string? NameEn { get; set; }

        [JsonPropertyName("withdraw_txfee")]
        public string? WithdrawTxfee { get; set; }

        [JsonPropertyName("withdraw_fee_percent")]
        public string? WithdrawFeePercent { get; set; }

        [JsonPropertyName("is_disabled")]
        public int IsDisabled { get; set; }

        [JsonPropertyName("isTag")]
        public int IsTag { get; set; }

        [JsonPropertyName("deposit_txfee")]
        public double? DepositTxfee { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
