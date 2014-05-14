using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DnsCache.Dns.ResourceRecords;

namespace DnsCache.Dns
{
    internal class DnsCacheManager
    {
        private readonly IPAddress[] nameservers;
        private readonly ConcurrentDictionary<QuestionResourceRecord, object> lockers = new ConcurrentDictionary<QuestionResourceRecord, object>();
        private readonly Dictionary<QuestionResourceRecord, DateTime> questionLastTimeRequested = new Dictionary<QuestionResourceRecord, DateTime>();
        private readonly Dictionary<QuestionResourceRecord, DnsMessage> cache = new Dictionary<QuestionResourceRecord, DnsMessage>();
        private const int ForwarderAskingDelay = 10 * 60;
        private const int DefaultTimeout = 1;
        private const int DefaultDnsPort = 53;

        public DnsCacheManager(IPAddress[] nameservers)
        {
            this.nameservers = nameservers;
        }

        public byte[] HandlePacket(byte[] packet)
        {
            DnsMessage message;
            if (!DnsMessage.TryPase(packet, out message))
            {
                Console.WriteLine("Parsing error");
                return null;
            }
            try
            {
                return HandleQueryMessage(message).GetBytes();
            }
            catch (Exception e)
            {
                Console.WriteLine("Internal error");
                Console.WriteLine(e);
                return GetServerFailMessage(message).GetBytes();
            }
        }

        private DnsMessage GetServerFailMessage(DnsMessage message)
        {
            var result = GetEmptyResponseMessage(message);
            result.ResponseCode = DnsMessageResponseCode.ServerFailure;
            return result;
        }

        private DnsMessage HandleQueryMessage(DnsMessage query)
        {
            if (query.Opcode != DnsMessageOpcode.Query && query.Opcode != DnsMessageOpcode.InversedQuery)
                throw new InvalidOperationException();
            var answer = GetEmptyResponseMessage(query);
            foreach (var question in query.Questions)
            {
                var locker = lockers.GetOrAdd(question, new object());
                lock (locker)
                {
                    lock (cache)
                    {
                        if (cache.ContainsKey(question) && DateTime.Now > GetExpirationTime(question))
                            cache.Remove(question);

                        if (!questionLastTimeRequested.ContainsKey(question) || 
                            (DateTime.Now - questionLastTimeRequested[question] < TimeSpan.FromSeconds(ForwarderAskingDelay) 
                                && !cache.ContainsKey(question)))
                        {
                            Monitor.Exit(cache);
                            AskForwarders(question);
                            Monitor.Enter(cache);
                            questionLastTimeRequested[question] = DateTime.Now;
                        }

                        if (!cache.ContainsKey(question)) 
                            continue;

                        var questionAnswer = cache[question];
                        answer.Answers.AddRange(questionAnswer.Answers);
                        answer.Authority.AddRange(questionAnswer.Authority);
                        answer.Additional.AddRange(questionAnswer.Additional);
                    }
                }
            }
            return answer;
        }

        private DateTime GetExpirationTime(QuestionResourceRecord question)
        {
            DateTime lastTimeRequested;
            lock (questionLastTimeRequested)
                lastTimeRequested = questionLastTimeRequested[question];
            var answer = cache[question];
            var ttl = answer.Answers.Concat(answer.Authority).Concat(answer.Additional).Select(e => e.Ttl).Min();
            return lastTimeRequested + TimeSpan.FromSeconds(ttl);
        }

        private DnsMessage GetEmptyResponseMessage(DnsMessage query)
        {
            var result = DnsMessage.Parse(query.GetBytes());
            result.Type = DnsMessageType.Response;
            return result;
        }

        private void AskForwarders(QuestionResourceRecord question)
        {
            var questionMessage = GetQuestionMessage(question);
            foreach (var nameserver in nameservers)
            {
                try
                {
                    var answer = AskForwarder(questionMessage.GetBytes(), nameserver);
                    lock (cache)
                        cache[question] = answer;
                    break;
                }
                catch
                { }
            }
        }

        private DnsMessage GetQuestionMessage(QuestionResourceRecord question)
        {
            var result = new DnsMessage();
            result.Id = (ushort)new Random((int)DateTime.Now.ToFileTime()).Next();
            result.QuestionsCount = 1;
            result.Questions.Add(question);
            return result;
        }

        private static DnsMessage AskForwarder(byte[] packet, IPAddress nameserver)
        {
            using (var udpClient = new UdpClient(AddressFamily.InterNetwork) { Client = { ReceiveTimeout = DefaultTimeout, SendTimeout = DefaultTimeout } })
            {
                udpClient.Connect(nameserver, DefaultDnsPort);
                udpClient.Send(packet, packet.Length);
                IPEndPoint remoteEp = null;
                var data = udpClient.Receive(ref remoteEp);
                return DnsMessage.Parse(data);
            }
        }
    }
}