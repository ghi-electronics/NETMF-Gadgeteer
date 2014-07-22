using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A TempHumidity module for Microsoft .NET Gadgeteer
    /// </summary>
    public class TempHumidSI70 : GTM.Module
    {
        private const byte MEASURE_HUMIDITY_HOLD = 0xE5;
        private const byte READ_TEMP_FROM_PREVIOUS = 0xE0;
        private const byte I2C_ADDRESS = 0x40;

        private GTI.SoftwareI2CBus i2c;
        private byte[] writeBuffer1;
        private byte[] writeBuffer2;
        private byte[] readBuffer1;
        private byte[] readBuffer2;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public TempHumidSI70(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('I', this);

            this.i2c = new GTI.SoftwareI2CBus(socket, Socket.Pin.Eight, Socket.Pin.Nine, TempHumidSI70.I2C_ADDRESS, 400, this);

            this.writeBuffer1 = new byte[1] { TempHumidSI70.MEASURE_HUMIDITY_HOLD };
            this.writeBuffer2 = new byte[1] { TempHumidSI70.READ_TEMP_FROM_PREVIOUS };
            this.readBuffer1 = new byte[2];
            this.readBuffer2 = new byte[2];
        }

        /// <summary>
        /// Obtains a single measurement.
        /// </summary>
        /// <returns>The measurement.</returns>
        public Measurement TakeMeasurement()
        {
            this.i2c.WriteRead(this.writeBuffer1, this.readBuffer1);
            this.i2c.WriteRead(this.writeBuffer2, this.readBuffer2);

            int rawRH = this.readBuffer1[0] << 8 | this.readBuffer1[1];
            int rawTemp = this.readBuffer2[0] << 8 | this.readBuffer2[1];

            double temperature = 175.72 * rawTemp / 65536.0 - 46.85;
            double relativeHumidity = 125.0 * rawRH / 65536.0 - 6.0;

            if (relativeHumidity < 0.0)
                relativeHumidity = 0.0;

            if (relativeHumidity > 100.0)
                relativeHumidity = 100.0;

            return new Measurement(temperature, relativeHumidity);
        }

        /// <summary>
        /// Result of a measurement.
        /// </summary>
        public class Measurement
        {
            /// <summary>
            /// The measured temperature in degrees Celsius.
            /// </summary>
            public double Temperature { get; private set; }

            /// <summary>
            /// The measured temperature in degrees Fahrenheit.
            /// </summary>
            public double TemperatureFahrenheit { get; private set; }

            /// <summary>
            /// The measured relative humidity.
            /// </summary>
            public double RelativeHumidity { get; private set; }

            /// <summary>
            /// Provides a string representation of the instance.
            /// </summary>
            /// <returns>A string describing the values contained in the object.</returns>
            public override string ToString()
            {
                return this.Temperature.ToString("F1") + " degrees Celsius, " + this.RelativeHumidity.ToString("F1") + "% relative humidity.";
            }

            internal Measurement(double temperature, double relativeHumidity)
            {
                this.RelativeHumidity = relativeHumidity;
                this.Temperature = temperature;
                this.TemperatureFahrenheit = temperature * 1.8 + 32.0;
            }
        }
    }
}
