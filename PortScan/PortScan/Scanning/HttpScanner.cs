using System.Text;
using PortScan.Common;

namespace PortScan.Scanning
{
    internal class HttpScanner : ApplicationLayerPortScanner
    {
        private static readonly PortId[] portIds =
        {
            new PortId(TransportProtocol.Tcp, 80),
            new PortId(TransportProtocol.Tcp, 8080),
            new PortId(TransportProtocol.Tcp, 8000),
        };

        private static readonly byte[] queryPacket = Encoding.ASCII.GetBytes("GET / HTTP/1.0\r\n\r\n");

        public override PortId[] PortIds
        {
            get { return portIds; }
        }

        public override byte[] QueryPacket
        {
            get { return queryPacket; }
        }

        public override ApplicationProtocol Protocol
        {
            get { return ApplicationProtocol.Http; }
        }
    }
}