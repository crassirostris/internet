using System.Net;
using PortScan.Common;

namespace PortScan.Scanning
{
    internal interface IPortScanner
    {
        void Scan(IPAddress addr, int scanFrom, int scanTo, PortScanningStatus portScanningStatus);
    }
}