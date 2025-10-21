namespace CryptoOculus.Models
{
    public class XtcomExchangeInfo
    {
        public int Rc { get; set; }
        public string? Mc { get; set; }
        public object[]? Ma { get; set; }
        public XtcomResult? Result { get; set; }
    }
    public class XtcomResult
    {
        public long Time { get; set; }
        public string? Version { get; set; }
        public required XtcomSymbol[] Symbols { get; set; }
    }
    public class XtcomSymbol
    {
        public int Id { get; set; }
        public string? Symbol { get; set; }
        public string? DisplayName { get; set; }
        public string? Type { get; set; }
        public required string State { get; set; }
        public object? StateTime { get; set; }
        public bool TradingEnabled { get; set; }
        public bool OpenapiEnabled { get; set; }
        public object? NextStateTime { get; set; }
        public object? NextState { get; set; }
        public int DepthMergePrecision { get; set; }
        public required string BaseCurrency { get; set; }
        public int BaseCurrencyPrecision { get; set; }
        public int BaseCurrencyId { get; set; }
        public string? BaseCurrencyLogo { get; set; }
        public required string QuoteCurrency { get; set; }
        public int QuoteCurrencyPrecision { get; set; }
        public int QuoteCurrencyId { get; set; }
        public int PricePrecision { get; set; }
        public int QuantityPrecision { get; set; }
        public string[]? OrderTypes { get; set; }
        public string[]? TimeInForces { get; set; }
        public int DisplayWeight { get; set; }
        public string? DisplayLevel { get; set; }
        public int[]? Plates { get; set; }
        public object[]? Filters { get; set; }
    }


    public class XtcomContractAddresses
    {
        public int Rc { get; set; }
        public string? Mc { get; set; }
        public object[]? Ma { get; set; }
        public XtcomContractResult[]? Result { get; set; }
    }
    public class XtcomContractResult
    {
        public required string Currency { get; set; }
        public required XtcomContractSupportChain[] SupportChains { get; set; }
    }
    public class XtcomContractSupportChain
    {
        public required string Chain { get; set; }
        public bool DepositEnabled { get; set; }
        public bool WithdrawEnabled { get; set; }
        public double WithdrawFeeAmount { get; set; }
        public required string WithdrawFeeCurrency { get; set; }
        public int WithdrawFeeCurrencyId { get; set; }
        public double WithdrawMinAmount { get; set; }
        public int DepositFeeRate { get; set; }
        public required string Contract { get; set; }
    }


    public class XtcomPrices
    {
        public int Rc { get; set; }
        public string? Mc { get; set; }
        public object[]? Ma { get; set; }
        public XtcomPricesResult[]? Result { get; set; }
    }
    public class XtcomPricesResult
    {
        public required string S { get; set; }
        public object? T { get; set; }
        public string? Cv { get; set; }
        public string? Cr { get; set; }
        public string? O { get; set; }
        public string? L { get; set; }
        public string? H { get; set; }
        public string? C { get; set; }
        public string? Q { get; set; }
        public string? V { get; set; }
        public required string Ap { get; set; }
        public string? Aq { get; set; }
        public required string Bp { get; set; }
        public string? Bq { get; set; }
    }


    public class XtcomOrderBook
    {
        public int Rc { get; set; }
        public string? Mc { get; set; }
        public object[]? Ma { get; set; }
        public XtcomOrderBookResult? Result { get; set; }
    }
    public class XtcomOrderBookResult
    {
        public object? Timestamp { get; set; }
        public long LastUpdateId { get; set; }
        public string[][]? Bids { get; set; }
        public string[][]? Asks { get; set; }
    }
}
