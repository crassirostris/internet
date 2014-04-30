using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace PortScan
{
    internal class DnsChecker : PortChecker
    {
        private static readonly byte[] dnsQueryPacket = Enumerable.Empty<byte>()
            .Concat(BitConverter.GetBytes((ushort)(42423)).Reverse())
            .Concat(BitConverter.GetBytes((ushort)(0x0100)).Reverse())
            .Concat(BitConverter.GetBytes((ushort)(1)).Reverse())
            .Concat(BitConverter.GetBytes((ushort)(0)).Reverse())
            .Concat(BitConverter.GetBytes((ushort)(0)).Reverse())
            .Concat(BitConverter.GetBytes((ushort)(0)).Reverse())
            .Concat(new byte[] { 4 })
            .Concat(BitConverter.GetBytes((ulong)(0x3967616702747600)).Reverse())
            .Concat(BitConverter.GetBytes((ushort)(1)).Reverse())
            .Concat(BitConverter.GetBytes((ushort)(1)).Reverse())
            .ToArray();

        private const int MaxPacketSize = 512;

        public override void Check(PortStatus status)
        {
            if (status.TransportLayerProtocols.Contains(TransportLayerProtocol.Tcp))
                CheckTcp(status);
            CheckUdp(status);
        }

        private void CheckUdp(PortStatus status)
        {
            if (CheckSocket(status, () => CreateUdpSocket(status)))
            {
                status.TransportLayerProtocols.Add(TransportLayerProtocol.Udp);
                status.ApplicationLevelProtocols.Add(ApplicationLevelProtocol.Dns);
            }
        }

        private void CheckTcp(PortStatus status)
        {
            if (CheckSocket(status, () => CreateTcpSocket(status)))
                status.ApplicationLevelProtocols.Add(ApplicationLevelProtocol.Dns);
        }

        private bool CheckSocket(PortStatus status, Func<Socket> socketInitializator)
        {
            try
            {
                using (var socket = socketInitializator())
                {
                    socket.Send(dnsQueryPacket);
                    var buffer = new byte[MaxPacketSize];
                    var recieved = socket.Receive(buffer);
                    if (CorrectDnsResponse(buffer.Take(recieved).ToArray()))
                        return true;
                }
            }
            catch { }
            return false;
        }

        private bool CorrectDnsResponse(byte[] buffer)
        {
            return buffer.Take(2).SequenceEqual(dnsQueryPacket.Take(2));
        }
    }
}