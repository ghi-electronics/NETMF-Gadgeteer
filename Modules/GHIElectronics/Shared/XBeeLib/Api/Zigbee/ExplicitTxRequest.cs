using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api.Zigbee
{
    /// <summary>
    /// Series 2 XBee.  Sends a packet to a remote radio.  
    /// The remote radio receives the packet as a ExplicitRxResponse packet.
    /// API ID: 0x11
    /// </summary>
    public class ExplicitTxRequest : TxRequest
    {
        public enum Endpoint
        {
            ZigbeeDeviceObject = 0,
            Command = 0xe6,
            Data = 0xe8
        }

        public byte SourceEndpoint { get; set; }
        public byte DestinationEndpoint { get; set; }
        public ushort ClusterId { get; set; }
        public ushort ProfileId { get; set; }

        public ExplicitTxRequest(XBeeAddress destination, byte[] payload, byte srcEndpoint, byte destEndpoint, ushort clusterId, ushort profileId)
            : this(payload, srcEndpoint, destEndpoint, clusterId, profileId)
        {
            if (destination is XBeeAddress16)
            {
                DestinationSerial = XBeeAddress64.Broadcast;
                DestinationAddress = (XBeeAddress16)destination;
            }
            else
            {
                DestinationSerial = (XBeeAddress64)destination;
                DestinationAddress = XBeeAddress16.Unknown;
            }
        }

        public ExplicitTxRequest(XBeeAddress64 destSerial, XBeeAddress16 destAddress, byte[] payload, byte srcEndpoint, byte destEndpoint, ushort clusterId, ushort profileId)
            : this(payload, srcEndpoint, destEndpoint, clusterId, profileId)
        {
            DestinationSerial = destSerial;
            DestinationAddress = destAddress;
        }

        protected ExplicitTxRequest(byte[] payload, byte srcEndpoint, byte destEndpoint, ushort clusterId, ushort profileId)
            : base(payload)
        {
            SourceEndpoint = srcEndpoint;
            DestinationEndpoint = destEndpoint;
            ClusterId = clusterId;
            ProfileId = profileId;
        }

        protected override void GetFrameHeader(OutputStream output)
        {
            base.GetFrameHeader(output);

            output.Write(SourceEndpoint);
            output.Write(DestinationEndpoint);
            output.Write(ClusterId);
            output.Write(ProfileId);
        }

        public override ApiId ApiId
        {
            get { return ApiId.ZnetExplicitTxRequest; }
        }

        public override string ToString()
        {
            return base.ToString() +
                ",srcEndpoint=" + ByteUtils.ToBase16(SourceEndpoint) +
                ",dstEndpoint=" + ByteUtils.ToBase16(DestinationEndpoint) +
                ",cluster=" + ByteUtils.ToBase16(ClusterId) +
                ",profile=" + ByteUtils.ToBase16(ProfileId);
        }
    }
}