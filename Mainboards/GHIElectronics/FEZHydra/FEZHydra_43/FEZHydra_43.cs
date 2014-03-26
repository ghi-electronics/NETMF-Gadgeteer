using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GT = Gadgeteer;

using GHIOSHW = GHI.Hardware;
using GHI.Hardware;
using GHI.System;
using GHI.Hardware.Storage;
using GHI.Pins;
using GTM = Gadgeteer.Modules;

namespace GHIElectronics.Gadgeteer
{
	/// <summary>
	/// Support class for GHI Electronics FEZHydra for Microsoft .NET Gadgeteer
	/// </summary>
	public class FEZHydra : GT.Mainboard
	{
		// The mainboard constructor gets called before anything else in Gadgeteer (module constructors, etc), 
		// so it can set up fields in Gadgeteer.dll specifying socket types supported, etc.

		/// <summary>
		/// Instantiates a new FEZHydra mainboard
		/// </summary>
		public FEZHydra()
		{
			// uncomment the following if you support NativeI2CWriteRead for faster DaisyLink performance
			// otherwise, the DaisyLink I2C interface will be supported in Gadgeteer.dll in managed code.
            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);

			this.NativeBitmapConverter = new BitmapConvertBPP(BitmapConverter);

			//this.GetStorageDeviceVolumeNames = new GetStorageDeviceVolumeNamesDelegate(_GetStorageDeviceVolumeNames);
			//this.MountStorageDevice = new StorageDeviceDelegate(_MountStorageDevice);
			//this.UnmountStorageDevice = new StorageDeviceDelegate(_UnmountStorageDevice);

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
			//
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
			//
			//socket.AnalogOutput = new FEZHydra_AnalogOut((Cpu.Pin)14);
			//GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 1, 2, 10);
			//socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_2;
			//socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_3;
			//socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_1;
			//socket.PWM7 = Cpu.PWMChannel.PWM_3;
			//socket.PWM8 = Cpu.PWMChannel.PWM_0;
			//socket.PWM9 = Cpu.PWMChannel.PWM_2;
			//GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion

			#region Socket 1
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'Z' };
			socket.CpuPins[3] = (Cpu.Pin)SpecialPurposePin.RESET;
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.TCK;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.RTC_BATT;
			socket.CpuPins[6] = (Cpu.Pin)SpecialPurposePin.TDO;
			socket.CpuPins[7] = (Cpu.Pin)SpecialPurposePin.TRST;
			socket.CpuPins[8] = (Cpu.Pin)SpecialPurposePin.TMS;
			socket.CpuPins[9] = (Cpu.Pin)SpecialPurposePin.TDI;

			// Z
			// N/A

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 1

