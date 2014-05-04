using System.Collections.Generic;

namespace PortScan
{
    internal class PortScanningStatus
    {
        private readonly HashSet<PortId> openPorts = new HashSet<PortId>();
        private readonly HashSet<PortId> closedPorts = new HashSet<PortId>();

        public HashSet<PortId> OpenPorts
        {
            get { return openPorts; }
        }

        public HashSet<PortId> ClosedPorts
        {
            get { return closedPorts; }
        }
    }
}