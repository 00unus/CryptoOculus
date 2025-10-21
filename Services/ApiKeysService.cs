using CryptoOculus.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CryptoOculus.Services
{
    public class ApiKeysService(IOptionsMonitor<ApiKeysOptions> apiKeysOptions, IWebHostEnvironment env)
    {
        private static readonly Lock _lock = new();

        public string GetSingle(string keyName)
        {
            return apiKeysOptions.CurrentValue.SingleKeys.TryGetValue(keyName, out var key) ? key : throw new KeyNotFoundException("Key not found!");
        }

        public string[] GetMultiple(string keyName)
        {
            return apiKeysOptions.CurrentValue.MultipleKeys.TryGetValue(keyName, out var keys) ? keys : throw new KeyNotFoundException("Keys not found!");
        }
        
        public void SetSingle(string keyName, string value)
        {
            lock (_lock)
            {
                ApiKeysOptions newApiKeysOptions = new()
                {
                    SingleKeys = new(apiKeysOptions.CurrentValue.SingleKeys),
                    MultipleKeys = new(apiKeysOptions.CurrentValue.MultipleKeys)
                };

                newApiKeysOptions.SingleKeys[keyName] = value;

                File.WriteAllText(Path.Combine(env.ContentRootPath, "Data/apiKeys.json"), JsonSerializer.Serialize(new { ApiKeys = newApiKeysOptions }, Helper.serializeOptions));
            }
        }

        public void SetMultiple(string keyName, string[] value)
        {
            lock (_lock)
            {
                ApiKeysOptions newApiKeysOptions = new()
                {
                    SingleKeys = new(apiKeysOptions.CurrentValue.SingleKeys),
                    MultipleKeys = new(apiKeysOptions.CurrentValue.MultipleKeys)
                };

                newApiKeysOptions.MultipleKeys[keyName] = value;

                File.WriteAllText(Path.Combine(env.ContentRootPath, "Data/apiKeys.json"), JsonSerializer.Serialize(new { ApiKeys = newApiKeysOptions }, Helper.serializeOptions));
            }
        }
    }
}