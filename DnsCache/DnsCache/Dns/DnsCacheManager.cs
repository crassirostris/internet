﻿using System;
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
        private const int DefaultRecievePacketSize = 1024;

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
                        if (!questionLastTimeRequested.ContainsKey(question) || 
                            (!cache.ContainsKey(question) && DateTime.Now - questionLastTimeRequested[question] < TimeSpan.FromSeconds(ForwarderAskingDelay)) ||
                            (cache.ContainsKey(question)  && DateTime.Now > GetExpirationTime(question)))
                        {
                            Monitor.Exit(cache);
                            Console.WriteLine("Cache miss on {0} {1} {2}", question.Name, question.Type, question.Class);
                            AskForwarders(question);
                            Monitor.Enter(cache);
                            questionLastTimeRequested[question] = DateTime.Now;
                        }

                        if (!cache.ContainsKey(question)) 
                            continue;

                        var questionAnswer = cache[question];
                        var elapsedSeconds = (DateTime.Now - questionLastTimeRequested[question]).Seconds;
                        answer.Answers.AddRange(AdjustTtl(questionAnswer.Answers, elapsedSeconds));
                        answer.Authority.AddRange(AdjustTtl(questionAnswer.Authority, elapsedSeconds));
                        answer.Additional.AddRange(AdjustTtl(questionAnswer.Additional, elapsedSeconds));
                    }
                }
            }
            return answer;
        }

        private IEnumerable<ResourceRecord> AdjustTtl(IEnumerable<ResourceRecord> resourceRecords, int adjustValue)
        {
            return resourceRecords.Select(e =>
            {
                var result = new ResourceRecord(e);
                result.Ttl = (uint) Math.Max(0, result.Ttl - adjustValue);
                return result;
            });
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
            var questionMessage = GetQuestionMessage(question).GetBytes();
            foreach (var nameserver in nameservers)
            {
                try
                {
                    var answer = AskForwarder(questionMessage, nameserver);
                    lock (cache)
                    {
                        cache[question] = answer;
                        foreach (var record in answer.Answers)
                            if (record.Type == ResourceRecordType.A && record.Name != question.Name)
                            {
                                var q = new QuestionResourceRecord
                                {
                                    Name = record.Name,
                                    Class = record.Class,
                                    Type = record.Type
                                };
                                cache[q] = answer;
                                questionLastTimeRequested[q] = DateTime.Now;
                            }
                    }
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
            result.Questions.Add(question);
            return result;
        }

        private static DnsMessage AskForwarder(byte[] packet, IPAddress nameserver)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { ReceiveTimeout = DefaultTimeout, SendTimeout = DefaultTimeout })
            {
                socket.Connect(nameserver, DefaultDnsPort);
                socket.Send(packet);
                var data = new byte[DefaultRecievePacketSize];
                socket.Poll(DefaultTimeout * 1000 * 1000, SelectMode.SelectRead);
                var length = socket.Receive(data);
                return DnsMessage.Parse(data.Take(length).ToArray());
            }
        }
    }
}