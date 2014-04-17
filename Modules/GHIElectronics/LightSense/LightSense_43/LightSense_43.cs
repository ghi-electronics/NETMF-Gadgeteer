using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A LightSense module for Microsoft .NET Gadgeteer
    /// </summary>
    public class LightSense : GTM.Module
    {
        private GTI.AnalogInput input;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public LightSense(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('A', this);

            this.input = GTI.AnalogInputFactory.Create(socket, Socket.Pin.Three, this);
        }

        /// <summary>
        /// The voltage returned from the sensor.
        /// </summary>
        /// <returns>The voltage value between 0.0 and 3.3</returns>
        public double ReadVoltage()
        {
            return this.input.ReadVoltage();
        }

        /// <summary>
        /// The proportion returned from the sensor.
        /// </summary>
        /// <returns>The value between 0.0 and 1.0</returns>
        public double ReadProportion()
        {
            return this.input.ReadProportion();
        }

        /// <summary>
        /// Returns the current sensor reading in lux.
        /// </summary>
        /// <returns>A reading in lux between 0 and MAX_ILLUMINANCE.</returns>
        public double GetIlluminance()
        {
            return this.input.ReadProportion() * LightSense.MAX_ILLUMINANCE;
        }

        /// <summary>
        /// The maximum amount of lux the sensor can detect before becoming saturated.
        /// </summary>
        public const double MAX_ILLUMINANCE = 1000;
    }
}