using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A GasSense module for Microsoft .NET Gadgeteer
    /// </summary>
    public class GasSense : GTM.Module
    {
        private GTI.AnalogInput input;
        private GTI.DigitalOutput enable;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public GasSense(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('A', this);

            this.input = GTI.AnalogInputFactory.Create(socket, Socket.Pin.Three, this);
            this.enable = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Four, false, this);
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
        /// Turns the heating element on or off. This may take up to 10 seconds befre a proper reading is taken.
        /// </summary>
        public bool HeatingElementEnabled
        {
            get
            {
                return this.enable.Read();
            }
            set
            {
                this.enable.Write(value);
            }
        }
    }
}