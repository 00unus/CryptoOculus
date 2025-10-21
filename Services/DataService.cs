using CryptoOculus.Models;
using Microsoft.Extensions.Options;

namespace CryptoOculus.Services
{
    public class DataService(IOptionsMonitor<List<NetworkList>> networkLists)
    {
        public int? GetNetworkId(string networkName)
        {
            foreach (NetworkList networkList in networkLists.CurrentValue)
            {
                foreach (string network in networkList.CEXs)
                {
                    if (network.Equals(networkName.Replace(" ", ""), StringComparison.CurrentCultureIgnoreCase))
                    {
                        return networkList.Id;
                    }
                }
            }

            return null;
        }

        public string GetNetworkName(int networkId)
        {
            foreach (NetworkList networkList in networkLists.CurrentValue)
            {
                if (networkList.Id == networkId)
                {
                    return networkList.NetworkName;
                }
            }

            throw new("ERROR! NETWORK ID NOT FOUND!");
        }
    }
}