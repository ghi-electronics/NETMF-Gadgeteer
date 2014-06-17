using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// An AMPM35 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class AMPM35 : GTM.Module
    {
        private GTI.AnalogOutput analogOut;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public AMPM35(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('O', this);

            this.analogOut = GTI.AnalogOutputFactory.Create(socket, Socket.Pin.Five, this);
        }

        /// <summary>
        /// The output port that is amplified.
        /// </summary>
        public GTI.AnalogOutput Output
        {
            get
            {
                return this.analogOut;
            }
        }
    }
}
