using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// An LED7C module for Microsoft .NET Gadgeteer
    /// </summary>
    public class LED7C : GTM.Module
    {
		/// <summary>
		/// The possible display colors.
		/// </summary>
        public enum Color
        {
            /// <summary>
            /// Red
            /// </summary>
            Red = (1 << 2) | (0 << 1) | 0,
            
            /// <summary>
            /// Green
            /// </summary>
            Green = (0 << 2) | (1 << 1) | 0,
            
            /// <summary>
            /// Blue
            /// </summary>
            Blue = (0 << 2) | (0 << 1) | 1,
            
            /// <summary>
            /// Yellow
            /// </summary>
            Yellow = (1 << 2) | (1 << 1) | 0,
            
            /// <summary>
            /// Cyan
            /// </summary>
            Cyan = (0 << 2) | (1 << 1) | 1,
            
            /// <summary>
            /// Magenta
            /// </summary>
            Magenta = (1 << 2) | (0 << 1) | 1,
            
            /// <summary>
            /// White
            /// </summary>
            White = (1 << 2) | (1 << 1) | 1,

            /// <summary>
            /// Off
            /// </summary>
            Off = 0,

        }

        private GTI.DigitalOutput red;
        private GTI.DigitalOutput blue;
        private GTI.DigitalOutput green;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public LED7C(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported(new char[] { 'X', 'Y' }, this);

            this.red = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Four, false, this);
            this.blue = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three, false, this);
            this.green = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Five, false, this);
        }

        /// <summary>
        /// Sets the color of the LED.
        /// </summary>
        /// <param name="color">The color to set to.</param>
        public void SetColor(Color color)
        {
            int c = (int)color;

            this.red.Write((c & 4) != 0);
            this.green.Write((c & 2) != 0);
            this.blue.Write((c & 1) != 0);
        }
    }
}
