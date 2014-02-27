using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

//using G120 = GHI.Hardware.G120;
using G120 = GHI.Hardware.G120;
using GHI.Hardware;
using GHI.System;
using GHI.IO;

namespace GHIElectronics.Gadgeteer
{
	/// <summary>
	/// Support class for GHI Electronics FEZSpider II for Microsoft .NET Gadgeteer
	/// </summary>
	public class FEZSpiderII : GT.Mainboard
	{
		/// <summary>
		/// Instantiates a new FEZSpider II mainboard using the GHI G120 SoM, for Microsoft .NET Gadgeteer
		/// </summary>
		public FEZSpiderII()
		{
			// uncomment the following if you support NativeI2CWriteRead for faster DaisyLink performance
			// otherwise, the DaisyLink I2C interface will be supported in Gadgeteer.dll in managed code.
            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);

			this.NativeBitmapConverter = new BitmapConvertBPP(BitmapConverter);

			#region Socket Setup

			GT.Socket socket;

			Cpu.Pin I2C_SDA = G120.Pin.P0_27;
			Cpu.Pin I2C_SCL = G120.Pin.P0_28;

			Cpu.Pin special = (Cpu.Pin)(-2);
			Cpu.Pin NotConnected = Cpu.Pin.GPIO_NONE;

			//if (special < 0)
			//    throw new ApplicationException("don't forget to do the special pins!!");

			// For each socket on the mainboard, create, configure and register a Socket object with Gadgeteer.dll
			// This specifies:
			// - the SupportedTypes character array matching the list on the mainboard
			// - the CpuPins array (indexes [3] to [9].  [1,2,10] are constant (3.3V, 5V, GND) and [0] is unused.  This is normally based on an enumeration supplied in the NETMF port used.
			// - for other functionality, e.g. UART, SPI, etc, properties in the Socket class are set as appropriate to enable Gadgeteer.dll to access this functionality.
			// See the Mainboard Builder's Guide and specifically the Socket Types specification for more details
			// The two examples below are not realistically implementable sockets, but illustrate how to initialize a wide range of socket functionality.

			#region Example Sockets
			// This example socket 1 supports many types
			// Type 'D' - no additional action
			// Type 'I' - I2C pins must be used for the correct CpuPins
			// Type 'K' and 'U' - UART pins and UART handshaking pins must be used for the correct CpuPins, and the SerialPortName property must be set.
			// Type 'S' - SPI pins must be used for the correct CpuPins, and the SPIModule property must be set 
			// Type 'X' - the NativeI2CWriteRead function pointer is set (though by default "nativeI2C" is null) 
			/*socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'D', 'I', 'K', 'S', 'U', 'X' };
			socket.CpuPins[3] = (Cpu.Pin)1;
			socket.CpuPins[4] = (Cpu.Pin)52;
			socket.CpuPins[5] = (Cpu.Pin)23;
			socket.CpuPins[6] = (Cpu.Pin)12;
			socket.CpuPins[7] = (Cpu.Pin)34;
			socket.CpuPins[8] = (Cpu.Pin)5;
			socket.CpuPins[9] = (Cpu.Pin)7;
			
			socket.SerialPortName = "COM1";
			socket.SPIModule = SPI.SPI_module.SPI1;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			// This example socket 2 supports many types
			// Type 'A' - AnalogInput3-5 properties are set and GT.Socket.SocketInterfaces.SetAnalogInputFactors call is made
			// Type 'O' - AnalogOutput property is set
			// Type 'P' - PWM7-9 properties are set
			// Type 'Y' - the NativeI2CWriteRead function pointer is set (though by default "nativeI2C" is null) 
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'A', 'O', 'P', 'Y' };
			socket.CpuPins[3] = (Cpu.Pin)11;
			socket.CpuPins[4] = (Cpu.Pin)5;
			socket.CpuPins[5] = (Cpu.Pin)3;
			socket.CpuPins[6] = (Cpu.Pin)66;
			// Pin 7 not connected on this socket, so it is left unspecified
			socket.CpuPins[8] = (Cpu.Pin)59;
			socket.CpuPins[9] = (Cpu.Pin)18;
			
			socket.AnalogOutput = new FEZSpiderII_AnalogOut((Cpu.Pin)14);
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 1, 2, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_2;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_3;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_1;
			socket.PWM7 = Cpu.PWMChannel.PWM_3;
			socket.PWM8 = Cpu.PWMChannel.PWM_0;
			socket.PWM9 = Cpu.PWMChannel.PWM_2;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			*/
			#endregion

