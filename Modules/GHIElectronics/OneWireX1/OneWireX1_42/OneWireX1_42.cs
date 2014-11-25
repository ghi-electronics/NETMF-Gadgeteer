using Microsoft.SPOT.Hardware;
using System;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A OneWire X1 module for Microsoft .NET Gadgeteer
	/// </summary>
	[Obsolete]
	public class OneWireX1 : GTM.Module
	{
		private OneWire oneWire;
		private OutputPort port;

		/// <summary>Constructs a new OneWireX1 module.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public OneWireX1(int socketNumber)
		{
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);

			socket.EnsureTypeIsSupported(new char[] { 'X', 'Y' }, this);
			socket.ReservePin(Socket.Pin.Three, this);

			this.port = new OutputPort(socket.CpuPins[(int)Socket.Pin.Three], false);
			this.oneWire = new Microsoft.SPOT.Hardware.OneWire(this.port);
		}

		/// <summary>
		/// Returns the native NETMF OneWire interface object.
		/// </summary>
		public OneWire Interface
		{
			get
			{
				return this.oneWire;
			}
		}
	}
}
