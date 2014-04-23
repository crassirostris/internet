using System;
using System.Linq;

namespace SntpServer
{
    public enum Mode : byte
    {
        Reserved,
        SymmetricActive,
        SymmetricPassive,
        Client,
        Server,
        Broadcast,
        NtpControlMessage,
        PrivateUse
    }

    public struct SntpPacket
    {
        public const int MinimumPacketLength = 48;

        public byte Leap { get; set; }
        public byte Version { get; set; }
        public Mode Mode { get; set; }
        public byte Stratum { get; set; }
        public byte Poll { get; set; }
        public byte Precision { get; set; }
        public uint RootDelay { get; set; }
        public uint RootDispersion { get; set; }
        public uint ReferenceId { get; set; }
        public decimal ReferenceTimestamp { get; set; }
        public decimal OriginTimestamp { get; set; }
        public decimal RecieveTimestamp { get; set; }
        public decimal TransmitTimestamp { get; set; }

        private static decimal TimestampFromNtpFormat(ulong timestamp)
        {
            return (decimal) (timestamp) / ((ulong)(1) << 32);
        }

        private static ulong TimestampToNtpFormat(decimal timestamp)
        {
            return (ulong)(timestamp * ((ulong)(1) << 32));
        }

        public static SntpPacket FromBytes(byte[] data)
        {
            return new SntpPacket
            {
                Leap = (byte) (data[0] >> 6),
                Version = (byte) ((data[0] >> 3) & 0x7),
                Mode = (Mode) (data[0] & 0x7),
                Stratum = data[1],
                Poll = data[2],
                Precision = data[3],
                RootDelay = BitConverter.ToUInt32(data.Skip(4).Take(4).Reverse().ToArray(), 0),
                RootDispersion = BitConverter.ToUInt32(data.Skip(8).Take(4).Reverse().ToArray(), 0),
                ReferenceId = BitConverter.ToUInt32(data.Skip(12).Take(4).Reverse().ToArray(), 0),
                ReferenceTimestamp = TimestampFromNtpFormat(BitConverter.ToUInt64(data.Skip(16).Take(8).Reverse().ToArray(), 0)),
                OriginTimestamp = TimestampFromNtpFormat(BitConverter.ToUInt64(data.Skip(32).Take(8).Reverse().ToArray(), 0)),
                RecieveTimestamp   = TimestampFromNtpFormat(BitConverter.ToUInt64(data.Skip(24).Take(8).Reverse().ToArray(), 0)),
                TransmitTimestamp  = TimestampFromNtpFormat(BitConverter.ToUInt64(data.Skip(40).Take(8).Reverse().ToArray(), 0))
            };
        }

        public byte[] ToBytes()
        {
            return new[] { (byte) ((Leap << 6) | (Version << 3) | (byte)(Mode)), Stratum, Poll, Precision }
                .Concat(BitConverter.GetBytes(RootDelay).Reverse())
                .Concat(BitConverter.GetBytes(RootDispersion).Reverse())
                .Concat(BitConverter.GetBytes(ReferenceId).Reverse())
                .Concat(BitConverter.GetBytes(TimestampToNtpFormat(ReferenceTimestamp)).Reverse())
                .Concat(BitConverter.GetBytes(TimestampToNtpFormat(OriginTimestamp)).Reverse())
                .Concat(BitConverter.GetBytes(TimestampToNtpFormat(RecieveTimestamp)).Reverse())
                .Concat(BitConverter.GetBytes(TimestampToNtpFormat(TransmitTimestamp)).Reverse())
                .ToArray();
        }
    }
}
