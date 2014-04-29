using System;
using System.Net;

namespace TracertAs.Whois
{
    internal class LacnicRirCleint : RirClient
    {
        private static readonly string[] generalRecordsFeatures =
        {
            "0.0.0.0 - 255.255.255.255",
            "whois.afrinic.net",
            "whois.ripe.net",
            "whois.arin.net",
            "whois.apnic.net",
            "Reserved:"
        };

        public override string RirName
        {
            get { return "lacnic"; }
        }

        protected override string[] GeneralRecordsFeatures
        {
            get { return generalRecordsFeatures; }
        }

        public override string FormWhoisQuery(IPAddress address)
        {
            return address.ToString() + "\r\n";
        }

        protected override AddressInformation ExtractInformation(IPAddress address, string record)
        {
            var lines = record.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new AddressInformation
            {
                Address = address,
                Country = LastExtractedOrDefault(lines, "country: "),
                Organization = LastExtractedOrDefault(lines, "owner-c: "),
                Asn = LastExtractedOrDefault(lines, "aut-num: "),
                NetworkName = null
            };
            return result;
        }
    }
}