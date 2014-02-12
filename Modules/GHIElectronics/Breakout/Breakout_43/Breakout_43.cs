using System;

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
    /// Represents a breakout module to interface with custom electronics.
    /// </summary>
    public class Breakout : GTM.Module
    {
        private Socket BreakoutSocket;

        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public Breakout(int socketNumber)
        {
            BreakoutSocket = Socket.GetSocket(socketNumber, true, this, null);
        }

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
            return SocketInterfaces.DigitalInputFactory.Create(BreakoutSocket, pin, glitchFilterMode, resistorMode, this);
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
            return SocketInterfaces.DigitalOutputFactory.Create(BreakoutSocket, pin, initialState, this);
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
            return SocketInterfaces.DigitalIOFactory.Create(BreakoutSocket, pin, initialState, glitchFilterMode, resistorMode, this);
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
            return SocketInterfaces.InterruptInputFactory.Create(BreakoutSocket, pin, glitchFilterMode, resistorMode, interruptMode, this);
        }

        /// <summary>
        /// Returns an analog input interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <returns>The interface.</returns>
        public SocketInterfaces.AnalogInput SetupAnalogInput(Socket.Pin pin)
        {
            return SocketInterfaces.AnalogInputFactory.Create(BreakoutSocket, pin, this);
        }

        // TODO:  Determine whether AnalogOutput should be added to Gadgeteer.SocketInterfaces


        /// <summary>
        /// Returns an analog output interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to assign to the interface.</param>
        /// <returns>The interface.</returns>
        public SocketInterfaces.AnalogOutput SetupAnalogOutput(Socket.Pin pin)
        {
            return SocketInterfaces.AnalogOutputFactory.Create(BreakoutSocket, pin, this);
        }

        /// <summary>
        ///  Returns an pulse width modulation (PWM) output interface associated with the specified pin on this module.
        /// </summary>
        /// <param name="pin">The pin to use for the PWM interface.</param>
        /// <returns>The PWM interface.</returns>
        public SocketInterfaces.PwmOutput SetupPWMOutput(Socket.Pin pin)
        {
            return SocketInterfaces.PwmOutputFactory.Create(BreakoutSocket, pin, false, this);
        }
    }
}
