using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TracertAs.Whois
{
    internal abstract class RirClient
    {
        private const int WhoisPort = 43;
        private const int SocketTimeout = 1000;
        public abstract string RirName { get; }
        protected abstract string[] GeneralRecordsFeatures { get; }

        public EndPoint RirEndPoint
        {
            get
            {
                var ip = Dns.GetHostAddresses(String.Format("whois.{0}.net", RirName)).First(e => e.AddressFamily == AddressFamily.InterNetwork);
                return new IPEndPoint(ip, WhoisPort);
            }
        }

        public String GetRecordForAddress(IPAddress address)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.ReceiveTimeout = SocketTimeout;
                socket.SendTimeout = SocketTimeout;
                socket.Connect(RirEndPoint);
                socket.Send(Encoding.ASCII.GetBytes(FormWhoisQuery(address)));
                var response = Encoding.ASCII.GetString(socket.RecieveAllBytes());
                return response;
            }
        }

        public abstract string FormWhoisQuery(IPAddress address);

        public AddressInformation GetAddressInformation(IPAddress address)
        {
            var record = GetRecordForAddress(address);
            return IsGeneralRecord(record) ? null : ExtractInformation(address, record);
        }

        protected string LastExtractedOrDefault(IEnumerable<string> lines, string prefix)
        {
            var result = lines.LastOrDefault(e => e.StartsWith(prefix));
            if (result != null)
            {
                var components = result.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                result = components.Length > 1 ? components[1].Trim() : "";
            }
            return result;
        }

        protected bool IsGeneralRecord(string record)
        {
            return GeneralRecordsFeatures.Any(record.Contains);
        }

        protected abstract AddressInformation ExtractInformation(IPAddress address, string record);
    }
}
