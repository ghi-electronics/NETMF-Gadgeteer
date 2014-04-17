using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules; 

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A Moisture module for Microsoft .NET Gadgeteer
    /// </summary>
    public class Moisture : GTM.Module
    {
        private GTI.AnalogInput input;
        private GTI.DigitalOutput enable;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public Moisture(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('A', this);

            this.input = GTI.AnalogInputFactory.Create(socket, Socket.Pin.Three, this);
            this.enable = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Six, true, this);
        }

        /// <summary>
        /// The moisture reading from the sensor.
        /// </summary>
        /// <returns>A value where 0 is fully dry and 1000 (or greater) is completely wet.</returns>
        public int ReadMoisture()
        {
            return (int)(this.input.ReadProportion() * 1600);
        }

        /// <summary>
        /// Turns sensor on and off.
        /// </summary>
        public bool Enabled
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