using GHI.IO;
using GHI.IO.Storage;
using GHI.Processor;
using GHI.Usb;
using GHI.Usb.Host;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using EMX = GHI.Pins.EMX;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace GHIElectronics.Gadgeteer {
	/// <summary>The mainboard class for the FEZ Spider.</summary>
	public class FEZSpider : GT.Mainboard {
		private OutputPort debugLed;
		private IRemovable[] storageDevices;

		/// <summary>The name of the mainboard.</summary>
		public override string MainboardName {
			get { return "GHI Electronics FEZ Spider"; }
		}

		/// <summary>The current version of the mainboard hardware.</summary>
		public override string MainboardVersion {
			get { return "1.0"; }
		}

		private enum SpecialPurposePin {
			ETH_RX_DM = -6,
			ETH_RX_DP = -7,
			ETH_TX_DM = -8,
			ETH_TX_DP = -9,
			USBH_DM = -10,
			USBH_DP = -11,
			USB_VBUS = -12,
			USBD_DM = -13,
			USBD_DP = -14,
			RTC_BATT = -15,
			RESET = -16,
			LED_SPEED = -17,
			LED_LINK = -18,
			TCK = -19,
			TDO = -20,
			TMS = -21,
			TRST = -22,
			TDI = -23,
		}

		/// <summary>Constructs a new instance.</summary>
		public FEZSpider() {
			this.debugLed = null;
			this.storageDevices = new IRemovable[2];

			Controller.Start();

			this.NativeBitmapConverter = this.BitmapConverter;

			GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
			GT.Socket socket;

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'D', 'I' };
			socket.CpuPins[3] = EMX.IO21;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_DM;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_DP;
			socket.CpuPins[6] = EMX.IO19;
			socket.CpuPins[7] = EMX.IO75;
			socket.CpuPins[8] = EMX.IO12;
			socket.CpuPins[9] = EMX.IO11;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'Z' };
			socket.CpuPins[3] = (Cpu.Pin)SpecialPurposePin.RESET;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.TCK;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.RTC_BATT;
			socket.CpuPins[6] = (Cpu.Pin)SpecialPurposePin.TDO;
			socket.CpuPins[7] = (Cpu.Pin)SpecialPurposePin.TRST;
			socket.CpuPins[8] = (Cpu.Pin)SpecialPurposePin.TMS;
			socket.CpuPins[9] = (Cpu.Pin)SpecialPurposePin.TDI;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
			socket.SupportedTypes = new char[] { 'H', 'I' };
			socket.CpuPins[3] = EMX.IO1;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBH_DM;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBH_DP;
			socket.CpuPins[6] = EMX.IO0;
			socket.CpuPins[8] = EMX.IO12;
			socket.CpuPins[9] = EMX.IO11;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
			socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
			socket.CpuPins[3] = EMX.IO33;
			socket.CpuPins[4] = EMX.IO37;
			socket.CpuPins[5] = EMX.IO32;
			socket.CpuPins[6] = EMX.IO31;
			socket.CpuPins[7] = EMX.IO34;
			socket.CpuPins[8] = EMX.IO12;
			socket.CpuPins[9] = EMX.IO11;
			socket.SerialPortName = "COM2";
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
			socket.SupportedTypes = new char[] { 'F', 'Y' };
			socket.CpuPins[3] = EMX.IO23;
			socket.CpuPins[4] = EMX.IO43;
			socket.CpuPins[5] = EMX.IO41;
			socket.CpuPins[6] = EMX.IO44;
			socket.CpuPins[7] = EMX.IO40;
			socket.CpuPins[8] = EMX.IO39;
			socket.CpuPins[9] = EMX.IO42;
			socket.I2CBusIndirector = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
			socket.SupportedTypes = new char[] { 'C', 'S', 'Y' };
			socket.CpuPins[3] = EMX.IO18;
			socket.CpuPins[4] = EMX.IO20;
			socket.CpuPins[5] = EMX.IO22;
			socket.CpuPins[6] = EMX.IO10;
			socket.CpuPins[7] = EMX.IO36;
			socket.CpuPins[8] = EMX.IO38;
			socket.CpuPins[9] = EMX.IO35;
			socket.I2CBusIndirector = nativeI2C;
			socket.SPIModule = SPI.SPI_module.SPI2;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(7);
			socket.SupportedTypes = new char[] { 'E' };
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.LED_SPEED;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.LED_LINK;
			socket.CpuPins[6] = (Cpu.Pin)SpecialPurposePin.ETH_TX_DM;
			socket.CpuPins[7] = (Cpu.Pin)SpecialPurposePin.ETH_TX_DP;
			socket.CpuPins[8] = (Cpu.Pin)SpecialPurposePin.ETH_RX_DM;
			socket.CpuPins[9] = (Cpu.Pin)SpecialPurposePin.ETH_RX_DP;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(8);
			socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
			socket.CpuPins[3] = EMX.IO30;
			socket.CpuPins[4] = EMX.IO29;
			socket.CpuPins[5] = EMX.IO28;
			socket.CpuPins[6] = EMX.IO16;
			socket.CpuPins[7] = EMX.IO74;
			socket.CpuPins[8] = EMX.IO48;
			socket.CpuPins[9] = EMX.IO49;
			socket.SerialPortName = "COM3";
			socket.I2CBusIndirector = nativeI2C;
			socket.PWM7 = Cpu.PWMChannel.PWM_5;
			socket.PWM8 = Cpu.PWMChannel.PWM_4;
			socket.PWM9 = Cpu.PWMChannel.PWM_3;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
			socket.SupportedTypes = new char[] { 'A', 'O', 'S', 'U', 'Y' };
			socket.CpuPins[3] = EMX.IO46;
			socket.CpuPins[4] = EMX.IO6;
			socket.CpuPins[5] = EMX.IO7;
			socket.CpuPins[6] = EMX.IO15;
			socket.CpuPins[7] = EMX.IO24;
			socket.CpuPins[8] = EMX.IO25;
			socket.CpuPins[9] = EMX.IO27;
			socket.SerialPortName = "COM4";
			socket.I2CBusIndirector = nativeI2C;
			socket.SPIModule = SPI.SPI_module.SPI1;
			socket.AnalogOutput5 = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0;
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_7;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_2;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_3;
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			GT.Socket.SocketInterfaces.SetAnalogOutputFactors(socket, 3.3, 0, 10);
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
			socket.SupportedTypes = new char[] { 'A', 'I', 'T', 'X' };
			socket.CpuPins[3] = EMX.IO45;
			socket.CpuPins[4] = EMX.IO5;
			socket.CpuPins[5] = EMX.IO8;
			socket.CpuPins[6] = EMX.IO73;
			socket.CpuPins[7] = EMX.IO72;
			socket.CpuPins[8] = EMX.IO12;
			socket.CpuPins[9] = EMX.IO11;
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_6;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_1;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_0;
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
			socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
			socket.CpuPins[3] = EMX.IO26;
			socket.CpuPins[4] = EMX.IO3;
			socket.CpuPins[5] = EMX.IO2;
			socket.CpuPins[6] = EMX.IO9;
			socket.CpuPins[7] = EMX.IO14;
			socket.CpuPins[8] = EMX.IO13;
			socket.CpuPins[9] = EMX.IO50;
			socket.SerialPortName = "COM1";
			socket.I2CBusIndirector = nativeI2C;
			socket.PWM7 = Cpu.PWMChannel.PWM_1;
			socket.PWM8 = Cpu.PWMChannel.PWM_0;
			socket.PWM9 = Cpu.PWMChannel.PWM_2;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
			socket.SupportedTypes = new char[] { 'B', 'Y' };
			socket.CpuPins[3] = EMX.IO70;
			socket.CpuPins[4] = EMX.IO57;
			socket.CpuPins[5] = EMX.IO58;
			socket.CpuPins[6] = EMX.IO59;
			socket.CpuPins[7] = EMX.IO60;
			socket.CpuPins[8] = EMX.IO63;
			socket.CpuPins[9] = EMX.IO61;
			socket.I2CBusIndirector = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(13);
			socket.SupportedTypes = new char[] { 'G' };
			socket.CpuPins[3] = EMX.IO51;
			socket.CpuPins[4] = EMX.IO52;
			socket.CpuPins[5] = EMX.IO53;
			socket.CpuPins[6] = EMX.IO54;
			socket.CpuPins[7] = EMX.IO55;
			socket.CpuPins[8] = EMX.IO56;
			socket.CpuPins[9] = EMX.IO17;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(14);
			socket.SupportedTypes = new char[] { 'R', 'Y' };
			socket.CpuPins[3] = EMX.IO69;
			socket.CpuPins[4] = EMX.IO65;
			socket.CpuPins[5] = EMX.IO66;
			socket.CpuPins[6] = EMX.IO67;
			socket.CpuPins[7] = EMX.IO68;
			socket.CpuPins[8] = EMX.IO62;
			socket.CpuPins[9] = EMX.IO64;
			socket.I2CBusIndirector = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);
		}

		/// <summary>The storage device volume names supported by this mainboard.</summary>
		/// <returns>The volume names.</returns>
		public override string[] GetStorageDeviceVolumeNames() {
			return new string[] { "SD", "USB" };
		}

		/// <summary>Mounts the device with the given name.</summary>
		/// <param name="volumeName">The device to mount.</param>
		/// <returns>Whether or not the mount was successful.</returns>
		public override bool MountStorageDevice(string volumeName) {
			try {
				if (volumeName == "SD" && this.storageDevices[0] == null) {
					this.storageDevices[0] = new SDCard();
					this.storageDevices[0].Mount();

					return true;
				}
				else if (volumeName == "USB" && this.storageDevices[1] == null) {
					foreach (BaseDevice dev in Controller.GetConnectedDevices()) {
						if (dev.GetType() == typeof(MassStorage)) {
							this.storageDevices[1] = (MassStorage)dev;
							this.storageDevices[1].Mount();

							return true;
						}
					}
				}
			}
			catch {
			}

			return false;
		}

		/// <summary>Unmounts the device with the given name.</summary>
		/// <param name="volumeName">The device to unmount.</param>
		/// <returns>Whether or not the unmount was successful.</returns>
		public override bool UnmountStorageDevice(string volumeName) {
			if (volumeName == "SD" && this.storageDevices[0] != null) {
				this.storageDevices[0].Dispose();
				this.storageDevices[0] = null;
			}
			else if (volumeName == "USB" && this.storageDevices[1] != null) {
				this.storageDevices[1].Dispose();
				this.storageDevices[1] = null;
			}
			else {
				return false;
			}

			return true;
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
				this.debugLed = new OutputPort(EMX.IO47, on);

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
				case 90: Display.CurrentRotation = Display.Rotation.Clockwise90; break;
				case 180: Display.CurrentRotation = Display.Rotation.Half; break;
				case 270: Display.CurrentRotation = Display.Rotation.CounterClockwise90; break;
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