using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GT = Gadgeteer;

//using EMX = GHI.Processor.EMX;
using EMX = GHI.Pins.EMX;
using GHI.Processor;
using GHI.IO;
using GHI.IO.Storage;
using GTM = Gadgeteer.Modules;

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
            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
			

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

			// X
			
			
			// K/U
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
			
			// Y
			
			
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
			socket.CpuPins[3] = EMX.IO30;
			socket.CpuPins[4] = EMX.IO29;
			socket.CpuPins[5] = EMX.IO28;
			socket.CpuPins[6] = EMX.IO16;
			socket.CpuPins[7] = EMX.IO74;
			socket.CpuPins[8] = EMX.IO48;
			socket.CpuPins[9] = EMX.IO49;

			// Y
			
			
			// U
			socket.SerialPortName = "COM3";

			// P
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

			// Y
			
			
			// U
			socket.SerialPortName = "COM4";
			
			// S
			socket.SPIModule = SPI.SPI_module.SPI1;

            // O
            socket.AnalogOutput5 = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0;
			
			// A
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_7;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_2;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_3;

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

			// X
			
			
			// A
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_6;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_1;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_0;
			
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

			// Y
			
			
			// U
			socket.SerialPortName = "COM1";
			
			// P
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

			// Y
			

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
			
			// Y
			
			
			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion
		}

		void BitmapConverter(Bitmap bmp, byte[] pixelBytes, GT.Mainboard.BPP bpp)
		{
			if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE)
				throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_LE supported");

			GHI.Utilities.Bitmaps.Convert(bmp, GHI.Utilities.Bitmaps.BitsPerPixel.BPP16_BGR_BE);
		}
		
		private Removable _storage;

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
            if (volumeName != "SD") throw new ArgumentException("volumeName");

            // implement this if you support storage devices. This should result in a <see cref="Microsoft.SPOT.IO.RemovableMedia.Insert"/> event if successful and return true if the volumeName is supported.
            _storage = new SD(SD.SDInterface.MCI);
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
		private const Cpu.Pin DebugLedPin = EMX.IO47;

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
}
