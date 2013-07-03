namespace NETMF.OpenSource.XBee.Api.Common
{
    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public abstract class DiscoverResult
    {
        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <value>
        ///     <para>
        ///         
        ///     </para>
        /// </value>
        /// <remarks>
        ///     
        /// </remarks>
        public NodeInfo NodeInfo { get; set; }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <returns>
        ///     A string value...
        /// </returns>
        public override string ToString()
        {
            return "address=" + NodeInfo.NetworkAddress
                   + ", serial=" + NodeInfo.SerialNumber
                   + ", id=" + NodeInfo.NodeIdentifier;
        }
    }
}