using System;
using System.IO;

namespace SntpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            decimal timeDelta = default(decimal);
            if (args.Length > 0 && !decimal.TryParse(args[0], out timeDelta))
                ShowHelp();
            var server = new SntpUdpServer(timeDelta);
            server.Run();
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage: {0} [time_delta]", Path.GetFileName(Environment.GetCommandLineArgs()[0]));
            Environment.Exit(0);
        }
    }
}
