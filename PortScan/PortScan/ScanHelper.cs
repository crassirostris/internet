using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PortScan
{
    internal static class ScanHelper
    {
        private static readonly int scanFrom;
        private static readonly int scanTo;

        private static readonly PortChecker[] transportLayerCheckers = 
        {
            new TcpChecker()
        };

        private static readonly PortChecker[] applicationLayerCheckers =
        {
            new HttpChecker(),
            new DnsChecker()
        };


        private const int DefaultScanFrom = 1;
        private const int DefaultScanTo = 65535;

        static ScanHelper()
        {
            scanFrom = ConfigHelper.GetIntFromConfig("scanFrom", DefaultScanFrom);
            scanTo = ConfigHelper.GetIntFromConfig("scanTo", DefaultScanTo);
        }

        public static IEnumerable<PortStatus> Scan(IPAddress addr)
        {
            return Enumerable.Range(scanFrom, scanTo - scanFrom + 1)
                .AsParallel()
                .Select(port => ScanPort(addr, port))
                .Where(e => e.TransportLayerProtocols.Count > 0);
        }

        private static PortStatus ScanPort(IPAddress addr, int port)
        {
            Console.WriteLine("Scanning {0}...", port);
            var status = new PortStatus(addr, port);
            foreach (var checker in transportLayerCheckers)
                checker.Check(status);
            foreach (var checker in applicationLayerCheckers)
                checker.Check(status);
            if (status.ApplicationLevelProtocols.Count + status.TransportLayerProtocols.Count > 0)
                Console.WriteLine(string.Join(Environment.NewLine, new []
                {
                    "Success!",
                    status.ToString()
                }));
            return status;
        }
    }
}
