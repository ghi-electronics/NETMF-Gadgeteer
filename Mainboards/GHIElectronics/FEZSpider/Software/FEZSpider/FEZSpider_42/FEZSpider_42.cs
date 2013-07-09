using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GT = Gadgeteer;

//using EMX = GHI.Premium.Hardware.EMX;
using EMX = GHI.Hardware.EMX;
using GHI.Premium.Hardware;
using GHI.Premium.System;
using GHI.Premium.IO;

namespace GHIElectronics.Gadgeteer
{
	/// <summary>
	/// Support class for GHI Electronics FEZSpider, using the GHI EMX SoM, for Microsoft .NET Gadgeteer
	/// </summary>
	public class FEZSpider : GT.Mainboard
	{
		// The mainboard constructor gets called before anything else in Gadgeteer (module constructors, etc), 
		// so it can set up fields in Gadgeteer.dll specifying socket types supported, etc.
		
		/// <summary>
		/// Instantiates a new FEZSpider mainboard
		/// </summary>
		public FEZSpider()
		{
			// uncomment the following if you support NativeI2CWriteRead for faster DaisyLink performance
			// otherwise, the DaisyLink I2C interface will be supported in Gadgeteer.dll in managed code.
			GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate nativeI2C = new GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate(NativeI2CWriteRead);

			this.NativeBitmapConverter = new BitmapConvertBPP(BitmapConverter);

			// For each socket on the mainboard, create, configure and register a Socket object with Gadgeteer.dll
			// This specifies:
			// - the SupportedTypes character array matching the list on the mainboard
			// - the CpuPins array (indexes [3] to [9].  [1,2,10] are constant (3.3V, 5V, GND) and [0] is unused.  This is normally based on an enumeration supplied in the NETMF port used.
			// - for other functionality, e.g. UART, SPI, etc, properties in the Socket class are set as appropriate to enable Gadgeteer.dll to access this functionality.
			// See the Mainboard Builder's Guide and specifically the Socket Types specification for more details
			// The two examples below are not realistically implementable sockets, but illustrate how to initialize a wide range of socket functionality.

			// This example socket 1 supports many types
			// Type 'D' - no additional action
			// Type 'I' - I2C pins must be used for the correct CpuPins
			// Type 'K' and 'U' - UART pins and UART handshaking pins must be used for the correct CpuPins, and the SerialPortName property must be set.
			// Type 'S' - SPI pins must be used for the correct CpuPins, and the SPIModule property must be set 
			// Type 'X' - the NativeI2CWriteRead function pointer is set (though by default "nativeI2C" is null) 
			#region Example Sockets
			//socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			//socket.SupportedTypes = new char[] { 'D', 'I', 'K', 'S', 'U', 'X' };
			//socket.CpuPins[3] = (Cpu.Pin)1;
			//socket.CpuPins[4] = (Cpu.Pin)52;
			//socket.CpuPins[5] = (Cpu.Pin)23;
			//socket.CpuPins[6] = (Cpu.Pin)12;
			//socket.CpuPins[7] = (Cpu.Pin)34;
			//socket.CpuPins[8] = (Cpu.Pin)5;
			//socket.CpuPins[9] = (Cpu.Pin)7;
			//socket.NativeI2CWriteRead = nativeI2C;
			//socket.SerialPortName = "COM1";
			//socket.SPIModule = SPI.SPI_module.SPI1;
			//GT.Socket.SocketInterfaces.RegisterSocket(socket);

			//// This example socket 2 supports many types
			//// Type 'A' - AnalogInput3-5 properties are set and GT.Socket.SocketInterfaces.SetAnalogInputFactors call is made
			//// Type 'O' - AnalogOutput property is set
			//// Type 'P' - PWM7-9 properties are set
			//// Type 'Y' - the NativeI2CWriteRead function pointer is set (though by default "nativeI2C" is null) 
			//socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			//socket.SupportedTypes = new char[] { 'A', 'O', 'P', 'Y' };
			//socket.CpuPins[3] = (Cpu.Pin)11;
			//socket.CpuPins[4] = (Cpu.Pin)5;
			//socket.CpuPins[5] = (Cpu.Pin)3;
			//socket.CpuPins[6] = (Cpu.Pin)66;
			//// Pin 7 not connected on this socket, so it is left unspecified
			//socket.CpuPins[8] = (Cpu.Pin)59;
			//socket.CpuPins[9] = (Cpu.Pin)18;
			//socket.NativeI2CWriteRead = nativeI2C;
			//socket.AnalogOutput = new FEZSpider_AnalogOut((Cpu.Pin)14);
			//GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 1, 2, 10);
			//socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_2;
			//socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_3;
			//socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_1;
			//socket.PWM7 = Cpu.PWMChannel.PWM_3;
			//socket.PWM8 = Cpu.PWMChannel.PWM_0;
			//socket.PWM9 = Cpu.PWMChannel.PWM_2;
			//GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket Setup
			// Create sockets.  Use the same socket variable to avoid copy-paste errors that often happen if we use socket1, socket2, etc.
			GT.Socket socket;

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'D', 'I' };
			socket.CpuPins[3] = EMX.Pin.IO21;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_DM;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_DP;
			socket.CpuPins[6] = EMX.Pin.IO19;
			socket.CpuPins[7] = EMX.Pin.IO75;
			socket.CpuPins[8] = EMX.Pin.IO12;
			socket.CpuPins[9] = EMX.Pin.IO11;
			
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
			socket.CpuPins[3] = EMX.Pin.IO1;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBH_DM;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBH_DP;
			socket.CpuPins[6] = EMX.Pin.IO0;
			socket.CpuPins[8] = EMX.Pin.IO12;
			socket.CpuPins[9] = EMX.Pin.IO11;
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
			socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
			socket.CpuPins[3] = EMX.Pin.IO33;
			socket.CpuPins[4] = EMX.Pin.IO37;
			socket.CpuPins[5] = EMX.Pin.IO32;
			socket.CpuPins[6] = EMX.Pin.IO31;
			socket.CpuPins[7] = EMX.Pin.IO34;
			socket.CpuPins[8] = EMX.Pin.IO12;
			socket.CpuPins[9] = EMX.Pin.IO11;

