using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DnsCache.Servers
{
    internal class TcpServer : Server
    {
        private readonly Func<byte[], byte[]> action;

        public TcpServer(int port, Func<byte[], byte[]> action)
            : base(new IPEndPoint(0L, port), new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            this.action = action;
        }

        protected override void ProcessConnection()
        {
            var clientSocket = socket.Accept();
            Console.WriteLine("Accepted connection from {0}", clientSocket.RemoteEndPoint);
            Task.Run(() =>
            {
                try
                {
                    ProcessClient(clientSocket);
                    clientSocket.Close();
                }
                catch
                { }
            });
        }

        protected override void InitializeListener()
        {
            socket.Listen(Int32.MaxValue);
        }

        protected override void ProcessClient(Socket clientSocket)
        {
            var lengthBuffer = new byte[2];
            if (clientSocket.Receive(lengthBuffer, 2, SocketFlags.None) < 2)
                return;
            var length = BitConverter.ToUInt16(lengthBuffer, 0);
            var buffer = new byte[length];
            if (clientSocket.Receive(buffer, length, SocketFlags.None) != length)
                return;
            var result = action(buffer);
            if (result == null)
                return;
            clientSocket.Send(BitConverter.GetBytes((uint)result.Length));
            clientSocket.Send(result);
        }
    }
}
