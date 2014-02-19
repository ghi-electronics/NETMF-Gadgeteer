//#define WAITING_FOR_ASSEMBLIES
using GTM = Gadgeteer.Modules;

using GHI.Premium.Hardware;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2C), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

#if WAITING_FOR_ASSEMBLIES
    public class CANDW : GTM.Module
    {
    }
}
#else
    /// <summary>
    /// A CANDW module for Microsoft .NET Gadgeteer
    /// </summary>
    public class CANDW : GTM.Module
    {
        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public CANDW(int socketNumber)
        {
            // This finds the Socket instance from the user-specified socket number.  
            // This will generate user-friendly error messages if the socket is invalid.
            // If there is more than one socket on this module, then instead of "null" for the last parameter, 
            // put text that identifies the socket to the user (e.g. "S" if there is a socket type S)
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('C', this);
        }

        void m_CAN_ErrorReceivedEvent(CAN sender, CANErrorReceivedEventArgs args)
        {
            //this._PostDone = new PostMessagesDoneEventHandler(this.OnPostMessagesFinished);
            //this._PostDone(m_numMessagesSent);
            this._ErrorReceived = new ErrorReceivedEventHandler(this.OnErrorReceived);
            this._ErrorReceived(sender, args);
        }

        /// <summary>
        /// Event delegate
        /// </summary>
        /// <param name="sender">Sending module</param>
        /// <param name="args">Error args</param>
        public delegate void ErrorReceivedEventHandler(CAN sender, CANErrorReceivedEventArgs args);
        
        /// <summary>
        /// Event for when an error is received
        /// </summary>
        public event ErrorReceivedEventHandler ErrorReceived;
        
        private ErrorReceivedEventHandler _ErrorReceived;

        /// <summary>
        /// Sends the error event.
        /// </summary>
        /// <param name="sender">Sending module</param>
        /// <param name="args">Error arguments</param>
        protected virtual void OnErrorReceived(CAN sender, CANErrorReceivedEventArgs args)
        {
            this.ErrorReceived(sender, args);
        }

        void m_CAN_DataReceivedEvent(CAN sender, CANDataReceivedEventArgs args)
        {
            this._DataReceived = new DataReceivedEventHandler(this.OnDataReceived);
            this._DataReceived(sender, args);
        }

        /// <summary>
        /// Event delegate
        /// </summary>
        /// <param name="sender">Sending module</param>
        /// <param name="args">Data args</param>
        public delegate void DataReceivedEventHandler(CAN sender, CANDataReceivedEventArgs args);
        
        /// <summary>
        /// Event for when data is received
        /// </summary>
        public event DataReceivedEventHandler DataReceived;
        
        private DataReceivedEventHandler _DataReceived;

        /// <summary>
        /// Sends the data received event
        /// </summary>
        /// <param name="sender">Sending module</param>
        /// <param name="args">Data args</param>
        protected virtual void OnDataReceived(CAN sender, CANDataReceivedEventArgs args)
        {
            if (this.DataReceived == null)
                this.DataReceived = new DataReceivedEventHandler(this.OnDataReceived);
            this.DataReceived(sender, args);
        }

        private CAN m_CAN;

        //public CAN GetCAN
        //{
        //    get { return m_CAN; }
        //}

        /// <summary>
        /// Initializes CAN.
        /// </summary>
        /// <param name="bitRate">The desired bitrate, if known. Otherwise, use the other overload to calculate a bitrate. See http://wiki.tinyclr.com/index.php?title=CAN for more info.</param>
        /// <param name="receiveBufferSize">Specifies the receive buffer size (number of internally buffered CAN messages). Defaulted to 100.</param>
        public void InitializeCAN(uint bitRate, int receiveBufferSize)
        {
            m_CAN = new CAN(CAN.Channel.Channel_1, bitRate, receiveBufferSize = 100);

            m_CAN.DataReceivedEvent += new CANDataReceivedEventHandler(m_CAN_DataReceivedEvent);
            m_CAN.ErrorReceivedEvent += new CANErrorReceivedEventHandler(m_CAN_ErrorReceivedEvent);
        }

        /// <summary>
        /// Initializes CAN.
        /// </summary>
        /// <param name="T1">See http://wiki.tinyclr.com/index.php?title=CAN for calculation.</param>
        /// <param name="T2">See http://wiki.tinyclr.com/index.php?title=CAN for calculation.</param>
        /// <param name="BRP">See http://wiki.tinyclr.com/index.php?title=CAN for calculation.</param>
        /// <param name="receiveBufferSize">Specifies the receive buffer size (number of internally buffered CAN messages). Defaulted to 100.</param>
        public void InitializeCAN(int T1, int T2, int BRP, int receiveBufferSize = 100)
        {
            m_CAN = new CAN(CAN.Channel.Channel_1, (uint)(((T2 - 1) << 20) | ((T1 - 1) << 16) | ((BRP - 1) << 0)), receiveBufferSize);

            m_CAN.DataReceivedEvent += new CANDataReceivedEventHandler(m_CAN_DataReceivedEvent);
            m_CAN.ErrorReceivedEvent += new CANErrorReceivedEventHandler(m_CAN_ErrorReceivedEvent);
        }

        private System.Threading.Thread m_PostMessagesThread;
        
        /// <summary>
        /// The list of messages to be sent when calling PostMessages.
        /// </summary>
        public CAN.Message[] msgList;

        /// <summary>
        ///  Posts (queues for writing) CAN messages
        /// </summary>
        /// <param name="offset">Offset into the buffer to start sending from.</param>
        /// <param name="count">Number of messages to write.</param>
        /// <returns></returns>
        public CAN_PostState PostMessages(int offset, int count)
        {
            if (m_PostMessagesThread != null)
            {
                if (m_PostMessagesThread.IsAlive)
                    return CAN_PostState.Fail;
            }

            m_PostMessagesThread = new System.Threading.Thread(this._PostMessagesThread);
            m_PostMessagesThread.Start();

            return CAN_PostState.Success;
        }

        private int m_numMessagesSent = 0;

        /// <summary>
        /// The number of messages sent during the last PostMessages call.
        /// </summary>
        public int NumMessagesSent
        {
            get { return m_numMessagesSent; }
        }

        private void _PostMessagesThread()
        {
            m_numMessagesSent = 0;

            while (true)
            {
                m_numMessagesSent += m_CAN.PostMessages(msgList, m_numMessagesSent, msgList.Length - m_numMessagesSent);

                if (m_numMessagesSent == msgList.Length)
                    break;

                System.Threading.Thread.Sleep(1);
            }

            //send "im done" event here
            //if( this._PostDone == null)
                this._PostDone = new PostMessagesDoneEventHandler(this.OnPostMessagesFinished);
            this._PostDone(m_numMessagesSent);
        }

        /// <summary>
        /// Event delegate
        /// </summary>
        /// <param name="numPosted">Number of messages posted</param>
        public delegate void PostMessagesDoneEventHandler(int numPosted);
        
        /// <summary>
        /// Event for when messages have been successfully sent
        /// </summary>
        public event PostMessagesDoneEventHandler PostDone;

        private PostMessagesDoneEventHandler _PostDone;

        /// <summary>
        /// Sends the event when messages are successfully sent
        /// </summary>
        /// <param name="numPosted">number of messages posted</param>
        protected virtual void OnPostMessagesFinished(int numPosted)
        {
            this.PostDone(m_numMessagesSent);
        }

        /// <summary>
        /// Represents the state of the <see cref="CANDW"/> while attempting to post messages.
        /// </summary>
        public enum CAN_PostState
        {
            /// <summary>
            /// Failed to write messages, as it is still busy writing.
            /// </summary>
            Fail = -1,
            /// <summary>
            /// Successfully began writing messages.
            /// </summary>
            Success = 1
        }

        /// <summary>
        /// A boolean value denoting if all posted (queued for writing) messages are sent.
        /// </summary>
        /// <returns>A boolean value denoting if all posted (queued for writing) messages are sent.</returns>
        public bool GetPostedMessagesSent()
        {
            return m_CAN.PostedMessagesSent;
        }

        /// <summary>
        /// The number of messages ready to be read.
        /// </summary>
        /// <returns>The number of messages ready to be read.</returns>
        public int GetReceivedMessagesCount()
        {
            return m_CAN.ReceivedMessagesCount;
        }

        /// <summary>
        /// CAN receive error count.
        /// </summary>
        /// <returns>CAN receive error count.</returns>
        public int GetReceiveErrorCount()
        {
            return m_CAN.ReceiveErrorCount;
        }

        /// <summary>
        ///  CAN transmit error count.
        /// </summary>
        /// <returns> CAN transmit error count.</returns>
        public int GetTransmitErrorCount()
        {
            return m_CAN.TransmitErrorCount;
        }

        /// <summary>
        /// Resets the CAN controller
        /// </summary>
        /// <remarks>
        /// This methods resets the CAN controller. This is needed in the Bus Off condition
        ///     because the controller get disabled automatically.  Note that a reset causes
        ///     the hardware buffered messages to be lost (On EMX and USBizi, this is 2 for
        ///     receive and 3 for transmit). The software receive buffer is not affected.
        ///     The software filters are not affected either.
        /// </remarks>
        public void Reset()
        {
            m_CAN.Reset();
        }

#region Filters
        /// <summary>
        /// Sets explicit filters.
        /// </summary>
        /// <param name="filters">Messages' IDs to filter.</param>
        /// <remarks>
        /// This filters exact matches for message identifiers (standard or extended).
        ///     The provided filters will be copied internally and searched using an optimized
        ///     software search.  For example, to only receive messages with these IDs (0x1234,
        ///     0x5789, 0x12345678), do the following: uint[] explicitIDs = new uint[] {0x1234,
        ///     0x5789, 0x12345678}; can.SetExplicitFilters(explicitIDs);
        /// </remarks>
        public void SetExplicitFilters(uint[] filters)
        {
            m_CAN.SetExplicitFilters(filters);
        }

        /// <summary>
        /// Sets group (range) filters.
        /// </summary>
        /// <param name="lowerBounds">Group lower bounds for messages' IDs to filter. Each lower bound corresponds to an upper bound.</param>
        /// <param name="upperBounds">Group upper bounds for messages' IDs to filter. Each upper bound corresponds to a lower bound.</param>
        /// <remarks>
        /// This filters a group (range) of message identifiers (standard or extended).
        ///     The provided filters will be copied internally and searched using an optimized
        ///     software search.  Every pair of a lower bound at index i and an upper bound
        ///     at the same index defines a valid group. The provided groups MUST not overlap.
        ///     Otherwise, the method will throw an argument exception.  For example, to
        ///     only receive messages with these two groups of IDs [0x1200 to 0x1248] and
        ///     [0x500 to 0x1000], do the following: uint[] lowerBounds = new uint[] { 0x1200,
        ///     0x500 }; uint[] upperBounds = new uint[] { 0x1248, 0x1000 }; can.SetGroupFilters(lowerBounds,
        ///     upperBounds); Note that the bounds' limits are considered valid. In the above
        ///     example, 0x1200, 0x1248, 0x500 and 0x1000 are valid IDs and will pass through
        ///     to the receive buffer.
        /// </remarks>
        public void SetGroupFilters(uint[] lowerBounds, uint[] upperBounds)
        {
            m_CAN.SetGroupFilters(lowerBounds, upperBounds);
        }

        /// <summary>
        /// Disables the explicit filters.
        /// </summary>
        public void DisableExplicitFilters()
        {
            m_CAN.DisableExplicitFilters();
        }

        /// <summary>
        /// Disables the group filters.
        /// </summary>
        public void DisableGroupFilters()
        {
            m_CAN.DisableGroupFilters();
        }
        #endregion Filters
    }
}
#endif
