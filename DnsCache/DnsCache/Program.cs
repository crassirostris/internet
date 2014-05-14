using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using DnsCache.Dns;
using DnsCache.Helpers;
using DnsCache.Servers;

namespace DnsCache
{
    class Program
    {
        static void Main()
        {
            var nameservers = GetNameserversFromInterfaces().
                Concat(GetNameserversFromConfig())
                .ToArray();
            if (nameservers.Length == 0)
            {
                Console.Write("Failed to find at least one nameserver");
                Environment.Exit(0);
            }
            var dnsCacheManager = new DnsCacheManager(nameservers);
            var servers = CreateServer(dnsCacheManager);
            foreach (var server in servers)
                server.Run();
        }

        private static IEnumerable<IServer> CreateServer(DnsCacheManager dnsCacheManager)
        {
            var port = ConfigHelper.GetInt("port", ConfigHelper.DefaultDnsPort);
            return new IServer[]
            {
                new TcpServer(port, dnsCacheManager.HandlePacket),
                new UdpServer(port, dnsCacheManager.HandlePacket)
            };
        }

        private static IEnumerable<IPAddress> GetNameserversFromInterfaces()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .SelectMany(networkInterface => networkInterface
                    .GetIPProperties()
                    .DnsAddresses);
        }

        private static IEnumerable<IPAddress> GetNameserversFromConfig()
        {
            return ConfigHelper
                .GetValues("nameserver")
                .Select(ns =>
                {
                    IPAddress address;
                    return IPAddress.TryParse(ns, out address) ? address : null;
                })
                .Where(e => e != null);
        }
    }
}
