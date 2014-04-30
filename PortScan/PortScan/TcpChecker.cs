namespace PortScan
{
    internal class TcpChecker : PortChecker
    {
        public override void Check(PortStatus status)
        {
            try
            {
                using (var socket = CreateTcpSocket(status))
                    socket.Connect(status.Address, status.Port);
                status.TransportLayerProtocols.Add(TransportLayerProtocol.Tcp);
            }
            catch { }
        }
    }
}