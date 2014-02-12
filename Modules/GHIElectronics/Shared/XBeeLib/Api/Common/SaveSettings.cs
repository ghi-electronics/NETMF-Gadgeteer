namespace NETMF.OpenSource.XBee.Api.Common
{
    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public static class SaveSettings
    {

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="xbee" type="NETMF.OpenSource.XBee.Api.XBee">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public static void Write(XBeeApi xbee)
        {
            var request = xbee.Send(AtCmd.Write);
            Parse(request.GetResponse());
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="sender" type="NETMF.OpenSource.XBee.Api.XBee">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <param name="remoteXbee" type="NETMF.OpenSource.XBee.Api.XBeeAddress">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public static void Write(XBeeApi sender, XBeeAddress remoteXbee)
        {
            var request = sender.Send(AtCmd.Write).To(remoteXbee);
            Parse((AtResponse) request.GetResponse());
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="response" type="NETMF.OpenSource.XBee.Api.AtResponse">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public static void Parse(AtResponse response)
        {
            if (!response.IsOk)
                throw new XBeeException("Failed to save settings");
        }
    }
}