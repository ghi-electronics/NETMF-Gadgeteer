using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Collections;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A HubAP5 module for Microsoft .NET Gadgeteer
	/// </summary>
	public class HubAP5 : GTM.Module
	{
		private Socket[] sockets = new Socket[8];
		private IO60P16 io60;
		private ADS7830 ads;
		private Hashtable socketMap;
		private byte[] pinMap;
		private string[] typeMap;

		/// <summary>
		/// Returns the socket number for socket 1 on the hub.
		/// </summary>
		public int HubSocket1 { get { return this.sockets[0].SocketNumber; } }

		/// <summary>
		/// Returns the socket number for socket 2 on the hub.
		/// </summary>
		public int HubSocket2 { get { return this.sockets[1].SocketNumber; } }

		/// <summary>
		/// Returns the socket number for socket 3 on the hub.
		/// </summary>
		public int HubSocket3 { get { return this.sockets[2].SocketNumber; } }

		/// <summary>
		/// Returns the socket number for socket 4 on the hub.
		/// </summary>
		public int HubSocket4 { get { return this.sockets[3].SocketNumber; } }

		/// <summary>
		/// Returns the socket number for socket 5 on the hub.
		/// </summary>
		public int HubSocket5 { get { return this.sockets[4].SocketNumber; } }

		/// <summary>
		/// Returns the socket number for socket 6 on the hub.
		/// </summary>
		public int HubSocket6 { get { return this.sockets[5].SocketNumber; } }

		/// <summary>
		/// Returns the socket number for socket 7 on the hub.
		/// </summary>
		public int HubSocket7 { get { return this.sockets[6].SocketNumber; } }

		/// <summary>
		/// Returns the socket number for socket 8 on the hub.
		/// </summary>
		public int HubSocket8 { get { return this.sockets[7].SocketNumber; } }

		/// <summary>Constructs a new HubAP5 module.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public HubAP5(int socketNumber)
		{
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);

			socket.EnsureTypeIsSupported('I', this);

			this.io60 = new IO60P16(socket);
			this.ads = new ADS7830(socket);
			this.socketMap = new Hashtable();
			this.typeMap = new string[8] { "AY", "AY", "Y", "PY", "PY", "PY", "PY", "PY" };
			this.pinMap = new byte[56] {
											0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, //Socket 1
											0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, //Socket 2
											0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, //Socket 3
											0x20, 0x21, 0x22, 0x23, 0x74, 0x75, 0x76, //Socket 4
											0x14, 0x15, 0x16, 0x17, 0x71, 0x72, 0x73, //Socket 5
											0x10, 0x11, 0x12, 0x13, 0x66, 0x67, 0x70, //Socket 6
											0x04, 0x05, 0x06, 0x07, 0x63, 0x64, 0x65, //Socket 7
											0x00, 0x01, 0x02, 0x03, 0x60, 0x61, 0x62  //Socket 8
										 };

			for (int i = 0; i < 8; i++)
			{
				this.sockets[i] = Socket.SocketInterfaces.CreateUnnumberedSocket("Hub AP5 " + (i + 1).ToString());
				this.sockets[i].SupportedTypes = this.typeMap[i].ToCharArray();

				for (int j = 3; j <= 9; j++)
					this.sockets[i].CpuPins[j] = Socket.UnnumberedPin;

				this.socketMap[this.sockets[i].SocketNumber] = i;

				this.sockets[i].DigitalInputIndirector = (indirectedSocket, indirectedPin, glitchFilterMode, resistorMode, module) => new DigitalInputImplementation(this.GetPin(indirectedSocket, indirectedPin), glitchFilterMode, resistorMode, this.io60);
				this.sockets[i].DigitalOutputIndirector = (indirectedSocket, indirectedPin, initialState, module) => new DigitalOutputImplementation(this.GetPin(indirectedSocket, indirectedPin), initialState, this.io60);
				this.sockets[i].DigitalIOIndirector = (indirectedSocket, indirectedPin, initialState, glitchFilterMode, resistorMode, module) => new DigitalIOImplementation(this.GetPin(indirectedSocket, indirectedPin), initialState, glitchFilterMode, resistorMode, this.io60);
				this.sockets[i].AnalogInputIndirector = (indirectedSocket, indirectedPin, module) => new AnalogInputImplementation(this.GetPin(indirectedSocket, indirectedPin), this.ads, this.io60);
				this.sockets[i].PwmOutputIndirector = (indirectedSocket, indirectedPin, invert, module) => new PwmOutputImplementation(this.GetPin(indirectedSocket, indirectedPin), invert, this.io60);
				this.sockets[i].InterruptIndirector = (indirectedSocket, indirectedPin, glitchFilterMode, resistorMode, interruptMode, module) => new InterruptInputImplementation(this.GetPin(indirectedSocket, indirectedPin), glitchFilterMode, resistorMode, interruptMode, this.io60);

				Socket.SocketInterfaces.RegisterSocket(this.sockets[i]);
			}
		}

		private byte GetPin(Socket socket, Socket.Pin pin)
		{
			return this.pinMap[(int)(this.socketMap[socket.SocketNumber]) * 7 + (int)(pin) - 3];
		}

		private class DigitalInputImplementation : Socket.SocketInterfaces.DigitalInput
		{
			private IO60P16 io60;
			private byte pin;

			public DigitalInputImplementation(byte pin, GTI.GlitchFilterMode glitchFilter, GTI.ResistorMode resistorMode, IO60P16 io60)
			{
				this.io60 = io60;
				this.pin = pin;

				this.io60.setIOMode(this.pin, IO60P16.IOState.Input, resistorMode);
			}

			public override bool Read()
			{
				return this.io60.readDigital(this.pin);
			}
		}

		private class DigitalOutputImplementation : Socket.SocketInterfaces.DigitalOutput
		{
			private IO60P16 io60;
			private byte pin;

			public DigitalOutputImplementation(byte pin, bool initialState, IO60P16 io60)
			{
				this.io60 = io60;
				this.pin = pin;

				this.io60.setIOMode(this.pin, IO60P16.IOState.Output, GTI.ResistorMode.Disabled);
				this.Write(initialState);
			}

			public override bool Read()
			{
				return this.io60.readDigital(this.pin);
			}

			public override void Write(bool state)
			{
				this.io60.writeDigital(this.pin, state);
			}
		}

		private class DigitalIOImplementation : Socket.SocketInterfaces.DigitalIO
		{
			private IO60P16 io60;
			private byte pin;
			private Socket.SocketInterfaces.IOMode mode;
			private GTI.ResistorMode resistorMode;

			public DigitalIOImplementation(byte pin, bool initialState, GTI.GlitchFilterMode glitchFilter, GTI.ResistorMode resistorMode, IO60P16 io60)
			{
				this.io60 = io60;
				this.pin = pin;
				this.resistorMode = resistorMode;
				this.Mode = Socket.SocketInterfaces.IOMode.Input;
				this.io60.writeDigital(this.pin, initialState);
			}

			public override bool Read()
			{
				this.Mode = Socket.SocketInterfaces.IOMode.Input;
				return this.io60.readDigital(this.pin);
			}

			public override void Write(bool state)
			{
				this.Mode = Socket.SocketInterfaces.IOMode.Output;
				this.io60.writeDigital(this.pin, state);
			}

			public override Socket.SocketInterfaces.IOMode Mode
			{
				get
				{
					return this.mode;
				}
				set
				{
					this.mode = value;
					this.io60.setIOMode(this.pin, this.mode == Socket.SocketInterfaces.IOMode.Input ? IO60P16.IOState.Input : IO60P16.IOState.Output, this.resistorMode);
				}
			}
		}

		private class AnalogInputImplementation : Socket.SocketInterfaces.AnalogInput
		{
			private ADS7830 ads;
			private IO60P16 io60;
			private byte pin;
			private byte channel;
			private bool active;

			public AnalogInputImplementation(byte pin, ADS7830 ads, IO60P16 io60)
			{
				this.ads = ads;
				this.io60 = io60;
				this.pin = pin;
				this.active = false;

				var channels = new byte[12] { 0x50, 0, 0x51, 1, 0x52, 2, 0x40, 3, 0x41, 4, 0x42, 5 };
				for (int i = 0; i < 12; i += 2)
					if (channels[i] == this.pin)
						this.channel = channels[i + 1];
			}

			public override double ReadVoltage()
			{
				return this.ads.ReadVoltage(this.channel);
			}

			public override double ReadProportion()
			{
				return this.ReadVoltage() / 3.3;
			}

			public override bool IsActive
			{
				get
				{
					return this.active;
				}
				set
				{
					if (this.active == value)
						return;

					this.active = value;

					if (this.active)
						this.io60.setIOMode(this.pin, IO60P16.IOState.Input, IO60P16.ResistorMode.Floating);
				}
			}
		}

		private class PwmOutputImplementation : Socket.SocketInterfaces.PwmOutput
		{
			private IO60P16 io60;
			private byte pin;
			private bool isActive;

			public PwmOutputImplementation(byte pin, bool invert, IO60P16 io60)
			{
				this.io60 = io60;
				this.pin = pin;
				this.isActive = false;
			}

			public override void Set(double frequency, double dutyCycle)
			{
				if (frequency < 0) throw new ArgumentException("frequency");
				if (dutyCycle < 0 || dutyCycle > 1) throw new ArgumentException("dutyCycle");

				this.IsActive = true;

				this.io60.setPWM(this.pin, frequency, dutyCycle);
			}

			public override void Set(uint period, uint highTime, Socket.SocketInterfaces.PwmScaleFactor factor)
			{
				if (period == 0) throw new ArgumentException("period");
				if (highTime > period) throw new ArgumentException("highTime");

				double frequency = 0;

				switch (factor)
				{
					case Socket.SocketInterfaces.PwmScaleFactor.Milliseconds: frequency = 1000 / period; break;
					case Socket.SocketInterfaces.PwmScaleFactor.Microseconds: frequency = 1000000 / period; break;
					case Socket.SocketInterfaces.PwmScaleFactor.Nanoseconds: frequency = 1000000000 / period; break;
				}

				this.Set(frequency, (double)highTime / (double)period);
			}

			public override bool IsActive
			{
				get
				{
					return this.isActive;
				}
				set
				{
					if (this.isActive == value)
						return;

					this.isActive = !this.isActive;

					if (this.isActive)
					{
						this.io60.setIOMode(this.pin, IO60P16.IOState.PWM, GTI.ResistorMode.Disabled);
						this.io60.setPWM(this.pin, 1, 0.5);
					}
					else
					{
						this.io60.setIOMode(this.pin, IO60P16.IOState.Input, GTI.ResistorMode.Disabled);
					}
				}
			}
		}

		private class InterruptInputImplementation : Socket.SocketInterfaces.InterruptInput
		{
			private IO60P16 io60;
			private byte pin;

			protected override void OnInterruptFirstSubscribed()
			{
				
			}

			protected override void OnInterruptLastUnsubscribed()
			{
				
			}

			public InterruptInputImplementation(byte pin, GTI.GlitchFilterMode glitchFilter, GTI.ResistorMode resistorMode, GTI.InterruptMode interruptMode, IO60P16 io60)
			{
				this.io60 = io60;
				this.pin = pin;

				this.io60.setIOMode(this.pin, IO60P16.IOState.InputInterrupt, resistorMode);
				this.io60.registerInterruptHandler(this.pin, interruptMode, this.RaiseInterrupt);
			}

			public override bool Read()
			{
				return this.io60.readDigital(this.pin);
			}
		}

		private class IO60P16
		{
			private const byte INPUT_PORT_0_REGISTER = 0x00;
			private const byte OUTPUT_PORT_0_REGISTER = 0x08;
			private const byte INTERRUPT_PORT_0_REGISTER = 0x10;
			private const byte PORT_SELECT_REGISTER = 0x18;
			private const byte INTERRUPT_MASK_REGISTER = 0x19;

			private const byte PIN_DIRECTION_REGISTER = 0x1C;
			private const byte PIN_PULL_UP = 0x1D;
			private const byte PIN_PULL_DOWN = 0x1E;
			private const byte PIN_OPEN_DRAIN_HIGH = 0x1F;
			private const byte PIN_OPEN_DRAIN_LOW = 0x20;
			private const byte PIN_STRONG_DRIVE = 0x21;
			private const byte PIN_SLOW_STRONG_DRIVE = 0x22;
			private const byte PIN_HIGH_IMPEDENCE = 0x23;

			private const byte ENABLE_PWM_REGISTER = 0x1A;
			private const byte PWM_SELECT_REGISTER = 0x28;
			private const byte PWM_CONFIG = 0x29;
			private const byte PERIOD_REGISTER = 0x2A;
			private const byte PULSE_WIDTH_REGISTER = 0x2B;

			private const byte CLOCK_SOURCE = 0x3;

			private GTI.I2CBus io60Chip;
			private GTI.InterruptInput interrupt;
			private ArrayList interruptHandlers = new ArrayList();
			private byte[] write2 = new byte[2];
			private byte[] write1 = new byte[1];
			private byte[] read1 = new byte[1];
			private byte[] pwms = new byte[30] { 0x60, 0, 0x61, 1, 0x62, 2, 0x63, 3, 0x64, 4, 0x65, 5, 0x66, 6, 0x67, 7, 0x70, 8, 0x71, 9, 0x72, 10, 0x73, 11, 0x74, 12, 0x75, 13, 0x76, 14 };

			public delegate void InterruptHandler(bool state);

			private class InterruptRegistraton
			{
				public GTI.InterruptMode mode;
				public InterruptHandler handler;
				public byte pin;
			}

			public enum IOState
			{
				InputInterrupt,
				Input,
				Output,
				PWM
			}

			public enum ResistorMode
			{
				PullUp = IO60P16.PIN_PULL_UP,
				PullDown = IO60P16.PIN_PULL_DOWN,
				Floating = IO60P16.PIN_HIGH_IMPEDENCE,
			}

			private byte getPort(byte pin)
			{
				return (byte)(pin >> 4);
			}

			private byte getMask(byte pin)
			{
				return (byte)(1 << (pin & 0x0F));
			}

			private void writeRegister(byte register, byte value)
			{
				lock (this.io60Chip)
				{
					write2[0] = register;
					write2[1] = value;
					this.io60Chip.Write(write2, 250);
				}
			}

			private byte readRegister(byte register)
			{
				byte result;

				lock (this.io60Chip)
				{
					write1[0] = register;
					this.io60Chip.WriteRead(write1, read1, 250);
					result = read1[0];
				}

				return result;
			}

            private byte[] readRegisters(byte register, uint count)
            {
                byte[] result = new byte[count];

                lock (this.io60Chip)
                {
                    write1[0] = register;
                    this.io60Chip.WriteRead(write1, result, 250);
                }

                return result;
            }

			private void OnInterrupt(GTI.InterruptInput sender, bool value)
			{
				ArrayList interruptedPins = new ArrayList();

                byte[] intPorts = this.readRegisters(IO60P16.INTERRUPT_PORT_0_REGISTER, 8);
				for (byte i = 0; i < 8; i++)
					for (int j = 1, k = 0; j <= 128; j <<= 1, k++)
                        if ((intPorts[i] & j) != 0)
							interruptedPins.Add((i << 4) | k);

				foreach (int pin in interruptedPins)
				{
					lock (this.interruptHandlers)
					{
						foreach (InterruptRegistraton reg in this.interruptHandlers)
						{
							if (reg.pin == pin)
							{
								bool val = this.readDigital((byte)pin);
								if ((reg.mode == GTI.InterruptMode.RisingEdge && val) || (reg.mode == GTI.InterruptMode.FallingEdge && !val) || reg.mode == GTI.InterruptMode.RisingAndFallingEdge)
									reg.handler(val);
							}
						}
					}
				}
			}

			public IO60P16(Socket socket)
			{
				this.io60Chip = new GTI.I2CBus(socket, 0x20, 100, null);

				this.interrupt = new GTI.InterruptInput(socket, Socket.Pin.Three, GTI.GlitchFilterMode.On, GTI.ResistorMode.Disabled, GTI.InterruptMode.RisingEdge, null);
				this.interrupt.Interrupt += this.OnInterrupt;
			}

			public void registerInterruptHandler(byte pin, GTI.InterruptMode mode, InterruptHandler handler)
			{
				InterruptRegistraton reg = new InterruptRegistraton();
				reg.handler = handler;
				reg.mode = mode;
				reg.pin = pin;

				lock (this.interruptHandlers)
					this.interruptHandlers.Add(reg);
			}

			public void setIOMode(byte pin, IOState state, GTI.ResistorMode resistorMode)
			{
				switch (resistorMode)
				{
					case GTI.ResistorMode.Disabled: this.setIOMode(pin, state, ResistorMode.Floating); break;
					case GTI.ResistorMode.PullDown: this.setIOMode(pin, state, ResistorMode.PullDown); break;
					case GTI.ResistorMode.PullUp: this.setIOMode(pin, state, ResistorMode.PullUp); break;
				}
			}

			public void setIOMode(byte pin, IOState state, ResistorMode resistorMode)
			{
				this.writeRegister(IO60P16.PORT_SELECT_REGISTER, this.getPort(pin));

				byte mask = this.getMask(pin);
				byte val = this.readRegister(IO60P16.ENABLE_PWM_REGISTER);

				if (state == IOState.PWM)
				{
					this.writeRegister(IO60P16.ENABLE_PWM_REGISTER, (byte)(val | mask));

					this.writeDigital(pin, true);

					byte pwm = 255;
					for (var i = 0; i < 30; i += 2)
						if (this.pwms[i] == pin)
							pwm = this.pwms[i + 1];

					this.writeRegister(IO60P16.PWM_SELECT_REGISTER, pwm);
                    this.writeRegister(IO60P16.PWM_CONFIG, IO60P16.CLOCK_SOURCE); //93.75KHz clock

                    val = this.readRegister(IO60P16.PIN_STRONG_DRIVE);
                    this.writeRegister(IO60P16.PIN_STRONG_DRIVE, (byte)(val | mask));
				}
				else
				{
					this.writeRegister(IO60P16.ENABLE_PWM_REGISTER, (byte)(val & ~mask));
                    val = this.readRegister(IO60P16.PIN_DIRECTION_REGISTER);

					if (state == IOState.Output)
					{
						this.writeRegister(IO60P16.PIN_DIRECTION_REGISTER, (byte)(val & ~mask));

						val = this.readRegister(IO60P16.PIN_STRONG_DRIVE);
						this.writeRegister(IO60P16.PIN_STRONG_DRIVE, (byte)(val | mask));
					}
					else
					{
						this.writeRegister(IO60P16.PIN_DIRECTION_REGISTER, (byte)(val | mask));

                        val = this.readRegister((byte)resistorMode);
						this.writeRegister((byte)resistorMode, (byte)(val | mask));
					}
				}

                val = this.readRegister(IO60P16.INTERRUPT_MASK_REGISTER);
				if (state == IOState.InputInterrupt)
					this.writeRegister(IO60P16.INTERRUPT_MASK_REGISTER, (byte)(val & ~mask));
				else
					this.writeRegister(IO60P16.INTERRUPT_MASK_REGISTER, (byte)(val | mask));
			}

			//We're using the 93.75KHz clock source because it gives a good resolution around the 1KHz frequency
			//while still allowing the user to select frequencies such as 10KHz, but with reduced duty cycle
			//resolution.
			public void setPWM(byte pin, double frequency, double dutyCycle)
			{
				byte pwm = 255;
				for (var i = 0; i < 30; i += 2)
					if (this.pwms[i] == pin)
						pwm = this.pwms[i + 1];

				this.writeRegister((byte)(IO60P16.PWM_SELECT_REGISTER), pwm); // (byte)((pin % 8) + (this.getPort(pin) - 6) * 8));

				byte period = (byte)(93750 / frequency);

				this.writeRegister(IO60P16.PERIOD_REGISTER, period);
				this.writeRegister((byte)(IO60P16.PULSE_WIDTH_REGISTER), (byte)(period * dutyCycle));
			}

			public bool readDigital(byte pin)
			{
				byte b = this.readRegister((byte)(IO60P16.INPUT_PORT_0_REGISTER + this.getPort(pin)));

				return (b & this.getMask(pin)) != 0;
			}

			public void writeDigital(byte pin, bool value)
			{
				byte b = this.readRegister((byte)(IO60P16.OUTPUT_PORT_0_REGISTER + this.getPort(pin)));

				if (value)
					b |= this.getMask(pin);
				else
					b = (byte)(b & ~this.getMask(pin));

				this.writeRegister((byte)(IO60P16.OUTPUT_PORT_0_REGISTER + this.getPort(pin)), b);
			}
		}

		private class ADS7830
		{
			private const byte MAX_CHANNEL = 6;
			private const byte CMD_SD_SE = 0x80;
			private const byte CMD_PD_OFF = 0x00;
			private const byte CMD_PD_ON = 0x04;
			private const byte I2C_ADDRESS = 0x48;

			private GTI.I2CBus i2c;

			public ADS7830(Socket socket)
			{
				this.i2c = new GTI.I2CBus(socket, ADS7830.I2C_ADDRESS, 400, null);
			}

			public double ReadVoltage(byte channel)
			{
				if (channel >= ADS7830.MAX_CHANNEL)
					throw new Exception("Invalid channel.");

				byte[] command = new byte[1] { (byte)(ADS7830.CMD_SD_SE | ADS7830.CMD_PD_ON) };
				byte[] read = new byte[1];

				command[0] |= (byte)((channel % 2 == 0 ? channel / 2 : (channel - 1) / 2 + 4) << 4);

				lock (this.i2c)
					this.i2c.WriteRead(command, read, 250);

				return (double)read[0] / 255 * 3.3;
			}
		}
	}
}
