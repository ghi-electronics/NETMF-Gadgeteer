using GHI.Networking;
using System;
using System.Collections;
using System.IO.Ports;
using System.Text;
using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A CellularRadio module for Microsoft .NET Gadgeteer</summary>
	public class CellularRadio : GTM.Module.NetworkModule {
		private PPPSerialModem networkInterface;
		private SerialPort serial;
		private GTI.DigitalOutput power;
		private Queue newMessages;
		private Queue requestedMessages;
		private Thread worker;
		private bool powerOn;
		private byte[] buffer;
		private string responseBuffer;
		private bool running;
		private AutoResetEvent pppEvent;
		private string pppExpectedResponse;
		private AutoResetEvent atExpectedEvent;
		private string atExpectedResponse;

		private PinStateRequestedHandler onPinStateRequested;
		private GsmNetworkRegistrationChangedHandler onGsmNetworkRegistrationChanged;
		private GprsNetworkRegistrationChangedHandler onGprsNetworkRegistrationChanged;
		private SmsReceivedHandler onSmsReceived;
		private IncomingCallHandler onIncomingCall;
		private PhoneActivityRequestedHandler onPhoneActivityRequested;
		private ContactOpenRequested onContactRequested;
		private ClockRequestedHandler onClockRequested;
		private ImeiRequestedHandler onImeiRequested;
		private SignalStrengthRequestedHandler onSignalStrengthRequested;
		private OperatorRequestedHandler onOperatorRequested;
		private SmsListRequestedHandler onSmsListReceived;
		private CallEndedHandler onCallEnded;
		private CallConnectedHandler onCallConnected;
		private GprsAttachedHandler onGprsAttached;
		private LineReceivedHandler onLineReceived;
		private LineSentHandler onLineSent;

		/// <summary>Represents the delegate used for the PinStateRequested event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="pinState">Current state of the PIN</param>
		public delegate void PinStateRequestedHandler(CellularRadio sender, PinState pinState);

		/// <summary>Represents the delegate used for the GsmNetworkRegistrationChanged event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="networkState">Current state of the GSM network registration</param>
		public delegate void GsmNetworkRegistrationChangedHandler(CellularRadio sender, NetworkRegistrationState networkState);

		/// <summary>Represents the delegate used for the GprsNetworkRegistrationChanged event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="networkState">Current state of the GPRS network registration</param>
		public delegate void GprsNetworkRegistrationChangedHandler(CellularRadio sender, NetworkRegistrationState networkState);

		/// <summary>Represents the delegate used for the SmsReceived event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="message">Object containing the SMS</param>
		public delegate void SmsReceivedHandler(CellularRadio sender, Sms message);

		/// <summary>Represents the delegate used for the IncomingCall event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="caller">Number of the caller</param>
		public delegate void IncomingCallHandler(CellularRadio sender, string caller);

		/// <summary>Represents the delegate used for the SmsRequested event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="message">SMS that was requested</param>
		public delegate void SmsRequestedHandler(CellularRadio sender, Sms message);

		/// <summary>Represents the delegate used for the PhoneActivityRequested event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="activity">Current activity in which the phone is engaged</param>
		public delegate void PhoneActivityRequestedHandler(CellularRadio sender, PhoneActivity activity);

		/// <summary>Represents the delegate used for the ContactRequested event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="contact">Contact object with the requested phonebook entry</param>
		public delegate void ContactOpenRequested(CellularRadio sender, Contact contact);

		/// <summary>Represents the delegate used for the ClockRequested event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="clock">Module's date and time</param>
		public delegate void ClockRequestedHandler(CellularRadio sender, DateTime clock);

		/// <summary>Represents the delegate used for the ImeiRequested event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="imei">Module's International Mobile Equipment Identification number</param>
		public delegate void ImeiRequestedHandler(CellularRadio sender, string imei);

		/// <summary>Represents the delegate used for the SignalStrengthRequested event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="signalStrength">Strength of the signal</param>
		public delegate void SignalStrengthRequestedHandler(CellularRadio sender, SignalStrength signalStrength);

		/// <summary>Represents the delegate used for the OperatorRequested event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="operatorName">Name of the operator to which the module is connected. It is null if the module is not connected to any operator.</param>
		public delegate void OperatorRequestedHandler(CellularRadio sender, string operatorName);

		/// <summary>Represents the delegate used for the SmsListRequested event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="smsList">Strength of the signal</param>
		public delegate void SmsListRequestedHandler(CellularRadio sender, Sms[] smsList);

		/// <summary>Represents the delegate used for the CallEnded event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="reason">The reason the call has ended</param>
		public delegate void CallEndedHandler(CellularRadio sender, CallEndReason reason);

		/// <summary>Represents the delegate used for the CallConnected event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="number">Number to which the module is connected</param>
		public delegate void CallConnectedHandler(CellularRadio sender, string number);

		/// <summary>Represents the delegate used for the GprsAttached event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="ipAddress">Number to which the module is connected</param>
		public delegate void GprsAttachedHandler(CellularRadio sender, string ipAddress);

		/// <summary>Represents the delegate used for the LineReceived event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="line">The line that was received.</param>
		public delegate void LineReceivedHandler(CellularRadio sender, string line);

		/// <summary>Represents the delegate used for the LineSent event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="line">The line that was sent.</param>
		public delegate void LineSentHandler(CellularRadio sender, string line);

		/// <summary>Raised when the pin state is requested.</summary>
		public event PinStateRequestedHandler PinStateRequested;

		/// <summary>Raised when the module emits a network registration message.</summary>
		public event GsmNetworkRegistrationChangedHandler GsmNetworkRegistrationChanged;

		/// <summary>Raised when the module emits a network registration message.</summary>
		public event GprsNetworkRegistrationChangedHandler GprsNetworkRegistrationChanged;

		/// <summary>Raised when the module receives a new SMS.</summary>
		public event SmsReceivedHandler SmsReceived;

		/// <summary>Raised when the module detects an incoming call.</summary>
		public event IncomingCallHandler IncomingCall;

		/// <summary>Raised when the module receives a phone activity message.</summary>
		public event PhoneActivityRequestedHandler PhoneActivityRequested;

		/// <summary>Raised when a contact is requested.</summary>
		public event ContactOpenRequested ContactRequested;

		/// <summary>Raised when the clock is requested.</summary>
		public event ClockRequestedHandler ClockRequested;

		/// <summary>Raised when the IMEI is requested.</summary>
		public event ImeiRequestedHandler ImeiRequested;

		/// <summary>Raised when the signal strength is requested.</summary>
		public event SignalStrengthRequestedHandler SignalStrengthRequested;

		/// <summary>Raised when the operator is requested.</summary>
		public event OperatorRequestedHandler OperatorRequested;

		/// <summary>Raised when the sms list is requested.</summary>
		public event SmsListRequestedHandler SmsListReceived;

		/// <summary>Raised when the call ends.</summary>
		public event CallEndedHandler CallEnded;

		/// <summary>Raised when the call is connected.</summary>
		public event CallConnectedHandler CallConnected;

		/// <summary>Raised when the GPRS is attached.</summary>
		public event GprsAttachedHandler GprsAttached;

		/// <summary>Raised when a line is received. Useful for debugging.</summary>
		public event LineReceivedHandler LineReceived;

		/// <summary>Raised when a line is sent. Useful for debugging.</summary>
		public event LineSentHandler LineSent;

		/// <summary>The underlying network interface.</summary>
		public PPPSerialModem NetworkInterface {
			get {
				return this.networkInterface;
			}
		}

		/// <summary>Whether or not the network is connected. Make sure to also check the NetworkUp property to verify network state.</summary>
		public override bool IsNetworkConnected {
			get {
				return this.networkInterface.LinkConnected;
			}
		}

		/// <summary>Possible states of network registration.</summary>
		public enum NetworkRegistrationState {
			/// <summary>The module couldn't find a network.</summary>
			NotSearching,

			/// <summary>The module is registered to a network.</summary>
			Registered,

			/// <summary>The module is searching for a network.</summary>
			Searching,

			/// <summary>The module tried to register to a network, but it was denied.</summary>
			RegistrationDenied,

			/// <summary>Unknown failure.</summary>
			Unknown,

			/// <summary>The module is roaming.</summary>
			Roaming,

			/// <summary>There was an error.</summary>
			Error
		}

		/// <summary>Possible states of the SIM card.</summary>
		public enum PinState {
			/// <summary>The SIM is unlocked and ready to be used.</summary>
			Ready,

			/// <summary>The SIM is locked waiting for the PIN.</summary>
			Pin,

			/// <summary>The SIM is locked waiting for the PUK.</summary>
			Puk,

			/// <summary>The SIM is waiting for phone to SIM card (antitheft).</summary>
			PhPin,

			/// <summary>The SIM is waiting for phone to SIM PUK (antitheft).</summary>
			PhPuk,

			/// <summary>The SIM is waiting for second PIN.</summary>
			Pin2,

			/// <summary>The SIM is waiting for second PUK.</summary>
			Puk2,

			/// <summary>The SIM is not present.</summary>
			NotPresent
		}

		/// <summary>Possible states for a text message.</summary>
		public enum SmsState {
			/// <summary>Messages that were received and read</summary>
			ReceivedUnread,

			/// <summary>Messages that were received but not yet read</summary>
			ReceivedRead,

			/// <summary>Messages that were created but not yet sent</summary>
			StoredUnsent,

			/// <summary>Messages that were created and sent</summary>
			StoredSent,

			/// <summary>All messages</summary>
			All
		}

		/// <summary>Possible states of a call.</summary>
		public enum PhoneActivity {
			/// <summary>The phone is not calling or being called.</summary>
			Ready,

			/// <summary>The phone is ringing.</summary>
			Ringing,

			/// <summary>There is an active voice all.</summary>
			CallInProgress,

			/// <summary>The module is in an unknown state.</summary>
			Unknown,

			/// <summary>The communication line with the module is busy.</summary>
			CommLineBusy
		}

		/// <summary>Possible values of the strength of a signal.</summary>
		public enum SignalStrength {
			/// <summary>- 115dBm or less.</summary>
			VeryWeak,

			/// <summary>- 111dBm.</summary>
			Weak,

			/// <summary>- 110 to -54dBm.</summary>
			Strong,

			/// <summary>- 52dBm or greater.</summary>
			VeryStrong,

			/// <summary>Not known or undetectable.</summary>
			Unknown,

			/// <summary>There was an error in the response from the module.</summary>
			Error
		}

		/// <summary>Possible reasons for a call to be ended.</summary>
		public enum CallEndReason {
			/// <summary>No dial tone was found.</summary>
			NoDialTone,

			/// <summary>No carrier was found.</summary>
			NoCarrier,

			/// <summary>The line is busy.</summary>
			Busy
		}

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public CellularRadio(int socketNumber) {
			this.newMessages = new Queue();
			this.requestedMessages = new Queue();

			this.powerOn = false;

			var socket = GT.Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('K', this);

			this.buffer = new byte[1024];
			this.responseBuffer = "";
			this.worker = null;
			this.running = false;
			this.pppEvent = new AutoResetEvent(false);
			this.atExpectedEvent = new AutoResetEvent(false);
			this.power = GTI.DigitalOutputFactory.Create(socket, GT.Socket.Pin.Three, true, this);
			this.serial = new SerialPort(socket.SerialPortName, 19200, Parity.None, 8, StopBits.One);
			this.serial.Handshake = Handshake.RequestToSend;
			this.serial.Open();
			this.serial.Write(Encoding.UTF8.GetBytes("AT\r"), 0, 3);

			this.atExpectedResponse = string.Empty;
			this.pppExpectedResponse = string.Empty;

			Thread.Sleep(1000);

			if (this.serial.BytesToRead != 0) {
				this.power.Write(false);
				Thread.Sleep(1000);
				this.power.Write(true);
				Thread.Sleep(2200);

				this.serial.DiscardInBuffer();
				this.serial.DiscardOutBuffer();
			}

			this.onPinStateRequested = this.OnPinStateRequested;
			this.onGsmNetworkRegistrationChanged = this.OnGsmNetworkRegistrationChanged;
			this.onGprsNetworkRegistrationChanged = this.OnGprsNetworkRegistrationChanged;
			this.onSmsReceived = this.OnSmsReceived;
			this.onIncomingCall = this.OnIncomingCall;
			this.onPhoneActivityRequested = this.OnPhoneActivityRequested;
			this.onContactRequested = this.OnContactRequested;
			this.onClockRequested = this.OnClockRequested;
			this.onImeiRequested = this.OnImeiRequested;
			this.onSignalStrengthRequested = this.OnSignalStrengthRequested;
			this.onOperatorRequested = this.OnOperatorRequested;
			this.onSmsListReceived = this.OnSmsListReceived;
			this.onCallEnded = this.OnCallEnded;
			this.onCallConnected = this.OnCallConnected;
			this.onGprsAttached = this.OnGprsAttached;
			this.onLineReceived = this.OnLineReceived;
			this.onLineSent = this.OnLineSent;
		}

		/// <summary>Opens the underlying network interface and assigns the NETMF networking stack.</summary>
		/// <param name="apn">The APN to use.</param>
		public void UseThisNetworkInterface(string apn) {
			this.UseThisNetworkInterface(apn, "", "", PPPSerialModem.AuthenticationType.None);
		}

		/// <summary>Opens the underlying network interface and assigns the NETMF networking stack.</summary>
		/// <param name="apn">The APN to use.</param>
		/// <param name="username">The username to connect with.</param>
		/// <param name="password">The password to connect with.</param>
		/// <param name="authenticationType">The authentication type.</param>
		/// <remarks>When using this overload, "CONNECT" is the assumed response when PPP is ready after the below commands are sent:
		/// AT+CGDCONT=1,"IP","[APN]"
		/// ATDT*99***1#
		/// </remarks>
		public void UseThisNetworkInterface(string apn, string username, string password, PPPSerialModem.AuthenticationType authenticationType) {
			if (apn == null) throw new ArgumentNullException("apn");

			this.UseThisNetworkInterface(username, password, authenticationType, "CONNECT", "AT+CGDCONT=1,\"IP\",\"" + apn + "\"", "ATDT*99***1#");
		}

		/// <summary>Opens the underlying network interface and assigns the NETMF networking stack.</summary>
		/// <param name="username">The username to connect with.</param>
		/// <param name="password">The password to connect with.</param>
		/// <param name="authenticationType">The authentication type.</param>
		/// <param name="initializationResponse">The response to look for that signals the PPP interface is ready to be initialized.</param>
		/// <param name="initializationCommands">The AT commands to send to the device to prepare it for PPP.</param>
		/// <remarks>If there are no expected response or initialization commands supplied, the PPP interface will attempt to immediately initialize.</remarks>
		public void UseThisNetworkInterface(string username, string password, PPPSerialModem.AuthenticationType authenticationType, string initializationResponse, params string[] initializationCommands) {
			if (username == null) throw new ArgumentNullException("username");
			if (password == null) throw new ArgumentNullException("password");
			if (initializationResponse == null) throw new ArgumentNullException("initializationResponse");
			if (initializationCommands == null) throw new ArgumentNullException("initializationCommands");

			if (this.networkInterface != null && this.networkInterface.Opened)
				return;

			this.pppEvent.Reset();
			this.pppExpectedResponse = initializationResponse;

			foreach (var command in initializationCommands)
				this.SendATCommand(command);

			this.pppEvent.WaitOne();

			this.running = false;
			this.worker.Join();
			this.worker = null;

			this.networkInterface = new PPPSerialModem(this.serial);
			this.networkInterface.Open();
			this.networkInterface.Connect(authenticationType, username, password);

			this.NetworkSettings = this.networkInterface.NetworkInterface;
		}

		/// <summary>Powers on the module with default initialization commands.</summary>
		public void PowerOn() {
			this.PowerOn(new string[] {
				"AT",
				"ATE0",
				"AT+CMGF=1",
				"AT+CSDH=0",
				"AT+CPBS=\"SM\"",
				"AT+CPMS=\"SM\"",
				"AT+COLP=1",
				"AT+CGREG=1",
				"AT+CREG=1",
			});
		}

		/// <summary>Power on the module.</summary>
		/// <param name="initializationCommands">The initialization commands to send after power up.</param>
		public void PowerOn(params string[] initializationCommands) {
			if (this.powerOn) throw new InvalidOperationException("The module is already powered on.");
			if (initializationCommands == null) throw new ArgumentNullException("initializationCommands");

			this.serial.DiscardInBuffer();
			this.serial.DiscardOutBuffer();

			this.power.Write(false);
			Thread.Sleep(1000);
			this.power.Write(true);
			Thread.Sleep(2200);

			this.powerOn = true;
			this.running = true;
			this.responseBuffer = "";
			this.worker = new Thread(this.DoWork);
			this.worker.Start();

			foreach (var command in initializationCommands)
				this.SendATCommand(command);
		}

		/// <summary>Powers off the module.</summary>
		public void PowerOff() {
			if (!this.powerOn) throw new InvalidOperationException("The module is already powered off.");

			this.power.Write(false);
			Thread.Sleep(1000);
			this.power.Write(true);
			Thread.Sleep(1700);

			this.powerOn = false;
			this.running = false;

			if (this.worker != null) {
				this.worker.Join();
				this.worker = null;
			}

			this.responseBuffer = "";
		}

		/// <summary>Resets module.</summary>
		public void Reset() {
			this.PowerOff();
			Thread.Sleep(500);
			this.PowerOn();
		}

		/// <summary>Sends an AT command to the module without waiting for a response. It automatically appends the carriage return.</summary>
		/// <param name="atCommand">The AT command. See SIM900_ATC_V1.00 for reference.</param>
		public void SendATCommand(string atCommand) {
			if (!this.powerOn) throw new InvalidOperationException("The module is off.");
			if (atCommand.IndexOf("AT") == -1) throw new ArgumentException("atCommand", "The command must begin with AT.");

			if (atCommand.IndexOf("\r") < 0)
				atCommand += "\r";

			this.WriteLine(atCommand);

			Thread.Sleep(100);
		}

		/// <summary>Sends an AT command to the module not returning until the expected response is received. It automatically appends the carriage return.</summary>
		/// <param name="atCommand">The AT command. See SIM900_ATC_V1.00 for reference.</param>
		/// <param name="expectedResponse">What the response to this command should start with.</param>
		public void SendATCommand(string atCommand, string expectedResponse) {
			this.SendATCommand(atCommand, expectedResponse, Timeout.Infinite);
		}

		/// <summary>Sends an AT command to the module not returning until the expected response is received or the timeout expires. It automatically appends the carriage return.</summary>
		/// <param name="atCommand">The AT command. See SIM900_ATC_V1.00 for reference.</param>
		/// <param name="expectedResponse">What the response to this command should start with.</param>
		/// <param name="timeout">How long to wait for the response.</param>
		/// <returns>True if the command was received, false if it timed out.</returns>
		public bool SendATCommand(string atCommand, string expectedResponse, int timeout) {
			if (!this.powerOn) throw new InvalidOperationException("The module is off.");
			if (atCommand.IndexOf("AT") == -1) throw new ArgumentException("atCommand", "The command must begin with AT.");
			if (timeout == 0) throw new ArgumentException("timeout", "timeout cannot be 0.");

			if (atCommand.IndexOf("\r") < 0)
				atCommand += "\r";

			this.atExpectedEvent.Reset();
			this.atExpectedResponse = expectedResponse;

			this.WriteLine(atCommand);

			if (!this.atExpectedEvent.WaitOne(timeout, false))
				return false;

			this.atExpectedResponse = string.Empty;

			return true;
		}

		/// <summary>Send an SMS.</summary>
		/// <param name="number">The phone number to send to.</param>
		/// <param name="message">The message.</param>
		public void SendSms(string number, string message) {
			this.WriteLine("AT+CMGS=\"+" + number + "\"\r\n");
			Thread.Sleep(100);

			this.WriteLine(message);
			Thread.Sleep(100);

			this.WriteLine((char)26 + "\r");
		}

		/// <summary>Requests to the message at the specified position. Message is returned in the SmsReceived event.</summary>
		/// <param name="position">The position in memory where the message is stored.</param>
		/// <param name="markAsRead">Whether unread messages will be marked as read.</param>
		public void RequestSms(int position, bool markAsRead) {
			this.requestedMessages.Enqueue(position);

			this.SendATCommand("AT+CMGR=" + position + (markAsRead ? ",0" : ",1"));
		}

		/// <summary>Delete an SMS.</summary>
		/// <param name="position">The position in memory where the message is stored.</param>
		public void DeleteSms(int position) {
			this.SendATCommand("AT+CMGD=" + position);
		}

		/// <summary>Requests every SMS.</summary>
		/// <param name="state">The state of the message to filter by.</param>
		public void RequestSmsList(SmsState state) {
			switch (state) {
				case SmsState.All:
					this.SendATCommand("AT+CMGL=\"ALL\""); break;
				case SmsState.ReceivedRead:
					this.SendATCommand("AT+CMGL=\"REC READ\""); break;
				case SmsState.ReceivedUnread:
					this.SendATCommand("AT+CMGL=\"REC UNREAD\""); break;
				case SmsState.StoredSent:
					this.SendATCommand("AT+CMGL=\"STO SENT\""); break;
				case SmsState.StoredUnsent:
					this.SendATCommand("AT+CMGL=\"STO UNSENT\""); break;
			}
		}

		/// <summary>Deletes all SMSs stored in the SIM card.</summary>
		public void DeleteAllSms() {
			this.SendATCommand("AT+CMGD=0,4");
		}

		/// <summary>Picks up an incoming voice call.</summary>
		public void PickUp() {
			this.SendATCommand("ATA");
		}

		/// <summary>Hangs up an active call.</summary>
		public void HangUp() {
			this.SendATCommand("ATH");
		}

		/// <summary>Dials a number in order to start a voice call.</summary>
		/// <param name="number">Number to be called</param>
		public void Dial(string number) {
			this.SendATCommand((number.IndexOf("+") >= 0 ? "ATD" : "ATD+") + number + ";");
		}

		/// <summary>Redials the last number dialed.</summary>
		public void Redial() {
			this.SendATCommand("ATDL");
		}

		/// <summary>Raises the PhoneActivityRequested event, which contains the activity the phone is currently engaged in.</summary>
		public void RequestPhoneActivity() {
			this.SendATCommand("AT+CPAS");
		}

		/// <summary>Adds an entry to the SIM card's phonebook.</summary>
		/// <param name="number">The phone number.</param>
		/// <param name="name">The name.</param>
		public void SaveContact(string number, string name) {
			this.SendATCommand("AT +CPBW=,\"" + number + "\", ,\"" + name + "\"");
		}

		/// <summary>Adds an entry to the SIM card's phonebook.</summary>
		/// <param name="contact">Contact object containing the number and name.</param>
		public void SaveContact(Contact contact) {
			if (contact == null) throw new ArgumentNullException("contact");

			this.SaveContact(contact.PhoneNumber, contact.Name);
		}

		/// <summary>Adds an entry to the SIM card's phonebook.</summary>
		/// <param name="index">Index of the entry where the contact is going to be stored.</param>
		/// <param name="number">The phone number.</param>
		/// <param name="name">The name.</param>
		public void SaveContact(int index, string number, string name) {
			this.SendATCommand("AT+CPBW=" + index + ",\"" + number + "\", ,\"" + name + "\"");
		}

		/// <summary>Adds an entry to the SIM card's phonebook.</summary>
		/// <param name="index">Index of the entry where the contact is going to be stored.</param>
		/// <param name="contact">Contact object containing the number and name.</param>
		public void SaveContact(int index, Contact contact) {
			if (contact == null) throw new ArgumentNullException("contact");

			this.SaveContact(index, contact.PhoneNumber, contact.Name);
		}

		/// <summary>Raises the ContactRequested event, which contains the contact stored in the specified position.</summary>
		/// <param name="index">Index of the phonebook entry where the contact is stored.</param>
		public void RequestContact(int index) {
			this.SendATCommand("AT+CPBR=" + index);
		}

		/// <summary>Delete the contact at the specified position</summary>
		/// <param name="index">Index of the phonebook entry where the contact is stored.</param>
		public void DeleteContact(int index) {
			this.SendATCommand("AT+CPBW=" + index);
		}

		/// <summary>Sends a request to get the module's date and time</summary>
		public void RequestClock() {
			this.SendATCommand("AT+CCLK?");
		}

		/// <summary>Sets the module's internal date and time</summary>
		/// <param name="clock"></param>
		public void SetClock(DateTime clock) {
			this.SendATCommand("AT+CCLK=\"" + clock.ToString("yy") + "/" + clock.ToString("MM") + "/" + clock.ToString("dd") + "," + clock.ToString("HH:mm:ss") + "+00\"");
		}

		/// <summary>Raises the ImeiRequested event which contains the module international mobile equipment identification number.</summary>
		public void RequestImei() {
			this.SendATCommand("AT+GSN");
		}

		/// <summary>Raises the SignalStrengthRequested event which contains the strength of the signal.</summary>
		public void RequestSignalStrength() {
			this.SendATCommand("AT+CSQ");
		}

		/// <summary>Raises the OperatorRequested event which contains the name of the operator, if the module is connected to a network.</summary>
		public void RequestOperator() {
			this.SendATCommand("AT+COPS?");
		}

		/// <summary>Raises the PinStateRequested event which contains the current state of the PIN.</summary>
		public void RequestPinState() {
			this.SendATCommand("AT+CPIN?");
		}

		/// <summary>Attaches to the GPRS network.</summary>
		/// <param name="accessPointName">The APN.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		public void AttachGprs(string accessPointName, string username, string password) {
			this.SendATCommand("AT+CSTT=\"" + accessPointName + "\",\"" + username + "\",\"" + password + "\"");
			this.SendATCommand("AT+CIICR");

			Thread.Sleep(3000);

			this.SendATCommand("AT+CIFSR");
		}

		/// <summary>Detach from the GPRS network.</summary>
		public void DetachGprs() {
			this.SendATCommand("AT+CGATT=0");
		}

		/// <summary>Connects to a TCP server.</summary>
		/// <param name="server">IP address of the server.</param>
		/// <param name="port">Port on the server.</param>
		public void ConnectTcp(string server, int port) {
			this.SendATCommand("AT+CIPSTART=\"TCP\",\"" + server + "\"," + port);
		}

		/// <summary>Disconnects from TCP server.</summary>
		public void DisconnectTcp() {
			this.SendATCommand("AT+CIPCLOSE");
		}

		/// <summary>Configure the module as a TCP server.</summary>
		/// <param name="port">The port to listen on.</param>
		public void ConfigureTcpServer(int port) {
			this.SendATCommand("AT+CIPSERVER=1," + port);
		}

		/// <summary>Sends data over a TCP connection.</summary>
		/// <param name="data">Data to be sent</param>
		public void SendTcpData(string data) {
			this.SendATCommand("AT+CIPSEND");

			Thread.Sleep(1000);

			this.SendATCommand(data + (char)26);
		}

		private void DoWork() {
			var response = string.Empty;
			var command = string.Empty;
			string[] parts;
			Sms sms;

			while (this.running) {
				while (this.serial.BytesToRead != 0)
					this.ReadIn();

				while (this.ExtractLine(ref response)) {
					if (this.atExpectedResponse != string.Empty && response.IndexOf(this.atExpectedResponse) == 0)
						this.atExpectedEvent.Set();

					if (response[0] == '+' && response.IndexOf(":") != -1) {
						this.ParseCommand(ref command, ref response);

						switch (command) {
							#region Check Pin State (CPIN)
							case "CPIN":
								switch (response) {
									case "READY": this.OnPinStateRequested(this, PinState.Ready); break;
									case "SIM PIN2": this.OnPinStateRequested(this, PinState.Pin2); break;
									case "SIM PUK2": this.OnPinStateRequested(this, PinState.Puk2); break;
									case "PH_SIM PIN": this.OnPinStateRequested(this, PinState.PhPin); break;
									case "PH_SIM PUK": this.OnPinStateRequested(this, PinState.PhPuk); break;
									case "SIM PIN": this.OnPinStateRequested(this, PinState.Pin); break;
									case "SIM PUK": this.OnPinStateRequested(this, PinState.Puk); break;
								}

								break;

							#endregion Check Pin State (CPIN)

							#region Check GSM Network Registration (CREG)
							case "CREG":
								switch (response) {
									case "0": this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.NotSearching); break;
									case "1": this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Registered); break;
									case "2": this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Searching); break;
									case "3": this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.RegistrationDenied); break;
									case "4": this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Unknown); break;
									case "5": this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Roaming); break;
								}

								break;

							#endregion Check GSM Network Registration (CREG)

							#region Check GPRS Network Registration (CGREG)
							case "CGREG":
								switch (response) {
									case "0": this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.NotSearching); break;
									case "1": this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Registered); break;
									case "2": this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Searching); break;
									case "3": this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.RegistrationDenied); break;
									case "4": this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Unknown); break;
									case "5": this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Roaming); break;
								}

								break;

							#endregion Check GPRS Network Registration (CGREG)

							#region Check Phone Activity (CPAS)
							case "CPAS":
								switch (response) {
									case "0": this.OnPhoneActivityRequested(this, PhoneActivity.Ready); break;
									case "2": this.OnPhoneActivityRequested(this, PhoneActivity.Unknown); break;
									case "3": this.OnPhoneActivityRequested(this, PhoneActivity.Ringing); break;
									case "4": this.OnPhoneActivityRequested(this, PhoneActivity.CallInProgress); break;
								}

								break;

							#endregion Check Phone Activity (CPAS)

							#region Check Sms Received (CMTI)
							case "CMTI":
								var position = int.Parse(response.Split(',')[1]);

								this.newMessages.Enqueue(position);
								this.RequestSms(position, false);

								break;

							#endregion Check Sms Received (CMTI)

							#region Check Incoming Call (CLIP)
							case "CLIP":
								this.OnIncomingCall(this, response.Split(',')[0].Trim('\"'));

								break;

							#endregion Check Incoming Call (CLIP)

							#region Check Request Contact (CPBR)
							case "CPBR":
								parts = response.Split(',');

								this.OnContactRequested(this, new Contact(parts[1].Trim('\"'), parts[3].Trim('\"')));

								break;

							#endregion Check Request Contact (CPBR)

							#region Check Request Clock (CCLK)
							case "CCLK":
								this.OnClockRequested(this, new DateTime(int.Parse(response.Substring(1, 2)) + 2000, int.Parse(response.Substring(4, 2)), int.Parse(response.Substring(7, 2)), int.Parse(response.Substring(10, 2)), int.Parse(response.Substring(13, 2)), int.Parse(response.Substring(16, 2))));

								break;

							#endregion Check Request Clock (CCLK)

							#region Check Request IMEI (GSN)
							case "GSN":
								this.OnImeiRequested(this, response);

								break;

							#endregion Check Request IMEI (GSN)

							#region Check Request Signal Strength (CSQ)
							case "CSQ":
								int signal = int.Parse(response.Split(',')[0]);

								switch (signal) {
									case 0: this.OnSignalStrengthRequested(this, SignalStrength.VeryWeak); break;
									case 1: this.OnSignalStrengthRequested(this, SignalStrength.Weak); break;
									case 31: this.OnSignalStrengthRequested(this, SignalStrength.VeryStrong); break;
									case 99: this.OnSignalStrengthRequested(this, SignalStrength.Unknown); break;
									default:
										if (signal >= 2 && signal <= 30)
											this.OnSignalStrengthRequested(this, SignalStrength.Strong);

										break;
								}

								break;

							#endregion Check Request Signal Strength (CSQ)

							#region Check Request Operator (COPS)
							case "COPS":
								parts = response.Split(',');

								this.OnOperatorRequested(this, parts.Length == 3 ? parts[2] : null);

								break;

							#endregion Check Request Operator (COPS)

							#region Check Call Connected (COLP)
							case "COLP":
								parts = response.Split(',');

								if (parts.Length == 5)
									this.OnCallConnected(this, parts[0].Trim('\"'));

								break;

							#endregion Check Call Connected (COLP)

							#region Check Request SMS List (CMGL)
							case "CMGL":
								var smsList = new ArrayList();
								var first = true;

								do {
									if (!first) {
										this.responseBuffer = this.responseBuffer.Substring(2);

										this.ExtractLine(ref response);
										this.ParseCommand(ref command, ref response);
									}

									first = false;
									parts = response.Split(',');

									if (parts.Length == 6) {
										sms = new Sms();
										sms.PhoneNumber = parts[2].Trim('"');
										sms.Index = int.Parse(parts[0]);

										this.ExtractLine(ref sms.Message);

										switch (parts[1].Trim('"')) {
											case "REC UNREAD": sms.Status = SmsState.ReceivedUnread; break;
											case "REC READ": sms.Status = SmsState.ReceivedRead; break;
											case "STO UNSENT": sms.Status = SmsState.StoredUnsent; break;
											case "STO SENT": sms.Status = SmsState.StoredSent; break;
											default: sms.Status = SmsState.All; break;
										}

										if (parts[4].Length >= 7 && parts[5].Length >= 7)
											sms.Timestamp = new DateTime(int.Parse(parts[4].Substring(1, 2)) + 2000, int.Parse(parts[4].Substring(4, 2)), int.Parse(parts[4].Substring(7, 2)), int.Parse(parts[5].Substring(0, 2)), int.Parse(parts[5].Substring(3, 2)), int.Parse(parts[5].Substring(6, 2)));

										smsList.Add(sms);
									}

									Thread.Sleep(10);

									this.ReadIn();
								}
								while (this.responseBuffer.Length >= 8 && this.responseBuffer.Substring(0, 8) == "\r\n+CMGL:");

								this.OnSmsListReceived(this, (Sms[])smsList.ToArray(typeof(Sms)));

								break;

							#endregion Check Request SMS List (CMGL)

							#region Check Sms Requested (CMGR)
							case "CMGR":
								parts = response.Split(',');

								if (parts.Length == 5) {
									sms = new Sms();
									sms.PhoneNumber = parts[1].Trim('"');
									sms.Index = (int)this.requestedMessages.Dequeue();

									this.ExtractLine(ref sms.Message);

									switch (parts[0].Trim('"')) {
										case "REC UNREAD": sms.Status = SmsState.ReceivedUnread; break;
										case "REC READ": sms.Status = SmsState.ReceivedRead; break;
										case "STO UNSENT": sms.Status = SmsState.StoredUnsent; break;
										case "STO SENT": sms.Status = SmsState.StoredSent; break;
										default: sms.Status = SmsState.All; break;
									}

									if (parts[3].Length >= 7 && parts[4].Length >= 7)
										sms.Timestamp = new DateTime(int.Parse(parts[3].Substring(1, 2)) + 2000, int.Parse(parts[3].Substring(4, 2)), int.Parse(parts[3].Substring(7, 2)), int.Parse(parts[4].Substring(0, 2)), int.Parse(parts[4].Substring(3, 2)), int.Parse(parts[4].Substring(6, 2)));

									if (this.newMessages.Contains(sms.Index))
										this.newMessages.Dequeue();

									this.OnSmsReceived(this, sms);
								}

								break;

							#endregion Check Sms Requested (CMGR)
						}
					}
					else {
						#region Check No Carrier (NO CARRIER)
						if (response == "NO CARRIER")
							this.OnCallEnded(this, CallEndReason.NoCarrier);

						#endregion Check No Carrier (NO CARRIER)

						#region Check No Dial Tone (NO DIALTONE)
						if (response == "NO DIALTONE")
							this.OnCallEnded(this, CallEndReason.NoDialTone);

						#endregion Check No Dial Tone (NO DIALTONE)

						#region Check Busy (BUSY)
						if (response == "BUSY")
							this.OnCallEnded(this, CallEndReason.Busy);

						#endregion Check Busy (BUSY)

						#region Check Connect (CONNECT)
						if (response == this.pppExpectedResponse)
							this.pppEvent.Set();

						#endregion Check Connect (CONNECT)

						#region Check GPRS Attached (CISFR)
						var index = response.IndexOf(".");
						if (index >= 0 && index <= 3)
							this.OnGprsAttached(this, response);

						#endregion Check GPRS Attached (CISFR)
					}
				}

				Thread.Sleep(250);
			}
		}

		private void ReadIn() {
			if (this.serial.BytesToRead == 0)
				return;

			var read = this.serial.Read(this.buffer, 0, this.buffer.Length);

			for (var i = 0; i < read; i++)
				this.responseBuffer += (char)this.buffer[i];
		}

		private void WriteLine(string line) {
			var buffer = Encoding.UTF8.GetBytes(line);

			this.serial.Write(buffer, 0, buffer.Length);

			this.OnLineSent(this, line);
		}

		private bool ExtractLine(ref string line) {
			var index = this.responseBuffer.IndexOf("\r\n");

			if (index == -1) {
				if (this.serial.BytesToRead == 0)
					return false;

				do {
					this.ReadIn();

					Thread.Sleep(10);
				} while ((index = this.responseBuffer.IndexOf("\r\n")) == -1);
			}

			line = this.responseBuffer.Substring(0, index);

			this.responseBuffer = this.responseBuffer.Substring(index + 2);

			if (line == "\r\n" || line == "")
				return this.ExtractLine(ref line);

			this.OnLineReceived(this, line);

			return true;
		}

		private void ParseCommand(ref string command, ref string response) {
			var first = response.IndexOf("+") + 1;
			var last = response.IndexOf(":", first);

			command = response.Substring(first, last - first);
			response = response.Substring(last + 2);
		}

		private void OnPinStateRequested(CellularRadio sender, PinState pinState) {
			if (Program.CheckAndInvoke(this.PinStateRequested, this.onPinStateRequested, sender, pinState))
				this.PinStateRequested(sender, pinState);
		}

		private void OnGsmNetworkRegistrationChanged(CellularRadio sender, NetworkRegistrationState networkState) {
			if (Program.CheckAndInvoke(this.GsmNetworkRegistrationChanged, this.onGsmNetworkRegistrationChanged, sender, networkState))
				this.GsmNetworkRegistrationChanged(sender, networkState);
		}

		private void OnGprsNetworkRegistrationChanged(CellularRadio sender, NetworkRegistrationState networkState) {
			if (Program.CheckAndInvoke(this.GprsNetworkRegistrationChanged, this.onGprsNetworkRegistrationChanged, sender, networkState))
				this.GprsNetworkRegistrationChanged(sender, networkState);
		}

		private void OnSmsReceived(CellularRadio sender, Sms message) {
			if (Program.CheckAndInvoke(this.SmsReceived, this.onSmsReceived, sender, message))
				this.SmsReceived(sender, message);
		}

		private void OnIncomingCall(CellularRadio sender, string caller) {
			if (Program.CheckAndInvoke(this.IncomingCall, this.onIncomingCall, sender, caller))
				this.IncomingCall(sender, caller);
		}

		private void OnPhoneActivityRequested(CellularRadio sender, PhoneActivity activity) {
			if (Program.CheckAndInvoke(this.PhoneActivityRequested, this.onPhoneActivityRequested, sender, activity))
				this.PhoneActivityRequested(sender, activity);
		}

		private void OnContactRequested(CellularRadio sender, Contact contact) {
			if (Program.CheckAndInvoke(this.ContactRequested, this.onContactRequested, sender, contact))
				this.ContactRequested(sender, contact);
		}

		private void OnClockRequested(CellularRadio sender, DateTime clock) {
			if (Program.CheckAndInvoke(this.ClockRequested, this.onClockRequested, sender, clock))
				this.ClockRequested(sender, clock);
		}

		private void OnImeiRequested(CellularRadio sender, string imei) {
			if (Program.CheckAndInvoke(this.ImeiRequested, this.onImeiRequested, sender, imei))
				this.ImeiRequested(sender, imei);
		}

		private void OnSignalStrengthRequested(CellularRadio sender, SignalStrength signalStrength) {
			if (Program.CheckAndInvoke(this.SignalStrengthRequested, this.onSignalStrengthRequested, sender, signalStrength))
				this.SignalStrengthRequested(sender, signalStrength);
		}

		private void OnOperatorRequested(CellularRadio sender, string operatorName) {
			if (Program.CheckAndInvoke(this.OperatorRequested, this.onOperatorRequested, sender, operatorName))
				this.OperatorRequested(sender, operatorName);
		}

		private void OnSmsListReceived(CellularRadio sender, Sms[] smsList) {
			if (Program.CheckAndInvoke(this.SmsListReceived, this.onSmsListReceived, sender, smsList))
				this.SmsListReceived(sender, smsList);
		}

		private void OnCallEnded(CellularRadio sender, CallEndReason reason) {
			if (Program.CheckAndInvoke(this.CallEnded, this.onCallEnded, sender, reason))
				this.CallEnded(sender, reason);
		}

		private void OnCallConnected(CellularRadio sender, string number) {
			if (Program.CheckAndInvoke(this.CallConnected, this.onCallConnected, sender, number))
				this.CallConnected(sender, number);
		}

		private void OnGprsAttached(CellularRadio sender, string ipAddress) {
			if (Program.CheckAndInvoke(this.GprsAttached, this.onGprsAttached, sender, ipAddress))
				this.GprsAttached(sender, ipAddress);
		}

		private void OnLineReceived(CellularRadio sender, string line) {
			if (Program.CheckAndInvoke(this.LineReceived, this.onLineReceived, sender, line))
				this.LineReceived(sender, line);
		}

		private void OnLineSent(CellularRadio sender, string line) {
			if (Program.CheckAndInvoke(this.LineSent, this.onLineSent, sender, line))
				this.LineSent(sender, line);
		}

		/// <summary>Represents an SMS.</summary>
		public class Sms {
			/// <summary>The phone number.</summary>
			public string PhoneNumber;

			/// <summary>The message text.</summary>
			public string Message;

			/// <summary>The status of the message.</summary>
			public SmsState Status;

			/// <summary>The timestamp of the message.</summary>
			public DateTime Timestamp;

			/// <summary>The index of the message in the SIM card's memory</summary>
			public int Index;

			/// <summary>Creates a new instance.</summary>
			public Sms()
				: this("", "", SmsState.StoredUnsent, DateTime.MinValue) {
			}

			/// <summary>Creates a new instance.</summary>
			/// <param name="number">The phone number.</param>
			/// <param name="text">The message text.</param>
			/// <param name="state">The status of the message.</param>
			/// <param name="timestamp">The timestamp of the message.</param>
			public Sms(string number, string text, SmsState state, DateTime timestamp) {
				this.PhoneNumber = number;
				this.Message = text;
				this.Status = state;
				this.Timestamp = timestamp;
				this.Index = -1;
			}
		}

		/// <summary>Represents an entry in the phonebook.</summary>
		public class Contact {
			/// <summary>The phone number of the contact.</summary>
			public string PhoneNumber;

			/// <summary>The name of the contact.</summary>
			public string Name;

			/// <summary>Creates a new instance.</summary>
			/// <param name="number">The phone number.</param>
			/// <param name="name">The name.</param>
			public Contact(string number, string name) {
				this.PhoneNumber = number;
				this.Name = name;
			}
		}
	}
}