using System.Collections.Generic;
using System.Net;

namespace PortScan
{
    internal interface IApplicationLayerPortScanner
    {
        void Scan(IPAddress address, int scanFrom, int scanTo, PortScanningStatus portScanningStatus, Dictionary<PortId, ApplicationProtocol> detectedProtocols);
    }
}