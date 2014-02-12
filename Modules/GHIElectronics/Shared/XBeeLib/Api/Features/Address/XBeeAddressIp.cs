using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api
{
    public class XBeeAddressIp : XBeeAddress
    {
        public static readonly XBeeAddressIp Broadcast = new XBeeAddressIp("255.255.255.255"); 

        public new string Address
        {
            get { return Arrays.ToString(base.Address); } 
            set { base.Address = Arrays.ToByteArray(value); }
        }

        public XBeeAddressIp(byte[] ipAddress)
            : base(ipAddress)
        {
        }

        public XBeeAddressIp(string ipAddress)
            : base(Arrays.ToByteArray(ipAddress))
        {
        }

        public override string ToString()
        {
            return Address;
        }
    }
}