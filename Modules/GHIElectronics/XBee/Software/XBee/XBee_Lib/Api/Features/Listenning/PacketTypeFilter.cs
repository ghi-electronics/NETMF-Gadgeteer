using System;

namespace NETMF.OpenSource.XBee.Api
{
    public class PacketTypeFilter : IPacketFilter
    {
        private readonly Type _expectedType;
        
        public PacketTypeFilter(Type expectedType)
        {
            if (expectedType == null)
                throw new ArgumentException("expectedType needs to be specified");

            _expectedType = expectedType;
        }

        public virtual bool Accepted(XBeeResponse packet)
        {
            return _expectedType.IsInstanceOfType(packet);
        }

        public virtual bool Finished()
        {
            return false;
        }
    }
}