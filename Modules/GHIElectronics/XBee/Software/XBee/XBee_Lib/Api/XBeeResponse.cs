using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// The super class of all XBee Receive packets
    /// </summary>
    public abstract class XBeeResponse
    {
        // the raw (escaped) bytes of this packet (minus start byte)
        // this is the most compact representation of the packet;
        // useful for sending the packet over a wire (e.g. xml),
        // for later reconstitution
        private byte[] _rawPacketBytes;
        private byte[] _processedPacketBytes;

        public ushort Length { get; set; }
        public ApiId ApiId { get; set; }
        public byte Checksum { get; set; }

        /// <summary>
        /// Indicates an error occurred during the parsing of the packet.
        /// This may indicate a bug in this software or in the XBee firmware.
        /// Absence of an error does not indicate the request was successful
        /// you will need to inspect the status byte of the response object 
        /// (if available) to determine success.
        /// </summary>
        public bool Error { get; set; }

        /// <summary>
        /// Returns an array all bytes (as received off radio, including escape bytes)
        /// in packet except the start byte.  
        /// </summary>
        public byte[] RawPacketBytes
        {
            get { return _rawPacketBytes; }
            set
            {
                _rawPacketBytes = value;
                _processedPacketBytes = XBeePacket.UnEscapePacket(value);
            }
        }

        /// <summary>
        /// For internal use only.  Called after successful parsing 
        /// to allow subclass to do any final processing before delivery
        /// </summary>
        public virtual void Finish()
        {

        }

        /// <summary>
        /// All subclasses must implement to parse the packet from the input stream.
        /// The subclass must parse all bytes in the packet starting after the API_ID, and
        /// up to but not including the checksum.  Reading either more or less bytes that expected will
        /// result in an error.
        /// </summary>
        /// <param name="parser"></param>
        public abstract void Parse(IPacketParser parser);

        public bool Equals(XBeeResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Arrays.AreEqual(other.RawPacketBytes, RawPacketBytes)
                && Arrays.AreEqual(other._processedPacketBytes, _processedPacketBytes) 
                && Equals(other.Length, Length) 
                && Equals(other.ApiId, ApiId) 
                && other.Checksum == Checksum 
                && other.Error.Equals(Error);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (XBeeResponse)) return false;
            return Equals((XBeeResponse) obj);
        }

        public override string ToString()
        {
            return "ApiId=" + ByteUtils.ToBase16((byte)ApiId) +
                   ",Length=" + ByteUtils.ToBase16(Length) +
                   ",Checksum=" + ByteUtils.ToBase16(Checksum) +
                   ",Error=" + Error;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}