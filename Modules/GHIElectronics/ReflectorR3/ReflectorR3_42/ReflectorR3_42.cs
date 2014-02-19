using GTI = Gadgeteer.Interfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A Reflector R3 module for Microsoft .NET Gadgeteer
	/// </summary>
	public class ReflectorR3 : GTM.Module
	{
		private GTI.AnalogInput left;
		private GTI.AnalogInput center;
		private GTI.AnalogInput right;
		private GTI.DigitalOutput centerSwitch;

		/// <summary></summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public ReflectorR3(int socketNumber)
		{
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);

			this.left = new GTI.AnalogInput(socket, Socket.Pin.Three, this);
			this.center = new GTI.AnalogInput(socket, Socket.Pin.Four, this);
			this.right = new GTI.AnalogInput(socket, Socket.Pin.Five, this);
			this.centerSwitch = new GTI.DigitalOutput(socket, Socket.Pin.Six, true, this);
		}

		/// <summary>
		/// The reflectors on the module.
		/// </summary>
		public enum Reflectors
		{
			/// <summary>
			/// The left reflector.
			/// </summary>
			Left,
			/// <summary>
			/// The center reflector.
			/// </summary>
			Center,
			/// <summary>
			/// The right reflector.
			/// </summary>
			Right
		}

		/// <summary>
		/// Gets the reflective reading from one of the reflectors.
		/// </summary>
		/// <param name="reflector">The reflector to read from.</param>
		/// <returns>A number between 0 and 1 where 0 is no reflection and 1 is maximum reflection.</returns>
		public double Read(Reflectors reflector)
		{
			switch (reflector)
			{
				case Reflectors.Left: return 1 - this.left.ReadProportion();
				case Reflectors.Center: return 1 - this.center.ReadProportion();
				case Reflectors.Right: return 1 - this.right.ReadProportion();
				default: return 0;
			}
		}
	}
}
