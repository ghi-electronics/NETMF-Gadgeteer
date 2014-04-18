
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A NullModem module for Microsoft .NET Gadgeteer
    /// </summary>
    public class NullModem : GTM.Module
    {
        private GT.Socket socketA;
        private GT.Socket socketB;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public NullModem(int socketNumber)
        {
            this.socketA = Socket.GetSocket(socketNumber, true, this, null);
            this.socketA.EnsureTypeIsSupported('U', this);

            this.socketB = Socket.SocketInterfaces.CreateUnnumberedSocket(socketNumber.ToString() + "-" + " NullModem");
            this.socketB.SupportedTypes = new char[] { 'U' };

            this.socketB.CpuPins[3] = this.socketA.CpuPins[3];
            this.socketB.CpuPins[4] = this.socketA.CpuPins[5];
            this.socketB.CpuPins[5] = this.socketA.CpuPins[4];
            this.socketB.CpuPins[6] = this.socketA.CpuPins[7];
            this.socketB.CpuPins[7] = this.socketA.CpuPins[6];

            this.socketB.SerialPortName = this.socketA.SerialPortName;

            Socket.SocketInterfaces.RegisterSocket(this.socketB);
        }

        /// <summary>
        /// Returns the socket number for socket on the module.
        /// </summary>
        public int NullModemSocketB { get { return this.socketB.SocketNumber; } }
    }
}
