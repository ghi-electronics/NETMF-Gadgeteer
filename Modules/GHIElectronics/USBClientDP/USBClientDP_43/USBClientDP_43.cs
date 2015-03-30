using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A USBClientDP module for Microsoft .NET Gadgeteer</summary>
	public class USBClientDP : GTM.Module {

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The mainboard socket that has the module plugged into it.</param>
		public USBClientDP(int socketNumber) {
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('D', this);

			socket.ReservePin(Socket.Pin.Three, this);
			socket.ReservePin(Socket.Pin.Four, this);
			socket.ReservePin(Socket.Pin.Five, this);
			socket.ReservePin(Socket.Pin.Six, this);
			socket.ReservePin(Socket.Pin.Seven, this);
		}
	}
}