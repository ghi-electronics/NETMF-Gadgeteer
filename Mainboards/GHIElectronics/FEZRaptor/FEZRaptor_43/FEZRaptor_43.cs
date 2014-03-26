using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GT = Gadgeteer;
using G400 = GHI.Pins.G400;
using GHI.Hardware.Storage;
using GHI.Hardware;
using GHI.System;
using GTM = Gadgeteer.Modules;

namespace GHIElectronics.Gadgeteer
{
	/// <summary>
	/// Support class for GHI Electronics FEZRaptor for Microsoft .NET Gadgeteer
	/// </summary>
	public class FEZRaptor : GT.Mainboard
	{
		/// <summary>
		/// Instantiates a new FEZRaptor mainboard
		/// </summary>
		public FEZRaptor()
		{
			this.NativeBitmapConverter = this.BitmapConverter;

            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
			
			GT.Socket socket;

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'S', 'U', 'Y' };
			socket.CpuPins[3] = G400.PB0;
			socket.CpuPins[4] = G400.PA7;
			socket.CpuPins[5] = G400.PA8;
			socket.CpuPins[6] = G400.PB5;
			socket.CpuPins[7] = G400.PA22;
			socket.CpuPins[8] = G400.PA21;
			socket.CpuPins[9] = G400.PA23;
			socket.SPIModule = SPI.SPI_module.SPI2;
			socket.SerialPortName = "COM4";
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'A', 'I', 'X' };
			socket.CpuPins[3] = G400.PB14;
			socket.CpuPins[4] = G400.PB15;
			socket.CpuPins[5] = G400.PB16;
			socket.CpuPins[6] = G400.PB18;
			socket.CpuPins[8] = G400.PA30;
			socket.CpuPins[9] = G400.PA31;
			

			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_3;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_4;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_5;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
			socket.SupportedTypes = new char[] { 'S', 'X' };
			socket.CpuPins[3] = G400.PB1;
			socket.CpuPins[4] = G400.PC23;
			socket.CpuPins[5] = G400.PB3;
			socket.CpuPins[6] = G400.PA28;
			socket.CpuPins[7] = G400.PA12;
			socket.CpuPins[8] = G400.PA11;
			socket.CpuPins[9] = G400.PA13;
			socket.SPIModule = SPI.SPI_module.SPI1;
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
			socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
			socket.CpuPins[3] = G400.PA27;
			socket.CpuPins[4] = G400.PA0;
			socket.CpuPins[5] = G400.PA1;
			socket.CpuPins[6] = G400.PA2;
			socket.CpuPins[7] = G400.PA3;
			socket.CpuPins[8] = G400.PA30;
			socket.CpuPins[9] = G400.PA31;
			socket.SerialPortName = "COM2";
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
			socket.SupportedTypes = new char[] { 'Z' };
			socket.CpuPins[3] = (Cpu.Pin)SpecialPurposePin.RESET;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.TCK;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.RTC_BATT;
			socket.CpuPins[6] = (Cpu.Pin)SpecialPurposePin.TDO;
			socket.CpuPins[7] = (Cpu.Pin)SpecialPurposePin.TRST;
			socket.CpuPins[8] = (Cpu.Pin)SpecialPurposePin.TMS;
			socket.CpuPins[9] = (Cpu.Pin)SpecialPurposePin.TDI;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
			socket.SupportedTypes = new char[] { 'H', 'I' };
			socket.CpuPins[3] = G400.PD4;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_C_DM;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_C_DP;
			socket.CpuPins[6] = G400.PA24;
			socket.CpuPins[8] = G400.PA30;
			socket.CpuPins[9] = G400.PA31;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(7);
			socket.SupportedTypes = new char[] { 'H', 'I' };
			socket.CpuPins[3] = G400.PA25;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_B_DM;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_B_DP;
			socket.CpuPins[6] = G400.PA4;
			socket.CpuPins[8] = G400.PA30;
			socket.CpuPins[9] = G400.PA31;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(8);
			socket.SupportedTypes = new char[] { 'D', 'I' };
			socket.CpuPins[3] = G400.PD6;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_A_DM;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_A_DP;
			socket.CpuPins[6] = G400.PD0;
			socket.CpuPins[7] = G400.PD5;
			socket.CpuPins[8] = G400.PA30;
			socket.CpuPins[9] = G400.PA31;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
			socket.SupportedTypes = new char[] { 'F', 'Y' };
			socket.CpuPins[3] = G400.PD7;
			socket.CpuPins[4] = G400.PA15;
			socket.CpuPins[5] = G400.PA18;
			socket.CpuPins[6] = G400.PA16;
			socket.CpuPins[7] = G400.PA19;
			socket.CpuPins[8] = G400.PA20;
			socket.CpuPins[9] = G400.PA17;
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
			socket.SupportedTypes = new char[] { 'C', 'I', 'U', 'X' };
			socket.CpuPins[3] = G400.PA29;
			socket.CpuPins[4] = G400.PA10;
			socket.CpuPins[5] = G400.PA9;
			socket.CpuPins[6] = G400.PA26;
			socket.CpuPins[8] = G400.PA30;
			socket.CpuPins[9] = G400.PA31;
			socket.SerialPortName = "COM1";
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
			socket.SupportedTypes = new char[] { 'C', 'S', 'U', 'X' };
			socket.CpuPins[3] = G400.PC26;
			socket.CpuPins[4] = G400.PA5;
			socket.CpuPins[5] = G400.PA6;
			socket.CpuPins[6] = G400.PB4;
			socket.CpuPins[7] = G400.PA12;
			socket.CpuPins[8] = G400.PA11;
			socket.CpuPins[9] = G400.PA13;
			socket.SPIModule = SPI.SPI_module.SPI1;
			socket.SerialPortName = "COM3";
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
			socket.SupportedTypes = new char[] { 'I', 'U', 'X' };
			socket.CpuPins[3] = G400.PC31;
			socket.CpuPins[4] = G400.PC16;
			socket.CpuPins[5] = G400.PC17;
			socket.CpuPins[6] = G400.PB2;
			socket.CpuPins[8] = G400.PA30;
			socket.CpuPins[9] = G400.PA31;
			socket.SerialPortName = "COM6";
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(13);
			socket.SupportedTypes = new char[] { 'A', 'I', 'X' };
			socket.CpuPins[3] = G400.PB17;
			socket.CpuPins[4] = G400.PB6;
			socket.CpuPins[5] = G400.PB7;
			socket.CpuPins[6] = G400.PC22;
			socket.CpuPins[8] = G400.PA30;
			socket.CpuPins[9] = G400.PA31;
			

			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_6;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_7;
			socket.AnalogInput5 = (Cpu.AnalogChannel)8;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(14);
			socket.SupportedTypes = new char[] { 'A', 'I', 'T', 'X' };
			socket.CpuPins[3] = G400.PB11;
			socket.CpuPins[4] = G400.PB12;
			socket.CpuPins[5] = G400.PB13;
			socket.CpuPins[6] = G400.PD1;
			socket.CpuPins[7] = G400.PD2;
			socket.CpuPins[8] = G400.PA30;
			socket.CpuPins[9] = G400.PA31;
			

			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_0;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_1;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_2;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(15);
			socket.SupportedTypes = new char[] { 'R', 'Y' };
			socket.CpuPins[3] = G400.PC11;
			socket.CpuPins[4] = G400.PC12;
			socket.CpuPins[5] = G400.PC13;
			socket.CpuPins[6] = G400.PC14;
			socket.CpuPins[7] = G400.PC15;
			socket.CpuPins[8] = G400.PC27;
			socket.CpuPins[9] = G400.PC28;
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(16);
			socket.SupportedTypes = new char[] { 'G', 'Y' };
			socket.CpuPins[3] = G400.PC5;
			socket.CpuPins[4] = G400.PC6;
			socket.CpuPins[5] = G400.PC7;
			socket.CpuPins[6] = G400.PC8;
			socket.CpuPins[7] = G400.PC9;
			socket.CpuPins[8] = G400.PC10;
			socket.CpuPins[9] = G400.PC21;
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(17);
			socket.SupportedTypes = new char[] { 'B', 'Y' };
			socket.CpuPins[3] = G400.PC0;
			socket.CpuPins[4] = G400.PC1;
			socket.CpuPins[5] = G400.PC2;
			socket.CpuPins[6] = G400.PC3;
			socket.CpuPins[7] = G400.PC4;
			socket.CpuPins[8] = G400.PC29;
			socket.CpuPins[9] = G400.PC30;
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(18);
			socket.SupportedTypes = new char[] { 'A', 'P', 'Y' };
			socket.CpuPins[3] = G400.PB8;
			socket.CpuPins[4] = G400.PB9;
			socket.CpuPins[5] = G400.PB10;
			socket.CpuPins[6] = G400.PC24;
			socket.CpuPins[7] = G400.PC18;
			socket.CpuPins[8] = G400.PC19;
			socket.CpuPins[9] = G400.PC20;
			

			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = (Cpu.AnalogChannel)9;
			socket.AnalogInput4 = (Cpu.AnalogChannel)10;
			socket.AnalogInput5 = (Cpu.AnalogChannel)11;

