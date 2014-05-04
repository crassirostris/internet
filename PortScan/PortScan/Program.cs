using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace PortScan
{
    class Program
    {
        private const int DefaultScanFrom = 1;
        private const int DefaultScanTo = 65535;

        private const string PortDescriptionsLocation = @"descriptions.txt";

        static void Main(string[] args)
        {
            if (args.Length < 1)
                ShowHelp();
            int scanFrom = ConfigHelper.GetIntFromConfig("scanFrom", DefaultScanFrom);
            int scanTo = ConfigHelper.GetIntFromConfig("scanTo", DefaultScanTo);
            var psm = new PortScanManager(new Dictionary<PortId, string>(), scanFrom, scanTo);
            var portDescriptions = LoadPortDescriptinos();
            foreach (var addr in args)
            {
                var ipAddresses = Dns.GetHostAddresses(addr);
                if (ipAddresses.Length == 0)
                {
                    Console.WriteLine("Could not resolve {0}", addr);
                    continue;
                }
                var ip = ipAddresses.First(e => e.AddressFamily == AddressFamily.InterNetwork);
                var portStatuses = psm.Scan(ip);
                foreach (var portId in portStatuses.Keys)
                {
                    Console.WriteLine("Port {0} is open", portId);
                    if (portStatuses[portId] == ApplicationProtocol.None)
                        if (portDescriptions.ContainsKey(portId))
                            Console.WriteLine("Suspected protocol: {0}", portDescriptions[portId]);
                        else
                            Console.WriteLine("Unknown protocol");
                    else
                        Console.WriteLine("Found protocol: {0}", portStatuses[portId]);
                }
            }
        }

        private static Dictionary<PortId, string> LoadPortDescriptinos()
        {
            var lines = File.ReadAllLines(PortDescriptionsLocation);
            var records = lines
                .Select(line => line.Split(new[] { '\t' }, StringSplitOptions.None))
                .Where(record => record.Length >= 4)
                .Where(record => record.All(column => column != String.Empty));
            var result = new Dictionary<PortId, string>();
            foreach (var record in records)
            {
                int portNumber;
                TransportProtocol protocol;
                if (!int.TryParse(record[1], out portNumber) || !Enum.TryParse(record[2], true, out protocol))
                    continue;
                var portId = new PortId(protocol, portNumber);
                result[portId] = string.Format("{0} ({1})", record[0], record[3]);
            }
            return result;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage: {0} address1 [address2 ...]", Path.GetFileName(Environment.GetCommandLineArgs()[0]));
            Environment.Exit(0);
        }
    }
}