			#region Socket 1
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'F', 'Y' };
			socket.CpuPins[3] = G120.Pin.P0_18;
			socket.CpuPins[4] = G120.Pin.P1_6;
			socket.CpuPins[5] = G120.Pin.P1_7;
			socket.CpuPins[6] = G120.Pin.P1_3;
			socket.CpuPins[7] = G120.Pin.P1_11;
			socket.CpuPins[8] = G120.Pin.P1_12;
			socket.CpuPins[9] = G120.Pin.P1_2;

			// F
			// N/A

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 2
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'Z' };
			socket.CpuPins[3] = special;
			socket.CpuPins[4] = special;
			socket.CpuPins[5] = special;
			socket.CpuPins[6] = NotConnected;
			socket.CpuPins[7] = NotConnected;
			socket.CpuPins[8] = special;
			socket.CpuPins[9] = NotConnected;

			// Z
			// N/A

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 3
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
			socket.SupportedTypes = new char[] { 'I', 'U', 'X' };
			socket.CpuPins[3] = G120.Pin.P0_15;
			socket.CpuPins[4] = G120.Pin.P0_2;
			socket.CpuPins[5] = G120.Pin.P0_3;
			socket.CpuPins[6] = G120.Pin.P1_9;
			socket.CpuPins[7] = NotConnected;
			socket.CpuPins[8] = I2C_SDA;
			socket.CpuPins[9] = I2C_SCL;

			// I
			// N/A

			// U
			socket.SerialPortName = "COM1";

			// X
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			#endregion

			#region Socket 4
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
			socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
			socket.CpuPins[3] = G120.Pin.P0_1;
			socket.CpuPins[4] = G120.Pin.P2_0;
			socket.CpuPins[5] = G120.Pin.P0_16;
			socket.CpuPins[6] = G120.Pin.P0_6;
			socket.CpuPins[7] = G120.Pin.P0_17;
			socket.CpuPins[8] = I2C_SDA;
			socket.CpuPins[9] = I2C_SCL;

			// I
			// N/A

			// K/U
			socket.SerialPortName = "COM2";

			// X
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 5
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
			socket.SupportedTypes = new char[] { 'C', 'S', 'X' };
			socket.CpuPins[3] = G120.Pin.P2_10;
			socket.CpuPins[4] = G120.Pin.P0_5;
			socket.CpuPins[5] = G120.Pin.P0_4;
			socket.CpuPins[6] = G120.Pin.P1_16;
			socket.CpuPins[7] = (Cpu.Pin)9;
			socket.CpuPins[8] = (Cpu.Pin)8;
			socket.CpuPins[9] = (Cpu.Pin)7;

			// C
			// N/A

			// S
			socket.SPIModule = SPI.SPI_module.SPI2;

			// X
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 6
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
			socket.SupportedTypes = new char[] { 'A', 'I', 'T', 'X' };
			socket.CpuPins[3] = G120.Pin.P0_25;
			socket.CpuPins[4] = G120.Pin.P0_24;
			socket.CpuPins[5] = G120.Pin.P0_23;
			socket.CpuPins[6] = G120.Pin.P1_0;
			socket.CpuPins[7] = G120.Pin.P1_1;
			socket.CpuPins[8] = I2C_SDA;
			socket.CpuPins[9] = I2C_SCL;

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
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 7
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(7);
			socket.SupportedTypes = new char[] { 'D', 'I' };
			socket.CpuPins[3] = G120.Pin.P0_13;
			socket.CpuPins[4] = special;
			socket.CpuPins[5] = special;
			socket.CpuPins[6] = G120.Pin.P1_10;
			socket.CpuPins[7] = G120.Pin.P1_4;
			socket.CpuPins[8] = I2C_SDA;
			socket.CpuPins[9] = I2C_SCL;

			// D
			// N/A

			// I
			// N/A

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 8
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(8);
			socket.SupportedTypes = new char[] { 'S', 'U', 'X' };
			socket.CpuPins[3] = G120.Pin.P2_11;
			socket.CpuPins[4] = G120.Pin.P0_10;
			socket.CpuPins[5] = G120.Pin.P0_11;
			socket.CpuPins[6] = G120.Pin.P1_14;
			socket.CpuPins[7] = (Cpu.Pin)9;
			socket.CpuPins[8] = (Cpu.Pin)8;
			socket.CpuPins[9] = (Cpu.Pin)7;

			// S
			socket.SPIModule = SPI.SPI_module.SPI2;

			// U
			socket.SerialPortName = "COM3";

			// X
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 9
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
			socket.SupportedTypes = new char[] { 'A', 'I', 'O', 'X' };
			socket.CpuPins[3] = G120.Pin.P0_12;
			socket.CpuPins[4] = G120.Pin.P1_30;
			socket.CpuPins[5] = G120.Pin.P0_26;
			socket.CpuPins[6] = G120.Pin.P1_17;
			socket.CpuPins[7] = NotConnected;
			socket.CpuPins[8] = I2C_SDA;
			socket.CpuPins[9] = I2C_SCL;

			// A
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_6;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_4;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_3;

			// I
			// N/A

            // O
            socket.AnalogOutput5 = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0;

			// X
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 10
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
			socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
			socket.CpuPins[3] = G120.Pin.P0_0;
			socket.CpuPins[4] = G120.Pin.P4_28;
			socket.CpuPins[5] = G120.Pin.P4_29;
			socket.CpuPins[6] = G120.Pin.P1_31;
			socket.CpuPins[7] = G120.Pin.P3_24;
			socket.CpuPins[8] = G120.Pin.P3_25;
			socket.CpuPins[9] = G120.Pin.P3_26;

			// P
			socket.PWM7 = Cpu.PWMChannel.PWM_6;
			socket.PWM8 = Cpu.PWMChannel.PWM_7;
			socket.PWM9 = (Cpu.PWMChannel)8;

			// U
			socket.SerialPortName = "COM4";

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 11
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
			socket.SupportedTypes = new char[] { 'H', 'I' };
			socket.CpuPins[3] = G120.Pin.P2_21;
			socket.CpuPins[4] = special;
			socket.CpuPins[5] = special;
			socket.CpuPins[6] = G120.Pin.P1_19;
			socket.CpuPins[7] = NotConnected;
			socket.CpuPins[8] = I2C_SDA;
			socket.CpuPins[9] = I2C_SCL;

			// H
			// N/A

			// I
			// N/A

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 12
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
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
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 13
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(13);
			socket.SupportedTypes = new char[] { 'G' };
			socket.CpuPins[3] = G120.Pin.P1_20;
			socket.CpuPins[4] = G120.Pin.P1_21;
			socket.CpuPins[5] = G120.Pin.P1_22;
			socket.CpuPins[6] = G120.Pin.P1_23;
			socket.CpuPins[7] = G120.Pin.P1_24;
			socket.CpuPins[8] = G120.Pin.P1_25;
			socket.CpuPins[9] = G120.Pin.P1_5;

			// G
			// N/A

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 14
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(14);
			socket.SupportedTypes = new char[] { 'R', 'Y' };
			socket.CpuPins[3] = G120.Pin.P2_12;
			socket.CpuPins[4] = G120.Pin.P2_6;
			socket.CpuPins[5] = G120.Pin.P2_7;
			socket.CpuPins[6] = G120.Pin.P2_8;
			socket.CpuPins[7] = G120.Pin.P2_9;
			socket.CpuPins[8] = G120.Pin.P2_3;
			socket.CpuPins[9] = G120.Pin.P2_5;

			// R
			// N/A

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			#endregion

			#endregion //Socket Setup
		}

		void BitmapConverter(Bitmap bmp, byte[] pixelBytes, GT.Mainboard.BPP bpp)
		{
			if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE)
				throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_LE supported");

			Util.BitmapConvertBPP(bmp.GetBitmap(), pixelBytes, Util.BPP_Type.BPP16_BGR_BE);
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
			_storage.Mount();

			return true;// volumeName == "SD";
		}

		/// <summary>
		/// Functionality provided by mainboard to ummount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a Microsoft.SPOT.IO.RemovableMedia.Eject event if successful.
		/// </summary>
		public override bool UnmountStorageDevice(string volumeName)
		{
			// implement this if you support storage devices. This should result in a <see cref="Microsoft.SPOT.IO.RemovableMedia.Eject"/> event if successful and return true if the volumeName is supported.
			_storage.Unmount();
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
			var config = new Configuration.LCD.Configurations();

			//if (lcdConfig.LCDControllerEnabled == false)
			//{
			//    // EMX firmware has PixelClockDivider 0xFF as special value meaning "don't run"
			//    config.PixelClockDivider = 0xff;
			//}

			config.Height = (uint)height;
			config.HorizontalBackPorch = lcdConfig.HorizontalBackPorch;
			config.HorizontalFrontPorch = lcdConfig.HorizontalFrontPorch;
			config.HorizontalSyncPolarity = lcdConfig.HorizontalSyncPulseIsActiveHigh;
			config.HorizontalSyncPulseWidth = lcdConfig.HorizontalSyncPulseWidth;
            config.OutputEnableIsFixed = lcdConfig.UsesCommonSyncPin; //not the proper property, but we needed it;
            config.OutputEnablePolarity = lcdConfig.CommonSyncPinIsActiveHigh; //not the proper property, but we needed it;

			// removed
			//config.PixelClockDivider = lcdConfig.PixelClockDivider;

            // added
            config.PixelClockRateKHz = lcdConfig.MaximumClockSpeed;

            config.PixelPolarity = lcdConfig.PixelDataIsValidOnClockRisingEdge;

			// removed
			//config.PriorityEnable = lcdConfig.PriorityEnable;

			config.VerticalBackPorch = lcdConfig.VerticalBackPorch;
			config.VerticalFrontPorch = lcdConfig.VerticalFrontPorch;
			config.VerticalSyncPolarity = lcdConfig.VerticalSyncPulseIsActiveHigh;
			config.VerticalSyncPulseWidth = lcdConfig.VerticalSyncPulseWidth;
			config.Width = (uint)width;

			if (Configuration.LCD.Set(config))
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
            var config = new Configuration.LCD.Configurations();
            config = Configuration.LCD.HeadlessConfig;
            config.Width = 0;
            config.Height = 0;
            config.PixelClockRateKHz = 0;

            if (Configuration.LCD.Set(config))
            {
                Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
                Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

                Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
            }
        }

		// change the below to the debug led pin on this mainboard
		private const Cpu.Pin DebugLedPin = G120.Pin.P1_8;

		// NOTE: This is set to true because of the hardware design of the FEZSpider II. Low = led on, High = off.
		private Microsoft.SPOT.Hardware.OutputPort debugled = new OutputPort(DebugLedPin, true);
		
		/// <summary>
		/// Turns the debug LED on or off
		/// </summary>
		/// <param name="on">True if the debug LED should be on</param>
		public override void SetDebugLED(bool on)
		{
			// NOTE: This is negated because of the hardware design of the FEZSpider II. Low = led on, High = off.
			debugled.Write(!on);
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
			get { return "GHI Electronics FEZSpider II"; }
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

            private Cpu.Pin sdaPin;
            private Cpu.Pin sclPin;

            public InteropI2CBus(GT.Socket socket, GT.Socket.Pin sdaPin, GT.Socket.Pin sclPin, ushort address, int clockRateKHz, GTM.Module module)
            {
                this.sdaPin = socket.CpuPins[(int)sdaPin];
                this.sclPin = socket.CpuPins[(int)sclPin];
                this.Address = address;
                this.ClockRateKHz = clockRateKHz;
            }

            public override void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int numWritten, out int numRead)
            {
                SoftwareI2CBus.DirectI2CWriteRead(this.sclPin, this.sdaPin, 100, this.Address, writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out numWritten, out numRead);
            }
        }
	}
}
