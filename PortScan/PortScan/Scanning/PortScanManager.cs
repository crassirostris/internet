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
        private readonly TransportProtocol[] protocolsToScan;

        public PortScanManager(Dictionary<PortId, string> portDescription, int scanFrom, int scanTo, TransportProtocol[] protocolsToScan)
        {
            this.portDescription = portDescription;
            this.scanFrom = scanFrom;
            this.scanTo = scanTo;
            this.protocolsToScan = protocolsToScan;
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
            foreach (var scanner in portScanners.Where(scanner => protocolsToScan.Contains(scanner.Protocol)))
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
