using CryptoOculus.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class HoneypotIsService(IOptionsMonitor<List<TokenTransferTax>> tokenTransferTaxes, IOptionsMonitor<List<CheckedTokens>> checkedTokens, IHttpClientFactory httpClientFactory, ILogger<HoneypotIsService> logger, IWebHostEnvironment env) : IDnsUpdate
    {
        public string[] Hosts { get; } = ["api.honeypot.is"];
        public string[] Ips { get; set; } = [];

        private static readonly Lock locker = new();

        private void ValidateHoneypotIsV2(HttpRequestMessage request)
        {
            _ = ClientService.Deserialize<HoneypotIsV2>(request);
            //if (honeypotIsV2 is not null && honeypotIsV2.Error is not null && (honeypotIsV2.Error == "Invalid Address" || honeypotIsV2.Error == "No pairs found" || honeypotIsV2.Error == "Token not found"))
        }

        public async Task<double?> GetTransferTax(string contractAddress)
        {
            foreach (TokenTransferTax tokenTransferTax in tokenTransferTaxes.CurrentValue)
            {
                if (tokenTransferTax.ContractAddress.Equals(contractAddress, StringComparison.CurrentCultureIgnoreCase))
                {
                    return tokenTransferTax.TransferFee;
                }
            }

            foreach (CheckedTokens checkedToken in checkedTokens.CurrentValue)
            {
                if (checkedToken.ContractAddress.Equals(contractAddress, StringComparison.CurrentCultureIgnoreCase))
                {
                    return null;
                }
            }

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://{Ips[0]}/v2/IsHoneypot?address={contractAddress.ToUpper()}").WithVersion();
                request.Headers.Host = Hosts[0];
                request.Options.Set(HttpOptionKeys.ValidationDelegate, ValidateHoneypotIsV2);
                string clientName = ClientService.RotatingProxyClient("honeypotis");
                request.Options.Set(HttpOptionKeys.ClientName, clientName);
                HttpResponseMessage response = await httpClientFactory.CreateClient(clientName).SendAsync(request);

                HoneypotIsV2 honeypotIsV2 = JsonSerializer.Deserialize<HoneypotIsV2>(await response.Content.ReadAsStringAsync(), Helper.deserializeOptions)!;

                if (honeypotIsV2.SimulationSuccess && honeypotIsV2.SimulationResult is not null)
                {
                    lock (locker)
                    {
                        List<TokenTransferTax> tempTokenTransferTaxes = [.. tokenTransferTaxes.CurrentValue];
                        tempTokenTransferTaxes.Add(new() { ContractAddress = contractAddress.ToUpper(), TransferFee = honeypotIsV2.SimulationResult.TransferTax });

                        File.WriteAllText(Path.Combine(env.ContentRootPath, "Data/tokenTransferTaxes.json"), JsonSerializer.Serialize(new { TokenTransferTaxes = tempTokenTransferTaxes }, Helper.serializeOptions));
                    }

                    return honeypotIsV2.SimulationResult.TransferTax;
                }

                lock (locker)
                {
                    List<CheckedTokens> tempCheckedTokens = [.. checkedTokens.CurrentValue];
                    tempCheckedTokens.Add(new() { ContractAddress = contractAddress.ToUpper() });

                    File.WriteAllText(Path.Combine(env.ContentRootPath, "Cache/honeypotIsCheckedTokens.json"), JsonSerializer.Serialize(new { CheckedTokens = tempCheckedTokens }, Helper.serializeOptions));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Honeypot check failed!");
            }

            return null;
        }
    }
}