			// X
			socket.NativeI2CWriteRead = nativeI2C;
			
			// K/U
			socket.SerialPortName = "COM2";
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
			socket.SupportedTypes = new char[] { 'F', 'Y' };
			socket.CpuPins[3] = EMX.Pin.IO23;
			socket.CpuPins[4] = EMX.Pin.IO43;
			socket.CpuPins[5] = EMX.Pin.IO41;
			socket.CpuPins[6] = EMX.Pin.IO44;
			socket.CpuPins[7] = EMX.Pin.IO40;
			socket.CpuPins[8] = EMX.Pin.IO39;
			socket.CpuPins[9] = EMX.Pin.IO42;
			socket.NativeI2CWriteRead = nativeI2C;
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
			socket.SupportedTypes = new char[] { 'C', 'S', 'Y' };
			socket.CpuPins[3] = EMX.Pin.IO18;
			socket.CpuPins[4] = EMX.Pin.IO20;
			socket.CpuPins[5] = EMX.Pin.IO22;
			socket.CpuPins[6] = EMX.Pin.IO10;
			socket.CpuPins[7] = EMX.Pin.IO36;
			socket.CpuPins[8] = EMX.Pin.IO38;
			socket.CpuPins[9] = EMX.Pin.IO35;
			
			// Y
			socket.NativeI2CWriteRead = nativeI2C;
			
			// S
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
			socket.CpuPins[3] = EMX.Pin.IO30;
			socket.CpuPins[4] = EMX.Pin.IO29;
			socket.CpuPins[5] = EMX.Pin.IO28;
			socket.CpuPins[6] = EMX.Pin.IO16;
			socket.CpuPins[7] = EMX.Pin.IO74;
			socket.CpuPins[8] = EMX.Pin.IO48;
			socket.CpuPins[9] = EMX.Pin.IO49;

			// Y
			socket.NativeI2CWriteRead = nativeI2C;
			
			// U
			socket.SerialPortName = "COM3";

