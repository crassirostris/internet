using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using TracertAs.Tracing;
using TracertAs.Whois;

namespace TracertAs
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                ShowHelp();
            foreach (var ipString in args)
            {
                IPAddress ip;
                if (!IPAddress.TryParse(ipString, out ip))
                {
                    Console.WriteLine("{0} is not an ip", ipString);
                    continue;
                }
                if (!TraceHelper.Traceable(ip))
                {
                    Console.WriteLine("Failed to trace address {0}", ip);
                    continue;
                }
                var trace = TraceHelper.Trace(ip).ToArray();
                var info = trace.Select(WhoisHelper.GetIpInformation).ToArray();
                PrintTrace(info, ip);
            }
        }

        private static void PrintTrace(IEnumerable<AddressInformation> trace, IPAddress destination)
        {
            foreach (var addressInformation in trace)
                Console.WriteLine(addressInformation);
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage: {0} [ip1 [ip2 ...]]", Path.GetFileName(Environment.GetCommandLineArgs()[0]));
            Environment.Exit(0);
        }
    }
}
