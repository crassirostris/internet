using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Services;
using System.Text;

namespace TracertAs.Tracing
{
    internal static class TraceHelper
    {
        private static readonly Ping ping = new Ping();
        private const int Timeout = 1000;
        private static readonly byte[] data = Encoding.ASCII.GetBytes("1234567890");
        private const int AllowedFailedPings = 2;
        private const int TriesCount = 3;

        public static IEnumerable<IPAddress> GetTraceAddresses(IPAddress destinationAddress)
        {
            return Trace(destinationAddress).Distinct();
        }

        public static IEnumerable<IPAddress> Trace(IPAddress destinationAddress)
        {
            var failed = 0;
            Console.WriteLine("Tracing {0}...", destinationAddress);
            for (int i = 1; i < 256; i++)
            {
                for (int j = 0; j < TriesCount; j++)
                {
                    var reply = ping.Send(destinationAddress, Timeout, data, new PingOptions { Ttl = i });
                    if (reply == null)
                        continue;
                    if (reply.Status == IPStatus.TtlExpired)
                    {
                        failed = 0;
                        Console.Write("{0} -> ", reply.Address);
                        yield return reply.Address;
                        break;
                    }
                    if (reply.Status == IPStatus.Success || (j == TriesCount - 1 && failed >= AllowedFailedPings))
                    {
                        failed = 0;
                        Console.WriteLine("{0}", destinationAddress);
                        yield return destinationAddress;
                        yield break;
                    }
                    if (j == TriesCount - 1)
                    {
                        Console.Write("* -> ");
                        ++failed;
                    }
                }
            }
            yield return destinationAddress;
        }

        public static bool Traceable(IPAddress ip)
        {
            var reply = ping.Send(ip, Timeout);
            return reply != null && reply.Status == IPStatus.Success;
        }
    }
}