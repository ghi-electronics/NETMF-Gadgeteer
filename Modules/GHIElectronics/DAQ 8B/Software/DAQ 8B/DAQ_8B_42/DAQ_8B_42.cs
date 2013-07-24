﻿using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A DAQ 8B module for Microsoft .NET Gadgeteer
	/// </summary>
	public class DAQ_8B : GTM.Module
	{
		private GTI.DigitalInput MISO;
		private GTI.DigitalOutput MOSI;
		private GTI.DigitalOutput CLOCK;
		private GTI.DigitalOutput CS;

		/// <summary>Constructs a new instance of a DAQ 8B module.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public DAQ_8B(int socketNumber)
		{
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			//socket.EnsureTypeIsSupported('S', this);

			//new GTI.SPI(socket, new GTI.SPI.Configuration(false, 1, 1, false, true, 25000), GTI.SPI.Sharing.Shared, socket, Socket.Pin.Six, this);
			this.CS = new GTI.DigitalOutput(socket, Socket.Pin.Six, true, this);
			this.MISO = new GTI.DigitalInput(socket, Socket.Pin.Eight, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, this);
			this.MOSI = new GTI.DigitalOutput(socket, Socket.Pin.Seven, false, this);
			this.CLOCK = new GTI.DigitalOutput(socket, Socket.Pin.Nine, false, this);
		}

		/// <summary>
		/// The channels on the board.
		/// </summary>
		public enum Channel
		{
			/// <summary>
			/// The channel marked P1 on the board.
			/// </summary>
			P1 = 0x00,
			/// <summary>
			/// The channel marked P2 on the board.
			/// </summary>
			P2 = 0x01,
			/// <summary>
			/// The channel marked P3 on the board.
			/// </summary>
			P3 = 0x02,
			/// <summary>
			/// The channel marked P4 on the board.
			/// </summary>
			P4 = 0x03,
			/// <summary>
			/// The channel marked P5 on the board.
			/// </summary>
			P5 = 0x04,
			/// <summary>
			/// The channel marked P6 on the board.
			/// </summary>
			P6 = 0x05,
			/// <summary>
			/// The channel marked P7 on the board.
			/// </summary>
			P7 = 0x06,
			/// <summary>
			/// The channel marked P8 on the board.
			/// </summary>
			P8 = 0x07
		}

		/// <summary>
		/// Queries the board for a voltage reading on the given channel.
		/// </summary>
		/// <param name="channel">The channel to query.</param>
		/// <returns>The voltage on the given channel between 0 and 4.096V.</returns>
		public double GetReading(Channel channel)
		{
			return 4.096 * ((this.SPIWriteRead(((0x3C49 | ((int)channel << 7)) << 2)) >> 2) / 0x3FFF);
		}

		private int SPIWriteRead(int write)
		{
			int read = 0;

			this.CS.Write(false);

			for (ushort j = 0, mask = 0x8000; j < 14; j++, mask >>= 1)
			{
				this.CLOCK.Write(false);

				this.MOSI.Write((write & mask) != 0);

				this.CLOCK.Write(true);

				if (this.MISO.Read())
					read |= mask;
			}

			this.MOSI.Write(false);
			this.CLOCK.Write(false);
			this.CS.Write(true);

			return read;
		}
	}
}