using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GT = Gadgeteer;

//using G120 = GHI.Premium.Hardware.G120;
using G120 = GHI.Hardware.G120;
using GHI.Premium.Hardware;
using GHI.Premium.System;
using GHI.Premium.IO;

namespace GHIElectronics.Gadgeteer
{
	/// <summary>
	/// Support class for GHI Electronics FEZCobra II for Microsoft .NET Gadgeteer
	/// </summary>
	public class FEZCobra_II : GT.Mainboard
	{
		/// <summary>
		/// Instantiates a new GHI Electronics FEZCobra II mainboard
		/// </summary>
		public FEZCobra_II()
		{
			// uncomment the following if you support NativeI2CWriteRead for faster DaisyLink performance
			// otherwise, the DaisyLink I2C interface will be supported in Gadgeteer.dll in managed code.
			GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate nativeI2C = new GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate(NativeI2CWriteRead);

			this.NativeBitmapConverter = new BitmapConvertBPP(BitmapConverter);

			GT.Socket socket;

			// For each socket on the mainboard, create, configure and register a Socket object with Gadgeteer.dll
			// This specifies:
			// - the SupportedTypes character array matching the list on the mainboard
			// - the CpuPins array (indexes [3] to [9].  [1,2,10] are constant (3.3V, 5V, GND) and [0] is unused.  This is normally based on an enumeration supplied in the NETMF port used.
			// - for other functionality, e.g. UART, SPI, etc, properties in the Socket class are set as appropriate to enable Gadgeteer.dll to access this functionality.
			// See the Mainboard Builder's Guide and specifically the Socket Types specification for more details
			// The two examples below are not realistically implementable sockets, but illustrate how to initialize a wide range of socket functionality.

			#region Example Sockets
			//// This example socket 1 supports many types
			//// Type 'D' - no additional action
			//// Type 'I' - I2C pins must be used for the correct CpuPins
			//// Type 'K' and 'U' - UART pins and UART handshaking pins must be used for the correct CpuPins, and the SerialPortName property must be set.
			//// Type 'S' - SPI pins must be used for the correct CpuPins, and the SPIModule property must be set 
			//// Type 'X' - the NativeI2CWriteRead function pointer is set (though by default "nativeI2C" is null) 
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
			//socket.AnalogOutput = new FEZCobra_II_AnalogOut((Cpu.Pin)14);
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

			#region Socket 1
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'B', 'Y' };
			socket.CpuPins[3] = G120.Pin.P2_13;
			socket.CpuPins[4] = G120.Pin.P1_26;
			socket.CpuPins[5] = G120.Pin.P1_27;
			socket.CpuPins[6] = G120.Pin.P1_28;
			socket.CpuPins[7] = G120.Pin.P1_29;
			socket.CpuPins[8] = G120.Pin.P2_4;
			socket.CpuPins[9] = G120.Pin.P2_2;

			// B
			// N/A

			// Y
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 1

			#region Socket 2
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'G' };
			socket.CpuPins[3] = G120.Pin.P1_20;
			socket.CpuPins[4] = G120.Pin.P1_21;
			socket.CpuPins[5] = G120.Pin.P1_22;
			socket.CpuPins[6] = G120.Pin.P1_23;
			socket.CpuPins[7] = G120.Pin.P1_24;
			socket.CpuPins[8] = G120.Pin.P1_25;
			socket.CpuPins[9] = G120.Pin.P1_19;

			// G
			// N/A

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 2

			#region Socket 3
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
			socket.SupportedTypes = new char[] { 'R', 'Y' };
			socket.CpuPins[3] = G120.Pin.P2_12;
			socket.CpuPins[4] = G120.Pin.P2_6;
			socket.CpuPins[5] = G120.Pin.P2_7;
			socket.CpuPins[6] = G120.Pin.P2_8;
			socket.CpuPins[7] = G120.Pin.P2_9;
			socket.CpuPins[8] = G120.Pin.P2_3;
			socket.CpuPins[9] = G120.Pin.P2_5;

			// Y
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 3

