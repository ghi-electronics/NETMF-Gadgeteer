using GHI.IO;
using GHI.IO.Storage;
using GHI.Networking;
using GHI.Processor;
using GHI.Usb;
using GHI.Usb.Host;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using G120 = GHI.Pins.G120;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace GHIElectronics.Gadgeteer
{
    /// <summary>
    /// The mainboard class for the FEZ Cobra II Net.
    /// </summary>
    public class FEZCobraIINet : GT.Mainboard
    {
        private OutputPort debugLed;
        private Removable[] storageDevices;
        private Device usbMassStorageDevice;
        private EthernetENC28J60 ethernet;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public FEZCobraIINet()
        {
            this.debugLed = null;
            this.storageDevices = new Removable[2];
            this.usbMassStorageDevice = null;
            this.ethernet = null;

            Controller.DeviceConnected += (a, b) =>
            {
                if (b.Device.Type == GHI.Usb.Device.DeviceType.MassStorage)
                {
                    this.usbMassStorageDevice = b.Device;
                    this.usbMassStorageDevice.Disconnected += (c, d) => this.UnmountStorageDevice("USB MassStorage");
                }
            };

            this.NativeBitmapConverter = this.BitmapConverter;

            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
            GT.Socket socket;


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
            socket.SupportedTypes = new char[] { 'B', 'Y' };
            socket.CpuPins[3] = G120.P2_13;
            socket.CpuPins[4] = G120.P1_26;
            socket.CpuPins[5] = G120.P1_27;
            socket.CpuPins[6] = G120.P1_28;
            socket.CpuPins[7] = G120.P1_29;
            socket.CpuPins[8] = G120.P2_4;
            socket.CpuPins[9] = G120.P2_2;
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
            socket.SupportedTypes = new char[] { 'G' };
            socket.CpuPins[3] = G120.P1_20;
            socket.CpuPins[4] = G120.P1_21;
            socket.CpuPins[5] = G120.P1_22;
            socket.CpuPins[6] = G120.P1_23;
            socket.CpuPins[7] = G120.P1_24;
            socket.CpuPins[8] = G120.P1_25;
            socket.CpuPins[9] = G120.P1_19;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
            socket.SupportedTypes = new char[] { 'R', 'Y' };
            socket.CpuPins[3] = G120.P2_12;
            socket.CpuPins[4] = G120.P2_6;
            socket.CpuPins[5] = G120.P2_7;
            socket.CpuPins[6] = G120.P2_8;
            socket.CpuPins[7] = G120.P2_9;
            socket.CpuPins[8] = G120.P2_3;
            socket.CpuPins[9] = G120.P2_5;
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
            socket.SupportedTypes = new char[] { 'A', 'I', 'T', 'X' };
            socket.CpuPins[3] = G120.P0_25;
            socket.CpuPins[4] = G120.P0_24;
            socket.CpuPins[5] = G120.P0_23;
            socket.CpuPins[6] = G120.P1_0;
            socket.CpuPins[7] = G120.P1_1;
            socket.CpuPins[8] = G120.P0_27;
            socket.CpuPins[9] = G120.P0_28;
            socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_2;
            socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_1;
            socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_0;
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
            socket.SupportedTypes = new char[] { 'U', 'X' };
            socket.CpuPins[3] = G120.P0_13;
            socket.CpuPins[4] = G120.P0_2;
            socket.CpuPins[5] = G120.P0_3;
            socket.CpuPins[6] = G120.P1_4;
            socket.SerialPortName = "COM1";
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
            socket.SupportedTypes = new char[] { 'S', 'X' };
            socket.CpuPins[3] = G120.P2_21;
            socket.CpuPins[4] = G120.P1_14;
            socket.CpuPins[5] = G120.P1_16;
            socket.CpuPins[6] = G120.P1_17;
            socket.CpuPins[7] = (Cpu.Pin)9;
            socket.CpuPins[8] = (Cpu.Pin)8;
            socket.CpuPins[9] = (Cpu.Pin)7;
            socket.SPIModule = SPI.SPI_module.SPI2;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(7);
            socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
            socket.CpuPins[3] = G120.P0_4;
            socket.CpuPins[4] = G120.P4_28;
            socket.CpuPins[5] = G120.P4_29;
            socket.CpuPins[6] = G120.P1_30;
            socket.CpuPins[7] = G120.P3_26;
            socket.CpuPins[8] = G120.P3_25;
            socket.CpuPins[9] = G120.P3_24;
            socket.SerialPortName = "COM4";
            socket.PWM7 = (Cpu.PWMChannel)8;
            socket.PWM8 = Cpu.PWMChannel.PWM_7;
            socket.PWM9 = Cpu.PWMChannel.PWM_6;
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(8);
            socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
            socket.CpuPins[3] = G120.P0_10;
            socket.CpuPins[4] = G120.P2_0;
            socket.CpuPins[5] = G120.P0_16;
            socket.CpuPins[6] = G120.P0_6;
            socket.CpuPins[7] = G120.P0_17;
            socket.CpuPins[8] = G120.P0_27;
            socket.CpuPins[9] = G120.P0_28;
            socket.SerialPortName = "COM2";
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
            socket.SupportedTypes = new char[] { 'A', 'O', 'S', 'X' };
            socket.CpuPins[3] = G120.P0_12;
            socket.CpuPins[4] = G120.P1_31;
            socket.CpuPins[5] = G120.P0_26;
            socket.CpuPins[6] = G120.P1_5;
            socket.CpuPins[7] = G120.P0_18;
            socket.CpuPins[8] = G120.P0_17;
            socket.CpuPins[9] = G120.P0_15;
            socket.SPIModule = SPI.SPI_module.SPI1;
            socket.AnalogOutput5 = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0;
            socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_6;
            socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_5;
            socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_3;
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
            socket.SupportedTypes = new char[] { 'C', 'I', 'X' };
            socket.CpuPins[3] = G120.P0_11;
            socket.CpuPins[4] = G120.P0_1;
            socket.CpuPins[5] = G120.P0_0;
            socket.CpuPins[6] = G120.P0_5;
            socket.CpuPins[8] = G120.P0_27;
            socket.CpuPins[9] = G120.P0_28;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);
        }

        /// <summary>
        /// The name of the mainboard.
        /// </summary>
        public override string MainboardName
        {
            get { return "GHI Electronics FEZ Cobra II Net"; }
        }

        /// <summary>
        /// The current version of the mainboard hardware.
        /// </summary>
        public override string MainboardVersion
        {
            get { return "Rev A"; }
        }

        /// <summary>
        /// The storage device volume names supported by this mainboard.
        /// </summary>
        /// <returns>The volume names.</returns>
        public override string[] GetStorageDeviceVolumeNames()
        {
            return new string[] { "SD", "USB MassStorage" };
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

                case "USB MassStorage":
                    if (this.usbMassStorageDevice == null) throw new InvalidOperationException("No USB MassStorage device is plugged into the device.");

                    this.storageDevices[1] = new UsbMassStorage(this.usbMassStorageDevice);
                    this.storageDevices[1].Mount();

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

                case "USB MassStorage":
                    if (this.storageDevices[1] == null) throw new InvalidOperationException("This volume is not mounted.");

                    this.storageDevices[1].Unmount();
                    this.storageDevices[1].Dispose();
                    this.storageDevices[1] = null;

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
                case 90: Configuration.Display.CurrentRotation = Configuration.Display.Rotation.Clockwise90; break;
                case 180: Configuration.Display.CurrentRotation = Configuration.Display.Rotation.Half; break;
                case 270: Configuration.Display.CurrentRotation = Configuration.Display.Rotation.CounterClockwise90; break;
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
                this.debugLed = new OutputPort(G120.P1_15, on);

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

        /// <summary>
        /// Represents the ENC28J60 chip on the mainboard.
        /// </summary>
        public EthernetENC28J60 Ethernet
        {
            get
            {
                if (this.ethernet == null)
                    this.ethernet = new EthernetENC28J60(SPI.SPI_module.SPI2, G120.P1_10, G120.P2_11, G120.P1_9, 4000);

                return this.ethernet;
            }
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
    }
}