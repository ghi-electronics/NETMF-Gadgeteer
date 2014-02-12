namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// RF module status messages are sent from the module in response to specific conditions.
    /// API ID: 0x8a
    /// </summary>
    public class ModemStatusResponse : XBeeResponse, INoRequestResponse
    {
        public ModemStatus Status { get; set; }

        public override void Parse(IPacketParser parser)
        {
            var value = parser.Read("Modem Status");

            Status = value < (byte)ModemStatus.StackError 
                ? (ModemStatus)value 
                : ModemStatus.StackError;
        }

        public override string ToString()
        {
            return base.ToString()
                   + ", status=" + Status;
        }
    }
}