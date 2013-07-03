namespace NETMF.OpenSource.XBee.Api.Wpan
{
    /// <summary>
    /// When a TX Request is completed, the module sends a TX Status message. 
    /// This message will indicate if the packet was transmitted successfully or if there was a failure.
    /// API Identifier Value: 0x89
    /// </summary>
    public class TxStatusResponse : XBeeFrameIdResponse
    {
        public enum TxStatus
        {
		    Success = 0,
		    NoAck = 1,
		    CcaFailure = 2,
		    Purged = 3
        }

        public TxStatus Status { get; set; }

        public override void Parse(IPacketParser parser)
        {
            base.Parse(parser);
            Status = (TxStatus) parser.Read("TX Status");
        }
    }
}