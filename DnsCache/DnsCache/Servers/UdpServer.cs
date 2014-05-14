using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DnsCache.Servers
{
    internal class UdpServer : Server
    {
        private readonly Func<byte[], byte[]> action;
        private const int MaxDgamLength = 512;
        private readonly byte[] buffer = new byte[MaxDgamLength];

        public UdpServer(int port, Func<byte[], byte[]> action)
            : base(new IPEndPoint(0L, port), new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            this.action = action;
        }

        protected override void ProcessConnection()
        {
            EndPoint remoteEp = new IPEndPoint(0, 0);
            var length = socket.ReceiveFrom(buffer, ref remoteEp);
            Console.WriteLine("Recieved {0} bytes from {1}", length, remoteEp);
            var localBuffer = buffer.Take(length).ToArray();
            Task.Run(() =>
            {
                try
                {
                    var result = action(localBuffer);
                    if (result == null)
                        return;
                    using (var client = new UdpClient())
                    {
                        client.Connect((IPEndPoint) remoteEp);
                        client.Send(result, result.Length);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        protected override void InitializeListener()
        {
        }

        protected override void ProcessClient(Socket clientSocket)
        {
        }
    }
}
