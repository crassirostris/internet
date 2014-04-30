using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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

    internal static class WaitHelper
    {
        public static void Wait(Action action, int timeout)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);
            try
            {

                Task.Run(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception)
                    {
                    }
                }).Wait(cts.Token);
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }
    }
}