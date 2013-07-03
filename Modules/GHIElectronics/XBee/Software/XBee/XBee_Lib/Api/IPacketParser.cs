namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    ///   TODO: Update Comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public interface IPacketParser
    {
        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        byte Read();

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        byte Read(string context);

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        byte[] ReadRemainingBytes();

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        int FrameDataBytesRead { get; }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        int RemainingBytes { get; }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        int BytesRead { get; }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        ushort Length { get; }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        ApiId ApiId { get; }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
	      XBeeAddress16 ParseAddress16();

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
	      XBeeAddress64 ParseAddress64();
    }
}