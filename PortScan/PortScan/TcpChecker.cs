using System;

namespace PortScan
{
    internal class TcpChecker : PortChecker
    {
        public override void Check(PortStatus status)
        {
            try
            {
                using (CreateTcpSocket(status))
                {
                }
                status.TransportLayerProtocols.Add(TransportLayerProtocol.Tcp);
            }
            catch
            { }
        }
    }
}