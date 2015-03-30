using Microsoft.SPOT;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using Encoder = System.Text.Encoding;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A WiFiRN171 module for Microsoft .NET Gadgeteer</summary>
	[Obsolete]
	public class WiFiRN171 : GTM.Module {
		private string commandModeResponse;
		private string serialBuffer;
		private int timeout;
		private bool useDhcp;
		private bool inCommandMode;
		private AutoResetEvent updateComplete;
		private AutoResetEvent commandResponseComplete;
		private GTI.DigitalOutput reset;
		private GTI.DigitalOutput rts;
		private bool awaitingPostData;
		private bool httpEnabled;
		private string httpBuffer;
		private HttpRequest currentRequest;
		private HttpRequestReceivedHandler onHttpRequestReceived;
		private DataReceivedHandler onDataReceived;
		private LineReceivedHandler onLineReceived;
		private ConnectionEstablishedHandler onConnectionEstablished;
		private ConnectionClosedHandler onConnectionClosed;

		/// <summary>A delegate representing receipt of an HTTP request.</summary>
		/// <param name="sender">The object that sent this event.</param>
		/// <param name="e">The HTTP session.</param>
		public delegate void HttpRequestReceivedHandler(WiFiRN171 sender, HttpSession e);

		/// <summary>A delegate representing data received.</summary>
		/// <param name="sender">The object that sent this event.</param>
		/// <param name="e">The data received.</param>
		public delegate void DataReceivedHandler(WiFiRN171 sender, string e);

		/// <summary>A delegate representing line received.</summary>
		/// <param name="sender">The object that sent this event.</param>
		/// <param name="e">The line received.</param>
		public delegate void LineReceivedHandler(WiFiRN171 sender, string e);

		/// <summary>A delegate representing connection opening.</summary>
		/// <param name="sender">The object that sent this event.</param>
		/// <param name="e">The event arguments.</param>
		public delegate void ConnectionEstablishedHandler(WiFiRN171 sender, EventArgs e);

		/// <summary>A delegate representing connection closure.</summary>
		/// <param name="sender">The object that sent this event.</param>
		/// <param name="e">The event arguments.</param>
		public delegate void ConnectionClosedHandler(WiFiRN171 sender, EventArgs e);

		/// <summary>Fired when a HTTP Request is received</summary>
		public event HttpRequestReceivedHandler HttpRequestReceived;

		/// <summary>Fired when any data is received</summary>
		public event DataReceivedHandler DataReceived;

		/// <summary>Fired when a complete line of data has been received</summary>
		public event LineReceivedHandler LineReceived;

		/// <summary>Fired when an connection has been establised to a remote client.</summary>
		public event ConnectionEstablishedHandler ConnectionEstablished;

		/// <summary>Fired when an connection to a remote client has closed.</summary>
		public event ConnectionClosedHandler ConnectionClosed;

		/// <summary>The baudrate of the device.</summary>
		public int BaudRate {
			get {
				return this.SerialPort.BaudRate;
			}

			set {
				if (value <= 0) throw new ArgumentOutOfRangeException("value", "value must be positive.");

				this.SerialPort.Close();
				this.SerialPort.BaudRate = value;
				this.SerialPort.Open();
			}
		}

		/// <summary>The timeout of the device.</summary>
		public int Timeout {
			get {
				return this.timeout;
			}

			set {
				if (value <= 0) throw new ArgumentOutOfRangeException("value", "value must be positive.");

				this.timeout = value;
			}
		}

		/// <summary>If the device is ready.</summary>
		public bool Ready { get; private set; }

		/// <summary>The local IP of the device.</summary>
		public string LocalIP { get; private set; }

		/// <summary>The local listening port of the device.</summary>
		public string LocalListenPort { get; private set; }

		/// <summary>The raw underlying serial port object.</summary>
		public GTI.Serial SerialPort { get; private set; }

		/// <summary>The port to print debug messages to when DebugPrintEnabled is true. If null, messages are sent to Debug.Print.</summary>
		public GTI.Serial DebugPort { get; set; }

		/// <summary>Represents the socket protocal.</summary>
		public enum SocketProtocol {

			/// <summary>UPD Mode: Connection-less protocol with no handshaking</summary>
			UDP = 1,

			/// <summary>TCP Server Mode: TCP Connection with handshaking (Client and Server)</summary>
			TCPServer = 2,

			/// <summary>Secure Connection Mode: Only send to the stored host-ip</summary>
			SecureConnection = 4,

			/// <summary>TCP Client Mode: TCP Connection with handshaking (Client Only)</summary>
			TCPClient = 8
		}

		/// <summary>Represents the encyrption mode to use.</summary>
		public enum WirelessEncryptionMode {

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

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public WiFiRN171(int socketNumber) {
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);

			var useFlowControl = socket.SupportsType('K');

			this.LocalIP = "0.0.0.0";
			this.LocalListenPort = "0";
			this.DebugPort = null;
			this.Ready = false;

			this.commandModeResponse = "";
			this.serialBuffer = "";
			this.timeout = 10000;
			this.useDhcp = false;
			this.inCommandMode = false;
			this.updateComplete = new AutoResetEvent(false);
			this.commandResponseComplete = new AutoResetEvent(false);
			this.awaitingPostData = false;
			this.httpEnabled = false;
			this.httpBuffer = "";
			this.currentRequest = null;

			this.onHttpRequestReceived = this.OnHttpRequestReceived;
			this.onDataReceived = this.OnDataReceived;
			this.onLineReceived = this.OnLineReceived;
			this.onConnectionEstablished = this.OnConnectionEstablished;
			this.onConnectionClosed = this.OnConnectionClosed;

			if (!useFlowControl)
				socket.EnsureTypeIsSupported('U', this);

			this.reset = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three, true, this);
			this.rts = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Six, false, this);
			this.SerialPort = GTI.SerialFactory.Create(socket, 115200, GTI.SerialParity.None, GTI.SerialStopBits.One, 8, useFlowControl ? GTI.HardwareFlowControl.Required : GTI.HardwareFlowControl.NotRequired, this);
			this.SerialPort.DataReceived += this.OnSerialDataReceived;
			this.SerialPort.Open();

			this.Reset();

			var end = DateTime.Now.AddMilliseconds(this.timeout);
			while (!this.Ready) {
				if (end <= DateTime.Now && this.timeout > 0)
					throw new TimeoutException();

				Thread.Sleep(1);
			}

			this.EnterCommandMode();

			this.ExecuteCommand("set sys printlvl 0");
			this.ExecuteCommand("set comm remote 0");

			if (useFlowControl)
				this.ExecuteCommand("set uart flow 1");

			this.ExecuteCommand("set uart tx 1");

			this.ExitCommandMode();
		}

		/// <summary>Reboots the module.</summary>
		public void Reboot() {
			this.SerialPort.DiscardInBuffer();
			this.SerialPort.DiscardOutBuffer();

			this.Ready = false;

			this.Reset();

			var end = DateTime.Now.AddMilliseconds(this.timeout);
			while (!this.Ready) {
				if (end <= DateTime.Now && this.timeout > 0)
					throw new TimeoutException();

				Thread.Sleep(1);
			}
		}

		/// <summary>Updates the firmware.</summary>
		/// <returns>Whether it was successful or not.</returns>
		public bool UpdateFirmware() {
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

		/// <summary>Creates an access point with the given SSID.</summary>
		/// <param name="ssid">The SSID to use.</param>
		public void CreateAccessPoint(string ssid) {
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

		/// <summary>Enables DHCP mode.</summary>
		/// <param name="gateway">The gateway address.</param>
		/// <param name="subnetMask">The subnet mask.</param>
		/// <param name="dnsAddress">The DNS address.</param>
		public void EnableDhcp(string gateway, string subnetMask, string dnsAddress) {
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

		/// <summary>Enables static ip mode.</summary>
		/// <param name="ipAddress">The ip address</param>
		/// <param name="gateway">The gateway address.</param>
		/// <param name="subnetMask">The subnet mask.</param>
		/// <param name="dnsAddress">The DNS address.</param>
		public void EnableStaticIP(string ipAddress, string gateway, string subnetMask, string dnsAddress) {
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

		/// <summary>Attempt to join a wireless network with given parameters.</summary>
		/// <param name="ssid">The ssid.</param>
		/// <param name="passphrase">The passphrase.</param>
		public void JoinWirelessNetwork(string ssid, string passphrase) {
			this.JoinWirelessNetwork(ssid, passphrase, 0, WirelessEncryptionMode.Open);
		}

		/// <summary>Attempt to join a wireless network with given parameters.</summary>
		/// <param name="ssid">The ssid.</param>
		/// <param name="passphrase">The passphrase.</param>
		/// <param name="channel">The channel.</param>
		/// <param name="authenticationMode">The authentication mode.</param>
		public void JoinWirelessNetwork(string ssid, string passphrase, int channel, WirelessEncryptionMode authenticationMode) {
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

		/// <summary>Sets the socket protocol.</summary>
		/// <param name="protocol">The desired protocol.</param>
		public void SetProtocol(SocketProtocol protocol) {
			this.EnterCommandMode();
			this.ExecuteCommand("set ip protocol " + protocol.ToString());
			this.ExitCommandMode();
		}

		/// <summary>Set the port the WiFly module should listen for connections on.</summary>
		/// <param name="port">The port to listen on.</param>
		public void SetListenPort(int port) {
			if (port < 0) throw new ArgumentOutOfRangeException("port", "port must be non-negative.");

			this.EnterCommandMode();
			this.ExecuteCommand("set ip local " + port.ToString());
			this.ExitCommandMode();
		}

		/// <summary>Sets the device to listen port on port 80 and allow HTTP request parsing.</summary>
		public void EnableHttpServer() {
			this.EnterCommandMode();
			this.ExecuteCommand("set ip local 80");
			this.ExitCommandMode();

			this.httpEnabled = true;
		}

		/// <summary>Sends data to the currently connected client.</summary>
		/// <param name="data">The data to send.</param>
		public void Send(byte[] data) {
			this.Send(data, 0, data.Length);
		}

		/// <summary>Sends data to the currently connected client.</summary>
		/// <param name="data">The data to send.</param>
		/// <param name="offset">The offset into the buffer to send at.</param>
		/// <param name="count">The number of bytes to send.</param>
		public void Send(byte[] data, int offset, int count) {
			if (data == null) throw new ArgumentNullException("data");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must be at least zero.");
			if (count <= 0) throw new ArgumentOutOfRangeException("count", "count must be positive.");
			if (offset + count > data.Length) throw new ArgumentOutOfRangeException("buffer", "buffer.Length must be at least offset + count.");

			while (this.SerialPort.BytesToWrite > 0)
				Thread.Sleep(10);

			this.SerialPort.Write(data, offset, count);
		}

		/// <summary>Executes the command.</summary>
		/// <param name="command">The command to execute.</param>
		public void ExecuteCommand(string command) {
			if (command == null) throw new ArgumentNullException("command");

			if (command.IndexOf("\r") < 0)
				command += "\r";

			this.WriteCommand(command);

			Thread.Sleep(10);

			if (!this.commandResponseComplete.WaitOne(this.timeout, true))
				throw new TimeoutException();
		}

		/// <summary>Enters command mode.</summary>
		public void EnterCommandMode() {
			Thread.Sleep(100);

			this.WriteCommand("$$$");

			var end = DateTime.Now.AddMilliseconds(this.timeout);
			while (!this.inCommandMode) {
				if (end <= DateTime.Now && this.timeout > 0)
					throw new TimeoutException();

				Thread.Sleep(1);
			}
		}

		/// <summary>Exits command mode.</summary>
		public void ExitCommandMode() {
			this.WriteCommand("exit\r");

			var end = DateTime.Now.AddMilliseconds(this.timeout);
			while (this.inCommandMode) {
				if (end <= DateTime.Now && this.timeout > 0)
					throw new TimeoutException();

				Thread.Sleep(1);
			}
		}

		/// <summary>Writes to the device.</summary>
		/// <param name="command">The command to write.</param>
		/// <returns>Whether or not it was successful</returns>
		public void WriteCommand(string command) {
			if (command == null) throw new ArgumentNullException("command");

			this.WriteCommand(Encoder.UTF8.GetBytes(command));
		}

		/// <summary>Writes to the device.</summary>
		/// <param name="command">The command to write.</param>
		/// <returns>Whether or not it was successful</returns>
		public void WriteCommand(byte[] command) {
			if (command == null) throw new ArgumentNullException("command");

			this.commandModeResponse = "";

			this.SerialPort.Write(command);
		}

		private void GetCommand(string command) {
			this.EnterCommandMode();

			this.ExecuteCommand("get " + command);
			this.ExecuteCommand("get " + command);

			if (!this.commandResponseComplete.WaitOne(this.timeout, true))
				throw new TimeoutException();

			this.ExitCommandMode();
		}

		private void OnSerialDataReceived(GTI.Serial sender) {
			var available = this.SerialPort.BytesToRead;
			var data = new byte[available];
			var size = this.SerialPort.Read(data, 0, available);

			string line = new string(Encoder.UTF8.GetChars(data, 0, size));

			if (line == null)
				return;

			this.serialBuffer += line;

			this.Log(line);

			if (this.serialBuffer.IndexOf("*OPEN*") >= 0) {
				this.serialBuffer = this.Replace("*OPEN*", "", this.serialBuffer);
				this.OnConnectionEstablished(this, null);
			}

			if (this.serialBuffer.IndexOf("*CLOS*") >= 0) {
				this.serialBuffer = this.Replace("*CLOS*", "", this.serialBuffer);
				this.OnConnectionClosed(this, null);
			}

			if (this.serialBuffer.IndexOf("\r\r") >= 0) {
				this.serialBuffer = this.Replace("\r\r", "\r", this.serialBuffer);
			}

			if (this.serialBuffer.IndexOf("\r\n") >= 0) {
				int index = -1;

				while ((index = this.serialBuffer.IndexOf("\r\n")) >= 0) {
					string newLine = this.serialBuffer.Substring(0, index) + "\r\n";

					this.serialBuffer = this.serialBuffer.Substring(index + 2);

					this.OnSerialLineReceived(newLine);
				}
			}

			if (this.currentRequest != null && this.awaitingPostData && this.serialBuffer.Length >= this.currentRequest.PostLength) {
				this.OnSerialLineReceived(this.serialBuffer);
			}

			this.OnDataReceived(this, line);
		}

		private void OnSerialLineReceived(string line) {
			if (line.IndexOf("*READY*") >= 0)
				this.Ready = true;

			if (line.Length >= 3 && !this.inCommandMode && line.Substring(0, 3) == "CMD")
				this.inCommandMode = true;

			if (this.inCommandMode) {
				this.commandModeResponse += line;

				if (this.commandModeResponse.IndexOf("EXIT") >= 0) {
					this.inCommandMode = false;

					return;
				}

				if (this.commandModeResponse.IndexOf("UPDATE OK") >= 0) {
					this.updateComplete.Set();
					this.commandResponseComplete.Set();
				}

				if (this.commandModeResponse.IndexOf("Set Factory Defaults") >= 0)
					this.commandResponseComplete.Set();

				if (this.commandModeResponse.IndexOf("AOK") >= 0)
					this.commandResponseComplete.Set();

				if (this.commandModeResponse.IndexOf("ERR") >= 0) {
					this.commandResponseComplete.Set();

					throw new ErrorReceivedException(line);
				}

				if (!this.useDhcp && this.commandModeResponse.IndexOf("Associated!") >= 0)
					this.commandResponseComplete.Set();

				if (line.Length >= 3) {
					if (line.Substring(0, 3) == "IP=") {
						this.LocalIP = line.Substring(3).Split(':')[0];

						this.commandResponseComplete.Set();
					}

					//Check to see if the appropriate firmware is loaded
					if (line.Substring(0, 5) == "File=") {
						this.LocalIP = line.Substring(5).Split(':')[0];

						this.commandResponseComplete.Set();
					}
				}
			}
			else {
				if (this.httpEnabled)
					this.ParseHttp(line);

				this.OnLineReceived(this, line);
			}
		}

		private void ParseHttp(string buffer) {
			if (buffer.Length <= 0)
				return;

			if (buffer.IndexOf("GET") < 0 || buffer.IndexOf("POST") < 0)
				return;

			buffer = this.Replace("*OPEN*", "", buffer);
			buffer = this.Replace("*CLOS*", "", buffer);

			this.httpBuffer += buffer;

			if (this.awaitingPostData) {
				int length = this.ParseInt(this.currentRequest.HeaderData["Content-length"]);

				if (this.httpBuffer.Length >= length) {
					this.awaitingPostData = false;
					this.currentRequest.PostData = httpBuffer;

					this.OnHttpRequestReceived(this, new HttpSession(this.currentRequest, this.SerialPort));
				}

				return;
			}

			int index = this.httpBuffer.IndexOf("\r\n\r\n");
			if (index >= 0) {
				this.ParseHttpHeader();

				if (this.currentRequest.Type == HttpRequest.RequestType.Get) {
					this.httpBuffer = "";

					this.OnHttpRequestReceived(this, new HttpSession(this.currentRequest, this.SerialPort));
				}
				else if (currentRequest.Type == HttpRequest.RequestType.Post) {
					this.awaitingPostData = true;
					this.httpBuffer = this.httpBuffer.Substring(index + 4);
				}
			}
		}

		private void ParseHttpHeader() {
			if (this.httpBuffer.IndexOf("GET") >= 0)
				this.currentRequest.Type = HttpRequest.RequestType.Get;
			else if (this.httpBuffer.IndexOf("POST") >= 0)
				this.currentRequest.Type = HttpRequest.RequestType.Post;

			int startIndex = httpBuffer.IndexOf("/");
			int endIndex = startIndex >= 0 ? this.httpBuffer.IndexOf(" ", startIndex) : -1;

			if (startIndex >= 0 && endIndex > startIndex) {
				int i = this.httpBuffer.IndexOf("?");

				if (i > 0) {
					this.currentRequest.Url = this.httpBuffer.Substring(startIndex, i - startIndex);

					var queryParameters = this.httpBuffer.Substring(i + 1, endIndex - i - 1).Split('=', '&');
					for (int j = 0; j < queryParameters.Length; j += 2)
						this.currentRequest.QueryData[this.UrlDecode(queryParameters[j])] = this.UrlDecode(queryParameters[j + 1]);
				}
				else {
					this.currentRequest.Url = this.httpBuffer.Substring(startIndex, endIndex - startIndex);
				}

				this.httpBuffer = this.httpBuffer.Substring(this.httpBuffer.IndexOf("\r\n") + 2);
			}

			int index = -1;
			string lineBuffer = "";
			bool needsLength = true;

			while ((index = this.httpBuffer.IndexOf("\r\n")) >= 0) {
				if (this.currentRequest.Type == HttpRequest.RequestType.Post && needsLength) {
					lineBuffer = this.httpBuffer.Substring(0, index);

					if (lineBuffer.IndexOf("Content-Length:") == 0) {
						this.currentRequest.PostLength = this.ParseInt(lineBuffer.Substring(16));

						needsLength = false;
					}

					if (lineBuffer.IndexOf(": ") > 0)
						this.currentRequest.HeaderData[lineBuffer.Substring(0, lineBuffer.IndexOf(": "))] = lineBuffer.Substring((lineBuffer.IndexOf(": ") + 2));
				}
				else {
					lineBuffer = this.httpBuffer.Substring(0, index);

					if (lineBuffer.IndexOf(": ") > 0)
						this.currentRequest.HeaderData[lineBuffer.Substring(0, lineBuffer.IndexOf(": "))] = lineBuffer.Substring((lineBuffer.IndexOf(": ") + 2));

					lineBuffer = "";
				}

				this.httpBuffer = this.httpBuffer.Substring(index + 2);
			}
		}

		private void Log(string message) {
			if (!this.DebugPrintEnabled)
				return;

			if (this.DebugPort == null)
				Debug.Print(message);
			else
				this.DebugPort.Write(Encoder.UTF8.GetBytes(message));
		}

		private void Reset() {
			this.reset.Write(false);
			Thread.Sleep(100);

			this.reset.Write(true);
			Thread.Sleep(250);
		}

		private string Replace(string oldValue, string newValue, string source) {
			return new StringBuilder(source).Replace(oldValue, newValue).ToString();
		}

		private int ParseInt(string source) {
			try {
				return int.Parse(source);
			}
			catch {
				return 0;
			}
		}

		private string UrlDecode(string buffer) {
			if (buffer.IndexOf("%") < 0)
				return buffer;

			StringBuilder builder = new StringBuilder();
			int index = -1;

			while ((index = buffer.IndexOf("%")) >= 0) {
				builder.Append(buffer.Substring(0, index));

				int value = 0;

				if (buffer[index + 1] >= 'A')
					value = (buffer[index + 1] - 48 - 7) << 4;
				else
					value = (buffer[index + 1] - 48) << 4;

				if (buffer[index + 2] >= 'A')
					value |= (byte)(buffer[index + 2] - 48 - 7);
				else
					value |= (byte)(buffer[index + 2] - 48);

				builder.Append((char)value);

				buffer = buffer.Substring(index + 3);
			}

			return builder.ToString();
		}

		private void OnHttpRequestReceived(WiFiRN171 sender, HttpSession e) {
			if (Program.CheckAndInvoke(this.HttpRequestReceived, this.onHttpRequestReceived, sender, e))
				this.HttpRequestReceived(sender, e);
		}

		private void OnDataReceived(WiFiRN171 sender, string e) {
			if (Program.CheckAndInvoke(this.DataReceived, this.onDataReceived, sender, e))
				this.DataReceived(sender, e);
		}

		private void OnLineReceived(WiFiRN171 sender, string e) {
			if (Program.CheckAndInvoke(this.LineReceived, this.onLineReceived, sender, e))
				this.LineReceived(sender, e);
		}

		private void OnConnectionEstablished(WiFiRN171 sender, EventArgs e) {
			if (Program.CheckAndInvoke(this.ConnectionEstablished, this.onConnectionEstablished, sender, e))
				this.ConnectionEstablished(sender, e);
		}

		private void OnConnectionClosed(WiFiRN171 sender, EventArgs e) {
			if (Program.CheckAndInvoke(this.ConnectionClosed, this.onConnectionClosed, sender, e))
				this.ConnectionClosed(sender, e);
		}
		/// <summary>An exception thrown when a timeout occurs.</summary>
		[Serializable]
		public class TimeoutException : global::System.Exception {

			internal TimeoutException()
				: base() {
			}

			internal TimeoutException(string message)
				: base(message) {
			}

			internal TimeoutException(string message, Exception innerException)
				: base(message, innerException) {
			}
		}

		/// <summary>An exception thrown when an errror is received.</summary>
		[Serializable]
		public class ErrorReceivedException : global::System.Exception {

			internal ErrorReceivedException()
				: base() {
			}

			internal ErrorReceivedException(string message)
				: base(message) {
			}

			internal ErrorReceivedException(string message, Exception innerException)
				: base(message, innerException) {
			}
		}
		/// <summary>A Hashtable that works with strings.</summary>
		public class StringHashtable : Hashtable {

			/// <summary>Indexer for the value with the given key.</summary>
			/// <param name="i">The key.</param>
			/// <returns>The value.</returns>
			public string this[string i] {
				get {
					return (string)base[i];
				}

				set {
					base[i] = value;
				}
			}

			/// <summary>Constructs a new instance.</summary>
			public StringHashtable()
				: base() {
			}
		}

		/// <summary>Represents an HTTP request.</summary>
		public class HttpRequest {

			/// <summary>The request type.</summary>
			public RequestType Type { get; set; }

			/// <summary>The header data.</summary>
			public StringHashtable HeaderData { get; private set; }

			/// <summary>The query data.</summary>
			public StringHashtable QueryData { get; private set; }

			/// <summary>The URL.</summary>
			public string Url { get; set; }

			/// <summary>The posted data.</summary>
			public string PostData { get; set; }

			/// <summary>The length of the posted data.</summary>
			public int PostLength { get; set; }

			/// <summary>Whether or not there was posted data.</summary>
			public bool HasPostData {
				get {
					return (this.PostData.Length > 0);
				}
			}

			/// <summary>Represents an HTTP request type.</summary>
			public enum RequestType {

				/// <summary>A GET request.</summary>
				Get,

				/// <summary>A POST request.</summary>
				Post
			}

			/// <summary>Constructs a new instance.</summary>
			public HttpRequest() {
				this.HeaderData = new StringHashtable();
				this.QueryData = new StringHashtable();
				this.Url = string.Empty;
				this.PostData = string.Empty;
				this.PostLength = 0;
			}
		}

		/// <summary>An HTTP response.</summary>
		public class HttpResponse {
			private ResponseStatus status;
			private GTI.Serial stream;

			/// <summary>The header data of the response.</summary>
			public StringHashtable HeaderData { get; private set; }

			/// <summary>The status code of the reponse.</summary>
			public ResponseStatus StatusCode {
				get {
					return this.status;
				}

				set {
					status = value;
					this.HeaderData["Status"] = "HTTP/1.1 " + this.GetResponseText(value);
				}
			}

			/// <summary>The possible response statuses.</summary>
			public enum ResponseStatus {

				/// <summary>The Continue status</summary>
				Continue = 100,

				/// <summary>The SwitchingProtocols status</summary>
				SwitchingProtocols = 101,

				/// <summary>The OK status</summary>
				Ok = 200,

				/// <summary>The Created status</summary>
				Created = 201,

				/// <summary>The Accepted status</summary>
				Accepted = 202,

				/// <summary>The NonAuthoritativeInformation status</summary>
				NonAuthoritativeInformation = 203,

				/// <summary>The NoContent status</summary>
				NoContent = 204,

				/// <summary>The ResetContent status</summary>
				ResetContent = 205,

				/// <summary>The BadRequest status</summary>
				BadRequest = 400,

				/// <summary>The Unauthorized status</summary>
				Unauthorized = 401,

				/// <summary>The Forbidden status</summary>
				Forbidden = 403,

				/// <summary>The NotFound status</summary>
				NotFound = 404,

				/// <summary>The MethodNotAllowed status</summary>
				MethodNotAllowed = 405,

				/// <summary>The NotAcceptable status</summary>
				NotAcceptable = 406,

				/// <summary>The ProxyAuthenticationRequired status</summary>
				ProxyAuthenticationRequired = 407,

				/// <summary>The RequestTimeout status</summary>
				RequestTimeout = 408,

				/// <summary>The Conflict status</summary>
				Conflict = 409,

				/// <summary>The Gone status</summary>
				Gone = 410,

				/// <summary>The LengthRequired status</summary>
				LengthRequired = 411,

				/// <summary>The PreconditionFailed status</summary>
				PreconditionFailed = 412,

				/// <summary>The RequestEntityTooLarge status</summary>
				RequestEntityTooLarge = 413,

				/// <summary>The RequestUriTooLong status</summary>
				RequestUriTooLong = 414,

				/// <summary>The UnsupportedMediaType status</summary>
				UnsupportedMediaType = 415,

				/// <summary>The RequestedRangeNotSatisfiable status</summary>
				RequestedRangeNotSatisfiable = 416,

				/// <summary>The ExpectationFailed status</summary>
				ExpectationFailed = 417,

				/// <summary>The InternalServerError status</summary>
				InternalServerError = 500,

				/// <summary>The NotImplemented status</summary>
				NotImplemented = 501,

				/// <summary>The BadGateway status</summary>
				BadGateway = 502,

				/// <summary>The ServiceUnavailable status</summary>
				ServiceUnavailable = 503,

				/// <summary>The GatewayTimeout status</summary>
				GatewayTimeout = 504,

				/// <summary>The HttpVersionNotSupported status</summary>
				HttpVersionNotSupported = 505
			}

			/// <summary>Constructs a new instance.</summary>
			/// <param name="stream">The stream to use.</param>
			public HttpResponse(GTI.Serial stream) {
				if (stream == null) throw new ArgumentNullException("stream");

				this.stream = stream;

				this.HeaderData = new StringHashtable();
			}

			/// <summary>Sends a response.</summary>
			/// <param name="document">The body of the response.</param>
			public void Send(byte[] document) {
				this.Send(document, true);
			}

			/// <summary>Sends a response.</summary>
			/// <param name="document">The body of the response.</param>
			/// <param name="sendHeader">Whether or not to send the header as part of the response.</param>
			public void Send(byte[] document, bool sendHeader) {
				if (document == null) throw new ArgumentNullException("document");

				this.Send(document, 0, document.Length, sendHeader);
			}

			/// <summary>Sends a response.</summary>
			/// <param name="document">The body of the response.</param>
			/// <param name="offset">The offset into the document.</param>
			/// <param name="count">The amount to transfer starting at offset from the document.</param>
			/// <param name="sendHeader">Whether or not to send the header as part of the response.</param>
			public void Send(byte[] document, int offset, int count, bool sendHeader) {
				if (document == null) throw new ArgumentNullException("document");
				if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must be non-negative.");
				if (count < 0) throw new ArgumentOutOfRangeException("count", "count must be non-negative.");
				if (document.Length < offset + count) throw new ArgumentOutOfRangeException("document", "document.Length must be at least offset + count.");

				if (sendHeader)
					this.stream.Write(Encoder.UTF8.GetBytes(this.HeaderToString()));

				this.stream.Write(document, offset, count);
			}

			private string HeaderToString() {
				string header = string.Empty;

				if (this.HeaderData.Contains("Request"))
					header += this.HeaderData["Request"] + "\r\n";

				if (this.HeaderData.Contains("Status"))
					header += this.HeaderData["Status"] + "\r\n";

				foreach (DictionaryEntry entry in this.HeaderData)
					if ((string)entry.Key != "Request" && (string)entry.Key != "Status")
						header += (string)entry.Key + ": " + (string)entry.Value + "\r\n";

				return header + "\r\n";
			}

			private string GetResponseText(ResponseStatus status) {
				switch (status) {
					case ResponseStatus.Accepted: return "202 Accepted";
					case ResponseStatus.BadGateway: return "502 Bad Gateway";
					case ResponseStatus.BadRequest: return "400 Bad Gateway";
					case ResponseStatus.Conflict: return "409 Conflict";
					case ResponseStatus.Continue: return "100 Continue";
					case ResponseStatus.Created: return "201 Created";
					case ResponseStatus.ExpectationFailed: return "417 Expectation Fail";
					case ResponseStatus.Forbidden: return "403 Forbidden";
					case ResponseStatus.GatewayTimeout: return "504 Gateway Timeout";
					case ResponseStatus.Gone: return "410 Gone";
					case ResponseStatus.HttpVersionNotSupported: return "505 HTTP Version Not Supported";
					case ResponseStatus.InternalServerError: return "500 Internal Server Error";
					case ResponseStatus.LengthRequired: return "411 Length Required";
					case ResponseStatus.MethodNotAllowed: return "405 Method Not Allowed";
					case ResponseStatus.NoContent: return "204 No Content";
					case ResponseStatus.NonAuthoritativeInformation: return "203 Non-Authoritative Information";
					case ResponseStatus.NotAcceptable: return "406 Not Acceptable";
					case ResponseStatus.NotFound: return "404 Not Found";
					case ResponseStatus.NotImplemented: return "501 Not Implemented";
					case ResponseStatus.Ok: return "200 OK";
					case ResponseStatus.PreconditionFailed: return "412 Precondition Failed";
					case ResponseStatus.ProxyAuthenticationRequired: return "407 Proxy Authentication Required";
					case ResponseStatus.RequestedRangeNotSatisfiable: return "416 Requested Range Not Satisfiable";
					case ResponseStatus.RequestEntityTooLarge: return "413 Request Entity Too Large";
					case ResponseStatus.RequestTimeout: return "408 Request Timeout";
					case ResponseStatus.RequestUriTooLong: return "413 Request Entity Too Large";
					case ResponseStatus.ResetContent: return "205 Reset Content";
					case ResponseStatus.ServiceUnavailable: return "503 Service Unavailable";
					case ResponseStatus.SwitchingProtocols: return "101 Switching Protocols";
					case ResponseStatus.Unauthorized: return "401 Unauthorized";
					case ResponseStatus.UnsupportedMediaType: return "415 Unsupported Media Type";
					default: return string.Empty;
				}
			}
		}

		/// <summary>Represents an HTTP session.</summary>
		public class HttpSession {
			private HttpRequest request;
			private HttpResponse response;

			/// <summary>The response.</summary>
			public HttpResponse Response {
				get {
					return this.response;
				}
			}

			/// <summary>The request.</summary>
			public HttpRequest Request {
				get {
					return this.request;
				}
			}

			/// <summary>Constructs a new instance.</summary>
			/// <param name="request">The http request the session will represent.</param>
			/// <param name="stream">The stream of serial data.</param>
			public HttpSession(HttpRequest request, GTI.Serial stream) {
				if (request == null) throw new ArgumentNullException("request");
				if (stream == null) throw new ArgumentNullException("stream");

				this.request = request;
				this.response = new HttpResponse(stream);
			}
		}
	}
}