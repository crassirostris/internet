using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PortScan.Common;

namespace PortScan.Scanning
{
    internal class UdpScanner : IPortScanner
    {
        private const int PayloadSize = 1024;
        private const int TimeoutMilliseconds = 1000;
        private const int ScanningBulkSize = 1000;

        public TransportProtocol Protocol { get { return TransportProtocol.Udp; } }

        private void ScanBulk(IPAddress addr, int scanFrom, int scanTo, PortScanningStatus portScanningStatus)
        {
            var sockets = new List<Socket>();
            var random = new Random((int)DateTime.Now.ToFileTime());
            var randomPayload = new byte[PayloadSize];
            random.NextBytes(randomPayload);
            foreach (var portNumber in Enumerable.Range(scanFrom, scanTo - scanFrom + 1))
            {
                var remoteEp = new IPEndPoint(addr, portNumber);
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(remoteEp);
                try
                {
                    socket.Send(randomPayload, 0, randomPayload.Length, SocketFlags.None);
                }
                catch { }
                sockets.Add(socket);
            }
            Thread.Sleep(TimeoutMilliseconds);
            foreach (var socket in sockets)
            {
                var portId = new PortId(TransportProtocol.Udp, ((IPEndPoint)socket.RemoteEndPoint).Port);
                try
                {
                    if (socket.Poll(0, SelectMode.SelectRead))
                    {
                        socket.Receive(randomPayload, PayloadSize, SocketFlags.None);
                        lock (portScanningStatus)
                            portScanningStatus.OpenPorts.Add(portId);
                    }

                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == 10054)
                        lock (portScanningStatus)
                            portScanningStatus.ClosedPorts.Add(portId);
                }
                finally
                {
                    socket.Close();
                }
            }
        }

        public void Scan(IPAddress addr, int scanFrom, int scanTo, PortScanningStatus portScanningStatus)
        {
            for (int scanBulkFrom = scanFrom; scanBulkFrom <= scanTo; scanBulkFrom += ScanningBulkSize)
                ScanBulk(addr, scanBulkFrom, Math.Min(scanTo, scanBulkFrom + ScanningBulkSize), portScanningStatus);
        }
    }
}