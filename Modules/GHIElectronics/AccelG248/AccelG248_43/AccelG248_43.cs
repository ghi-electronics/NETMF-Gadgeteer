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
        /// <returns>The X acceleration between -1 and 1.</returns>
        public double GetX()
        {
            var data = this.ReadRegister(0x01, 2);

            double value = (data[0] << 2) | (data[1] >> 6);

            if (value > 511.0)
                value = value - 1024.0;

            value /= 512.0;

            return value;
        }

        /// <summary>
        /// Gets the current Y acceleration value.
        /// </summary>
        /// <returns>The Y acceleration between -1 and 1.</returns>
        public double GetY()
        {
            var data = this.ReadRegister(0x03, 2);

            double value = (data[0] << 2) | (data[1] >> 6);

            if (value > 511.0)
                value = value - 1024.0;

            value /= 512.0;

            return value;
        }

        /// <summary>
        /// Gets the current Z acceleration value.
        /// </summary>
        /// <returns>The Z acceleration between -1 and 1.</returns>
        public double GetZ()
        {
            var data = this.ReadRegister(0x05, 2);

            double value = (data[0] << 2) | (data[1] >> 6);

            if (value > 511.0)
                value = value - 1024.0;

            value /= 512.0;

            return value;
        }

        /// <summary>
        /// Gets the current acceleration values.
        /// </summary>
        /// <returns>The acceleration.</returns>
        public Acceleration GetAcceleration()
        {
            double x, y, z;

            this.GetXYZ(out x, out y, out z);

            return new Acceleration { X = x, Y = y, Z = z };
        }

        /// <summary>
        /// Gets the current acceleration values.
        /// </summary>
        /// <param name="x">The x acceleration between -1 and 1.</param>
        /// <param name="y">The y acceleration between -1 and 1.</param>
        /// <param name="z">The z acceleration between -1 and 1.</param>
        public void GetXYZ(out double x, out double y, out double z)
        {
            var data = this.ReadRegister(0x01, 6);

            x = (data[0] << 2) | (data[1] >> 6);
            y = (data[2] << 2) | (data[3] >> 6);
            z = (data[4] << 2) | (data[5] >> 6);

            if (x > 511.0)
                x -= 1024.0;

            if (y > 511.0)
                y -= 1024.0;

            if (z > 511.0)
                z -= 1024.0;

            x /= 512.0;
            y /= 512.0;
            z /= 512.0;
        }

        /// <summary>
        /// Represents an acceleration.
        /// </summary>
        public struct Acceleration
        {
            /// <summary>
            /// The x acceleration between -1 and 1.
            /// </summary>
            public double X { get; set; }

            /// <summary>
            /// The y acceleration between -1 and 1.
            /// </summary>
            public double Y { get; set; }

            /// <summary>
            /// The z acceleration between -1 and 1.
            /// </summary>
            public double Z { get; set; }

            /// <summary>
            /// Returns the string representation of this object.
            /// </summary>
            /// <returns>The string representation.</returns>
            public override string ToString()
            {
                return "(" + this.X.ToString("F2") + ", " + this.Y.ToString("F2") + ", " + this.Z.ToString("F2") + ")";
            }
        }
    }
}
