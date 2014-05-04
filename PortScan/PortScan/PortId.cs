using System;

namespace PortScan
{
    internal struct PortId
    {
        public TransportProtocol TransportProtocol { get; private set; }
        public int PortNumber { get; private set; }

        public PortId(TransportProtocol transportProtocol, int portNumber) : this()
        {
            TransportProtocol = transportProtocol;
            PortNumber = portNumber;
        }

        public override string ToString()
        {
            return String.Format("{0}/{1}", TransportProtocol, PortNumber);
        }
    }
}