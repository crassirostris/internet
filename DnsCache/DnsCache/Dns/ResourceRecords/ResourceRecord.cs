using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnsCache.Dns.ResourceRecords
{
    internal class ResourceRecord
    {
        private byte[] data = new byte[0];
        public string Name { get; set; }
        public ResourceRecordType Type { get; set; }
        public ResourceRecordClass Class { get; set; }
        public uint Ttl { get; set; }
        public int DataLength { get; set; }

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        public static ResourceRecord Parse(byte[] data, ref int offset)
        {
            var result = new ResourceRecord();
            result.Name = ReadName(data, ref offset);
            result.Type = (ResourceRecordType) BitConverter.ToUInt16(data.Skip(offset).Take(2).Reverse().ToArray(), 0);
            if (result.Type == ResourceRecordType.OPT)
                return result;
            result.Class = (ResourceRecordClass) BitConverter.ToUInt16(data.Skip(offset += 2).Take(2).Reverse().ToArray(), 0);
            result.Ttl = BitConverter.ToUInt32(data.Skip(offset += 2).Take(4).Reverse().ToArray(), 0);
            result.DataLength = BitConverter.ToUInt16(data.Skip(offset += 4).Take(2).Reverse().ToArray(), 0);
            result.Data = data.Skip(offset += 2).Take(result.DataLength).ToArray();
            offset += result.DataLength;
            return result;
        }

        public byte[] GetBytes()
        {
            return NameToBytes()
                .Concat(BitConverter.GetBytes((ushort)Type).Reverse())
                .Concat(BitConverter.GetBytes((ushort)Class).Reverse())
                .Concat(BitConverter.GetBytes(Ttl).Reverse())
                .Concat(BitConverter.GetBytes((ushort)DataLength).Reverse())
                .Concat(Data)
                .ToArray();

        }

        private IEnumerable<byte> NameToBytes()
        {
            var result = Enumerable.Empty<byte>();
            foreach (var chunk in Name.Split(new[] { '.' }))
            {
                result = result.Concat(new[] { (byte)chunk.Length });
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
                        name.Append((char) data[pointer + i]);
                    name.Append('.');
                }
                if (length >> 6 == 3)
                    pointer = ((length & 0x3F) << 8) + data[pointer];
            }
            while ((length = data[offset++]) > 0)
            {
                if (length >> 6 > 0)
                {
                    ++offset;
                    break;
                }
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
            var other = (ResourceRecord) obj;
            return Name == other.Name 
                && Type == other.Type
                && Class == other.Class
                && DataLength == other.DataLength
                && Data.SequenceEqual(other.Data);
            
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Type.GetHashCode() ^ Class.GetHashCode() ^ Ttl.GetHashCode();
        }
    }
}