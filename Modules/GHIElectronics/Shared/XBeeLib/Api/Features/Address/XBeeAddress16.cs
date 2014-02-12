using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// Represents a 16-bit XBee Address.
    /// </summary>
    public class XBeeAddress16 : XBeeAddress
    {
        public static readonly XBeeAddress16 Broadcast = new XBeeAddress16(0xFF, 0xFF);
        public static readonly XBeeAddress16 Unknown = new XBeeAddress16(0xFF, 0xFE);

        public new ushort Address
        {
            get { return UshortUtils.ToUshort(base.Address); }
            set { base.Address = Arrays.ToByteArray(value); }
        }

        public XBeeAddress16() 
            : base(new byte[2])
        {
        }

        public XBeeAddress16(byte[] address)
            : base(address)
        {
        }

        public XBeeAddress16(byte msb, byte lsb)
            : base(new[] { msb, lsb })
        {
        }

        public XBeeAddress16(ushort address) 
            : base(Arrays.ToByteArray(address))
        {
        }
    }
}