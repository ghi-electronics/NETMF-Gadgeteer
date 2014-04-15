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
    /// The mainboard class for the FEZ Hydra.
    /// </summary>
    public class FEZHydra : GT.Mainboard
    {
        private OutputPort debugLed;
        private Removable[] storageDevices;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public FEZHydra()
        {
            this.debugLed = null;
            this.storageDevices = new Removable[1];

            this.NativeBitmapConverter = this.BitmapConverter;

            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
            GT.Socket socket;


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
            socket.SupportedTypes = new char[] { 'Z' };
            socket.CpuPins[3] = (Cpu.Pin)SpecialPurposePin.RESET;
            socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.TCK;
            socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.RTC_BATT;
            socket.CpuPins[6] = (Cpu.Pin)SpecialPurposePin.TDO;
            socket.CpuPins[7] = (Cpu.Pin)SpecialPurposePin.TRST;
            socket.CpuPins[8] = (Cpu.Pin)SpecialPurposePin.TMS;
            socket.CpuPins[9] = (Cpu.Pin)SpecialPurposePin.TDI;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
            socket.SupportedTypes = new char[] { 'D' };
            socket.CpuPins[3] = Generic.GetPin('B', 19);
            socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_DM;
            socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_DP;
            socket.CpuPins[6] = Generic.GetPin('B', 18);
            socket.CpuPins[7] = Generic.GetPin('B', 22);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
            socket.SupportedTypes = new char[] { 'S', 'Y' };
            socket.CpuPins[3] = Generic.GetPin('B', 8);
            socket.CpuPins[4] = Generic.GetPin('B', 9);
            socket.CpuPins[5] = Generic.GetPin('B', 12);
            socket.CpuPins[6] = Generic.GetPin('B', 13);
            socket.CpuPins[7] = Generic.GetPin('A', 26);
            socket.CpuPins[8] = Generic.GetPin('A', 25);
            socket.CpuPins[9] = Generic.GetPin('A', 27);
            socket.I2CBusIndirector = nativeI2C;
            socket.SPIModule = SPI.SPI_module.SPI1;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
            socket.SupportedTypes = new char[] { 'S', 'U', 'Y' };
            socket.CpuPins[3] = Generic.GetPin('B', 2);
            socket.CpuPins[4] = Generic.GetPin('A', 11);
            socket.CpuPins[5] = Generic.GetPin('A', 12);
            socket.CpuPins[6] = Generic.GetPin('B', 14);
            socket.CpuPins[7] = Generic.GetPin('A', 26);
            socket.CpuPins[8] = Generic.GetPin('A', 25);
            socket.CpuPins[9] = Generic.GetPin('A', 27);
            socket.SerialPortName = "COM3";
            socket.I2CBusIndirector = nativeI2C;
            socket.SPIModule = SPI.SPI_module.SPI1;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
            socket.SupportedTypes = new char[] { 'I', 'U', 'X' };
            socket.CpuPins[3] = Generic.GetPin('A', 9);
            socket.CpuPins[4] = Generic.GetPin('A', 22);
            socket.CpuPins[5] = Generic.GetPin('A', 21);
            socket.CpuPins[6] = Generic.GetPin('A', 10);
            socket.CpuPins[8] = Generic.GetPin('A', 23);
            socket.CpuPins[9] = Generic.GetPin('A', 24);
            socket.SerialPortName = "COM1";
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
            socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
            socket.CpuPins[3] = Generic.GetPin('D', 17);
            socket.CpuPins[4] = Generic.GetPin('A', 13);
            socket.CpuPins[5] = Generic.GetPin('A', 14);
            socket.CpuPins[6] = Generic.GetPin('A', 29);
            socket.CpuPins[7] = Generic.GetPin('A', 30);
            socket.CpuPins[8] = Generic.GetPin('A', 23);
            socket.CpuPins[9] = Generic.GetPin('A', 24);
            socket.SerialPortName = "COM4";
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(7);
            socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
            socket.CpuPins[3] = Generic.GetPin('D', 19);
            socket.CpuPins[4] = Generic.GetPin('A', 6);
            socket.CpuPins[5] = Generic.GetPin('A', 7);
            socket.CpuPins[6] = Generic.GetPin('D', 20);
            socket.CpuPins[7] = Generic.GetPin('D', 14);
            socket.CpuPins[8] = Generic.GetPin('D', 15);
            socket.CpuPins[9] = Generic.GetPin('D', 16);
            socket.I2CBusIndirector = nativeI2C;
            socket.SerialPortName = "COM2";
            socket.PWM7 = Cpu.PWMChannel.PWM_0;
            socket.PWM8 = Cpu.PWMChannel.PWM_1;
            socket.PWM9 = Cpu.PWMChannel.PWM_2;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(8);
            socket.SupportedTypes = new char[] { 'F', 'Y' };
            socket.CpuPins[3] = Generic.GetPin('D', 11);
            socket.CpuPins[4] = Generic.GetPin('A', 0);
            socket.CpuPins[5] = Generic.GetPin('A', 3);
            socket.CpuPins[6] = Generic.GetPin('A', 1);
            socket.CpuPins[7] = Generic.GetPin('A', 4);
            socket.CpuPins[8] = Generic.GetPin('A', 5);
            socket.CpuPins[9] = Generic.GetPin('A', 2);
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
            socket.SupportedTypes = new char[] { 'Y' };
            socket.CpuPins[3] = Generic.GetPin('D', 9);
            socket.CpuPins[4] = Generic.GetPin('D', 10);
            socket.CpuPins[5] = Generic.GetPin('D', 12);
            socket.CpuPins[6] = Generic.GetPin('D', 1);
            socket.CpuPins[7] = Generic.GetPin('D', 3);
            socket.CpuPins[8] = Generic.GetPin('D', 4);
            socket.CpuPins[9] = Generic.GetPin('D', 2);
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
            socket.SupportedTypes = new char[] { 'R', 'Y' };
            socket.CpuPins[3] = Generic.GetPin('C', 22);
            socket.CpuPins[4] = Generic.GetPin('C', 23);
            socket.CpuPins[5] = Generic.GetPin('C', 24);
            socket.CpuPins[6] = Generic.GetPin('C', 25);
            socket.CpuPins[7] = Generic.GetPin('C', 20);
            socket.CpuPins[8] = Generic.GetPin('C', 4);
            socket.CpuPins[9] = Generic.GetPin('C', 5);
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
            socket.SupportedTypes = new char[] { 'G', 'Y' };
            socket.CpuPins[3] = Generic.GetPin('C', 15);
            socket.CpuPins[4] = Generic.GetPin('C', 16);
            socket.CpuPins[5] = Generic.GetPin('C', 17);
            socket.CpuPins[6] = Generic.GetPin('C', 18);
            socket.CpuPins[7] = Generic.GetPin('C', 19);
            socket.CpuPins[8] = Generic.GetPin('C', 21);
            socket.CpuPins[9] = Generic.GetPin('C', 3);
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
            socket.SupportedTypes = new char[] { 'B', 'Y' };
            socket.CpuPins[3] = Generic.GetPin('C', 9);
            socket.CpuPins[4] = Generic.GetPin('C', 10);
            socket.CpuPins[5] = Generic.GetPin('C', 11);
            socket.CpuPins[6] = Generic.GetPin('C', 12);
            socket.CpuPins[7] = Generic.GetPin('C', 13);
            socket.CpuPins[8] = Generic.GetPin('C', 7);
            socket.CpuPins[9] = Generic.GetPin('C', 6);
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(13);
            socket.SupportedTypes = new char[] { 'A', 'T', 'Y' };
            socket.CpuPins[3] = Generic.GetPin('D', 6);
            socket.CpuPins[4] = Generic.GetPin('A', 20);
            socket.CpuPins[5] = Generic.GetPin('A', 18);
            socket.CpuPins[6] = Generic.GetPin('B', 1);
            socket.CpuPins[7] = Generic.GetPin('B', 28);
            socket.CpuPins[8] = Generic.GetPin('B', 26);
            socket.CpuPins[9] = Generic.GetPin('B', 29);
            socket.I2CBusIndirector = nativeI2C;
            socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_4;
            socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_3;
            socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_1;
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(14);
            socket.SupportedTypes = new char[] { 'A', 'X' };
            socket.CpuPins[3] = Generic.GetPin('D', 7);
            socket.CpuPins[4] = Generic.GetPin('A', 19);
            socket.CpuPins[5] = Generic.GetPin('A', 17);
            socket.CpuPins[6] = Generic.GetPin('B', 0);
            socket.CpuPins[7] = Generic.GetPin('B', 30);
            socket.CpuPins[8] = Generic.GetPin('B', 31);
            socket.CpuPins[9] = Generic.GetPin('B', 27);
            socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_5;
            socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_2;
            socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_0;
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 10);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);
        }

        /// <summary>
        /// The name of the mainboard.
        /// </summary>
        public override string MainboardName
        {
            get { return "GHI Electronics FEZ Hydra"; }
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
            Configuration.Display.Height = (uint)height;
            Configuration.Display.HorizontalBackPorch = timing.HorizontalBackPorch;
            Configuration.Display.HorizontalFrontPorch = timing.HorizontalFrontPorch;
            Configuration.Display.HorizontalSyncPolarity = timing.HorizontalSyncPulseIsActiveHigh;
            Configuration.Display.HorizontalSyncPulseWidth = timing.HorizontalSyncPulseWidth;
            Configuration.Display.OutputEnableIsFixed = timing.UsesCommonSyncPin; //not the proper property, but we needed it;
            Configuration.Display.OutputEnablePolarity = timing.CommonSyncPinIsActiveHigh; //not the proper property, but we needed it;
            Configuration.Display.PixelClockRateKHz = timing.MaximumClockSpeed;
            Configuration.Display.PixelPolarity = timing.PixelDataIsValidOnClockRisingEdge;
            Configuration.Display.VerticalBackPorch = timing.VerticalBackPorch;
            Configuration.Display.VerticalFrontPorch = timing.VerticalFrontPorch;
            Configuration.Display.VerticalSyncPolarity = timing.VerticalSyncPulseIsActiveHigh;
            Configuration.Display.VerticalSyncPulseWidth = timing.VerticalSyncPulseWidth;
            Configuration.Display.Width = (uint)width;

            if (Configuration.Display.Save())
            {
                Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
                Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

                Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
            }

            switch (orientationDeg)
            {
                case 0: Configuration.Display.CurrentRotation = Configuration.Display.Rotation.Normal; break;
                default: throw new ArgumentOutOfRangeException("orientationDeg", "orientationDeg must be 0, 90, 180, or 270.");
            }
        }

        /// <summary>
        /// Ensures that the RGB socket pins are available by disabling the display controller if needed.
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

        /// <summary>
        /// Sets the state of the debug LED.
        /// </summary>
        /// <param name="on">The new state.</param>
        public override void SetDebugLED(bool on)
        {
            if (this.debugLed == null)
                this.debugLed = new OutputPort(Generic.GetPin('D', 18), on);

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

        private void BitmapConverter(Bitmap bitmap, byte[] pixelBytes, GT.Mainboard.BPP bpp)
        {
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE) throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

            GHI.Utilities.Bitmaps.Convert(bitmap, GHI.Utilities.Bitmaps.BitsPerPixel.BPP16_BGR_BE, pixelBytes);
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

        private enum SpecialPurposePin
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