namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public class AddressPacketFilter : IPacketFilter
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
            return packet is RemoteAtResponse
                || packet is Zigbee.RxResponse
                || packet is Zigbee.TxStatusResponse
                || packet is Zigbee.NodeIdentificationResponse;
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