using System;
using System.Collections.Generic;
using System.Linq;
using DnsCache.Dns.ResourceRecords;

namespace DnsCache.Dns
{
    internal class DnsMessage
    {
        private byte[] bytes;
        private List<QuestionResourceRecord> questions;
        private List<ResourceRecord> answers;
        private List<ResourceRecord> authority;
        private List<ResourceRecord> additional;
        public ushort Id { get; set; }
        public DnsMessageType Type { get; set; }
        public DnsMessageOpcode Opcode { get; set; }
        public bool IsAuthoritativeAnswer { get; set; }
        public bool IsTruncated { get; set; }
        public bool IsRecursionDesired { get; set; }
        public bool IsRecursionAvaliable { get; set; }
        public DnsMessageResponseCode ResponseCode { get; set; }
        public ushort QuestionsCount { get; set; }
        public ushort AnswersCount { get; set; }
        public ushort AuthorityCount { get; set; }
        public ushort AdditionalCount { get; set; }

        public List<QuestionResourceRecord> Questions
        {
            get { return questions ?? (questions = new List<QuestionResourceRecord>()); }
            set { questions = value; }
        }

        public List<ResourceRecord> Answers
        {
            get { return answers ?? (answers = new List<ResourceRecord>()); }
            set { answers = value; }
        }

        public List<ResourceRecord> Authority
        {
            get { return authority ?? (authority = new List<ResourceRecord>()); }
            set { authority = value; }
        }

        public List<ResourceRecord> Additional
        {
            get { return additional ?? (additional = new List<ResourceRecord>()); }
            set { additional = value; }
        }

        public byte[] Bytes
        {
            get { return bytes ?? (bytes = GetBytes()); }
            set { bytes = value; }
        }

        public DnsMessage()
        {
            IsRecursionDesired = true;
        }

        public static DnsMessage Parse(byte[] data)
        {
            var result = new DnsMessage();
            int offset = 0;
            result.bytes = data;
            result.Id = BitConverter.ToUInt16(data.Skip(offset).Take(2).Reverse().ToArray(), 0);

            var flags = BitConverter.ToUInt16(data.Skip(offset += 2).Take(2).Reverse().ToArray(), 0);
            result.Type = (DnsMessageType) (flags >> 15);
            result.Opcode = (DnsMessageOpcode) ((flags >> 11) & 0xF);
            result.IsAuthoritativeAnswer = (flags & (1 << 10)) > 0;
            result.IsTruncated = (flags & (1 << 9)) > 0;
            result.IsTruncated = (flags & (1 << 9)) > 0;
            result.IsRecursionDesired = (flags & (1 << 8)) > 0;
            result.IsRecursionAvaliable = (flags & (1 << 7)) > 0;
            result.ResponseCode = (DnsMessageResponseCode) (flags & 0xF);

            result.QuestionsCount = BitConverter.ToUInt16(data.Skip(offset += 2).Take(2).Reverse().ToArray(), 0);

            result.AnswersCount = BitConverter.ToUInt16(data.Skip(offset += 2).Take(2).Reverse().ToArray(), 0);

            result.AuthorityCount = BitConverter.ToUInt16(data.Skip(offset += 2).Take(2).Reverse().ToArray(), 0);

            result.AdditionalCount = BitConverter.ToUInt16(data.Skip(offset += 2).Take(2).Reverse().ToArray(), 0);

            offset += 2;

            for (int i = 0; i < result.QuestionsCount; i++)
                result.Questions.Add(QuestionResourceRecord.Parse(data, ref offset));
            for (int i = 0; i < result.AnswersCount; i++)
                result.Answers.Add(ResourceRecord.Parse(data, ref offset));
            for (int i = 0; i < result.AuthorityCount; i++)
                result.Authority.Add(ResourceRecord.Parse(data, ref offset));
            for (int i = 0; i < result.AdditionalCount; i++)
            {
                var record = ResourceRecord.Parse(data, ref offset);
                if (record.Type == ResourceRecordType.OPT)
                {
                    result.AdditionalCount = (ushort) i;
                    break;
                }
                result.Additional.Add(record);
            }

            return result;
        }

        public byte[] GetBytes()
        {
            var result = Enumerable.Empty<byte>();
            result = result.Concat(BitConverter.GetBytes(Id).Reverse());

            ushort flags = 0;
            flags = (ushort)((flags) | ((ushort)Type) << 1);
            flags = (ushort)((flags) | ((ushort)Opcode) << 4);
            flags = (ushort)((flags) | (IsAuthoritativeAnswer ? 1 : 0) << 1);
            flags = (ushort)((flags) | (IsTruncated ? 1 : 0) << 1);
            flags = (ushort)((flags) | (IsRecursionDesired ? 1 : 0) << 1);
            flags = (ushort)((flags) | (IsRecursionAvaliable ? 1 : 0) << 1);
            flags = (ushort)((flags) | (0) << 3);
            flags = (ushort)((flags) | (ushort) ResponseCode);
            result = result.Concat(BitConverter.GetBytes(flags).Reverse());

            result = result.Concat(BitConverter.GetBytes(QuestionsCount).Reverse());
            result = result.Concat(BitConverter.GetBytes(AnswersCount).Reverse());
            result = result.Concat(BitConverter.GetBytes(AuthorityCount).Reverse());
            result = result.Concat(BitConverter.GetBytes(AdditionalCount).Reverse());

            result = Questions.Aggregate(result, (current, record) => current.Concat(record.GetBytes()));
            result = Answers.Aggregate(result, (current, record) => current.Concat(record.GetBytes()));
            result = Authority.Aggregate(result, (current, record) => current.Concat(record.GetBytes()));
            result = Additional.Aggregate(result, (current, record) => current.Concat(record.GetBytes()));

            return result.ToArray();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DnsMessage))
                return false;
            if (obj == this)
                return true;
            return GetBytes().SequenceEqual((obj as DnsMessage).GetBytes());
        }

        public override int GetHashCode()
        {
            return GetBytes().Aggregate(0, (i, b) => (i << 8) ^ b ^ (i >> 24));
        }

        public static bool TryPase(byte[] packet, out DnsMessage message)
        {
            message = default (DnsMessage);
            try
            {
                message = Parse(packet);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
