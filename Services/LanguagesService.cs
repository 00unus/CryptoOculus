using CryptoOculus.Models;
using Microsoft.Extensions.Options;

namespace CryptoOculus.Services
{
    public class LanguagesService(IOptionsMonitor<Languages> languages)
    {
        public string GetString(string languageCode, string key)
        {
            if (languages.CurrentValue.TryGetValue(languageCode.ToLower(), out Dictionary<string, string>? language))
            {
                return language.TryGetValue(key, out string? languageString) ? languageString : throw new Exception($"{key} not found for \"{languageCode}\" language");
            }

            else
            {
                languages.CurrentValue.TryGetValue("en", out Dictionary<string, string>? english);

                return english!.TryGetValue(key, out string? languageString) ? languageString : throw new Exception($"{key} not found for \"en\" language");
            }
        }

        public string[] GetLanguageCodes()
        {
            return [.. languages.CurrentValue.Keys.ToArray()];
        }
    }
}