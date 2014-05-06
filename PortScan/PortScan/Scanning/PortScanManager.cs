using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PortScan.Common;

namespace PortScan.Scanning
{
    internal class PortScanManager
    {
        private readonly Dictionary<PortId, string> portDescription;
        private readonly int scanFrom;
        private readonly int scanTo;

        public PortScanManager(Dictionary<PortId, string> portDescription, int scanFrom, int scanTo)
        {
            this.portDescription = portDescription;
            this.scanFrom = scanFrom;
            this.scanTo = scanTo;
        }

        private static readonly IPortScanner[] portScanners =
        {
            new TcpScanner(),
            new UdpScanner()
        };

        private static readonly IApplicationLayerPortScanner[] applicationLayerPortScanners =
        {
            new HttpScanner(),
            new DnsScanner(),
            new NtpScanner()
        };

        public Dictionary<PortId, ApplicationProtocol> Scan(IPAddress addr)
        {
            var portScanningStatus = new PortScanningStatus();
            Console.WriteLine("Scanning ports...");
            foreach (var scanner in portScanners)
                scanner.Scan(addr, scanFrom, scanTo, portScanningStatus);
            Console.WriteLine("Found {0} opened ports", portScanningStatus.OpenPorts.Count);
            Console.WriteLine("Found {0} closed ports", portScanningStatus.ClosedPorts.Count);
            var detectedProtocols = portScanningStatus.OpenPorts.ToDictionary(id => id, id => ApplicationProtocol.None);
            foreach (var scanner in applicationLayerPortScanners)
                scanner.Scan(addr, scanFrom, scanTo, portScanningStatus, detectedProtocols);
            return detectedProtocols;
        }
    }
}
