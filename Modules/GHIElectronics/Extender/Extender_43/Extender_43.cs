using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// An Extender module for Microsoft .NET Gadgeteer.
    /// </summary>
    public class Extender : GTM.Module
    {
        private Socket socketA;
        private Socket socketB;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The mainboard socket that has the module plugged into it.</param>
        public Extender(int socketNumber)
        {
            this.socketA = Socket.GetSocket(socketNumber, true, this, null);

            this.socketB = Socket.SocketInterfaces.CreateUnnumberedSocket(socketNumber.ToString() + "-" + " Extender");
            this.socketB.SupportedTypes = this.socketA.SupportedTypes;

            for (int i = 3; i < 10; i++)
                this.socketB.CpuPins[i] = this.socketA.CpuPins[i];

            this.socketB.SerialPortName = this.socketA.SerialPortName;
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
        public int ExtenderSocketB { get { return this.socketB.SocketNumber; } }

        /// <summary>
        /// Creates a digital input on the given pin.
        /// </summary>
        /// <param name="pin">The pin to create the interface on.</param>
        /// <param name="glitchFilterMode">The glitch filter mode for the interface.</param>
        /// <param name="resistorMode">The resistor mode for the interface.</param>
        /// <returns>The new interface.</returns>
        public GTI.DigitalInput CreateDigitalInput(Socket.Pin pin, GTI.GlitchFilterMode glitchFilterMode, GTI.ResistorMode resistorMode)
        {
            return GTI.DigitalInputFactory.Create(this.socketB, pin, glitchFilterMode, resistorMode, this);
        }

        /// <summary>
        /// Creates a digital output on the given pin.
        /// </summary>
        /// <param name="pin">The pin to create the interface on.</param>
        /// <param name="initialState">The initial state for the interface.</param>
        /// <returns>The new interface.</returns>
        public GTI.DigitalOutput CreateDigitalOutput(Socket.Pin pin, bool initialState)
        {
            return GTI.DigitalOutputFactory.Create(this.socketB, pin, initialState, this);
        }

        /// <summary>
        /// Creates a digital input/output on the given pin.
        /// </summary>
        /// <param name="pin">The pin to create the interface on.</param>
        /// <param name="initialState">The initial state for the interface.</param>
        /// <param name="glitchFilterMode">The glitch filter mode for the interface.</param>
        /// <param name="resistorMode">The resistor mode for the interface.</param>
        /// <returns>The new interface.</returns>
        public GTI.DigitalIO CreateDigitalIO(Socket.Pin pin, bool initialState, GTI.GlitchFilterMode glitchFilterMode, GTI.ResistorMode resistorMode)
        {
            return GTI.DigitalIOFactory.Create(this.socketB, pin, initialState, glitchFilterMode, resistorMode, this);
        }

        /// <summary>
        /// Creates an interrupt input on the given pin.
        /// </summary>
        /// <param name="pin">The pin to create the interface on.</param>
        /// <param name="glitchFilterMode">The glitch filter mode for the interface.</param>
        /// <param name="resistorMode">The resistor mode for the interface.</param>
        /// <param name="interruptMode">The interrupt mode for the interface.</param>
        /// <returns>The new interface.</returns>
        public GTI.InterruptInput CreateInterruptInput(Socket.Pin pin, GTI.GlitchFilterMode glitchFilterMode, GTI.ResistorMode resistorMode, GTI.InterruptMode interruptMode)
        {
            return GTI.InterruptInputFactory.Create(this.socketB, pin, glitchFilterMode, resistorMode, interruptMode, this);
        }

        /// <summary>
        /// Creates an analog input on the given pin.
        /// </summary>
        /// <param name="pin">The pin to create the interface on.</param>
        /// <returns>The new interface.</returns>
        public GTI.AnalogInput CreateAnalogInput(Socket.Pin pin)
        {
            return GTI.AnalogInputFactory.Create(this.socketB, pin, this);
        }

        /// <summary>
        /// Creates an analog output on the given pin.
        /// </summary>
        /// <param name="pin">The pin to create the interface on.</param>
        /// <returns>The new interface.</returns>
        public GTI.AnalogOutput CreateAnalogOutput(Socket.Pin pin)
        {
            return GTI.AnalogOutputFactory.Create(this.socketB, pin, this);
        }

        /// <summary>
        /// Creates a pwm output on the given pin.
        /// </summary>
        /// <param name="pin">The pin to create the interface on.</param>
        /// <returns>The new interface.</returns>
        public GTI.PwmOutput CreatePwmOutput(Socket.Pin pin)
        {
            return GTI.PwmOutputFactory.Create(this.socketB, pin, false, this);
        }
    }
}