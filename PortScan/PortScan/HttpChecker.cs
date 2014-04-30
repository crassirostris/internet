using System.Text;

namespace PortScan
{
    internal class HttpChecker : PortChecker
    {
        private const string HttpRequest = "GET / HTTP/1.0\r\n\r\n";
        private const string HttpResponsePrefix = "HTTP/1";

        public override void Check(PortStatus status)
        {
            try
            {
                if (!status.TransportLayerProtocols.Contains(TransportLayerProtocol.Tcp))
                    return;
                using (var socket = CreateTcpSocket(status))
                {
                    socket.Send(Encoding.UTF8.GetBytes(HttpRequest));
                    var response = Encoding.UTF8.GetString(socket.ReceiveAllBytes());
                    if (IsHttpResponse(response))
                        status.ApplicationLevelProtocols.Add(ApplicationLevelProtocol.Http);
                }
            }
            catch { }
        }

        private static bool IsHttpResponse(string response)
        {
            return response.StartsWith(HttpResponsePrefix);
        }
    }
}