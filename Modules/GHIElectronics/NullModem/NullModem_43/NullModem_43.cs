
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
            this.socketB.CpuPins[8] = this.socketA.CpuPins[8];
            this.socketB.CpuPins[9] = this.socketA.CpuPins[9];
            this.socketB.SPIModule = this.socketA.SPIModule;
            this.socketB.AnalogOutput5 = this.socketA.AnalogOutput5;
            this.socketB.AnalogInput3 = this.socketA.AnalogInput3;
            this.socketB.AnalogInput4 = this.socketA.AnalogInput4;
            this.socketB.AnalogInput5 = this.socketA.AnalogInput5;
            this.socketB.PWM7 = this.socketA.PWM7;
            this.socketB.PWM8 = this.socketA.PWM8;
            this.socketB.PWM9 = this.socketA.PWM9;
            this.socketB.AnalogInputIndirector = this.socketA.AnalogInputIndirector;
            this.socketB.AnalogOutputIndirector = this.socketA.AnalogOutputIndirector;
            this.socketB.DigitalInputIndirector = this.socketA.DigitalInputIndirector;
            this.socketB.DigitalIOIndirector = this.socketA.DigitalIOIndirector;
            this.socketB.DigitalOutputIndirector = this.socketA.DigitalOutputIndirector;
            this.socketB.I2CBusIndirector = this.socketA.I2CBusIndirector;
            this.socketB.InterruptIndirector = this.socketA.InterruptIndirector;
            this.socketB.PwmOutputIndirector = this.socketA.PwmOutputIndirector;
            this.socketB.SpiIndirector = this.socketA.SpiIndirector;
            this.socketB.SerialIndirector = this.socketA.SerialIndirector;

            Socket.SocketInterfaces.RegisterSocket(this.socketB);
        }

        /// <summary>
        /// Returns the socket number for socket on the module.
        /// </summary>
        public int NullModemSocketB { get { return this.socketB.SocketNumber; } }
    }
}
