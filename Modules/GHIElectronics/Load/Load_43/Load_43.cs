using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A module that has seven 3Amp 50V digital switches for Microsoft .NET Gadgeteer.</summary>
	public class Load : GTM.Module {

		/// <summary>Pin 1 on the module.</summary>
		public GTI.DigitalOutput P1 { get; private set; }

		/// <summary>Pin 2 on the module.</summary>
		public GTI.DigitalOutput P2 { get; private set; }

		/// <summary>Pin 3 on the module.</summary>
		public GTI.DigitalOutput P3 { get; private set; }

		/// <summary>Pin 4 on the module.</summary>
		public GTI.DigitalOutput P4 { get; private set; }

		/// <summary>Pin 5 on the module.</summary>
		public GTI.DigitalOutput P5 { get; private set; }

		/// <summary>Pin 6 on the module.</summary>
		public GTI.DigitalOutput P6 { get; private set; }

		/// <summary>Pin 7 on the module.</summary>
		public GTI.DigitalOutput P7 { get; private set; }

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public Load(int socketNumber) {
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('Y', this);

			this.P1 = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three, false, this);
			this.P2 = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Four, false, this);
			this.P3 = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Five, false, this);
			this.P4 = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Six, false, this);
			this.P5 = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Seven, false, this);
			this.P6 = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Eight, false, this);
			this.P7 = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Nine, false, this);
		}
	}
}