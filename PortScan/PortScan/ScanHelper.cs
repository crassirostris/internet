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
                .WithDegreeOfParallelism(Environment.ProcessorCount / 2)
                .Select(port => ScanPort(addr, port))
                .Where(e => e.TransportLayerProtocols.Count > 0);
        }

        private static PortStatus ScanPort(IPAddress addr, int port)
        {
            var status = new PortStatus(addr, port);
            foreach (var checker in transportLayerCheckers)
                checker.Check(status);
            foreach (var checker in applicationLayerCheckers)
                checker.Check(status);
            Console.Write("{0} ", status.Port);
            return status;
        }
    }
}
