using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SntpServer
{
    public abstract class UdpServer
    {
        private readonly Thread workingThread;
        private readonly UdpClient listener;
        private readonly Queue<Tuple<byte[], IPEndPoint>> datagramsQueue = new Queue<Tuple<byte[], IPEndPoint>>();

        protected readonly IPEndPoint localEp;

        public UdpServer(int port)
        {
            workingThread = new Thread(SocketListeningThreadFunc) { IsBackground = false };
            localEp = new IPEndPoint(IPAddress.Any, port);
            listener = new UdpClient(localEp);
        }

        protected void EnqueueDaragramm(byte[] datagramm, IPEndPoint endPoint)
        {
            lock (datagramsQueue)
            {
                datagramsQueue.Enqueue(Tuple.Create(datagramm, endPoint));
            }
        }

        protected abstract void ClientHandlingThreadFunc(byte[] data, IPEndPoint clientEndPoint);

        private void SocketListeningThreadFunc()
        {
            while (true)
            {
                lock (datagramsQueue)
                {
                    while (datagramsQueue.Count > 0)
                    {
                        var datagramm = datagramsQueue.Dequeue();
                        listener.Send(datagramm.Item1, datagramm.Item1.Length, datagramm.Item2);
                    }
                }
                if (listener.Available <= 0) 
                    continue;
                IPEndPoint clientEndPoint = null;
                var data = listener.Receive(ref clientEndPoint);
                Console.WriteLine("Got request from {0}", clientEndPoint);
                Task.Run(() => ClientHandlingThreadFunc(data, clientEndPoint));
            }
        }

        public void Run()
        {
            Console.WriteLine("Starting server...");
            workingThread.Start();
        }

        public void Stop()
        {
            listener.Close();
        }
    }
}
