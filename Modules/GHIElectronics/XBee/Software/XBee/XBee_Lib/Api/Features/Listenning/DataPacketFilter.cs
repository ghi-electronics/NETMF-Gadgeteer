namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public class DataPacketFilter : IPacketFilter
    {
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
        public bool Accepted(XBeeResponse packet)
        {
            return packet is Wpan.RxResponse 
                || packet is Zigbee.RxResponse;
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <returns>
        ///     A bool value...
        /// </returns>
        public bool Finished()
        {
            return false;
        }
    }
}