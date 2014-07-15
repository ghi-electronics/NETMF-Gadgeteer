using System;
using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A TempHumidity module for Microsoft .NET Gadgeteer
    /// </summary>
    public class TempHumidity : GTM.Module
    {
        private Thread timer;
        private GTI.DigitalIO data;
        private GTI.DigitalOutput sck;
        private bool running;
        private int interval;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public TempHumidity(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported(new char[] { 'X', 'Y' }, this);

            this.sck = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Four, false, this);
            this.data = GTI.DigitalIOFactory.Create(socket, Socket.Pin.Three, true, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, this);
            this.running = false;
        }

        /// <summary>
        /// Obtains a single measurement and raises the event when complete.
        /// </summary>
        public void RequestSingleMeasurement()
        {
            if (this.timer != null && this.timer.IsAlive) throw new InvalidOperationException("You cannot request a single measurement while continuous measurements are being taken.");

            this.running = false;
            this.timer = new Thread(this.TakeMeasurement);
            this.timer.Start();
        }

        /// <summary>
        /// Starts taking measurements and fires MeasurementComplete when a new measurement is available.
        /// </summary>
        public void StartTakingMeasurements()
        {
            this.running = true;
            this.timer = new Thread(this.TakeMeasurement);
            this.timer.Start();
        }

        /// <summary>
        /// Stops taking measurements.
        /// </summary>
        public void StopTakingMeasurements()
        {
            this.running = false;
            this.timer.Join();
        }

        /// <summary>
        /// The interval in milliseconds at which measurements are taken.
        /// </summary>
        public int MeasurementInterval
        {
            get
            {
                return this.interval;
            }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("value", "value must be positive.");

                this.interval = value;
            }
        }

        private void TakeMeasurement()
        {
            do
            {
                this.ResetCommuncation();

                this.TransmissionStart();

                double temperature = -39.65 + 0.01 * this.MeasureTemperature();

                this.TransmissionStart();

                int rawHumidity = this.MeasureHumidity();
                double humidity = -2.0468 + 0.0367 * rawHumidity - 1.5955E-6 * rawHumidity * rawHumidity;
                humidity = (temperature - 25) * (0.01 + 0.00008 * rawHumidity) + humidity;

                temperature = Math.Round(100.0 * temperature) / 100.0;
                humidity = Math.Round(100.0 * humidity) / 100.0;

                this.OnMeasurementComplete(this, new MeasurementCompleteEventArgs(temperature, humidity));

                Thread.Sleep(this.interval);
            } while (this.running);
        }

        private void TransmissionStart()
        {
            this.data.Write(true);
            this.sck.Write(true);
            this.data.Write(false);
            this.sck.Write(false);
            this.sck.Write(true);
            this.data.Write(true);
            this.sck.Write(false);
        }

        private int MeasureTemperature()
        {
            this.data.Write(false);

            this.sck.Write(true);
            this.sck.Write(false);

            this.sck.Write(true);
            this.sck.Write(false);

            this.sck.Write(true);
            this.sck.Write(false);

            this.sck.Write(true);
            this.sck.Write(false);

            this.sck.Write(true);
            this.sck.Write(false);

            this.sck.Write(true);
            this.sck.Write(false);

            this.data.Write(true);

            this.sck.Write(true);
            this.sck.Write(false);

            this.data.Write(false);

            this.data.Write(true);

            this.sck.Write(true);
            this.sck.Write(false);

            this.sck.Write(true);

            this.data.Read();

            this.sck.Write(false);

            while (this.data.Read())
                Thread.Sleep(1);

            int reading = 0;

            for (int i = 0; i < 8; i++)
            {
                reading |= this.data.Read() ? (1 << (15 - i)) : 0;
                this.sck.Write(true);
                this.sck.Write(false);
            }

            this.data.Write(false);

            this.sck.Write(true);
            this.sck.Write(false);

            for (int i = 8; i < 16; i++)
            {
                reading |= this.data.Read() ? (1 << (15 - i)) : 0;
                this.sck.Write(true);
                this.sck.Write(false);
            }

            this.data.Write(true);

            return reading;
        }

        private int MeasureHumidity()
        {
            this.data.Write(false);

            this.sck.Write(true);  
            this.sck.Write(false); 

            this.sck.Write(true);  
            this.sck.Write(false); 

            this.sck.Write(true);  
            this.sck.Write(false); 

            this.sck.Write(true);  
            this.sck.Write(false); 

            this.sck.Write(true);  
            this.sck.Write(false); 

            this.data.Write(true); 

            this.sck.Write(true);
            this.sck.Write(false);

            this.data.Write(false);

            this.sck.Write(true);  
            this.sck.Write(false);

            this.data.Write(true); 

            this.sck.Write(true);
            this.sck.Write(false); 

            this.sck.Write(true);

            this.data.Read();

            this.sck.Write(false);

            while (this.data.Read())
                Thread.Sleep(1);

            int reading = 0;
            for (int i = 0; i < 8; i++)
            {
                reading |= this.data.Read() ? (1 << (15 - i)) : 0;
                this.sck.Write(true);
                this.sck.Write(false);
            }

            this.data.Write(false);

            this.sck.Write(true);
            this.sck.Write(false);

            for (int i = 8; i < 16; i++)
            {
                reading |= this.data.Read() ? (1 << (15 - i)) : 0;
                this.sck.Write(true);
                this.sck.Write(false);
            }

            this.data.Write(true);

            return reading;
        }

        private void ResetCommuncation()
        {
            this.data.Write(true);

            for (int i = 0; i < 9; i++)
            {
                this.sck.Write(true);
                this.sck.Write(false);
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
            /// The measured relative humidity.
            /// </summary>
            public double RelativeHumidity { get; private set; }

            internal MeasurementCompleteEventArgs(double temperature, double relativeHumidity)
            {
                this.Temperature = temperature;
                this.RelativeHumidity = relativeHumidity;
            }

            /// <summary>
            /// Provides a string representation of the instance.
            /// </summary>
            /// <returns>A string describing the values contained in the object.</returns>
            public override string ToString()
            {
                return "Temperature: " + Temperature.ToString("f2") + " degrees Celsius. Relative humidity: " + RelativeHumidity.ToString("f2") + ".";
            }
        }

        /// <summary>
        /// Represents the delegate used for the MeasurementComplete event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void MeasurementCompleteEventHandler(TempHumidity sender, MeasurementCompleteEventArgs e);

        /// <summary>
        /// Raised when a measurement reading is complete.
        /// </summary>
        public event MeasurementCompleteEventHandler MeasurementComplete;

        private MeasurementCompleteEventHandler onMeasurementComplete;

        private void OnMeasurementComplete(TempHumidity sender, MeasurementCompleteEventArgs e)
        {
            if (this.onMeasurementComplete == null)
                this.onMeasurementComplete = this.OnMeasurementComplete;

            if (Program.CheckAndInvoke(this.MeasurementComplete, this.onMeasurementComplete, sender, e))
                this.MeasurementComplete(sender, e);
        }
    }
}
