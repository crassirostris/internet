using System.Configuration;

namespace PortScan.Helpers
{
    internal static class ConfigHelper
    {
        public static int GetIntFromConfig(string key, int defaultValue)
        {
            int value;
            var strValue = ConfigurationManager.AppSettings[key];
            return int.TryParse(strValue, out value) ? value : defaultValue;
        }
    }
}
