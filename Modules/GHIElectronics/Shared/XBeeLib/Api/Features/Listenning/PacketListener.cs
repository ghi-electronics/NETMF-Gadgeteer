using System.Collections;
using System.Threading;

namespace NETMF.OpenSource.XBee.Api
{
    public class PacketListener : IPacketListener
    {
        private bool _finished;

        protected IPacketFilter Filter;
        protected readonly ArrayList Packets;
        protected Timer TimeoutTimer;
        protected ResponseHandler ResponseHandler;
        protected AutoResetEvent FinishedFlag;

        public bool Finished
        {
            get { return _finished; }
            protected set
            {
                _finished = value;

                if (!_finished) 
                    return;

                if (TimeoutTimer != null)
                    TimeoutTimer.Dispose();
                    
                FinishedFlag.Set();
            }
        }

        public PacketListener(IPacketFilter filter = null, int timeout = 5000, ResponseHandler responseHandler = null)
        {
            Filter = filter;
            Packets = new ArrayList();
            FinishedFlag = new AutoResetEvent(false);
            ResponseHandler = responseHandler;
           
            if (timeout > 0)
                TimeoutTimer = new Timer(s => OnTimeout(), null, timeout, -1); 
        }

        public virtual void ProcessPacket(XBeeResponse packet)
        {
            if (Finished)
                return;

            var packetAccepted = Filter == null || Filter.Accepted(packet);

            Finished = (Filter != null && Filter.Finished());

            if (!packetAccepted)
                return;

            if (ResponseHandler != null)
            {
                ResponseHandler.Invoke(packet, Finished);
            }
            else
            {
                Packets.Add(packet);
            }
        }

        public XBeeResponse[] GetPackets(int timeout = -1)
        {
            FinishedFlag.WaitOne(timeout, false);

            if (Packets.Count == 0)
                return new XBeeResponse[0];

            return (XBeeResponse[])Packets.ToArray(typeof(XBeeResponse));
        }

        protected virtual void OnTimeout()
        {
            Finished = true;

            if (ResponseHandler != null)
                ResponseHandler.Invoke(null, Finished);
        }
    }
}