			// P
			socket.PWM7 = Cpu.PWMChannel.PWM_5;
			socket.PWM8 = Cpu.PWMChannel.PWM_4;
			socket.PWM9 = Cpu.PWMChannel.PWM_3;
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
			socket.SupportedTypes = new char[] { 'A', 'O', 'S', 'U', 'Y' };
			socket.CpuPins[3] = EMX.Pin.IO46;
			socket.CpuPins[4] = EMX.Pin.IO6;
			socket.CpuPins[5] = EMX.Pin.IO7;
			socket.CpuPins[6] = EMX.Pin.IO15;
			socket.CpuPins[7] = EMX.Pin.IO24;
			socket.CpuPins[8] = EMX.Pin.IO25;
			socket.CpuPins[9] = EMX.Pin.IO27;

			// Y
			socket.NativeI2CWriteRead = nativeI2C;
			
			// U
			socket.SerialPortName = "COM4";
			
			// S
			socket.SPIModule = SPI.SPI_module.SPI1;
			
			// O
			socket.AnalogOutput = new FEZSpider_AnalogOut((Cpu.Pin)socket.CpuPins[5]);
			
			// A
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_7;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_2;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_3;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
			socket.SupportedTypes = new char[] { 'A', 'I', 'T', 'X' };
			socket.CpuPins[3] = EMX.Pin.IO45;
			socket.CpuPins[4] = EMX.Pin.IO5;
			socket.CpuPins[5] = EMX.Pin.IO8;
			socket.CpuPins[6] = EMX.Pin.IO73;
			socket.CpuPins[7] = EMX.Pin.IO72;
			socket.CpuPins[8] = EMX.Pin.IO12;
			socket.CpuPins[9] = EMX.Pin.IO11;

			// X
			socket.NativeI2CWriteRead = nativeI2C;
			
			// A
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_6;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_1;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_0;
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
			socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
			socket.CpuPins[3] = EMX.Pin.IO26;
			socket.CpuPins[4] = EMX.Pin.IO3;
			socket.CpuPins[5] = EMX.Pin.IO2;
			socket.CpuPins[6] = EMX.Pin.IO9;
			socket.CpuPins[7] = EMX.Pin.IO14;
			socket.CpuPins[8] = EMX.Pin.IO13;
			socket.CpuPins[9] = EMX.Pin.IO50;

			// Y
			socket.NativeI2CWriteRead = nativeI2C;
			
			// U
			socket.SerialPortName = "COM1";
			
			// P
			socket.PWM7 = Cpu.PWMChannel.PWM_1;
			socket.PWM8 = Cpu.PWMChannel.PWM_0;
			socket.PWM9 = Cpu.PWMChannel.PWM_2;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
			socket.SupportedTypes = new char[] { 'B', 'Y' };
			socket.CpuPins[3] = EMX.Pin.IO70;
			socket.CpuPins[4] = EMX.Pin.IO57;
			socket.CpuPins[5] = EMX.Pin.IO58;
			socket.CpuPins[6] = EMX.Pin.IO59;
			socket.CpuPins[7] = EMX.Pin.IO60;
			socket.CpuPins[8] = EMX.Pin.IO63;
			socket.CpuPins[9] = EMX.Pin.IO61;

			// Y
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(13);
			socket.SupportedTypes = new char[] { 'G' };
			socket.CpuPins[3] = EMX.Pin.IO51;
			socket.CpuPins[4] = EMX.Pin.IO52;
			socket.CpuPins[5] = EMX.Pin.IO53;
			socket.CpuPins[6] = EMX.Pin.IO54;
			socket.CpuPins[7] = EMX.Pin.IO55;
			socket.CpuPins[8] = EMX.Pin.IO56;
			socket.CpuPins[9] = EMX.Pin.IO17;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(14);
			socket.SupportedTypes = new char[] { 'R', 'Y' };
			socket.CpuPins[3] = EMX.Pin.IO69;
			socket.CpuPins[4] = EMX.Pin.IO65;
			socket.CpuPins[5] = EMX.Pin.IO66;
			socket.CpuPins[6] = EMX.Pin.IO67;
			socket.CpuPins[7] = EMX.Pin.IO68;
			socket.CpuPins[8] = EMX.Pin.IO62;
			socket.CpuPins[9] = EMX.Pin.IO64;
			
