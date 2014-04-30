using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PortScan
{
    internal class PortStatus
    {
        private readonly HashSet<TransportLayerProtocol> transportLayerProtocols = new HashSet<TransportLayerProtocol>();
        private readonly HashSet<ApplicationLevelProtocol> applicationLevelProtocols = new HashSet<ApplicationLevelProtocol>();

        public PortStatus(IPAddress address, int port)
        {
            Address = address;
            Port = port;
        }

        public IPAddress Address { get; private set; }
        public int Port { get; private set; }

        public HashSet<TransportLayerProtocol> TransportLayerProtocols
        {
            get { return transportLayerProtocols; }
        }

        public HashSet<ApplicationLevelProtocol> ApplicationLevelProtocols
        {
            get { return applicationLevelProtocols; }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, new []
            {
                string.Format("For port {0}", Port),
                string.Format("Transport Layer Protocols avaliable: {0}", string.Join(", ", transportLayerProtocols.Select(e => e.ToString()))),
                string.Format("Application Layer Protocols avaliable: {0}", string.Join(", ", applicationLevelProtocols.Select(e => e.ToString())))
            });
        }
    }
}