namespace NETMF.OpenSource.XBee.Api
{

    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public class PacketIdFilter : PacketTypeFilter
    {
        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        /// <remarks>
        ///     
        /// </remarks>
        public int ExpectedPacketId;


        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        private bool _finished;


        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="packetId" type="int">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public PacketIdFilter(int packetId)
            : base(typeof(XBeeFrameIdResponse))
        {
            ExpectedPacketId = packetId;
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="request" type="NETMF.OpenSource.XBee.Api.XBeeRequest">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public PacketIdFilter(XBeeRequest request)
            : this (request.FrameId)
        {
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="packet" type="NETMF.OpenSource.XBee.Api.XBeeResponse">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <returns>
        ///     A bool value...
        /// </returns>
        public override bool Accepted(XBeeResponse packet)
        {
            if (!base.Accepted(packet))
                return false;

            var frameIdResponse = (XBeeFrameIdResponse)packet;

            var accepted = ExpectedPacketId == PacketIdGenerator.DefaultId 
                || frameIdResponse.FrameId == ExpectedPacketId;

            _finished = accepted;
            return accepted;
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <returns>
        ///     A bool value...
        /// </returns>
        public override bool Finished()
        {
            return _finished;
        }
    }
}