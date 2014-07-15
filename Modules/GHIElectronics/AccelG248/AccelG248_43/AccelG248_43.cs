using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// An AccelG248 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class AccelG248 : GTM.Module
    {
        private GTI.I2CBus i2c;
        private byte[] buffer;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public AccelG248(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('I', this);

            this.buffer = new byte[1];
            this.i2c = GTI.I2CBusFactory.Create(socket, 0x1C, 400, this);
            this.i2c.Write(0x2A, 0x01);
        }

        private byte[] ReadRegister(byte register, int count)
        {
            byte[] result = new byte[count];

            this.buffer[0] = register;

            this.i2c.WriteRead(this.buffer, result);

            return result;
        }

        /// <summary>
        /// Gets the current X acceleration value.
        /// </summary>
        /// <returns>The X acceleration.</returns>
        public int GetX()
        {
            var data = this.ReadRegister(0x01, 2);

            int value = ((data[0] << 2) | (data[1] >> 6)) & 0x3F;

            if (value > 511)
                value = value - 1024;

            return value;
        }

        /// <summary>
        /// Gets the current Y acceleration value.
        /// </summary>
        /// <returns>The Y acceleration.</returns>
        public int GetY()
        {
            var data = this.ReadRegister(0x03, 2);

            int value = data[0] << 2 | data[1] >> 6 & 0x3F;

            if (value > 511)
                value = value - 1024;

            return value;
        }

        /// <summary>
        /// Gets the current Z acceleration value.
        /// </summary>
        /// <returns>The Z acceleration.</returns>
        public int GetZ()
        {
            var data = this.ReadRegister(0x05, 2);

            int value = data[0] << 2 | data[1] >> 6 & 0x3F;

            if (value > 511)
                value = value - 1024;

            return value;
        }

        /// <summary>
        /// Gets the current acceleration values.
        /// </summary>
        /// <returns>The acceleration.</returns>
        public Acceleration GetAcceleration()
        {
            int x, y, z;

            this.GetXYZ(out x, out y, out z);

            return new Acceleration { X = x, Y = y, Z = z };
        }

        /// <summary>
        /// Gets the current acceleration values.
        /// </summary>
        /// <param name="x">The x acceleration.</param>
        /// <param name="y">The y acceleration.</param>
        /// <param name="z">The z acceleration.</param>
        public void GetXYZ(out int x, out int y, out int z)
        {
            var data = this.ReadRegister(0x01, 6);

            x = data[0] << 2 | data[1] >> 6 & 0x3F;
            y = data[2] << 2 | data[3] >> 6 & 0x3F;
            z = data[4] << 2 | data[5] >> 6 & 0x3F;

            if (x > 511)
                x -= 1024;

            if (y > 511)
                y -= 1024;

            if (z > 511)
                z -= 1024;
        }

        /// <summary>
        /// Represents an acceleration.
        /// </summary>
        public struct Acceleration
        {
            /// <summary>
            /// The x acceleration.
            /// </summary>
            public int X { get; set; }

            /// <summary>
            /// The y acceleration.
            /// </summary>
            public int Y { get; set; }

            /// <summary>
            /// The z acceleration.
            /// </summary>
            public int Z { get; set; }

            /// <summary>
            /// Returns the string representation of this object.
            /// </summary>
            /// <returns>The string representation.</returns>
            public override string ToString()
            {
                return "(" + this.X.ToString() + ", " + this.Y.ToString() + ", " + this.Z.ToString() + ")";
            }
        }
    }
}
