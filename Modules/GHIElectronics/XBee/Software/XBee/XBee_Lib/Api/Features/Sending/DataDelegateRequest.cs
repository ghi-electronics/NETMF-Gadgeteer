namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public class DataDelegateRequest : DataRequest
    {
        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        private XBeeApi.PayloadDelegate _bytesDelegate;

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
        public override byte[] Payload
        {
            get { return _bytesDelegate.Invoke(); }
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="xbee" type="NETMF.OpenSource.XBee.Api.XBee">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public DataDelegateRequest(XBeeApi xbee) 
            : base(xbee)
        {
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="payloadDelegate" type="NETMF.OpenSource.XBee.Api.XBee.PayloadDelegate">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        internal void Init(XBeeApi.PayloadDelegate payloadDelegate)
        {
            Init();
            _bytesDelegate = payloadDelegate;
        }
    }
}