			socket.PWM7 = Cpu.PWMChannel.PWM_0;
			socket.PWM8 = Cpu.PWMChannel.PWM_1;
			socket.PWM9 = Cpu.PWMChannel.PWM_2;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
		}

		void BitmapConverter(Bitmap bmp, byte[] pixelBytes, GT.Mainboard.BPP bpp)
		{
			if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE)
				throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_LE supported");

			GHI.System.Utilities.BitmapHelpers.Convert(bmp, GHI.System.Utilities.BitmapHelpers.BitsPerPixel.BPP16_BGR_BE);
		}

		private static string[] sdVolumes = new string[] { "SD", "USB Mass Storage" };
		private Removable storage;

		/// <summary>
		/// Allows mainboards to support storage device mounting/umounting.  This provides modules with a list of storage device volume names supported by the mainboard. 
		/// </summary>
		public override string[] GetStorageDeviceVolumeNames()
		{
			return sdVolumes;
		}

		/// <summary>
		/// Functionality provided by mainboard to mount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a <see cref="Microsoft.SPOT.IO.RemovableMedia.Insert"/> event if successful.
		/// </summary>
		public override bool MountStorageDevice(string volumeName)
		{
			this.storage = new Removable(volumeName);
			this.storage.Mount();

			return true;
		}

		/// <summary>
		/// Functionality provided by mainboard to ummount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a <see cref="Microsoft.SPOT.IO.RemovableMedia.Eject"/> event if successful.
		/// </summary>
		public override bool UnmountStorageDevice(string volumeName)
		{
			this.storage.Unmount();
			this.storage.Dispose();

			return true;
		}

		/// <summary>
		/// Changes the programming interafces to the one specified
		/// </summary>
		/// <param name="programmingInterface">The programming interface to use</param>
		public override void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface)
		{
			throw new NotSupportedException();
        }

        /// <summary>
        /// Configure the onboard display controller to fulfil the requirements of a display using the RGB sockets.
        /// If doing this requires rebooting, then the method must reboot and not return.
        /// If there is no onboard display controller, then NotSupportedException must be thrown.
        /// </summary>
        /// <param name="displayModel">Display model name.</param>
        /// <param name="width">Display physical width in pixels, ignoring the orientation setting.</param>
        /// <param name="height">Display physical height in lines, ignoring the orientation setting.</param>
        /// <param name="orientationDeg">Display orientation in degrees.</param>
        /// <param name="lcdConfig">The required timings from an LCD controller.</param>
        protected override void OnOnboardControllerDisplayConnected(string displayModel, int width, int height, int orientationDeg, GT.Modules.Module.DisplayModule.TimingRequirements lcdConfig)
        {
            Configuration.Display.Height = (uint)height;
            Configuration.Display.HorizontalBackPorch = lcdConfig.HorizontalBackPorch;
            Configuration.Display.HorizontalFrontPorch = lcdConfig.HorizontalFrontPorch;
            Configuration.Display.HorizontalSyncPolarity = lcdConfig.HorizontalSyncPulseIsActiveHigh;
            Configuration.Display.HorizontalSyncPulseWidth = lcdConfig.HorizontalSyncPulseWidth;
            Configuration.Display.OutputEnableIsFixed = lcdConfig.UsesCommonSyncPin; //not the proper property, but we needed it;
            Configuration.Display.OutputEnablePolarity = lcdConfig.CommonSyncPinIsActiveHigh; //not the proper property, but we needed it;
            Configuration.Display.PixelClockRateKHz = lcdConfig.MaximumClockSpeed;
            Configuration.Display.PixelPolarity = lcdConfig.PixelDataIsValidOnClockRisingEdge;
            Configuration.Display.VerticalBackPorch = lcdConfig.VerticalBackPorch;
            Configuration.Display.VerticalFrontPorch = lcdConfig.VerticalFrontPorch;
            Configuration.Display.VerticalSyncPolarity = lcdConfig.VerticalSyncPulseIsActiveHigh;
            Configuration.Display.VerticalSyncPulseWidth = lcdConfig.VerticalSyncPulseWidth;
            Configuration.Display.Width = (uint)width;

            if (Configuration.Display.Save())
            {
                Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
                Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

                // A new configuration was set, so we must reboot
                Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
            }
        }

        /// <summary>
        /// Ensures that the pins on R, G and B sockets (which also have other socket types) are available for use for non-display purposes.
        /// If doing this requires rebooting, then the method must reboot and not return.
        /// If there is no onboard display controller, or it is not possible to disable the onboard display controller, then NotSupportedException must be thrown.
        /// </summary>
        public override void EnsureRgbSocketPinsAvailable()
        {
            if (Configuration.Display.Disable())
            {
                Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
                Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

                Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
            }
        }

		// change the below to the debug led pin on this mainboard
		private const Cpu.Pin DebugLedPin = G400.PD3;

		private Microsoft.SPOT.Hardware.OutputPort debugled = new OutputPort(DebugLedPin, false);
		/// <summary>
		/// Turns the debug LED on or off
		/// </summary>
		/// <param name="on">True if the debug LED should be on</param>
		public override void SetDebugLED(bool on)
		{
			debugled.Write(on);
		}

		/// <summary>
		/// This performs post-initialization tasks for the mainboard.  It is called by Gadgeteer.Program.Run and does not need to be called manually.
		/// </summary>
		public override void PostInit()
		{
			return;
		}

		/// <summary>
		/// The mainboard name, which is printed at startup in the debug window
		/// </summary>
		public override string MainboardName
		{
			get { return "GHI Electronics FEZRaptor"; }
		}

		/// <summary>
		/// The mainboard version, which is printed at startup in the debug window
		/// </summary>
		public override string MainboardVersion
		{
			get { return "1.0"; }
		}

        private class InteropI2CBus : GT.SocketInterfaces.I2CBus
        {
            public override ushort Address { get; set; }
            public override int Timeout { get; set; }
            public override int ClockRateKHz { get; set; }

            private SoftwareI2CBus i2c;

            public InteropI2CBus(GT.Socket socket, GT.Socket.Pin sdaPin, GT.Socket.Pin sclPin, ushort address, int clockRateKHz, GTM.Module module)
            {
                this.i2c = new SoftwareI2CBus(socket.CpuPins[(int)sclPin], socket.CpuPins[(int)sdaPin]);
                this.Address = address;
                this.ClockRateKHz = clockRateKHz;
            }

            public override void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int numWritten, out int numRead)
            {
                this.i2c.WriteRead((byte)this.Address, writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out numWritten, out numRead);
            }
        }

		private enum SpecialPurposePin
		{
			USBD_A_DM = -9,
			USBD_A_DP = -10,
			USBD_B_DM = -11,
			USBD_B_DP = -12,
			USBD_C_DM = -13,
			USBD_C_DP = -14,
			RTC_BATT = -15,
			RESET = -16,
			TCK = -19,
			TDO = -20,
			TMS = -21,
			TRST = -22,
			TDI = -23,
		}
	}
}
