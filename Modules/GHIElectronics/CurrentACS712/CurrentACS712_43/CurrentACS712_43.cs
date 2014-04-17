using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A CurrentACS712 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class CurrentACS712 : GTM.Module
    {
        private GTI.AnalogInput input;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public CurrentACS712(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('A', this);

            this.input = GTI.AnalogInputFactory.Create(socket, Socket.Pin.Five, this);
        }

        /// <summary>
        /// Reads the alternating current value.
        /// </summary>
        /// <returns>The AC reading.</returns>
        public double ReadACCurrent()
        {
            double sum = 0.0;

            for (int i = 0; i < 400; i++)
                sum += this.input.ReadProportion();

            sum /= 400;

            return 21.2 * sum - 13.555;
        }

        /// <summary>
        /// Reads the direct current value.
        /// </summary>
        /// <returns>The DC reading.</returns>
        public double ReadDCCurrent()
        {
            double read = 0.0;
            double calculation = 0.0;

            for (int i = 0; i < 400; i++)
            {
                read = this.input.ReadProportion();
                read = 21.3 * read - 13.555;
                read *= read < 0 ? -1 : 1;

                if (calculation < read)
                    calculation = read;
            }

            calculation /= 1.41421356;

            return calculation;
        }
    }
}
