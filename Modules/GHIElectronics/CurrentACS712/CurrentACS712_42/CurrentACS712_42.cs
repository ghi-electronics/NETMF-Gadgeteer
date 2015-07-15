using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;
using System;

namespace Gadgeteer.Modules.GHIElectronics {
    /// <summary>
    /// A Current ACS712 module for Microsoft .NET Gadgeteer
    /// </summary>
    [Obsolete]
    public class CurrentACS712 : GTM.Module {
        // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
        // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2C), you must do a few more steps
        // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
        // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
        // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

        private GTI.AnalogInput ain;
        private const int AC_SAMPLE_COUNT = 400;

        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public CurrentACS712(int socketNumber) {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('A', this);

            ain = new GTI.AnalogInput(socket, Socket.Pin.Five, this);
        }

        /// <summary>
        /// Returns a reading of the measured AC current.
        /// </summary>
        /// <returns>AC current reading.</returns>
        public double Read_AC_Current() {
            double read = 0.0;
            double calculation = 0.0;

            for (int i = 0; i < 400; i++) {
                read += ain.ReadProportion();
            }

            read /= 400;
            calculation = 21.2 * read - 13.555;

            return calculation;
        }

        /// <summary>
        /// Returns a reading of the measured DC current.
        /// </summary>
        /// <returns>DC current reading.</returns>
        public double Read_DC_Current() {
            double read = 0.0;
            double calculation = 0.0;

            for (int i = 0; i < 400; i++) {
                read = ain.ReadProportion();
                read = 21.3 * read - 13.555;
                read = read < 0 ? read * -1 : read;

                if (calculation < read)
                    calculation = read;
            }

            calculation /= 1.41421356;

            return calculation;
        }
    }
}
