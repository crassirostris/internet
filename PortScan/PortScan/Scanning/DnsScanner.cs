using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using PortScan.Common;

namespace PortScan.Scanning
{
    internal class DnsScanner : ApplicationLayerPortScanner
    {
        private static readonly PortId[] portIds =
        {
            new PortId(TransportProtocol.Tcp, 53),
            new PortId(TransportProtocol.Udp, 53),
        };

        private static readonly byte[] queryPacket =
        {
            0x57, 0x8e, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x70, 0x6f, 0x72,
            0x6e, 0x74, 0x75, 0x62, 0x65, 0x03, 0x63, 0x6f, 0x6d, 0x00, 0x00, 0x01, 0x00, 0x01,
        };

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
            get { return ApplicationProtocol.Dns; }
        }
        public override void Scan(IPAddress address, int scanFrom, int scanTo, PortScanningStatus portScanningStatus, Dictionary<PortId, ApplicationProtocol> detectedProtocols)
        {
            foreach (var portId in PortIds)
            {
                if (portScanningStatus.ClosedPorts.Contains(portId) || portId.PortNumber < scanFrom || portId.PortNumber > scanTo)
                    continue;
                try
                {
                    var socket = new Socket(AddressFamily.InterNetwork,
                        portId.TransportProtocol == TransportProtocol.Tcp ? SocketType.Stream : SocketType.Dgram,
                        portId.TransportProtocol == TransportProtocol.Tcp ? ProtocolType.Tcp : ProtocolType.Udp)
                    {
                        ReceiveTimeout = Timeout,
                        SendTimeout = Timeout
                    };
                    var remoteEp = new IPEndPoint(address, portId.PortNumber);
                    socket.Connect(remoteEp);
                    var query = portId.TransportProtocol == TransportProtocol.Udp
                        ? queryPacket
                        : BitConverter.GetBytes((ushort) queryPacket.Length).Reverse().Concat(queryPacket).ToArray();
                    socket.Send(query, query.Length, SocketFlags.None);
                    var responseBuffer = new byte[ResponseSize];
                    if (socket.Receive(responseBuffer, ResponseSize, SocketFlags.None) > 0)
                        detectedProtocols[portId] = Protocol;
                }
                catch
                {
                }
            }
        }
    }
}