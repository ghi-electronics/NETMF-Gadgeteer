using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// An IRReceiver module for Microsoft .NET Gadgeteer
    /// </summary>
    public class IRReceiver : GTM.Module
    {
        private long lastTick;
        private long bitTime;
        private uint pattern;
        private bool streaming;
        private uint shiftBit;
        private bool newPress;
        private InterruptPort input;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public IRReceiver(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('X', this);

            this.newPress = false;
            this.lastTick = DateTime.Now.Ticks;

            this.input = new InterruptPort(socket.CpuPins[3], false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
            this.input.OnInterrupt += OnInterrupt;
        }

        private void OnInterrupt(uint data1, uint data2, DateTime time)
        {
            this.bitTime = time.Ticks - lastTick;
            this.lastTick = time.Ticks;

            //Testing showed that the togglebit didn't work reliably with quick actions, so we test for a bit timeout of 100ms giving better results.
            if (this.bitTime > 1000000)
                this.newPress = true;

            if (this.bitTime > 26670) //3 * halftime (half_bittime = 889 us)
            {
                this.bitTime = 0;
                this.pattern = 0;

                if (data2 == 0)
                {
                    this.streaming = true;
                    this.shiftBit = 1;
                    this.pattern |= this.shiftBit;
                }
                else
                {
                    this.streaming = false;
                }

                return;
            }

            if (this.streaming)
            {
                if (this.bitTime > 10668) //half_bittime * 1.2 (half_bittime = 889 us)
                {
                    this.shiftBit = data2 == 0 ? 1U : 0U;
                    this.pattern <<= 1;
                    this.pattern |= this.shiftBit;
                }
                else
                {
                    if (data2 == 0)
                    {
                        this.pattern <<= 1;
                        this.pattern |= this.shiftBit;
                    }
                }

                if ((this.pattern & 0x2000) > 0) //14 bits
                {
                    if (this.newPress)
                    {
                        this.OnSignalReceived(this, new SignalReceivedEventArgs(pattern & 0x3F));
                        this.newPress = false;
                    }

                    this.pattern = 0;
                    this.bitTime = 0;
                    this.streaming = false;
                }
            }
        }

        /// <summary>
        /// Event arguments for the signal received event.
        /// </summary>
        public class SignalReceivedEventArgs : EventArgs
        {
            /// <summary>
            /// The button that was pressed.
            /// </summary>
            public uint Button { get; private set; }

            /// <summary>
            /// The time that the button was read.
            /// </summary>
            public DateTime ReadTime { get; private set; }

            internal SignalReceivedEventArgs(uint button)
            {
                this.Button = button;
                this.ReadTime = DateTime.Now;
            }
        }

        /// <summary>
        /// The delegate that is used to handle the IR event.
        /// </summary>
        /// <param name="sender">The <see cref="IRReceiver"/> object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void SignalReceivedEventHandler(IRReceiver sender, SignalReceivedEventArgs e);

        /// <summary>
        /// Raised when the module detects an IR signal.
        /// </summary>
        public event SignalReceivedEventHandler SignalReceived;

        private SignalReceivedEventHandler onSignalReceived;

        private void OnSignalReceived(IRReceiver sender, SignalReceivedEventArgs e)
        {
            if (this.onSignalReceived == null)
                this.onSignalReceived = this.OnSignalReceived;

            if (Program.CheckAndInvoke(this.SignalReceived, this.onSignalReceived, sender, e))
                this.SignalReceived(sender, e);
        }
    }
}