			#region Socket 2
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'D' };
			socket.CpuPins[3] = Generic.GetPin('B', 19);
			socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_DM;
			socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_DP;
			socket.CpuPins[6] = Generic.GetPin('B', 18);
			socket.CpuPins[7] = Generic.GetPin('B', 22);
			socket.CpuPins[8] = Cpu.Pin.GPIO_NONE;
			socket.CpuPins[9] = Cpu.Pin.GPIO_NONE;

			// D
			// N/A

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 2

			#region Socket 3
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
			socket.SupportedTypes = new char[] { 'S', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('B', 8); //PWM0
			socket.CpuPins[4] = Generic.GetPin('B', 9); //PWM1
			socket.CpuPins[5] = Generic.GetPin('B', 12);
			socket.CpuPins[6] = Generic.GetPin('B', 13);
			socket.CpuPins[7] = Generic.GetPin('A', 26); //MOSI
			socket.CpuPins[8] = Generic.GetPin('A', 25); //MISO
			socket.CpuPins[9] = Generic.GetPin('A', 27); //SCK
			
			// S
			socket.SPIModule = SPI.SPI_module.SPI1;

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 3

			#region Socket 4
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
			socket.SupportedTypes = new char[] { 'S', 'U', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('B', 2);
			socket.CpuPins[4] = Generic.GetPin('A', 11); //TXD1
			socket.CpuPins[5] = Generic.GetPin('A', 12); //RXD1
			socket.CpuPins[6] = Generic.GetPin('B', 14);
			socket.CpuPins[7] = Generic.GetPin('A', 26); //MOSI
			socket.CpuPins[8] = Generic.GetPin('A', 25); //MISO
			socket.CpuPins[9] = Generic.GetPin('A', 27); //SCK

			// S
			socket.SPIModule = SPI.SPI_module.SPI1;

			// U
			socket.SerialPortName = "COM3";

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 4

			#region Socket 5
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
			socket.SupportedTypes = new char[] { 'I', 'U', 'X' };
			socket.CpuPins[3] = Generic.GetPin('A', 9); //RTS0
			socket.CpuPins[4] = Generic.GetPin('A', 22); //DTXD
			socket.CpuPins[5] = Generic.GetPin('A', 21); //DRXD
			socket.CpuPins[6] = Generic.GetPin('A', 10); //CTS0
			socket.CpuPins[8] = Generic.GetPin('A', 23); // I2C_SDA
			socket.CpuPins[9] = Generic.GetPin('A', 24); // I2C_SCL

			// I
			// N/A

			// U
			socket.SerialPortName = "COM1";

			// X
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 5

			#region Socket 6
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
			socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
			socket.CpuPins[3] = Generic.GetPin('D', 17);
			socket.CpuPins[4] = Generic.GetPin('A', 13); //TXD2
			socket.CpuPins[5] = Generic.GetPin('A', 14); //RXD2
			socket.CpuPins[6] = Generic.GetPin('A', 29); //TXD2
			socket.CpuPins[7] = Generic.GetPin('A', 30); //CTS2
			socket.CpuPins[8] = Generic.GetPin('A', 23); //I2C_SDA
			socket.CpuPins[9] = Generic.GetPin('A', 24); //I2C_SCL

			// I
			// N/A

			// K/U
			socket.SerialPortName = "COM4";
			
			// X
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 6

			#region Socket 7
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(7);
			socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('D', 19);
			socket.CpuPins[4] = Generic.GetPin('A', 6); //TXD0
			socket.CpuPins[5] = Generic.GetPin('A', 7); //RXD0
			socket.CpuPins[6] = Generic.GetPin('D', 20);
			socket.CpuPins[7] = Generic.GetPin('D', 14); //PWM0
			socket.CpuPins[8] = Generic.GetPin('D', 15); //PWM1
			socket.CpuPins[9] = Generic.GetPin('D', 16); //PWM2

			// P
			socket.PWM7 = Cpu.PWMChannel.PWM_0;
			socket.PWM8 = Cpu.PWMChannel.PWM_1;
			socket.PWM9 = Cpu.PWMChannel.PWM_2;

			// U
			socket.SerialPortName = "COM2";

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 7

			#region Socket 8
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(8);
			socket.SupportedTypes = new char[] { 'F', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('D', 11);
			socket.CpuPins[4] = Generic.GetPin('A', 0); //MC_DA0
			socket.CpuPins[5] = Generic.GetPin('A', 3); //MC_DA1
			socket.CpuPins[6] = Generic.GetPin('A', 1); //MC_CDA
			socket.CpuPins[7] = Generic.GetPin('A', 4); //MC_DA2
			socket.CpuPins[8] = Generic.GetPin('A', 5); //MC_DA3
			socket.CpuPins[9] = Generic.GetPin('A', 2); //MC_CK

			// F
			// N/A

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 8

			#region Socket 9
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
			socket.SupportedTypes = new char[] { 'Y' };
			socket.CpuPins[3] = Generic.GetPin('D', 9);
			socket.CpuPins[4] = Generic.GetPin('D', 10);
			socket.CpuPins[5] = Generic.GetPin('D', 12); //PCK1
			socket.CpuPins[6] = Generic.GetPin('D', 1); //AC97_FS
			socket.CpuPins[7] = Generic.GetPin('D', 3); //AC97_TX
			socket.CpuPins[8] = Generic.GetPin('D', 4); //AC97_RX
			socket.CpuPins[9] = Generic.GetPin('D', 2); //AC97_CK

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 9

			#region Socket 10
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
			socket.SupportedTypes = new char[] { 'R', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('C', 22);  //LCD_R0
			socket.CpuPins[4] = Generic.GetPin('C', 23); //LCD_R1
			socket.CpuPins[5] = Generic.GetPin('C', 24); //LCD_R2
			socket.CpuPins[6] = Generic.GetPin('C', 25);  //LCD_R3
			socket.CpuPins[7] = Generic.GetPin('C', 20);  //LCD_R4
			socket.CpuPins[8] = Generic.GetPin('C', 4);  //LCD_VSYNC
			socket.CpuPins[9] = Generic.GetPin('C', 5);  //LCD_HSYNC

			// R
			// N/A

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 10

			#region Socket 11
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
			socket.SupportedTypes = new char[] { 'G', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('C', 15); //LCD_G0
			socket.CpuPins[4] = Generic.GetPin('C', 16); //LCD_G1
			socket.CpuPins[5] = Generic.GetPin('C', 17); //LCD_G2
			socket.CpuPins[6] = Generic.GetPin('C', 18);  //LCD_G3
			socket.CpuPins[7] = Generic.GetPin('C', 19);  //LCD_G4
			socket.CpuPins[8] = Generic.GetPin('C', 21);  //LCD_G5
			socket.CpuPins[9] = Generic.GetPin('C', 3);  //LCD_PWM

			// G
			// N/A

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 11

			#region Socket 12
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
			socket.SupportedTypes = new char[] { 'B', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('C', 9); //LCD_B0
			socket.CpuPins[4] = Generic.GetPin('C', 10); //LCD_B1
			socket.CpuPins[5] = Generic.GetPin('C', 11); //LCD_B2
			socket.CpuPins[6] = Generic.GetPin('C', 12);  //LCD_B3
			socket.CpuPins[7] = Generic.GetPin('C', 13);  //LCD_B4
			socket.CpuPins[8] = Generic.GetPin('C', 7);  //LCD_EN
			socket.CpuPins[9] = Generic.GetPin('C', 6);  //LCD_CLK

			// B
			// N/A

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 12

			#region Socket 13
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(13);
			socket.SupportedTypes = new char[] { 'A', 'T', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('D', 6); //GPAD4
			socket.CpuPins[4] = Generic.GetPin('A', 20); //AD3YM
			socket.CpuPins[5] = Generic.GetPin('A', 18); //AD1XM
			socket.CpuPins[6] = Generic.GetPin('B', 1); //RXD3
			socket.CpuPins[7] = Generic.GetPin('B', 28);
			socket.CpuPins[8] = Generic.GetPin('B', 26);
			socket.CpuPins[9] = Generic.GetPin('B', 29);

			// A
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_4;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_3;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_1;

			// T
			// N/A
			
			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 13

			#region Socket 14
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(14);
			socket.SupportedTypes = new char[] { 'A', 'X' };
			socket.CpuPins[3] = Generic.GetPin('D', 7); //GPAD5
			socket.CpuPins[4] = Generic.GetPin('A', 19); //AD2YP
			socket.CpuPins[5] = Generic.GetPin('A', 17); //AD0XP
			socket.CpuPins[6] = Generic.GetPin('B', 0); //TXD3
			socket.CpuPins[7] = Generic.GetPin('B', 30);
			socket.CpuPins[8] = Generic.GetPin('B', 31);
			socket.CpuPins[9] = Generic.GetPin('B', 27);

			// A
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_5;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_2;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_0;

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 14
		}

        private static string[] sdVolumes = new string[] { "SD" };
        private Removable _storage;

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
            _storage = new Removable(volumeName);
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

		void BitmapConverter(Bitmap bmp, byte[] pixelBytes, GT.Mainboard.BPP bpp)
        {
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE)
                throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_LE supported");

            GHI.System.Utilities.BitmapHelpers.Convert(bmp, GHI.System.Utilities.BitmapHelpers.BitsPerPixel.BPP16_BGR_BE);
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

		private Microsoft.SPOT.Hardware.OutputPort debugled = new OutputPort(Generic.GetPin('D', 18), false);
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
			get { return "GHI Electronics FEZHydra"; }
		}

		/// <summary>
		/// The mainboard version, which is printed at startup in the debug window
		/// </summary>
		public override string MainboardVersion
		{
			get { return "1.2"; }
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

		#region Special Purpose Pins
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
		#endregion
	}

}
