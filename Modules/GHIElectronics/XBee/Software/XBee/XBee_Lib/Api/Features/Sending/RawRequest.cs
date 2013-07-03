namespace NETMF.OpenSource.XBee.Api
{

    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public class RawRequest : RequestBase
    {
        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        /// <remarks>
        ///     
        /// </remarks>
        protected XBeeRequest Request;

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="xbee" type="NETMF.OpenSource.XBee.Api.XBee">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public RawRequest(XBeeApi xbee)
            : base(xbee)
        {
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
        internal void Init(XBeeRequest request)
        {
            Init();
            Request = request;
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
            if (Request.FrameId == PacketIdGenerator.NoResponseId)
                ExpectedResponse = Response.None;

            return Request;
        }
    }
}
