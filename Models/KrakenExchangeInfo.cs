using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class KrakenExchangeInfo
    {
        [JsonPropertyName("error")]
        public required object[] Error { get; set; }

        [JsonPropertyName("result")]
        public Dictionary<string, KrakenExchangeResult>? Result { get; set; }
    }
    public class KrakenExchangeResult
    {
        [JsonPropertyName("altname")]
        public required string Altname { get; set; }

        [JsonPropertyName("wsname")]
        public required string Wsname { get; set; }

        [JsonPropertyName("aclass_base")]
        public string? AclassBase { get; set; }

        [JsonPropertyName("base")]
        public required string Base { get; set; }

        [JsonPropertyName("aclass_quote")]
        public string? AclassQuote { get; set; }

        [JsonPropertyName("quote")]
        public required string Quote { get; set; }

        [JsonPropertyName("lot")]
        public string? Lot { get; set; }

        [JsonPropertyName("cost_decimals")]
        public int CostDecimals { get; set; }

        [JsonPropertyName("pair_decimals")]
        public int PairDecimals { get; set; }

        [JsonPropertyName("lot_decimals")]
        public int LotDecimals { get; set; }

        [JsonPropertyName("lot_multiplier")]
        public int LotMultiplier { get; set; }

        [JsonPropertyName("leverage_buy")]
        public object[]? LeverageBuy { get; set; }

        [JsonPropertyName("leverage_sell")]
        public object[]? LeverageSell { get; set; }

        [JsonPropertyName("fees")]
        public double[][]? Fees { get; set; }

        [JsonPropertyName("fees_maker")]
        public double[][]? FeesMaker { get; set; }

        [JsonPropertyName("fee_volume_currency")]
        public string? FeeVolumeCurrency { get; set; }

        [JsonPropertyName("margin_call")]
        public int MarginCall { get; set; }

        [JsonPropertyName("margin_stop")]
        public int MarginStop { get; set; }

        [JsonPropertyName("ordermin")]
        public string? Ordermin { get; set; }

        [JsonPropertyName("costmin")]
        public string? Costmin { get; set; }

        [JsonPropertyName("tick_size")]
        public string? TickSize { get; set; }

        [JsonPropertyName("status")]
        public required string Status { get; set; }
    }


    public class KrakenDepositMethods
    {
        [JsonPropertyName("result")]
        public KrakenDepositMethodsResult[]? Result { get; set; }

        [JsonPropertyName("errors")]
        public object[]? Errors { get; set; }
    }
    public class KrakenDepositMethodsResult
    {
        [JsonPropertyName("asset")]
        public required string Asset { get; set; }

        [JsonPropertyName("asset_class")]
        public required string AssetClass { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type_id")]
        public string? TypeId { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("fee")]
        public string? Fee { get; set; }

        [JsonPropertyName("fee_percentage")]
        public string? FeePercentage { get; set; }

        [JsonPropertyName("limits")]
        public object? Limits { get; set; }

        [JsonPropertyName("direct_funding")]
        public bool DirectFunding { get; set; }

        [JsonPropertyName("information")]
        public KrakenDepositMethodsInformation[]? Information { get; set; }

        [JsonPropertyName("name_display")]
        public string? NameDisplay { get; set; }

        [JsonPropertyName("funding_limits")]
        public KrakenDepositMethodsFundingLimit[]? FundingLimits { get; set; }

        [JsonPropertyName("sort_weight")]
        public int? SortWeight { get; set; }

        [JsonPropertyName("allow_new_funding_id")]
        public int? AllowNewFundingId { get; set; }

        [JsonPropertyName("deposit_network_info")]
        public KrakenDepositMethodsDepositNetworkInfo? DepositNetworkInfo { get; set; }

        [JsonPropertyName("caip2_id")]
        public string? Caip2Id { get; set; }

        [JsonPropertyName("network_id")]
        public string? NetworkId { get; set; }
    }
    public class KrakenDepositMethodsInformation
    {
        [JsonPropertyName("miscellaneous")]
        public KrakenDepositMethodsMiscellaneous? Miscellaneous { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("bank")]
        public KrakenDepositMethodsBank? Bank { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        [JsonPropertyName("is_new")]
        public bool? IsNew { get; set; }

        [JsonPropertyName("tag")]
        public string? Tag { get; set; }
    }
    public class KrakenDepositMethodsFundingLimit
    {
        [JsonPropertyName("limit_type")]
        public string? LimitType { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("limits")]
        public object? Limits { get; set; }
    }
    public class KrakenDepositMethodsDepositNetworkInfo
    {
        [JsonPropertyName("network")]
        public required string Network { get; set; }

        [JsonPropertyName("explorer")]
        public string? Explorer { get; set; }

        [JsonPropertyName("confirmations")]
        public string? Confirmations { get; set; }

        [JsonPropertyName("confirmation_time")]
        public string? ConfirmationTime { get; set; }
    }
    public class KrakenDepositMethodsMiscellaneous
    {
        [JsonPropertyName("transaction_limits")]
        public KrakenDepositMethodsTransactionLimits? TransactionLimits { get; set; }

        [JsonPropertyName("deposit_limit")]
        public KrakenDepositMethodsDepositLimit? DepositLimit { get; set; }

        [JsonPropertyName("googlePayDepositMID")]
        public string? GooglePayDepositMID { get; set; }

        [JsonPropertyName("allowCreditCards")]
        public bool AllowCreditCards { get; set; }
    }
    public class KrakenDepositMethodsBank
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("iban")]
        public string? Iban { get; set; }

        [JsonPropertyName("bic")]
        public string? Bic { get; set; }
    }
    public class KrakenDepositMethodsDepositLimit
    {
        [JsonPropertyName("rolling")]
        public object? Rolling { get; set; }

        [JsonPropertyName("maxPerDeposit")]
        public string? MaxPerDeposit { get; set; }
    }
    public class KrakenDepositMethodsTransactionLimits
    {
        [JsonPropertyName("rolling")]
        public object? Rolling { get; set; }
    }


    public class KrakenWithdrawalMethods
    {
        [JsonPropertyName("result")]
        public KrakenWithdrawalMethodsResult[]? Result { get; set; }

        [JsonPropertyName("errors")]
        public object[]? Errors { get; set; }
    }
    public class KrakenWithdrawalMethodsResult
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("type_name")]
        public string? TypeName { get; set; }

        [JsonPropertyName("asset")]
        public required string Asset { get; set; }

        [JsonPropertyName("asset_class")]
        public string? AssetClass { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("fee")]
        public string? Fee { get; set; }

        [JsonPropertyName("fee_token")]
        public string? FeeToken { get; set; }

        [JsonPropertyName("min_amount")]
        public string? MinAmount { get; set; }

        [JsonPropertyName("max_amount")]
        public string? MaxAmount { get; set; }

        [JsonPropertyName("sort_weight")]
        public int SortWeight { get; set; }

        [JsonPropertyName("validate")]
        public object? Validate { get; set; }

        [JsonPropertyName("temp_disabled_public")]
        public bool TempDisabledPublic { get; set; }

        [JsonPropertyName("name_display")]
        public string? NameDisplay { get; set; }

        [JsonPropertyName("validate_meta")]
        public object? ValidateMeta { get; set; }

        [JsonPropertyName("withdrawal_network_info")]
        public KrakenWithdrawalMethodsWithdrawalNetworkInfo? WithdrawalNetworkInfo { get; set; }

        [JsonPropertyName("caip2_id")]
        public string? Caip2Id { get; set; }

        [JsonPropertyName("network_id")]
        public string? NetworkId { get; set; }

        [JsonPropertyName("address_verification_method")]
        public string? AddressVerificationMethod { get; set; }
    }
    public class KrakenWithdrawalMethodsWithdrawalNetworkInfo
    {
        [JsonPropertyName("network")]
        public required string Network { get; set; }

        [JsonPropertyName("explorer")]
        public string? Explorer { get; set; }

        [JsonPropertyName("confirmations")]
        public string? Confirmations { get; set; }

        [JsonPropertyName("confirmation_time")]
        public string? ConfirmationTime { get; set; }
    }


    public class KrakenContractAddresses
    {
        [JsonPropertyName("error")]
        public required object[] Error { get; set; }

        [JsonPropertyName("result")]
        public Dictionary<string, KrakenContractResult>? Result { get; set; }
    }
    public class KrakenContractResult
    {
        [JsonPropertyName("aclass")]
        public string? Aclass { get; set; }

        [JsonPropertyName("altname")]
        public required string Altname { get; set; }

        [JsonPropertyName("decimals")]
        public int Decimals { get; set; }

        [JsonPropertyName("display_decimals")]
        public int DisplayDecimals { get; set; }

        [JsonPropertyName("status")]
        public required string Status { get; set; }
    }


    public class KrakenPrices
    {
        [JsonPropertyName("error")]
        public required object[] Error { get; set; }

        [JsonPropertyName("result")]
        public Dictionary<string, KrakenPricesResult>? Result { get; set; }
    }
    public class KrakenPricesResult
    {
        [JsonPropertyName("a")]
        public required string[] A { get; set; }

        [JsonPropertyName("b")]
        public required string[] B { get; set; }

        [JsonPropertyName("c")]
        public string[]? C { get; set; }

        [JsonPropertyName("v")]
        public string[]? V { get; set; }

        [JsonPropertyName("p")]
        public string[]? P { get; set; }

        [JsonPropertyName("t")]
        public int[]? T { get; set; }

        [JsonPropertyName("l")]
        public string[]? L { get; set; }

        [JsonPropertyName("h")]
        public string[]? H { get; set; }

        [JsonPropertyName("o")]
        public string? O { get; set; }
    }


    public class KrakenOrderBook
    {
        [JsonPropertyName("error")]
        public required object[] Error { get; set; }

        [JsonPropertyName("result")]
        public Dictionary<string, KrakenOrderBookResult>? Result { get; set; }
    }
    public class KrakenOrderBookResult
    {
        [JsonPropertyName("asks")]
        public object[][]? Asks { get; set; }

        [JsonPropertyName("bids")]
        public object[][]? Bids { get; set; }
    }
}
