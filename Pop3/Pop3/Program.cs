using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Pop3
{
    class Message
    {
        public int Number { get; set; }
        public int Length { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public Message(int number, int length)
        {
            Number = number;
            Length = length;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, new[]
            {
                string.Format("Message #{0} ({1} bytes long)", Number, Length),
                string.Format("From:    {0}", From ?? "<Unknown Sender>"),
                string.Format("To:      {0}", To ?? "<Unknown Recipient>"),
                string.Format("Subject: {0}", Subject ?? "<No Subject>")
            });
        }
    }

    class Program
    {
        private const int BufferSize = 1024;
        private const int ServerPort = 110;
        private const int DefaultTimeout = 1000;
        private const string Pop3LineEnding = "\r\n";
        private static readonly byte[] buffer = new byte[BufferSize];
        private static readonly byte[] multilineTerminatingSequence = Encoding.ASCII.GetBytes(Pop3LineEnding + "." + Pop3LineEnding);
        private static readonly Regex encodingRegex = new Regex(@"charset=(\S+)");
        private static readonly Regex encodedPartRegex = new Regex(@"\=\?([^?]+)\?([BQbq])\?([^?]+)\?\=");
        private static Socket socket;
        private static IPAddress server;
        private static string username;
        private static string password;

        private static string[] Communicate(string message = null, bool multiline = false)
        {
            while (true)
            {
                try
                {
                    var terminatingSequence = multiline ? multilineTerminatingSequence : Encoding.ASCII.GetBytes(Pop3LineEnding);
                    if (message != null)
                        socket.Send(Encoding.ASCII.GetBytes(message + Pop3LineEnding));

                    var result = Enumerable.Empty<byte>();

                    int length;
                    while ((length = socket.Receive(buffer)) > 0)
                    {
                        result = result.Concat(buffer.Take(length).ToArray());
                        if (buffer.Take(length)
                                .Reverse()
                                .Take(terminatingSequence.Length)
                                .Reverse()
                                .SequenceEqual(terminatingSequence))
                            break;
                    }

                    var resultedBytes = result.ToArray();

                    var str = Encoding.ASCII.GetString(resultedBytes);
                    var m = encodingRegex.Match(str);
                    if (m.Success)
                    {
                        try
                        {
                            var encodingName = m.Groups[1].Value.Replace("\"", "").Replace(";", "");
                            var encoding = Encoding.GetEncoding(encodingName);
                            str = encoding.GetString(resultedBytes);
                        }
                        catch
                        { }
                    }

                    return str
                        .Split(new[] { Pop3LineEnding }, StringSplitOptions.None)
                        .Select(s => s.StartsWith(".") ? s.Substring(1) : s)
                        .ToArray();
                }
                catch
                {
                    RecreateSocket();
                }
            }
        }

        private static bool IsFailResponse(string[] response)
        {
            return response.Length == 0 || response[0].StartsWith("-");
        }

        private static string Decode(string str)
        {
            return encodedPartRegex.Replace(str, Evaluator);
        }

        private static string Evaluator(Match match)
        {
            try
            {
                return DecodeEncodedWord(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
            }
            catch
            {
                return match.Value;
            }
        }

        private static string DecodeEncodedWord(string encodingName, string type, string encodedText)
        {
            var encoding = Encoding.GetEncoding(encodingName);
            if (type.ToLower() == "b")
                return encoding.GetString(Convert.FromBase64String(encodedText));
            if (type.ToLower() == "q")
                return encoding.GetString(DecodeQuotedPrintableEncoding(encodedText));
            throw new InvalidOperationException();
        }

        private static byte[] DecodeQuotedPrintableEncoding(string encodedText)
        {
            var result = new List<byte>();
            for (int i = 0; i < encodedText.Length; i++)
            {
                if (encodedText[i] == '=')
                {
                    result.Add((byte) int.Parse(encodedText.Substring(i + 1, 2), NumberStyles.HexNumber));
                    i += 2;
                }
                else if (encodedText[i] == '_')
                    result.Add((byte) ' ');
                else
                    result.Add((byte) encodedText[i]);
            }
            return result.ToArray();
        }

        private static void RecreateSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = DefaultTimeout, SendTimeout = DefaultTimeout};
            socket.Connect(new IPEndPoint(server, ServerPort));
            try
            {
                if (socket.Receive(buffer) == 0 || buffer[0] == '-')
                    Exit("Server failed");
                socket.Send(Encoding.ASCII.GetBytes("USER " + username + Pop3LineEnding));
                if (socket.Receive(buffer) == 0 || buffer[0] == '-')
                    Exit("Failed to authenticate: invalid username");
                socket.Send(Encoding.ASCII.GetBytes("PASS " + password + Pop3LineEnding));
                if (socket.Receive(buffer) == 0 || buffer[0] == '-')
                    Exit("Failed to authenticate: invalid password");
            }
            catch (SocketException)
            {
                Exit("Failed to Authenticate");
            }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (args.Length < 3)
                Exit(null, true);

            server = GetServer(args[0]);
            username = args[1];
            password = args[2];

            RecreateSocket();

            try
            {
                var list = Communicate("LIST", true);
                if (IsFailResponse(list))
                    Exit("Failed to list mailbox");
                foreach (var message in list
                    .Skip(1)
                    .Where(e => e != String.Empty)
                    .Select(s => s.Split(' '))
                    .Select(chunks => new Message(int.Parse(chunks[0]), int.Parse(chunks[1]))))
                {
                    var headers =
                        Communicate(string.Format("TOP {0} 0", message.Number), true).Skip(1).ToArray();
                    headers = ParseHeaders(headers);
                    headers = headers
                        .Select(Decode)
                        .ToArray();

                    message.From = ExtractField(headers, "From");
                    message.Subject = ExtractField(headers, "Subject");
                    message.To = ExtractField(headers, "To");

                    Console.WriteLine(message);
                    Console.WriteLine();
                }
            }
            catch
            {
                Console.WriteLine("Something gone wrong");
            }
        }

        private static string[] ParseHeaders(IEnumerable<string> headers)
        {
            var newHeaders = new List<string>();
            foreach (var header in headers.Where(header => header != String.Empty))
            {
                if (header[0] == ' ' || header[0] == '\t')
                    newHeaders[newHeaders.Count - 1] = newHeaders[newHeaders.Count - 1] + header.Substring(1);
                else
                    newHeaders.Add(header);
            }
            return newHeaders.ToArray();
        }

        private static string ExtractField(IEnumerable<string> messageBody, string fieldName)
        {
            var result = messageBody.FirstOrDefault(e => e.ToLower().StartsWith(fieldName.ToLower()));
            return result == null ? null : result.Substring(result.IndexOf(':') + 1);
        }

        private static IPAddress GetServer(string addrStr)
        {
            var addr = Dns.GetHostAddresses(addrStr).FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            if (addr == null)
                Exit("Filed to resolve server address");
            return addr;
        }

        private static void Exit(string message = null, bool showHelp = false)
        {
            if (message != null)
                Console.WriteLine(message);
            if (showHelp)
                Console.WriteLine("Usage: {0} <server> <username> <password>", Environment.GetCommandLineArgs()[0]);
            Environment.Exit(0);
        }
    }
}
