using Microsoft.SPOT;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A PulseOximeter module for Microsoft .NET Gadgeteer
    /// </summary>
    public class PulseOximeter : GTM.Module
    {
        private Thread workerThread;
        private GTI.Serial serialPort;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public PulseOximeter(int socketNumber)
        {
            this.IsProbeAttached = false;
            this.LastReading = null;

            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            this.serialPort = GTI.SerialFactory.Create(socket, 4800, GTI.SerialParity.Even, GTI.SerialStopBits.One, 8, GTI.HardwareFlowControl.NotRequired, this);
            this.serialPort.Open();

            this.workerThread = new Thread(this.DoWork);
            this.workerThread.Start();
        }

        private void DoWork()
        {
            bool sync = false;
            byte[] data = new byte[5];

            while (true)
            {
                int totalRead = 0;
                if (!sync)
                {
                    int b = this.serialPort.ReadByte();
                    if (b < 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    if (((b >> 7) & 0x1) != 1)
                        continue;

                    data[0] = (byte)b;
                    totalRead = 1;
                    sync = true;
                }

                while (totalRead < 5)
                {
                    int read = this.serialPort.Read(data, totalRead, 5 - totalRead);
                    if (read < 0)
                    {
                        this.DebugPrint("Serial error");
                        sync = false;

                        if (this.IsProbeAttached)
                        {
                            this.IsProbeAttached = false;

                            this.OnProbeDetached(this, null);
                        }

                        continue;
                    }

                    totalRead += read;
                }

                if (((data[0] >> 7) & 0x1) != 1)
                {
                    this.DebugPrint("Lost sync");
                    sync = false;

                    if (this.IsProbeAttached)
                    {
                        this.IsProbeAttached = false;

                        this.OnProbeDetached(this, null);
                    }

                    continue;
                }

                bool probeAttached = ((data[2] >> 4) & 0x1) == 0;

                if (!probeAttached && this.IsProbeAttached)
                {
                    this.IsProbeAttached = false;
                    this.OnProbeDetached(this, null);
                }

                if (!probeAttached || ((data[0] >> 6) & 0x1) != 1) 
                    continue;

                int signalStrength = data[0] & 0xF;
                int pulseRate = ((data[2] << 1) & 0x80) + (data[3] & 0x7F);
                int spO2 = data[4] & 0x7F;

                if (pulseRate == 255 || spO2 == 127) 
                    continue;

                this.LastReading = new Reading(pulseRate, spO2, signalStrength);

                if (probeAttached && !this.IsProbeAttached)
                {
                    this.IsProbeAttached = true;
                    this.OnProbeAttached(this, null);
                }

                this.OnHeartbeat(this, this.LastReading);
            }
        }

        /// <summary>
        /// Whether the PulseOximeter's probe is attached to a finger.
        /// </summary>
        public bool IsProbeAttached { get; private set; }

        /// <summary>
        /// The most recent valid reading from the pulse oximeter
        /// </summary>
        public Reading LastReading { get; private set; }

        /// <summary>
        /// A class representing a pulse oximeter reading
        /// </summary>
        public class Reading
        {
            internal Reading(int pulseRate, int spo2, int signalStrength)
            {
                this.PulseRate = pulseRate;
                this.SignalStrength = signalStrength;
                this.SPO2 = spo2;
            }

            /// <summary>
            /// The pulse rate automatically averaged over time.
            /// </summary>
            public int PulseRate { get; private set; }

            /// <summary>
            /// The oxygen saturation between 0 and 100.
            /// </summary>
            public int SPO2 { get; private set; }

            /// <summary>
            /// The signal strength between 0 and 15.
            /// </summary>
            public int SignalStrength { get; private set; }
        }

        /// <summary>
        /// Represents the delegate used for the Heartbeat event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void HeartbeatEventHandler(PulseOximeter sender, Reading e);

        /// <summary>
        /// Represents the delegate used for the ProbeAttached event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void ProbeAttachedEventHandler(PulseOximeter sender, EventArgs e);

        /// <summary>
        /// Represents the delegate used for the ProbeDetached event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void ProbeDetachedEventHandler(PulseOximeter sender, EventArgs e);

        /// <summary>
        /// Raised when the module detects a heartbeat.
        /// </summary>
        public event HeartbeatEventHandler Heartbeat;

        /// <summary>
        /// Raised when the module detects that the probe is placed on a finger.
        /// </summary>
        public event ProbeAttachedEventHandler ProbeAttached;

        /// <summary>
        /// Raised when the module detects that the probe is removed from a finger.
        /// </summary>
        public event ProbeDetachedEventHandler ProbeDetached;

        private HeartbeatEventHandler onHeartbeat;
        private ProbeAttachedEventHandler onProbeAttached;
        private ProbeDetachedEventHandler onProbeDetached;

        private void OnHeartbeat(PulseOximeter sender, Reading e)
        {
            if (this.onHeartbeat == null)
                this.onHeartbeat = this.OnHeartbeat;

            if (Program.CheckAndInvoke(this.Heartbeat, this.onHeartbeat, sender, e))
                this.Heartbeat(sender, e);
        }

        private void OnProbeAttached(PulseOximeter sender, EventArgs e)
        {
            if (this.onProbeAttached == null)
                this.onProbeAttached = this.OnProbeAttached;

            if (Program.CheckAndInvoke(this.ProbeAttached, this.onProbeAttached, sender, e))
                this.ProbeAttached(sender, e);
        }

        private void OnProbeDetached(PulseOximeter sender, EventArgs e)
        {
            if (this.onProbeDetached == null)
                this.onProbeDetached = this.OnProbeDetached;

            if (Program.CheckAndInvoke(this.ProbeDetached, this.onProbeDetached, sender, e))
                this.ProbeDetached(sender, e);
        }
    }
}