			#region Socket 4
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
			socket.SupportedTypes = new char[] { 'A', 'I', 'T', 'X' };
			socket.CpuPins[3] = G120.Pin.P0_25;
			socket.CpuPins[4] = G120.Pin.P0_24;
			socket.CpuPins[5] = G120.Pin.P0_23;
			socket.CpuPins[6] = G120.Pin.P1_0;
			socket.CpuPins[7] = G120.Pin.P1_1;
			socket.CpuPins[8] = G120.Pin.P0_27;
			socket.CpuPins[9] = G120.Pin.P0_28;

			// A
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_2;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_1;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_0;

			// I
			// N/A

			// T
			// N/A
		 
			// X
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 4

			#region Socket 5
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
			socket.SupportedTypes = new char[] { 'U', 'X' };
			socket.CpuPins[3] = G120.Pin.P0_13;
			socket.CpuPins[4] = G120.Pin.P0_2;
			socket.CpuPins[5] = G120.Pin.P0_3;
			socket.CpuPins[6] = G120.Pin.P1_4;
			socket.CpuPins[7] = G120.Pin.GPIO_NONE;
			socket.CpuPins[8] = G120.Pin.GPIO_NONE;
			socket.CpuPins[9] = G120.Pin.GPIO_NONE;

			// U
			socket.SerialPortName = "COM1";

			// X
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 5

			#region Socket 6
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
			socket.SupportedTypes = new char[] { 'S', 'X' };
			socket.CpuPins[3] = G120.Pin.P2_21;
			socket.CpuPins[4] = G120.Pin.P1_14;
			socket.CpuPins[5] = G120.Pin.P1_16;
			socket.CpuPins[6] = G120.Pin.P1_17;
			socket.CpuPins[7] = (Cpu.Pin)9; //P0.9
			socket.CpuPins[8] = (Cpu.Pin)8; //P0.8
			socket.CpuPins[9] = (Cpu.Pin)7; //P0.7

			// S
			socket.SPIModule = SPI.SPI_module.SPI2;

			// X
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 6

			#region Extended Sockets
			#region Socket 7
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(7);
			socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
			socket.CpuPins[3] = G120.Pin.P0_4;
			socket.CpuPins[4] = G120.Pin.P4_28;
			socket.CpuPins[5] = G120.Pin.P4_29;
			socket.CpuPins[6] = G120.Pin.P1_30;
			socket.CpuPins[7] = G120.Pin.P3_26;
			socket.CpuPins[8] = G120.Pin.P3_25;
			socket.CpuPins[9] = G120.Pin.P3_24;

			// P
			socket.PWM7 = (Cpu.PWMChannel)8;
			socket.PWM8 = Cpu.PWMChannel.PWM_7;
			socket.PWM9 = Cpu.PWMChannel.PWM_6;

			// U
			socket.SerialPortName = "COM4";

			// Y
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 7

			#region Socket 8
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(8);
			socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
			socket.CpuPins[3] = G120.Pin.P0_10;
			socket.CpuPins[4] = G120.Pin.P2_0;
			socket.CpuPins[5] = G120.Pin.P0_16;
			socket.CpuPins[6] = G120.Pin.P0_6;
			socket.CpuPins[7] = G120.Pin.P0_17;
			socket.CpuPins[8] = G120.Pin.P0_27;
			socket.CpuPins[9] = G120.Pin.P0_28;

			// I
			// N/A

			// K/U
			socket.SerialPortName = "COM3";

			// X
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 8

			#region Socket 9
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
			socket.SupportedTypes = new char[] { 'A', 'O', 'S', 'X' };
			socket.CpuPins[3] = G120.Pin.P0_12;
			socket.CpuPins[4] = G120.Pin.P1_31;
			socket.CpuPins[5] = G120.Pin.P0_26;
			socket.CpuPins[6] = G120.Pin.P1_5;
			socket.CpuPins[7] = G120.Pin.P0_18;
			socket.CpuPins[8] = G120.Pin.P0_17;
			socket.CpuPins[9] = G120.Pin.P0_15;

			// A
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_6;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_5;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_3;

			// O
			socket.AnalogOutput = new FEZCobra_II_AnalogOut((Cpu.Pin)socket.CpuPins[5]);

