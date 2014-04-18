using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A DAQ8B module for Microsoft .NET Gadgeteer
	/// </summary>
	public class DAQ8B : GTM.Module
	{
		private GTI.DigitalInput miso;
		private GTI.DigitalOutput mosi;
		private GTI.DigitalOutput clock;
		private GTI.DigitalOutput cs;

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public DAQ8B(int socketNumber)
		{
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('S', this);

			this.cs = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Six, true, this);
			this.miso = GTI.DigitalInputFactory.Create(socket, Socket.Pin.Eight, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, this);
			this.mosi = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Seven, false, this);
			this.clock = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Nine, false, this);
		}

		/// <summary>
		/// The channels on the board.
		/// </summary>
		public enum Channel
		{
			/// <summary>
			/// The channel marked P1 on the board.
			/// </summary>
			P1 = 0x0200,
			/// <summary>
			/// The channel marked P2 on the board.
			/// </summary>
			P2 = 0x0400,
			/// <summary>
			/// The channel marked P3 on the board.
			/// </summary>
			P3 = 0x0600,
			/// <summary>
			/// The channel marked P4 on the board.
			/// </summary>
			P4 = 0x0800,
			/// <summary>
			/// The channel marked P5 on the board.
			/// </summary>
			P5 = 0x0A00,
			/// <summary>
			/// The channel marked P6 on the board.
			/// </summary>
			P6 = 0x0C00,
			/// <summary>
			/// The channel marked P7 on the board.
			/// </summary>
			P7 = 0x0E00,
			/// <summary>
			/// The channel marked P8 on the board.
			/// </summary>
			P8 = 0x0000
		}

		/// <summary>
		/// Gets the voltage on the given channel.
		/// </summary>
		/// <param name="channel">The channel to read.</param>
		/// <returns>The voltage on the given channel between 0 and 4.096V.</returns>
		public double GetReading(Channel channel)
		{
			this.writeRead((ushort)(0xF124 | (int)channel));
			return 4.096 * (double)this.writeRead(0x0000) / 65535.0;
		}

		private ushort writeRead(ushort write)
		{
			ushort read = 0;

			this.cs.Write(false);

			for (ushort j = 0, mask = 0x8000; j < 16; j++, mask >>= 1)
			{
				this.clock.Write(false);

				this.mosi.Write((write & mask) != 0);

				this.clock.Write(true);

				if (this.miso.Read())
					read |= mask;
			}

			this.mosi.Write(true);
			this.clock.Write(false);
			this.cs.Write(true);

			return read;
		}
	}
}