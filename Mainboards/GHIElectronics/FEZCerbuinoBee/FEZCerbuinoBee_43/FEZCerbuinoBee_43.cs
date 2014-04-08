using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GT = Gadgeteer;

using GHI.Processor;
using GHI.IO;
using GHI.IO.Storage;
using GHI.Pins;
using GTM = Gadgeteer.Modules;

namespace GHIElectronics.Gadgeteer
{
	/// <summary>
	/// Support class for GHI Electronics FEZCerbuinoBee for Microsoft .NET Gadgeteer
	/// </summary>
	public class FEZCerbuinoBee : GT.Mainboard
	{
		private bool configSet = false;
		// The mainboard constructor gets called before anything else in Gadgeteer (module constructors, etc), 
		// so it can set up fields in Gadgeteer.dll specifying socket types supported, etc.

		/// <summary>
		/// Instantiates a new FEZCerbuinoBee mainboard
		/// </summary>
		public FEZCerbuinoBee()
		{
			// uncomment the following if you support NativeI2CWriteRead for faster DaisyLink performance
            // otherwise, the DaisyLink I2C interface will be supported in Gadgeteer.dll in managed code.
            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
			

			this.NativeBitmapConverter = new BitmapConvertBPP(BitmapConverter);
			this.NativeBitmapCopyToSpi = this.NativeSPIBitmapPaint;

			GT.Socket socket;

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
			//socket.AnalogOutput = new FEZCerbuinoBee_AnalogOut((Cpu.Pin)14);
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
			socket.SupportedTypes = new char[] { 'P', 'S', 'U', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('A', 14);
			socket.CpuPins[4] = Generic.GetPin('B', 10);
			socket.CpuPins[5] = Generic.GetPin('B', 11);
			socket.CpuPins[6] = Generic.GetPin('A', 13);
			socket.CpuPins[7] = Generic.GetPin('B', 5);
			socket.CpuPins[8] = Generic.GetPin('B', 4);
			socket.CpuPins[9] = Generic.GetPin('B', 3);

			//P
			socket.PWM7 = Cpu.PWMChannel.PWM_6;
			socket.PWM8 = Cpu.PWMChannel.PWM_7;
			socket.PWM9 = (Cpu.PWMChannel)8;

			// S
			socket.SPIModule = SPI.SPI_module.SPI1;

			// U
			socket.SerialPortName = "COM3";

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 1

			#region Socket 2
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'A', 'I', 'K', 'U', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('A', 6);
			socket.CpuPins[4] = Generic.GetPin('A', 2);
			socket.CpuPins[5] = Generic.GetPin('A', 3);
			socket.CpuPins[6] = Generic.GetPin('A', 1);
			socket.CpuPins[7] = Generic.GetPin('A', 0);
			socket.CpuPins[8] = Generic.GetPin('B', 7);
			socket.CpuPins[9] = Generic.GetPin('B', 6);

			// A
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_0;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_1;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_2;

			// I
			// N/A

			// K/U
			socket.SerialPortName = "COM2";

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 2

			#region Socket 3
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
			socket.SupportedTypes = new char[] { 'A', 'O', 'P', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('C', 0);
			socket.CpuPins[4] = Generic.GetPin('C', 1);
			socket.CpuPins[5] = Generic.GetPin('A', 4);
			socket.CpuPins[6] = Generic.GetPin('C', 5);
			socket.CpuPins[7] = Generic.GetPin('B', 8);
			socket.CpuPins[8] = Generic.GetPin('A', 7);
			socket.CpuPins[9] = Generic.GetPin('B', 9);

			// A
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_3;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_4;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_5;

            // O
            socket.AnalogOutput5 = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0;

			// P
			socket.PWM7 = (Cpu.PWMChannel)14;
			socket.PWM8 = Cpu.PWMChannel.PWM_1;
			socket.PWM9 = (Cpu.PWMChannel)15;

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 3
		}

		private void NativeSPIBitmapPaint(Bitmap bitmap, SPI.Configuration config, int xSrc, int ySrc, int width, int height, GT.Mainboard.BPP bpp)
		{
			if (bpp != BPP.BPP16_BGR_BE)
				throw new ArgumentException("Invalid BPP");

			if (!this.configSet)
            {
                throw new Exception("Implement SetSpecialDisplayConfig.");
                //Util.SetSpecialDisplayConfig(config, Util.BPP_Type.BPP16_BGR_LE);

				//this.configSet = true;
			}

			bitmap.Flush(xSrc, ySrc, width, height);
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
        /// <param name="timing">The required timings from an LCD controller.</param>
        protected override void OnOnboardControllerDisplayConnected(string displayModel, int width, int height, int orientationDeg, GT.Modules.Module.DisplayModule.TimingRequirements timing)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Ensures that the pins on R, G and B sockets (which also have other socket types) are available for use for non-display purposes.
        /// If doing this requires rebooting, then the method must reboot and not return.
        /// If there is no onboard display controller, or it is not possible to disable the onboard display controller, then NotSupportedException must be thrown.
        /// </summary>
        public override void EnsureRgbSocketPinsAvailable()
        {
            throw new NotSupportedException("This mainboard does not support an onboard display controller.");
        }

        private Microsoft.SPOT.Hardware.OutputPort debugled = new OutputPort(Generic.GetPin('B', 2), false);

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
			get { return "GHI Electronics FEZCerbuinoBee"; }
		}

		/// <summary>
		/// The mainboard version, which is printed at startup in the debug window
		/// </summary>
		public override string MainboardVersion
		{
			get { return "1.2"; }
        }

        void BitmapConverter(Bitmap bmp, byte[] pixelBytes, GT.Mainboard.BPP bpp)
        {
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE)
                throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_LE supported");

            GHI.Utilities.Bitmaps.Convert(bmp, GHI.Utilities.Bitmaps.BitsPerPixel.BPP16_BGR_BE);
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

	}
}
