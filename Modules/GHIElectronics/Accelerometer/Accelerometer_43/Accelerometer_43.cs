using Microsoft.SPOT;
using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A Accelerometer module for Microsoft .NET Gadgeteer
    /// </summary>
    public class Accelerometer : GTM.Module
    {
        private GTI.InterruptInput interrupt;
        private GTI.I2CBus i2c;
        private GT.Timer timer;
        private Range range;
        private Mode mode;
        private bool autoResetThresholdDetection;
        private byte[] read1;
        private byte[] write1;
        private int offsetX;
        private int offsetY;
        private int offsetZ;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public Accelerometer(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('I', this);

            this.read1 = new byte[1];
            this.write1 = new byte[1];
            this.autoResetThresholdDetection = false;

            this.interrupt = GTI.InterruptInputFactory.Create(socket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, GTI.InterruptMode.RisingEdge, this);
            this.interrupt.Interrupt += this.OnInterrupt;

            this.i2c = GTI.I2CBusFactory.Create(socket, 0x1D, 50, this);
            this.i2c.Timeout = 1000;

            this.timer = new GT.Timer(200);
            this.timer.Tick += (a) => this.TakeMeasurement();

            this.OperatingMode = Mode.Measurement;
            this.MeasurementRange = Range.TwoG;
        }

        private enum Mode
        {
            Standby = 0,
            Measurement = 1,
            LevelDetection = 2,
            PulseDetection = 3
        }

        private enum Register : byte
        {
            XOUTL = 0x00,
            XOUTH = 0x01,
            YOUTL = 0x02,
            YOUTH = 0x03,
            ZOUTL = 0x04,
            ZOUTH = 0x05,
            XOUT8 = 0x06,
            YOUT8 = 0x07,
            ZOUT8 = 0x08,
            STATUS = 0x09,
            DETSRC = 0x0A,
            TOUT = 0x0B,
            I2CAD = 0x0D,
            USRINF = 0x0E,
            WHOAMI = 0x0F,
            XOFFL = 0x10,
            XOFFH = 0x11,
            YOFFL = 0x12,
            YOFFH = 0x13,
            ZOFFL = 0x14,
            ZOFFH = 0x15,
            MCTL = 0x16,
            INTRST = 0x17,
            CTL1 = 0x18,
            CTL2 = 0x19,
            LDTH = 0x1A,
            PDTH = 0x1B,
            PW = 0x1C,
            LT = 0x1D,
            TW = 0x1E
        }

        private Mode OperatingMode
        {
            get
            {
                return this.mode;
            }
            set
            {
                this.mode = value;
                this.Write(Register.MCTL, (byte)(0x40 | (((byte)this.range) << 2) | (byte)value));
            }
        }

        /// <summary>
        /// The possible measurement ranges.
        /// </summary>
        public enum Range
        {
            /// <summary>
            /// +/- 2G measurement range
            /// </summary>
            TwoG = 2,

            /// <summary>
            /// +/- 4G measurement range
            /// </summary>
            FourG = 4,

            /// <summary>
            /// +/- 8G measurement range
            /// </summary>
            EightG = 8
        }

        /// <summary>
        /// The actual measurement range.
        /// </summary>
        public Range MeasurementRange
        {
            get
            {
                return this.range;
            }
            set
            {
                this.range = value;

                this.Write(Register.MCTL, (byte)(0x40 | (((byte)range) << 2) | (byte)this.OperatingMode));
            }
        }

        /// <summary>
        /// Calibrates the accelerometer with the given references.
        /// </summary>
        /// <param name="refX">An acceleration in the x axis representing the resting orientation of the accelerometer.</param>
        /// <param name="refY">An acceleration in the y axis representing the resting orientation of the accelerometer.</param>
        /// <param name="refZ">An acceleration in the z axis representing the resting orientation of the accelerometer.</param>
        public void Calibrate(int refX, int refY, int refZ)
        {
            this.OperatingMode = Mode.Measurement;

            var buffer = new byte[3];
            Read(Register.XOUT8, buffer);

            int x = ((((buffer[0] >> 7) == 1) ? -128 : 0) + (buffer[0] & 0x7F));
            int y = ((((buffer[1] >> 7) == 1) ? -128 : 0) + (buffer[1] & 0x7F));
            int z = ((((buffer[2] >> 7) == 1) ? -128 : 0) + (buffer[2] & 0x7F));

            double gravityValue = 0;
            switch (this.range)
            {
                case Range.TwoG: gravityValue = 64; break;
                case Range.FourG: gravityValue = 32; break;
                case Range.EightG: gravityValue = 16; break;
            }

            this.offsetX = -x + (int)(gravityValue * refX);
            this.offsetY = -y + (int)(gravityValue * refY);
            this.offsetZ = -z + (int)(gravityValue * refZ);
        }

        /// <summary>
        /// Calibrates the accelerometer. Make sure that the accelerometer is not moving and is resting on a flat surface when calling this method.
        /// </summary>
        public void Calibrate()
        {
            this.Calibrate(0, 0, 1);
        }

        /// <summary>
        /// Enables automatic detection and notification when an acceleration threshold is exceeded resulting in the ThresholdExceeded event being raised. Continous measurement is disabled when threshold detection mode is enabled.
        /// </summary>
        /// <param name="threshold">The acceleration threshold, between -8.0 and 8.0 G.</param>
        /// <param name="enableX">The enable threshold detection in the X axis.</param>
        /// <param name="enableY">The enable threshold detection in the Y axis.</param>
        /// <param name="enableZ">The enable threshold detection in the Z axis.</param>
        /// <param name="absolute">The absoulte threshold detection. If set to true the sign of the threshold is ignored, and the absolute value of the acceleration is compared with the absolute value of the threshold. If set to false, the sign of the threshold will be taken into account, the event will only be raised if the acceleration falls below a negative threshold or above a positive threshold.</param>
        /// <param name="detectFreefall">Freefall detection. If set to true, the ThresholdExceeded event will be raised when the acceleration in all the enabled axes is less than the absolute threshold. In order to detect freefall correctly, set the threshold to a small value and enable detection on all axes.</param>
        /// <param name="autoReset">Automatically reset the thershold detection. If set to false, the ThresholdExceeded will be raised only once, until the ResetThresholdDetection method is called manually. If set to true, the ResetThresholdDetection will be called automatically, and the event will be continously raised as long as the thershold conditions are exceeded.</param>
        public void EnableThresholdDetection(double threshold, bool enableX, bool enableY, bool enableZ, bool absolute, bool detectFreefall, bool autoReset)
        {
            this.StopTakingMeasurements();

            this.OperatingMode = Mode.LevelDetection;
            this.MeasurementRange = Range.EightG;
            this.autoResetThresholdDetection = autoReset;

            byte b = 0x00;
            b |= (byte)((enableX ? 0 : 1) << 3);
            b |= (byte)((enableY ? 0 : 1) << 4);
            b |= (byte)((enableZ ? 0 : 1) << 5);
            b |= (byte)((absolute ? 0 : 1) << 6);
            this.Write(Register.CTL1, b);

            b = 0x00;
            b |= (byte)((detectFreefall ? 1 : 0));
            this.Write(Register.CTL2, b);

            if (absolute)
            {
                this.Write(Register.LDTH, (byte)(System.Math.Abs((int)((threshold / 8.0) * 128.0)) & 0x7F));
            }
            else
            {
                this.Write(Register.LDTH, (byte)((byte)((threshold / 8.0) * 128.0)));
            }

            this.ResetThresholdDetection();
        }

        /// <summary>
        /// Reset the threshold detection process.
        /// </summary>
        public void ResetThresholdDetection()
        {
            this.Write(Register.INTRST, 0x03);
            this.Write(Register.INTRST, 0x00);
        }

        private void OnInterrupt(GTI.InterruptInput sender, bool value)
        {
            if (this.OperatingMode == Mode.LevelDetection)
            {
                this.OnThresholdExceededEvent(this, null);

                if (this.autoResetThresholdDetection)
                    this.ResetThresholdDetection();
            }
        }

        private byte ReadByte(Register register)
        {
            this.write1[0] = (byte)register;
            this.i2c.WriteRead(this.write1, this.read1);
            return this.read1[0];
        }

        private void Read(Register register, byte[] readBuffer)
        {
            this.read1[0] = (byte)register;
            this.i2c.WriteRead(this.read1, readBuffer);
        }

        private void Write(Register register, byte value)
        {
            this.i2c.Write((byte)register, value);
        }

        private void TakeMeasurement()
        {
            var buffer = new byte[3];

            this.Read(Register.XOUT8, buffer);

            double x = (((((buffer[0] >> 7) == 1) ? -128 : 0) + (buffer[0] & 0x7F)) + this.offsetX) / 128.0 * (double)this.range;
            double y = (((((buffer[1] >> 7) == 1) ? -128 : 0) + (buffer[1] & 0x7F)) + this.offsetY) / 128.0 * (double)this.range;
            double z = (((((buffer[2] >> 7) == 1) ? -128 : 0) + (buffer[2] & 0x7F)) + this.offsetZ) / 128.0 * (double)this.range;

            this.OnMeasurementComplete(this, new MeasurementCompleteEventArgs(x, y, z));
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
            public double X { get; private set; }

            /// <summary>
            /// Y-axis sensor data.
            /// </summary>
            public double Y { get; private set; }

            /// <summary>
            /// Z-axis sensor data.
            /// </summary>
            public double Z { get; private set; }

            internal MeasurementCompleteEventArgs(double x, double y, double z)
            {
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
                return "X: " + X.ToString("f2") + " Y: " + Y.ToString("f2") + " Z: " + Z.ToString("f2");
            }
        }

        /// <summary>
        /// Represents the delegate used for the MeasurementComplete event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void MeasurementCompleteEventHandler(Accelerometer sender, MeasurementCompleteEventArgs e);

        /// <summary>
        /// Represents the delegate used for the ThresholdExceeded event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void ThresholdExceededEventHandler(Accelerometer sender, EventArgs e);

        /// <summary>
        /// Raised when a measurement reading is complete.
        /// </summary>
        public event MeasurementCompleteEventHandler MeasurementComplete;

        /// <summary>
        /// Raised when an acceleration threshold is exceeded.
        /// </summary>
        public event ThresholdExceededEventHandler ThresholdExceeded;

        private MeasurementCompleteEventHandler onMeasurementComplete;
        private ThresholdExceededEventHandler onThresholdExceeded;

        private void OnMeasurementComplete(Accelerometer sender, MeasurementCompleteEventArgs e)
        {
            if (this.onMeasurementComplete == null)
                this.onMeasurementComplete = this.OnMeasurementComplete;

            if (Program.CheckAndInvoke(this.MeasurementComplete, this.onMeasurementComplete, sender, e))
                this.MeasurementComplete(sender, e);
        }

        private  void OnThresholdExceededEvent(Accelerometer sender, EventArgs e)
        {
            if (this.onThresholdExceeded == null) 
                this.onThresholdExceeded = this.OnThresholdExceededEvent;

            if (Program.CheckAndInvoke(this.ThresholdExceeded, this.onThresholdExceeded, sender, e))
                this.ThresholdExceeded(sender, e);
        }
    }
}
