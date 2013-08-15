using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GT = Gadgeteer;
using GHI.Hardware.G400;
using GHI.Premium.IO;
using GHI.Premium.Hardware;

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
			GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate nativeI2C = new GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate(this.NativeI2CWriteRead);
			
			GT.Socket socket;

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'S', 'U', 'Y' };
			socket.CpuPins[3] = Pin.PB0;
			socket.CpuPins[4] = Pin.PA7;
			socket.CpuPins[5] = Pin.PA8;
			socket.CpuPins[6] = Pin.PB5;
			socket.CpuPins[7] = Pin.PA22;
			socket.CpuPins[8] = Pin.PA21;
			socket.CpuPins[9] = Pin.PA23;
			socket.SPIModule = SPI.SPI_module.SPI2;
			socket.SerialPortName = "COM4";
			socket.NativeI2CWriteRead = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'A', 'I', 'X' };
			socket.CpuPins[3] = Pin.PB14;
			socket.CpuPins[4] = Pin.PB15;
			socket.CpuPins[5] = Pin.PB16;
			socket.CpuPins[6] = Pin.PB18;
			socket.CpuPins[8] = Pin.PA30;
			socket.CpuPins[9] = Pin.PA31;
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_3;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_4;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_5;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
			socket.SupportedTypes = new char[] { 'S', 'X' };
			socket.CpuPins[3] = Pin.PB1;
			socket.CpuPins[4] = Pin.PC23;
			socket.CpuPins[5] = Pin.PB3;
			socket.CpuPins[6] = Pin.PA28;
			socket.CpuPins[7] = Pin.PA12;
			socket.CpuPins[8] = Pin.PA11;
			socket.CpuPins[9] = Pin.PA13;
			socket.SPIModule = SPI.SPI_module.SPI1;
			socket.NativeI2CWriteRead = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
			socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
			socket.CpuPins[3] = Pin.PA27;
			socket.CpuPins[4] = Pin.PA0;
			socket.CpuPins[5] = Pin.PA1;
			socket.CpuPins[6] = Pin.PA2;
			socket.CpuPins[7] = Pin.PA3;
			socket.CpuPins[8] = Pin.PA30;
			socket.CpuPins[9] = Pin.PA31;
			socket.SerialPortName = "COM2";
			socket.NativeI2CWriteRead = nativeI2C;
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
			socket.CpuPins[3] = Pin.PD4;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_C_DM;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_C_DP;
			socket.CpuPins[6] = Pin.PA24;
			socket.CpuPins[8] = Pin.PA30;
			socket.CpuPins[9] = Pin.PA31;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(7);
			socket.SupportedTypes = new char[] { 'H', 'I' };
			socket.CpuPins[3] = Pin.PA25;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_B_DM;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_B_DP;
			socket.CpuPins[6] = Pin.PA4;
			socket.CpuPins[8] = Pin.PA30;
			socket.CpuPins[9] = Pin.PA31;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(8);
			socket.SupportedTypes = new char[] { 'D', 'I' };
			socket.CpuPins[3] = Pin.PD6;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_A_DM;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_A_DP;
			socket.CpuPins[6] = Pin.PD0;
			socket.CpuPins[7] = Pin.PD5;
			socket.CpuPins[8] = Pin.PA30;
			socket.CpuPins[9] = Pin.PA31;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
			socket.SupportedTypes = new char[] { 'F', 'Y' };
			socket.CpuPins[3] = Pin.PD7;
			socket.CpuPins[4] = Pin.PA15;
			socket.CpuPins[5] = Pin.PA18;
			socket.CpuPins[6] = Pin.PA16;
			socket.CpuPins[7] = Pin.PA19;
			socket.CpuPins[8] = Pin.PA20;
			socket.CpuPins[9] = Pin.PA17;
			socket.NativeI2CWriteRead = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
			socket.SupportedTypes = new char[] { 'C', 'I', 'U', 'X' };
			socket.CpuPins[3] = Pin.PA29;
			socket.CpuPins[4] = Pin.PA10;
			socket.CpuPins[5] = Pin.PA9;
			socket.CpuPins[6] = Pin.PA26;
			socket.CpuPins[8] = Pin.PA30;
			socket.CpuPins[9] = Pin.PA31;
			socket.SerialPortName = "COM1";
			socket.NativeI2CWriteRead = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
			socket.SupportedTypes = new char[] { 'C', 'S', 'U', 'X' };
			socket.CpuPins[3] = Pin.PC26;
			socket.CpuPins[4] = Pin.PA5;
			socket.CpuPins[5] = Pin.PA6;
			socket.CpuPins[6] = Pin.PB4;
			socket.CpuPins[7] = Pin.PA12;
			socket.CpuPins[8] = Pin.PA11;
			socket.CpuPins[9] = Pin.PA13;
			socket.SPIModule = SPI.SPI_module.SPI1;
			socket.SerialPortName = "COM3";
			socket.NativeI2CWriteRead = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
			socket.SupportedTypes = new char[] { 'I', 'U', 'X' };
			socket.CpuPins[3] = Pin.PC31;
			socket.CpuPins[4] = Pin.PC16;
			socket.CpuPins[5] = Pin.PC17;
			socket.CpuPins[6] = Pin.PB2;
			socket.CpuPins[8] = Pin.PA30;
			socket.CpuPins[9] = Pin.PA31;
			socket.SerialPortName = "COM6";
			socket.NativeI2CWriteRead = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(13);
			socket.SupportedTypes = new char[] { 'A', 'I', 'X' };
			socket.CpuPins[3] = Pin.PB17;
			socket.CpuPins[4] = Pin.PB6;
			socket.CpuPins[5] = Pin.PB7;
			socket.CpuPins[6] = Pin.PC22;
			socket.CpuPins[8] = Pin.PA30;
			socket.CpuPins[9] = Pin.PA31;
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_6;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_7;
			socket.AnalogInput5 = (Cpu.AnalogChannel)8;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(14);
			socket.SupportedTypes = new char[] { 'A', 'I', 'T', 'X' };
			socket.CpuPins[3] = Pin.PB11;
			socket.CpuPins[4] = Pin.PB12;
			socket.CpuPins[5] = Pin.PB13;
			socket.CpuPins[6] = Pin.PD1;
			socket.CpuPins[7] = Pin.PD2;
			socket.CpuPins[8] = Pin.PA30;
			socket.CpuPins[9] = Pin.PA31;
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_0;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_1;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_2;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(15);
			socket.SupportedTypes = new char[] { 'R', 'Y' };
			socket.CpuPins[3] = Pin.PC11;
			socket.CpuPins[4] = Pin.PC12;
			socket.CpuPins[5] = Pin.PC13;
			socket.CpuPins[6] = Pin.PC14;
			socket.CpuPins[7] = Pin.PC15;
			socket.CpuPins[8] = Pin.PC27;
			socket.CpuPins[9] = Pin.PC28;
			socket.NativeI2CWriteRead = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(16);
			socket.SupportedTypes = new char[] { 'G', 'Y' };
			socket.CpuPins[3] = Pin.PC5;
			socket.CpuPins[4] = Pin.PC6;
			socket.CpuPins[5] = Pin.PC7;
			socket.CpuPins[6] = Pin.PC8;
			socket.CpuPins[7] = Pin.PC9;
			socket.CpuPins[8] = Pin.PC10;
			socket.CpuPins[9] = Pin.PC21;
			socket.NativeI2CWriteRead = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(17);
			socket.SupportedTypes = new char[] { 'B', 'Y' };
			socket.CpuPins[3] = Pin.PC0;
			socket.CpuPins[4] = Pin.PC1;
			socket.CpuPins[5] = Pin.PC2;
			socket.CpuPins[6] = Pin.PC3;
			socket.CpuPins[7] = Pin.PC4;
			socket.CpuPins[8] = Pin.PC29;
			socket.CpuPins[9] = Pin.PC30;
			socket.NativeI2CWriteRead = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(18);
			socket.SupportedTypes = new char[] { 'A', 'P', 'Y' };
			socket.CpuPins[3] = Pin.PB8;
			socket.CpuPins[4] = Pin.PB9;
			socket.CpuPins[5] = Pin.PB10;
			socket.CpuPins[6] = Pin.PC24;
			socket.CpuPins[7] = Pin.PC18;
			socket.CpuPins[8] = Pin.PC19;
			socket.CpuPins[9] = Pin.PC20;
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = (Cpu.AnalogChannel)9;
			socket.AnalogInput4 = (Cpu.AnalogChannel)10;
			socket.AnalogInput5 = (Cpu.AnalogChannel)11;

			socket.PWM7 = Cpu.PWMChannel.PWM_0;
			socket.PWM8 = Cpu.PWMChannel.PWM_1;
			socket.PWM9 = Cpu.PWMChannel.PWM_2;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
		}

		bool NativeI2CWriteRead(GT.Socket socket, GT.Socket.Pin sda, GT.Socket.Pin scl, byte address, byte[] write, int writeOffset, int writeLen, byte[] read, int readOffset, int readLen, out int numWritten, out int numRead)
		{
			return SoftwareI2CBus.DirectI2CWriteRead(socket.CpuPins[(int)scl], socket.CpuPins[(int)sda], 100, address, write, writeOffset, writeLen, read, readOffset, readLen, out numWritten, out numRead);
		}

		private static string[] sdVolumes = new string[] { "SD", "USB Mass Storage" };
		private PersistentStorage storage;

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
			this.storage = new PersistentStorage(volumeName);
			this.storage.MountFileSystem();

			return true;
		}

		/// <summary>
		/// Functionality provided by mainboard to ummount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a <see cref="Microsoft.SPOT.IO.RemovableMedia.Eject"/> event if successful.
		/// </summary>
		public override bool UnmountStorageDevice(string volumeName)
		{
			this.storage.UnmountFileSystem();
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
		/// This sets the LCD configuration.  If the value GT.Mainboard.LCDConfiguration.HeadlessConfig (=null) is specified, no display support should be active.
		/// If a non-null value is specified but the property LCDControllerEnabled is false, the LCD controller should be disabled if present,
		/// though the Bitmap width/height for WPF should be modified to the Width and Height parameters.  This must reboot if the LCD configuration changes require a reboot.
		/// </summary>
		/// <param name="lcdConfig">The LCD Configuration</param>
		public override void SetLCDConfiguration(GT.Mainboard.LCDConfiguration lcdConfig)
		{
			var config = new Configuration.LCD.Configurations();
			
			if (lcdConfig.LCDControllerEnabled)
			{
				config.Height = lcdConfig.Height;
				config.HorizontalBackPorch = lcdConfig.HorizontalBackPorch;
				config.HorizontalFrontPorch = lcdConfig.HorizontalFrontPorch;
				config.HorizontalSyncPolarity = lcdConfig.HorizontalSyncPolarity;
				config.HorizontalSyncPulseWidth = lcdConfig.HorizontalSyncPulseWidth;
				config.OutputEnableIsFixed = lcdConfig.OutputEnableIsFixed;
				config.OutputEnablePolarity = lcdConfig.OutputEnablePolarity;
				config.PixelClockRateKHz = (uint)(133000 / lcdConfig.PixelClockDivider);
				config.PixelPolarity = lcdConfig.PixelPolarity;
				config.VerticalBackPorch = lcdConfig.VerticalBackPorch;
				config.VerticalFrontPorch = lcdConfig.VerticalFrontPorch;
				config.VerticalSyncPolarity = lcdConfig.VerticalSyncPolarity;
				config.VerticalSyncPulseWidth = lcdConfig.VerticalSyncPulseWidth;
				config.Width = lcdConfig.Width;
			}
			else
			{
				config = Configuration.LCD.HeadlessConfig;
				config.Width = lcdConfig.Width;
				config.Height = lcdConfig.Height;
				config.PixelClockRateKHz = 0;
			}

			if (Configuration.LCD.Set(config))
			{
				Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
				Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

				Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
			}
		}

		/// <summary>
		/// Configures rotation in the LCD controller. This must reboot if performing the LCD rotation requires a reboot.
		/// </summary>
		/// <param name="rotation">The LCD rotation to use</param>
		/// <returns>true if the rotation is supported</returns>
		public override bool SetLCDRotation(GT.Modules.Module.DisplayModule.LCDRotation rotation)
		{
			return false;
		}

		// change the below to the debug led pin on this mainboard
		private const Cpu.Pin DebugLedPin = Pin.PD3;

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
