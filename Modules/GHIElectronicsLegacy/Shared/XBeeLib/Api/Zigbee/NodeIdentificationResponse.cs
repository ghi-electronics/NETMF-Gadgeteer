using NETMF.OpenSource.XBee.Api.Common;
using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api.Zigbee
{
    /// <summary>
    /// This frame is received when a module transmits a node identification message to identify itself (when AO=0).
    /// The data portion of this frame is similar to a network discovery response frame (see <see cref="DiscoverResult"/>).
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// If the commissioning push button is pressed on a remote router device with 64-bit address 0x0013A20040522BAA, 
    /// 16-bit address 0x7D84, and default NI string, the preceding node identification indicator would be received. 
    /// Please note that 00 03 00 00 appears before the checksum with the DD value only if ATNO & 0x01.
    /// ]]>
    /// </example>
    public class NodeIdentificationResponse : XBeeResponse
    {
        public enum PacketOption
        {
            /// <summary>
            /// Packet Acknowledged
            /// </summary>
            Ack = 0x01,

            /// <summary>
            /// Packet was a broadcast packet
            /// </summary>
            Broadcast = 0x02
        }

        public enum SourceActions
        {
            /// <summary>
            /// Frame sent by node identific ation pushbutton event (see D0 command)
            /// </summary>
            Pushbutton = 1,

            /// <summary>
            /// Frame sent after joining event occurred (see <see cref="AtCmd.JoinNotification"/>).
            /// </summary>
            Joining = 2,

            /// <summary>
            /// Frame sent after power cycle event occurred (see <see cref="AtCmd.JoinNotification"/>).
            /// </summary>
            PowerCycle = 3
        }

        /// <summary>
        /// Serial and network address of node that transmited this packet
        /// this will be equal to remote serial and address if there were
        /// not hops in between.
        /// </summary>
        public NodeInfo Sender { get; set; }

        /// <summary>
        /// Serial and netowork address of remote node that was identified
        /// </summary>
        public NodeInfo Remote { get; set; }

        public PacketOption Option { get; set; }
        
        // these properties are all regarding the remote node
        public string NodeIdentifier { get; set; }
        public XBeeAddress16 ParentAddress { get; set; }
        public NodeType NodeType { get; set; }
        public SourceActions SourceAction { get; set; }

        /// <summary>
        /// Set to Digi's application profile ID.
        /// </summary>
        public ushort ProfileId { get; set; }

        /// <summary>
        /// Set to Digi's Manufacturer ID.
        /// </summary>
        public ushort MfgId { get; set; }

        public override void Parse(IPacketParser parser)
        {
            Sender = new NodeInfo
            {
                SerialNumber = parser.ParseAddress64(),
                NetworkAddress = parser.ParseAddress16()
            };

            Option = (PacketOption) parser.Read("Option");

            Remote = new NodeInfo
            {
                NetworkAddress = parser.ParseAddress16(), 
                SerialNumber = parser.ParseAddress64()
            };

            byte ch;

            // NI is terminated with 0
            while ((ch = parser.Read("Node Identifier")) != 0)
            {
                if (ch > 32 && ch < 126)
                    Remote.NodeIdentifier += (char) ch;
            }

            ParentAddress = parser.ParseAddress16();
            NodeType = (NodeType) parser.Read("Device Type");
            SourceAction = (SourceActions) parser.Read("Source Action");
            ProfileId = UshortUtils.ToUshort(parser.Read("Profile MSB"), parser.Read("Profile LSB"));
            MfgId = UshortUtils.ToUshort(parser.Read("MFG MSB"), parser.Read("MFG LSB"));
        }

        public override string ToString()
        {
            return base.ToString()
                   + ", sender=" + Sender
                   + ", remote=" + Remote
                   + ", nodeType=" + NodeType
                   + ", mfgId=" + MfgId
                   + ", nodeIdentifier=" + NodeIdentifier
                   + ", option=" + Option
                   + ", parentAddress=" + ParentAddress
                   + ", profileId=" + ProfileId
                   + ", sourceAction=" + SourceAction;
        }
    }
}