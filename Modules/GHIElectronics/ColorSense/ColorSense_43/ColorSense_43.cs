using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A ColorSense module for Microsoft .NET Gadgeteer
    /// </summary>
    public class ColorSense : GTM.Module
    {
        private GTI.DigitalOutput led;
        private GTI.SoftwareI2CBus i2c;
        private byte[] writeBuffer;
        private byte[] readBuffer;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public ColorSense(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported(new char[] {'X', 'Y'}, this);

            this.writeBuffer = new byte[1];
            this.readBuffer = new byte[2];
            this.led = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three, false, this);
            this.i2c = new GTI.SoftwareI2CBus(socket, Socket.Pin.Five, Socket.Pin.Four, 0x39, 100, this);
            this.i2c.Write(new byte[] { 0x80, 0x03 });
        }

        /// <summary>
        /// Holds the color data.
        /// </summary>
        public struct ColorData
        {
            /// <summary>
            /// Intensity of green-filtered channel
            /// </summary>
            public int Green { get; set; }

            /// <summary>
            /// Intensity of red-filtered channel
            /// </summary>
            public int Red { get; set; }

            /// <summary>
            /// Intensity of blue-filtered channel
            /// </summary>
            public int Blue { get; set; }

            /// <summary>
            /// Intensity of non-filtered channel
            /// </summary>
            public int Clear { get; set; }
        }

        /// <summary>
        /// Sets the state of the onboard LED.
        /// </summary>
        public bool LedEnabled
        {
            get
            {
                return this.led.Read();
            }
            set
            {
                this.led.Write(value);
            }
        }

        /// <summary>
        /// Reads the current color from the sensor.
        /// </summary>
        /// <returns>The measured color.</returns>
        public ColorData ReadColor()
        {
            return new ColorData { Green = this.ReadShort(0x90), Red = this.ReadShort(0x92), Blue = this.ReadShort(0x94), Clear = this.ReadShort(0x96) };
        }

        private int ReadShort(byte address)
        {
            this.writeBuffer[0] = address;

            this.i2c.WriteRead(this.writeBuffer, this.readBuffer);

            return this.readBuffer[0] | this.readBuffer[1] << 8;
        }
    }
}