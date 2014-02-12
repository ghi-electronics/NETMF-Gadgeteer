namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    ///   TODO: Update Comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public interface IPacketListener
    {
        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        bool Finished { get; }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        void ProcessPacket(XBeeResponse packet);

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        XBeeResponse[] GetPackets(int timeout);
    }
}