using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class HoneypotIsV2
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
        [JsonPropertyName("token")]
        public HoneypotIsV2Token? Token { get; set; }

        [JsonPropertyName("withToken")]
        public HoneypotIsV2WithToken? WithToken { get; set; }

        [JsonPropertyName("summary")]
        public HoneypotIsV2Summary? Summary { get; set; }

        [JsonPropertyName("simulationSuccess")]
        public bool SimulationSuccess { get; set; }

        [JsonPropertyName("honeypotResult")]
        public HoneypotIsV2HoneypotResult? HoneypotResult { get; set; }

        [JsonPropertyName("simulationResult")]
        public HoneypotIsV2SimulationResult? SimulationResult { get; set; }

        [JsonPropertyName("holderAnalysis")]
        public HoneypotIsV2HolderAnalysis? HolderAnalysis { get; set; }

        [JsonPropertyName("flags")]
        public string[]? Flags { get; set; }

        [JsonPropertyName("contractCode")]
        public HoneypotIsV2ContractCode? ContractCode { get; set; }

        [JsonPropertyName("chain")]
        public HoneypotIsV2Chain? Chain { get; set; }

        [JsonPropertyName("router")]
        public string? Router { get; set; }

        [JsonPropertyName("pair")]
        public HoneypotIsV2Pair? Pair { get; set; }

        [JsonPropertyName("pairAddress")]
        public string? PairAddress { get; set; }
    }

    public class HoneypotIsV2Chain
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("shortName")]
        public string? ShortName { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
    }

    public class HoneypotIsV2ContractCode
    {
        [JsonPropertyName("openSource")]
        public bool OpenSource { get; set; }

        [JsonPropertyName("rootOpenSource")]
        public bool RootOpenSource { get; set; }

        [JsonPropertyName("isProxy")]
        public bool IsProxy { get; set; }

        [JsonPropertyName("hasProxyCalls")]
        public bool HasProxyCalls { get; set; }
    }

    public class HoneypotIsV2Flag
    {
        [JsonPropertyName("flag")]
        public string? Flag { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("severity")]
        public string? Severity { get; set; }

        [JsonPropertyName("severityIndex")]
        public double SeverityIndex { get; set; }
    }

    public class HoneypotIsV2HolderAnalysis
    {
        [JsonPropertyName("holders")]
        public string? Holders { get; set; }

        [JsonPropertyName("successful")]
        public string? Successful { get; set; }

        [JsonPropertyName("failed")]
        public string? Failed { get; set; }

        [JsonPropertyName("siphoned")]
        public string? Siphoned { get; set; }

        [JsonPropertyName("averageTax")]
        public double AverageTax { get; set; }

        [JsonPropertyName("averageGas")]
        public double AverageGas { get; set; }

        [JsonPropertyName("highestTax")]
        public double HighestTax { get; set; }

        [JsonPropertyName("highTaxWallets")]
        public string? HighTaxWallets { get; set; }

        [JsonPropertyName("taxDistribution")]
        public object[]? TaxDistribution { get; set; }

        [JsonPropertyName("snipersFailed")]
        public double SnipersFailed { get; set; }

        [JsonPropertyName("snipersSuccess")]
        public double SnipersSuccess { get; set; }
    }

    public class HoneypotIsV2HoneypotResult
    {
        [JsonPropertyName("isHoneypot")]
        public bool IsHoneypot { get; set; }

        [JsonPropertyName("honeypotReason")]
        public string? HoneypotReason { get; set; }
    }

    public class HoneypotIsV2Pair
    {
        [JsonPropertyName("pair")]
        public HoneypotIsV2Pair? Pair { get; set; }

        [JsonPropertyName("chainId")]
        public string? ChainId { get; set; }

        [JsonPropertyName("reserves0")]
        public string? Reserves0 { get; set; }

        [JsonPropertyName("reserves1")]
        public string? Reserves1 { get; set; }

        [JsonPropertyName("liquidity")]
        public double Liquidity { get; set; }

        [JsonPropertyName("router")]
        public string? Router { get; set; }

        [JsonPropertyName("createdAtTimestamp")]
        public string? CreatedAtTimestamp { get; set; }

        [JsonPropertyName("creationTxHash")]
        public string? CreationTxHash { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("token0")]
        public string? Token0 { get; set; }

        [JsonPropertyName("token1")]
        public string? Token1 { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class HoneypotIsV2SimulationResult
    {
        [JsonPropertyName("buyTax")]
        public double BuyTax { get; set; }

        [JsonPropertyName("sellTax")]
        public double SellTax { get; set; }

        [JsonPropertyName("transferTax")]
        public double TransferTax { get; set; }

        [JsonPropertyName("buyGas")]
        public string? BuyGas { get; set; }

        [JsonPropertyName("sellGas")]
        public string? SellGas { get; set; }
    }

    public class HoneypotIsV2Summary
    {
        [JsonPropertyName("risk")]
        public string? Risk { get; set; }

        [JsonPropertyName("riskLevel")]
        public double RiskLevel { get; set; }

        [JsonPropertyName("flags")]
        public HoneypotIsV2Flag[]? Flags { get; set; }
    }

    public class HoneypotIsV2Token
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("decimals")]
        public double Decimals { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("totalHolders")]
        public double TotalHolders { get; set; }
    }

    public class HoneypotIsV2WithToken
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("decimals")]
        public double Decimals { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("totalHolders")]
        public double TotalHolders { get; set; }
    }
}
