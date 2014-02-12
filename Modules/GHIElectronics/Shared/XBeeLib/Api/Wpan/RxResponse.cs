namespace NETMF.OpenSource.XBee.Api.Wpan
{
    /// <summary>
    /// Series 1 XBee.  
    /// Common elements of 16 and 64 bit Address Receive packets.
    /// </summary>
    public class RxResponse : RxResponseBase
    {
        public byte[] Payload { get; set; }

        public override void Parse(IPacketParser parser)
        {
            Source = parser.ApiId == ApiId.Rx16Response
                         ? (XBeeAddress) parser.ParseAddress16()
                         : parser.ParseAddress64();

            base.Parse(parser);

            Payload = parser.ReadRemainingBytes();
        }

        public override string ToString()
        {
            return base.ToString()
                   + ",payload=byte[" + Payload.Length + "]";
        }
    }
}