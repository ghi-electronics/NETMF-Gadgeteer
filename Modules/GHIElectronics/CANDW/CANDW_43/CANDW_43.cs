using GHI.IO;
using Microsoft.SPOT;
using System;
using System.Threading;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A CANDW module for Microsoft .NET Gadgeteer
    /// </summary>
    public class CANDW : GTM.Module
    {
        private ControllerAreaNetwork can;
        private Thread workerThread;
        private object syncRoot;
        private int sent;
        private bool running;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public CANDW(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('C', this);

            this.syncRoot = new object();
            this.running = false;
            this.sent = 0;
            this.can = null;
            this.workerThread = null;
        }

        /// <summary>
        /// The underlying ControllerAreaNetwork object.
        /// </summary>
        public ControllerAreaNetwork Can
        {
            get
            {
                if (this.can == null) throw new InvalidOperationException("You must call Initialize first.");

                return this.can;
            }
        }

        /// <summary>
        /// Initializes the CAN bus.
        /// </summary>
        /// <param name="speed">The desired bus speed.</param>
        public void Initialize(ControllerAreaNetwork.Speed speed)
        {
            this.Initialize(speed, ControllerAreaNetwork.Channel.One);
        }

        /// <summary>
        /// Initializes the CAN bus.
        /// </summary>
        /// <param name="timings">The desired bus timings.</param>
        public void Initialize(ControllerAreaNetwork.Timings timings)
        {
            this.Initialize(timings, ControllerAreaNetwork.Channel.One);
        }

        /// <summary>
        /// Initializes the CAN bus.
        /// </summary>
        /// <param name="speed">The desired bus speed.</param>
        /// <param name="channel">The CAN channel to use.</param>
        public void Initialize(ControllerAreaNetwork.Speed speed, ControllerAreaNetwork.Channel channel)
        {
            this.can = new ControllerAreaNetwork(channel, speed);

            this.can.MessageAvailable += this.OnCanMessagesAvailable;
            this.can.ErrorReceived += this.OnCanErrorReceived;

            this.can.Enabled = true;
        }

        /// <summary>
        /// Initializes the CAN bus.
        /// </summary>
        /// <param name="timings">The desired bus timings.</param>
        /// <param name="channel">The CAN channel to use.</param>
        public void Initialize(ControllerAreaNetwork.Timings timings, ControllerAreaNetwork.Channel channel)
        {
            this.can = new ControllerAreaNetwork(channel, timings);

            this.can.MessageAvailable += this.OnCanMessagesAvailable;
            this.can.ErrorReceived += this.OnCanErrorReceived;

            this.can.Enabled = true;
        }

        /// <summary>
        /// Sends the given message over the CAN bus.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendMessage(ControllerAreaNetwork.Message message)
        {
            if (message == null) throw new ArgumentNullException("message");

            this.SendMessages(new ControllerAreaNetwork.Message[] { message });
        }

        /// <summary>
        /// Sends the given messages over the CAN bus.
        /// </summary>
        /// <param name="messages">The messages to send.</param>
        public void SendMessages(ControllerAreaNetwork.Message[] messages)
        {
            if (messages == null) throw new ArgumentNullException("messages");

            this.SendMessages(messages, 0, messages.Length);
        }

        /// <summary>
        /// Sends the given messages over the CAN bus.
        /// </summary>
        /// <param name="messages">The messages to send.</param>
        /// <param name="offset">Offset into the buffer to start sending from.</param>
        /// <param name="count">Number of messages to send.</param>
        public void SendMessages(ControllerAreaNetwork.Message[] messages, int offset, int count)
        {
            lock (this.syncRoot)
            {
                if (messages == null) throw new ArgumentNullException("messages");
                if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must not be negative.");
                if (count <= 0) throw new ArgumentOutOfRangeException("offset", "offset must be positive.");
                if (count + offset > messages.Length) throw new ArgumentOutOfRangeException("messages", "messages.Length must be at least offset + count.");
                if (this.workerThread != null && this.workerThread.IsAlive) throw new InvalidOperationException("You must wait until all other messages have finished sending.");
                if (this.can == null) throw new InvalidOperationException("You must call Initialize first.");

                this.running = true;
                this.workerThread = new Thread(() => this.DoWork(messages, offset, count));
                this.workerThread.Start();
            }
        }

        /// <summary>
        /// Cancels sending the messages queued by SendMessages.
        /// </summary>
        /// <returns>The number of messages sent so far.</returns>
        public int CancelSend()
        {
            lock (this.syncRoot)
            {
                if (this.can == null) throw new InvalidOperationException("You must call Initialize first.");
                if (this.workerThread == null || !this.workerThread.IsAlive) throw new InvalidOperationException("There is no send operation in progress.");

                this.running = false;
                this.workerThread.Join();

                return this.sent;
            }
        }

        /// <summary>
        /// Whether or not transmission is currently possible.
        /// </summary>
        public bool CanSend
        {
            get
            {
                return this.can.CanSend;
            }
        }

        /// <summary>
        /// Whether or not the transmit buffer is empty.
        /// </summary>
        public bool IsTransmitBufferEmpty
        {
            get
            {
                if (this.can == null) throw new InvalidOperationException("You must call Initialize first.");

                return this.can.IsTransmitBufferEmpty;
            }
        }

        /// <summary>
        /// The number of received messages available.
        /// </summary>
        public int AvailableMessages
        {
            get
            {
                if (this.can == null) throw new InvalidOperationException("You must call Initialize first.");

                return this.can.AvailableMessages;
            }
        }

        /// <summary>
        /// The number of receive errors encountered.
        /// </summary>
        public int ReceiveErrorCount
        {
            get
            {
                if (this.can == null) throw new InvalidOperationException("You must call Initialize first.");

                return this.can.ReceiveErrorCount;
            }
        }

        /// <summary>
        /// The number of transmit errors encountered.
        /// </summary>
        public int TransmitErrorCount
        {
            get
            {
                if (this.can == null) throw new InvalidOperationException("You must call Initialize first.");

                return this.can.TransmitErrorCount;
            }
        }

        /// <summary>
        /// Resets the CAN controller.
        /// </summary>
        /// <remarks>
        /// All hardware buffered messages will be lost. The software receive buffer is not affected.
        /// </remarks>
        public void Reset()
        {
            if (this.can == null) throw new InvalidOperationException("You must call Initialize first.");

            this.can.Reset();
        }

        /// <summary>
        /// Sets the explicit filters.
        /// </summary>
        /// <param name="filters">The message ids to filter.</param>
        /// <remarks>
        /// Any id not matching one of the filters is discarded. Pass null to disable the filter.
        /// </remarks>
        public void SetExplicitFilters(uint[] filters)
        {
            if (this.can == null) throw new InvalidOperationException("You must call Initialize first.");

            this.can.SetExplicitFilters(filters);
        }

        /// <summary>
        /// Sets group filters.
        /// </summary>
        /// <param name="lowerBounds">The lower bounds to filter</param>
        /// <param name="upperBounds">The upper bounds to filter</param>
        /// <remarks>
        /// Each entry in lowerBounds corresponds to the same-indexed entry in upperBounds. The bounds are inclusive, that is the provided bounds are valid ids.
        /// For example, to only receive messages with these two groups of IDs [0x1200 to 0x1248] and [0x500 to 0x1000], do the following:<br/>
        /// </remarks>
        public void SetGroupFilters(uint[] lowerBounds, uint[] upperBounds)
        {
            if (this.can == null) throw new InvalidOperationException("You must call Initialize first.");

            this.can.SetGroupFilters(lowerBounds, upperBounds);
        }

        private void DoWork(ControllerAreaNetwork.Message[] messages, int offset, int count)
        {
            this.sent = 0;

            while (this.running && this.sent < count)
            {
                this.sent += this.can.SendMessages(messages, this.sent + offset, messages.Length - this.sent);

                Thread.Sleep(1);
            }

            this.running = false;

            this.OnMessagesSent(this, null);
        }

        /// <summary>
        /// Event arguments for the MessagesReceived event.
        /// </summary>
        public class MessagesReceivedEventArgs : EventArgs
        {
            private ControllerAreaNetwork.Message[] messages;

            /// <summary>
            /// The messages received.
            /// </summary>
            public ControllerAreaNetwork.Message[] Messages { get { return this.messages; } }

            /// <summary>
            /// The number of messages received.
            /// </summary>
            public int MessageCount { get; private set; }

            internal MessagesReceivedEventArgs(ControllerAreaNetwork.Message[] messages)
            {
                this.messages = messages;
                this.MessageCount = messages.Length;
            }
        }

        /// <summary>
        /// Event arguments for the ErrorReceived event.
        /// </summary>
        public class ErrorReceivedEventArgs : EventArgs
        {
            /// <summary>
            /// The error received.
            /// </summary>
            public ControllerAreaNetwork.Error Error { get; private set; }

            internal ErrorReceivedEventArgs(ControllerAreaNetwork.Error error)
            {
                this.Error = error;
            }
        }

        /// <summary>
        /// The delegate that is used to handle the data received event.
        /// </summary>
        /// <param name="sender">The <see cref="CANDW"/> object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void MessagesReceivedEventHandler(CANDW sender, MessagesReceivedEventArgs e);

        /// <summary>
        /// The delegate that is used to handle the error received event.
        /// </summary>
        /// <param name="sender">The <see cref="CANDW"/> object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void ErrorReceivedEventHandler(CANDW sender, ErrorReceivedEventArgs e);

        /// <summary>
        /// The delegate that is used to handle the posted messages done event.
        /// </summary>
        /// <param name="sender">The <see cref="CANDW"/> object that raised the event.</param>
        /// <param name="e">The number of messages posted.</param>
        public delegate void MessagesSentEventHandler(CANDW sender, EventArgs e);

        /// <summary>
        /// Raised when data is received.
        /// </summary>
        public event MessagesReceivedEventHandler MessagesReceived;

        /// <summary>
        /// Raised when an error is received.
        /// </summary>
        public event ErrorReceivedEventHandler ErrorReceived;

        /// <summary>
        /// Raised when the messages have been successfully sent.
        /// </summary>
        public event MessagesSentEventHandler MessagesSent;

        private MessagesReceivedEventHandler onMessagesReceived;
        private ErrorReceivedEventHandler onErrorReceived;
        private MessagesSentEventHandler onMessagesSent;

        private void OnMessagesReceived(CANDW sender, MessagesReceivedEventArgs e)
        {
            if (this.onMessagesReceived == null)
                this.onMessagesReceived = this.OnMessagesReceived;

            if (Program.CheckAndInvoke(this.MessagesReceived, this.onMessagesReceived, sender, e))
                this.MessagesReceived(sender, e);
        }

        private void OnErrorReceived(CANDW sender, ErrorReceivedEventArgs e)
        {
            if (this.onErrorReceived == null)
                this.onErrorReceived = this.OnErrorReceived;

            if (Program.CheckAndInvoke(this.ErrorReceived, this.onErrorReceived, sender, e))
                this.ErrorReceived(sender, e);
        }

        private void OnMessagesSent(CANDW sender, EventArgs e)
        {
            if (this.onMessagesSent == null)
                this.onMessagesSent = this.OnMessagesSent;

            if (Program.CheckAndInvoke(this.MessagesSent, this.onMessagesSent, sender, e))
                this.MessagesSent(sender, e);
        }

        private void OnCanMessagesAvailable(ControllerAreaNetwork sender, ControllerAreaNetwork.MessageAvailableEventArgs e)
        {
            this.OnMessagesReceived(this, new MessagesReceivedEventArgs(this.can.ReadMessages()));
        }

        private void OnCanErrorReceived(ControllerAreaNetwork sender, ControllerAreaNetwork.ErrorReceivedEventArgs e)
        {
            this.OnErrorReceived(this, new ErrorReceivedEventArgs(e.Error));
        }
    }
}
