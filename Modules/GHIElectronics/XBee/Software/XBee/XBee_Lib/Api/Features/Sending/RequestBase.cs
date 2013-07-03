using System;
using NETMF.OpenSource.XBee.Api.Common;

namespace NETMF.OpenSource.XBee.Api
{
    public abstract class RequestBase : IRequest
    {
        protected enum Response
        {
            None,
            Single,
            Multiple
        }

        protected readonly XBeeApi LocalXBee;
        protected IPacketFilter Filter;
        protected Response ExpectedResponse;
        protected int TimeoutValue;
        protected NodeInfo DestinationNode;
        protected XBeeAddress DestinationAddress;

        protected RequestBase(XBeeApi localXBee)
        {
            LocalXBee = localXBee;
        }

        protected void Init()
        {
            ExpectedResponse = Response.Single;
            TimeoutValue = PacketParser.DefaultParseTimeout;
            DestinationAddress = null;
            DestinationNode = null;
            Filter = null;
        }

        #region IRequest Members

        public IRequest Use(IPacketFilter filter)
        {
            Filter = filter;
            return this;
        }

        public IRequest Timeout(int value)
        {
            TimeoutValue = value;
            return this;
        }

        public IRequest NoTimeout()
        {
            TimeoutValue = -1;
            return this;
        }

        public IRequest To(ushort networkAddress)
        {
            DestinationAddress = new XBeeAddress16(networkAddress);
            return this;
        }

        public IRequest To(ulong serialNumber)
        {
            DestinationAddress = new XBeeAddress64(serialNumber);
            return this;
        }

        public IRequest To(XBeeAddress destination)
        {
            DestinationAddress = destination;
            return this;
        }

        public IRequest To(NodeInfo destination)
        {
            DestinationNode = destination;
            return this;
        }

        public IRequest ToAll()
        {
            DestinationAddress = XBeeAddress64.Broadcast;
            return this;
        }

        public AsyncSendResult Invoke()
        {
            var request = CreateRequest();

            switch (ExpectedResponse)
            {
                case Response.None:
                    LocalXBee.SendNoReply(request);
                    return null;

                case Response.Single:
                case Response.Multiple:
                    InitFilter(request);
                    return LocalXBee.BeginSend(request, Filter, TimeoutValue);

                default:
                    throw new NotImplementedException();
            }
        }

        public void Invoke(ResponseHandler responseHandler)
        {
            var request = CreateRequest();

            switch (ExpectedResponse)
            {
                case Response.None:
                    LocalXBee.SendNoReply(request);
                    break;

                case Response.Single:
                case Response.Multiple:
                    InitFilter(request);
                    LocalXBee.BeginSend(request, responseHandler, Filter, TimeoutValue);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public XBeeResponse[] GetResponses()
        {
            ExpectedResponse = Response.Multiple;
            return Invoke().EndReceive(TimeoutValue);
        }

        public XBeeResponse GetResponse()
        {
            ExpectedResponse = Response.Single;
            var responses = Invoke().EndReceive(TimeoutValue);

            if (responses.Length == 0)
                throw new XBeeTimeoutException();

            return responses[0];
        }

        public void NoResponse()
        {
            ExpectedResponse = Response.None;
            Invoke();
        }

        #endregion

        protected abstract XBeeRequest CreateRequest();

        protected void InitFilter(XBeeRequest request)
        {
            if (Filter == null)
            {
                Filter = request is AtCommand
                           ? new AtResponseFilter((AtCommand)request)
                           : new PacketIdFilter(request);   
            }
            else if (Filter is PacketIdFilter)
            {
                (Filter as PacketIdFilter).ExpectedPacketId = request.FrameId;   
            }
        }
    }
}