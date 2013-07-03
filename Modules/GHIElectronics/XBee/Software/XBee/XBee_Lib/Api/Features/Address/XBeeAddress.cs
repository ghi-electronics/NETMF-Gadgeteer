using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api
{
    public abstract class XBeeAddress
    {
        public byte[] Address { get; protected set; }

        public byte this[int index]
        {
            get { return Address[index]; }
            set { Address[index] = value; }
        }

        protected XBeeAddress(byte[] address)
        {
            Address = address;
        }

        public override string ToString()
        {
            return ByteUtils.ToBase16(Address);
        }

        public static bool operator ==(XBeeAddress a1, XBeeAddress a2)
        {
            if (ReferenceEquals(null, a1) && ReferenceEquals(null, a2))
                return true;

            return !ReferenceEquals(null, a1) && a1.Equals(a2);
        }

        public static bool operator !=(XBeeAddress a1, XBeeAddress a2)
        {
            return !(a1 == a2);
        }

        public bool Equals(XBeeAddress other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Arrays.AreEqual(other.Address, Address);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is XBeeAddress)) return false;
            return Equals((XBeeAddress) obj);
        }

        public override int GetHashCode()
        {
            return (Address != null ? Arrays.HashCode(Address) : 0);
        }
    }
}