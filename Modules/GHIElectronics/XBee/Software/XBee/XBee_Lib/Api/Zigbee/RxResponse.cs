namespace NETMF.OpenSource.XBee.Api.Zigbee
{
    /// <summary>
    /// Series 2 XBee. This packet is received when a remote XBee sends a TxRequest
    /// API ID: 0x90
    /// </summary>
    /// <remarks>
    /// ZNet RX packets do not include RSSI since it is a mesh network and potentially requires several
    /// hops to get to the destination.  The RSSI of the last hop is available using the DB AT command.
    /// If your network is not mesh (i.e. composed of a single coordinator and end devices -- no routers) 
    /// then the DB command should provide accurate RSSI.
    /// </remarks>
    public class RxResponse : RxResponseBase, INoRequestResponse
    {
        public byte[] Payload { get; set; }

        protected override void ParseFramePayload(IPacketParser parser)
        {
            Payload = parser.ReadRemainingBytes();	
        }

        public override string ToString()
        {
            return base.ToString()
                   + ",payload=byte[" + Payload.Length + "]";
        }
    }
}