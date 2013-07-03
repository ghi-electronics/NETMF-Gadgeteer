using System;
using System.Collections;
using NETMF.OpenSource.XBee.Api;
using NETMF.OpenSource.XBee.Api.Common;
using NETMF.OpenSource.XBee.Api.Wpan;
using NETMF.OpenSource.XBee.Api.Zigbee;
using NETMF.OpenSource.XBee.Util;
using AtCmd = NETMF.OpenSource.XBee.Api.Common.AtCmd;
using DiscoverResult = NETMF.OpenSource.XBee.Api.Common.DiscoverResult;
using RxResponse = NETMF.OpenSource.XBee.Api.Zigbee.RxResponse;
using TxRequest = NETMF.OpenSource.XBee.Api.Wpan.TxRequest;
using TxStatusResponse = NETMF.OpenSource.XBee.Api.Zigbee.TxStatusResponse;

namespace NETMF.OpenSource.XBee
{
    /// <summary>
    /// This is an API for communicating with Digi XBeeApi 802.15.4 and ZigBee radios
    /// </summary>
    public class XBeeApi
    {
        private readonly IXBeeConnection _connection;
        private readonly PacketParser _parser;
        private readonly PacketIdGenerator _idGenerator;

        private IPacketListener _addressLookupListener;
        private bool _addressLookupEnabled;
        private XBeeRequest _currentRequest;

        private IPacketListener _dataReceivedListener;
        private bool _dataReceivedEventEnabled;

        private IPacketListener _modemStatusListener;
        private bool _modemStatusEventEnabled;

        private readonly AtRequest _atRequest;
        private readonly DataRequest _dataRequest;
        private readonly RawRequest _rawRequest;
        private readonly DataDelegateRequest _dataDelegateRequest;
        
        public Hashtable AddressLookup { get; private set; }
        public XBeeConfiguration Config { get; private set; }

        public bool IsConnected()
        {
            return _connection != null && _connection.Connected;
        }

        protected XBeeApi()
        {
            _parser = new PacketParser();
            _idGenerator = new PacketIdGenerator();
            _atRequest = new AtRequest(this);
            _dataRequest = new DataRequest(this);
            _rawRequest = new RawRequest(this);
            _dataDelegateRequest = new DataDelegateRequest(this);
        }

        public XBeeApi(IXBeeConnection connection) 
            : this()
        {
            _connection = connection;
            _connection.DataReceived += (data, offset, count) =>
            {
                var buffer = new byte[count];
                Array.Copy(data, offset, buffer, 0, count);
                _parser.AddToParse(buffer);
            };
        }

        public XBeeApi(string portName, int baudRate)
            : this(new SerialConnection(portName, baudRate))
        {
        }

        public void Open()
        {
            _parser.Start();
            _connection.Open();
            ReadConfiguration();
            EnableDataReceivedEvent();
            EnableModemStatusEvent();

            if (Config.IsSeries2())
                EnableAddressLookup();
        }

        public void Close()
        {
            _parser.Stop();
            _connection.Close();
        }

        public void ReadConfiguration()
        {
            var readAttempts = 2;

            while (readAttempts > 0)
            {
                try
                {
                    Config = XBeeConfiguration.Read(this);
                    break;
                }
                catch (XBeeTimeoutException)
                {
                    readAttempts--;
                }
            }

            if (Config == null)
            {
                const string message = "AT command timed-out while attempt to read configuration. "
                     + "The XBeeApi radio must be in API mode (AP=2) to use with this library";

                Logger.Error(message);
                throw new XBeeException(message);
            }

            if (Config.ApiMode != ApiModes.EnabledWithEscaped)
            {
                Logger.LowDebug("XBeeApi radio is in API mode without escape characters (AP=1)."
                                + " The radio must be configured in API mode with escape bytes "
                                + "(AP=2) for use with this library.");

                Config.SetApiMode(ApiModes.EnabledWithEscaped);
                Config.Save();

                Logger.Debug("Successfully set AP mode to ApiMode.EnabledWithEscaped");
            }

            if (!Logger.IsActive(LogLevel.Info))
                return;

            Logger.Info(Config.ToString());
        }

        public void AddPacketListener(IPacketListener listener)
        {
            _parser.AddPacketListener(listener);
        }

        public void RemovePacketListener(IPacketListener listener)
        {
            _parser.RemovePacketListener(listener);
        }

        public void EnableAddressLookup()
        {
            if (_addressLookupEnabled)
                return;

            if (AddressLookup == null)
                AddressLookup = new Hashtable();

            if (_addressLookupListener == null)                 
                _addressLookupListener = new PacketListener(new AddressPacketFilter(), -1, OnAddressReceived);
            
            AddPacketListener(_addressLookupListener);
            _addressLookupEnabled = true;
        }

