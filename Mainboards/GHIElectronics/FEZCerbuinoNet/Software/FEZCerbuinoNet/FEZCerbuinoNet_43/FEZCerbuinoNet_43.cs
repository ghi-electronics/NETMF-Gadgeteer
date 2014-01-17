using System;
using GHI.OSHW.Hardware;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using FEZCerb_Pins = GHI.Hardware.FEZCerb.Pin;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace GHIElectronics.Gadgeteer
{
	/// <summary>
	/// Support class for GHI Electronics FEZCerbuinoNet for Microsoft .NET Gadgeteer
	/// </summary>
	public class FEZCerbuinoNet : GT.Mainboard
	{
		private bool configSet = false;

		/// <summary>
		/// Instantiates a new FEZCerbuinoNet mainboard
		/// </summary>
		public FEZCerbuinoNet()
        {
            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);

			this.NativeBitmapConverter = new BitmapConvertBPP(this.BitmapConverter);
			this.NativeBitmapCopyToSpi = this.NativeSPIBitmapPaint;

			GT.Socket socket;

			#region Socket 1
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'P', 'S', 'U', 'X' };
			socket.CpuPins[3] = FEZCerb_Pins.PC13;
			socket.CpuPins[4] = FEZCerb_Pins.PC6;
			socket.CpuPins[5] = FEZCerb_Pins.PC7;
			socket.CpuPins[6] = FEZCerb_Pins.PB0;
			socket.CpuPins[7] = FEZCerb_Pins.PB5;
			socket.CpuPins[8] = FEZCerb_Pins.PB4;
			socket.CpuPins[9] = FEZCerb_Pins.PB3;

			//P
			socket.PWM7 = Cpu.PWMChannel.PWM_6;
			socket.PWM8 = Cpu.PWMChannel.PWM_7;
			socket.PWM9 = (Cpu.PWMChannel)8;

			// S
			socket.SPIModule = SPI.SPI_module.SPI1;

			// U
			socket.SerialPortName = "COM6";

			// Y
			

			GT.Socket.SocketInterfaces.RegisterSocket(socket);
			#endregion Socket 1

			#region Socket 2
			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'A', 'I', 'K', 'U', 'Y' };
			socket.CpuPins[3] = FEZCerb_Pins.PA6;
			socket.CpuPins[4] = FEZCerb_Pins.PA2;
			socket.CpuPins[5] = FEZCerb_Pins.PA3;
			socket.CpuPins[6] = FEZCerb_Pins.PA1;
			socket.CpuPins[7] = FEZCerb_Pins.PA0;
			socket.CpuPins[8] = FEZCerb_Pins.PB7;
			socket.CpuPins[9] = FEZCerb_Pins.PB6;

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
			socket.CpuPins[3] = FEZCerb_Pins.PC0;
			socket.CpuPins[4] = FEZCerb_Pins.PC1;
			socket.CpuPins[5] = FEZCerb_Pins.PA4;
			socket.CpuPins[6] = FEZCerb_Pins.PC5;
			socket.CpuPins[7] = FEZCerb_Pins.PB8;
			socket.CpuPins[8] = FEZCerb_Pins.PA7;
			socket.CpuPins[9] = FEZCerb_Pins.PB9;

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
				Util.SetSpecialDisplayConfig(config, Util.BPP_Type.BPP16_BGR_LE);

				this.configSet = true;
			}

			bitmap.Flush(xSrc, ySrc, width, height);
		}

		private static string[] sdVolumes = new string[] { "SD" };

		/// <summary>
		/// Allows mainboards to support storage device mounting/umounting.  This provides modules with a list of storage device volume names supported by the mainboard. 
		/// </summary>
		public override string[] GetStorageDeviceVolumeNames()
		{
			return sdVolumes;
		}

		/// <summary>
		/// Functionality provided by mainboard to ummount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a Microsoft.SPOT.IO.RemovableMedia.Eject event if successful.
		/// </summary>
		public override bool MountStorageDevice(string volumeName)
		{
			StorageDev.MountSD();

			return volumeName == "SD";
		}

		/// <summary>
		/// Functionality provided by mainboard to ummount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a Microsoft.SPOT.IO.RemovableMedia.Eject event if successful.
		/// </summary>
		public override bool UnmountStorageDevice(string volumeName)
		{
			StorageDev.UnmountSD();

			return volumeName == "SD";
		}

		/// <summary>
		/// Changes the programming interafces to the one specified
		/// </summary>
		/// <param name="programmingInterface">The programming interface to use</param>
		public override void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface)
		{
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

		private const Cpu.Pin DebugLedPin = FEZCerb_Pins.PB2;

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
			get { return "GHI Electronics FEZCerbuinoNet"; }
		}

		/// <summary>
		/// The mainboard version, which is printed at startup in the debug window
		/// </summary>
		public override string MainboardVersion
		{
			get { return "1.0"; }
		}

		private void BitmapConverter(Bitmap bmp, byte[] pixelBytes, GT.Mainboard.BPP bpp)
		{
			if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE)
				throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_LE supported");

			GHI.OSHW.Hardware.Util.BitmapConvertBPP(bmp.GetBitmap(), pixelBytes, Util.BPP_Type.BPP16_BGR_BE);
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
                GHI.OSHW.Hardware.SoftwareI2CBus.DirectI2CWriteRead(this.sclPin, this.sdaPin, 100, this.Address, writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out numWritten, out numRead);
            }
        }
	}
}
