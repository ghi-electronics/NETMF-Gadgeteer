using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2C), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// Represents a cable extender module which can also be used as a breakout module to interface with custom electronics, or a snooping module to monitor signals on individual pins.
    /// </summary>
    /// /// <example>
    /// <para>The following example uses a <see cref="Extender"/> object to break out individual pins on a Gadgeteer socket. 
    /// This module simply provides the interfaces available on the socket (GPIO, PWM, etc).
    /// </para>
    /// </example>
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
            this.socketB.CpuPins[3] = this.socketA.CpuPins[3];
            this.socketB.CpuPins[4] = this.socketA.CpuPins[5];
            this.socketB.CpuPins[5] = this.socketA.CpuPins[4];
            this.socketB.CpuPins[6] = this.socketA.CpuPins[7];
            this.socketB.CpuPins[7] = this.socketA.CpuPins[6];
            this.socketB.SerialPortName = this.socketA.SerialPortName;
            this.socketB.SPIModule = this.socketA.SPIModule;
            this.socketB.AnalogOutput = this.socketA.AnalogOutput;
            this.socketB.AnalogInput3 = this.socketA.AnalogInput3;
            this.socketB.AnalogInput4 = this.socketA.AnalogInput4;
            this.socketB.AnalogInput5 = this.socketA.AnalogInput5;
            this.socketB.PWM7 = this.socketA.PWM7;
            this.socketB.PWM8 = this.socketA.PWM8;
            this.socketB.PWM9 = this.socketA.PWM9;
            this.socketB.AnalogInputIndirector = this.socketA.AnalogInputIndirector;
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
        /// Returns a digital input interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <param name="glitchFilterMode">
        ///  A value from the <see cref="T:Microsoft.Gadgeteer.Interfaces.GlitchFilterMode"/> enumeration that specifies 
        ///  whether to enable the glitch filter on this interface.
        /// </param>
        /// <param name="resistorMode">The resistor mode for the interface port.</param>
        /// <returns>The interface.</returns>
        /// <exception cref="System.Exception">
        ///  The specified pin has already been reserved on this module.
        /// </exception>
        public Interfaces.DigitalInput SetupDigitalInput(Socket.Pin pin, Interfaces.GlitchFilterMode glitchFilterMode, Interfaces.ResistorMode resistorMode)
        {
            return new Interfaces.DigitalInput(this.socketB, pin, glitchFilterMode, resistorMode, this);
        }

        /// <summary>
        /// Returns a digital output interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <param name="initialState">The initial state to place on the interface output port.</param>
        /// <returns>The interface.</returns>
        /// <exception cref="System.Exception">
        ///  The specified pin has already been reserved on this module.
        /// </exception>
        public Interfaces.DigitalOutput SetupDigitalOutput(Socket.Pin pin, bool initialState)
        {
            return new Interfaces.DigitalOutput(this.socketB, pin, initialState, this);
        }

        /// <summary>
        /// Returns a digital input/output interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <param name="initialState">
        ///  The initial state to place on the interface port; 
        ///  this value becomes effective as soon as the port is enabled as an output port.
        /// </param>
        /// <param name="glitchFilterMode">
        ///  A value from the <see cref="T:Microsoft.Gadgeteer.Interfaces.GlitchFilterMode"/> enumeration that specifies 
        ///  whether to enable the glitch filter on this interface.
        /// </param>
        /// <param name="resistorMode">The resistor mode for the interface port.</param>
        /// <returns>The interface.</returns>
        /// <exception cref="System.Exception">
        ///  The specified pin has already been reserved on this module.
        /// </exception>
        public Interfaces.DigitalIO SetupDigitalIO(Socket.Pin pin, bool initialState, Interfaces.GlitchFilterMode glitchFilterMode, Interfaces.ResistorMode resistorMode)
        {
            return new Interfaces.DigitalIO(this.socketB, pin, initialState, glitchFilterMode, resistorMode, this);
        }

        /// <summary>
        /// Returns an interrupt input interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <param name="glitchFilterMode">
        ///  A value from the <see cref="T:Microsoft.Gadgeteer.Interfaces.GlitchFilterMode"/> enumeration that specifies 
        ///  whether to enable the glitch filter on this interface.
        /// </param>
        /// <param name="resistorMode">The resistor mode for the interface port.</param>
        /// <param name="interruptMode">The interrupt mode for the interface port.</param>
        /// <returns>The interface</returns>
        /// <exception cref="System.Exception">
        ///  The specified pin has already been reserved on this module.
        /// </exception>
        public Interfaces.InterruptInput SetupInterruptInput(Socket.Pin pin, Interfaces.GlitchFilterMode glitchFilterMode, Interfaces.ResistorMode resistorMode, Interfaces.InterruptMode interruptMode)
        {
            return new Interfaces.InterruptInput(this.socketB, pin, glitchFilterMode, resistorMode, interruptMode, this);
        }

        /// <summary>
        /// Returns an analog input interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <returns>The interface.</returns>
        public Interfaces.AnalogInput SetupAnalogInput(Socket.Pin pin)
        {
            return new Interfaces.AnalogInput(this.socketB, pin, this);
        }

        // TODO:  Determine whether AnalogOutput should be added to Gadgeteer.Interfaces


        /// <summary>
        /// Returns an analog output interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <returns>The interface.</returns>
        public Interfaces.AnalogOutput SetupAnalogOutput(Socket.Pin pin)
        {
            return new Interfaces.AnalogOutput(this.socketB, pin, this);
        }

        /// <summary>
        ///  Returns an pulse width modulation (PWM) output interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to use for the PWM interface.</param>
        /// <returns>The PWM interface.</returns>
        public Interfaces.PWMOutput SetupPWMOutput(Socket.Pin pin)
        {
            return new Interfaces.PWMOutput(this.socketB, pin, false, this);
        }
    }
}