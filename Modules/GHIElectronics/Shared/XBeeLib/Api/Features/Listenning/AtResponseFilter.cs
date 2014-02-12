namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public class AtResponseFilter : PacketIdFilter
    {
        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        private readonly ushort _atCmd;

        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        private bool _finished;

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="atCommand" type="ushort">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <param name="packetId" type="int">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public AtResponseFilter(ushort atCommand, int packetId)
            : base(packetId)
        {
            _atCmd = atCommand;
        }


        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="atRequest" type="NETMF.OpenSource.XBee.Api.AtCommand">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public AtResponseFilter(AtCommand atRequest)
            : this(atRequest.Command, atRequest.FrameId)
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

            if (!(packet is AtResponse))
                return false;

            var accepted = (packet as AtResponse).Command == _atCmd;

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