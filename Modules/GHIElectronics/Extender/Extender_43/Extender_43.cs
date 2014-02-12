using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2CBus), you must do a few more steps
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
        private Socket ExtenderSocket;

        /// <summary></summary>
        /// <param name="socketNumber">The mainboard socket that has the module plugged into it.</param>
        public Extender(int socketNumber)
        {
            ExtenderSocket = Socket.GetSocket(socketNumber, true, this, null);
        }

        /// <summary>
        /// The mainboard socket number which this Extender module is plugged into.
        /// </summary>
        public int ExtenderSocketNumber { get { return ExtenderSocket.SocketNumber; } }

        /// <summary>
        /// Returns a digital input interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <param name="glitchFilterMode">
        ///  A value from the <see cref="T:Microsoft.Gadgeteer.SocketInterfaces.GlitchFilterMode"/> enumeration that specifies 
        ///  whether to enable the glitch filter on this interface.
        /// </param>
        /// <param name="resistorMode">The resistor mode for the interface port.</param>
        /// <returns>The interface.</returns>
        /// <exception cref="System.Exception">
        ///  The specified pin has already been reserved on this module.
        /// </exception>
        public SocketInterfaces.DigitalInput SetupDigitalInput(Socket.Pin pin, SocketInterfaces.GlitchFilterMode glitchFilterMode, SocketInterfaces.ResistorMode resistorMode)
        {
            return SocketInterfaces.DigitalInputFactory.Create(ExtenderSocket, pin, glitchFilterMode, resistorMode, this);
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
        public SocketInterfaces.DigitalOutput SetupDigitalOutput(Socket.Pin pin, bool initialState)
        {
            return SocketInterfaces.DigitalOutputFactory.Create(ExtenderSocket, pin, initialState, this);
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
        ///  A value from the <see cref="T:Microsoft.Gadgeteer.SocketInterfaces.GlitchFilterMode"/> enumeration that specifies 
        ///  whether to enable the glitch filter on this interface.
        /// </param>
        /// <param name="resistorMode">The resistor mode for the interface port.</param>
        /// <returns>The interface.</returns>
        /// <exception cref="System.Exception">
        ///  The specified pin has already been reserved on this module.
        /// </exception>
        public SocketInterfaces.DigitalIO SetupDigitalIO(Socket.Pin pin, bool initialState, SocketInterfaces.GlitchFilterMode glitchFilterMode, SocketInterfaces.ResistorMode resistorMode)
        {
            return SocketInterfaces.DigitalIOFactory.Create(ExtenderSocket, pin, initialState, glitchFilterMode, resistorMode, this);
        }

        /// <summary>
        /// Returns an interrupt input interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <param name="glitchFilterMode">
        ///  A value from the <see cref="T:Microsoft.Gadgeteer.SocketInterfaces.GlitchFilterMode"/> enumeration that specifies 
        ///  whether to enable the glitch filter on this interface.
        /// </param>
        /// <param name="resistorMode">The resistor mode for the interface port.</param>
        /// <param name="interruptMode">The interrupt mode for the interface port.</param>
        /// <returns>The interface</returns>
        /// <exception cref="System.Exception">
        ///  The specified pin has already been reserved on this module.
        /// </exception>
        public SocketInterfaces.InterruptInput SetupInterruptInput(Socket.Pin pin, SocketInterfaces.GlitchFilterMode glitchFilterMode, SocketInterfaces.ResistorMode resistorMode, SocketInterfaces.InterruptMode interruptMode)
        {
            return SocketInterfaces.InterruptInputFactory.Create(ExtenderSocket, pin, glitchFilterMode, resistorMode, interruptMode, this);
        }

        /// <summary>
        /// Returns an analog input interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <returns>The interface.</returns>
        public SocketInterfaces.AnalogInput SetupAnalogInput(Socket.Pin pin)
        {
            return SocketInterfaces.AnalogInputFactory.Create(ExtenderSocket, pin, this);
        }

        // TODO:  Determine whether AnalogOutput should be added to Gadgeteer.SocketInterfaces


        /// <summary>
        /// Returns an analog output interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <returns>The interface.</returns>
        public SocketInterfaces.AnalogOutput SetupAnalogOutput(Socket.Pin pin)
        {
            return SocketInterfaces.AnalogOutputFactory.Create(ExtenderSocket, pin, this);
        }

        /// <summary>
        ///  Returns an pulse width modulation (PWM) output interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to use for the PWM interface.</param>
        /// <returns>The PWM interface.</returns>
        public SocketInterfaces.PwmOutput SetupPWMOutput(Socket.Pin pin)
        {
            return SocketInterfaces.PwmOutputFactory.Create(ExtenderSocket, pin, false, this);
        }
    }
}