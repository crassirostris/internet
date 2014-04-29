using System;
using System.Net;

namespace TracertAs.Whois
{
    internal class AddressInformation
    {
        public static AddressInformation GetUnknownAddressInformation(IPAddress unknownAddress)
        {
            return new AddressInformation { Address = unknownAddress, Unkown = true };
        }
        private bool Unkown { get; set; }
        public IPAddress Address { get; set; }
        public string Organization { get; set; }
        public string Country { get; set; }
        public string Asn { get; set; }
        public string NetworkName { get; set; }

        public override string ToString()
        {
            if (Unkown)
                return string.Format("For address {0} no information. Unknown or private use", Address);
            var title = String.Format("For address {0}", Address);
            var countiry = String.Format("\t | Country: {0}", Country);
            var organization = String.Format("\t | Organization: {0}", Organization);
            var asn = String.Format("\t | Autonomic System Number: {0}", Asn);
            var networkName = String.Format("\t | Network Name: {0}", NetworkName);
            return String.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}", Environment.NewLine, title, countiry, organization, asn, networkName);
        }
    }
}