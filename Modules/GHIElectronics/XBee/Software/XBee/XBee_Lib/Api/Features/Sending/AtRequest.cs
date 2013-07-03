namespace NETMF.OpenSource.XBee.Api
{

    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public class AtRequest : RequestBase
    {
        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        protected ushort AtCommand;

        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        /// <remarks>
        ///     
        /// </remarks>
        protected byte[] Value;

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="localXBee" type="NETMF.OpenSource.XBee.Api.XBee">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public AtRequest(XBeeApi localXBee) 
            : base(localXBee)
        {
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="atCommand" type="ushort">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <param name="value" type="byte[]">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        internal void Init(ushort atCommand, params byte[] value)
        {
            Init();
            AtCommand = atCommand;
            Value = value;
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <returns>
        ///     A NETMF.OpenSource.XBee.Api.XBeeRequest value...
        /// </returns>
        protected override XBeeRequest CreateRequest()
        {
            // if address other than null or local XBee serial number was provided
            // the AT command will be sent to remote node

            if (DestinationNode != null)
                return LocalXBee.CreateRequest(AtCommand, DestinationNode, Value);

            return DestinationAddress == null
                ? LocalXBee.CreateRequest(AtCommand, Value)
                : LocalXBee.CreateRequest(AtCommand, DestinationAddress, Value);
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="responseHandler" type="NETMF.OpenSource.XBee.Api.AtResponseHandler">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public void Invoke(AtResponseHandler responseHandler)
        {
            Invoke((response, finished) => 
                responseHandler.Invoke(response as AtResponse));
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <returns>
        ///     A NETMF.OpenSource.XBee.Api.AtResponse value...
        /// </returns>
        public new AtResponse GetResponse()
        {
            return base.GetResponse() as AtResponse;
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <returns>
        ///     A byte[] value...
        /// </returns>
        public byte[] GetResponsePayload()
        {
            return GetResponse().Value;
        }
    }
}
