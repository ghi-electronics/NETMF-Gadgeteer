using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api
{
    public class DataRequest : RequestBase
    {
        public virtual byte[] Payload { get; set; }

        public DataRequest(XBeeApi xbee)
            : base(xbee)
        {
        }

        internal void Init(string payload)
        {
            Init(Arrays.ToByteArray(payload));
        }
        
        internal void Init(byte[] payload)
        {
            Init();
            Payload = payload;
        }

        protected override XBeeRequest CreateRequest()
        {
            if (DestinationNode != null) 
               return LocalXBee.CreateRequest(Payload, DestinationNode);

            if (DestinationAddress == null)
                DestinationAddress = XBeeAddress64.Broadcast;

            return LocalXBee.CreateRequest(Payload, DestinationAddress);
        }
    }
}
