using System.Net;

namespace CryptoOculus.Services
{
    public class DnsUpdateService(ILogger<CexCompareService> logger)
    {
        public async Task UpdateService(IDnsUpdate[] dnsUpdates, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UpdateAsync(dnsUpdates, cancellationToken);

                await Task.Delay(3000, cancellationToken);
            }
        }

        public async Task UpdateAsync(IDnsUpdate[] dnsUpdates, CancellationToken cancellationToken)
        {
            async Task DnsUpdateTask(IDnsUpdate dnsUpdate)
            {
                List<Task<string>> hostTasks = [];

                foreach (string host in dnsUpdate.Hosts)
                {
                    hostTasks.Add(ResolveIpAsync(host, cancellationToken));
                }

                dnsUpdate.Ips = await Task.WhenAll(hostTasks);
            }

            List<Task> tasks = [];

            foreach (IDnsUpdate dnsUpdate in dnsUpdates)
            {
                tasks.Add(DnsUpdateTask(dnsUpdate));
            }

            await Task.WhenAll(tasks);
        }

        private async Task<string> ResolveIpAsync(string host, CancellationToken cancellationToken)
        {
            try
            {
                return (await Dns.GetHostAddressesAsync(host, System.Net.Sockets.AddressFamily.InterNetwork, cancellationToken)).FirstOrDefault()!.ToString();
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "{host} resolve error!", host);
                return "0.0.0.0";
            }
        }
    }
}