        public void DisableAddressLookup()
        {
            if (!_addressLookupEnabled)
                return;

            AddressLookup.Clear();
            RemovePacketListener(_addressLookupListener);
            _addressLookupEnabled = false;
        }

        public void EnableDataReceivedEvent()
        {
            if (_dataReceivedEventEnabled)
                return;

            if (_dataReceivedListener == null)
                _dataReceivedListener = new PacketListener(new DataPacketFilter(), -1, OnDataReceived);

            AddPacketListener(_dataReceivedListener);
            _dataReceivedEventEnabled = true;
        }

        public void DisableDataReceivedEvent()
        {
            if (!_dataReceivedEventEnabled)
                return;

            RemovePacketListener(_dataReceivedListener);
            _dataReceivedEventEnabled = false;
        }

        public void EnableModemStatusEvent()
        {
            if (_modemStatusEventEnabled)
                return;

            if (_modemStatusListener == null)
                _modemStatusListener = new PacketListener(new PacketTypeFilter(typeof(ModemStatusResponse)), -1, OnModemStatusReceived);

            AddPacketListener(_modemStatusListener);
            _modemStatusEventEnabled = true;
        }

        public void DisableModemStatusEvent()
        {
            if (!_modemStatusEventEnabled)
                return;

            RemovePacketListener(_modemStatusListener);
            _modemStatusEventEnabled = false;
        }

        public void DiscoverNodes(DiscoverResultHandler handler)
        {
            Send(AtCmd.NodeDiscoverTimeout).Invoke(timeoutResponse =>
            {
                var timeout = GetDiscoverTimeout(timeoutResponse);
                var request = CreateDiscoverRequest(timeout);
                request.Invoke((response, finished) =>
                {
                    if (response != null)
                        handler(GetDiscoverResponse(response));
                });
            });
        }

        public DiscoverResult[] DiscoverNodes()
        {
            var timeoutResponse = Send(AtCmd.NodeDiscoverTimeout).GetResponse();
            var timeout = GetDiscoverTimeout(timeoutResponse);
            var request = CreateDiscoverRequest(timeout);
            var responses = request.GetResponses();
            var result = new DiscoverResult[responses.Length];

            for (var i = 0; i < responses.Length; i++)
                result[i] = GetDiscoverResponse(responses[i]);

            return result;
        }

        public void Reset(ResetMode mode = ResetMode.Software)
        {
            switch (mode)
            {
                case ResetMode.Software:
                    var modemStatusFilter = new PacketTypeFilter(typeof(ModemStatusResponse));
                    var response = Send(AtCmd.SoftwareReset).Use(modemStatusFilter).GetResponse();
                    var modemStatus = ((ModemStatusResponse) response).Status;
                    if (modemStatus != ModemStatus.WatchdogTimerReset)
                        throw new XBeeException("Unexpected modem status: " + modemStatus);
                    break;

                case ResetMode.RestoreDefaults:
                    Send(AtCmd.RestoreDefaults).GetResponse();
                    break;

                case ResetMode.Network:
                    if (Config.IsSeries1())
                        throw new NotSupportedException("Series 1 has no network reset command");

                    Send(AtCmd.NetworkReset).GetResponse();
                    break;
            }
        }

        protected XBeeAddress16 GetNetworkAddress(XBeeAddress serialAddress)
        {
            if (!(serialAddress is XBeeAddress64))
                throw new ArgumentException();

            return _addressLookupEnabled && AddressLookup.Contains(serialAddress)
                ? (XBeeAddress16)AddressLookup[serialAddress]
                : XBeeAddress16.Unknown;
        }

        public delegate void DiscoverResultHandler(DiscoverResult node);

        // Creating requests

        public XBeeRequest CreateRequest(string payload, XBeeAddress destination)
        {
            return CreateRequest(Arrays.ToByteArray(payload), destination);
        }

        public XBeeRequest CreateRequest(byte[] payload, XBeeAddress destination)
        {
            if (Config.IsSeries1())
                return new TxRequest(destination, payload) 
                    {FrameId = _idGenerator.GetNext()};

            if (!(destination is XBeeAddress64) || destination == null)
                throw new ArgumentException("64 bit address expected", "destination");

            return new Api.Zigbee.TxRequest((XBeeAddress64)destination, GetNetworkAddress(destination), payload) 
                { FrameId = _idGenerator.GetNext() };
        }

        public XBeeRequest CreateRequest(string payload, NodeInfo destination)
        {
            return CreateRequest(Arrays.ToByteArray(payload), destination);
        }

        public XBeeRequest CreateRequest(byte[] payload, NodeInfo destination)
        {
            if (Config.IsSeries1())
                return new TxRequest(destination.SerialNumber, payload) 
                    { FrameId = _idGenerator.GetNext() };

            return new Api.Zigbee.TxRequest(destination.SerialNumber, destination.NetworkAddress, payload)
                { FrameId = _idGenerator.GetNext() };
        }

