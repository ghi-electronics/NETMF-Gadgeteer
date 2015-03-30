using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A USBClientEDP module for Microsoft .NET Gadgeteer</summary>
	public class USBClientEDP : GTM.Module {

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public USBClientEDP(int socketNumber) {
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