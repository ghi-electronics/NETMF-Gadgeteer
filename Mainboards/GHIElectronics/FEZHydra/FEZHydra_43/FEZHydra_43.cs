using GHI.IO;
using GHI.IO.Storage;
using GHI.Pins;
using GHI.Processor;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using FEZHydraPins = GHI.Pins.FEZHydra;

namespace GHIElectronics.Gadgeteer {
	/// <summary>The mainboard class for the FEZ Hydra.</summary>
	public class FEZHydra : GT.Mainboard {
		private OutputPort debugLed;
		private SDCard sdCard;

		/// <summary>The name of the mainboard.</summary>
		public override string MainboardName {
			get { return "GHI Electronics FEZ Hydra"; }
		}

		/// <summary>The current version of the mainboard hardware.</summary>
		public override string MainboardVersion {
			get { return "1.2"; }
		}

		/// <summary>Constructs a new instance.</summary>
		public FEZHydra() {
			this.debugLed = null;
			this.sdCard = null;

			this.NativeBitmapConverter = this.BitmapConverter;

			GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
			GT.Socket socket;

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'Z' };
			socket.CpuPins[3] = GT.Socket.UnnumberedPin;
			socket.CpuPins[4] = GT.Socket.UnnumberedPin;
			socket.CpuPins[5] = GT.Socket.UnnumberedPin;
			socket.CpuPins[6] = GT.Socket.UnnumberedPin;
			socket.CpuPins[7] = GT.Socket.UnnumberedPin;
			socket.CpuPins[8] = GT.Socket.UnnumberedPin;
			socket.CpuPins[9] = GT.Socket.UnnumberedPin;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'D' };
			socket.CpuPins[3] = FEZHydraPins.Socket2.Pin3;
			socket.CpuPins[4] = GT.Socket.UnnumberedPin;
			socket.CpuPins[5] = GT.Socket.UnnumberedPin;
			socket.CpuPins[6] = FEZHydraPins.Socket2.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket2.Pin7;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
			socket.SupportedTypes = new char[] { 'S', 'X' };
			socket.CpuPins[3] = FEZHydraPins.Socket3.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket3.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket3.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket3.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket3.Pin7;
			socket.CpuPins[8] = FEZHydraPins.Socket3.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket3.Pin9;
			socket.I2CBusIndirector = nativeI2C;
			socket.SPIModule = SPI.SPI_module.SPI1;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
			socket.SupportedTypes = new char[] { 'S', 'U', 'X' };
			socket.CpuPins[3] = FEZHydraPins.Socket4.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket4.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket4.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket4.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket4.Pin7;
			socket.CpuPins[8] = FEZHydraPins.Socket4.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket4.Pin9;
			socket.SerialPortName = "COM3";
			socket.I2CBusIndirector = nativeI2C;
			socket.SPIModule = SPI.SPI_module.SPI1;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
			socket.SupportedTypes = new char[] { 'I', 'U', 'X' };
			socket.CpuPins[3] = FEZHydraPins.Socket5.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket5.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket5.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket5.Pin6;
			socket.CpuPins[8] = FEZHydraPins.Socket5.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket5.Pin9;
			socket.SerialPortName = "COM1";
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
			socket.SupportedTypes = new char[] { 'I', 'U', 'X' };
			socket.CpuPins[3] = FEZHydraPins.Socket6.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket6.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket6.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket6.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket6.Pin7;
			socket.CpuPins[8] = FEZHydraPins.Socket6.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket6.Pin9;
			socket.SerialPortName = "COM4";
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(7);
			socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
			socket.CpuPins[3] = FEZHydraPins.Socket7.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket7.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket7.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket7.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket7.Pin7;
			socket.CpuPins[8] = FEZHydraPins.Socket7.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket7.Pin9;
			socket.I2CBusIndirector = nativeI2C;
			socket.SerialPortName = "COM2";
			socket.PWM7 = Cpu.PWMChannel.PWM_0;
			socket.PWM8 = Cpu.PWMChannel.PWM_1;
			socket.PWM9 = Cpu.PWMChannel.PWM_2;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(8);
			socket.SupportedTypes = new char[] { 'F', 'Y' };
			socket.CpuPins[3] = FEZHydraPins.Socket8.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket8.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket8.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket8.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket8.Pin7;
			socket.CpuPins[8] = FEZHydraPins.Socket8.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket8.Pin9;
			socket.I2CBusIndirector = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
			socket.SupportedTypes = new char[] { 'Y' };
			socket.CpuPins[3] = FEZHydraPins.Socket9.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket9.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket9.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket9.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket9.Pin7;
			socket.CpuPins[8] = FEZHydraPins.Socket9.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket9.Pin9;
			socket.I2CBusIndirector = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
			socket.SupportedTypes = new char[] { 'R', 'Y' };
			socket.CpuPins[3] = FEZHydraPins.Socket10.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket10.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket10.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket10.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket10.Pin7;
			socket.CpuPins[8] = FEZHydraPins.Socket10.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket10.Pin9;
			socket.I2CBusIndirector = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
			socket.SupportedTypes = new char[] { 'G', 'Y' };
			socket.CpuPins[3] = FEZHydraPins.Socket11.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket11.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket11.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket11.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket11.Pin7;
			socket.CpuPins[8] = FEZHydraPins.Socket11.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket11.Pin9;
			socket.I2CBusIndirector = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
			socket.SupportedTypes = new char[] { 'B', 'Y' };
			socket.CpuPins[3] = FEZHydraPins.Socket12.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket12.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket12.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket12.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket12.Pin7;
			socket.CpuPins[8] = FEZHydraPins.Socket12.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket12.Pin9;
			socket.I2CBusIndirector = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(13);
			socket.SupportedTypes = new char[] { 'A', 'T', 'Y' };
			socket.CpuPins[3] = FEZHydraPins.Socket13.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket13.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket13.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket13.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket13.Pin7;
			socket.CpuPins[8] = FEZHydraPins.Socket13.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket13.Pin9;
			socket.I2CBusIndirector = nativeI2C;
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_4;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_3;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_1;
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(14);
			socket.SupportedTypes = new char[] { 'A', 'X' };
			socket.CpuPins[3] = FEZHydraPins.Socket14.Pin3;
			socket.CpuPins[4] = FEZHydraPins.Socket14.Pin4;
			socket.CpuPins[5] = FEZHydraPins.Socket14.Pin5;
			socket.CpuPins[6] = FEZHydraPins.Socket14.Pin6;
			socket.CpuPins[7] = FEZHydraPins.Socket14.Pin7;
			socket.CpuPins[8] = FEZHydraPins.Socket14.Pin8;
			socket.CpuPins[9] = FEZHydraPins.Socket14.Pin9;
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_5;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_2;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_0;
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			GT.Socket.SocketInterfaces.RegisterSocket(socket);
		}

		/// <summary>The storage device volume names supported by this mainboard.</summary>
		/// <returns>The volume names.</returns>
		public override string[] GetStorageDeviceVolumeNames() {
			return new string[] { "SD" };
		}

		/// <summary>Mounts the device with the given name.</summary>
		/// <param name="volumeName">The device to mount.</param>
		/// <returns>Whether or not the mount was successful.</returns>
		public override bool MountStorageDevice(string volumeName) {
			if (volumeName == "SD") {
				this.sdCard = new SDCard();
				this.sdCard.Mount();

				return true;
			}

			return false;
		}

		/// <summary>Unmounts the device with the given name.</summary>
		/// <param name="volumeName">The device to unmount.</param>
		/// <returns>Whether or not the unmount was successful.</returns>
		public override bool UnmountStorageDevice(string volumeName) {
			if (volumeName == "SD") {
				this.sdCard.Dispose();
				this.sdCard = null;

				return true;
			}

			return false;
		}

		/// <summary>Ensures that the RGB socket pins are available by disabling the display controller if needed.</summary>
		public override void EnsureRgbSocketPinsAvailable() {
			if (Display.Disable()) {
				Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
				Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

				Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
			}
		}

		/// <summary>Sets the state of the debug LED.</summary>
		/// <param name="on">The new state.</param>
		public override void SetDebugLED(bool on) {
			if (this.debugLed == null)
				this.debugLed = new OutputPort(FEZHydraPins.DebugLed, on);

			this.debugLed.Write(on);
		}

		/// <summary>Sets the programming mode of the device.</summary>
		/// <param name="programmingInterface">The new programming mode.</param>
		public override void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface) {
			throw new NotSupportedException();
		}

		/// <summary>This performs post-initialization tasks for the mainboard. It is called by Gadgeteer.Program.Run and does not need to be called manually.</summary>
		public override void PostInit() {
		}

		/// <summary>
		/// Configure the onboard display controller to fulfil the requirements of a display using the RGB sockets. If doing this requires rebooting, then the method must reboot and not return. If
		/// there is no onboard display controller, then NotSupportedException must be thrown.
		/// </summary>
		/// <param name="displayModel">Display model name.</param>
		/// <param name="width">Display physical width in pixels, ignoring the orientation setting.</param>
		/// <param name="height">Display physical height in lines, ignoring the orientation setting.</param>
		/// <param name="orientationDeg">Display orientation in degrees.</param>
		/// <param name="timing">The required timings from an LCD controller.</param>
		protected override void OnOnboardControllerDisplayConnected(string displayModel, int width, int height, int orientationDeg, GTM.Module.DisplayModule.TimingRequirements timing) {
			switch (orientationDeg) {
				case 0: Display.CurrentRotation = Display.Rotation.Normal; break;
				default: throw new ArgumentOutOfRangeException("orientationDeg", "orientationDeg must be 0, 90, 180, or 270.");
			}

			Display.Height = (uint)height;
			Display.HorizontalBackPorch = timing.HorizontalBackPorch;
			Display.HorizontalFrontPorch = timing.HorizontalFrontPorch;
			Display.HorizontalSyncPolarity = timing.HorizontalSyncPulseIsActiveHigh;
			Display.HorizontalSyncPulseWidth = timing.HorizontalSyncPulseWidth;
			Display.OutputEnableIsFixed = timing.UsesCommonSyncPin; //not the proper property, but we needed it;
			Display.OutputEnablePolarity = timing.CommonSyncPinIsActiveHigh; //not the proper property, but we needed it;
			Display.PixelClockRateKHz = timing.MaximumClockSpeed;
			Display.PixelPolarity = timing.PixelDataIsValidOnClockRisingEdge;
			Display.VerticalBackPorch = timing.VerticalBackPorch;
			Display.VerticalFrontPorch = timing.VerticalFrontPorch;
			Display.VerticalSyncPolarity = timing.VerticalSyncPulseIsActiveHigh;
			Display.VerticalSyncPulseWidth = timing.VerticalSyncPulseWidth;
			Display.Width = (uint)width;

			if (Display.Save()) {
				Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
				Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

				Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
			}
		}

		private void BitmapConverter(Bitmap bitmap, byte[] pixelBytes, GT.Mainboard.BPP bpp) {
			if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE) throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

			GHI.Utilities.Bitmaps.Convert(bitmap, GHI.Utilities.Bitmaps.BitsPerPixel.BPP16_BGR_BE, pixelBytes);
		}

		private class InteropI2CBus : GT.SocketInterfaces.I2CBus {
			private SoftwareI2CBus softwareBus;

			public override ushort Address { get; set; }

			public override int Timeout { get; set; }

			public override int ClockRateKHz { get; set; }

			public InteropI2CBus(GT.Socket socket, GT.Socket.Pin sdaPin, GT.Socket.Pin sclPin, ushort address, int clockRateKHz, GTM.Module module) {
				this.Address = address;
				this.ClockRateKHz = clockRateKHz;

				this.softwareBus = new SoftwareI2CBus(socket.CpuPins[(int)sclPin], socket.CpuPins[(int)sdaPin]);
			}

			public override void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int numWritten, out int numRead) {
				this.softwareBus.WriteRead((byte)this.Address, writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out numWritten, out numRead);
			}
		}
	}
}