        public AtCommand CreateRequest(ushort atCommand, params byte[] value)
        {
            return new AtCommand(atCommand, value) { FrameId = _idGenerator.GetNext() };
        }

        public RemoteAtCommand CreateRequest(ushort atCommand, XBeeAddress destination, params byte[] value)
        {
            if (destination is XBeeAddress16)
                throw new ArgumentException("64 bit address expected", "destination");

            return new RemoteAtCommand(atCommand, (XBeeAddress64) destination, GetNetworkAddress(destination), value) 
                { FrameId = _idGenerator.GetNext() };
        }

        public RemoteAtCommand CreateRequest(ushort atCommand, NodeInfo destination, params byte[] value)
        {
            return new RemoteAtCommand(atCommand, destination.SerialNumber, destination.NetworkAddress, value) 
                { FrameId = _idGenerator.GetNext() };
        }

        // Sending requests

        public DataRequest Send(string payload)
        {
            _dataRequest.Init(payload);
            return _dataRequest;
        }

        public DataRequest Send(params byte[] payload)
        {
            _dataRequest.Init(payload);
            return _dataRequest;
        }

        public AtRequest Send(AtCmd atCommand, params byte[] value)
        {
            _atRequest.Init((ushort)atCommand, value);
            return _atRequest;
        }

        public AtRequest Send(Api.Wpan.AtCmd atCommand, params byte[] value)
        {
            _atRequest.Init((ushort)atCommand, value);
            return _atRequest;
        }

        public AtRequest Send(Api.Zigbee.AtCmd atCommand, params byte[] value)
        {
            _atRequest.Init((ushort)atCommand, value);
            return _atRequest;
        }

        public RawRequest Send(XBeeRequest request)
        {
            _rawRequest.Init(request);
            return _rawRequest;
        }

        public DataDelegateRequest Send(PayloadDelegate payloadDelegate)
        {
            _dataDelegateRequest.Init(payloadDelegate);
            return _dataDelegateRequest;
        }

        public delegate byte[] PayloadDelegate();

        public void SendNoReply(XBeeRequest request)
        {
            // we don't expect any response to this request
            request.FrameId = PacketIdGenerator.NoResponseId;
            SendRequest(request);
        }

        public AsyncSendResult BeginSend(XBeeRequest request, IPacketFilter filter = null, int timeout = 5000)
        {
            var responseListener = new PacketListener(filter, timeout);
            AddPacketListener(responseListener);
            SendRequest(request);
            return new AsyncSendResult(this, responseListener);
        }

        public void BeginSend(XBeeRequest request, ResponseHandler responseHandler, IPacketFilter filter = null, int timeout = 5000)
        {
            AddPacketListener(new PacketListener(filter, timeout, responseHandler));
            SendRequest(request);
        }

        protected void SendRequest(XBeeRequest request)
        {
            IsRequestSupported(request);

            if (_addressLookupEnabled)
                _currentRequest = request;

            if (Logger.IsActive(LogLevel.Debug))
                Logger.Debug("Sending " + request.GetType().Name + ": " + request);

            var bytes = XBeePacket.GetBytes(request);

            if (Logger.IsActive(LogLevel.LowDebug))
                Logger.LowDebug("Sending " + ByteUtils.ToBase16(bytes));
            
            _connection.Send(bytes);
        }

        protected void IsRequestSupported(XBeeRequest request)
        {
            // can be null when reading Config
            if (Config == null)
                return;

            if (Config.IsSeries1() && request is IZigbeePacket)
                throw new ArgumentException("You are connected to a Series 1 radio but attempting to send Series 2 requests");

            if (Config.IsSeries2() && request is IWpanPacket)
                throw new ArgumentException("You are connected to a Series 2 radio but attempting to send Series 1 requests");
        }

        // Receiving responses

        public XBeeResponse[] EndReceive(AsyncSendResult asyncResult, int timeout = -1)
        {
            return asyncResult.EndReceive(timeout);
        }

        public XBeeResponse Receive(Type expectedType = null, int timeout = PacketParser.DefaultParseTimeout)
        {
            var listener = new PacketListener(new PacketTypeFilter(expectedType ?? typeof(XBeeResponse)));

            try
            {
                AddPacketListener(listener);
                var responses =  listener.GetPackets(timeout);

                if (responses.Length == 0)
                    throw new XBeeTimeoutException();

                return responses[0];
            }
            finally
            {
                _parser.RemovePacketListener(listener);
            }
        }

        public XBeeResponse[] CollectResponses(int timeout = -1, Type expectedPacketType = null, byte maxPacketCount = byte.MaxValue)
        {
            return CollectResponses(timeout, new PacketCountFilter(maxPacketCount, expectedPacketType ?? typeof(XBeeResponse)));
        }

