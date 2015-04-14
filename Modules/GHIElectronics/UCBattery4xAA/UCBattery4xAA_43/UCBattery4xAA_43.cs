using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A UCBattery4xAA module for Microsoft .NET Gadgeteer</summary>
	public class UCBattery4xAA : GTM.Module {
		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public UCBattery4xAA(int socketNumber) {
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('D', this);

			socket.ReservePin(Socket.Pin.Four, this);
			socket.ReservePin(Socket.Pin.Five, this);
		}
	}
}