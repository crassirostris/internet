using System;
using System.Linq;
using System.Net;

namespace TracertAs.Whois
{
    internal class ArinRirClient : RirClient
    {
        private static readonly string[] generalRecordsFeatures =
        {
            "Allocated to RIPE NCC",
            "Transferred to AfriNIC",
            "Transferred to APNIC",
            "Transferred to LACNIC",
            "IANA Special Use"
        };

        public override string RirName
        {
            get { return "arin"; }
        }

        protected override string[] GeneralRecordsFeatures
        {
            get { return generalRecordsFeatures; }
        }

        public override string FormWhoisQuery(IPAddress address)
        {
            return String.Format("n + {0}\r\n", address);
        }

        protected override AddressInformation ExtractInformation(IPAddress address, string record)
        {
            var lines = record.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new AddressInformation
            {
                Address = address,
                Country = LastExtractedOrDefault(lines, "Country: "),
                Organization = LastExtractedOrDefault(lines, "OrgId: "),
                Asn = LastExtractedOrDefault(lines, "OriginAS: "),
                NetworkName = LastExtractedOrDefault(lines, "NetName: ")
            };
            return result;
        }
    }
}