using System;
using System.Collections;
using Microsoft.SPOT;
using System.Threading;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A CellularRadio module for Microsoft .NET Gadgeteer
    /// </summary>
    public class CellularRadio : GTM.Module
    {
        #region PROPERTIES
        private int powerOnDelay;

        /// <summary>
        /// Power on the CellularRadio module.
        /// </summary>
        /// <param name="chargeTimeSeconds">Time, in seconds, to allow the supercapacitor on the CellularRadio module to charge up before turning on. Recommended time is 40 seconds from a cold start.</param>
        public void PowerOn(int chargeTimeSeconds)
        {
            powerOnDelay = chargeTimeSeconds * 1000;

            Thread powerOnThread = new Thread(new ThreadStart(PowerOnSequenceThread));
            powerOnThread.Start();
        }

        /// <summary>
        /// Power on the CellularRadio module.
        /// </summary>
        public void PowerOn()
        {
            TimeSpan t = GT.Timer.GetMachineTime();
            if (t.Seconds - 40 < 3) PowerOn(3);
            else PowerOn(t.Seconds - 40);
        }


        private void PowerOnSequenceThread()
        {
            Thread.Sleep(powerOnDelay);
            DebugPrint("Turning ON");
            if (!isPowerOn)
            {
                isPowerOn = true;
                if (!serialLine.IsOpen)
                {
                    serialLine.Open();
                    Thread.Sleep(100);
                    readerThread.Resume();
                    Thread.Sleep(100);
                }

                DebugPrint("Turning module on");
                pwrkey.Write(true);
                Thread.Sleep(1200);
                pwrkey.Write(false);
                Thread.Sleep(3000);

                SendATCommand("AT");

                //Set SMS mode to text
                SendATCommand("AT+CMGF=1");
                SendATCommand("AT+CSDH=0");
                // Set the phonebook to be stored in the SIM card
                SendATCommand("AT+CPBS=\"SM\"");
                // Set the sms to be stored in the SIM card
                SendATCommand("AT+CPMS=\"SM\"");
                SendATCommand("AT+CNMI=2,1,0,1,0");
                //// Sets how connected lines are presented
                SendATCommand("AT+COLP=1");
                // Enable GPRS network registration status
                SendATCommand("AT+CGREG=1");
                // Enable GSM network registration status
                SendATCommand("AT+CREG=1");

            }

        }

        /// <summary>
        /// Powers off the Cellular Radio module
        /// </summary>
        public void PowerOff()
        {
            if (isPowerOn)
            {
                pwrkey.Write(true);
                Thread.Sleep(1200);
                pwrkey.Write(false);
                Thread.Sleep(500);
                isPowerOn = false;
                //SendATCommand("AT+CPOWD=1");
            }
        }

        /// <summary>
        /// Time interval in milliseconds to allow the module to initialize 
        /// </summary>
        public int StartupInterval
        {
            get
            {
                return startupInterval;
            }
            set
            {
                startupInterval = value;
            }
        }
        #endregion

        #region PRIVATE ATTRIBUTES
        // Serial line that sends commands to the GSM module
        private GTI.Serial serialLine;
        // Serial reader thread
        private Thread readerThread;
        // Tells whether the module is on
        private bool isPowerOn = false;
        // Power line to the GSM module
        private GTI.DigitalOutput pwrkey;
        private Socket socket;
        private Queue newMessages = new Queue();
        private Queue requestedMessages = new Queue();
        private bool isModuleBusy = false;
        private int startupInterval = 40000;
        #endregion

        #region ENUM DICTIONARY
        /// <summary>
        /// Possible retured states by methods
        /// </summary>
        public enum ReturnedState
        {
            /// <summary>
            /// Operation successful
            /// </summary>
            OK,
            /// <summary>
            /// Error in the operation. See method documentation.
            /// </summary>
            Error,
            /// <summary>
            /// Cellular Radio Module is off
            /// </summary>
            ModuleIsOff,
            /// <summary>
            /// Command syntax is incorrect
            /// </summary>
            InvalidCommand,
            /// <summary>
            /// Module is busy
            /// </summary>
            ModuleBusy

        }
        /// <summary>
        /// Possible states of network registration
        /// </summary>
        public enum NetworkRegistrationState
        {
            /// <summary>
            /// Module couldn't find a network
            /// </summary>
            NotSearching,
            /// <summary>
            /// Module is registered to a network
            /// </summary>
            Registered,
            /// <summary>
            /// Module is searching for a network
            /// </summary>
            Searching,
            /// <summary>
            /// Module tried to register to a network, but it was denied
            /// </summary>
            RegistrationDenied,
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown,
            /// <summary>
            /// Roaming
            /// </summary>
            Roaming,
            /// <summary>
            /// Error
            /// </summary>
            Error
        }

        /// <summary>
        /// Possible states of the SIM card
        /// </summary>
        public enum PINState
        {
            /// <summary>
            /// SIM is unlocked and ready to be used.
            /// </summary>
            Ready,
            /// <summary>
            /// SIM is locked waiting for the PIN
            /// </summary>
            PIN,
            /// <summary>
            /// SIM is locked waiting for the PUK
            /// </summary>
            PUK,
            /// <summary>
            /// SIM is waiting for phone to SIM card (antitheft) 
            /// </summary>
            PH_PIN,// ----> Not sure of how this works
            /// <summary>
            /// SIM is waiting for phone to SIM PUK (antitheft) 
            /// </summary>
            PH_PUK, // ----> Not sure of how this works
            /// <summary>
            /// SIM is waiting for second PIN
            /// </summary>
            PIN2,
            /// <summary>
            /// SIM is waiting for second PUK
            /// </summary>
            PUK2,
            /// <summary>
            /// SIM is not present
            /// </summary>
            NotPresent
        }

        /// <summary>
        /// Possible states for a text message
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
        /// Possible states of a call
        /// </summary>
        public enum PhoneActivityType
        {
            /// <summary>
            /// Phone is not calling or being called
            /// </summary>
            Ready,
            /// <summary>
            /// Ringer is active
            /// </summary>
            Ringing,
            /// <summary>
            /// Active voice all
            /// </summary>
            CallInProgress,
            /// <summary>
            /// Module is not guaranteed to respond.
            /// </summary>
            Unknown,
            /// <summary>
            /// Communication line with the Cellular Radio Module is busy
            /// </summary>
            CommLineBusy
        }

        /// <summary>
        /// Possible values of the strength of a signal
        /// </summary>
        public enum SignalStrengthType
        {
            /// <summary>
            /// -115dBm or less
            /// </summary>
            VeryWeak,
            /// <summary>
            /// -111dBm
            /// </summary>
            Weak,
            /// <summary>
            /// -110..-54dBm
            /// </summary>
            Strong,
            /// <summary>
            /// -52dBm or greater
            /// </summary>
            VeryStrong,
            /// <summary>
            /// Not known or undetectable
            /// </summary>
            Unknown,
            /// <summary>
            /// Error in the response from the GSM Module
            /// </summary>
            Error
        }
        /// <summary>
        /// Reasons for a call to be ended
        /// </summary>
        public enum CallEndType
        {
            /// <summary>
            /// No dial tone was found
            /// </summary>
            NoDialTone,
            /// <summary>
            /// No carrier was found
            /// </summary>
            NoCarrier,
            /// <summary>
            /// Line is busy
            /// </summary>
            Busy
        }
        #endregion

        #region CONSTRUCTOR
        /// <summary>Instantiates a Cellular Radio Module</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public CellularRadio(int socketNumber)
        {
            socket = Socket.GetSocket(socketNumber, true, this, null);
            pwrkey = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three, false, this);
            serialLine = GTI.SerialFactory.Create(socket, 19200, GTI.SerialParity.None, GTI.SerialStopBits.One, 8, GTI.HardwareFlowControl.Required, this);

            serialLine.Open();
            serialLine.Write("AT");
            Thread.Sleep(1000);
            String response = "";
            while (serialLine.BytesToRead > 0)
            {
                response += (char)serialLine.ReadByte();
            }
            DebugPrint("RESP: " + response);
            if (response.Length != 0)
            {
                DebugPrint("Sending off pulse");
                pwrkey.Write(true);
                Thread.Sleep(1200);
                pwrkey.Write(false);
                Thread.Sleep(500);
            }
            isPowerOn = false;

            readerThread = new Thread(new ThreadStart(run));
            readerThread.Start();
        }

        #endregion

        #region COMMUNICATION

        /// <summary>
        /// Sends an AT command to the Cellular Radio Module. It automatically appends the carriage return.
        /// </summary>
        /// <param name="atCommand">String containing the AT command. See SIM900_ATC_V1.00 for reference.</param>
        /// <returns>
        /// ReturnedState.OK - Command was sent
        /// ReturnedState.ModuleIsOff - Module is off
        /// ReturnedState.Error - Serial line is not open
        /// ReturnedState.InvalidCommand - Invalid AT command
        /// </returns>
        public ReturnedState SendATCommand(string atCommand)
        {
            // Check if module is busy
            if (isModuleBusy) return ReturnedState.ModuleBusy;

            // Append carriage return
            if (atCommand.IndexOf("\r") < 0) atCommand += "\r";

            // Check if string is an AT command
            if (atCommand.IndexOf("AT") < 0) return ReturnedState.InvalidCommand;

            // Check if module is on
            if (!isPowerOn) return ReturnedState.ModuleIsOff;

            // Check if serial line is open
            if (serialLine.IsOpen) serialLine.Write(atCommand);
            else return ReturnedState.Error;
            Thread.Sleep(100);
            DebugPrint("SENT: " + atCommand);
            return ReturnedState.OK;
        }
        #endregion

        #region SERIAL READER THREAD
        private void run()
        {
            string response;
            while (true)
            {
                if (serialLine.IsOpen)
                {
                    response = "";
                    while (serialLine.BytesToRead > 0)
                    {
                        response += (char)serialLine.ReadByte();
                        //DebugPrint(commBuff);
                    }
                    if (response.Length > 0) DebugPrint("<" + response + ">");

                    //Response Parsing
                    int first;
                    int last;
                    string reply;

                    #region Check Pin State (CPIN)
                    if (response.IndexOf("+CPIN:") > 0)
                    {

                        first = response.IndexOf("+CPIN:");
                        last = response.IndexOf("\n", first);
                        reply = (response.Substring(first, last - first)).Trim();

                        if (reply.IndexOf("READY") > -1)
                        {
                            OnPinStateRetrieved(this, PINState.Ready);
                        }
                        else if (reply.IndexOf("SIM PIN2") > -1)
                        {
                            OnPinStateRetrieved(this, PINState.PIN2);
                        }
                        else if (reply.IndexOf("SIM PUK2") > -1)
                        {
                            OnPinStateRetrieved(this, PINState.PUK2);
                        }
                        else if (reply.IndexOf("PH_SIM PIN") > -1)
                        {
                            OnPinStateRetrieved(this, PINState.PH_PIN);
                        }
                        else if (reply.IndexOf("PH_SIM PUK") > -1)
                        {
                            OnPinStateRetrieved(this, PINState.PH_PUK);
                        }
                        else if (reply.IndexOf("SIM PIN") > -1)
                        {
                            OnPinStateRetrieved(this, PINState.PIN);
                        }
                        else if (reply.IndexOf("SIM PUK") > -1)
                        {
                            OnPinStateRetrieved(this, PINState.PUK);
                        }
                        else
                        {
                            OnPinStateRetrieved(this, PINState.NotPresent);
                        }
                    }
                    #endregion

                    #region Check GSM Network Registration (CREG)
                    if (response.IndexOf("+CREG:") > 0)
                    {
                        string cmd = "+CREG:";
                        first = response.IndexOf(cmd) + cmd.Length;
                        last = response.IndexOf("\n", first);
                        reply = (response.Substring(first, last - first)).Trim();

                        try
                        {
                            int state = int.Parse(reply);
                            switch (state)
                            {
                                case 0:
                                    OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.NotSearching);
                                    break;
                                case 1:
                                    OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Registered);
                                    break;
                                case 2:
                                    OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Searching);
                                    break;
                                case 3:
                                    OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.RegistrationDenied);
                                    break;
                                case 4:
                                    OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Unknown);
                                    break;
                                case 5:
                                    OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Roaming);
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            OnGsmNetworkRegistrationChanged(this, NetworkRegistrationState.Error);
                        }
                    }
                    #endregion

                    #region Check GPRS Network Registration (CGREG)
                    if (response.IndexOf("+CGREG:") > 0)
                    {
                        string cmd = "+CGREG:";

                        first = response.IndexOf(cmd) + cmd.Length;
                        last = response.IndexOf("\n", first);
                        reply = (response.Substring(first, last - first)).Trim();
                        try
                        {
                            int state = int.Parse(reply);
                            switch (state)
                            {
                                case 0:
                                    OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.NotSearching);
                                    break;
                                case 1:
                                    OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Registered);
                                    break;
                                case 2:
                                    OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Searching);
                                    break;
                                case 3:
                                    OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.RegistrationDenied);
                                    break;
                                case 4:
                                    OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Unknown);
                                    break;
                                case 5:
                                    OnGprsNetworkRegistrationChanged(this, NetworkRegistrationState.Roaming);
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            DebugPrint("CGREG ERROR");
                        }
                    }
                    #endregion

                    #region Check Sms Received (CMTI)
                    if (response.IndexOf("+CMTI:") > 0)
                    {

                        string cmd = "+CMTI:";
                        first = response.IndexOf(cmd) + cmd.Length;
                        last = response.IndexOf("\n", first);
                        reply = (response.Substring(first, last - first)).Trim();
                        char[] sep = { ',' };
                        string[] split = reply.Split(sep);

                        try
                        {
                            int position = int.Parse(split[1]);
                            newMessages.Enqueue(position);
                            RetrieveSms(position, false);
                        }
                        catch (Exception)
                        {
                            DebugPrint("ERROR");
                        }
                    }

                    #endregion

                    #region Check Incoming Call (CLIP)
                    if (response.IndexOf("+CLIP:") > 0)
                    {

                        string cmd = "+CLIP:";
                        first = response.IndexOf(cmd) + cmd.Length;
                        last = response.IndexOf("\n", first);
                        reply = (response.Substring(first, last - first)).Trim();
                        char[] sep = { ',' };
                        string[] split = reply.Split(sep);
                        if (split.Length > 1)
                        {
                            OnIncomingCall(this, split[0].Trim('\"'));
                        }
                    }
                    #endregion

                    #region Check Sms Retrieved (CMGR)
                    if (response.IndexOf("+CMGR:") > 0)
                    {

                        string cmd = "+CMGR:";
                        first = response.IndexOf(cmd) + cmd.Length;
                        int mid = response.IndexOf("\n", first);
                        last = response.IndexOf("OK", mid);

                        Sms sms = new Sms();
                        // Get message
                        sms.TextMessage = (response.Substring(mid, last - mid)).Trim();


                        reply = (response.Substring(first, mid - first)).Trim();
                        char[] sep = { ',' };
                        string[] split = reply.Split(sep);

                        if (split.Length == 5)
                        {
                            // Get number
                            sms.TelephoneNumber = split[1].Trim('\"');

                            // Get status
                            if (split[0].IndexOf("REC UNREAD") > 0)
                            {
                                sms.Status = SmsState.ReceivedUnread;
                            }
                            else if (split[0].IndexOf("REC READ") > 0)
                            {
                                sms.Status = SmsState.ReceivedRead;
                            }
                            else if (split[0].IndexOf("STO UNSENT") > 0)
                            {
                                sms.Status = SmsState.StoredUnsent;
                            }
                            else if (split[0].IndexOf("STO SENT") > 0)
                            {
                                sms.Status = SmsState.StoredSent;
                            }
                            else
                            {
                                // ERROR
                                sms.Status = SmsState.All;
                            }

                            // Get timestamp
                            try
                            {
                                if ((split[3].Length < 7) || (split[4].Length < 7))
                                {
                                    sms.Timestamp = new DateTime();
                                }
                                else
                                {

                                    DateTime timestamp = new DateTime(int.Parse(split[3].Substring(1, 2)) + 2000, //Year
                                    int.Parse(split[3].Substring(4, 2)), //Month
                                    int.Parse(split[3].Substring(7, 2)), // Day
                                    int.Parse(split[4].Substring(0, 2)), // Hour
                                    int.Parse(split[4].Substring(3, 2)), // Minute
                                    int.Parse(split[4].Substring(6, 2))); // Second
                                    sms.Timestamp = timestamp;
                                    int reqIndex = (int)requestedMessages.Dequeue();
                                    DebugPrint("REQ: " + reqIndex);
                                    sms.Index = reqIndex;
                                    if (newMessages.Contains(reqIndex))
                                    {
                                        newMessages.Dequeue();
                                        OnSmsReceived(this, sms);
                                    }
                                    else
                                    {
                                        OnSmsRetrieved(this, sms);
                                    }
                                }
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
                        string atCommand = "+CPAS:";
                        first = response.IndexOf(atCommand) + atCommand.Length;
                        last = response.IndexOf("\n", first);
                        reply = (response.Substring(first, last - first)).Trim();
                        DebugPrint("R:[" + reply + "]");
                        try
                        {
                            int status = int.Parse(reply);
                            switch (status)
                            {
                                case 0:
                                    OnPhoneActivityRetrieved(this, PhoneActivityType.Ready);
                                    break;
                                case 2:
                                    OnPhoneActivityRetrieved(this, PhoneActivityType.Unknown);
                                    break;
                                case 3:
                                    OnPhoneActivityRetrieved(this, PhoneActivityType.Ringing);
                                    break;
                                case 4:
                                    OnPhoneActivityRetrieved(this, PhoneActivityType.CallInProgress);
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            DebugPrint("Error");
                        }

                    }
                    #endregion

                    #region Check Retrieve Contact (CPBR)
                    if (response.IndexOf("+CPBR:") > 0)
                    {

                        string cmd = "+CPBR:";
                        first = response.IndexOf(cmd) + cmd.Length;
                        last = response.IndexOf("\n", first);
                        reply = (response.Substring(first, last - first)).Trim();
                        char[] sep = { ',' };
                        string[] split = reply.Split(sep);

                        try
                        {
                            Contact contact = new Contact(split[1].Trim('\"'), split[3].Trim('\"'));
                            OnContactRetrieved(this, contact);
                        }
                        catch (Exception)
                        {
                            DebugPrint("ERROR");
                            OnSmsReceived(this, null);
                        }
                    }
                    #endregion

                    #region Check Retrieve Clock (CCLK)
                    if (response.IndexOf("+CCLK:") > 0)
                    {
                        string atCommand = "+CCLK:";
                        first = response.IndexOf(atCommand) + atCommand.Length;
                        last = response.IndexOf("OK", first);

                        string s = (response.Substring(first, last - first)).Trim();
                        try
                        {
                            DateTime ret = new DateTime(int.Parse(s.Substring(1, 2)) + 2000,
                            int.Parse(s.Substring(4, 2)),
                            int.Parse(s.Substring(7, 2)),
                            int.Parse(s.Substring(10, 2)),
                            int.Parse(s.Substring(13, 2)),
                            int.Parse(s.Substring(16, 2)));
                            OnClockRetrieved(this, ret);
                        }
                        catch (Exception)
                        {
                            DebugPrint("Clock Error");
                        }
                    }
                    #endregion

                    #region Check Retrieve IMEI (GSN)
                    if (response.IndexOf("GSN") > 0)
                    {
                        string atCommand = "GSN";
                        first = response.IndexOf(atCommand) + atCommand.Length;
                        last = response.IndexOf("OK", first);
                        reply = (response.Substring(first, last - first)).Trim();
                        OnImeiRetrieved(this, reply);
                    }

                    #endregion

                    #region Check Retrieve Signal Strength (CSQ)
                    if (response.IndexOf("+CSQ:") > 0)
                    {
                        string atCommand = "+CSQ:";

                        //String parsing
                        // Return format: +CSQ: <rssi>,<ber>
                        first = response.IndexOf(atCommand) + atCommand.Length;
                        last = response.IndexOf("OK", first);

                        reply = (response.Substring(first, last - first)).Trim();
                        char[] sep = new char[1];
                        sep[0] = ',';
                        //DebugPrint("R: " + s);
                        String[] split = reply.Split(sep);
                        if (split.Length == 2)
                        {
                            try
                            {
                                int signal = int.Parse(split[0]);
                                switch (signal)
                                {
                                    case 0: OnSignalStrengthRetrieved(this, SignalStrengthType.VeryWeak); break;
                                    case 1: OnSignalStrengthRetrieved(this, SignalStrengthType.Weak); break;
                                    case 31: OnSignalStrengthRetrieved(this, SignalStrengthType.VeryStrong); break;
                                    case 99: OnSignalStrengthRetrieved(this, SignalStrengthType.Unknown); break;
                                    default:
                                        if (signal >= 2 && signal <= 30) OnSignalStrengthRetrieved(this, SignalStrengthType.Strong);
                                        break;
                                }
                            }
                            catch (Exception)
                            {
                                DebugPrint("Signal Strength Error");
                            }
                        }
                    }
                    #endregion

                    #region Check Retrieve Operator (COPS)
                    if (response.IndexOf("+COPS:") > 0)
                    {
                        string atCommand = "+COPS:";

                        //String parsing  
                        // Return format: +COPS:<mode>[,<format>,<oper>]
                        first = response.IndexOf(atCommand) + atCommand.Length;
                        last = response.IndexOf("OK", first);
                        reply = (response.Substring(first, last - first)).Trim();
                        char[] sep = new char[1];
                        sep[0] = ',';
                        String[] split = reply.Split(sep);
                        if (split.Length == 3)
                        {
                            OnOperatorRetrieved(this, split[2]);
                        }
                        else
                        {
                            OnOperatorRetrieved(this, null);
                        }

                    }
                    #endregion

                    #region Check Power On Error (NORMAL POWER DOWN)
                    if (response.IndexOf("NORMAL POWER DOWN") > 0)
                    {
                        if (isPowerOn)
                        {
                            Reset();
                        }
                    }
                    #endregion

                    #region Check Retrieve SMS List (CMGL)
                    if (response.IndexOf("+CMGL:") > 0)
                    {
                        first = 0;
                        string cmd = "+CMGL:";
                        Sms sms;
                        int mid;
                        ArrayList smsList = new ArrayList();
                        char[] sep = { ',' };
                        do
                        {
                            first = response.IndexOf(cmd, first) + cmd.Length;
                            mid = response.IndexOf("\n", first);
                            last = response.IndexOf("\n", mid + 1);
                            reply = (response.Substring(first, mid - first)).Trim();
                            string[] split = reply.Split(sep);

                            sms = new Sms();
                            if (split.Length == 6)
                            {
                                // Get index
                                try
                                {
                                    sms.Index = int.Parse(split[0]);
                                }
                                catch (Exception) { };
                                // Get message
                                sms.TextMessage = (response.Substring(mid, last - mid)).Trim();
                                // Get number
                                sms.TelephoneNumber = split[2].Trim('\"');

                                // Get status
                                if (split[1].IndexOf("REC UNREAD") > 0)
                                {
                                    sms.Status = SmsState.ReceivedUnread;
                                }
                                else if (split[1].IndexOf("REC READ") > 0)
                                {
                                    sms.Status = SmsState.ReceivedRead;
                                }
                                else if (split[1].IndexOf("STO UNSENT") > 0)
                                {
                                    sms.Status = SmsState.StoredUnsent;
                                }
                                else if (split[1].IndexOf("STO SENT") > 0)
                                {
                                    sms.Status = SmsState.StoredSent;
                                }
                                else
                                {
                                    // ERROR
                                    sms.Status = SmsState.All;
                                }

                                // Get timestamp
                                try
                                {
                                    if ((split[4].Length < 7) || (split[5].Length < 7))
                                    {
                                        sms.Timestamp = new DateTime();
                                    }
                                    else
                                    {

                                        DateTime timestamp = new DateTime(int.Parse(split[4].Substring(1, 2)) + 2000, //Year
                                        int.Parse(split[4].Substring(4, 2)), //Month
                                        int.Parse(split[4].Substring(7, 2)), // Day
                                        int.Parse(split[5].Substring(0, 2)), // Hour
                                        int.Parse(split[5].Substring(3, 2)), // Minute
                                        int.Parse(split[5].Substring(6, 2))); // Second
                                        sms.Timestamp = timestamp;
                                        smsList.Add(sms);
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        while (response.IndexOf("+CMGL", mid) > 0);
                        OnSmsListRetrieved(this, smsList);
                    }
                    #endregion

                    #region Check No Carrier (NO CARRIER)
                    if (response.IndexOf("NO CARRIER") > 0)
                    {
                        OnCallEnded(this, CallEndType.NoCarrier);
                        if (isModuleBusy) isModuleBusy = false;
                    }
                    #endregion

                    #region Check No Dial Tone (NO DIALTONE)
                    if (response.IndexOf("NO DIALTONE") > 0)
                    {
                        OnCallEnded(this, CallEndType.NoDialTone);
                        if (isModuleBusy) isModuleBusy = false;
                    }
                    #endregion

                    #region Check Busy (BUSY)
                    if (response.IndexOf("BUSY") > 0)
                    {
                        OnCallEnded(this, CallEndType.Busy);
                        if (isModuleBusy) isModuleBusy = false;
                    }
                    #endregion


                    #region Check Call Connected (COLP)
                    if (response.IndexOf("+COLP:") > 0)
                    {
                        if (isModuleBusy) isModuleBusy = false;
                        string atCommand = "+COLP";

                        //String parsing  
                        // Return format: +COPS:<mode>[,<format>,<oper>]
                        first = response.IndexOf(atCommand) + atCommand.Length;
                        last = response.IndexOf("\n", first);
                        reply = (response.Substring(first, last - first)).Trim();
                        char[] sep = new char[1];
                        sep[0] = ',';
                        String[] split = reply.Split(sep);
                        if (split.Length == 5)
                        {
                            OnCallConnected(this, split[0].Trim('\"'));
                        }
                    }
                    #endregion

                    #region Check Module Initialization (Call Ready)
                    if (response.IndexOf("Call Ready") >= 0)
                    {
                        OnModuleInitialized(this);
                    }
                    #endregion

                    #region Check GPRS Attached (CISFR)

                    if (response.IndexOf("CIFSR") > 0)
                    {
                        string atCommand = "AT+CIFSR\n\n\n";
                        if (response.IndexOf("ERROR") < 0)
                        {
                            first = response.IndexOf(atCommand) + atCommand.Length;
                            last = response.IndexOf("\n", first + 1);
                            reply = (response.Substring(first, last - first)).Trim();
                            OnGprsAttached(this, reply);
                        }
                    }
                    #endregion

                    #region Check SendSms Result
                    if (response.IndexOf("+CMGS:") >= 0)
                    {
                        if (isModuleBusy) isModuleBusy = false;
                    }
                    if ((response.IndexOf("ERROR") >= 0) && (isModuleBusy))
                    {
                        isModuleBusy = false;
                    }
                    #endregion

                }
                Thread.Sleep(100);
            }
        }
        #endregion

        #region DELEGATES AND EVENTS
        #region Pin State Retrieved
        /// <summary>
        /// Represents the delegate used for the <see cref="PinStateRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="pinState">Current state of the PIN</param>
        public delegate void PinStateRetrievedHandler(CellularRadio sender, PINState pinState);
        /// <summary>
        /// Event raised by the <see cref="RetrievePinState"/> method.
        /// </summary>
        public event PinStateRetrievedHandler PinStateRetrieved;
        private PinStateRetrievedHandler onPinStateRetrieved;

        /// <summary>
        /// Raises the <see cref="PinStateRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="pinState">Current state of the PIN</param>
        protected virtual void OnPinStateRetrieved(CellularRadio sender, PINState pinState)
        {
            if (onPinStateRetrieved == null) onPinStateRetrieved = new PinStateRetrievedHandler(OnPinStateRetrieved);
            if (Program.CheckAndInvoke(PinStateRetrieved, onPinStateRetrieved, sender, pinState))
            {
                PinStateRetrieved(sender, pinState);
            }
        }
        #endregion

        #region GSM Network Registration Changed
        /// <summary>
        /// Represents the delegate used for the <see cref="GsmNetworkRegistrationChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="networkState">Current state of the GSM network registration</param>
        public delegate void GsmNetworkRegistrationChangedHandler(CellularRadio sender, NetworkRegistrationState networkState);
        /// <summary>
        /// Event raised when the module emits a network registration message.
        /// </summary>
        public event GsmNetworkRegistrationChangedHandler GsmNetworkRegistrationChanged;
        private GsmNetworkRegistrationChangedHandler onGsmNetworkRegistrationChanged;

        /// <summary>
        /// Raises the <see cref="GsmNetworkRegistrationChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="networkState">Current state of the GSM network registration</param>
        protected virtual void OnGsmNetworkRegistrationChanged(CellularRadio sender, NetworkRegistrationState networkState)
        {
            if (onGsmNetworkRegistrationChanged == null) onGsmNetworkRegistrationChanged = new GsmNetworkRegistrationChangedHandler(OnGsmNetworkRegistrationChanged);
            if (Program.CheckAndInvoke(GsmNetworkRegistrationChanged, onGsmNetworkRegistrationChanged, sender, networkState))
            {
                GsmNetworkRegistrationChanged(sender, networkState);
            }
        }
        #endregion

        #region GPRS Network Registration Changed
        /// <summary>
        /// Represents the delegate used for the <see cref="GprsNetworkRegistrationChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="networkState">Current state of the GPRS network registration</param>
        public delegate void GprsNetworkRegistrationChangedHandler(CellularRadio sender, NetworkRegistrationState networkState);
        /// <summary>
        /// Event raised when the module emits a network registration message.
        /// </summary>
        public event GprsNetworkRegistrationChangedHandler GprsNetworkRegistrationChanged;
        private GprsNetworkRegistrationChangedHandler onGprsNetworkRegistrationChanged;

        /// <summary>
        /// Raises the <see cref="GprsNetworkRegistrationChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="networkState">Current state of the GPRS network registration</param>
        protected virtual void OnGprsNetworkRegistrationChanged(CellularRadio sender, NetworkRegistrationState networkState)
        {
            if (onGprsNetworkRegistrationChanged == null) onGprsNetworkRegistrationChanged = new GprsNetworkRegistrationChangedHandler(OnGprsNetworkRegistrationChanged);
            if (Program.CheckAndInvoke(GprsNetworkRegistrationChanged, onGprsNetworkRegistrationChanged, sender, networkState))
            {
                GprsNetworkRegistrationChanged(sender, networkState);
            }
        }
        #endregion

        #region Sms Received
        /// <summary>
        /// Represents the delegate used for the <see cref="SmsReceived"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Object containing the SMS message</param>
        public delegate void SmsReceivedHandler(CellularRadio sender, Sms message);
        /// <summary>
        /// Event raised when the module receives a new SMS message.
        /// </summary>
        public event SmsReceivedHandler SmsReceived;
        private SmsReceivedHandler onSmsReceived;

        /// <summary>
        /// Raises the <see cref="SmsReceived"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="message">Object containing the SMS message</param>
        protected virtual void OnSmsReceived(CellularRadio sender, Sms message)
        {
            if (onSmsReceived == null) onSmsReceived = new SmsReceivedHandler(OnSmsReceived);
            if (Program.CheckAndInvoke(SmsReceived, onSmsReceived, sender, message))
            {
                SmsReceived(sender, message);
            }
        }
        #endregion

        #region Incoming Call
        /// <summary>
        /// Represents the delegate used for the <see cref="IncomingCall"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="caller">Number of the caller</param>
        public delegate void IncomingCallHandler(CellularRadio sender, string caller);
        /// <summary>
        /// Event raised when the module detects an incoming call.
        /// </summary>
        public event IncomingCallHandler IncomingCall;
        private IncomingCallHandler onIncomingCall;

        /// <summary>
        /// Raises the <see cref="IncomingCall"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="caller">Number of the caller</param>
        protected virtual void OnIncomingCall(CellularRadio sender, string caller)
        {
            if (onIncomingCall == null) onIncomingCall = new IncomingCallHandler(IncomingCall);
            if (Program.CheckAndInvoke(IncomingCall, onIncomingCall, sender, caller))
            {
                IncomingCall(sender, caller);
            }
        }
        #endregion

        #region SMS Retrieved
        /// <summary>
        /// Represents the delegate used for the <see cref="SmsRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">SMS message that was requested</param>
        public delegate void SmsRetrievedHandler(CellularRadio sender, Sms message);
        /// <summary>
        /// Event raised when the module detects an incoming call.
        /// </summary>
        public event SmsRetrievedHandler SmsRetrieved;
        private SmsRetrievedHandler onSmsRetrieved;

        /// <summary>
        /// Raises the <see cref="SmsRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="message">SMS message that was requested</param>
        protected virtual void OnSmsRetrieved(CellularRadio sender, Sms message)
        {
            if (onSmsRetrieved == null) onSmsRetrieved = new SmsRetrievedHandler(SmsRetrieved);
            if (Program.CheckAndInvoke(SmsRetrieved, onSmsRetrieved, sender, message))
            {
                SmsRetrieved(sender, message);
            }
        }
        #endregion

        #region Phone Activity Retrieved
        /// <summary>
        /// Represents the delegate used for the <see cref="PhoneActivityRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="activity">Current activity in which the phone is engaged</param>
        public delegate void PhoneActivityRetrievedHandler(CellularRadio sender, PhoneActivityType activity);
        /// <summary>
        /// Event raised when the module receives a phone activity message.
        /// </summary>
        public event PhoneActivityRetrievedHandler PhoneActivityRetrieved;
        private PhoneActivityRetrievedHandler onPhoneActivityRetrieved;

        /// <summary>
        /// Raises the <see cref="PhoneActivityRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="activity">Current activity in which the phone is engaged</param>
        protected virtual void OnPhoneActivityRetrieved(CellularRadio sender, PhoneActivityType activity)
        {
            if (onPhoneActivityRetrieved == null) onPhoneActivityRetrieved = new PhoneActivityRetrievedHandler(PhoneActivityRetrieved);
            if (Program.CheckAndInvoke(PhoneActivityRetrieved, onPhoneActivityRetrieved, sender, activity))
            {
                PhoneActivityRetrieved(sender, activity);
            }
        }
        #endregion

        #region Contact Retrieved
        /// <summary>
        /// Represents the delegate used for the <see cref="ContactRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="contact">Contact object with the requested phonebook entry</param>
        public delegate void ContactOpenRetrieved(CellularRadio sender, Contact contact);
        /// <summary>
        /// Event raised by the <see cref="RetrieveContact"/> method
        /// </summary>
        public event ContactOpenRetrieved ContactRetrieved;
        private ContactOpenRetrieved onContactRetrieved;

        /// <summary>
        /// Raises the <see cref="ContactRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="contact">Contact object with the requested phonebook entry</param>
        protected virtual void OnContactRetrieved(CellularRadio sender, Contact contact)
        {
            if (onContactRetrieved == null) onContactRetrieved = new ContactOpenRetrieved(ContactRetrieved);
            if (Program.CheckAndInvoke(ContactRetrieved, onContactRetrieved, sender, contact))
            {
                ContactRetrieved(sender, contact);
            }
        }
        #endregion

        #region Clock Retrieved
        /// <summary>
        /// Represents the delegate used for the <see cref="ClockRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="clock">Module's date and time</param>
        public delegate void ClockRetrievedHandler(CellularRadio sender, DateTime clock);
        /// <summary>
        /// Event raised when the module receives a phone activity message.
        /// </summary>
        public event ClockRetrievedHandler ClockRetrieved;
        private ClockRetrievedHandler onClockRetrieved;

        /// <summary>
        /// Raises the <see cref="ClockRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="clock">Module's date and time</param>
        protected virtual void OnClockRetrieved(CellularRadio sender, DateTime clock)
        {
            if (onClockRetrieved == null) onClockRetrieved = new ClockRetrievedHandler(ClockRetrieved);
            if (Program.CheckAndInvoke(ClockRetrieved, onClockRetrieved, sender, clock))
            {
                ClockRetrieved(sender, clock);
            }
        }
        #endregion

        #region IMEI Retrieved
        /// <summary>
        /// Represents the delegate used for the <see cref="ImeiRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="imei">Module's International Mobile Equipment Identification number</param>
        public delegate void ImeiRetrievedHandler(CellularRadio sender, string imei);
        /// <summary>
        /// Event raised when the module receives a phone activity message.
        /// </summary>
        public event ImeiRetrievedHandler ImeiRetrieved;
        private ImeiRetrievedHandler onImeiRetrieved;

        /// <summary>
        /// Raises the <see cref="ImeiRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="imei">Module's International Mobile Equipment Identification number</param>
        protected virtual void OnImeiRetrieved(CellularRadio sender, string imei)
        {
            if (onImeiRetrieved == null) onImeiRetrieved = new ImeiRetrievedHandler(ImeiRetrieved);
            if (Program.CheckAndInvoke(ImeiRetrieved, onImeiRetrieved, sender, imei))
            {
                ImeiRetrieved(sender, imei);
            }
        }
        #endregion

        #region Signal Strength Retrieved
        /// <summary>
        /// Represents the delegate used for the <see cref="SignalStrengthRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="signalStrength">Strength of the signal</param>
        public delegate void SignalStrengthRetrievedHandler(CellularRadio sender, SignalStrengthType signalStrength);
        /// <summary>
        /// Event raised when the module receives a phone activity message.
        /// </summary>
        public event SignalStrengthRetrievedHandler SignalStrengthRetrieved;
        private SignalStrengthRetrievedHandler onSignalStrengthRetrieved;

        /// <summary>
        /// Raises the <see cref="SignalStrengthRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="signalStrength">Strength of the signal</param>
        protected virtual void OnSignalStrengthRetrieved(CellularRadio sender, SignalStrengthType signalStrength)
        {
            if (onSignalStrengthRetrieved == null) onSignalStrengthRetrieved = new SignalStrengthRetrievedHandler(SignalStrengthRetrieved);
            if (Program.CheckAndInvoke(SignalStrengthRetrieved, onSignalStrengthRetrieved, sender, signalStrength))
            {
                SignalStrengthRetrieved(sender, signalStrength);
            }
        }
        #endregion

        #region Operator Retrieved
        /// <summary>
        /// Represents the delegate used for the <see cref="OperatorRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="operatorName">Name of the operator to which the module is connected. It is null if the module is not connected to any operator.</param>
        public delegate void OperatorRetrievedHandler(CellularRadio sender, string operatorName);
        /// <summary>
        /// Event raised by the <see cref="RetrieveOperator"/> method.
        /// </summary>
        public event OperatorRetrievedHandler OperatorRetrieved;
        private OperatorRetrievedHandler onOperatorRetrieved;

        /// <summary>
        /// Raises the <see cref="OperatorRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="operatorName">Strength of the signal</param>
        protected virtual void OnOperatorRetrieved(CellularRadio sender, string operatorName)
        {
            if (onOperatorRetrieved == null) onOperatorRetrieved = new OperatorRetrievedHandler(OperatorRetrieved);
            if (Program.CheckAndInvoke(OperatorRetrieved, onOperatorRetrieved, sender, operatorName))
            {
                OperatorRetrieved(sender, operatorName);
            }
        }
        #endregion

        #region SMS List Retrieved
        /// <summary>
        /// Represents the delegate used for the <see cref="SmsListRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="smsList">Strength of the signal</param>
        public delegate void SmsListRetrievedHandler(CellularRadio sender, ArrayList smsList);
        /// <summary>
        /// Event raised when the module receives a phone activity message.
        /// </summary>
        public event SmsListRetrievedHandler SmsListRetrieved;
        private SmsListRetrievedHandler onSmsListRetrieved;

        /// <summary>
        /// Raises the <see cref="SmsListRetrieved"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="smsList">Strength of the signal</param>
        protected virtual void OnSmsListRetrieved(CellularRadio sender, ArrayList smsList)
        {
            if (onSmsListRetrieved == null) onSmsListRetrieved = new SmsListRetrievedHandler(SmsListRetrieved);
            if (Program.CheckAndInvoke(SmsListRetrieved, onSmsListRetrieved, sender, smsList))
            {
                SmsListRetrieved(sender, smsList);
            }
        }
        #endregion

        #region Call Ended
        /// <summary>
        /// Represents the delegate used for the <see cref="CallEnded"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="reason">The reason the call has ended</param>
        public delegate void CallEndedHandler(CellularRadio sender, CallEndType reason);
        /// <summary>
        /// Event raised when the module receives a phone activity message.
        /// </summary>
        public event CallEndedHandler CallEnded;
        private CallEndedHandler onCallEnded;

        /// <summary>
        /// Raises the <see cref="CallEnded"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="reason">The reason the call has ended</param>
        protected virtual void OnCallEnded(CellularRadio sender, CallEndType reason)
        {
            if (onCallEnded == null) onCallEnded = new CallEndedHandler(CallEnded);
            if (Program.CheckAndInvoke(CallEnded, onCallEnded, sender, reason))
            {
                CallEnded(sender, reason);
            }
        }
        #endregion

        #region Call Connected
        /// <summary>
        /// Represents the delegate used for the <see cref="CallConnected"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="number"> Number to which the module is connected</param>
        public delegate void CallConnectedHandler(CellularRadio sender, string number);
        /// <summary>
        /// Event raised when the module receives a phone activity message.
        /// </summary>
        public event CallConnectedHandler CallConnected;
        private CallConnectedHandler onCallConnected;

        /// <summary>
        /// Raises the <see cref="CallConnected"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="number">Number to which the module is connected</param>
        protected virtual void OnCallConnected(CellularRadio sender, string number)
        {
            if (onCallConnected == null) onCallConnected = new CallConnectedHandler(CallConnected);
            if (Program.CheckAndInvoke(CallConnected, onCallConnected, sender, number))
            {
                CallConnected(sender, number);
            }
        }
        #endregion

        #region GPRS Attached
        /// <summary>
        /// Represents the delegate used for the <see cref="GprsAttached"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="ipAddress"> Number to which the module is connected</param>
        public delegate void GprsAttachedHandler(CellularRadio sender, string ipAddress);
        /// <summary>
        /// Event raised when the module receives a phone activity message.
        /// </summary>
        public event GprsAttachedHandler GprsAttached;
        private GprsAttachedHandler onGprsAttached;

        /// <summary>
        /// Raises the <see cref="GprsAttached"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        /// <param name="ipAddress">Number to which the module is connected</param>
        protected virtual void OnGprsAttached(CellularRadio sender, string ipAddress)
        {
            if (onGprsAttached == null) onGprsAttached = new GprsAttachedHandler(GprsAttached);
            if (Program.CheckAndInvoke(GprsAttached, onGprsAttached, sender, ipAddress))
            {
                GprsAttached(sender, ipAddress);
            }
        }
        #endregion

        #region Module Initialized
        /// <summary>
        /// Represents the delegate used for the <see cref="ModuleInitialized"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        public delegate void ModuleInitializedHandler(CellularRadio sender);
        /// <summary>
        /// Event raised when the module receives a phone activity message.
        /// </summary>
        public event ModuleInitializedHandler ModuleInitialized;
        private ModuleInitializedHandler onModuleInitialized;

        /// <summary>
        /// Raises the <see cref="ModuleInitialized"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        protected virtual void OnModuleInitialized(CellularRadio sender)
        {
            if (onModuleInitialized == null) onModuleInitialized = new ModuleInitializedHandler(ModuleInitialized);
            if (Program.CheckAndInvoke(ModuleInitialized, onModuleInitialized, sender))
            {
                ModuleInitialized(sender);
            }
        }
        #endregion

        #endregion

        #region SMS
        /// <summary>
        /// SMS Text Message
        /// </summary>
        public class Sms
        {
            /// <summary>
            /// Number
            /// </summary>
            public string TelephoneNumber;
            /// <summary>
            /// Message content
            /// </summary>
            public string TextMessage;
            /// <summary>
            /// Status of the message
            /// </summary>
            public SmsState Status;
            /// <summary>
            /// Date and time when the message was sent or received
            /// </summary>
            public DateTime Timestamp;
            /// <summary>
            /// Index of the message in the SIM card's memory
            /// </summary>
            public int Index
            {
                get
                {
                    return index;
                }
                internal set
                {
                    index = value;
                }
            }
            private int index;
            /// <summary>
            /// Instantiates a new SMS with empty number, and content, marks it as unsent and with the current time as the timestamp.
            /// </summary>
            public Sms()
            {
                TelephoneNumber = "";
                TextMessage = "";
                Status = SmsState.StoredUnsent;
                Timestamp = DateTime.Now;
                Index = -1;
            }

            /// <summary>
            /// Instantiates a new SMS message with the given parameters.
            /// </summary>
            /// <param name="number">Number</param>
            /// <param name="text">Message content</param>
            /// <param name="state">Status</param>
            /// <param name="timestamp">Timestamp</param>
            public Sms(string number, string text, SmsState state, DateTime timestamp)
            {
                this.TelephoneNumber = number;
                this.TextMessage = text;
                this.Status = state;
                this.Timestamp = timestamp;
            }

            private Sms(string number, string text, SmsState state, DateTime timestamp, int index)
            {
                this.TelephoneNumber = number;
                this.TextMessage = text;
                this.Status = state;
                this.Timestamp = timestamp;
                this.Index = index;
            }

        }

        /// <summary>
        /// Send an SMS text message
        /// </summary>
        /// <param name="number">String with the telephone number</param>
        /// <param name="message">Body of the message</param>
        /// <returns></returns>
        public ReturnedState SendSms(string number, string message)
        {
            // Check if module is on
            if (!isPowerOn) return ReturnedState.ModuleIsOff;

            // Check if serial line is open
            if (serialLine.IsOpen)
            {
                //Check if module is busy
                if (isModuleBusy) return ReturnedState.ModuleBusy;
                isModuleBusy = true;

                serialLine.Write("AT+CMGS=\"+" + number + "\"\r\n");
                Thread.Sleep(100);
                serialLine.Write(message);
                Thread.Sleep(100);
                serialLine.Write((char)26 + "\r");
                //semaphore = true;
                //while (semaphore) Thread.Sleep(10);
                return ReturnedState.OK;
            }
            else return ReturnedState.Error;
        }

        /// <summary>
        /// Requests to read the SMS message in the specified position. Message is returned in the <see cref="SmsRetrieved"/> event.
        /// </summary>
        /// <param name="position">Position in memory where the message is stored</param>
        /// <param name="markAsRead">Whether unread messages will be marked as read</param>
        /// <returns></returns>
        public ReturnedState RetrieveSms(int position, bool markAsRead)
        {
            if (isModuleBusy) return ReturnedState.ModuleBusy;
            requestedMessages.Enqueue(position);
            if (markAsRead)
                return SendATCommand("AT+CMGR=" + position + ",0");
            else
                return SendATCommand("AT+CMGR=" + position + ",1");
        }

        /// <summary>
        /// Delete an SMS message
        /// </summary>
        /// <param name="position">Position in memory where the message is stored</param>
        public ReturnedState DeleteSms(int position)
        {
            return SendATCommand("AT+CMGD=" + position);
        }

        /// <summary>
        /// Requests to get all SMS in a given state or all of them.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public ReturnedState RetrieveSmsList(SmsState state)
        {
            switch (state)
            {
                case SmsState.All:
                    return SendATCommand("AT+CMGL=\"ALL\"");
                case SmsState.ReceivedRead:
                    return SendATCommand("AT+CMGL=\"REC READ\"");
                case SmsState.ReceivedUnread:
                    return SendATCommand("AT+CMGL=\"REC UNREAD\"");
                case SmsState.StoredSent:
                    return SendATCommand("AT+CMGL=\"STO SENT\"");
                case SmsState.StoredUnsent:
                    return SendATCommand("AT+CMGL=\"STO UNSENT\"");
                default:
                    return ReturnedState.InvalidCommand;
            }

        }

        /// <summary>
        /// Deletes all SMS messages stored in the SIM card
        /// </summary>
        /// <returns></returns>
        public ReturnedState DeleteAllSms()
        {
            return SendATCommand("AT+CMGD=0,4");
        }
        #endregion

        #region VOICE CALLS
        /// <summary>
        /// Picks up an incoming voice call
        /// </summary>
        /// <returns></returns>
        public ReturnedState PickUp()
        {
            return SendATCommand("ATA");
        }

        /// <summary>
        /// Hangs up an active call
        /// </summary>
        /// <returns></returns>
        public ReturnedState HangUp()
        {
            return SendATCommand("ATH");
        }

        /// <summary>
        /// Dials a number in order to start a voice call
        /// </summary>
        /// <param name="number">Number to be called</param>
        /// <returns></returns>
        public ReturnedState Dial(string number)
        {
            ReturnedState ret;
            if (number.IndexOf("+") >= 0)
                ret = SendATCommand("ATD" + number + ";");
            else
                ret = SendATCommand("ATD+" + number + ";");
            if (ret == ReturnedState.OK)
            {
                isModuleBusy = true;
            }
            return ret;
        }

        /// <summary>
        /// Redials the last number dialled
        /// </summary>
        /// <returns></returns>
        public ReturnedState Redial()
        {
            return SendATCommand("ATDL");
        }

        /// <summary>
        /// Raises the <see cref="PhoneActivityRetrieved"/> event, which contains the activity in which the phone is currently engaged.
        /// </summary>
        /// <returns></returns>
        public ReturnedState RetrievePhoneActivity()
        {
            return SendATCommand("AT+CPAS");
        }
        #endregion

        #region PHONEBOOK
        /// <summary>
        /// Entry of the phonebook
        /// </summary>
        public class Contact
        {
            /// <summary>
            /// String with the telephone number
            /// </summary>
            public string telephoneNumber;
            /// <summary>
            /// Name of the entry
            /// </summary>
            public string name;
            /// <summary>
            /// Instantiates a Phonebook entry from a number and a name
            /// </summary>
            /// <param name="telephoneNumber"></param>
            /// <param name="name"></param>
            public Contact(string telephoneNumber, string name)
            {
                this.telephoneNumber = telephoneNumber;
                this.name = name;
            }
        }

        /// <summary>
        /// Adds an entry to the SIM card phonebook
        /// </summary>
        /// <param name="number">Telephone number</param>
        /// <param name="name">Name</param>
        public ReturnedState SaveContact(string number, string name)
        {
            return SendATCommand("AT +CPBW=,\"" + number + "\", ,\"" + name + "\"");
        }

        /// <summary>
        /// Adds an entry to the SIM card phonebook
        /// </summary>
        /// <param name="contact">Contact object containing the number and name</param>
        public ReturnedState SaveContact(Contact contact)
        {
            if (contact == null)
                return ReturnedState.Error;
            else
                return SaveContact(contact.telephoneNumber, contact.name);
        }

        /// <summary>
        /// Adds an entry to the SIM card phonebook
        /// </summary>
        /// <param name="index">Index of the entry where the contact is going to be stored</param>
        /// <param name="number">Number</param>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public ReturnedState SaveContact(int index, string number, string name)
        {
            return SendATCommand("AT+CPBW=" + index + ",\"" + number + "\", ,\"" + name + "\"");
        }

        /// <summary>
        /// Adds an entry to the SIM card phonebook
        /// </summary>
        /// <param name="index">Index of the entry where the contact is going to be stored</param>
        /// <param name="contact">Contact object containing the number and name</param>
        /// <returns></returns>
        public ReturnedState SaveContact(int index, Contact contact)
        {
            if (contact == null)
                return ReturnedState.Error;
            else
                return SaveContact(index, contact.telephoneNumber, contact.name);
        }

        /// <summary>
        /// Raises the <see cref="ContactRetrieved"/> event, which contains the contact stored in the specified position.
        /// </summary>
        /// <param name="index">Index of the phonebook entry where the contact is stored</param>
        /// <returns></returns>
        public ReturnedState RetrieveContact(int index)
        {
            return SendATCommand("AT+CPBR=" + index);
        }

        /// <summary>
        /// Delete the contact at the specified position
        /// </summary>
        /// <param name="index">Index of the phonebook entry where the contact is stored</param>
        /// <returns></returns>
        public ReturnedState DeleteContact(int index)
        {
            return SendATCommand("AT+CPBW=" + index);
        }
        #endregion

        #region TIME
        /// <summary>
        /// Sends a request to get the module's date and time
        /// </summary>
        /// <returns></returns>
        public ReturnedState RetrieveClock()
        {
            return SendATCommand("AT+CCLK?");
        }

        /// <summary>
        /// Sets the module's internal date and time
        /// </summary>
        /// <param name="clock"></param>
        /// <returns></returns>
        public ReturnedState SetClock(DateTime clock)
        {
            // Convert dateTime to format: +CCLK: “YY/MM/DD,HH:MM:SS+02”
            //string ret = SendATCommandAndGetResponse("AT+CCLK=\"" + dateTime.ToString("YY") +  + "\"");
            string command;
            command = "AT+CCLK=\"" +
                clock.ToString("yy") + "/" +
                clock.ToString("MM") + "/" +
                clock.ToString("dd") + "," +
                clock.ToString("HH:mm:ss") + "+00\"";
            return SendATCommand(command);
        }
        #endregion

        #region GENERAL ENQUIRIES
        /// <summary>
        /// Raises the <see cref="ImeiRetrieved"/> event which contains the module international mobile equipment identification number.
        /// </summary>
        /// <returns></returns>
        public ReturnedState RetrieveImei()
        {
            return SendATCommand("AT+GSN");
        }

        /// <summary>
        /// Resets module
        /// </summary>
        public void Reset()
        {
            PowerOff();
            PowerOn(2);
        }


        /// <summary>
        /// Raises the <see cref="SignalStrengthRetrieved"/> event which contains the strength of the signal.
        /// </summary>
        /// <returns></returns>
        public ReturnedState RetrieveSignalStrength()
        {
            return SendATCommand("AT+CSQ");
        }

        /// <summary>
        /// Raises the <see cref="OperatorRetrieved"/> event which contains the name of the operator, if the module is connected to a network.
        /// </summary>
        /// <returns></returns>
        public ReturnedState RetrieveOperator()
        {
            return SendATCommand("AT+COPS?");
        }

        /// <summary>
        /// Raises the <see cref="PinStateRetrieved"/> event which contains the current state of the PIN.
        /// </summary>
        /// <returns></returns>
        public ReturnedState RetrievePinState()
        {
            return SendATCommand("AT+CPIN?");
        }
        #endregion

        #region GPRS

        /// <summary>
        /// Attach GPRS
        /// </summary>
        /// <param name="accessPointName"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public ReturnedState AttachGPRS(string accessPointName, string username, string password)
        {
            if (isModuleBusy) return ReturnedState.ModuleBusy;
            SendATCommand("AT+CSTT=\"" + accessPointName + "\",\"" + username + "\",\"" + password + "\"");

            SendATCommand("AT+CIICR");
            Thread.Sleep(3000);

            return SendATCommand("AT+CIFSR");
        }

        /// <summary>
        /// Detach GPRS
        /// </summary>
        /// <returns></returns>
        public ReturnedState DetachGprs()
        {
            return SendATCommand("AT+CGATT=0");
        }

        /// <summary>
        /// Connects to a TCP server
        /// </summary>
        /// <param name="server">IP address of the server</param>
        /// <param name="port">Port in the server</param>
        /// <returns></returns>
        public ReturnedState ConnectTCP(string server, int port)
        {
            return SendATCommand("AT+CIPSTART=\"TCP\",\"" + server + "\"," + port);
        }

        /// <summary>
        /// Disconnects from TCP server
        /// </summary>
        /// <returns></returns>
        public ReturnedState DisconnectTCP()
        {
            return SendATCommand("AT+CIPCLOSE");
        }

        /// <summary>
        /// Configure the module as a TCP server
        /// </summary>
        /// <param name="port">Port</param>
        /// <returns></returns>
        public ReturnedState ConfigureTCPServer(int port)
        {
            return SendATCommand("AT+CIPSERVER=1," + port);
        }

        /// <summary>
        /// Sends data over a TCP connection
        /// </summary>
        /// <param name="data">Data to be sent</param>
        /// <returns></returns>
        public ReturnedState SendTcpData(string data)
        {
            SendATCommand("AT+CIPSEND");
            Thread.Sleep(1000);
            return SendATCommand(data + (char)26);

        }
        #endregion
    }
}
