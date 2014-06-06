using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A Compass module for Microsoft .NET Gadgeteer
    /// </summary>
    public class Compass : GTM.Module
    {
        private GTI.InterruptInput dataReady;
        private GTI.I2CBus i2c;
        private GT.Timer timer;
        private byte[] writeBuffer1;
        private byte[] readBuffer6;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public Compass(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('I', this);
            
            this.writeBuffer1 = new byte[1];
            this.readBuffer6 = new byte[6];

            this.timer = new GT.Timer(200);
            this.timer.Tick += (a) => this.TakeMeasurement();

            this.dataReady = GTI.InterruptInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, GTI.InterruptMode.RisingEdge, this);
            this.dataReady.Interrupt += this.OnInterrupt;

            this.i2c = GTI.I2CBusFactory.Create(socket, 0x1E, 100, this);
        }

        /// <summary>
        /// Possible sensing gain values.
        /// </summary>
        public enum Gain : byte
        {
            /// <summary>
            /// +/- 0.88 Ga
            /// </summary>
            Gain1 = 0x00,

            /// <summary>
            /// +/- 1.2 Ga (default)
            /// </summary>
            Gain2 = 0x20,

            /// <summary>
            /// +/- 1.9 Ga
            /// </summary>
            Gain3 = 0x40,

            /// <summary>
            /// +/- 2.5 Ga
            /// </summary>
            Gain4 = 0x60,

            /// <summary>
            /// +/- 4.0 Ga
            /// </summary>
            Gain5 = 0x80,

            /// <summary>
            /// +/- 4.7 Ga
            /// </summary>
            Gain6 = 0xA0,

            /// <summary>
            /// +/- 5.6 Ga
            /// </summary>
            Gain7 = 0xC0,

            /// <summary>
            /// +/- 8.1 Ga
            /// </summary>
            Gain8 = 0xE0,
        }

        private enum Register : byte
        {
            CRA = 0x00,
            CRB = 0x01,
            MR = 0x02,
            DXRA = 0x03,
            DXRB = 0x04,
            DZRA = 0x05,
            DZRB = 0x06,
            DYRA = 0x07,
            DYRB = 0x08,
            SR = 0x09,
            IRA = 0x0A,
            IRB = 0x0B,
            IRC = 0x0C
        }

        private enum Mode : byte
        {
            Continous = 0x00,
            SingleMode = 0x01,
            IdleMode = 0x02,
            SleepMode = 0x03
        }

        private void TakeMeasurement()
        {
            this.i2c.Write((byte)Register.MR, (byte)Mode.SingleMode);
        }

        void OnInterrupt(GTI.InterruptInput sender, bool value)
        {
            this.writeBuffer1[0] = (byte)Register.DXRA;
            this.i2c.WriteRead(this.writeBuffer1, this.readBuffer6);

            int rawX = (this.readBuffer6[0] << 8) | this.readBuffer6[1];
            int rawZ = (this.readBuffer6[2] << 8) | this.readBuffer6[3];
            int rawY = (this.readBuffer6[4] << 8) | this.readBuffer6[5];

            rawX = ((rawX >> 15) == 1 ? -32767 : 0) + (rawX & 0x7FFF);
            rawZ = ((rawZ >> 15) == 1 ? -32767 : 0) + (rawZ & 0x7FFF);
            rawY = ((rawY >> 15) == 1 ? -32767 : 0) + (rawY & 0x7FFF);

            if (rawX == -4096 || rawY == -4096 || rawZ == -4096)
            {
                this.DebugPrint("Invalid data read. Measurement discarded.");

                return;
            }


            this.OnMeasurementComplete(this, new MeasurementCompleteEventArgs(Math.Atan2((double)rawY, (double)rawX) * (180 / 3.14159265) + 180, rawX, rawY, rawZ));
        }

        /// <summary>
        /// Sets the sensor gain value.
        /// </summary>
        /// <param name="gain">The gain value.</param>
        public void SetGain(Gain gain)
        {
            this.i2c.Write((byte)Register.CRB, (byte)gain);
        }

        /// <summary>
        /// Obtains a single measurement and raises the event when complete.
        /// </summary>
        public void RequestSingleMeasurement()
        {
            if (this.timer.IsRunning) throw new InvalidOperationException("You cannot request a single measurement while continuous measurements are being taken.");

            this.timer.Behavior = Timer.BehaviorType.RunOnce;
            this.timer.Start();
        }

        /// <summary>
        /// Starts taking measurements and fires MeasurementComplete when a new measurement is available.
        /// </summary>
        public void StartTakingMeasurements()
        {
            this.timer.Behavior = Timer.BehaviorType.RunContinuously;
            this.timer.Start();
        }

        /// <summary>
        /// Stops taking measurements.
        /// </summary>
        public void StopTakingMeasurements()
        {
            this.timer.Stop();
        }

        /// <summary>
        /// The interval at which measurements are taken.
        /// </summary>
        public TimeSpan MeasurementInterval
        {
            get
            {
                return this.timer.Interval;
            }
            set
            {
                var wasRunning = this.timer.IsRunning;

                this.timer.Stop();
                this.timer.Interval = value;

                if (wasRunning)
                    this.timer.Start();
            }
        }
        
        /// <summary>
        /// Event arguments for the MeasurementComplete event.
        /// </summary>
        public class MeasurementCompleteEventArgs : Microsoft.SPOT.EventArgs
        {
            /// <summary>
            /// X-axis sensor data.
            /// </summary>
            public int X { get; private set; }

            /// <summary>
            /// Y-axis sensor data.
            /// </summary>
            public int Y { get; private set; }

            /// <summary>
            /// Z-axis sensor data.
            /// </summary>
            public int Z { get; private set; }

            /// <summary>
            /// Angle of heading in the XY plane in radians.
            /// </summary>
            public double Angle { get; private set; }

            internal MeasurementCompleteEventArgs(double angle, int x, int y, int z)
            {
                this.Angle = angle;
                this.X = x;
                this.Y = y;
                this.Z = z;
            }

            /// <summary>
            /// Provides a string representation of the instance.
            /// </summary>
            /// <returns>A string describing the values contained in the object.</returns>
            public override string ToString()
            {
                return "Angle: " + Angle.ToString("f2") + " X: " + X.ToString("f2") + " Y: " + Y.ToString("f2") + " Z: " + Z.ToString("f2");
            }
        }

        /// <summary>
        /// Represents the delegate used for the MeasurementComplete event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void MeasurementCompleteEventHandler(Compass sender, MeasurementCompleteEventArgs e);

        /// <summary>
        /// Raised when a measurement reading is complete.
        /// </summary>
        public event MeasurementCompleteEventHandler MeasurementComplete;

        private MeasurementCompleteEventHandler onMeasurementComplete;

        private void OnMeasurementComplete(Compass sender, MeasurementCompleteEventArgs e)
        {
            if (this.onMeasurementComplete == null)
                this.onMeasurementComplete = this.OnMeasurementComplete;

            if (Program.CheckAndInvoke(this.MeasurementComplete, this.onMeasurementComplete, sender, e))
                this.MeasurementComplete(sender, e);
        }
    }
}
