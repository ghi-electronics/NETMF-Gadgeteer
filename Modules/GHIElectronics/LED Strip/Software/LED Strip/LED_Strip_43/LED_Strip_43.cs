using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A LED Strip module for Microsoft .NET Gadgeteer
    /// </summary>
    public class LED_Strip : GTM.Module
    {
        private GTI.DigitalOutput[] LEDs = new GTI.DigitalOutput[7];

		/// <summary>
		/// The maximum value a bitmask can have.
		/// </summary>
        public readonly uint MAX_VALUE = 0x7F;

		/// <summary>
		/// Gets the number of LEDs on the module.
		/// </summary>
		public int LedCount
		{
			get
			{
				return 7;
			}
		}

        /// <summary>Constructor</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public LED_Strip(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('Y', this);

            for (int i = 0; i < 7; i++)
            {
                this.LEDs[i] = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three + i, false, this);
            }
        }

        /// <summary>
        /// Turns a specified LED on, indicated by the numbering on the PCB.
        /// </summary>
        /// <param name="led">LED to turn on.</param>
        public void TurnLEDOn(int led)
        {
            if (led > 6)
                throw new ArgumentOutOfRangeException();

			this.SetLED(led, true);
        }

        /// <summary>
        /// Turns a specified LED off, indicated by the numbering on the PCB.
        /// </summary>
        /// <param name="led">LED to turn off.</param>
        public void TurnLEDOff(int led)
        {
            if (led > 6)
                throw new ArgumentOutOfRangeException();

			this.SetLED(led, false);
        }

        /// <summary>
        /// Sets an LED to the specified state.
        /// </summary>
        /// <param name="led">LED to change. Indicated be the numbering on the PCB.</param>
        /// <param name="state">State to set. True is on. False is off.</param>
        public void SetLED(int led, bool state)
        {
            if (led > 6)
                throw new ArgumentOutOfRangeException();

            LEDs[led].Write(state);
        }

        /// <summary>
        /// Sets the LEDs on the module to the value passed in.
        /// </summary>
        /// <param name="mask">The bit mask to set the LEDs to.</param>
        public void SetBitmask(uint mask)
        {
            if (mask > MAX_VALUE)
                throw new ArgumentOutOfRangeException();

            uint value = 1;

            for (int i = 0; i < 7; i++)
            {
                if ((mask & value) == value)
					this.TurnLEDOn(i);
				else
					this.TurnLEDOff(i);

                value = value << 1;
            }
        }

		/// <summary>
		/// Turns all of the LEDs on.
		/// </summary>
		public void TurnAllLedsOn()
		{
			for (int i = 0; i < 7; i++)
				this.TurnLEDOn(i);
		}

		/// <summary>
		/// Turns all of the LEDs off.
		/// </summary>
		public void TurnAllLedsOff()
		{
			for (int i = 0; i < 7; i++)
				this.TurnLEDOff(i);
		}

		/// <summary>
		/// Gets or sets the value of the specified LED.
		/// </summary>
		/// <param name="index">The LED whose state to get or set.</param>
		/// <returns>Whether or not the LED is on or off.</returns>
		public bool this[int index]
		{
			get
			{
				if (index >= this.LedCount || index < 0)
					throw new IndexOutOfRangeException();

				return this.LEDs[index].Read();
			}
			set
			{
				if (index >= this.LedCount || index < 0)
					throw new IndexOutOfRangeException();

				this.SetLED(index, value);
			}
		}
    }
}
