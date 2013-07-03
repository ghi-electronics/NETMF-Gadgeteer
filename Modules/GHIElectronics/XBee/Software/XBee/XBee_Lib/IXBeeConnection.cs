namespace NETMF.OpenSource.XBee
{
    /// <summary>
    ///   TODO: Update Comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public interface IXBeeConnection
    {
        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        void Open();

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        void Close();


        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        bool Connected { get; }


        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="data" type="byte[]">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        void Send(byte[] data);


        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="data" type="byte[]">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <param name="offset" type="int">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <param name="count" type="int">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        void Send(byte[] data, int offset, int count);


        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        event DataReceivedEventHandler DataReceived;
    }


    /// <summary>
    ///     
    /// </summary>
    /// <param name="data" type="byte[]">
    ///     <para>
    ///         
    ///     </para>
    /// </param>
    /// <param name="offset" type="int">
    ///     <para>
    ///         
    ///     </para>
    /// </param>
    /// <param name="count" type="int">
    ///     <para>
    ///         
    ///     </para>
    /// </param>
    public delegate void DataReceivedEventHandler(byte[] data, int offset, int count);
}