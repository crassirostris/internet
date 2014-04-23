namespace SntpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            decimal timeDelta = default(decimal);
            if (args.Length > 0)
                timeDelta = decimal.Parse(args[0]);
            var server = new SntpUdpServer(123, timeDelta);
            server.Run();
        }
    }
}
