using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A LightSensor module for Microsoft .NET Gadgeteer
    /// </summary>
    public class LightSensor : GTM.Module
    {
        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public LightSensor(int socketNumber)
        {
            // This finds the Socket instance from the user-specified socket number.  
            // This will generate user-friendly error messages if the socket is invalid.
            // If there is more than one socket on this module, then instead of "null" for the last parameter, 
            // put text that identifies the socket to the user (e.g. "S" if there is a socket type S)
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('A', this);

            this.input = new GTI.AnalogInput(socket, Socket.Pin.Three, this);
        }

        private GTI.AnalogInput input;

        /// <summary>
        /// Returns the current voltage reading of the light sensor
        /// </summary>
        public double ReadLightSensorVoltage()
        {
            return input.ReadVoltage();
        }

        /// <summary>
        ///  Returns the current strength of the light relative to its maximum: range 0.0 to 100.0
        /// </summary>
        public double ReadLightSensorPercentage()
        {
            return (input.ReadProportion() * 100);
        }
    }
}
