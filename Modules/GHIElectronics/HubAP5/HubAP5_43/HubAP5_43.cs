using System;
using System.Collections;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A HubAP5 module for Microsoft .NET Gadgeteer</summary>
	public class HubAP5 : GTM.Module {
		private Socket[] sockets = new Socket[8];
		private IO60P16 io60;
		private ADS7830 ads;
		private Hashtable socketMap;
		private byte[] pinMap;
		private string[] typeMap;

		/// <summary>Returns the socket number for socket 1 on the hub.</summary>
		public int HubSocket1 { get { return this.sockets[0].SocketNumber; } }

		/// <summary>Returns the socket number for socket 2 on the hub.</summary>
		public int HubSocket2 { get { return this.sockets[1].SocketNumber; } }

		/// <summary>Returns the socket number for socket 3 on the hub.</summary>
		public int HubSocket3 { get { return this.sockets[2].SocketNumber; } }

		/// <summary>Returns the socket number for socket 4 on the hub.</summary>
		public int HubSocket4 { get { return this.sockets[3].SocketNumber; } }

		/// <summary>Returns the socket number for socket 5 on the hub.</summary>
		public int HubSocket5 { get { return this.sockets[4].SocketNumber; } }

		/// <summary>Returns the socket number for socket 6 on the hub.</summary>
		public int HubSocket6 { get { return this.sockets[5].SocketNumber; } }

		/// <summary>Returns the socket number for socket 7 on the hub.</summary>
		public int HubSocket7 { get { return this.sockets[6].SocketNumber; } }

		/// <summary>Returns the socket number for socket 8 on the hub.</summary>
		public int HubSocket8 { get { return this.sockets[7].SocketNumber; } }

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public HubAP5(int socketNumber) {
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

			for (int i = 0; i < 8; i++) {
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

		private byte GetPin(Socket socket, Socket.Pin pin) {
			return this.pinMap[(int)(this.socketMap[socket.SocketNumber]) * 7 + (int)(pin) - 3];
		}

		private class DigitalInputImplementation : GTI.DigitalInput {
			private IO60P16 io60;
			private byte pin;

			public DigitalInputImplementation(byte pin, GTI.GlitchFilterMode glitchFilter, GTI.ResistorMode resistorMode, IO60P16 io60) {
				this.io60 = io60;
				this.pin = pin;

				this.io60.SetIOMode(this.pin, IO60P16.IOState.Input, resistorMode);
			}

			public override bool Read() {
				return this.io60.ReadDigital(this.pin);
			}
		}

		private class DigitalOutputImplementation : GTI.DigitalOutput {
			private IO60P16 io60;
			private byte pin;

			public DigitalOutputImplementation(byte pin, bool initialState, IO60P16 io60) {
				this.io60 = io60;
				this.pin = pin;

				this.io60.SetIOMode(this.pin, IO60P16.IOState.Output, GTI.ResistorMode.Disabled);
				this.Write(initialState);
			}

			public override bool Read() {
				return this.io60.ReadDigital(this.pin);
			}

			public override void Write(bool state) {
				this.io60.WriteDigital(this.pin, state);
			}
		}

		private class DigitalIOImplementation : GTI.DigitalIO {
			private IO60P16 io60;
			private byte pin;
			private GTI.IOMode mode;
			private GTI.ResistorMode resistorMode;

			public override GTI.IOMode Mode {
				get {
					return this.mode;
				}

				set {
					this.mode = value;
					this.io60.SetIOMode(this.pin, this.mode == GTI.IOMode.Input ? IO60P16.IOState.Input : IO60P16.IOState.Output, this.resistorMode);
				}
			}

			public DigitalIOImplementation(byte pin, bool initialState, GTI.GlitchFilterMode glitchFilter, GTI.ResistorMode resistorMode, IO60P16 io60) {
				this.io60 = io60;
				this.pin = pin;
				this.resistorMode = resistorMode;
				this.Mode = GTI.IOMode.Input;
				this.io60.WriteDigital(this.pin, initialState);
			}

			public override bool Read() {
				this.Mode = GTI.IOMode.Input;
				return this.io60.ReadDigital(this.pin);
			}

			public override void Write(bool state) {
				this.Mode = GTI.IOMode.Output;
				this.io60.WriteDigital(this.pin, state);
			}
		}

		private class AnalogInputImplementation : GTI.AnalogInput {
			private ADS7830 ads;
			private IO60P16 io60;
			private byte pin;
			private byte channel;
			private bool active;

			public override bool IsActive {
				get {
					return this.active;
				}

				set {
					if (this.active == value)
						return;

					this.active = value;

					if (this.active)
						this.io60.SetIOMode(this.pin, IO60P16.IOState.Input, IO60P16.ResistorMode.Floating);
				}
			}

			public AnalogInputImplementation(byte pin, ADS7830 ads, IO60P16 io60) {
				this.ads = ads;
				this.io60 = io60;
				this.pin = pin;
				this.active = false;

				var channels = new byte[12] { 0x50, 0, 0x51, 1, 0x52, 2, 0x40, 3, 0x41, 4, 0x42, 5 };
				for (int i = 0; i < 12; i += 2)
					if (channels[i] == this.pin)
						this.channel = channels[i + 1];
			}

			public override double ReadVoltage() {
				return this.ads.ReadVoltage(this.channel);
			}

			public override double ReadProportion() {
				return this.ReadVoltage() / 3.3;
			}
		}

		private class PwmOutputImplementation : GTI.PwmOutput {
			private IO60P16 io60;
			private byte pin;
			private bool isActive;

			public override bool IsActive {
				get {
					return this.isActive;
				}

				set {
					if (this.isActive == value)
						return;

					this.isActive = !this.isActive;

					if (this.isActive) {
						this.io60.SetIOMode(this.pin, IO60P16.IOState.Pwm, GTI.ResistorMode.Disabled);
						this.io60.SetPWM(this.pin, 1, 0.5);
					}
					else {
						this.io60.SetIOMode(this.pin, IO60P16.IOState.Input, GTI.ResistorMode.Disabled);
					}
				}
			}

			public PwmOutputImplementation(byte pin, bool invert, IO60P16 io60) {
				this.io60 = io60;
				this.pin = pin;
				this.isActive = false;
			}

			public override void Set(double frequency, double dutyCycle) {
				if (frequency <= 0) throw new ArgumentOutOfRangeException("frequency", "frequency must be positive.");
				if (dutyCycle < 0 || dutyCycle > 1) throw new ArgumentOutOfRangeException("dutyCycle", "dutyCycle must be between zero and one.");

				this.IsActive = true;

				this.io60.SetPWM(this.pin, frequency, dutyCycle);
			}

			public override void Set(uint period, uint highTime, GTI.PwmScaleFactor factor) {
				if (period == 0) throw new ArgumentOutOfRangeException("period", "pedior must be positive.");
				if (highTime > period) throw new ArgumentOutOfRangeException("highTime", "highTime must be no more than period.");

				double frequency = 0;

				switch (factor) {
					case GTI.PwmScaleFactor.Milliseconds: frequency = 1000 / period; break;
					case GTI.PwmScaleFactor.Microseconds: frequency = 1000000 / period; break;
					case GTI.PwmScaleFactor.Nanoseconds: frequency = 1000000000 / period; break;
				}

				this.Set(frequency, (double)highTime / (double)period);
			}
		}

		private class InterruptInputImplementation : GTI.InterruptInput {
			private IO60P16 io60;
			private byte pin;

			public InterruptInputImplementation(byte pin, GTI.GlitchFilterMode glitchFilter, GTI.ResistorMode resistorMode, GTI.InterruptMode interruptMode, IO60P16 io60) {
				this.io60 = io60;
				this.pin = pin;

				this.io60.SetIOMode(this.pin, IO60P16.IOState.InputInterrupt, resistorMode);
				this.io60.RegisterInterruptHandler(this.pin, interruptMode, this.RaiseInterrupt);
			}

			public override bool Read() {
				return this.io60.ReadDigital(this.pin);
			}

			protected override void OnInterruptFirstSubscribed() {
			}

			protected override void OnInterruptLastUnsubscribed() {
			}
		}

		private class IO60P16 {
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

			private const byte CLOCK_SOURCE_32KHZ = 0x00;
			private const byte CLOCK_SOURCE_24MHZ = 0x01;
			private const byte CLOCK_SOURCE_1MHZ = 0x02;
			private const byte CLOCK_SOURCE_94KHZ = 0x03;
			private const byte CLOCK_SOURCE_367HZ = 0x04;

			private GTI.I2CBus io60Chip;
			private GTI.InterruptInput interrupt;
			private ArrayList interruptHandlers;
			private byte[] write2;
			private byte[] write1;
			private byte[] read1;
			private byte[] pwms;

			public delegate void InterruptHandler(bool state);

			public enum IOState {
				InputInterrupt,
				Input,
				Output,
				Pwm
			}

			public enum ResistorMode {
				PullUp = IO60P16.PIN_PULL_UP,
				PullDown = IO60P16.PIN_PULL_DOWN,
				Floating = IO60P16.PIN_HIGH_IMPEDENCE,
			}

			public IO60P16(Socket socket) {
				this.interruptHandlers = new ArrayList();
				this.write2 = new byte[2];
				this.write1 = new byte[1];
				this.read1 = new byte[1];
				this.pwms = new byte[30] { 0x60, 0, 0x61, 1, 0x62, 2, 0x63, 3, 0x64, 4, 0x65, 5, 0x66, 6, 0x67, 7, 0x70, 8, 0x71, 9, 0x72, 10, 0x73, 11, 0x74, 12, 0x75, 13, 0x76, 14 };

				this.io60Chip = GTI.I2CBusFactory.Create(socket, 0x20, 100, null);

				this.interrupt = GTI.InterruptInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.On, GTI.ResistorMode.Disabled, GTI.InterruptMode.RisingEdge, null);
				this.interrupt.Interrupt += this.OnInterrupt;
			}

			public void RegisterInterruptHandler(byte pin, GTI.InterruptMode mode, InterruptHandler handler) {
				InterruptRegistraton reg = new InterruptRegistraton();
				reg.handler = handler;
				reg.mode = mode;
				reg.pin = pin;

				lock (this.interruptHandlers)
					this.interruptHandlers.Add(reg);
			}

			public void SetIOMode(byte pin, IOState state, GTI.ResistorMode resistorMode) {
				switch (resistorMode) {
					case GTI.ResistorMode.Disabled: this.SetIOMode(pin, state, ResistorMode.Floating); break;
					case GTI.ResistorMode.PullDown: this.SetIOMode(pin, state, ResistorMode.PullDown); break;
					case GTI.ResistorMode.PullUp: this.SetIOMode(pin, state, ResistorMode.PullUp); break;
				}
			}

			public void SetIOMode(byte pin, IOState state, ResistorMode resistorMode) {
				this.WriteRegister(IO60P16.PORT_SELECT_REGISTER, this.GetPort(pin));

				byte mask = this.GetMask(pin);
				byte val = this.ReadRegister(IO60P16.ENABLE_PWM_REGISTER);

				if (state == IOState.Pwm) {
					this.WriteRegister(IO60P16.ENABLE_PWM_REGISTER, (byte)(val | mask));

					this.WriteDigital(pin, true);

					val = this.ReadRegister(IO60P16.PIN_STRONG_DRIVE);
					this.WriteRegister(IO60P16.PIN_STRONG_DRIVE, (byte)(val | mask));
				}
				else {
					this.WriteRegister(IO60P16.ENABLE_PWM_REGISTER, (byte)(val & ~mask));
					val = this.ReadRegister(IO60P16.PIN_DIRECTION_REGISTER);

					if (state == IOState.Output) {
						this.WriteRegister(IO60P16.PIN_DIRECTION_REGISTER, (byte)(val & ~mask));

						val = this.ReadRegister(IO60P16.PIN_STRONG_DRIVE);
						this.WriteRegister(IO60P16.PIN_STRONG_DRIVE, (byte)(val | mask));
					}
					else {
						this.WriteRegister(IO60P16.PIN_DIRECTION_REGISTER, (byte)(val | mask));

						val = this.ReadRegister((byte)resistorMode);
						this.WriteRegister((byte)resistorMode, (byte)(val | mask));
					}
				}

				val = this.ReadRegister(IO60P16.INTERRUPT_MASK_REGISTER);
				if (state == IOState.InputInterrupt)
					this.WriteRegister(IO60P16.INTERRUPT_MASK_REGISTER, (byte)(val & ~mask));
				else
					this.WriteRegister(IO60P16.INTERRUPT_MASK_REGISTER, (byte)(val | mask));
			}

			public void SetPWM(byte pin, double frequency, double dutyCycle) {
				byte pwm = 255;
				for (var i = 0; i < 30; i += 2)
					if (this.pwms[i] == pin)
						pwm = this.pwms[i + 1];

				var period = 0.0;
				byte clockSource = 0;

				if (frequency <= 1.45) {
					throw new ArgumentOutOfRangeException("frequency", "The frequency is too low.");
				}
				else if (frequency <= 125.5) {
					period = 367.6 / frequency;
					clockSource = IO60P16.CLOCK_SOURCE_367HZ;
				}
				else if (frequency <= 367.7) {
					period = 32000.0 / frequency;
					clockSource = IO60P16.CLOCK_SOURCE_32KHZ;
				}
				else if (frequency <= 5882.4) {
					period = 93750.0 / frequency;
					clockSource = IO60P16.CLOCK_SOURCE_94KHZ;
				}
				else if (frequency <= 94117.7) {
					period = 1500000.0 / frequency;
					clockSource = IO60P16.CLOCK_SOURCE_1MHZ;
				}
				else if (frequency <= 12000000.0) {
					period = 24000000.0 / frequency;
					clockSource = IO60P16.CLOCK_SOURCE_24MHZ;
				}
				else {
					throw new ArgumentOutOfRangeException("frequency", "The frequency is too high.");
				}

				this.WriteRegister(IO60P16.PWM_SELECT_REGISTER, pwm);
				this.WriteRegister(IO60P16.PWM_CONFIG, clockSource);
				this.WriteRegister(IO60P16.PERIOD_REGISTER, (byte)period);
				this.WriteRegister((byte)(IO60P16.PULSE_WIDTH_REGISTER), (byte)((byte)period * dutyCycle));
			}

			public bool ReadDigital(byte pin) {
				byte b = this.ReadRegister((byte)(IO60P16.INPUT_PORT_0_REGISTER + this.GetPort(pin)));

				return (b & this.GetMask(pin)) != 0;
			}

			public void WriteDigital(byte pin, bool value) {
				byte b = this.ReadRegister((byte)(IO60P16.OUTPUT_PORT_0_REGISTER + this.GetPort(pin)));

				if (value)
					b |= this.GetMask(pin);
				else
					b = (byte)(b & ~this.GetMask(pin));

				this.WriteRegister((byte)(IO60P16.OUTPUT_PORT_0_REGISTER + this.GetPort(pin)), b);
			}

			private byte GetPort(byte pin) {
				return (byte)(pin >> 4);
			}

			private byte GetMask(byte pin) {
				return (byte)(1 << (pin & 0x0F));
			}

			private void WriteRegister(byte register, byte value) {
				lock (this.io60Chip) {
					write2[0] = register;
					write2[1] = value;
					this.io60Chip.Write(write2);
				}
			}

			private byte ReadRegister(byte register) {
				byte result;

				lock (this.io60Chip) {
					write1[0] = register;
					this.io60Chip.WriteRead(write1, read1);
					result = read1[0];
				}

				return result;
			}

			private byte[] ReadRegisters(byte register, uint count) {
				byte[] result = new byte[count];

				lock (this.io60Chip) {
					write1[0] = register;
					this.io60Chip.WriteRead(write1, result);
				}

				return result;
			}

			private void OnInterrupt(GTI.InterruptInput sender, bool value) {
				ArrayList interruptedPins = new ArrayList();

				byte[] intPorts = this.ReadRegisters(IO60P16.INTERRUPT_PORT_0_REGISTER, 8);
				for (byte i = 0; i < 8; i++)
					for (int j = 1, k = 0; j <= 128; j <<= 1, k++)
						if ((intPorts[i] & j) != 0)
							interruptedPins.Add((i << 4) | k);

				foreach (int pin in interruptedPins) {
					lock (this.interruptHandlers) {
						foreach (InterruptRegistraton reg in this.interruptHandlers) {
							if (reg.pin == pin) {
								bool val = this.ReadDigital((byte)pin);
								if ((reg.mode == GTI.InterruptMode.RisingEdge && val) || (reg.mode == GTI.InterruptMode.FallingEdge && !val) || reg.mode == GTI.InterruptMode.RisingAndFallingEdge)
									reg.handler(val);
							}
						}
					}
				}
			}
			private class InterruptRegistraton {
				public GTI.InterruptMode mode;
				public InterruptHandler handler;
				public byte pin;
			}
		}

		private class ADS7830 {
			private const byte MAX_CHANNEL = 6;
			private const byte CMD_SD_SE = 0x80;
			private const byte CMD_PD_OFF = 0x00;
			private const byte CMD_PD_ON = 0x04;
			private const byte I2C_ADDRESS = 0x48;

			private GTI.I2CBus i2c;

			public ADS7830(Socket socket) {
				this.i2c = GTI.I2CBusFactory.Create(socket, ADS7830.I2C_ADDRESS, 400, null);
			}

			public double ReadVoltage(byte channel) {
				if (channel >= ADS7830.MAX_CHANNEL)
					throw new ArgumentOutOfRangeException("channel", "Invalid channel.");

				byte[] command = new byte[1] { (byte)(ADS7830.CMD_SD_SE | ADS7830.CMD_PD_ON) };
				byte[] read = new byte[1];

				command[0] |= (byte)((channel % 2 == 0 ? channel / 2 : (channel - 1) / 2 + 4) << 4);

				lock (this.i2c)
					this.i2c.WriteRead(command, read);

				return (double)read[0] / 255 * 3.3;
			}
		}
	}
}