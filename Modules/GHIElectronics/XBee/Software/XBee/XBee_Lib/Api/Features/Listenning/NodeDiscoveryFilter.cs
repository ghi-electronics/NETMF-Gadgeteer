namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// TODO: Update comments    
    /// </summary>
    public class NodeDiscoveryFilter : AtResponseFilter
    {
        private bool _finished;
        /// <summary>
        /// Constructor.
        /// TODO: Update comments    
        /// </summary>      
        public NodeDiscoveryFilter(int packetId = PacketIdGenerator.DefaultId)
            : base((ushort) Common.AtCmd.NodeDiscover, packetId)
        {
        }

        public override bool Accepted(XBeeResponse packet)
        {
            if (!base.Accepted(packet))
                return false;

            var atResponse = (AtResponse)packet;

            // empty response is received in series 1 modules
            // in series 2 the timeout determines the end of discovery
            if (atResponse.Value == null || atResponse.Value.Length == 0)
            {
                _finished = true;
                return false;
            }

            return true;
        }

        public override bool Finished()
        {
            return _finished;
        }
    }
}