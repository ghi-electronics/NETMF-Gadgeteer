using Microsoft.SPOT;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using Encoder = System.Text.Encoding;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A WiFiRN171 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class WiFiRN171 : GTM.Module
    {
        /// <summary>
        /// An exception thrown when a timeout occurs.
        /// </summary>
        [Serializable]
        public class TimeoutException : global::System.Exception
        {
            internal TimeoutException() : base() { }
            internal TimeoutException(string message) : base(message) { }
            internal TimeoutException(string message, Exception innerException) : base(message, innerException) { }
        }
        /// <summary>
        /// An exception thrown when an errror is received.
        /// </summary>
        [Serializable]
        public class ErrorReceivedException : global::System.Exception
        {
            internal ErrorReceivedException() : base() { }
            internal ErrorReceivedException(string message) : base(message) { }
            internal ErrorReceivedException(string message, Exception innerException) : base(message, innerException) { }
        }

        private string commandModeResponse;
        private string serialBuffer;
        private int timeout;
        private bool useDhcp;
        private bool inCommandMode;
        private AutoResetEvent updateComplete;
        private AutoResetEvent commandResponseComplete;
        private GTI.DigitalOutput reset;
        private GTI.DigitalOutput rts;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public WiFiRN171(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            this.LocalIP = "0.0.0.0";
            this.LocalListenPort = "0";
            this.DebugPort = null;
            this.Ready = false;

            var useFlowControlEnabled = socket.SupportsType('K');

            this.commandModeResponse = "";
            this.serialBuffer = "";
            this.timeout = 10000;
            this.useDhcp = false;
            this.inCommandMode = false;
            this.updateComplete = new AutoResetEvent(false);
            this.commandResponseComplete = new AutoResetEvent(false);

            this.onHttpRequestReceived = this.OnHttpRequestReceived;
            this.onDataReceived = this.OnDataReceived;
            this.onLineReceived = this.OnLineReceived;
            this.onConnectionEstablished = this.OnConnectionEstablished;
            this.onConnectionClosed = this.OnConnectionClosed;

            if (!useFlowControlEnabled)
                socket.EnsureTypeIsSupported('U', this);
            
            this.reset = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three, true, this);
            this.rts = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Six, false, this);
            this.SerialPort = GTI.SerialFactory.Create(socket, 115200, GTI.SerialParity.None, GTI.SerialStopBits.One, 8, useFlowControlEnabled ? GTI.HardwareFlowControl.Required : GTI.HardwareFlowControl.NotRequired, this);
            this.SerialPort.DataReceived += this.OnSerialDataReceived;
            this.SerialPort.Open();

            this.Reset();

            var end = DateTime.Now.AddMilliseconds(this.timeout);
            while (!this.Ready)
            {
                if (end <= DateTime.Now && this.timeout > 0)
                    throw new TimeoutException();

                Thread.Sleep(1);
            }

            this.EnterCommandMode();

            this.ExecuteCommand("set sys printlvl 0");
            this.ExecuteCommand("set comm remote 0");

            if (useFlowControlEnabled)
                this.ExecuteCommand("set uart flow 1");

            this.ExecuteCommand("set uart tx 1");

            this.ExitCommandMode();
        }

        /// <summary>
        /// The baudrate of the device.
        /// </summary>
        public int BaudRate
        {
            get
            {
                return this.SerialPort.BaudRate;
            }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value", "value must be positive.");

                this.SerialPort.Close();
                this.SerialPort.BaudRate = value;
                this.SerialPort.Open();
            }
        }

        /// <summary>
        /// The timeout of the device.
        /// </summary>
        public int Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value", "value must be positive.");

                this.timeout = value;
            }
        }

        /// <summary>
        /// If the device is ready.
        /// </summary>
        public bool Ready { get; private set; }

        /// <summary>
        /// The local IP of the device.
        /// </summary>
        public string LocalIP { get; private set; }

        /// <summary>
        /// The local listening port of the device.
        /// </summary>
        public string LocalListenPort { get; private set; }

        /// <summary>
        /// The raw underlying serial port object.
        /// </summary>
        public GTI.Serial SerialPort { get; private set; }

        /// <summary>
        /// The port to print debug messages to when DebugPrintEnabled is true. If null, messages are sent to Debug.Print.
        /// </summary>
        public GTI.Serial DebugPort { get; set; }

        /// <summary>
        /// Reboots the module.
        /// </summary>
        public void Reboot()
        {
            this.SerialPort.DiscardInBuffer();
            this.SerialPort.DiscardOutBuffer();

            this.Ready = false;

            this.Reset();

            var end = DateTime.Now.AddMilliseconds(this.timeout);
            while (!this.Ready)
            {
                if (end <= DateTime.Now && this.timeout > 0)
                    throw new TimeoutException();

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Updates the firmware.
        /// </summary>
        /// <returns>Whether it was successful or not.</returns>
        public bool UpdateFirmware()
        {
            this.ExitCommandMode();
            this.EnterCommandMode();

            this.ExecuteCommand("set ftp address 0");
            this.ExecuteCommand("set dns name rn.microchip.com");
            this.ExecuteCommand("save");
            
            this.ExecuteCommand("ftp update wifly7-245.img");

            if (!this.updateComplete.WaitOne(this.timeout, true))
                throw new TimeoutException();

            return true;
        }

        /// <summary>
        /// Creates an access point with the given SSID.
        /// </summary>
        /// <param name="ssid">The SSID to use.</param>
        public void CreateAccessPoint(string ssid)
        {
            if (ssid == null) throw new ArgumentNullException("ssid");

            this.ExitCommandMode();

            this.EnterCommandMode();

            this.ExecuteCommand("set wlan channel 2");
            this.ExecuteCommand("set wlan join 7");
            this.ExecuteCommand("set ip address 192.168.1.1");
            this.ExecuteCommand("set ip gateway 192.168.1.1");
            this.ExecuteCommand("set ip netmask 255.255.255.0");
            this.ExecuteCommand("set ip dhcp 4");
            this.ExecuteCommand("join " + ssid);
            
            this.ExitCommandMode();
        }

        /// <summary>
        /// Enables DHCP mode.
        /// </summary>
        /// <param name="gateway">The gateway address.</param>
        /// <param name="subnetMask">The subnet mask.</param>
        /// <param name="dnsAddress">The DNS address.</param>
        public void EnableDhcp(string gateway, string subnetMask, string dnsAddress)
        {
            if (gateway == null) throw new ArgumentNullException("gateway");
            if (subnetMask == null) throw new ArgumentNullException("subnetMask");
            if (dnsAddress == null) throw new ArgumentNullException("dnsAddress");

            this.useDhcp = true;

            this.EnterCommandMode();

            this.ExecuteCommand("set ip dhcp 1");
            this.ExecuteCommand("set ip gateway " + gateway);
            this.ExecuteCommand("set ip netmask " + subnetMask);
            this.ExecuteCommand("set dns address " + dnsAddress);

            this.ExitCommandMode();
        }

        /// <summary>
        /// Enables static ip mode.
        /// </summary>
        /// <param name="ipAddress">The ip address</param>
        /// <param name="gateway">The gateway address.</param>
        /// <param name="subnetMask">The subnet mask.</param>
        /// <param name="dnsAddress">The DNS address.</param>
        public void EnableStaticIP(string ipAddress, string gateway, string subnetMask, string dnsAddress)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");
            if (gateway == null) throw new ArgumentNullException("gateway");
            if (subnetMask == null) throw new ArgumentNullException("subnetMask");
            if (dnsAddress == null) throw new ArgumentNullException("dnsAddress");

            this.useDhcp = false;

            this.EnterCommandMode();

            this.ExecuteCommand("set ip dhcp 0");
            this.ExecuteCommand("set ip address " + ipAddress);
            this.ExecuteCommand("set ip gateway " + gateway);
            this.ExecuteCommand("set ip netmask " + subnetMask);
            this.ExecuteCommand("set dns address " + dnsAddress);

            this.ExitCommandMode();
        }

        /// <summary>
        /// Attempt to join a wireless network with given parameters.
        /// </summary>
        /// <param name="ssid">The ssid.</param>
        /// <param name="passphrase">The passphrase.</param>
        public void JoinWirelessNetwork(string ssid, string passphrase)
        {
            this.JoinWirelessNetwork(ssid, passphrase, 0, WirelessEncryptionMode.Open);
        }

        /// <summary>
        /// Attempt to join a wireless network with given parameters.
        /// </summary>
        /// <param name="ssid">The ssid.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="authenticationMode">The authentication mode.</param>
        public void JoinWirelessNetwork(string ssid, string passphrase, int channel, WirelessEncryptionMode authenticationMode)
        {
            if (ssid == null) throw new ArgumentNullException("ssid");
            if (passphrase == null) throw new ArgumentNullException("passphrase");
            if (channel < 0) throw new ArgumentOutOfRangeException("channel", "channel must be non-negative.");

            this.EnterCommandMode();

            this.ExecuteCommand("set wlan ssid " + ssid);
            this.ExecuteCommand("set wlan channel " + channel.ToString());
            this.ExecuteCommand("set wlan auth " + authenticationMode.ToString());
            this.ExecuteCommand("set wlan phrase " + passphrase);
            this.ExecuteCommand("join");
            
            if (this.useDhcp)
                this.ExecuteCommand("get ip");

            this.ExitCommandMode();
        }

        /// <summary>
        /// Sets the socket protocol.
        /// </summary>
        /// <param name="protocol">The desired protocol.</param>
        public void SetProtocol(SocketProtocol protocol)
        {
            this.EnterCommandMode();
            this.ExecuteCommand("set ip protocol " + protocol.ToString());
            this.ExitCommandMode();
        }

        /// <summary>
        /// Set the port the WiFly module should listen for connections on.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        public void SetListenPort(int port)
        {
            if (port < 0) throw new ArgumentOutOfRangeException("port", "port must be non-negative.");

            this.EnterCommandMode();
            this.ExecuteCommand("set ip local " + port.ToString());
            this.ExitCommandMode();
        }

        /// <summary>
        /// Sets the device to listen port on port 80 and allow HTTP request parsing.
        /// </summary>
        public void EnableHttpServer()
        {
            this.EnterCommandMode();
            this.ExecuteCommand("set ip local 80");
            this.ExitCommandMode();
            
            this.HttpEnabled = true;
        }

        /// <summary>
        /// Sends data to the currently connected client.
        /// </summary>
        /// <param name="data">The data to send.</param>
        public void Send(byte[] data)
        {
            this.Send(data, 0, data.Length);
        }

        /// <summary>
        /// Sends data to the currently connected client.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="offset">The offset into the buffer to send at.</param>
        /// <param name="count">The number of bytes to send.</param>
        public void Send(byte[] data, int offset, int count)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must be at least zero.");
            if (count <= 0) throw new ArgumentOutOfRangeException("count", "count must be positive.");
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException("buffer", "buffer.Length must be at least offset + count.");

            while (this.SerialPort.BytesToWrite > 0)
                Thread.Sleep(10);

            this.SerialPort.Write(data, offset, count);
        }

        #region "HTTP Parser"

        private bool HttpEnabled = false;
        private bool HttpStream = false;
        private string HttpBuffer = "";
        private HttpRequest currentRequest;
        private bool awaitingPostData = false;

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

        private void ParseBufferForHttp(string buffer)
        {
            if (buffer.Length <= 0)
            {
                return;
            }

            //Remove connection open tag
            if (buffer.IndexOf("*OPEN*") >= 0)
                buffer = Replace("*OPEN*", "", buffer);

            //Remove connection close tag
            if (buffer.IndexOf("*CLOS*") >= 0)
                buffer = Replace("*CLOS*", "", buffer);

            if (!HttpStream && buffer.IndexOf("GET") >= 0)
                HttpStream = true;

            if (!HttpStream && buffer.IndexOf("POST") >= 0)
                HttpStream = true;

            if (HttpStream)
            {
                //Otherwise append to buffer
                HttpBuffer += buffer;

                int index = -1;
                if (awaitingPostData)
                {
                    int len;
                    ParseInt(currentRequest.HeaderData["Content-length"], out len);

                    if (HttpBuffer.Length >= len)
                    {
                        awaitingPostData = false;
                        HttpStream = false;

                        currentRequest.PostData = HttpBuffer;
                        OnHttpRequestReceived(this, new HttpStream(currentRequest, this.SerialPort));
                    }
                }

                else if ((index = HttpBuffer.IndexOf("\r\n\r\n")) >= 0)
                {
                    //Set the current request
                    currentRequest = _ParseHttpHeader(HttpBuffer);

                    if (currentRequest.RequestType == HttpRequest.HttpRequestType.GET)
                    {
                        HttpBuffer = "";
                        HttpStream = false;

                        OnHttpRequestReceived(this, new HttpStream(currentRequest, this.SerialPort));
                    }
                    else if (currentRequest.RequestType == HttpRequest.HttpRequestType.POST)
                    {
                        awaitingPostData = true;
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
                        ParseInt(line_buffer.Substring(16), out request.PostLength);

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

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void ExecuteCommand(string command)
        {
            if (command == null) throw new ArgumentNullException("command");

            if (command.IndexOf("\r") < 0)
                command += "\r";

            this.WriteCommand(command);

            Thread.Sleep(10);

            if (!this.commandResponseComplete.WaitOne(this.timeout, true))
                throw new TimeoutException();
        }

        /// <summary>
        /// Enters command mode.
        /// </summary>
        public void EnterCommandMode()
        {
            Thread.Sleep(100);

            this.WriteCommand("$$$");

            var end = DateTime.Now.AddMilliseconds(this.timeout);
            while (!this.inCommandMode)
            {
                if (end <= DateTime.Now && this.timeout > 0)
                    throw new TimeoutException();

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Exits command mode.
        /// </summary>
        public void ExitCommandMode()
        {
            this.WriteCommand("exit\r");

            var end = DateTime.Now.AddMilliseconds(this.timeout);
            while (this.inCommandMode)
            {
                if (end <= DateTime.Now && this.timeout > 0)
                    throw new TimeoutException();

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Writes to the device.
        /// </summary>
        /// <param name="command">The command to write.</param>
        /// <returns>Whether or not it was successful</returns>
        public void WriteCommand(string command)
        {
            if (command == null) throw new ArgumentNullException("command");

            this.WriteCommand(Encoder.UTF8.GetBytes(command));
        }

        /// <summary>
        /// Writes to the device.
        /// </summary>
        /// <param name="command">The command to write.</param>
        /// <returns>Whether or not it was successful</returns>
        public void WriteCommand(byte[] command)
        {
            if (command == null) throw new ArgumentNullException("command");

            this.commandModeResponse = "";

            this.SerialPort.Write(command);
        }

        private void GetCommand(string command)
        {
            this.EnterCommandMode();

            this.ExecuteCommand("get " + command);
            this.ExecuteCommand("get " + command);

            if (!this.commandResponseComplete.WaitOne(this.timeout, true))
                throw new TimeoutException();

            this.ExitCommandMode();
        }

        private void OnSerialDataReceived(GTI.Serial sender)
        {
            var available = this.SerialPort.BytesToRead;
            var data = new byte[available];
            var size = this.SerialPort.Read(data, 0, available);

            string line = new string(Encoder.UTF8.GetChars(data, 0, size));

            if (line == null)
                return;

            this.serialBuffer += line;

            this.Log(line);

            if (this.serialBuffer.IndexOf("*OPEN*") >= 0)
            {
                this.serialBuffer = this.Replace("*OPEN*", "", this.serialBuffer);
                this.OnConnectionEstablished(this, null);
            }

            if (this.serialBuffer.IndexOf("*CLOS*") >= 0)
            {
                this.serialBuffer = this.Replace("*CLOS*", "", this.serialBuffer);
                this.OnConnectionClosed(this, null);
            }

            if (this.serialBuffer.IndexOf("\r\r") >= 0)
            {
                this.serialBuffer = this.Replace("\r\r", "\r", this.serialBuffer);
            }

            if (this.serialBuffer.IndexOf("\r\n") >= 0)
            {
                int index = -1;

                while ((index = this.serialBuffer.IndexOf("\r\n")) >= 0)
                {
                    string newLine = this.serialBuffer.Substring(0, index) + "\r\n";

                    this.serialBuffer = this.serialBuffer.Substring(index + 2);

                    this.OnSerialLineReceived(newLine);
                }
            }

            if (this.currentRequest != null && this.awaitingPostData && this.serialBuffer.Length >= this.currentRequest.PostLength)
            {
                this.OnSerialLineReceived(this.serialBuffer);
            }

            this.OnDataReceived(this, line);
        }

        private void OnSerialLineReceived(string line)
        {
            if (line.IndexOf("*READY*") >= 0)
                this.Ready = true;

            if (line.Length >= 3 && !this.inCommandMode && line.Substring(0, 3) == "CMD")
                this.inCommandMode = true;

            if (this.inCommandMode)
            {
                this.commandModeResponse += line;

                if (this.commandModeResponse.IndexOf("EXIT") >= 0)
                {
                    this.inCommandMode = false;

                    return;
                }

                if (this.commandModeResponse.IndexOf("UPDATE OK") >= 0)
                {
                    this.updateComplete.Set();
                    this.commandResponseComplete.Set();
                }

                if (this.commandModeResponse.IndexOf("Set Factory Defaults") >= 0)
                    this.commandResponseComplete.Set();

                if (this.commandModeResponse.IndexOf("AOK") >= 0)
                    this.commandResponseComplete.Set();

                if (this.commandModeResponse.IndexOf("ERR") >= 0)
                {
                    this.commandResponseComplete.Set();

                    throw new ErrorReceivedException(line);
                }

                if (!this.useDhcp && this.commandModeResponse.IndexOf("Associated!") >= 0)
                    this.commandResponseComplete.Set();

                if (line.Length >= 3)
                {
                    if (line.Substring(0, 3) == "IP=")
                    {
                        this.LocalIP = line.Substring(3).Split(':')[0];

                        this.commandResponseComplete.Set();
                    }

                    //Check to see if the appropriate firmware is loaded
                    if (line.Substring(0, 5) == "File=")
                    {
                        this.LocalIP = line.Substring(5).Split(':')[0];

                        this.commandResponseComplete.Set();
                    }
                }
            }
            else
            {
                if (this.HttpEnabled)
                    this.ParseBufferForHttp(line);

                this.OnLineReceived(this, line);
            }
        }

        private void Log(string message)
        {
            if (!this.DebugPrintEnabled)
                return;

            if (this.DebugPort == null)
                Debug.Print(message);
            else
                this.DebugPort.Write(Encoder.UTF8.GetBytes(message));
        }

        private void Reset()
        {
            this.reset.Write(false);
            Thread.Sleep(100);

            this.reset.Write(true);
            Thread.Sleep(250);
        }

        private string Replace(string oldValue, string newValue, string source)
        {
            return new StringBuilder(source).Replace(oldValue, newValue).ToString();
        }

        private bool ParseInt(string source, out int result)
        {
            try
            {
                result = int.Parse(source);

                return true;
            }
            catch
            {
                result = 0;

                return false;
            }
        }

        /// <summary>
        /// A delegate representing receipt of an HTTP request.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">The HTTP request.</param>
        public delegate void HttpRequestReceivedHandler(WiFiRN171 sender, HttpStream e);

        /// <summary>
        /// A delegate representing data received.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">The data received.</param>
        public delegate void DataReceivedHandler(WiFiRN171 sender, string e);

        /// <summary>
        /// A delegate representing line received.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">The line received.</param>
        public delegate void LineReceivedHandler(WiFiRN171 sender, string e);

        /// <summary>
        /// A delegate representing connection opening.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void ConnectionEstablishedHandler(WiFiRN171 sender, EventArgs e);

        /// <summary>
        /// A delegate representing connection closure.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void ConnectionClosedHandler(WiFiRN171 sender, EventArgs e);

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
        /// Fired when an connection has been establised to a remote client.
        /// </summary>
        public event ConnectionEstablishedHandler ConnectionEstablished;

        /// <summary>
        /// Fired when an connection to a remote client has closed.
        /// </summary>
        public event ConnectionClosedHandler ConnectionClosed;

        private HttpRequestReceivedHandler onHttpRequestReceived;
        private DataReceivedHandler onDataReceived;
        private LineReceivedHandler onLineReceived;
        private ConnectionEstablishedHandler onConnectionEstablished;
        private ConnectionClosedHandler onConnectionClosed;

        private void OnHttpRequestReceived(WiFiRN171 sender, HttpStream e)
        {
            if (Program.CheckAndInvoke(this.HttpRequestReceived, this.onHttpRequestReceived, sender, e))
                this.HttpRequestReceived(sender, e);
        }

        private void OnDataReceived(WiFiRN171 sender, string e)
        {
            if (Program.CheckAndInvoke(this.DataReceived, this.onDataReceived, sender, e))
                this.DataReceived(sender, e);
        }

        private void OnLineReceived(WiFiRN171 sender, string e)
        {
            if (Program.CheckAndInvoke(this.LineReceived, this.onLineReceived, sender, e))
                this.LineReceived(sender, e);
        }

        private void OnConnectionEstablished(WiFiRN171 sender, EventArgs e)
        {
            if (Program.CheckAndInvoke(this.ConnectionEstablished, this.onConnectionEstablished, sender, e))
                this.ConnectionEstablished(sender, e);
        }

        private void OnConnectionClosed(WiFiRN171 sender, EventArgs e)
        {
            if (Program.CheckAndInvoke(this.ConnectionClosed, this.onConnectionClosed, sender, e))
                this.ConnectionClosed(sender, e);
        }

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
            TCPServer = 2,

            /// <summary>
            /// Secure Connection Mode: Only send to the stored host-ip
            /// </summary>
            SecureConnection = 4,

            /// <summary>
            /// TCP Client Mode: TCP Connection with handshaking (Client Only)
            /// </summary>
            TCPClient = 8
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
            /// <summary>Mixed WPA1 and WPA2-PSK</summary>
            MixedWPA1_WPA2 = 3,
            /// <summary>Wi-Fi Protected Access (WPA) II (uses preshared key)</summary>
            WPA2_PSK = 4
        }
    }
}