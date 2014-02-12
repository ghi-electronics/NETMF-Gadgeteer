using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// Supported by both series 1 (10C8 firmware and later) and series 2.
    /// Represents a response, corresponding to a RemoteAtCommand.
    /// API ID: 0x97
    /// </summary>
    public class RemoteAtResponse : AtResponse
    {
        public XBeeAddress64 RemoteSerial { get; set; }
        public XBeeAddress16 RemoteAddress { get; set; }

        public bool IsNetworkAddressKnown
        {
            get { return RemoteAddress != XBeeAddress16.Unknown; }
        }

        public override void Parse(IPacketParser parser)
        {
            FrameId = parser.Read("Frame Id");

            RemoteSerial = parser.ParseAddress64();
            RemoteAddress = parser.ParseAddress16();

            Command = UshortUtils.ToUshort(
                parser.Read("AT Response Char 1"),
                parser.Read("AT Response Char 2"));

            Status = (AtResponseStatus) parser.Read("AT Response Status");
            Value = parser.ReadRemainingBytes();
        }

        public override string ToString()
        {
            return base.ToString()
                   + ",remoteAddress64=" + RemoteSerial
                   + ",remoteAddress16=" + RemoteAddress;
        }
    }
}