			// Y
			socket.NativeI2CWriteRead = nativeI2C;
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion
		}

		bool NativeI2CWriteRead(GT.Socket socket, GT.Socket.Pin sda, GT.Socket.Pin scl, byte address, byte[] write, int writeOffset, int writeLen, byte[] read, int readOffset, int readLen, out int numWritten, out int numRead)
		{
			// implement this method if you support NativeI2CWriteRead for faster DaisyLink performance
			// otherwise, the DaisyLink I2C interface will be supported in Gadgeteer.dll in managed code. 
			return SoftwareI2CBus.DirectI2CWriteRead(socket.CpuPins[(int)scl], socket.CpuPins[(int)sda], 100, address, write, writeOffset, writeLen, read, readOffset, readLen, out numWritten, out numRead);
		}

		void BitmapConverter(byte[] bitmapBytes, byte[] pixelBytes, GT.Mainboard.BPP bpp)
		{
			if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE)
				throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_LE supported");

			Util.BitmapConvertBPP(bitmapBytes, pixelBytes, Util.BPP_Type.BPP16_BGR_BE);
		}
		
		private PersistentStorage _storage;

		private static string[] sdVolumes = new string[] { "SD", "USB Mass Storage" };

		/// <summary>
		/// Allows mainboards to support storage device mounting/umounting.  This provides modules with a list of storage device volume names supported by the mainboard. 
		/// </summary>
		public override string[] GetStorageDeviceVolumeNames()
		{
			return sdVolumes;
		}

		/// <summary>
		/// Functionality provided by mainboard to mount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a Microsoft.SPOT.IO.RemovableMedia.Insert event if successful.
		/// </summary>
		public override bool MountStorageDevice(string volumeName)
		{
			// implement this if you support storage devices. This should result in a <see cref="Microsoft.SPOT.IO.RemovableMedia.Insert"/> event if successful and return true if the volumeName is supported.
			_storage = new PersistentStorage(volumeName);
			_storage.MountFileSystem();

			return true;// volumeName == "SD";
		}

		/// <summary>
		/// Functionality provided by mainboard to ummount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a Microsoft.SPOT.IO.RemovableMedia.Eject event if successful.
		/// </summary>
		public override bool UnmountStorageDevice(string volumeName)
		{
			// implement this if you support storage devices. This should result in a <see cref="Microsoft.SPOT.IO.RemovableMedia.Eject"/> event if successful and return true if the volumeName is supported.
			_storage.UnmountFileSystem();
			_storage.Dispose();

			return true;// volumeName == "SD";
		}

		/// <summary>
		/// Changes the programming interafces to the one specified
		/// </summary>
		/// <param name="programmingInterface">The programming interface to use</param>
		public override void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface)
		{
			// Change the reflashing interface to the one specified, if possible.
			// This is an advanced API that we don't expect people to call much.
		}


		/// <summary>
		/// This sets the LCD configuration.  If the value GT.Mainboard.LCDConfiguration.HeadlessConfig (=null) is specified, no display support should be active.
		/// If a non-null value is specified but the property LCDControllerEnabled is false, the LCD controller should be disabled if present,
		/// though the Bitmap width/height for WPF should be modified to the Width and Height parameters.  This must reboot if the LCD configuration changes require a reboot.
		/// </summary>
		/// <param name="lcdConfig">The LCD Configuration</param>
		public override void SetLCDConfiguration(GT.Mainboard.LCDConfiguration lcdConfig)
		{
			if (lcdConfig.LCDControllerEnabled == false)
			{
				//Configuration.LCD.Set(Configuration.LCD.HeadlessConfig);
				var config = new Configuration.LCD.Configurations();
				config = Configuration.LCD.HeadlessConfig;

				config.Width = lcdConfig.Width;
				config.Height = lcdConfig.Height;

				// removed
				//config.PixelClockDivider = 0xFF;

				// added
				config.PixelClockRateKHz = 0;

				if (Configuration.LCD.Set(config))
				{
					Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
					Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

					// A new configuration was set, so we must reboot
					Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
				}
			}
			else
			{
				var config = new Configuration.LCD.Configurations();

				//if (lcdConfig.LCDControllerEnabled == false)
				//{
				//    // EMX firmware has PixelClockDivider 0xFF as special value meaning "don't run"
				//    config.PixelClockDivider = 0xff;
				//}

				config.Height = lcdConfig.Height;
				config.HorizontalBackPorch = lcdConfig.HorizontalBackPorch;
				config.HorizontalFrontPorch = lcdConfig.HorizontalFrontPorch;
				config.HorizontalSyncPolarity = lcdConfig.HorizontalSyncPolarity;
				config.HorizontalSyncPulseWidth = lcdConfig.HorizontalSyncPulseWidth;
				config.OutputEnableIsFixed = lcdConfig.OutputEnableIsFixed;
				config.OutputEnablePolarity = lcdConfig.OutputEnablePolarity;
				
				// removed
				//config.PixelClockDivider = lcdConfig.PixelClockDivider;
				
				// added
				config.PixelClockRateKHz = (uint)(72000 / lcdConfig.PixelClockDivider);

				config.PixelPolarity = lcdConfig.PixelPolarity;
				
				// removed
				//config.PriorityEnable = lcdConfig.PriorityEnable;
				
				config.VerticalBackPorch = lcdConfig.VerticalBackPorch;
				config.VerticalFrontPorch = lcdConfig.VerticalFrontPorch;
				config.VerticalSyncPolarity = lcdConfig.VerticalSyncPolarity;
				config.VerticalSyncPulseWidth = lcdConfig.VerticalSyncPulseWidth;
				config.Width = lcdConfig.Width;

				if (Configuration.LCD.Set(config))
				{
					Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
					Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

					// A new configuration was set, so we must reboot
					Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
				}
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
		private const Cpu.Pin DebugLedPin = EMX.Pin.IO47;

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
			get { return "GHI Electronics FEZSpider"; }
		}

		/// <summary>
		/// The mainboard version, which is printed at startup in the debug window
		/// </summary>
		public override string MainboardVersion
		{
			get { return "1.0"; }
		}
	   
		enum SpecialPurposePin
		{

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
	}

	internal class FEZSpider_AnalogOut : GT.Socket.SocketInterfaces.AnalogOutput
	{
		private AnalogOutput aout = null;

		Cpu.Pin pin;
		const double MIN_VOLTAGE = 0;
		const double MAX_VOLTAGE = 3.3;

		public FEZSpider_AnalogOut(Cpu.Pin pin)
		{
			this.pin = pin;
		}

		public double MinOutputVoltage
		{
			get
			{
				return FEZSpider_AnalogOut.MIN_VOLTAGE;
			}
		}

		public double MaxOutputVoltage
		{
			get
			{
				return FEZSpider_AnalogOut.MAX_VOLTAGE;
			}
		}

		public bool Active
		{
			get
			{
				return this.aout != null;
			}
			set
			{
				if (value == this.Active) 
					return;

				if (value)
				{
					this.aout = new AnalogOutput(Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0, 1 / FEZSpider_AnalogOut.MAX_VOLTAGE, 0, 10);
					this.SetVoltage(FEZSpider_AnalogOut.MIN_VOLTAGE);
				}
				else
				{
					this.aout.Dispose();
					this.aout = null;
				}
			}
		}

		public void SetVoltage(double voltage)
		{
			this.Active = true;

			if (voltage < FEZSpider_AnalogOut.MIN_VOLTAGE)
				throw new ArgumentOutOfRangeException("The minimum voltage of the analog output interface is " + FEZSpider_AnalogOut.MIN_VOLTAGE.ToString() + "V");

			if (voltage > FEZSpider_AnalogOut.MAX_VOLTAGE)
				throw new ArgumentOutOfRangeException("The minimum voltage of the analog output interface is " + FEZSpider_AnalogOut.MAX_VOLTAGE.ToString() + "V");

			this.aout.Write(voltage);
		}
	}
}
