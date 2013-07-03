namespace NETMF.OpenSource.XBee.Api
{

  /// <summary>
  ///   TODO: Update Comments
  ///     
  /// </summary>
  /// <remarks>
  ///     
  /// </remarks>
    public interface IPacketFilter
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
        bool Accepted(XBeeResponse packet);


        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <returns>
        ///     A bool value...
        /// </returns>
        bool Finished();
    }
}