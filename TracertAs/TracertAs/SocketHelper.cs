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
            do
            {
                try
                {
                    var bytesRead = socket.Receive(buffer);
                    Array.Resize(ref result, result.Length + bytesRead);
                    Array.Copy(buffer, 0, result, result.Length - bytesRead, bytesRead);
                }
                catch (SocketException)
                {
                    return result;
                }
            } while (socket.Available > 0);
            return result;
        }
    }
}