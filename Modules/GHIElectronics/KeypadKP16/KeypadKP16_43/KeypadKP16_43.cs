using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A Keypad KP16 module for Microsoft .NET Gadgeteer
	/// </summary>
	public class Keypad_KP16 : GTM.Module
	{
		private GTI.DigitalOutput out1;
		private GTI.DigitalOutput out2;
		private GTI.DigitalInput in1;
		private GTI.DigitalInput in2;
		private GTI.DigitalInput in3;
		private GTI.DigitalInput in4;

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public Keypad_KP16(int socketNumber)
		{
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);

			socket.EnsureTypeIsSupported('Y', this);

			this.out1 = GT.SocketInterfaces.DigitalOutputFactory.Create(socket, GT.Socket.Pin.Three, true, null);
			this.out2 = GT.SocketInterfaces.DigitalOutputFactory.Create(socket, GT.Socket.Pin.Four, true, null);
			this.in1 = GT.SocketInterfaces.DigitalInputFactory.Create(socket, GT.Socket.Pin.Five, GT.SocketInterfaces.GlitchFilterMode.Off, GT.SocketInterfaces.ResistorMode.PullUp, null);
			this.in2 = GT.SocketInterfaces.DigitalInputFactory.Create(socket, GT.Socket.Pin.Six, GT.SocketInterfaces.GlitchFilterMode.Off, GT.SocketInterfaces.ResistorMode.PullUp, null);
			this.in3 = GT.SocketInterfaces.DigitalInputFactory.Create(socket, GT.Socket.Pin.Seven, GT.SocketInterfaces.GlitchFilterMode.Off, GT.SocketInterfaces.ResistorMode.PullUp, null);
			this.in4 = GT.SocketInterfaces.DigitalInputFactory.Create(socket, GT.Socket.Pin.Eight, GT.SocketInterfaces.GlitchFilterMode.Off, GT.SocketInterfaces.ResistorMode.PullUp, null);
		}

		/// <summary>
		/// The possible keys that can be pressed.
		/// </summary>
		public enum Key
		{
			/// <summary>
			/// The A key.
			/// </summary>
			A,
			/// <summary>
			/// The B key.
			/// </summary>
			B,
			/// <summary>
			/// The C key.
			/// </summary>
			C,
			/// <summary>
			/// The D key.
			/// </summary>
			D,
			/// <summary>
			/// The # key.
			/// </summary>
			Pound,
			/// <summary>
			/// The * key.
			/// </summary>
			Star,
			/// <summary>
			/// The 0 key.
			/// </summary>
			Zero,
			/// <summary>
			/// The 1 key.
			/// </summary>
			One,
			/// <summary>
			/// The 2 key.
			/// </summary>
			Two,
			/// <summary>
			/// The 3 key.
			/// </summary>
			Three,
			/// <summary>
			/// The 4 key.
			/// </summary>
			Four,
			/// <summary>
			/// The 5 key.
			/// </summary>
			Five,
			/// <summary>
			/// The 6 key.
			/// </summary>
			Six,
			/// <summary>
			/// The 7 key.
			/// </summary>
			Seven,
			/// <summary>
			/// The 8 key.
			/// </summary>
			Eight,
			/// <summary>
			/// The 9 key.
			/// </summary>
			Nine,
		}

		/// <summary>
		/// Determines whether or not a given key is pressed.
		/// </summary>
		/// <param name="key">The key whose state we want to check.</param>
		/// <returns>Whether or not the key is pressed.</returns>
		public bool IsKeyPressed(Key key)
		{
			bool out1 = false;
			bool out2 = false;

			if (key == Key.One || key == Key.Four || key == Key.Seven || key == Key.Star) { out1 = false; out2 = false; }
			else if (key == Key.Two || key == Key.Five || key == Key.Eight || key == Key.Zero) { out1 = true; out2 = false; }
			else if (key == Key.Three || key == Key.Six || key == Key.Nine || key == Key.Pound) { out1 = false; out2 = true; }
			else if (key == Key.A || key == Key.B || key == Key.C || key == Key.D) { out1 = true; out2 = true; }

			this.out1.Write(out1);
			this.out2.Write(out2);
	
			if (key == Key.One || key == Key.Two || key == Key.Three || key == Key.A) return !this.in1.Read();
			else if (key == Key.Four || key == Key.Five || key == Key.Six || key == Key.B) return !this.in2.Read();
			else if (key == Key.Seven || key == Key.Eight || key == Key.Nine || key == Key.C) return !this.in3.Read();
			else if (key == Key.Star || key == Key.Zero || key == Key.Pound || key == Key.D) return !this.in4.Read();

			return false;
		}
	}
}
