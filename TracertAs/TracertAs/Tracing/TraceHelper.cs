using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace TracertAs.Tracing
{
    internal static class TraceHelper
    {
        private static readonly Ping ping = new Ping();
        private const int Timeout = 1000;
        private static readonly byte[] data = Encoding.ASCII.GetBytes("1234567890");
        private const int TriesCount = 3;

        public static IEnumerable<IPAddress> Trace(IPAddress destinationAddress)
        {
            for (int i = 1; i < 256; i++)
            {
                for (int j = 0; j < TriesCount; j++)
                {
                    var reply = ping.Send(destinationAddress, Timeout, data, new PingOptions { Ttl = i });
                    if (reply == null)
                        continue;
                    if (reply.Status == IPStatus.TtlExpired)
                    {
                        yield return reply.Address;
                        break;
                    }
                    if (reply.Status == IPStatus.Success)
                    {
                        yield return destinationAddress;
                        yield break;
                    }
                }
            }
        }

        public static bool Traceable(IPAddress ip)
        {
            var reply = ping.Send(ip, Timeout);
            return reply != null && reply.Status == IPStatus.Success;
        }
    }
}