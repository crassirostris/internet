using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PortScan.Common;

namespace PortScan.Scanning
{
    internal class TcpScanner : IPortScanner
    {
        private const int TimeoutMilliseconds = 1000;

        public void Scan(IPAddress addr, int scanFrom, int scanTo, PortScanningStatus portScanningStatus)
        {
            var sockets = new List<Socket>();
            foreach (var portNumber in Enumerable.Range(scanFrom, scanTo - scanFrom + 1))
            {
                var remoteEp = new IPEndPoint(addr, portNumber);
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var socketEventArgs = new SocketAsyncEventArgs { RemoteEndPoint = remoteEp };
                socketEventArgs.Completed += (sender, args) =>
                {
                    var portId = new PortId(TransportProtocol.Tcp, ((IPEndPoint)args.RemoteEndPoint).Port);
                    lock (portScanningStatus)
                        if (args.SocketError == SocketError.Success)
                            portScanningStatus.OpenPorts.Add(portId);
                        else
                            portScanningStatus.ClosedPorts.Add(portId);
                };
                socket.ConnectAsync(socketEventArgs);
                sockets.Add(socket);
            }
            Thread.Sleep(TimeoutMilliseconds);
            foreach (var socket in sockets)
                socket.Dispose();
        }
    }
}