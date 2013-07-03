using System;
using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api.Zigbee
{
    /// <summary>
    /// Series 2 XBee.  Sends a packet to a remote radio.
    /// The remote radio receives the data as a RxResponse packet.
    /// API ID: 0x10
    /// </summary>
    public class TxRequest : XBeeRequest, IZigbeePacket
    {
        [Flags]
	    public enum Options
        {
            DisableAck = 0x01,
            EnableEncryption = 0x20,
            ExtendedTimeout = 0x40
	    }

        // 10/28/08 the datasheet states 72 is maximum payload size but I was able to push 75 through successfully, 
        // even with all bytes as escape bytes (a total post-escape packet size of 169!).

        /**
         * This is the maximum payload size for ZNet firmware, as specified in the datasheet.
         * This value is provided for reference only and is not enforced by this software unless
         * max size unless specified in the setMaxPayloadSize(byte) method.
         * Note: this size refers to the packet size prior to escaping the control bytes.
         * Note: ZB Pro firmware provides the ATNP command to determine max payload size.
         * For ZB Pro firmware, the TX Status will return a PAYLOAD_TOO_LARGE (0x74) delivery status 
         * if the payload size is exceeded
         */

        public const byte ZnetMaxPayloadSize = 72;
        public const byte DefaultBroadcastRadius = 0;

        public XBeeAddress64 DestinationSerial { get; set; }
        public XBeeAddress16 DestinationAddress { get; set; }
        public byte BroadcastRadius { get; set; }
        public Options Option { get; set; }
        public byte[] Payload { get; set; }
        public byte MaxPayloadSize { get; set; }

        public TxRequest(XBeeAddress destination, byte[] payload)
            : this(payload)
        {
            if (destination is XBeeAddress16)
            {
                DestinationSerial = XBeeAddress64.Broadcast;
                DestinationAddress = (XBeeAddress16) destination;
            }
            else
            {
                DestinationSerial = (XBeeAddress64) destination;
                DestinationAddress = XBeeAddress16.Unknown;
            }
        }

        public TxRequest(XBeeAddress64 destSerial, XBeeAddress16 destAddress, byte[] payload)
            : this(payload)
        {
            DestinationSerial = destSerial;
            DestinationAddress = destAddress;
        }

        protected TxRequest(byte[] payload)
        {
            Payload = payload;
            BroadcastRadius = DefaultBroadcastRadius;
            Option = 0;
        }

        public override ApiId ApiId
        {
            get { return ApiId.ZnetTxRequest; }
        }

        public override byte[] GetFrameData()
        {
            if (MaxPayloadSize > 0 && Payload.Length > MaxPayloadSize)
                throw new ArgumentException("Payload exceeds user-defined maximum payload size of "
                    + MaxPayloadSize + " bytes. Please package into multiple packets");

            var output = new OutputStream();

            GetFrameHeader(output);
            GetFrameOptions(output);
            GetFramePayload(output);

            return output.ToArray();
        }

        protected virtual void GetFrameHeader(OutputStream output)
        {
            output.Write((byte)ApiId);
            output.Write(FrameId);
            output.Write(DestinationSerial.Address);
            output.Write(DestinationAddress.Address);
        }

        protected virtual void GetFrameOptions(OutputStream output)
        {
            output.Write(BroadcastRadius);
            output.Write((byte)Option);
        }

        protected virtual void GetFramePayload(OutputStream output)
        {
            output.Write(Payload);
        }

        public override string ToString()
        {
            return base.ToString() +
                ",destAddr64=" + DestinationSerial +
                ",destAddr16=" + DestinationAddress +
                ",broadcastRadius=" + BroadcastRadius +
                ",option=" + Option +
                ",payload=byte[" + Payload.Length + "]";
        }
    }
}
