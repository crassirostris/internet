﻿using PortScan.Common;

namespace PortScan.Scanning
{
    internal class NtpScanner : ApplicationLayerPortScanner
    {
        private static readonly PortId[] portIds =
        {
            new PortId(TransportProtocol.Udp, 123),
        };

        private static readonly byte[] queryPacket =
        {
            0xdb, 0x00, 0x0a, 0xfa, 0x00, 0x00, 0x13, 0xb9, 0x00, 0x08, 0xca, 0xb2, 0x00, 0x00, 0x00, 0x00,
            0xd7, 0x10, 0xbb, 0x61, 0x5a, 0xc0, 0xb8, 0xc2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xd7, 0x10, 0xbb, 0x65, 0xda, 0xc0, 0xb8, 0xc2,
        };

        public override PortId[] PortIds
        {
            get { return portIds; }
        }

        public override byte[] QueryPacket
        {
            get { return queryPacket; }
        }

        public override ApplicationProtocol Protocol
        {
            get { return ApplicationProtocol.Ntp; }
        }
    }
}