			// S
			socket.SPIModule = SPI.SPI_module.SPI1;

			// X
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 9

			#region Socket 10
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
			socket.SupportedTypes = new char[] { 'C', 'I', 'X' };
			socket.CpuPins[3] = G120.Pin.P0_11;
			socket.CpuPins[4] = G120.Pin.P0_1;
			socket.CpuPins[5] = G120.Pin.P0_0;
			socket.CpuPins[6] = G120.Pin.P0_5;
			//socket.CpuPins[7] = G120.Pin.P3_26;
			socket.CpuPins[8] = G120.Pin.P0_27;
			socket.CpuPins[9] = G120.Pin.P0_28;

			// C
			// N/A

			// I
			// N/A

			// X
			socket.NativeI2CWriteRead = nativeI2C;

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 10

			#endregion Extended Sockets

			#endregion Socket Setup
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

		private static string[] sdVolumes = new string[] { "SD", "USB Mass Storage" };

		/// <summary>
		/// Allows mainboards to support storage device mounting/umounting.  This provides modules with a list of storage device volume names supported by the mainboard. 
		/// </summary>
		public override string[] GetStorageDeviceVolumeNames()
		{
			return sdVolumes;
		}

		private PersistentStorage _storage;

		/// <summary>
		/// Functionality provided by mainboard to mount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a <see cref="Microsoft.SPOT.IO.RemovableMedia.Insert"/> event if successful.
		/// </summary>
		public override bool MountStorageDevice(string volumeName)
		{
			_storage = new PersistentStorage(volumeName);
			_storage.MountFileSystem();
			_storage = null;

			return true;
		}

		/// <summary>
		/// Functionality provided by mainboard to ummount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a <see cref="Microsoft.SPOT.IO.RemovableMedia.Eject"/> event if successful.
		/// </summary>
		public override bool UnmountStorageDevice(string volumeName)
		{
			_storage.UnmountFileSystem();
			_storage.Dispose();

			return true;
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
				uint sysclk = Microsoft.SPOT.Hardware.Cpu.SystemClock;
				sysclk /= 1000;
				config.PixelClockRateKHz = (uint)(sysclk / lcdConfig.PixelClockDivider);

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
		private const Cpu.Pin DebugLedPin = G120.Pin.P1_15;

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
			get { return "GHI Electronics FEZCobra II"; }
		}

		/// <summary>
		/// The mainboard version, which is printed at startup in the debug window
		/// </summary>
		public override string MainboardVersion
		{
			get { return "1.0"; }
		}

	}

	internal class FEZCobra_II_AnalogOut : GT.Socket.SocketInterfaces.AnalogOutput
	{
		private AnalogOutput aout = null;

		Cpu.Pin pin;
		const double MIN_VOLTAGE = 0;
		const double MAX_VOLTAGE = 3.3;

		public FEZCobra_II_AnalogOut(Cpu.Pin pin)
		{
			this.pin = pin;
		}

		public double MinOutputVoltage
		{
			get
			{
				return FEZCobra_II_AnalogOut.MIN_VOLTAGE;
			}
		}

		public double MaxOutputVoltage
		{
			get
			{
				return FEZCobra_II_AnalogOut.MAX_VOLTAGE;
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
					this.aout = new AnalogOutput(Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0, 1 / FEZCobra_II_AnalogOut.MAX_VOLTAGE, 0, 10);
					this.SetVoltage(FEZCobra_II_AnalogOut.MIN_VOLTAGE);
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

			if (voltage < FEZCobra_II_AnalogOut.MIN_VOLTAGE)
				throw new ArgumentOutOfRangeException("The minimum voltage of the analog output interface is " + FEZCobra_II_AnalogOut.MIN_VOLTAGE.ToString() + "V");

			if (voltage > FEZCobra_II_AnalogOut.MAX_VOLTAGE)
				throw new ArgumentOutOfRangeException("The maximum voltage of the analog output interface is " + FEZCobra_II_AnalogOut.MAX_VOLTAGE.ToString() + "V");

			this.aout.Write(voltage);
		}
	}

}
