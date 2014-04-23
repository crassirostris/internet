using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Timers;

namespace SntpServer
{
    public class SntpUdpServer : UdpServer
    {
        private readonly decimal timestampDelta;
        private readonly List<IPEndPoint> references;
        private const decimal StartTimeOffset = (-1) * (decimal)(59926629600);
        private const int DefaultPort = 123;

        public SntpUdpServer(int port, decimal timestampDelta = 0, List<IPEndPoint> references = null) : base(port)
        {
            this.timestampDelta = timestampDelta;
            this.references = (references ?? new List<IPEndPoint>())
                .Concat((ConfigurationManager.AppSettings.GetValues("referenceClock") ?? new string[0])
                    .Select(
                        e =>
                            new IPEndPoint(
                                Dns.GetHostAddresses(e).First(addr => addr.AddressFamily == AddressFamily.InterNetwork),
                                DefaultPort))).ToList();
        }

        private SntpPacket FormResponsePacket(SntpPacket request)
        {
            var message = request.ToBytes();
            foreach (var referenceEndPoint in references)
            {
                using (var udpClient = new UdpClient())
                {
                    udpClient.Connect(referenceEndPoint);
                    udpClient.Send(message, message.Length);
                    IPEndPoint remoteEp = null;
                    var referenceResponseMessage = udpClient.Receive(ref remoteEp);
                    if (referenceResponseMessage.Length < SntpPacket.MinimumPacketLength) 
                        continue;
                    var referenceResponse = SntpPacket.FromBytes(referenceResponseMessage);
                    return ResponseFromReference(request, referenceResponse);
                }
            }
            return FormResponsePacketLocally(request);
        }

        private decimal GetClock()
        {
            return (decimal) (DateTime.Now.Ticks) / 10000000 + timestampDelta + StartTimeOffset;
        }

        private SntpPacket ResponseFromReference(SntpPacket request, SntpPacket referenceResponse)
        {
            var response = referenceResponse;
            response.Version = request.Version;
            response.Mode = Mode.Server;
            response.Poll = request.Poll;
            response.OriginTimestamp = request.TransmitTimestamp;
            response.RecieveTimestamp = GetClock();
            response.TransmitTimestamp = GetClock();
            response.ReferenceTimestamp += timestampDelta;
            return response;
        }

        private SntpPacket FormResponsePacketLocally(SntpPacket request)
        {
            var response = request;
            response.Mode = Mode.Server;
            response.ReferenceId = BitConverter.ToUInt32(localEp.Address.GetAddressBytes(), 0);
            response.OriginTimestamp = request.TransmitTimestamp;
            response.RecieveTimestamp = GetClock();
            response.ReferenceTimestamp = GetClock();
            response.TransmitTimestamp = GetClock();
            return response;
        }

        protected override void ClientHandlingThreadFunc(byte[] data, IPEndPoint clientEndPoint)
        {
            var packet = SntpPacket.FromBytes(data);
            if (packet.Mode != Mode.Client)
                return;
            var response = FormResponsePacket(packet).ToBytes();
            EnqueueDaragramm(response, clientEndPoint);
        }
    }
}
