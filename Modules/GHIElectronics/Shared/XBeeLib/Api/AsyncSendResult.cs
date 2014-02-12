using System;

namespace NETMF.OpenSource.XBee.Api
{
    public class AsyncSendResult : IDisposable
    {
        private XBeeApi _xbee;
        private IPacketListener _responseListener;
        private bool _finished;

        internal AsyncSendResult(XBeeApi xbee, IPacketListener responseListener)
        {
            _xbee = xbee;
            _responseListener = responseListener;
        }

        public XBeeResponse[] EndReceive(int timeout = -1)
        {
            if (_finished)
                throw new InvalidOperationException("EndReceive can be called only once!");

            try
            {
                return _responseListener.GetPackets(timeout);
            }
            finally
            {
                _xbee.RemovePacketListener(_responseListener);
                _responseListener = null;
                _xbee = null;
                _finished = true;
            }
        }

        public void Dispose()
        {
            if (!_finished)
                _xbee.RemovePacketListener(_responseListener);
        }
    }
}