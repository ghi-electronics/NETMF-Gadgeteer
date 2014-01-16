using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// Represents a BreadBoard_X1 module to interface with custom electronics.
	/// </summary>
	public class BreadBoard_X1 : GTM.Module
	{
		private Socket socket;

		/// <summary></summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public BreadBoard_X1(int socketNumber)
		{
			this.socket = Socket.GetSocket(socketNumber, true, this, null);
		}

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
			return new Interfaces.DigitalInput(this.socket, pin, glitchFilterMode, resistorMode, this);
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
			return new Interfaces.DigitalOutput(this.socket, pin, initialState, this);
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
			return new Interfaces.DigitalIO(this.socket, pin, initialState, glitchFilterMode, resistorMode, this);
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
			return new Interfaces.InterruptInput(this.socket, pin, glitchFilterMode, resistorMode, interruptMode, this);
		}

		/// <summary>
		/// Returns an analog input interface associated with the specified pin on this module.
		/// </summary>
		/// <param name="pin">The pin to assign to the interface.</param>
		/// <returns>The interface.</returns>
		public Interfaces.AnalogInput SetupAnalogInput(Socket.Pin pin)
		{
			return new Interfaces.AnalogInput(this.socket, pin, this);
		}

		/// <summary>
		/// Returns an analog output interface associated with the specified pin on this module.
		/// </summary>
		/// <param name="pin">The pin to assign to the interface.</param>
		/// <returns>The interface.</returns>
		public Interfaces.AnalogOutput SetupAnalogOutput(Socket.Pin pin)
		{
			return new Interfaces.AnalogOutput(this.socket, pin, this);
		}

		/// <summary>
		///  Returns an pulse width modulation (PWM) output interface associated with the specified pin on this module.
		/// </summary>
		/// <param name="pin">The pin to use for the PWM interface.</param>
		/// <param name="invert">Whether or not to invert the PWM signal.</param>
		/// <returns>The PWM interface.</returns>
		public Interfaces.PWMOutput SetupPWMOutput(Socket.Pin pin, bool invert = false)
		{
			return new Interfaces.PWMOutput(this.socket, pin, invert, this);
		}
	}
}
