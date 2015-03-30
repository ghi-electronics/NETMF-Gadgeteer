using System;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>An LED7R module for Microsoft .NET Gadgeteer</summary>
	public class LED7R : GTM.Module {
		private GTI.DigitalOutput[] leds;

		/// <summary>The number of LEDs on the module.</summary>
		public int LedCount {
			get {
				return 7;
			}
		}

		/// <summary>The state of the given LED.</summary>
		/// <param name="index">The LED whose state to get or set.</param>
		/// <returns>Whether or not the LED is on or off.</returns>
		public bool this[int index] {
			get {
				if (index >= this.LedCount || index < 0) throw new ArgumentOutOfRangeException("index", "index must be between 0 and LedCount.");

				return this.leds[index].Read();
			}

			set {
				if (index >= this.LedCount || index < 0) throw new ArgumentOutOfRangeException("index", "index must be between 0 and LedCount.");

				this.SetLed(index, value);
			}
		}

		/// <summary>Constructor</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public LED7R(int socketNumber) {
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);

			socket.EnsureTypeIsSupported('Y', this);

			this.leds = new GTI.DigitalOutput[this.LedCount];
			for (int i = 0; i < this.LedCount; i++) {
				this.leds[i] = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three + i, false, this);
			}
		}

		/// <summary>Turns the specified LED on.</summary>
		/// <param name="led">The LED to turn on.</param>
		public void TurnLedOn(int led) {
			if (led >= this.LedCount || led < 0) throw new ArgumentOutOfRangeException("led", "led must be between 0 and LedCount.");

			this.SetLed(led, true);
		}

		/// <summary>Turns the specified LED off.</summary>
		/// <param name="led">The LED to turn off.</param>
		public void TurnLedOff(int led) {
			if (led >= this.LedCount || led < 0) throw new ArgumentOutOfRangeException("led", "led must be between 0 and LedCount.");

			this.SetLed(led, false);
		}

		/// <summary>Sets the given LED to the given state.</summary>
		/// <param name="led">The LED to change to set.</param>
		/// <param name="state">The new LED state.</param>
		public void SetLed(int led, bool state) {
			if (led >= this.LedCount || led < 0) throw new ArgumentOutOfRangeException("led", "led must be between 0 and LedCount.");

			this.leds[led].Write(state);
		}

		/// <summary>Sets the LEDs on the module to the corresponding bit in the mask.</summary>
		/// <param name="mask">The bit mask to set the LEDs to.</param>
		public void SetBitmask(uint mask) {
			uint value = 1;

			for (int i = 0; i < this.LedCount; i++) {
				if ((mask & value) != 0)
					this.TurnLedOn(i);
				else
					this.TurnLedOff(i);

				value <<= 1;
			}
		}

		/// <summary>Turns on all of the LEDs.</summary>
		public void TurnAllLedsOn() {
			for (int i = 0; i < 7; i++)
				this.TurnLedOn(i);
		}

		/// <summary>Turns off all of the LEDs.</summary>
		public void TurnAllLedsOff() {
			for (int i = 0; i < 7; i++)
				this.TurnLedOff(i);
		}

		/// <summary>Turns all of the LEDs on up until, but not including, the LED numbered by endIndex. The rest are turned off.</summary>
		/// <param name="endIndex">The LED to stop before.</param>
		public void SetLeds(int endIndex) {
			if (endIndex > this.LedCount || endIndex < 0) throw new ArgumentOutOfRangeException("led", "led must be between 0 and LedCount.");

			int led = 0;

			for (; led < endIndex; led++)
				this.TurnLedOn(led);

			for (; led < this.LedCount; led++)
				this.TurnLedOff(led);
		}

		/// <summary>Turns on the LedCount * percentage LEDs starting at LED 0.</summary>
		/// <param name="percentage">The amount of LEDs to turn on.</param>
		public void SetPercentage(double percentage) {
			if (percentage > 1 || percentage < 0) throw new ArgumentOutOfRangeException("led", "led must be between 0 and LedCount.");

			this.SetLeds((int)(percentage * this.LedCount));
		}

		/// <summary>Animates the lights on the board, according to the passed in values.</summary>
		/// <param name="switchTime">Time between each operation in milliseconds</param>
		/// <param name="clockwise">Whether or not the animation should play in a clockwise motion.</param>
		/// <param name="on">Whether or not the animation should turn the lights on, false if the lights should be turned off.</param>
		/// <param name="remainOn">Whether or not a light should remain on when another one is lit, false if only one light should be lit at a time.</param>
		public void Animate(int switchTime, bool clockwise, bool on, bool remainOn) {
			int length = this.LedCount - 1;
			int i;
			int terminate;
			int dir;

			if (clockwise) {
				i = 0;
				terminate = length;
				dir = 1;
			}
			else {
				i = length - 1;
				terminate = -1;
				dir = -1;
			}

			for (; i != terminate; i += dir) {
				if (on) {
					if (!remainOn)
						this.TurnAllLedsOff();

					this.TurnLedOn(i + 1);
				}
				else {
					this.TurnLedOff(i + 1);
				}

				Thread.Sleep(switchTime);
			}
		}
	}
}