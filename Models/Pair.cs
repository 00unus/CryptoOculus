using System.Diagnostics.CodeAnalysis;

namespace CryptoOculus.Models
{
    public class Pair
    {
        public required int ExchangeId { get; set; }
        public required string ExchangeName { get; set; }

        public required string BaseAsset { get; set; }
        public required string QuoteAsset { get; set; }

        public double AskPrice { get; set; }
        public double AskQuantity { get; set; }
        public double AskAmount { get; set; }
        public double[]? AskPriceRange { get; set; }
        public double[][]? Asks { get; set; }

        public double BidPrice { get; set; }
        public double BidQuantity { get; set; }
        public double BidAmount { get; set; }
        public double[]? BidPriceRange { get; set; }
        public double[][]? Bids { get; set; }

        public required string Url { get; set; }
        public double? SpotTakerComission { get; set; }
        public AssetNetwork[] BaseAssetNetworks { get; set; } = [];

        public Pair() { }

        [SetsRequiredMembers]
        public Pair(Pair pair)
        {
            ExchangeId = pair.ExchangeId;
            ExchangeName = pair.ExchangeName;

            BaseAsset = pair.BaseAsset;
            QuoteAsset = pair.QuoteAsset;

            AskPrice = pair.AskPrice;
            AskQuantity = pair.AskQuantity;
            AskAmount = pair.AskAmount;
            AskPriceRange = pair.AskPriceRange?.ToArray();
            Asks = pair.Asks?.Select(innerArray => innerArray.ToArray()).ToArray();

            BidPrice = pair.BidPrice;
            BidQuantity = pair.BidQuantity;
            BidAmount = pair.BidAmount;
            BidPriceRange = pair.BidPriceRange?.ToArray();
            Bids = pair.Bids?.Select(innerArray => innerArray.ToArray()).ToArray();

            Url = pair.Url;
            SpotTakerComission = pair.SpotTakerComission;
            BaseAssetNetworks = pair.BaseAssetNetworks?.Select(network => new AssetNetwork(network)).ToArray() ?? [];
        }
    }
    public class AssetNetwork
    {
        public int? NetworkId { get; set; }
        public required string NetworkName { get; set; }
        public string? Address { get; set; }
        public bool DepositEnable { get; set; }
        public bool WithdrawEnable { get; set; }
        public double? TransferTax { get; set; }
        public double? WithdrawFee { get; set; }
        public double? DepositTax { get; set; }
        public required string DepositUrl { get; set; }
        public required string WithdrawUrl { get; set; }

        public AssetNetwork() { }

        [SetsRequiredMembers]
        public AssetNetwork(AssetNetwork assetNetwork)
        {
            NetworkId = assetNetwork.NetworkId;
            NetworkName = assetNetwork.NetworkName;
            Address = assetNetwork.Address;
            DepositEnable = assetNetwork.DepositEnable;
            WithdrawEnable = assetNetwork.WithdrawEnable;
            TransferTax = assetNetwork.TransferTax;
            WithdrawFee = assetNetwork.WithdrawFee;
            DepositTax = assetNetwork.DepositTax;
            DepositUrl = assetNetwork.DepositUrl;
            WithdrawUrl = assetNetwork.WithdrawUrl;
        }
    }
}