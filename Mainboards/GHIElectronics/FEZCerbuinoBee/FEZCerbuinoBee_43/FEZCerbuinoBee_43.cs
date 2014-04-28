using GHI.IO;
using GHI.IO.Storage;
using GHI.Pins;
using GHI.Processor;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace GHIElectronics.Gadgeteer
{
    /// <summary>
    /// The mainboard class for the FEZ Cerbuino Bee.
    /// </summary>
    public class FEZCerbuinoBee : GT.Mainboard
    {
        private bool configSet;
        private OutputPort debugLed;
        private Removable[] storageDevices;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public FEZCerbuinoBee()
        {
            this.configSet = false;
            this.debugLed = null;
            this.storageDevices = new Removable[1];

            this.NativeBitmapConverter = this.NativeBitmapConvert;
            this.NativeBitmapCopyToSpi = this.NativeBitmapSpi;

            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
            GT.Socket socket;


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'P', 'S', 'U', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('A', 14);
			socket.CpuPins[4] = Generic.GetPin('B', 10);
			socket.CpuPins[5] = Generic.GetPin('B', 11);
			socket.CpuPins[6] = Generic.GetPin('A', 13);
			socket.CpuPins[7] = Generic.GetPin('B', 5);
			socket.CpuPins[8] = Generic.GetPin('B', 4);
			socket.CpuPins[9] = Generic.GetPin('B', 3);
            socket.I2CBusIndirector = nativeI2C;
			socket.PWM7 = Cpu.PWMChannel.PWM_6;
			socket.PWM8 = Cpu.PWMChannel.PWM_7;
			socket.PWM9 = (Cpu.PWMChannel)8;
            socket.SPIModule = SPI.SPI_module.SPI1;
			socket.SerialPortName = "COM3";
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'A', 'I', 'K', 'U', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('A', 6);
			socket.CpuPins[4] = Generic.GetPin('A', 2);
			socket.CpuPins[5] = Generic.GetPin('A', 3);
			socket.CpuPins[6] = Generic.GetPin('A', 1);
			socket.CpuPins[7] = Generic.GetPin('A', 0);
			socket.CpuPins[8] = Generic.GetPin('B', 7);
            socket.CpuPins[9] = Generic.GetPin('B', 6);
            socket.I2CBusIndirector = nativeI2C;
            socket.SerialPortName = "COM2";
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_0;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_1;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_2;
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
			socket.SupportedTypes = new char[] { 'A', 'O', 'P', 'Y' };
			socket.CpuPins[3] = Generic.GetPin('C', 0);
			socket.CpuPins[4] = Generic.GetPin('C', 1);
			socket.CpuPins[5] = Generic.GetPin('A', 4);
			socket.CpuPins[6] = Generic.GetPin('C', 5);
			socket.CpuPins[7] = Generic.GetPin('B', 8);
			socket.CpuPins[8] = Generic.GetPin('A', 7);
            socket.CpuPins[9] = Generic.GetPin('B', 9);
            socket.I2CBusIndirector = nativeI2C;
            socket.PWM7 = (Cpu.PWMChannel)14;
            socket.PWM8 = Cpu.PWMChannel.PWM_1;
            socket.PWM9 = (Cpu.PWMChannel)15;
            socket.AnalogOutput5 = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0;
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_3;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_4;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_5;
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
            GT.Socket.SocketInterfaces.SetAnalogOutputFactors(socket, 3.3, 0, 12);
			GT.Socket.SocketInterfaces.RegisterSocket(socket);
        }

        /// <summary>
        /// The name of the mainboard.
        /// </summary>
        public override string MainboardName
        {
            get { return "GHI Electronics FEZ Cerbuino Bee"; }
        }

        /// <summary>
        /// The current version of the mainboard hardware.
        /// </summary>
        public override string MainboardVersion
        {
            get { return "1.2"; }
        }

        /// <summary>
        /// The storage device volume names supported by this mainboard.
        /// </summary>
        /// <returns>The volume names.</returns>
        public override string[] GetStorageDeviceVolumeNames()
        {
            return new string[] { "SD" };
        }

        /// <summary>
        /// Mounts the device with the given name.
        /// </summary>
        /// <param name="volumeName">The device to mount.</param>
        /// <returns>Whether or not the mount was successful.</returns>
        public override bool MountStorageDevice(string volumeName)
        {
            switch (volumeName)
            {
                case "SD":
                    this.storageDevices[0] = new SD(SD.SDInterface.MCI);
                    this.storageDevices[0].Mount();

                    break;

                default:
                    throw new ArgumentException("volumeName", "volumeName must be present in the array returned by GetStorageDeviceVolumeNames.");
            }

            return true;
        }

        /// <summary>
        /// Unmounts the device with the given name.
        /// </summary>
        /// <param name="volumeName">The device to unmount.</param>
        /// <returns>Whether or not the unmount was successful.</returns>
        public override bool UnmountStorageDevice(string volumeName)
        {
            switch (volumeName)
            {
                case "SD":
                    if (this.storageDevices[0] == null) throw new InvalidOperationException("This volume is not mounted.");

                    this.storageDevices[0].Unmount();
                    this.storageDevices[0].Dispose();
                    this.storageDevices[0] = null;

                    break;

                default:
                    throw new ArgumentException("volumeName", "volumeName must be present in the array returned by GetStorageDeviceVolumeNames.");
            }

            return true;
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
        protected override void OnOnboardControllerDisplayConnected(string displayModel, int width, int height, int orientationDeg, GTM.Module.DisplayModule.TimingRequirements timing)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Ensures that the RGB socket pins are available by disabling the display controller if needed.
        /// </summary>
        public override void EnsureRgbSocketPinsAvailable()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the state of the debug LED.
        /// </summary>
        /// <param name="on">The new state.</param>
        public override void SetDebugLED(bool on)
        {
            if (this.debugLed == null)
                this.debugLed = new OutputPort(Generic.GetPin('B', 2), on);

            this.debugLed.Write(on);
        }

        /// <summary>
        /// Sets the programming mode of the device.
        /// </summary>
        /// <param name="programmingInterface">The new programming mode.</param>
        public override void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This performs post-initialization tasks for the mainboard.  It is called by Gadgeteer.Program.Run and does not need to be called manually.
        /// </summary>
        public override void PostInit()
        {

        }

        private void NativeBitmapConvert(Bitmap bitmap, byte[] pixelBytes, GT.Mainboard.BPP bpp)
        {
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE) throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

            GHI.Utilities.Bitmaps.Convert(bitmap, GHI.Utilities.Bitmaps.BitsPerPixel.BPP16_BGR_BE, pixelBytes);
        }

        private void NativeBitmapSpi(Bitmap bitmap, SPI.Configuration config, int xSrc, int ySrc, int width, int height, GT.Mainboard.BPP bpp)
		{
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE) throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

			if (!this.configSet)
            {
                Configuration.Display.Populate(Configuration.Display.GHIDisplay.DisplayN18);
                Configuration.Display.SpiConfiguration = config;
                Configuration.Display.Bpp = GHI.Utilities.Bitmaps.BitsPerPixel.BPP16_BGR_BE;
                Configuration.Display.Save();

				this.configSet = true;
			}

			bitmap.Flush(xSrc, ySrc, width, height);
		}

        private class InteropI2CBus : GT.SocketInterfaces.I2CBus
        {
            public override ushort Address { get; set; }
            public override int Timeout { get; set; }
            public override int ClockRateKHz { get; set; }

            private SoftwareI2CBus softwareBus;

            public InteropI2CBus(GT.Socket socket, GT.Socket.Pin sdaPin, GT.Socket.Pin sclPin, ushort address, int clockRateKHz, GTM.Module module)
            {
                this.Address = address;
                this.ClockRateKHz = clockRateKHz;

                this.softwareBus = new SoftwareI2CBus(socket.CpuPins[(int)sclPin], socket.CpuPins[(int)sdaPin]);
            }

            public override void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int numWritten, out int numRead)
            {
                this.softwareBus.WriteRead((byte)this.Address, writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out numWritten, out numRead);
            }
        }
    }
}			