        public XBeeResponse[] CollectResponses(int timeout = -1, IPacketFilter filter = null)
        {
            var listener = new PacketListener(filter);

            try
            {
                AddPacketListener(listener);
                return listener.GetPackets(timeout);
            }
            finally
            {
                RemovePacketListener(listener);
            }
        }

        protected int GetDiscoverTimeout(AtResponse response)
        {
            // ms + 1 extra second
            return UshortUtils.ToUshort(response.Value) * 100 + 1000;
        }

        protected IRequest CreateDiscoverRequest(int timeout)
        {
            var filter = new NodeDiscoveryFilter();
            var request = Send(AtCmd.NodeDiscover).Use(filter).Timeout(timeout);
            return request;
        }

        protected DiscoverResult GetDiscoverResponse(XBeeResponse response)
        {
            var discoverResponse = Config.IsSeries1()
                    ? (DiscoverResult) Api.Wpan.DiscoverResult.Parse(response)
                    : Api.Zigbee.DiscoverResult.Parse(response);

            if (_addressLookupEnabled)
            {
                var nodeInfo = discoverResponse.NodeInfo;
                AddressLookup[nodeInfo.SerialNumber] = nodeInfo.NetworkAddress;
            }

            return discoverResponse;
        }

        protected void OnAddressReceived(XBeeResponse response, bool finished)
        {
            if (response is RemoteAtResponse)
            {
                var atResponse = response as RemoteAtResponse;
                AddressLookup[atResponse.RemoteSerial] = atResponse.RemoteAddress;
            }
            else if (response is RxResponse)
            {
                var zigbeeResponse = response as RxResponse;
                AddressLookup[zigbeeResponse.SourceSerial] = zigbeeResponse.SourceAddress;
            }
            else if (response is TxStatusResponse && _currentRequest is Api.Zigbee.TxRequest)
            {
                var txRequest = _currentRequest as Api.Zigbee.TxRequest;
                var txResponse = response as TxStatusResponse;
                var dataDelivered = txResponse.DeliveryStatus == TxStatusResponse.DeliveryResult.Success;

                AddressLookup[txRequest.DestinationSerial] = dataDelivered 
                    ? txResponse.DestinationAddress 
                    : XBeeAddress16.Unknown;
            }
            else if (response is NodeIdentificationResponse)
            {
                var identPacket = response as NodeIdentificationResponse;
                AddressLookup[identPacket.Sender.SerialNumber] = identPacket.Sender.NetworkAddress;
                AddressLookup[identPacket.Remote.SerialNumber] = identPacket.Remote.NetworkAddress;
            }
        }
        
        protected void OnDataReceived(XBeeResponse response, bool finished)
        {
            if (response is Api.Wpan.RxResponse)
            {
                var rxResponse = response as Api.Wpan.RxResponse;
                NotifyDataReceived(rxResponse.Payload, rxResponse.Source);
            }
            else if (response is RxResponse)
            {
                if (response is ExplicitRxResponse)
                {
                    var profileId = (response as ExplicitRxResponse).ProfileId;
                    var clusterId = (response as ExplicitRxResponse).ClusterId;

                    // if module AtCmd.ApiOptions has been set to value other than default (0)
                    // received API frames will be transported using explicit frames
                    // those frames have profile id set to Zigbee.ProfileId.Digi
                    if (profileId != (ushort)ProfileId.Digi)
                        return;

                    // cluster id will be set to ApiId value
                    switch ((ApiId)clusterId)
                    {
                        case ApiId.TxRequest16:
                        case ApiId.TxRequest64:
                        case ApiId.ZnetTxRequest:
                        case ApiId.ZnetExplicitTxRequest:
                            break;

                        default:
                            return;
                    }
                }

                var rxResponse = response as RxResponse;
                NotifyDataReceived(rxResponse.Payload, rxResponse.SourceSerial);
            }
        }

        protected void OnModemStatusReceived(XBeeResponse response, bool finished)
        {
            var statusResponse = (ModemStatusResponse) response;
            NotifyStatusChanged(statusResponse.Status);
        }

        protected void NotifyDataReceived(byte[] payload, XBeeAddress sender)
        {
            if (DataReceived != null)
                DataReceived(this, payload, sender);
        }

        protected void NotifyStatusChanged(ModemStatus status)
        {
            if (StatusChanged != null)
                StatusChanged(this, status);
        }

        public event XBeeDataReceivedEventHandler DataReceived;

        public event XBeeModemStatusEventHandler StatusChanged;

        public delegate void XBeeDataReceivedEventHandler(XBeeApi receiver, byte[] data, XBeeAddress sender);

        public delegate void XBeeModemStatusEventHandler(XBeeApi xbee, ModemStatus status);
    }
}