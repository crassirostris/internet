using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PortScan
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
                ShowHelp();
            foreach (var addr in args)
            {
                var ipAddresses = Dns.GetHostAddresses(addr);
                if (ipAddresses.Length == 0)
                {
                    Console.WriteLine("Could not resolve {0}", addr);
                    continue;
                }
                var ip = ipAddresses.First(e => e.AddressFamily == AddressFamily.InterNetwork);
                var portStatuses = ScanHelper.Scan(ip).ToArray();
                Console.WriteLine();
                foreach (var portStatus in portStatuses)
                    Console.WriteLine(portStatus);
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage: {0} address1 [address2 ...]", Path.GetFileName(Environment.GetCommandLineArgs()[0]));
            Environment.Exit(0);
        }
    }
}
