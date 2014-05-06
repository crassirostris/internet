using System.Net;

namespace PortScan
{
    internal interface IPortScanner
    {
        void Scan(IPAddress addr, int scanFrom, int scanTo, PortScanningStatus portScanningStatus);
    }
}