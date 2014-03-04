using System;
using Microsoft.SPOT;

using System.Collections;
using System.Text;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;
using System.Threading;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2C), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// A WiFi RN171 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class WiFiRN171 : GTM.Module
    {
        /// <summary>Constructor</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public WiFiRN171(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            string t_Command_Init = "$$$";

            _baud = 115200; //Changed from datasheet
            LocalIP = "0.0.0.0";
            _Command_Init = System.Text.Encoding.UTF8.GetBytes(t_Command_Init);
            _Port_Name = socket.SerialPortName;

            try
            {
                socket.EnsureTypeIsSupported(new char[] { 'K' }, this);
                _wifly = new GTI.Serial(socket, _baud, GTI.Serial.SerialParity.None, GTI.Serial.SerialStopBits.One, 8, GTI.Serial.HardwareFlowControl.Required, this);

                _Flow_Control_Enable = true;

                Debug.Print("Using hardware flow control");
            }
            catch
            {
                try
                {
                    socket.EnsureTypeIsSupported(new char[] { 'U' }, this);
                    _wifly = new GTI.Serial(socket, _baud, GTI.Serial.SerialParity.None, GTI.Serial.SerialStopBits.One, 8, GTI.Serial.HardwareFlowControl.NotRequired, this);
                }
                catch (Exception e)
                {
                    throw new Exception("Socket type is not supported", e);
                }
            }

            _wifly.Open();

            Reset = new GTI.DigitalOutput(socket, Socket.Pin.Three, true, this);
            RTS = new GTI.DigitalOutput(socket, Socket.Pin.Six, false, this);
        }

        //Added method for user-defined baud rates as well as
        //the ability to work with WiFly modules
        /// <summary>
        /// Sets the baud rate for the device.
        /// </summary>
        /// <param name="baud">The baudrate to use.</param>
        public void SetBaudRate(int baud = 115200)
        {
            _wifly.Close();
            _wifly.BaudRate = baud;
            _wifly.Open();

            _baud = baud;
        }

        #region "Enumerations"

        /// <summary>
        /// Represents the debug mode.
        /// </summary>
        public enum DebugMode
        {
            /// <summary>
            /// Use no debugging
            /// </summary>
            NoDebug,

            /// <summary>
            /// Report debugging to Visual Studio debug output
            /// </summary>
            StandardDebug,

            /// <summary>
            /// Re-direct debugging to a given serial port.
            /// Console Debugging
            /// </summary>
            SerialDebug
        };

        /// <summary>
        /// Represents the debug level.
        /// </summary>
        public enum DebugLevel
        {
            /// <summary>
            /// Only debug errors.
            /// </summary>
            DebugErrors,
            /// <summary>
            /// Debug everything.
            /// </summary>
            DebugAll
        };

        /// <summary>
        /// Represents the level of data to return.
        /// </summary>
        public enum DataReturnLevel
        {
            /// <summary>
            /// Returns all data.
            /// Command data and External Communications
            /// </summary>
            ReturnAll,

            /// <summary>
            /// Returns only External, non-command data
            /// </summary>
            ReturnIncomming
        };

        /// <summary>
        /// Represents the stream mode.
        /// </summary>
        public enum StreamMode
        {
            /// <summary>
            /// Idle
            /// </summary>
            NoStream,

            /// <summary>
            /// Command Data stream
            /// </summary>
            CommandStream,

            /// <summary>
            /// Stream for get reponses
            /// </summary>
            GetStream,

            /// <summary>
            /// Non-command data (external communications)
            /// </summary>
            DataStream
        };

        /// <summary>
        /// Represents the socket protocal.
        /// </summary>
        public enum SocketProtocol
        {
            /// <summary>
            /// UPD Mode: Connection-less protocol with no handshaking
            /// </summary>
            UDP = 1,

            /// <summary>
            /// TCP Server Mode: TCP Connection with handshaking (Client and Server)
            /// </summary>
            TCP_Server = 2,

            /// <summary>
            /// Secure Connection Mode: Only send to the stored host-ip
            /// </summary>
            Secure_Connection = 4,

            /// <summary>
            /// TCP Client Mode: TCP Connection with handshaking (Client Only)
            /// </summary>
            TCP_Client = 8
        }

        /// <summary>
        /// Represents the encyrption mode to use.
        /// </summary>
        public enum WirelessEncryptionMode
        {
            /// <summary>Open Authentication (No Passphrase required)</summary>
            Open = 0,
            /// <summary>128-bit Wired Equivalent Privacy (WEP)</summary>
            WEP_128 = 1,
            /// <summary>Wi-Fi Protected Access (WPA)</summary>
            WPA1 = 2,
            /// <summary>Mixed WPA1 &amp; WPA2-PSK</summary>
            MixedWPA1_WPA2 = 3,
            /// <summary>Wi-Fi Protected Access (WPA) II (uses preshared key)</summary>
            WPA2_PSK = 4
        }

        private enum RunMode
        {
            Normal = 0,
            Update_Wait = 1, //Reserved for future use
            Update_Fail = 2, //Reserved for future use
            Update_Okay = 3, //Reserved for future use
            Boot = 4
        }

        #endregion

        #region "Private Data Types"

        //Strings
        private string _Port_Name = "";
        private string _Command_Mode_Response = "";
        private string _Serial_String_Buffer = "";

        //Ints
        private int _baud = 0;
        private int _Timeout = 10000;

        //Classes
        private GTI.Serial _wifly;
        private GTI.Serial _debug_port;
        private static Thread listen = null;

        //Byte
        private byte[] _Command_Init = new byte[3];
        private byte[] _Serial_Byte_Buffer = new byte[1] { 0x00 };

        //Bools
        private bool _device_ready = false;
        private bool _DHCP = false;
        private bool _Command_Mode_Response_Complete = true;
        private bool _Command_Mode_Response_Okay = true;
        private bool _Flow_Control_Enable = false;

        //Pin declarations
        private GTI.DigitalOutput Reset;
        private GTI.DigitalOutput RTS;

        //Others
        private StreamMode _stream = StreamMode.NoStream;
        /// <summary>
        /// The current debug mode.
        /// </summary>
        public DebugMode _debug = DebugMode.NoDebug;
        private DebugLevel _debug_level = DebugLevel.DebugErrors;
        private DataReturnLevel _data_level = DataReturnLevel.ReturnIncomming;
        private DateTime _TimeOutDate;

        private RunMode _RunMode = RunMode.Normal;

        #endregion

        #region "Public Data Types"
        /// <summary>
        /// Is the device ready.
        /// </summary>
        public bool IsReady = false;

        //strings
        /// <summary>
        /// The local IP of the device.
        /// </summary>
        public string LocalIP { get; protected set; }
        /// <summary>
        /// The local listening port of the device.
        /// </summary>
        public string LocalListenPort { get; protected set; }
        #endregion

        #region "Public Events"

        //Delegates
        /// <summary>
        /// A delegate representing receipt of an HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        public delegate void HttpRequestReceivedHandler(HttpStream request);
        /// <summary>
        /// A delegate representing data received.
        /// </summary>
        /// <param name="data">The data received.</param>
        public delegate void DataReceivedHandler(string data);
        /// <summary>
        /// A delegate representing line received.
        /// </summary>
        /// <param name="line">The line received.</param>
        public delegate void LineReceivedHandler(string line);
        /// <summary>
        /// A delegate representing connection opening.
        /// </summary>
        public delegate void ConnectionEstablishedHandler();
        /// <summary>
        /// A delegate representing connection closure.
        /// </summary>
        public delegate void ConnectionClosedHandler();

        //Handlers

        /// <summary>
        /// Fired when a HTTP Request is received
        /// </summary>
        public event HttpRequestReceivedHandler HttpRequestReceived;

        /// <summary>
        /// Fired when any data is received
        /// </summary>
        public event DataReceivedHandler DataReceived;

        /// <summary>
        /// Fired when a complete line of data has been received
        /// </summary>
        public event LineReceivedHandler LineReceived;

        /// <summary>
        /// Fired when an connection has been establised to a
        /// remote client.
        /// </summary>
        public event ConnectionEstablishedHandler ConnectionEstablished;

        /// <summary>
        /// Fired when an connection to a remote client
        /// has closed.
        /// </summary>
        public event ConnectionClosedHandler ConnectionClosed;

        //Triggers

        /// <summary>
        /// Fired when an HTTP request is received.
        /// </summary>
        /// <param name="request">The request received.</param>
        protected virtual void OnHttpRequestReceived(HttpRequest request)
        {
            if (HttpRequestReceived != null)
            {
                HttpStream stream = new HttpStream(request, _wifly);
                HttpRequestReceived(stream);
            }
        }

        /// <summary>
        /// Fired when data is received.
        /// </summary>
        /// <param name="data">The line received.</param>
        protected virtual void OnDataReceived(string data)
        {
            if (DataReceived != null)
                DataReceived(data);
        }

        /// <summary>
        /// Fired when an a line is received.
        /// </summary>
        /// <param name="line">The HTTP request.</param>
        protected virtual void OnLineReceived(string line)
        {
            if (LineReceived != null)
                LineReceived(line);
        }

        /// <summary>
        /// Fired when the connection is opened.
        /// </summary>
        protected virtual void OnConnectionEstablished()
        {
            if (ConnectionEstablished != null)
                ConnectionEstablished();
        }

        /// <summary>
        /// Fired when the connection is closed.
        /// </summary>
        protected virtual void OnConnectionClosed()
        {
            if (ConnectionClosed != null)
                ConnectionClosed();
        }

        #endregion

        #region "Construction And Initialization"

        //WiFly Construction Method
        //public WiFly(Cpu.Pin Reset_Pin, Cpu.Pin RTS_Pin, string ComPort = "COM1", int BaudRate = 9600, string CommandToken = "$", DebugMode Debug = DebugMode.NoDebug)
        //{
        //    LocalIP = "0.0.0.0";
        //    Reset = new OutputPort(Reset_Pin, true);

        //    string t_Command_Init = CommandToken + CommandToken + CommandToken;
        //    _Command_Init = System.Text.Encoding.UTF8.GetBytes(t_Command_Init);

        //    _Port_Name = ComPort;
        //    _baud = BaudRate;

        //    _wifly = new SerialPort(_Port_Name, _baud, Parity.None, 8, StopBits.One);
        //    //_wifly.Handshake = Handshake.RequestToSend;
        //    _wifly.ErrorReceived += new SerialErrorReceivedEventHandler(_wifly_ErrorReceived);
        //    _wifly.Open();

        //    RTS = new OutputPort(RTS_Pin, false);
        //}

        //void _wifly_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        //{
        //    //Debug.Print(e.EventType.ToString());
        //}

        /// <summary>
        /// Reboot the module
        /// </summary>
        public void Reboot()
        {
            _wifly.DiscardInBuffer();
            _wifly.DiscardOutBuffer();

            _device_ready = false;

            //Reset the module
            Reset.Write(false);
            Thread.Sleep(100);
            Reset.Write(true);
            Thread.Sleep(250);

            //Wait for the device to be ready
            _TimeOutDate = DateTime.Now.AddMilliseconds((double)_Timeout);
            while (!_device_ready)
            {
                if (_TimeOutDate <= DateTime.Now)
                    throw new Exception("Time out waiting for device to ready");

                Thread.Sleep(1);
            }

            Debug.Print("Device is ready");
        }

        /// <summary>
        /// Initialize the WiFly Module
        /// </summary>
        /// <param name="protocol">The socket protocol to initialize</param>
        /// <returns>True on successful initialization, false upon failure</returns>
        public bool Initialize(SocketProtocol protocol = SocketProtocol.TCP_Server)
        {
            if (listen == null)
            {
                listen = new Thread(_Serial_Listen);
                listen.Start();
            }

            _device_ready = false;

            //Reset the module
            Reset.Write(false);
            Thread.Sleep(100);
            Reset.Write(true);
            Thread.Sleep(250);

            //Wait for the device to be ready
            _TimeOutDate = DateTime.Now.AddMilliseconds((double)_Timeout);
            while (!_device_ready)
            {
                if (_TimeOutDate <= DateTime.Now)
                    return false;

                Thread.Sleep(1);
            }

            //Exit command mode..just in case
            if (!_Command_Mode_Exit())
                return false;

            //Enter command mode
            if (!_Command_Mode_Start())
                return false;

            //Set default initialization values
            if (!_Command_Execute("set sys printlvl 0"))
                return false;

            if (!_Command_Execute("set comm remote 0"))
                return false;

            //Setup hardware flow control
            if (_Flow_Control_Enable)
            {
                if (!_Command_Execute("set uart flow 1"))
                    return false;
            }

            //if (!_Command_Execute("set uart baud 115200"))
            //    return false;

            //Enable TX pin
            if (!_Command_Execute("set uart tx 1"))
                return false;

            //Exit command mode
            if (!_Command_Mode_Exit())
                return false;

            return true;
        }

        /// <summary>
        /// updates the firmware.
        /// </summary>
        /// <returns>Whether it was successful or not.</returns>
        public bool UpdateFirmware()
        {
            //Exit command mode..just in case
            if (!_Command_Mode_Exit())
                return false;

            //Enter command mode
            if (!_Command_Mode_Start())
                return false;

            //Setup FTP access
            if (!_Command_Execute("set ftp address 0"))
                return false;

            if (!_Command_Execute("set dns name rn.microchip.com"))
                return false;

            if (!_Command_Execute("save"))
                return false;

            //Being updating firmware
            _RunMode = RunMode.Update_Wait;

            _Command_Execute("ftp update wifly7-245.img");

            _TimeOutDate = DateTime.Now.AddMilliseconds((double)_Timeout);
            while (_RunMode == RunMode.Update_Wait)
            {
                if (_TimeOutDate <= DateTime.Now)
                    return false;

                Thread.Sleep(1);
            }

            if (_RunMode != RunMode.Update_Okay)
                return false;
            else
                _RunMode = RunMode.Normal;

            return true;
        }

        /// <summary>
        /// Creates an access point with the given SSID.
        /// </summary>
        /// <param name="SSID">The SSID to use.</param>
        /// <returns>Whether or not it was successful.</returns>
        public bool CreateAccessPoint(string SSID = "")
        {
            //Exit command mode..just in case
            if (!_Command_Mode_Exit())
                return false;

            //Enter command mode
            if (!_Command_Mode_Start())
                return false;

            //Setup default access point parameters
            if (!_Command_Execute("set wlan channel 2"))
                return false;

            if (!_Command_Execute("set wlan join 7"))
                return false;

            if (!_Command_Execute("set ip address 192.168.1.1"))
                return false;

            if (!_Command_Execute("set ip gateway 192.168.1.1"))
                return false;

            if (!_Command_Execute("set ip netmask 255.255.255.0"))
                return false;

            if (!_Command_Execute("set ip dhcp 4"))
                return false;

            //if (SSID.Length > 0)
            //{
            if (!_Command_Execute("join " + SSID))
                return false;
            //}

            //Exit command mode
            if (!_Command_Mode_Exit())
                return false;

            return true;
        }

        #endregion

        #region "Network Configuration"

        /// <summary>
        /// Enable DHPC Mode
        /// </summary>
        /// <param name="Gateway">The Gateway address.</param>
        /// <param name="SubnetMask">The Subnet mask.</param>
        /// <param name="DNS">The DNS address.</param>
        /// <returns>Whether or not it was successful</returns>
        public bool EnableDHCP(string Gateway = "192.168.1.1", string SubnetMask = "255.255.255.0", string DNS = "192.168.1.1")
        {
            _DHCP = true;

            //Enter command mode
            if (!_Command_Mode_Start())
                return false;

            //Set DHCP off
            if (!_Command_Execute("set ip dhcp 1"))
                return false;

            //Set requested gateway
            if (!_Command_Execute("set ip gateway " + Gateway))
                return false;

            //Set requested subnetmask
            if (!_Command_Execute("set ip netmask " + SubnetMask))
                return false;

            //Set requested DNS address
            if (!_Command_Execute("set dns address " + DNS))
                return false;

            //Exit command mode
            if (!_Command_Mode_Exit())
                return false;

            return true;
        }

        /// <summary>
        /// Enable Static IP Request
        /// </summary>
        /// <param name="IP">IP Requested</param>
        /// <param name="Gateway">Gateway</param>
        /// <param name="SubnetMask">Subnet Mask</param>
        /// <param name="DNS">DNS Server</param>
        /// <returns>Whether or not it was successful</returns>
        public bool EnableStaticIP(string IP, string Gateway = "192.168.1.1", string SubnetMask = "255.255.255.0", string DNS = "192.168.1.1")
        {
            _DHCP = false;

            //Enter command mode
            if (!_Command_Mode_Start())
                return false;

            //Set DHCP off
            if (!_Command_Execute("set ip dhcp 0"))
                return false;

            //Set requested IP address
            if (!_Command_Execute("set ip address " + IP))
                return false;

            //Set requested gateway
            if (!_Command_Execute("set ip gateway " + Gateway))
                return false;

            //Set requested subnetmask
            if (!_Command_Execute("set ip netmask " + SubnetMask))
                return false;

            //Set requested DNS address
            if (!_Command_Execute("set dns address " + DNS))
                return false;

            //Exit command mode
            if (!_Command_Mode_Exit())
                return false;

            return true;
        }

        /// <summary>
        /// Attempt to join a wireless network with given parameters.
        /// Function does not return until and IP address has been granted,
        /// or a time-out has occured.
        /// </summary>
        /// <param name="SSID"></param>
        /// <param name="Passphrase"></param>
        /// <param name="channel"></param>
        /// <param name="Authentication"></param>
        /// <returns>Whether or not it was successful</returns>
        public bool JoinWirelessNetwork(string SSID, string Passphrase, int channel = 0, WirelessEncryptionMode Authentication = WirelessEncryptionMode.Open)
        {
            //Enter command mode
            if (!_Command_Mode_Start())
                return false;

            //Set DHCP off
            if (!_Command_Execute("set wlan ssid " + SSID))
                return false;

            //Set requested IP address
            if (!_Command_Execute("set wlan channel " + channel.ToString()))
                return false;

            //Set requested gateway
            if (!_Command_Execute("set wlan auth " + Authentication.ToString()))
                return false;

            //Set requested subnetmask
            if (!_Command_Execute("set wlan phrase " + Passphrase))
                return false;

            //Set requested DNS address
            if (!_Command_Execute("join"))
                return false;

            if (_DHCP)
            {
                if (!_Command_Execute("get ip"))
                    return false;
            }

            //Exit command mode
            if (!_Command_Mode_Exit())
                return false;

            return true;
        }

        /// <summary>
        /// Set Socket Protocol
        /// </summary>
        /// <param name="protocol">Desired Protocol</param>
        /// <returns></returns>
        public bool SetProtocol(SocketProtocol protocol)
        {
            //Enter command mode
            if (!_Command_Mode_Start())
                return false;

            //Set the requested protocol
            if (!_Command_Execute("set ip protocol " + protocol.ToString()))
                return false;

            //Exit command mode
            if (!_Command_Mode_Exit())
                return false;

            return true;
        }

        /// <summary>
        /// Set the port the WiFly module should listen for connection on.
        /// </summary>
        /// <param name="port">Port</param>
        /// <returns></returns>
        public bool SetListenPort(int port)
        {
            //Enter command mode
            if (!_Command_Mode_Start())
                return false;

            //Set the requested protocol
            if (!_Command_Execute("set ip local " + port.ToString()))
                return false;

            //Exit command mode
            if (!_Command_Mode_Exit())
                return false;

            return true;
        }

        /// <summary>
        /// Set the device listen port to 80 and allow HTTP request parsing
        /// </summary>
        /// <returns></returns>
        public bool EnableHttpServer()
        {
            //Enter command mode
            if (!_Command_Mode_Start())
                return false;

            //Set the requested protocol
            if (!_Command_Execute("set ip local 80"))
                return false;

            //Exit command mode
            if (!_Command_Mode_Exit())
                return false;

            HttpEnabled = true;

            return true;
        }

        /// <summary>
        /// Send data to the currently connected client
        /// </summary>
        /// <param name="data">Data</param>
        public void Send(byte[] data)
        {
            while (_wifly.BytesToWrite > 0)
                Thread.Sleep(10);

            _wifly.Write(data, 0, data.Length);

            return;
        }

        //public void SendResponse(HttpResponse response)
        //{
        //    StringBuilder header = new StringBuilder();
        //    header.Append("HTTP/1.1 " + _GetResponseText(response.ResponseStatus) + "\r\n");
        //    header.Append("Content-Length: " + response.ContentLength.ToString() + "\r\n");
        //    header.Append("Content-Type: " + response.ContentType + "\r\n");

        //    //Append the other MetaData
        //    foreach (string data in response.MetaData)
        //    {
        //        header.Append(data + "\r\n");
        //    }

        //    //Append end of header
        //    header.Append("\r\n");

        //    //copy the header
        //    string _header = header.ToString();

        //    //Clear the header 
        //    header.Clear();

        //    //Send the header
        //    Send(ref _header);

        //    //return so the user can send the body
        //    return;
        //}

        #endregion

        #region "HTTP Parser"

        private bool HttpEnabled = false;
        private bool HttpStream = false;
        private string HttpBuffer = "";
        private HttpRequest current_request;
        private bool bAwaitingPostData = false;

        private string _GetResponseText(HttpResponse.ResponseStatus status)
        {
            string text = "";

            switch (status)
            {
                case HttpResponse.ResponseStatus.Accepted:
                    text = "202 Accepted";
                    break;

                case HttpResponse.ResponseStatus.BadGateway:
                    text = "502 Bad Gateway";
                    break;

                case HttpResponse.ResponseStatus.BadRequest:
                    text = "400 Bad Gateway";
                    break;

                case HttpResponse.ResponseStatus.Conflict:
                    text = "409 Conflict";
                    break;

                case HttpResponse.ResponseStatus.Continue:
                    text = "100 Continue";
                    break;

                case HttpResponse.ResponseStatus.Created:
                    text = "201 Created";
                    break;

                case HttpResponse.ResponseStatus.ExpectationFailed:
                    text = "417 Expectation Fail";
                    break;

                case HttpResponse.ResponseStatus.Forbidden:
                    text = "403 Forbidden";
                    break;

                case HttpResponse.ResponseStatus.GatewayTimeout:
                    text = "504 Gateway Timeout";
                    break;

                case HttpResponse.ResponseStatus.Gone:
                    text = "410 Gone";
                    break;

                case HttpResponse.ResponseStatus.HTTPVersionNotSupported:
                    text = "505 HTTP Version Not Supported";
                    break;

                case HttpResponse.ResponseStatus.InternalServerError:
                    text = "500 Internal Server Error";
                    break;

                case HttpResponse.ResponseStatus.LengthRequired:
                    text = "411 Length Required";
                    break;

                case HttpResponse.ResponseStatus.MethodNotAllowed:
                    text = "405 Method Not Allowed";
                    break;

                case HttpResponse.ResponseStatus.NoContent:
                    text = "204 No Content";
                    break;

                case HttpResponse.ResponseStatus.NonAuthoritativeInformation:
                    text = "203 Non-Authoritative Information";
                    break;

                case HttpResponse.ResponseStatus.NotAcceptable:
                    text = "406 Not Acceptable";
                    break;

                case HttpResponse.ResponseStatus.NotFound:
                    text = "404 Not Found";
                    break;

                case HttpResponse.ResponseStatus.NotImplemented:
                    text = "501 Not Implemented";
                    break;

                case HttpResponse.ResponseStatus.OK:
                    text = "200 OK";
                    break;

                case HttpResponse.ResponseStatus.PreconditionFailed:
                    text = "412 Precondition Failed";
                    break;

                case HttpResponse.ResponseStatus.ProxyAuthenticationRequired:
                    text = "407 Proxy Authentication Required";
                    break;

                case HttpResponse.ResponseStatus.RequestedRangeNotSatisfiable:
                    text = "416 Requested Range Not Satisfiable";
                    break;

                case HttpResponse.ResponseStatus.RequestEntityTooLarge:
                    text = "413 Request Entity Too Large";
                    break;

                case HttpResponse.ResponseStatus.RequestTimeout:
                    text = "408 Request Timeout";
                    break;

                case HttpResponse.ResponseStatus.RequestUriTooLong:
                    text = "413 Request Entity Too Large";
                    break;

                case HttpResponse.ResponseStatus.ResetContent:
                    text = "205 Reset Content";
                    break;

                case HttpResponse.ResponseStatus.ServiceUnavailable:
                    text = "503 Service Unavailable";
                    break;

                case HttpResponse.ResponseStatus.SwitchingProtocols:
                    text = "101 Switching Protocols";
                    break;

                case HttpResponse.ResponseStatus.Unauthorized:
                    text = "401 Unauthorized";
                    break;

                case HttpResponse.ResponseStatus.UnsupportedMediaType:
                    text = "415 Unsupported Media Type";
                    break;
            }

            return text;
        }

        private void _ParseBufferForHttp(string buffer)
        {
            if (buffer.Length <= 0)
            {
                return;
            }

            //Remove connection open tag
            if (buffer.IndexOf("*OPEN*") >= 0)
                buffer = StringReplace("*OPEN*", "", buffer);

            //Remove connection close tag
            if (buffer.IndexOf("*CLOS*") >= 0)
                buffer = StringReplace("*CLOS*", "", buffer);

            if (!HttpStream && buffer.IndexOf("GET") >= 0)
                HttpStream = true;

            if (!HttpStream && buffer.IndexOf("POST") >= 0)
                HttpStream = true;

            if (HttpStream)
            {
                //Otherwise append to buffer
                HttpBuffer += buffer;

                int index = -1;
                if (bAwaitingPostData)
                {
                    int len;
                    StringToInt(current_request.HeaderData["Content-length"], out len);

                    if (HttpBuffer.Length >= len)
                    {
                        bAwaitingPostData = false;
                        HttpStream = false;

                        current_request.PostData = HttpBuffer;
                        OnHttpRequestReceived(current_request);
                    }
                }

                else if ((index = HttpBuffer.IndexOf("\r\n\r\n")) >= 0)
                {
                    //Set the current request
                    current_request = _ParseHttpHeader(HttpBuffer);

                    if (current_request.RequestType == HttpRequest.HttpRequestType.GET)
                    {
                        HttpBuffer = "";
                        HttpStream = false;

                        OnHttpRequestReceived(current_request);
                    }
                    else if (current_request.RequestType == HttpRequest.HttpRequestType.POST)
                    {
                        bAwaitingPostData = true;
                        HttpBuffer = HttpBuffer.Substring(index + 4);
                    }

                    //We can return without further parsing
                    return;
                }
            }
        }

        private ArrayList _ParsePostData(string buffer)
        {
            ArrayList list = new ArrayList();
            //Separate POST elements
            //int index = -1;
            string[] data = buffer.Split(new char[1] { '&' });
            foreach (string param in data)
            {
                if (param.Length > 0)
                {
                    int separate_index = param.IndexOf("=");

                    list.Add(
                        new DictionaryEntry(
                            _UrlDecode(param.Substring(0, separate_index)),
                            _UrlDecode(param.Substring(separate_index + 1))
                        )
                    );
                }
            }
            //while ((index = buffer.IndexOf("&")) >= 0)
            //{
            //    int separate_index = buffer.IndexOf("=");

            //    //Add the key/value pair
            //    list.Add(
            //        new DictionaryEntry(
            //            _UrlDecode(buffer.Substring(0, separate_index)),
            //            _UrlDecode(buffer.Substring(separate_index + 1, index))
            //        )
            //    );

            //    //shift the buffer forward
            //    buffer = buffer.Substring(index + 1);
            //}

            return list;
        }

        private string _UrlDecode(string buffer)
        {
            //There was no encoded data
            if (buffer.IndexOf("%") < 0)
                return buffer;

            StringBuilder build_buffer = new StringBuilder();

            string working_buffer = "";

            int index = -1;
            while ((index = buffer.IndexOf("%")) >= 0)
            {
                //Add data before encoded char
                build_buffer.Append(buffer.Substring(0, index));

                //Grab the encoded char
                working_buffer = buffer.Substring(index + 1, 2);

                //Shift the buffer forward
                buffer = buffer.Substring(index + 3);

                byte high;
                byte low;

                if (working_buffer[0] >= 'A')
                    high = (byte)(working_buffer[0] - 48 - 7);
                else
                    high = (byte)(working_buffer[0] - 48);

                if (working_buffer[1] >= 'A')
                    low = (byte)(working_buffer[1] - 48 - 7);
                else
                    low = (byte)(working_buffer[1] - 48);

                byte num = 0;
                num = (byte)(high << 4);
                num = (byte)(num | (low));

                //Add the new byte to the string
                build_buffer.Append(num);

            }

            return build_buffer.ToString();
        }

        private HttpRequest _ParseHttpHeader(string buffer)
        {
            HttpRequest request = new HttpRequest();

            //Get the request type
            if (buffer.IndexOf("GET") >= 0)
                request.RequestType = HttpRequest.HttpRequestType.GET;
            else if (buffer.IndexOf("POST") >= 0)
                request.RequestType = HttpRequest.HttpRequestType.POST;

            //Get the requested document
            int start_index = buffer.IndexOf("/");
            int end_index = start_index >= 0 ? buffer.IndexOf(" ", start_index) : -1;

            if (start_index >= 0 && end_index > start_index)
            {
                int i = buffer.IndexOf("?");

                if (i > 0)
                {
                    string query = buffer.Substring(i + 1, end_index - (i + 1));
                    request.URL = buffer.Substring(start_index, i - start_index);

                    var paras = query.Split('=', '&');

                    for (int j = 0; j < paras.Length; j += 2)
                        request.QueryData[_UrlDecode(paras[j])] = _UrlDecode(paras[j + 1]);
                }
                else
                {
                    request.URL = buffer.Substring(start_index, end_index - start_index);
                }

                //Shift the buffer forward
                buffer = buffer.Substring(buffer.IndexOf("\r\n") + 2);
            }

            //Inflate the individual elements
            int index = -1;
            string line_buffer = "";
            bool keep_check = true;

            while ((index = buffer.IndexOf("\r\n")) >= 0)
            {
                //For POST headers, we need to determine the length of the post data
                if (request.RequestType == HttpRequest.HttpRequestType.POST && keep_check)
                {
                    line_buffer = buffer.Substring(0, index);

                    if (line_buffer.IndexOf("Content-Length:") == 0)
                    {
                        StringToInt(line_buffer.Substring(16), out request.PostLength);

                        //Once we have the content length, we no longer need to parse
                        //the individual lines
                        keep_check = false;
                    }

                    if (line_buffer.IndexOf(": ") > 0)
                    {
                        request.HeaderData[line_buffer.Substring(0, line_buffer.IndexOf(": "))]
                            =
                            line_buffer.Substring((line_buffer.IndexOf(": ") + 2));
                    }
                }
                else
                {
                    line_buffer = buffer.Substring(0, index);

                    //Add the element
                    if (line_buffer.IndexOf(": ") > 0)
                    {
                        request.HeaderData[line_buffer.Substring(0, line_buffer.IndexOf(": "))]
                            =
                            line_buffer.Substring((line_buffer.IndexOf(": ") + 2));
                    }

                    line_buffer = "";
                }

                //Shift the buffer forward
                buffer = buffer.Substring(index + 2);
            }

            return request;
        }

        #endregion

        #region "Command Mode"

        //Attempt to execute a command
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="Command">The command to execute.</param>
        /// <returns>Whether or not it was successful</returns>
        public bool _Command_Execute(string Command)
        {
            //Append the return
            if (Command.IndexOf("\r") < 0)
                Command += "\r";

            _Command_Mode_Write(Command);

            Thread.Sleep(10);

            //Wait until the device has responded to the last command
            _TimeOutDate = DateTime.Now.AddMilliseconds((double)_Timeout);
            while (!_Command_Mode_Response_Complete)
            {
                //Did we time-out?
                if (_TimeOutDate <= DateTime.Now && _Timeout > 0)
                    return false;

                Thread.Sleep(1);
            }

            return _Command_Mode_Response_Okay;
        }

        //Initiate command mode
        /// <summary>
        /// Starts command mode.
        /// </summary>
        /// <returns>Whether or not it was successful</returns>
        public bool _Command_Mode_Start()
        {
            Thread.Sleep(100);

            _Command_Mode_Write(_Command_Init);

            //Wait until we are in command mode
            _TimeOutDate = DateTime.Now.AddMilliseconds((double)_Timeout);
            while (_stream != StreamMode.CommandStream)
            {
                //Did we time-out?
                if (_TimeOutDate <= DateTime.Now && _Timeout > 0)
                    return false;

                Thread.Sleep(1);
            }

            return true;
        }

        //Exit command mode
        /// <summary>
        /// Exits command mode.
        /// </summary>
        /// <returns>Whether or not it was successful</returns>
        public bool _Command_Mode_Exit()
        {
            _Command_Mode_Write("exit\r");

            //Wait until we are out of command mode
            _TimeOutDate = DateTime.Now.AddMilliseconds((double)_Timeout);
            while (_stream == StreamMode.CommandStream)
            {
                //Did we time-out?
                if (_TimeOutDate <= DateTime.Now && _Timeout > 0)
                    return false;

                Thread.Sleep(1);
            }

            return true;
        }

        //Command write string method
        /// <summary>
        /// Write to the device.
        /// </summary>
        /// <param name="Command">The command to write.</param>
        /// <returns>Whether or not it was successful</returns>
        public void _Command_Mode_Write(string Command)
        {
            //Await response
            _Command_Mode_Response = "";
            _Command_Mode_Response_Complete = false;

            //Convert string to byte array
            byte[] _Command = System.Text.Encoding.UTF8.GetBytes(Command);
            _wifly.Write(_Command, 0, _Command.Length);

            return;
        }

        //Command write bytes method
        /// <summary>
        /// Write to the device.
        /// </summary>
        /// <param name="Command">The command to write.</param>
        /// <returns>Whether or not it was successful</returns>
        public void _Command_Mode_Write(byte[] Command)
        {
            //Await response
            _Command_Mode_Response = "";
            _Command_Mode_Response_Complete = false;

            _wifly.Write(Command, 0, Command.Length);
            return;
        }

        //Command get method
        private void _Command_Mode_Get(string Command)
        {
            //Enter command mode
            if (!_Command_Mode_Start())
                return;

            //Get the requested configuration
            if (!_Command_Execute("get " + Command))
                if (!_Command_Execute("get " + Command))
                    return;

            _TimeOutDate = DateTime.Now.AddMilliseconds((double)_Timeout);
            while (!_Command_Mode_Response_Complete)
            {
                //Did we timeout?
                if (_TimeOutDate <= DateTime.Now && _Timeout > 0)
                    return;

                Thread.Sleep(1);
            }

            //Exit command mode
            if (!_Command_Mode_Exit())
                return;
        }

        #endregion

        #region "Serial Communications"

        /// <summary>
        /// Returns the raw underlying serial port object.
        /// </summary>
        public GTI.Serial SerialPort
        {
            get
            {
                return _wifly;
            }
        }

        //Threaded listener for incomming data
        private void _Serial_Listen()
        {
            _Serial_Byte_Buffer = new byte[1024];

            while (true)
            {
                if (_wifly.BytesToRead > 0)
                {
                    int i = _wifly.Read(_Serial_Byte_Buffer, 0, 1024);

                    //RTS.Write(true);
                    _Serial_Data_Received(_Serial_Byte_Buffer, i);
                    //RTS.Write(false);
                }
                Thread.Sleep(50);
            }
        }

        //Data received
        private void _Serial_Data_Received(byte[] data, int size)
        {
            //Convert bytes into an indexable string
            string line = new string(System.Text.Encoding.UTF8.GetChars(data, 0, size));

            if (line == null)
            {
                return;
            }

            _Serial_String_Buffer += line;

            //Report all incomming data to the debug
            if (_debug_level == DebugLevel.DebugAll)
            {
                _Print_Debug(line);
            }

            //Handle Connection open request tags
            if (_Serial_String_Buffer.IndexOf("*OPEN*") >= 0)
            {
                _Serial_String_Buffer = StringReplace("*OPEN*", "", _Serial_String_Buffer);
                OnConnectionEstablished();
            }

            //Handle Connection close request tags
            else if (_Serial_String_Buffer.IndexOf("*CLOS*") >= 0)
            {
                _Serial_String_Buffer = StringReplace("*CLOS*", "", _Serial_String_Buffer);
                OnConnectionClosed();
            }

            if (_Serial_String_Buffer.IndexOf("\r\r") >= 0)
            {
                _Serial_String_Buffer = StringReplace("\r\r", "\r", _Serial_String_Buffer);
            }

            //Is the buffer now a complete line of data?
            if (_Serial_String_Buffer.IndexOf("\r\n") >= 0)
            {
                int index = -1;
                while ((index = _Serial_String_Buffer.IndexOf("\r\n")) >= 0)
                {
                    string new_line = _Serial_String_Buffer.Substring(0, index);
                    _Serial_String_Buffer = _Serial_String_Buffer.Substring(index + 2);
                    new_line = new_line + "\r\n";

                    _Serial_Line_Received(new_line);
                }
            }

            //End of post-data
            if (current_request != null && bAwaitingPostData && _Serial_String_Buffer.Length >= current_request.PostLength)
            {
                _Serial_Line_Received(_Serial_String_Buffer);
            }

            OnDataReceived(line);
        }

        //Data received complete
        private void _Serial_Line_Received(string line)
        {

            if (line.IndexOf("*READY*") >= 0)
                _device_ready = true;

            //Are we entering command mode?
            if (line.Length >= 3)
            {
                if (_stream != StreamMode.CommandStream && line.Substring(0, 3) == "CMD")
                    _stream = StreamMode.CommandStream;
            }

            //Are we in command mode waiting for response?
            if (_stream == StreamMode.CommandStream)
            {
                //Append line to the response
                _Command_Mode_Response += line;

                //Are we leaving command mode?
                if (_Command_Mode_Response.IndexOf("EXIT") >= 0)
                {
                    _stream = StreamMode.NoStream;

                    //Report data to user-event
                    if (_data_level == DataReturnLevel.ReturnAll)
                        OnLineReceived(line);

                    //This is all we need to do with this event
                    return;
                }

                //Are we updating the firmware?
                if (_Command_Mode_Response.IndexOf("UPDATE OK") >= 0)
                {
                    _Command_Mode_Response_Okay = true;
                    _Command_Mode_Response_Complete = true;
                }

                //Are we updating the firmware?
                if (_Command_Mode_Response.IndexOf("Set Factory Defaults") >= 0)
                {
                    _Command_Mode_Response_Okay = true;
                    _Command_Mode_Response_Complete = true;
                }

                //Did the command execute without error?
                if (_Command_Mode_Response.IndexOf("AOK") >= 0)
                {
                    _Command_Mode_Response_Okay = true;
                    _Command_Mode_Response_Complete = true;
                }

                //Did the command execute with an error?
                else if (_Command_Mode_Response.IndexOf("ERR") >= 0)
                {
                    _Print_Debug("ERROR! --- " + line);

                    _Command_Mode_Response_Okay = false;
                    _Command_Mode_Response_Complete = true;
                }

                if (!_DHCP && _Command_Mode_Response.IndexOf("Associated!") >= 0)
                {
                    _Command_Mode_Response_Okay = true;
                    _Command_Mode_Response_Complete = true;
                }

                if (line.Length >= 3)
                {
                    if (line.Substring(0, 3) == "IP=")
                    {
                        string line_buffer = line.Substring(3);
                        string[] split_ip = line_buffer.Split(new char[] { ':' });
                        LocalIP = split_ip[0];

                        _Command_Mode_Response_Okay = true;
                        _Command_Mode_Response_Complete = true;
                    }

                    //Check to see if the appropriate firmware is loaded
                    if (line.Substring(0, 5) == "File=")
                    {
                        string line_buffer = line.Substring(5);
                        string[] split_ip = line_buffer.Split(new char[] { ':' });
                        LocalIP = split_ip[0];

                        _Command_Mode_Response_Okay = true;
                        _Command_Mode_Response_Complete = true;
                    }
                }

                //Report data to user-event
                if (_data_level == DataReturnLevel.ReturnAll)
                    OnLineReceived(line);
            }

            else
            {
                if (HttpEnabled)
                {
                    _ParseBufferForHttp(line);
                }

                OnLineReceived(line);
            }
        }

        #endregion

        #region "Debugging"
        private void _Print_Debug(string message)
        {
            switch (_debug)
            {
                //Do nothing
                case DebugMode.NoDebug:
                    break;

                //Output Debugging info to the serial port
                case DebugMode.SerialDebug:
                    //Convert the message to bytes
                    byte[] message_buffer = System.Text.Encoding.UTF8.GetBytes(message);
                    _debug_port.Write(message_buffer, 0, message_buffer.Length);
                    break;

                //Print message to the standard debug output
                case DebugMode.StandardDebug:
                    Debug.Print(message);
                    break;
            }
        }

        /// <summary>
        /// Set the serial port to be used as the debugging output.
        /// This is much faster than using Debug.Print
        /// </summary>
        /// <param name="DebugPort">The serial port</param>
        public void SetDebugPort(Gadgeteer.Interfaces.Serial DebugPort)
        {
            _debug_port = DebugPort;

            //Open the serial port if it is not already
            if (!_debug_port.IsOpen)
                _debug_port.Open();

            _debug = DebugMode.SerialDebug;
        }

        /// <summary>
        /// Set the debugging level.
        /// DebugLevel.DebugErrors only report error data
        /// DebugLevel.DebugAll will report all incoming and outgoing data
        /// </summary>
        /// <param name="Debug_Level">The debug level</param>
        public void SetDebugLevel(DebugLevel Debug_Level)
        {
            _debug_level = Debug_Level;
        }
        #endregion

        #region "Timeout"
        /// <summary>
        /// Set the timeout for module communications
        /// </summary>
        /// <param name="Timeout">The amount of time in ms before timeout occurs</param>
        public void SetTimeout(int Timeout = 3000)
        {
            _Timeout = Timeout;
        }
        #endregion

        #region "Logical Tools"

        //This is needed because String.IndexOf is returning false randomly
        int IndexIn(string needle, string haystack)
        {
            int found_index = -1;
            int needle_length = needle.Length;

            for (int i = 0; i < haystack.Length; i++)
            {
                if ((i + needle_length) < haystack.Length)
                {
                    if (haystack.Substring(i, needle_length) == needle)
                    {
                        found_index = i;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return found_index;
        }

        //String replace method
        string StringReplace(string token, string text, string haystack)
        {
            string left = "";
            string right = "";
            string buffer = haystack;
            int index = buffer.IndexOf(token);

            while (index >= 0)
            {
                int other_index = index + token.Length;

                left = buffer.Substring(0, index);
                right = buffer.Substring(other_index);
                buffer = left + text + right;

                index = buffer.IndexOf(token);
            }

            return buffer;
        }

        //Qt style StartsWith function with PHP string function syntax
        private bool StartsWith(string needle, string haystack)
        {
            //Avoid Out of range exceptions
            if (needle.Length > haystack.Length)
                return false;

            //Grab the beginning of the string
            string buffer = haystack.Substring(0, needle.Length);

            //Does the substring match the needle?
            return buffer == needle ? true : false;
        }

        //Exception-less string to int method
        private bool StringToInt(string str, out int integer)
        {
            int total = 0;
            bool bOkay = true;

            for (int i = 0; i < str.Length; i++)
            {
                uint temp = str[i];
                temp = temp - 48;

                if (temp > 9 || temp < 0)
                {
                    bOkay = false;
                    break;
                }

                total = total * 10;
                total = total + (int)temp;
            }

            integer = bOkay ? total : 0;

            return bOkay;
        }

        #endregion


    }
}

