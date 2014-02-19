using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2CBus), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// A AccelG248 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class AccelG248 : GTM.Module
    {
        private GTI.I2CBus i2c;

        /// <summary>Constructor</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public AccelG248(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('I', this);

            i2c = GTI.I2CBusFactory.Create(socket, 0x1C, 400, this);

            WriteRegister(0x2A, 1);
        }

        private void WriteRegister(byte reg, byte value)
        {
            byte[] RegisterNum = new byte[2] { reg, value };
            i2c.Write(RegisterNum);
        }

        private byte[] ReadRegister(byte reg, int readcount)
        {
            byte[] RegisterNum = new byte[1] { reg };

            // create read buffer to read the register
            byte[] RegisterValue = new byte[readcount];

            //int out_num_write = 0;
            //int out_num_read = 0;

            //i2cdev.WriteRead(RegisterNum, 0, 1, RegisterValue, 0, readcount, out out_num_write, out out_num_read);
            i2c.WriteRead(RegisterNum, RegisterValue);

            return RegisterValue;
        }

        /// <summary>
        /// Returns X acceleration. 
        /// </summary>
        /// <returns>X acceleration</returns>
        public int GetX()
        {
            byte[] test = ReadRegister(0x1, 2);

            int X = test[0] << 2 | test[1] >> 6 & 0x3F;

            if (X > 511)
                X = X - 1024;

            return X;
        }

        /// <summary>
        /// Returns Y acceleration.
        /// </summary>
        /// <returns>Y acceleration</returns>
        public int GetY()
        {
            byte[] test = ReadRegister(0x3, 2);

            int Y = test[0] << 2 | test[1] >> 6 & 0x3F;

            if (Y > 511)
                Y = Y - 1024;

            return Y;
        }

        /// <summary>
        /// Returns Z acceleration.
        /// </summary>
        /// <returns>Z acceleration</returns>
        public int GetZ()
        {
            byte[] test = ReadRegister(0x5, 2);

            int Z = test[0] << 2 | test[1] >> 6 & 0x3F;

            if (Z > 511)
                Z = Z - 1024;

            return Z;
        }

        /// <summary>
        /// Obtains the X, Y, and Z accelerations.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        public void GetXYZ(out int X, out int Y, out int Z)
        {
            byte[] test = ReadRegister(0x1, 6);

            X = test[0] << 2 | test[1] >> 6 & 0x3F;
            Y = test[2] << 2 | test[3] >> 6 & 0x3F;
            Z = test[4] << 2 | test[5] >> 6 & 0x3F;

            if (X > 511)
                X -= 1024;

            if (Y > 511)
                Y -= 1024;

            if (Z > 511)
                Z -= 1024;
        }
    }
}
