using System;
using System.Net.Sockets;

namespace PortScan
{
    internal abstract class PortChecker
    {
        protected static int SocketTimeout;
        private const int DefaultSocketTimeout = 1000;

        protected static Socket CreateUdpSocket(PortStatus status)
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveTimeout = SocketTimeout,
                SendTimeout = SocketTimeout
            };
            WaitHelper.Wait(() => sock.Connect(status.Address, status.Port), SocketTimeout);
            return sock;
        }

        protected static Socket CreateTcpSocket(PortStatus status)
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = SocketTimeout,
                SendTimeout = SocketTimeout
            };
            WaitHelper.Wait(() => sock.Connect(status.Address, status.Port), SocketTimeout);
            return sock;
        }

        static PortChecker()
        {
            SocketTimeout = ConfigHelper.GetIntFromConfig("socketTimeout", DefaultSocketTimeout);
        }

        public abstract void Check(PortStatus status);
    }
}