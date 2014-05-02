using System;
using System.Net;

namespace TracertAs.Whois
{
    internal class RipeRirClient : RirClient
    {
        private static readonly string[] generalRecordsFeatures =
        {
            "inetnum:      0.0.0.0 - 255.255.255.255",
            "inetnum:        0.0.0.0 - 255.255.255.255",
            "address space for private internets",
            "not managed by the RIPE NCC"
        };

        public override string RirName
        {
            get { return "ripe"; }
        }

        protected override string[] GeneralRecordsFeatures
        {
            get { return generalRecordsFeatures; }
        }

        public override string FormWhoisQuery(IPAddress address)
        {
            return address + "\r\n";
        }

        protected override AddressInformation ExtractInformation(IPAddress address, string record)
        {
            var lines = record.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new AddressInformation
            {
                Address = address,
                Country = LastExtractedOrDefault(lines, "country: "),
                Organization = LastExtractedOrDefault(lines, "admin-c: "),
                Asn = LastExtractedOrDefault(lines, "origin: "),
                NetworkName = LastExtractedOrDefault(lines, "netname: ")
                
            };
            return result;
        }
    }
}