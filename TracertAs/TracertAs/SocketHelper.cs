using System;
using System.Net.Sockets;

namespace TracertAs
{
    internal static class SocketHelper
    {
        private const int BufferSize = 4096;

        public static byte[] RecieveAllBytes(this Socket socket)
        {
            var buffer = new byte[BufferSize];
            var result = new byte[0];
            try
            {
                int bytesRead;
                while ((bytesRead = socket.Receive(buffer)) > 0)
                {
                    Array.Resize(ref result, result.Length + bytesRead);
                    Array.Copy(buffer, 0, result, result.Length - bytesRead, bytesRead);
                }
            }
            catch (SocketException)
            {
                return result;
            }
            return result;
        }
    }
}