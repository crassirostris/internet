using System.Collections.Generic;
using System.Configuration;

namespace DnsCache.Helpers
{
    internal static class ConfigHelper
    {
        public const int DefaultDnsPort = 53;

        public static int GetInt(string key, int defaultValue)
        {
            var str = ConfigurationManager.AppSettings.Get(key);
            int result;
            return int.TryParse(str, out result) ? result : defaultValue;
        }

        public static IEnumerable<string> GetValues(string key)
        {
            return ConfigurationManager.AppSettings.GetValues(key);
        }
    }
}
