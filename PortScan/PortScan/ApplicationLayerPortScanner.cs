using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PortScan
{
    internal abstract class ApplicationLayerPortScanner : IApplicationLayerPortScanner
    {
        public const int Timeout = 1000;
        public const int DefaultResponseSize = 4096;
        public abstract PortId[] PortIds { get; }
        public abstract byte[] QueryPacket { get; }
        public abstract ApplicationProtocol Protocol { get; }
        public virtual int ResponseSize { get { return DefaultResponseSize; } }

        public virtual void Scan(IPAddress address, int scanFrom, int scanTo, PortScanningStatus portScanningStatus, Dictionary<PortId, ApplicationProtocol> detectedProtocols)
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
                    socket.Send(QueryPacket, QueryPacket.Length, SocketFlags.None);
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