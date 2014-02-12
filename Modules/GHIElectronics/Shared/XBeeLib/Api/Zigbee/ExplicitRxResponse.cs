using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api.Zigbee
{
    /// <summary>
    /// Series 2 XBee. This packet is received when a remote XBee sends a ExplicitTxRequest
    /// Radio must be configured for explicit frames to use this class (AO=1)
    /// </summary>
    public class ExplicitRxResponse : RxResponse
    {
        public byte SourceEndpoint { get; set; }
        public byte DestinationEndpoint { get; set; }
        public ushort ClusterId { get; set; }
        public ushort ProfileId { get; set; }

        protected override void ParseFrameHeader(IPacketParser parser)
        {
            base.ParseFrameHeader(parser);

            SourceEndpoint = parser.Read("Reading Source Endpoint");
            DestinationEndpoint = parser.Read("Reading Destination Endpoint");
            ClusterId = UshortUtils.ToUshort(parser.Read("Reading Cluster Id MSB"), parser.Read("Reading Cluster Id LSB"));
            ProfileId = UshortUtils.ToUshort(parser.Read("Reading Profile Id MSB"), parser.Read("Reading Profile Id LSB"));
        }

        public override string ToString()
        {
            return base.ToString() +
                   ",srcEndpoint=" + ByteUtils.ToBase16(SourceEndpoint) +
                   ",dstEndpoint=" + ByteUtils.ToBase16(DestinationEndpoint) +
                   ",clusterId=" + ByteUtils.ToBase16(ClusterId) +
                   ",profileId=" + ByteUtils.ToBase16(ProfileId);
        }
    }
}