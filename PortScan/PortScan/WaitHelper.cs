using System;
using System.Threading;
using System.Threading.Tasks;

namespace PortScan
{
    internal static class WaitHelper
    {
        public static void Wait(Action action, int timeout)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);
            Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception)
                {
                }
            }).Wait(cts.Token);
        }
    }
}