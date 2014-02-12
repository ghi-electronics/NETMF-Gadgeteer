using System;

namespace NETMF.OpenSource.XBee.Api.Zigbee
{
    /// <summary>
    /// Series 2 XBee.  This is sent out the UART of the transmitting XBee immediately following
    /// a Transmit packet.  Indicates if the Transmit packet (TxRequest) was successful.
    /// </summary>
    public class TxStatusResponse : XBeeFrameIdResponse
    {
        public enum DeliveryResult
        {
            Success = 0x00,

            /// <summary>
            /// MAC ACK Failure.
            /// </summary>
            MacAckFailure = 0x01,

            /// <summary>
            /// CCA Failure.
            /// </summary>
            CcaFailure = 0x02,

            InvalidDestinationEndpoint = 0x15,
            NetworkAckFailure = 0x21,
            NotJoinedToNetwork = 0x22,
            SelfAddressed = 0x23,
            AddressNotFound = 0x24,
            RouteNotFound = 0x25,

            /// <summary>
            /// Broadcast source failed to hear a neighbor relay the message.
            /// </summary>
            BroadcastFailed = 0x26,

            /// <summary>
            /// Invalid binding table index.
            /// </summary>
            InvalidBinding = 0x2B,

            /// <summary>
            /// Lack of free buffers, timers, etc.
            /// </summary>
            ResourceError = 0x2C,

            /// <summary>
            /// Attempted broadcast with APS transmission.
            /// </summary>
            AttemptedBroadcast = 0x2D,

            /// <summary>
            /// Attempted unicast with APS transmission, but EE=0.
            /// </summary>
            AttemptedUnicast = 0x2E,

            /// <summary>
            /// Lack of free buffers, timers, etc.
            /// </summary>
            ResourceError2 = 0x32,

            /// <summary>
            /// Data payload too large.
            /// </summary>
            PayloadTooLarge = 0x74,

            /// <summary>
            /// Indirect message unrequested.
            /// </summary>
            MessageUnrequested = 0x75
        }

        [Flags]
        public enum DiscoveryResult
        {
            NoOverhead = 0x00,
            Address = 0x01,
            Route = 0x02,
            ExtendedTimeout = 0x40
        }

        public XBeeAddress16 DestinationAddress { get; set; }
        public byte RetryCount { get; set; }
        public DeliveryResult DeliveryStatus { get; set; }
        public DiscoveryResult DiscoveryStatus { get; set; }

        /// <summary>
        /// Returns true if the delivery status is SUCCESS
        /// </summary>
        public bool IsSuccess { get { return DeliveryStatus == DeliveryResult.Success; } }

        public override void Parse(IPacketParser parser)
        {
            base.Parse(parser);
            DestinationAddress = parser.ParseAddress16();
            RetryCount = parser.Read("ZNet Tx Status Tx Count");
            DeliveryStatus = (DeliveryResult) parser.Read("ZNet Tx Status Delivery Status");
            DiscoveryStatus = (DiscoveryResult) parser.Read("ZNet Tx Status Discovery Status");
        }

        public override string ToString()
        {
            return base.ToString() +
            ",destinationAddress=" + DestinationAddress +
            ",retryCount=" + RetryCount +
            ",deliveryStatus=" + DeliveryStatus +
            ",discoveryStatus=" + DiscoveryStatus;
        }
    }
}