using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class CoinwExchangeInfo
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("data")]
        public CoinwExchangeDatum[]? Data { get; set; }

        [JsonPropertyName("failed")]
        public bool Failed { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
    public class CoinwExchangeDatum
    {
        [JsonPropertyName("currencyBase")]
        public required string CurrencyBase { get; set; }

        [JsonPropertyName("maxBuyCount")]
        public string? MaxBuyCount { get; set; }

        [JsonPropertyName("pricePrecision")]
        public int PricePrecision { get; set; }

        [JsonPropertyName("minBuyPrice")]
        public string? MinBuyPrice { get; set; }

        [JsonPropertyName("currencyPair")]
        public required string CurrencyPair { get; set; }

        [JsonPropertyName("minBuyAmount")]
        public string? MinBuyAmount { get; set; }

        [JsonPropertyName("maxBuyPrice")]
        public string? MaxBuyPrice { get; set; }

        [JsonPropertyName("currencyQuote")]
        public required string CurrencyQuote { get; set; }

        [JsonPropertyName("countPrecision")]
        public int CountPrecision { get; set; }

        [JsonPropertyName("minBuyCount")]
        public string? MinBuyCount { get; set; }

        [JsonPropertyName("state")]
        public int State { get; set; }

        [JsonPropertyName("maxBuyAmount")]
        public string? MaxBuyAmount { get; set; }
    }


    public class CoinwContractAddresses
    {
        public required long Timestamp { get; set; }
        public required CoinwCurrency[] Currencies { get; set; }
    }
    public class CoinwCurrency
    {
        public required string Currency { get; set; }
        public required AssetNetwork[] Networks { get; set; }
    }


    public class CoinwCoinOption
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public CoinwCoinOptionData[]? Data { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
    }
    public class CoinwCoinOptionData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("logo")]
        public string? Logo { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("withdrawStatus")]
        public int WithdrawStatus { get; set; }

        [JsonPropertyName("rechargeStatus")]
        public int RechargeStatus { get; set; }

        [JsonPropertyName("hyperWithdrawStatus")]
        public int HyperWithdrawStatus { get; set; }

        [JsonPropertyName("hyperRechargeStatus")]
        public int HyperRechargeStatus { get; set; }

        [JsonPropertyName("rateOfUSDT")]
        public string? RateOfUSDT { get; set; }

        [JsonPropertyName("rateOfCountry")]
        public string? RateOfCountry { get; set; }

        [JsonPropertyName("memoTag")]
        public string? MemoTag { get; set; }

        [JsonPropertyName("useMemoTag")]
        public int UseMemoTag { get; set; }

        [JsonPropertyName("memoRequired")]
        public int MemoRequired { get; set; }

        [JsonPropertyName("chainNameList")]
        public string[]? ChainNameList { get; set; }

        [JsonPropertyName("memoChainNames")]
        public required CoinwCoinOptionMemoChainName[] MemoChainNames { get; set; }

        [JsonPropertyName("availableTransfer")]
        public string? AvailableTransfer { get; set; }

        [JsonPropertyName("availableTransferOfCountry")]
        public string? AvailableTransferOfCountry { get; set; }
    }
    public class CoinwCoinOptionMemoChainName
    {
        [JsonPropertyName("chainName")]
        public required string ChainName { get; set; }

        [JsonPropertyName("chainNameAlias")]
        public string? ChainNameAlias { get; set; }

        [JsonPropertyName("memoTag")]
        public string? MemoTag { get; set; }

        [JsonPropertyName("useMemoTag")]
        public int UseMemoTag { get; set; }

        [JsonPropertyName("memoRequired")]
        public int MemoRequired { get; set; }

        [JsonPropertyName("withdrawStatus")]
        public int WithdrawStatus { get; set; }

        [JsonPropertyName("rechargeStatus")]
        public int RechargeStatus { get; set; }
    }


    public class CoinwAddressList
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public CoinwAddressListData[]? Data { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
    }
    public class CoinwAddressListData
    {
        [JsonPropertyName("chainName")]
        public required string ChainName { get; set; }

        [JsonPropertyName("chainNameAlias")]
        public string? ChainNameAlias { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("memo")]
        public object? Memo { get; set; }

        [JsonPropertyName("memoTag")]
        public string? MemoTag { get; set; }

        [JsonPropertyName("useMemoTag")]
        public int UseMemoTag { get; set; }

        [JsonPropertyName("minRechargeAmount")]
        public string? MinRechargeAmount { get; set; }

        [JsonPropertyName("confirmCount")]
        public string? ConfirmCount { get; set; }

        [JsonPropertyName("minTransferInAmount")]
        public string? MinTransferInAmount { get; set; }

        [JsonPropertyName("contractAddress")]
        public string? ContractAddress { get; set; }
    }
    

    public class CoinwWalletCoin
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("data")]
        public CoinwWalletCoinData? Data { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
    }
    public class CoinwWalletCoinData
    {
        [JsonPropertyName("amount")]
        public string? Amount { get; set; }

        [JsonPropertyName("amountOfCountry")]
        public string? AmountOfCountry { get; set; }

        [JsonPropertyName("amountUsd")]
        public object? AmountUsd { get; set; }

        [JsonPropertyName("walletAccountType")]
        public string? WalletAccountType { get; set; }

        [JsonPropertyName("itemList")]
        public required CoinwWalletCoinItemList[] ItemList { get; set; }

        [JsonPropertyName("totalAmount")]
        public string? TotalAmount { get; set; }

        [JsonPropertyName("totalAmountOfCountry")]
        public string? TotalAmountOfCountry { get; set; }
    }
    public class CoinwWalletCoinItemList
    {
        [JsonPropertyName("totalAmount")]
        public string? TotalAmount { get; set; }

        [JsonPropertyName("totalAmountUsd")]
        public string? TotalAmountUsd { get; set; }

        [JsonPropertyName("availableAmount")]
        public string? AvailableAmount { get; set; }

        [JsonPropertyName("holdAmount")]
        public string? HoldAmount { get; set; }

        [JsonPropertyName("coinId")]
        public int CoinId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("totalAmountOfCountry")]
        public string? TotalAmountOfCountry { get; set; }

        [JsonPropertyName("availableAmountOfCountry")]
        public string? AvailableAmountOfCountry { get; set; }

        [JsonPropertyName("holdAmountOfCountry")]
        public string? HoldAmountOfCountry { get; set; }

        [JsonPropertyName("logo")]
        public string? Logo { get; set; }

        [JsonPropertyName("areaType")]
        public string? AreaType { get; set; }

        [JsonPropertyName("memoTag")]
        public string? MemoTag { get; set; }

        [JsonPropertyName("useMemoTag")]
        public int UseMemoTag { get; set; }

        [JsonPropertyName("memoRequired")]
        public int MemoRequired { get; set; }

        [JsonPropertyName("withdrawDigit")]
        public int WithdrawDigit { get; set; }

        [JsonPropertyName("withdrawStatus")]
        public int WithdrawStatus { get; set; }

        [JsonPropertyName("rechargeStatus")]
        public int RechargeStatus { get; set; }

        [JsonPropertyName("hyperpayRechargeStatus")]
        public int HyperpayRechargeStatus { get; set; }

        [JsonPropertyName("hyperpayWithdrawStatus")]
        public int HyperpayWithdrawStatus { get; set; }

        [JsonPropertyName("chainNameList")]
        public string[]? ChainNameList { get; set; }

        [JsonPropertyName("memoChainNames")]
        public CoinwWalletCoinMemoChainName[]? MemoChainNames { get; set; }

        [JsonPropertyName("withdrawCountLimitList")]
        public CoinwWalletCoinWithdrawCountLimitList[]? WithdrawCountLimitList { get; set; }

        [JsonPropertyName("withdrawFeeList")]
        public required CoinwWalletCoinWithdrawFeeList[] WithdrawFeeList { get; set; }

        [JsonPropertyName("hyperpayWithdrawMin")]
        public string? HyperpayWithdrawMin { get; set; }

        [JsonPropertyName("hyperpayWithdrawMax")]
        public string? HyperpayWithdrawMax { get; set; }

        [JsonPropertyName("minimumService")]
        public string? MinimumService { get; set; }

        [JsonPropertyName("withdraw24LimitQty")]
        public string? Withdraw24LimitQty { get; set; }

        [JsonPropertyName("withdraw24LimitRemaining")]
        public string? Withdraw24LimitRemaining { get; set; }

        [JsonPropertyName("otcTag")]
        public int? OtcTag { get; set; }

        [JsonPropertyName("lockCoin")]
        public object? LockCoin { get; set; }

        [JsonPropertyName("tradeMappingId")]
        public int? TradeMappingId { get; set; }

        [JsonPropertyName("isEtf")]
        public int? IsEtf { get; set; }

        [JsonPropertyName("supportTransfer")]
        public bool? SupportTransfer { get; set; }

        [JsonPropertyName("internalMax")]
        public string? InternalMax { get; set; }

        [JsonPropertyName("internalMin")]
        public string? InternalMin { get; set; }
    }
    public class CoinwWalletCoinMemoChainName
    {
        [JsonPropertyName("chainName")]
        public required string ChainName { get; set; }

        [JsonPropertyName("chainNameAlias")]
        public string? ChainNameAlias { get; set; }

        [JsonPropertyName("memoTag")]
        public string? MemoTag { get; set; }

        [JsonPropertyName("useMemoTag")]
        public int UseMemoTag { get; set; }

        [JsonPropertyName("memoRequired")]
        public int MemoRequired { get; set; }

        [JsonPropertyName("withdrawStatus")]
        public int WithdrawStatus { get; set; }

        [JsonPropertyName("rechargeStatus")]
        public int RechargeStatus { get; set; }
    }
    public class CoinwWalletCoinWithdrawCountLimitList
    {
        [JsonPropertyName("chainName")]
        public required string ChainName { get; set; }

        [JsonPropertyName("minWithdraw")]
        public string? MinWithdraw { get; set; }

        [JsonPropertyName("maxWithdraw")]
        public string? MaxWithdraw { get; set; }
    }
    public class CoinwWalletCoinWithdrawFeeList
    {
        [JsonPropertyName("fee")]
        public string? Fee { get; set; }

        [JsonPropertyName("fixedFee")]
        public string? FixedFee { get; set; }

        [JsonPropertyName("chainName")]
        public required string ChainName { get; set; }

        [JsonPropertyName("chainNameAlias")]
        public string? ChainNameAlias { get; set; }

        [JsonPropertyName("retePrice")]
        public string? RetePrice { get; set; }

        [JsonPropertyName("symbolLogo")]
        public string? SymbolLogo { get; set; }
    }


    public class CoinwPrices
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, CoinwPricesData>? Data { get; set; }

        [JsonPropertyName("failed")]
        public bool Failed { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
    public class CoinwPricesData
    {
        [JsonPropertyName("percentChange")]
        public string? PercentChange { get; set; }

        [JsonPropertyName("high24hr")]
        public string? High24hr { get; set; }

        [JsonPropertyName("last")]
        public string? Last { get; set; }

        [JsonPropertyName("highestBid")]
        public required string HighestBid { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("isFrozen")]
        public int IsFrozen { get; set; }

        [JsonPropertyName("baseVolume")]
        public string? BaseVolume { get; set; }

        [JsonPropertyName("lowestAsk")]
        public required string LowestAsk { get; set; }

        [JsonPropertyName("low24hr")]
        public string? Low24hr { get; set; }
    }


    public class CoinwOrderBook
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("data")]
        public CoinwOrderBookData? Data { get; set; }

        [JsonPropertyName("failed")]
        public bool Failed { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
    public class CoinwOrderBookData
    {
        [JsonPropertyName("asks")]
        public string[][]? Asks { get; set; }

        [JsonPropertyName("bids")]
        public string[][]? Bids { get; set; }
    }
}
