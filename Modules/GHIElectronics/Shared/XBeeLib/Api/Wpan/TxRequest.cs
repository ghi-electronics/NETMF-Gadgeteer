using System;
using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api.Wpan
{
    /// <summary>
    /// Series 1 XBee.
    /// Class for 16 and 64 bit address Transmit packets.
    /// </summary>
    public class TxRequest : XBeeRequest, IWpanPacket
    {
        public enum Options
        {
		    Unicast = 0,
		    DisableAck = 1,
		    Broadcast = 4
        }

        /// <summary>
        /// Maximum payload size as specified in the series 1 XBee manual.
        /// This is provided for reference only and is not used for validation
        /// </summary>
        public static byte MaxPayloadSize = 100;

        private byte[] _payload;

        public byte[] Payload
        {
            get { return _payload; }
            set
            {
                if (value != null && value.Length > MaxPayloadSize)
                    throw new ArgumentOutOfRangeException("Max payload length is " + MaxPayloadSize);

                _payload = value;
            }
        }

        public Options Option { get; set; }

        public XBeeAddress Destination { get; set; }

        public override ApiId ApiId
        {
            get 
            { 
                return (Destination is XBeeAddress16) 
                    ? ApiId.TxRequest16 
                    : ApiId.TxRequest64; 
            }
        }

        public TxRequest(XBeeAddress destination, byte[] payload, Options option = Options.Unicast)
        {
            Destination = destination;
            Payload = payload;
            Option = option;
        }

        public override byte[] GetFrameData()
        {
            var output = new OutputStream();

            output.Write((byte)ApiId);
            output.Write(FrameId);
            output.Write(Destination.Address);
            output.Write((byte)Option);

            if (Payload != null)
                output.Write(Payload);

            return output.ToArray();
        }

        public override string ToString()
        {
            return base.ToString() 
                + ",destination=" + Destination
                + ",option=" + Option
                + ",payload=byte[" + Payload.Length + "]";
        }
    }
}