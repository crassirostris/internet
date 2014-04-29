using System.Linq;
using System.Net;

namespace TracertAs.Whois
{
    internal static class WhoisHelper
    {
        private static readonly RirClient[] clients =
        {
            new RipeRirClient(),
            new AfrinicRirClient(),
            new ArinRirClient(),
            new ApnicRirClient(),
            new LacnicRirCleint()
        };

        public static AddressInformation GetIpInformation(IPAddress address)
        {
            var result = clients
                .Select(rirClient => rirClient.GetAddressInformation(address))
                .FirstOrDefault(addrInfo => addrInfo != null);
            return result ?? AddressInformation.GetUnknownAddressInformation(address);
        }
    }
}