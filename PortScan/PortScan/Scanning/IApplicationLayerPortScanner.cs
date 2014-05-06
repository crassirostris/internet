using System.Collections.Generic;
using System.Net;
using PortScan.Common;

namespace PortScan.Scanning
{
    internal interface IApplicationLayerPortScanner
    {
        void Scan(IPAddress address, int scanFrom, int scanTo, PortScanningStatus portScanningStatus, Dictionary<PortId, ApplicationProtocol> detectedProtocols);
    }
}