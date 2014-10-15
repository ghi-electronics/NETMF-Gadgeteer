using GHI.Networking;
using System;
using System.Collections;
using System.IO.Ports;
using System.Text;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A CellularRadio module for Microsoft .NET Gadgeteer
    /// </summary>
    public class CellularRadio : GTM.Module.NetworkModule
    {
        private PPPSerialModem networkInterface;
        private SerialPort serial;
        private GTI.DigitalOutput power;
        private Queue newMessages;
        private Queue requestedMessages;
        private Thread worker;
        private bool powerOn;
        private bool moduleBusy;

        /// <summary>
        /// Represents an SMS.
        /// </summary>
        public class Sms
        {
            /// <summary>
            /// The phone number.
            /// </summary>
            public string PhoneNumber;

            /// <summary>
            /// The message text.
            /// </summary>
            public string Message;

            /// <summary>
            /// The status of the message.
            /// </summary>
            public SmsState Status;

            /// <summary>
            /// The timestamp of the message.
            /// </summary>
            public DateTime Timestamp;

            /// <summary>
            /// The index of the message in the SIM card's memory
            /// </summary>
            public int Index;

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            public Sms()
                : this("", "", SmsState.StoredUnsent, DateTime.Now)
            {

            }

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="number">The phone number.</param>
            /// <param name="text">The message text.</param>
            /// <param name="state">The status of the message.</param>
            /// <param name="timestamp">The timestamp of the message.</param>
            public Sms(string number, string text, SmsState state, DateTime timestamp)
            {
                this.PhoneNumber = number;
                this.Message = text;
                this.Status = state;
                this.Timestamp = timestamp;
                this.Index = -1;
            }
        }

        /// <summary>
        /// Represents an entry in the phonebook.
        /// </summary>
        public class Contact
        {
            /// <summary>
            /// The phone number of the contact.
            /// </summary>
            public string PhoneNumber;

            /// <summary>
            /// The name of the contact.
            /// </summary>
            public string Name;

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="number">The phone number.</param>
            /// <param name="name">The name.</param>
            public Contact(string number, string name)
            {
                this.PhoneNumber = number;
                this.Name = name;
            }
        }

        /// <summary>
        /// Method return success states.
        /// </summary>
        public enum ReturnedState
        {
            /// <summary>
            /// The operation was successful.
            /// </summary>
            OK,

            /// <summary>
            /// There was an error in the operation.
            /// </summary>
            Error,

            /// <summary>
            /// The module is off.
            /// </summary>
            ModuleIsOff,

            /// <summary>
            /// The command syntax is incorrect.
            /// </summary>
            InvalidCommand,

            /// <summary>
            /// The module is busy.
            /// </summary>
            ModuleBusy
        }

        /// <summary>
        /// Possible states of network registration.
        /// </summary>
        public enum NetworkRegistrationState
        {
            /// <summary>
            /// The module couldn't find a network.
            /// </summary>
            NotSearching,

            /// <summary>
            /// The module is registered to a network.
            /// </summary>
            Registered,

            /// <summary>
            /// The module is searching for a network.
            /// </summary>
            Searching,

            /// <summary>
            /// The module tried to register to a network, but it was denied.
            /// </summary>
            RegistrationDenied,

            /// <summary>
            /// Unknown failure.
            /// </summary>
            Unknown,

            /// <summary>
            /// The module is roaming.
            /// </summary>
            Roaming,

            /// <summary>
            /// There was an error.
            /// </summary>
            Error
        }

        /// <summary>
        /// Possible states of the SIM card.
        /// </summary>
        public enum PinState
        {
            /// <summary>
            /// The SIM is unlocked and ready to be used.
            /// </summary>
            Ready,

            /// <summary>
            /// The SIM is locked waiting for the PIN.
            /// </summary>
            Pin,

            /// <summary>
            /// The SIM is locked waiting for the PUK.
            /// </summary>
            Puk,

            /// <summary>
            /// The SIM is waiting for phone to SIM card (antitheft).
            /// </summary>
            PhPin,

            /// <summary>
            /// The SIM is waiting for phone to SIM PUK (antitheft).
            /// </summary>
            PhPuk,

            /// <summary>
            /// The SIM is waiting for second PIN.
            /// </summary>
            Pin2,

            /// <summary>
            /// The SIM is waiting for second PUK.
            /// </summary>
            Puk2,

            /// <summary>
            /// The SIM is not present.
            /// </summary>
            NotPresent
        }

        /// <summary>
        /// Possible states for a text message.
        /// </summary>
        public enum SmsState
        {
            /// <summary>
            /// Messages that were received and read
            /// </summary>
            ReceivedUnread,

            /// <summary>
            /// Messages that were received but not yet read
            /// </summary>
            ReceivedRead,

            /// <summary>
            /// Messages that were created but not yet sent
            /// </summary>
            StoredUnsent,

            /// <summary>
            /// Messages that were created and sent
            /// </summary>
            StoredSent,

            /// <summary>
            /// All messages
            /// </summary>
            All
        }

        /// <summary>
        /// Possible states of a call.
        /// </summary>
        public enum PhoneActivity
        {
            /// <summary>
            /// The phone is not calling or being called.
            /// </summary>
            Ready,

            /// <summary>
            /// The phone is ringing.
            /// </summary>
            Ringing,

            /// <summary>
            /// There is an active voice all.
            /// </summary>
            CallInProgress,

            /// <summary>
            /// The module is in an unknown state.
            /// </summary>
            Unknown,

            /// <summary>
            /// The communication line with the module is busy.
            /// </summary>
            CommLineBusy
        }

        /// <summary>
        /// Possible values of the strength of a signal.
        /// </summary>
        public enum SignalStrength
        {
            /// <summary>
            /// -115dBm or less.
            /// </summary>
            VeryWeak,

            /// <summary>
            /// -111dBm.
            /// </summary>
            Weak,

            /// <summary>
            /// -110 to -54dBm.
            /// </summary>
            Strong,

            /// <summary>
            /// -52dBm or greater.
            /// </summary>
            VeryStrong,

            /// <summary>
            /// Not known or undetectable.
            /// </summary>
            Unknown,

            /// <summary>
            /// There was an error in the response from the module.
            /// </summary>
            Error
        }

        /// <summary>
        /// Possible reasons for a call to be ended.
        /// </summary>
        public enum CallEndReason
        {
            /// <summary>
            /// No dial tone was found.
            /// </summary>
            NoDialTone,

            /// <summary>
            /// No carrier was found.
            /// </summary>
            NoCarrier,

            /// <summary>
            /// The line is busy.
            /// </summary>
            Busy
        }

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public CellularRadio(int socketNumber)
        {
            this.newMessages = new Queue();
            this.requestedMessages = new Queue();

            this.powerOn = false;
            this.moduleBusy = false;

            var socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('K', this);

            this.power = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three, false, this);
            this.serial = new SerialPort(socket.SerialPortName, 19200, Parity.None, 8, StopBits.One);
            this.serial.Handshake = Handshake.RequestToSend;

			this.serial.Open();

			this.WriteLine("AT\n");

			Thread.Sleep(1000);

            if (this.serial.BytesToRead != 0)
			{
				this.power.Write(true);
				Thread.Sleep(1200);

				this.power.Write(false);
				Thread.Sleep(500);

                this.serial.DiscardInBuffer();
                this.serial.DiscardOutBuffer();
			}

            this.worker = new Thread(this.DoWork);
            this.worker.Start();

            this.onPinStateRequested = this.OnPinStateRequested;
            this.onGsmNetworkRegistrationChanged = this.OnGsmNetworkRegistrationChanged;
            this.onGprsNetworkRegistrationChanged = this.OnGprsNetworkRegistrationChanged;
            this.onSmsReceived = this.OnSmsReceived;
            this.onIncomingCall = this.OnIncomingCall;
            this.onSmsRequested = this.OnSmsRequested;
            this.onPhoneActivityRequested = this.OnPhoneActivityRequested;
            this.onContactRequested = this.OnContactRequested;
            this.onClockRequested = this.OnClockRequested;
            this.onImeiRequested = this.OnImeiRequested;
            this.onSignalStrengthRequested = this.OnSignalStrengthRequested;
            this.onOperatorRequested = this.OnOperatorRequested;
            this.onSmsListRequested = this.OnSmsListRequested;
            this.onCallEnded = this.OnCallEnded;
            this.onCallConnected = this.OnCallConnected;
            this.onGprsAttached = this.OnGprsAttached;
            this.onModuleInitialized = this.OnModuleInitialized;
        }

        /// <summary>
        /// The underlying network interface.
        /// </summary>
        public PPPSerialModem NetworkInterface
        {
            get
            {
                return this.networkInterface;
            }
        }

        /// <summary>
        /// Opens the underlying network interface and assigns the NETMF networking stack.
        /// </summary>
        /// <param name="apn">The APN to use.</param>
        public void UseThisNetworkInterface(string apn)
        {
            this.UseThisNetworkInterface(apn, "", "", PPPSerialModem.AuthenticationType.None);
        }

        /// <summary>
        /// Opens the underlying network interface and assigns the NETMF networking stack.
        /// </summary>
        /// <param name="apn">The APN to use.</param>
        /// <param name="username">The username to connect with.</param>
        /// <param name="password">The password to connect with.</param>
        /// <param name="authenticationType">The authentication type.</param>
        public void UseThisNetworkInterface(string apn, string username, string password, PPPSerialModem.AuthenticationType authenticationType)
        {
            if (this.networkInterface != null && this.networkInterface.Opened)
                return;

            this.serial.DiscardInBuffer();
            this.serial.DiscardOutBuffer();

            this.SendATCommand("AT+CGDCONT=2,\"IP\",\"" + apn + "\"");
            this.SendATCommand("ATDT*99***2#");

            Thread.Sleep(2500);

            this.worker.Abort();

            this.networkInterface = new PPPSerialModem(this.serial);
            this.networkInterface.Open();
            this.networkInterface.Connect(authenticationType, username, password);

            this.NetworkSettings = this.networkInterface.NetworkInterface;
        }

        /// <summary>
        /// Whether or not the network is connected. Make sure to also check the NetworkUp property to verify network state.
        /// </summary>
        public override bool IsNetworkConnected
        {
            get
            {
                return this.networkInterface.LinkConnected;
            }
        }

        /// <summary>
        /// Powers on the module.
        /// </summary>
        /// <param name="chargeTimeSeconds">The number of seconds to allow the supercapacitor on the module to charge before turning on. Recommended time is 40 seconds from a cold start.</param>
        public void PowerOn(int chargeTimeSeconds)
        {
            new Thread(() => this.PowerOnSequenceThread(chargeTimeSeconds * 1000)).Start();
        }

        /// <summary>
        /// Power on the module.
        /// </summary>
        public void PowerOn()
        {
            this.PowerOn(30);
        }

        /// <summary>
        /// Powers off the module.
        /// </summary>
        public void PowerOff()
        {
            if (this.powerOn)
            {
                this.power.Write(true);
                Thread.Sleep(1200);

                this.power.Write(false);
                Thread.Sleep(500);

                this.powerOn = false;
            }
        }

        /// <summary>
        /// Resets module.
        /// </summary>
        public void Reset()
        {
            this.PowerOff();
            this.PowerOn(2);
        }

        /// <summary>
        /// Sends an AT command to the module. It automatically appends the carriage return.
        /// </summary>
        /// <param name="atCommand">The AT command. See SIM900_ATC_V1.00 for reference.</param>
        /// <returns>The module response to the AT command.</returns>
        public ReturnedState SendATCommand(string atCommand)
        {
            if (this.moduleBusy)
                return ReturnedState.ModuleBusy;

            if (atCommand.IndexOf("\r") < 0)
                atCommand += "\r";

            if (atCommand.IndexOf("AT") < 0)
                return ReturnedState.InvalidCommand;

            if (!powerOn)
                return ReturnedState.ModuleIsOff;

            if (!this.serial.IsOpen)
                return ReturnedState.Error;

            this.WriteLine(atCommand);

            Thread.Sleep(100);

            return ReturnedState.OK;
        }

        /// <summary>
        /// Send an SMS.
        /// </summary>
        /// <param name="number">The phone number to send to.</param>
        /// <param name="message">The message.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState SendSms(string number, string message)
        {
            if (!this.powerOn)
                return ReturnedState.ModuleIsOff;

            if (!this.serial.IsOpen)
                return ReturnedState.Error;

            if (this.moduleBusy)
                return ReturnedState.ModuleBusy;

            this.moduleBusy = true;

            this.WriteLine("AT+CMGS=\"+" + number + "\"\r\n");
            Thread.Sleep(100);

            this.WriteLine(message);
            Thread.Sleep(100);

            this.WriteLine((char)26 + "\r");

            return ReturnedState.OK;
        }

        /// <summary>
        /// Requests to the message at the specified position. Message is returned in the SmsRequested event.
        /// </summary>
        /// <param name="position">The position in memory where the message is stored.</param>
        /// <param name="markAsRead">Whether unread messages will be marked as read.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState RequestSms(int position, bool markAsRead)
        {
            if (this.moduleBusy)
                return ReturnedState.ModuleBusy;

            this.requestedMessages.Enqueue(position);

            return this.SendATCommand("AT+CMGR=" + position + (markAsRead ? ",0" : ",1"));
        }

        /// <summary>
        /// Delete an SMS.
        /// </summary>
        /// <param name="position">The position in memory where the message is stored.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState DeleteSms(int position)
        {
            return this.SendATCommand("AT+CMGD=" + position);
        }

        /// <summary>
        /// Requests every SMS.
        /// </summary>
        /// <param name="state">The state of the message to filter by.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState RequestSmsList(SmsState state)
        {
            switch (state)
            {
                case SmsState.All:
                    return this.SendATCommand("AT+CMGL=\"ALL\"");
                case SmsState.ReceivedRead:
                    return this.SendATCommand("AT+CMGL=\"REC READ\"");
                case SmsState.ReceivedUnread:
                    return this.SendATCommand("AT+CMGL=\"REC UNREAD\"");
                case SmsState.StoredSent:
                    return this.SendATCommand("AT+CMGL=\"STO SENT\"");
                case SmsState.StoredUnsent:
                    return this.SendATCommand("AT+CMGL=\"STO UNSENT\"");
                default:
                    return ReturnedState.InvalidCommand;
            }

        }

        /// <summary>
        /// Deletes all SMSs stored in the SIM card.
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState DeleteAllSms()
        {
            return this.SendATCommand("AT+CMGD=0,4");
        }

        /// <summary>
        /// Picks up an incoming voice call.
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState PickUp()
        {
            return this.SendATCommand("ATA");
        }

        /// <summary>
        /// Hangs up an active call.
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState HangUp()
        {
            return this.SendATCommand("ATH");
        }

        /// <summary>
        /// Dials a number in order to start a voice call.
        /// </summary>
        /// <param name="number">Number to be called</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState Dial(string number)
        {
            var result = this.SendATCommand((number.IndexOf("+") >= 0 ? "ATD" : "ATD+") + number + ";");

            if (result == ReturnedState.OK)
                this.moduleBusy = true;

            return result;
        }

        /// <summary>
        /// Redials the last number dialed.
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState Redial()
        {
            return this.SendATCommand("ATDL");
        }

        /// <summary>
        /// Raises the PhoneActivityRequested event, which contains the activity the phone is currently engaged in.
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState RequestPhoneActivity()
        {
            return this.SendATCommand("AT+CPAS");
        }

        /// <summary>
        /// Adds an entry to the SIM card's phonebook.
        /// </summary>
        /// <param name="number">The phone number.</param>
        /// <param name="name">The name.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState SaveContact(string number, string name)
        {
            return this.SendATCommand("AT +CPBW=,\"" + number + "\", ,\"" + name + "\"");
        }

        /// <summary>
        /// Adds an entry to the SIM card's phonebook.
        /// </summary>
        /// <param name="contact">Contact object containing the number and name.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState SaveContact(Contact contact)
        {
            if (contact == null) throw new ArgumentNullException("contact");

            return this.SaveContact(contact.PhoneNumber, contact.Name);
        }

        /// <summary>
        /// Adds an entry to the SIM card's phonebook.
        /// </summary>
        /// <param name="index">Index of the entry where the contact is going to be stored.</param>
        /// <param name="number">The phone number.</param>
        /// <param name="name">The name.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState SaveContact(int index, string number, string name)
        {
            return this.SendATCommand("AT+CPBW=" + index + ",\"" + number + "\", ,\"" + name + "\"");
        }

        /// <summary>
        /// Adds an entry to the SIM card's phonebook.
        /// </summary>
        /// <param name="index">Index of the entry where the contact is going to be stored.</param>
        /// <param name="contact">Contact object containing the number and name.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState SaveContact(int index, Contact contact)
        {
            if (contact == null) throw new ArgumentNullException("contact");

            return this.SaveContact(index, contact.PhoneNumber, contact.Name);
        }

        /// <summary>
        /// Raises the ContactRequested event, which contains the contact stored in the specified position.
        /// </summary>
        /// <param name="index">Index of the phonebook entry where the contact is stored.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState RequestContact(int index)
        {
            return this.SendATCommand("AT+CPBR=" + index);
        }

        /// <summary>
        /// Delete the contact at the specified position
        /// </summary>
        /// <param name="index">Index of the phonebook entry where the contact is stored.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState DeleteContact(int index)
        {
            return this.SendATCommand("AT+CPBW=" + index);
        }

        /// <summary>
        /// Sends a request to get the module's date and time
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState RequestClock()
        {
            return this.SendATCommand("AT+CCLK?");
        }

        /// <summary>
        /// Sets the module's internal date and time
        /// </summary>
        /// <param name="clock"></param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState SetClock(DateTime clock)
        {
            return this.SendATCommand("AT+CCLK=\"" + clock.ToString("yy") + "/" + clock.ToString("MM") + "/" + clock.ToString("dd") + "," + clock.ToString("HH:mm:ss") + "+00\"");
        }

        /// <summary>
        /// Raises the ImeiRequested event which contains the module international mobile equipment identification number.
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState RequestImei()
        {
            return this.SendATCommand("AT+GSN");
        }

        /// <summary>
        /// Raises the SignalStrengthRequested event which contains the strength of the signal.
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState RequestSignalStrength()
        {
            return this.SendATCommand("AT+CSQ");
        }

        /// <summary>
        /// Raises the OperatorRequested event which contains the name of the operator, if the module is connected to a network.
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState RequestOperator()
        {
            return this.SendATCommand("AT+COPS?");
        }

        /// <summary>
        /// Raises the PinStateRequested event which contains the current state of the PIN.
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState RequestPinState()
        {
            return this.SendATCommand("AT+CPIN?");
        }

        /// <summary>
        /// Attaches to the GPRS network.
        /// </summary>
        /// <param name="accessPointName">The APN.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState AttachGprs(string accessPointName, string username, string password)
        {
            if (this.moduleBusy)
                return ReturnedState.ModuleBusy;

            this.SendATCommand("AT+CSTT=\"" + accessPointName + "\",\"" + username + "\",\"" + password + "\"");
            this.SendATCommand("AT+CIICR");

            Thread.Sleep(3000);

            return this.SendATCommand("AT+CIFSR");
        }

        /// <summary>
        /// Detach from the GPRS network.
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState DetachGprs()
        {
            return this.SendATCommand("AT+CGATT=0");
        }

        /// <summary>
        /// Connects to a TCP server.
        /// </summary>
        /// <param name="server">IP address of the server.</param>
        /// <param name="port">Port on the server.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState ConnectTcp(string server, int port)
        {
            return this.SendATCommand("AT+CIPSTART=\"TCP\",\"" + server + "\"," + port);
        }

        /// <summary>
        /// Disconnects from TCP server.
        /// </summary>
        /// <returns>The module response to the command.</returns>
        public ReturnedState DisconnectTcp()
        {
            return this.SendATCommand("AT+CIPCLOSE");
        }

        /// <summary>
        /// Configure the module as a TCP server.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState ConfigureTcpServer(int port)
        {
            return this.SendATCommand("AT+CIPSERVER=1," + port);
        }

        /// <summary>
        /// Sends data over a TCP connection.
        /// </summary>
        /// <param name="data">Data to be sent</param>
        /// <returns>The module response to the command.</returns>
        public ReturnedState SendTcpData(string data)
        {
            this.SendATCommand("AT+CIPSEND");

            Thread.Sleep(1000);

            return this.SendATCommand(data + (char)26);

        }

        private void PowerOnSequenceThread(int delay)
        {
            Thread.Sleep(delay);

            if (!this.powerOn)
            {
                this.powerOn = true;

                this.power.Write(true);
                Thread.Sleep(1200);

                this.power.Write(false);
                Thread.Sleep(1200);

                this.SendATCommand("AT");
                
                //Set SMS mode to text
				this.SendATCommand("AT+CMGF=1");
				this.SendATCommand("AT+CSDH=0");
                
                // Set the phonebook to be stored in the SIM card
                this.SendATCommand("AT+CPBS=\"SM\"");
                
                // Set the sms to be stored in the SIM card
                this.SendATCommand("AT+CPMS=\"SM\"");
                
                // Sets how connected lines are presented
                this.SendATCommand("AT+COLP=1");
                
                // Enable GPRS network registration status
                this.SendATCommand("AT+CGREG=1");
                
                // Enable GSM network registration status
                this.SendATCommand("AT+CREG=1");

                this.OnModuleInitialized(this);
            }
        }

        private void DoWork()
        {
            var buffer = new byte[256];
            var response = string.Empty;

            while (true)
            {
                Thread.Sleep(100);

                if (serial.IsOpen)
                {
                    response = string.Empty;

                    while (this.serial.BytesToRead != 0)
                    {
                        var read = this.serial.Read(buffer, 0, buffer.Length);

                        for (var i = 0; i < read; i++)
                            response += (char)buffer[i];
                    }

                    if (response == string.Empty)
                        continue;

                    #region Check Pin State (CPIN)
                    if (response.IndexOf("+CPIN:") > 0)
                    {
                        var reply = this.Between(response, "+CPIN", "\n");

                        if (reply.IndexOf("READY") > -1)
                        {
                            this.OnPinStateRequested(this, PinState.Ready);
                        }
                        else if (reply.IndexOf("SIM PIN2") > -1)
                        {
                            this.OnPinStateRequested(this, PinState.Pin2);
                        }
                        else if (reply.IndexOf("SIM PUK2") > -1)
                        {
                            this.OnPinStateRequested(this, PinState.Puk2);
                        }
                        else if (reply.IndexOf("PH_SIM PIN") > -1)
                        {
                            this.OnPinStateRequested(this, PinState.PhPin);
                        }
                        else if (reply.IndexOf("PH_SIM PUK") > -1)
                        {
                            this.OnPinStateRequested(this, PinState.PhPuk);
                        }
                        else if (reply.IndexOf("SIM PIN") > -1)
                        {
                            this.OnPinStateRequested(this, PinState.Pin);
                        }
                        else if (reply.IndexOf("SIM PUK") > -1)
                        {
                            this.OnPinStateRequested(this, PinState.Puk);
                        }
                        else
                        {
                            this.OnPinStateRequested(this, PinState.NotPresent);
                        }
                    }
                    #endregion

                    #region Check GSM Network Registration (CREG)
                    if (response.IndexOf("+CREG:") > 0)
                    {
                        try
                        {
                            switch (int.Parse(this.Between(response, "+CREG:", "\n")))
                            {
                                case 0: this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.NotSearching); break;
                                case 1: this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Registered); break;
                                case 2: this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Searching); break;
                                case 3: this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.RegistrationDenied); break;
                                case 4: this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Unknown); break;
                                case 5: this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Roaming); break;
                            }
                        }
                        catch (Exception)
                        {
                            this.OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Error);
                        }
                    }
                    #endregion

                    #region Check GPRS Network Registration (CGREG)
                    if (response.IndexOf("+CGREG:") > 0)
                    {
                        try
                        {
                            switch (int.Parse(this.Between(response, "+CGREG:", "\n")))
                            {
                                case 0: this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.NotSearching); break;
                                case 1: this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Registered); break;
                                case 2: this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Searching); break;
                                case 3: this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.RegistrationDenied); break;
                                case 4: this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Unknown); break;
                                case 5: this.OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Roaming); break;
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    #endregion

                    #region Check Sms Received (CMTI)
                    if (response.IndexOf("+CMTI:") > 0)
                    {
                        var parts = this.Between(response, "+CMTI:", "\n").Split(',');

                        try
                        {
                            var position = int.Parse(parts[1]);

                            this.newMessages.Enqueue(position);
                            this.RequestSms(position, false);
                        }
                        catch (Exception)
                        {

                        }
                    }

                    #endregion

                    #region Check Incoming Call (CLIP)
                    if (response.IndexOf("+CLIP:") > 0)
                    {
                        var parts = this.Between(response, "+CLIP:", "\n").Split(',');

                        if (parts.Length > 1)
                            this.OnIncomingCall(this, parts[0].Trim('\"'));
                    }
                    #endregion

                    #region Check Sms Requested (CMGR)
                    if (response.IndexOf("+CMGR:") > 0)
                    {
                        var first = response.IndexOf("+CMGR:") + 6;
                        var mid = response.IndexOf("\n", first);
                        var last = response.IndexOf("OK", mid);
                        var parts = response.Substring(first, mid - first).Trim().Split(',');

                        var sms = new Sms();

                        sms.Message = response.Substring(mid, last - mid).Trim();

                        if (parts.Length == 5)
                        {
                            sms.PhoneNumber = parts[1].Trim('\"');

                            if (parts[0].IndexOf("REC UNREAD") > 0)
                            {
                                sms.Status = SmsState.ReceivedUnread;
                            }
                            else if (parts[0].IndexOf("REC READ") > 0)
                            {
                                sms.Status = SmsState.ReceivedRead;
                            }
                            else if (parts[0].IndexOf("STO UNSENT") > 0)
                            {
                                sms.Status = SmsState.StoredUnsent;
                            }
                            else if (parts[0].IndexOf("STO SENT") > 0)
                            {
                                sms.Status = SmsState.StoredSent;
                            }
                            else
                            {
                                sms.Status = SmsState.All;
                            }

                            try
                            {
                                if (parts[3].Length >= 7 && parts[4].Length >= 7)
                                {
                                    sms.Timestamp = new DateTime(int.Parse(parts[3].Substring(1, 2)) + 2000, int.Parse(parts[3].Substring(4, 2)), int.Parse(parts[3].Substring(7, 2)), int.Parse(parts[4].Substring(0, 2)), int.Parse(parts[4].Substring(3, 2)), int.Parse(parts[4].Substring(6, 2)));
                                }
                                else
                                {
                                    sms.Timestamp = new DateTime();
                                }

                                sms.Index = (int)this.requestedMessages.Dequeue();

                                if (this.newMessages.Contains(sms.Index))
                                    this.newMessages.Dequeue();

                                this.OnSmsRequested(this, sms);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }

                    #endregion

                    #region Check Phone Activity (CPAS)
                    if (response.IndexOf("+CPAS:") > 0)
                    {
                        try
                        {
                            switch (int.Parse(this.Between(response, "+CPAS", "\n")))
                            {
                                case 0: this.OnPhoneActivityRequested(this, PhoneActivity.Ready); break;
                                case 2: this.OnPhoneActivityRequested(this, PhoneActivity.Unknown); break;
                                case 3: this.OnPhoneActivityRequested(this, PhoneActivity.Ringing); break;
                                case 4: this.OnPhoneActivityRequested(this, PhoneActivity.CallInProgress); break;
                            }
                        }
                        catch (Exception)
                        {

                        }

                    }
                    #endregion

                    #region Check Request Contact (CPBR)
                    if (response.IndexOf("+CPBR:") > 0)
                    {
                        var parts = this.Between(response, "+CPBR", "\n").Split(',');

                        try
                        {
                            this.OnContactRequested(this, new Contact(parts[1].Trim('\"'), parts[3].Trim('\"')));
                        }
                        catch (Exception)
                        {
                            this.OnSmsReceived(this, null);
                        }
                    }
                    #endregion

                    #region Check Request Clock (CCLK)
                    if (response.IndexOf("+CCLK:") > 0)
                    {
                        var str = this.Between(response, "+CCLK:", "OK");

                        try
                        {
                            this.OnClockRequested(this, new DateTime(int.Parse(str.Substring(1, 2)) + 2000, int.Parse(str.Substring(4, 2)), int.Parse(str.Substring(7, 2)), int.Parse(str.Substring(10, 2)), int.Parse(str.Substring(13, 2)), int.Parse(str.Substring(16, 2))));
                        }
                        catch (Exception)
                        {

                        }
                    }
                    #endregion

                    #region Check Request IMEI (GSN)
                    if (response.IndexOf("GSN") > 0)
                        this.OnImeiRequested(this, this.Between(response, "GSN", "OK"));
                    #endregion

                    #region Check Request Signal Strength (CSQ)
                    if (response.IndexOf("+CSQ:") > 0)
                    {
                        var parts = this.Between(response, "+CSQ:", "OK").Split(',');

                        if (parts.Length == 2)
                        {
                            try
                            {
                                int signal = int.Parse(parts[0]);

                                switch (signal)
                                {
                                    case 0: this.OnSignalStrengthRequested(this, SignalStrength.VeryWeak); break;
                                    case 1: this.OnSignalStrengthRequested(this, SignalStrength.Weak); break;
                                    case 31: this.OnSignalStrengthRequested(this, SignalStrength.VeryStrong); break;
                                    case 99: this.OnSignalStrengthRequested(this, SignalStrength.Unknown); break;
                                    default:
                                        if (signal >= 2 && signal <= 30) 
                                            this.OnSignalStrengthRequested(this, SignalStrength.Strong);

                                        break;
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    #endregion

                    #region Check Request Operator (COPS)
                    if (response.IndexOf("+COPS:") > 0)
                    {
                        var parts = this.Between(response, "+COPS:", "OK").Split(',');
                        if (parts.Length == 3)
                            this.OnOperatorRequested(this, parts.Length == 3 ? parts[2] : null);
                    }
                    #endregion

                    #region Check PowerOn Error (NORMAL POWER DOWN)
                    if (response.IndexOf("NORMAL POWER DOWN") > 0 && this.powerOn)
                        this.Reset();
                    #endregion

                    #region Check Request SMS List (CMGL)
                    if (response.IndexOf("+CMGL:") > 0)
                    {
                        var smsList = new ArrayList();

                        do
                        {
                            var first = response.IndexOf("+CMGL:") + 6;
                            var mid = response.IndexOf("\n", first);
                            var last = response.IndexOf("\n", mid + 1);
                            var parts = response.Substring(first, mid - first).Trim().Split(',');

                            response = response.Substring(last);

                            if (parts.Length == 6)
                            {
                                var sms = new Sms();

                                sms.Message = response.Substring(mid, last - mid).Trim();
                                sms.PhoneNumber = parts[2].Trim('\"');

                                try
                                {
                                    sms.Index = int.Parse(parts[0]);
                                }
                                catch (Exception)
                                {

                                };

                                if (parts[1].IndexOf("REC UNREAD") > 0)
                                {
                                    sms.Status = SmsState.ReceivedUnread;
                                }
                                else if (parts[1].IndexOf("REC READ") > 0)
                                {
                                    sms.Status = SmsState.ReceivedRead;
                                }
                                else if (parts[1].IndexOf("STO UNSENT") > 0)
                                {
                                    sms.Status = SmsState.StoredUnsent;
                                }
                                else if (parts[1].IndexOf("STO SENT") > 0)
                                {
                                    sms.Status = SmsState.StoredSent;
                                }
                                else
                                {
                                    sms.Status = SmsState.All;
                                }

                                try
                                {
                                    if (parts[4].Length >= 7 && parts[5].Length >= 7)
                                    {
                                        sms.Timestamp = new DateTime(int.Parse(parts[4].Substring(1, 2)) + 2000, int.Parse(parts[4].Substring(4, 2)), int.Parse(parts[4].Substring(7, 2)), int.Parse(parts[5].Substring(0, 2)), int.Parse(parts[5].Substring(3, 2)), int.Parse(parts[5].Substring(6, 2)));
                                    }
                                    else
                                    {
                                        sms.Timestamp = new DateTime();
                                    }
                                }
                                catch (Exception)
                                {

                                }

                                smsList.Add(sms);
                            }
                        }
                        while (response.IndexOf("+CMGL") > 0);

                        this.OnSmsListRequested(this, smsList);
                    }
                    #endregion

                    #region Check No Carrier (NO CARRIER)
                    if (response.IndexOf("NO CARRIER") > 0)
                    {
                        this.OnCallEnded(this, CallEndReason.NoCarrier);

                        if (this.moduleBusy)
                            this.moduleBusy = false;
                    }
                    #endregion

                    #region Check No Dial Tone (NO DIALTONE)
                    if (response.IndexOf("NO DIALTONE") > 0)
                    {
                        this.OnCallEnded(this, CallEndReason.NoDialTone);

                        if (this.moduleBusy)
                            this.moduleBusy = false;
                    }
                    #endregion

                    #region Check Busy (BUSY)
                    if (response.IndexOf("BUSY") > 0)
                    {
                        this.OnCallEnded(this, CallEndReason.Busy);

                        if (this.moduleBusy)
                            this.moduleBusy = false;
                    }
                    #endregion

                    #region Check Call Connected (COLP)
                    if (response.IndexOf("+COLP:") > 0)
                    {
                        if (this.moduleBusy)
                            this.moduleBusy = false;

                        var parts = this.Between(response, "+COLP", "\n").Split(',');
                        if (parts.Length == 5)
                            this.OnCallConnected(this, parts[0].Trim('\"'));
                    }
                    #endregion

                    #region Check GPRS Attached (CISFR)
                    if (response.IndexOf("CIFSR") > 0 && response.IndexOf("ERROR") < 0)
                        this.OnGprsAttached(this, this.Between(response, "AT+CIFSR\n\n\n", "\n"));
                    #endregion

                    #region Check SendSms Result
                    if (response.IndexOf("+CMGS:") >= 0 && this.moduleBusy)
                        this.moduleBusy = false;

                    if (response.IndexOf("ERROR") >= 0 && this.moduleBusy)
                        this.moduleBusy = false;
                    #endregion
                }
            }
        }

        private string Between(string source, string left, string right)
        {
            var first = source.IndexOf(left) + left.Length;
            var last = source.IndexOf(right, first + 1);

            return source.Substring(first, last - first).Trim();
        }

        private void WriteLine(string line)
        {
            var buffer = Encoding.UTF8.GetBytes(line);

            this.serial.Write(buffer, 0, buffer.Length);
        }

        private PinStateRequestedHandler onPinStateRequested;
        private GsmNetworkRegistrationChangedHandler onGsmNetworkRegistrationChanged;
        private GprsNetworkRegistrationChangedHandler onGprsNetworkRegistrationChanged;
        private SmsReceivedHandler onSmsReceived;
        private IncomingCallHandler onIncomingCall;
        private SmsRequestedHandler onSmsRequested;
        private PhoneActivityRequestedHandler onPhoneActivityRequested;
        private ContactOpenRequested onContactRequested;
        private ClockRequestedHandler onClockRequested;
        private ImeiRequestedHandler onImeiRequested;
        private SignalStrengthRequestedHandler onSignalStrengthRequested;
        private OperatorRequestedHandler onOperatorRequested;
        private SmsListRequestedHandler onSmsListRequested;
        private CallEndedHandler onCallEnded;
        private CallConnectedHandler onCallConnected;
        private GprsAttachedHandler onGprsAttached;
        private ModuleInitializedHandler onModuleInitialized;

        /// <summary>
        /// Represents the delegate used for the PinStateRequested event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="pinState">Current state of the PIN</param>
        public delegate void PinStateRequestedHandler(CellularRadio sender, PinState pinState);

        /// <summary>
        /// Represents the delegate used for the GsmNetworkRegistrationChanged event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="networkState">Current state of the GSM network registration</param>
        public delegate void GsmNetworkRegistrationChangedHandler(CellularRadio sender, NetworkRegistrationState networkState);

        /// <summary>
        /// Represents the delegate used for the GprsNetworkRegistrationChanged event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="networkState">Current state of the GPRS network registration</param>
        public delegate void GprsNetworkRegistrationChangedHandler(CellularRadio sender, NetworkRegistrationState networkState);

        /// <summary>
        /// Represents the delegate used for the SmsReceived event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Object containing the SMS</param>
        public delegate void SmsReceivedHandler(CellularRadio sender, Sms message);

        /// <summary>
        /// Represents the delegate used for the IncomingCall event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="caller">Number of the caller</param>
        public delegate void IncomingCallHandler(CellularRadio sender, string caller);

        /// <summary>
        /// Represents the delegate used for the SmsRequested event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">SMS that was requested</param>
        public delegate void SmsRequestedHandler(CellularRadio sender, Sms message);

        /// <summary>
        /// Represents the delegate used for the PhoneActivityRequested event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="activity">Current activity in which the phone is engaged</param>
        public delegate void PhoneActivityRequestedHandler(CellularRadio sender, PhoneActivity activity);

        /// <summary>
        /// Represents the delegate used for the ContactRequested event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="contact">Contact object with the requested phonebook entry</param>
        public delegate void ContactOpenRequested(CellularRadio sender, Contact contact);

        /// <summary>
        /// Represents the delegate used for the ClockRequested event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="clock">Module's date and time</param>
        public delegate void ClockRequestedHandler(CellularRadio sender, DateTime clock);

        /// <summary>
        /// Represents the delegate used for the ImeiRequested event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="imei">Module's International Mobile Equipment Identification number</param>
        public delegate void ImeiRequestedHandler(CellularRadio sender, string imei);

        /// <summary>
        /// Represents the delegate used for the SignalStrengthRequested event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="signalStrength">Strength of the signal</param>
        public delegate void SignalStrengthRequestedHandler(CellularRadio sender, SignalStrength signalStrength);

        /// <summary>
        /// Represents the delegate used for the OperatorRequested event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="operatorName">Name of the operator to which the module is connected. It is null if the module is not connected to any operator.</param>
        public delegate void OperatorRequestedHandler(CellularRadio sender, string operatorName);

        /// <summary>
        /// Represents the delegate used for the SmsListRequested event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="smsList">Strength of the signal</param>
        public delegate void SmsListRequestedHandler(CellularRadio sender, ArrayList smsList);

        /// <summary>
        /// Represents the delegate used for the CallEnded event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="reason">The reason the call has ended</param>
        public delegate void CallEndedHandler(CellularRadio sender, CallEndReason reason);

        /// <summary>
        /// Represents the delegate used for the CallConnected event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="number"> Number to which the module is connected</param>
        public delegate void CallConnectedHandler(CellularRadio sender, string number);

        /// <summary>
        /// Represents the delegate used for the GprsAttached event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="ipAddress"> Number to which the module is connected</param>
        public delegate void GprsAttachedHandler(CellularRadio sender, string ipAddress);

        /// <summary>
        /// Represents the delegate used for the ModuleInitialized event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        public delegate void ModuleInitializedHandler(CellularRadio sender);

        /// <summary>
        /// Raised when the pin state is requested.
        /// </summary>
        public event PinStateRequestedHandler PinStateRequested;

        /// <summary>
        /// Raised when the module emits a network registration message.
        /// </summary>
        public event GsmNetworkRegistrationChangedHandler GsmNetworkRegistrationChanged;

        /// <summary>
        /// Raised when the module emits a network registration message.
        /// </summary>
        public event GprsNetworkRegistrationChangedHandler GprsNetworkRegistrationChanged;

        /// <summary>
        /// Raised when the module receives a new SMS.
        /// </summary>
        public event SmsReceivedHandler SmsReceived;

        /// <summary>
        /// Raised when the module detects an incoming call.
        /// </summary>
        public event IncomingCallHandler IncomingCall;

        /// <summary>
        /// Raised when an SMS is requested.
        /// </summary>
        public event SmsRequestedHandler SmsRequested;

        /// <summary>
        /// Raised when the module receives a phone activity message.
        /// </summary>
        public event PhoneActivityRequestedHandler PhoneActivityRequested;

        /// <summary>
        /// Raised when a contact is requested.
        /// </summary>
        public event ContactOpenRequested ContactRequested;

        /// <summary>
        /// Raised when the clock is requested.
        /// </summary>
        public event ClockRequestedHandler ClockRequested;

        /// <summary>
        /// Raised when the IMEI is requested.
        /// </summary>
        public event ImeiRequestedHandler ImeiRequested;

        /// <summary>
        /// Raised when the signal strength is requested.
        /// </summary>
        public event SignalStrengthRequestedHandler SignalStrengthRequested;

        /// <summary>
        /// Raised when the operator is requested.
        /// </summary>
        public event OperatorRequestedHandler OperatorRequested;

        /// <summary>
        /// Raised when the sms list is requested.
        /// </summary>
        public event SmsListRequestedHandler SmsListRequested;

        /// <summary>
        /// Raised when the call ends.
        /// </summary>
        public event CallEndedHandler CallEnded;

        /// <summary>
        /// Raised when the call is connected.
        /// </summary>
        public event CallConnectedHandler CallConnected;

        /// <summary>
        /// Raised when the GPRS is attached.
        /// </summary>
        public event GprsAttachedHandler GprsAttached;

        /// <summary>
        /// Raised when the module finished initialization.
        /// </summary>
        public event ModuleInitializedHandler ModuleInitialized;

        private void OnPinStateRequested(CellularRadio sender, PinState pinState)
        {
            if (Program.CheckAndInvoke(this.PinStateRequested, this.onPinStateRequested, sender, pinState))
                this.PinStateRequested(sender, pinState);
        }

        private void OnGsmNetworkRegistrationChanged(CellularRadio sender, NetworkRegistrationState networkState)
        {
            if (Program.CheckAndInvoke(this.GsmNetworkRegistrationChanged, this.onGsmNetworkRegistrationChanged, sender, networkState))
                this.GsmNetworkRegistrationChanged(sender, networkState);
        }

        private void OnGprsNetworkRegistrationChanged(CellularRadio sender, NetworkRegistrationState networkState)
        {
            if (Program.CheckAndInvoke(this.GprsNetworkRegistrationChanged, this.onGprsNetworkRegistrationChanged, sender, networkState))
                this.GprsNetworkRegistrationChanged(sender, networkState);
        }

        private void OnSmsReceived(CellularRadio sender, Sms message)
        {
            if (Program.CheckAndInvoke(this.SmsReceived, this.onSmsReceived, sender, message))
                this.SmsReceived(sender, message);
        }

        private void OnIncomingCall(CellularRadio sender, string caller)
        {
            if (Program.CheckAndInvoke(this.IncomingCall, this.onIncomingCall, sender, caller))
                this.IncomingCall(sender, caller);
        }

        private void OnSmsRequested(CellularRadio sender, Sms message)
        {
            if (Program.CheckAndInvoke(this.SmsRequested, this.onSmsRequested, sender, message))
                this.SmsRequested(sender, message);
        }

        private void OnPhoneActivityRequested(CellularRadio sender, PhoneActivity activity)
        {
            if (Program.CheckAndInvoke(this.PhoneActivityRequested, this.onPhoneActivityRequested, sender, activity))
                this.PhoneActivityRequested(sender, activity);
        }

        private void OnContactRequested(CellularRadio sender, Contact contact)
        {
            if (Program.CheckAndInvoke(this.ContactRequested, this.onContactRequested, sender, contact))
                this.ContactRequested(sender, contact);
        }

        private void OnClockRequested(CellularRadio sender, DateTime clock)
        {
            if (Program.CheckAndInvoke(this.ClockRequested, this.onClockRequested, sender, clock))
                this.ClockRequested(sender, clock);
        }

        private void OnImeiRequested(CellularRadio sender, string imei)
        {
            if (Program.CheckAndInvoke(this.ImeiRequested, this.onImeiRequested, sender, imei))
                this.ImeiRequested(sender, imei);
        }

        private void OnSignalStrengthRequested(CellularRadio sender, SignalStrength signalStrength)
        {
            if (Program.CheckAndInvoke(this.SignalStrengthRequested, this.onSignalStrengthRequested, sender, signalStrength))
                this.SignalStrengthRequested(sender, signalStrength);
        }

        private void OnOperatorRequested(CellularRadio sender, string operatorName)
        {
            if (Program.CheckAndInvoke(this.OperatorRequested, this.onOperatorRequested, sender, operatorName))
                this.OperatorRequested(sender, operatorName);
        }

        private void OnSmsListRequested(CellularRadio sender, ArrayList smsList)
        {
            if (Program.CheckAndInvoke(this.SmsListRequested, this.onSmsListRequested, sender, smsList))
                this.SmsListRequested(sender, smsList);
        }

        private void OnCallEnded(CellularRadio sender, CallEndReason reason)
        {
            if (Program.CheckAndInvoke(this.CallEnded, this.onCallEnded, sender, reason))
                this.CallEnded(sender, reason);
        }

        private void OnCallConnected(CellularRadio sender, string number)
        {
            if (Program.CheckAndInvoke(this.CallConnected, this.onCallConnected, sender, number))
                this.CallConnected(sender, number);
        }

        private void OnGprsAttached(CellularRadio sender, string ipAddress)
        {
            if (Program.CheckAndInvoke(this.GprsAttached, this.onGprsAttached, sender, ipAddress))
                this.GprsAttached(sender, ipAddress);
        }

        private void OnModuleInitialized(CellularRadio sender)
        {
            if (Program.CheckAndInvoke(this.ModuleInitialized, this.onModuleInitialized, sender))
                this.ModuleInitialized(sender);
        }
    }
}