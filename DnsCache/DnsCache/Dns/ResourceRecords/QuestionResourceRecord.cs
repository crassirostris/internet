using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnsCache.Dns.ResourceRecords
{
    internal class QuestionResourceRecord
    {
        public string Name { get; set; }
        public ResourceRecordType Type { get; set; }
        public ResourceRecordClass Class { get; set; }

        public static QuestionResourceRecord Parse(byte[] data, ref int offset)
        {
            var result = new QuestionResourceRecord();
            result.Name = ReadName(data, ref offset);
            result.Type = (ResourceRecordType)BitConverter.ToUInt16(data.Skip(offset).Take(2).Reverse().ToArray(), 0);
            result.Class = (ResourceRecordClass)BitConverter.ToUInt16(data.Skip(offset += 2).Take(2).Reverse().ToArray(), 0);
            offset += 2;
            return result;
        }

        public byte[] GetBytes()
        {
            return NameToBytes()
                .Concat(BitConverter.GetBytes((ushort)Type).Reverse())
                .Concat(BitConverter.GetBytes((ushort)Class).Reverse())
                .ToArray();

        }

        private IEnumerable<byte> NameToBytes()
        {
            var result = Enumerable.Empty<byte>();
            foreach (var chunk in Name.Split(new[] { '.' }))
            {
                result = result.Concat(new[] { (byte) chunk.Length });
                result = result.Concat(chunk.Select(ch => (byte)ch));
            }
            return result;
        }

        private static string ReadName(byte[] data, ref int offset)
        {
            var name = new StringBuilder();
            int pointer = offset;
            int length;
            while ((length = data[pointer++]) > 0)
            {
                if (length >> 6 == 0)
                {
                    for (int i = 0; i < length; i++)
                        name.Append((char)data[pointer++]);
                    name.Append('.');
                }
                if (length >> 6 == 3)
                    pointer = length & 0x3F;
            }
            while ((length = data[offset++]) > 0)
            {
                if (length >> 6 > 0)
                    break;
                offset += length;
            }
            return name.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var other = (QuestionResourceRecord)obj;
            return Name == other.Name
                   && Type == other.Type
                   && Class == other.Class;

        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Type.GetHashCode() ^ Class.GetHashCode();
        }
    }
}
