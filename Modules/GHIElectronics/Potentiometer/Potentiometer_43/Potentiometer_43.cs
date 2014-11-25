using System;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A Potentiometer module for Microsoft .NET Gadgeteer
    /// </summary>
    [Obsolete]
    public class Potentiometer : GTM.Module
    {
        private GTI.AnalogInput input;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The mainboard socket that has the module plugged into it.</param>
        public Potentiometer(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('A', this);

            this.input = GTI.AnalogInputFactory.Create(socket, Socket.Pin.Three, this);
        }

        /// <summary>
        /// Gets the current voltage reading of the potentiometer.
        /// </summary>
        public double ReadVoltage()
        {
            return this.input.ReadVoltage();
        }

        /// <summary>
        ///  Gets the current position of the potentiometer between 0.0 and 1.0.
        /// </summary>
        public double ReadProportion()
        {
            return this.input.ReadProportion();
        }
    }
}
