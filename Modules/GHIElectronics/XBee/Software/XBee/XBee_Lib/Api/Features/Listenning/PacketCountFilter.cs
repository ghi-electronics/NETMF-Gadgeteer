using System;

namespace NETMF.OpenSource.XBee.Api
{
    public class PacketCountFilter : PacketTypeFilter
    {
        private int _expectedCount;
        private bool _finished;

        public PacketCountFilter(int expectedCount, Type expectedType)
            : base(expectedType)
        {
            _expectedCount = expectedCount;
        }

        public override bool Accepted(XBeeResponse packet)
        {
            if (!base.Accepted(packet))
                return false;

            if (_expectedCount == 0)
                return false;

            _expectedCount--;
            _finished = _expectedCount == 0;
            return true;
        }

        public override bool Finished()
        {
            return _finished;
        }
    }
}