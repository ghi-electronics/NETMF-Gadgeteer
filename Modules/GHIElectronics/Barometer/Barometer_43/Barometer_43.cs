using System;
using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A Barometer module for Microsoft .NET Gadgeteer
    /// </summary>
    public class Barometer : GTM.Module
    {
        private static ushort ADC_ADDRESS = 0x77;
        private static ushort EEPROM_ADDR = 0x50;

        private GTI.I2CBus i2c;
        private GTI.DigitalOutput xclr;
        private Coefficients coefficients;
        private GT.Timer timer;
        private byte[] writeBuffer1;
        private byte[] readBuffer1;
        private byte[] readBuffer2;
        private byte[] readBuffer18;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public Barometer(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('I', this);
            
            this.writeBuffer1 = new byte[1];
            this.readBuffer1 = new byte[1];
            this.readBuffer2 = new byte[2];
            this.readBuffer18 = new byte[18];
            this.coefficients = new Coefficients();
            
            this.xclr = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three, false, this);
            this.i2c = GTI.I2CBusFactory.Create(socket, Barometer.EEPROM_ADDR, 100, this);

            this.ReadFactoryCalibrationData();

            this.i2c.Address = Barometer.ADC_ADDRESS;

            this.timer = new GT.Timer(200);
            this.timer.Tick += (a) => this.TakeMeasurement();
        }

        private enum Register : byte
        {
            COEFF = 0x10,
            DATD1 = 0xFF,
            DATD2 = 0xF0
        }

        private struct Coefficients
        {
            public int C1 { get; set; }
            public int C2 { get; set; }
            public int C3 { get; set; }
            public int C4 { get; set; }
            public int C5 { get; set; }
            public int C6 { get; set; }
            public int C7 { get; set; }

            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
            public int D { get; set; }
        }

        private void ReadFactoryCalibrationData()
        {
            this.xclr.Write(false);

            this.ReadRegisters(Register.COEFF, this.readBuffer18);

            this.coefficients.C1 = (this.readBuffer18[0] << 8) + this.readBuffer18[1];
            this.coefficients.C2 = (this.readBuffer18[2] << 8) + this.readBuffer18[3];
            this.coefficients.C3 = (this.readBuffer18[4] << 8) + this.readBuffer18[5];
            this.coefficients.C4 = (this.readBuffer18[6] << 8) + this.readBuffer18[7];
            this.coefficients.C5 = (this.readBuffer18[8] << 8) + this.readBuffer18[9];
            this.coefficients.C6 = (this.readBuffer18[10] << 8) + this.readBuffer18[11];
            this.coefficients.C7 = (this.readBuffer18[12] << 8) + this.readBuffer18[13];
            this.coefficients.A = this.readBuffer18[14];
            this.coefficients.B = this.readBuffer18[15];
            this.coefficients.C = this.readBuffer18[16];
            this.coefficients.D = this.readBuffer18[17];
        }

        private void ReadRegisters(Register register, byte[] buffer)
        {
            this.writeBuffer1[0] = (byte)register;
            this.i2c.WriteRead(this.writeBuffer1, buffer);
        }

        private int ReadRegister(byte address)
        {
            this.writeBuffer1[0] = address;

            this.i2c.WriteRead(this.writeBuffer1, this.readBuffer2);

            return (this.readBuffer2[0] << 8) | this.readBuffer2[1];
        }

        private void TakeMeasurement()
        {
            this.xclr.Write(true);

            this.i2c.Write(new byte[] { 0xFF, 0xF0 });
            Thread.Sleep(40);
            int d1 = this.ReadRegister(0xFD);

            this.i2c.Write(new byte[] { 0xFF, 0xE8 });
            Thread.Sleep(40);
            int d2 = this.ReadRegister(0xFD);

            this.xclr.Write(false);

            double dUT = d2 - this.coefficients.C5 - ((d2 - this.coefficients.C5) / Math.Pow(2, 7) * ((d2 - this.coefficients.C5) / Math.Pow(2, 7)) * (d2 >= this.coefficients.C5 ? this.coefficients.A : this.coefficients.B) / Math.Pow(2, this.coefficients.C));
            double off = (this.coefficients.C2 + (this.coefficients.C4 - 1024) * dUT / Math.Pow(2, 14)) * 4;
            double sens = this.coefficients.C1 + this.coefficients.C3 * dUT / Math.Pow(2, 10);
            double x = sens * (d1 - 7168) / Math.Pow(2, 14) - off;
            double p = x * 10 / Math.Pow(2, 5) + this.coefficients.C7;
            double t = 250 + dUT * this.coefficients.C6 / Math.Pow(2, 16) - dUT / Math.Pow(2, this.coefficients.D);

            this.OnMeasurementComplete(this, new MeasurementCompleteEventArgs(t / 10, p / 10));
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
            /// The measured temperature in degrees Celsius.
            /// </summary>
            public double Temperature { get; private set; }

            /// <summary>
            /// The measured atmospheric pressure in hectopascals.
            /// </summary>
            public double Pressure { get; private set; }
            
            internal MeasurementCompleteEventArgs(double temperature, double pressure)
            {
                this.Temperature = temperature;
                this.Pressure = pressure;
            }

            /// <summary>
            /// Provides a string representation of the instance.
            /// </summary>
            /// <returns>A string describing the values contained in the object.</returns>
            public override string ToString()
            {
                return "Temperature: " + Temperature.ToString("f2") + " degrees Celsius. Pressure: " + Pressure.ToString("f2") + " hPa.";
            }
        }

        /// <summary>
        /// Represents the delegate used for the MeasurementComplete event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void MeasurementCompleteEventHandler(Barometer sender, MeasurementCompleteEventArgs e);

        /// <summary>
        /// Raised when a measurement reading is complete.
        /// </summary>
        public event MeasurementCompleteEventHandler MeasurementComplete;

        private MeasurementCompleteEventHandler onMeasurementComplete;

        private void OnMeasurementComplete(Barometer sender, MeasurementCompleteEventArgs e)
        {
            if (this.onMeasurementComplete == null) 
                this.onMeasurementComplete = this.OnMeasurementComplete;

            if (Program.CheckAndInvoke(this.MeasurementComplete, this.onMeasurementComplete, sender, e))
                this.MeasurementComplete(sender, e);
        }
    }
}
