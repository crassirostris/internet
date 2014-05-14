using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DnsCache.Servers
{
    internal abstract class Server : IServer
    {
        protected readonly Socket socket;

        protected Server(EndPoint localEp, Socket socket)
        {
            //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.socket = socket;
            socket.Bind(localEp);
        }

        public void Run()
        {
            var t = new Thread(Listen);
            t.IsBackground = false;
            t.Start();
        }

        protected virtual void Listen()
        {
            Console.WriteLine("Listening on {0}:{1}", socket.ProtocolType, ((IPEndPoint)(socket.LocalEndPoint)).Port);
            try
            {
                InitializeListener();
                while (true)
                {
                    ProcessConnection();
                }
            }
            catch (SocketException e)
            {
                Console.Write(e);
            }
        }

        protected abstract void ProcessConnection();

        protected abstract void InitializeListener();
        protected abstract void ProcessClient(Socket clientSocket);

        public void Stop()
        {
            socket.Close();
        }
    }
}