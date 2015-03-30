using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A FEZtive module for Microsoft .NET Gadgeteer</summary>
	[Obsolete]
	public class FEZtive : GTM.Module {
		private GTI.Spi spi;
		private Color[] leds;
		private byte[] zeroes;

		/// <summary>Red.</summary>
		public static Color Red { get; private set; }

		/// <summary>Blue.</summary>
		public static Color Blue { get; private set; }

		/// <summary>Green.</summary>
		public static Color Green { get; private set; }

		/// <summary>White.</summary>
		public static Color White { get; private set; }

		/// <summary>Black.</summary>
		public static Color Black { get; private set; }

		static FEZtive() {
			FEZtive.Red = new Color(127, 0, 0);
			FEZtive.Blue = new Color(0, 0, 127);
			FEZtive.Green = new Color(0, 127, 0);
			FEZtive.White = new Color(127, 127, 127);
			FEZtive.Black = new Color(0, 0, 0);
		}

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public FEZtive(int socketNumber) {
			var socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('S', this);

			this.spi = GTI.SpiFactory.Create(socket, new GTI.SpiConfiguration(true, 0, 0, false, true, 1000), GTI.SpiSharing.Shared, socket, Socket.Pin.Six, this);
			this.leds = null;
			this.zeroes = null;
		}

		/// <summary>Initializes the module.</summary>
		/// <param name="numberOfLeds">The number of leds. Each strip contains 80.</param>
		public void Initialize(int numberOfLeds = 80) {
			if (numberOfLeds < 1) throw new ArgumentOutOfRangeException("leds", "leds must be positive.");
			if (this.leds != null) throw new InvalidOperationException("The module has already been initialized.");

			this.leds = new Color[numberOfLeds];
			this.zeroes = new byte[3 * ((numberOfLeds + 63) / 64)];

			for (int i = 0; i < numberOfLeds; i++)
				this.leds[i] = new Color(0, 0, 0);
		}

		/// <summary>Sets all LEDs to the specified color.</summary>
		/// <param name="color">The new color.</param>
		public void SetAll(Color color) {
			if (this.leds == null) throw new InvalidOperationException("The module is not initialized.");

			this.spi.Write(this.zeroes);

			for (int i = 0; i < this.leds.Length; i += 2) {
				this.leds[i] = color;
				this.leds[i + 1] = color;

				this.spi.Write(this.leds[i].GetForRender());
				this.spi.Write(this.leds[i + 1].GetForRender());
			}

			this.spi.Write(this.zeroes);
		}

		/// <summary>Sets all LEDs to the specified colors.</summary>
		/// <param name="colors">The array to set every LED to.</param>
		public void SetAll(Color[] colors) {
			if (this.leds == null) throw new InvalidOperationException("The module is not initialized.");

			if (colors.Length != this.leds.Length) throw new ArgumentOutOfRangeException("colors", "colors.Length is invalid.");

			for (int i = 0; i < leds.Length; i += 2) {
				this.SetLED(colors[i], i);
				this.SetLED(colors[i + 1], i + 1);

				this.spi.Write(this.leds[i].GetForRender());
				this.spi.Write(this.leds[i + 1].GetForRender());
			}

			this.spi.Write(this.zeroes);
		}

		/// <summary>Sets the specified LED to the specified color.</summary>
		/// <param name="color">The new color.</param>
		/// <param name="led">The LED to set.</param>
		public void SetLED(Color color, int led) {
			if (this.leds == null) throw new InvalidOperationException("The module is not initialized.");

			this.leds[led] = color;

			this.Redraw();
		}

		/// <summary>Turns all LEDs off (Black)</summary>
		public void Clear() {
			if (this.leds == null) throw new InvalidOperationException("The module is not initialized.");

			this.SetAll(FEZtive.Black);
		}

		/// <summary>Redraws all of the colors. Only to be used after a change was made to the Color array.</summary>
		public void Redraw() {
			if (this.leds == null) throw new InvalidOperationException("The module is not initialized.");

			this.spi.Write(this.zeroes);

			for (int i = 0; i < leds.Length; i += 2) {
				this.spi.Write(this.leds[i].GetForRender());
				this.spi.Write(this.leds[i + 1].GetForRender());
			}

			this.spi.Write(this.zeroes);
		}

		/// <summary>Describes a color.</summary>
		public class Color {

			/// <summary>The amount of red.</summary>
			public byte Red { get; set; }

			/// <summary>The amount of green.</summary>
			public byte Green { get; set; }

			/// <summary>The amount of blue.</summary>
			public byte Blue { get; set; }

			/// <summary>Constructs a new instances.</summary>
			/// <param name="red">The amount of red.</param>
			/// <param name="green">The amount of green.</param>
			/// <param name="blue">The amount of blue.</param>
			public Color(byte red, byte green, byte blue) {
				this.Red = red;
				this.Green = green;
				this.Blue = blue;
			}

			internal byte[] GetForRender() {
				return new byte[] { (byte)(0x80 | this.Green), (byte)(0x80 | this.Red), (byte)(0x80 | this.Blue) };
			}
		}
	}
}