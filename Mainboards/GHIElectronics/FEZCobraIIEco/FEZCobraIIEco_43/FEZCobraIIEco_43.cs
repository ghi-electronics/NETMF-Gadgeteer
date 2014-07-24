using GHI.IO;
using GHI.IO.Storage;
using GHI.Processor;
using GHI.Usb;
using GHI.Usb.Host;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Hardware;
using System;
using G120 = GHI.Pins.G120;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace GHIElectronics.Gadgeteer
{
    /// <summary>
    /// The mainboard class for the FEZ Cobra II Eco.
    /// </summary>
    public class FEZCobraIIEco : GT.Mainboard
    {
        private InterruptPort ldr0;
        private InterruptPort ldr1;
        private OutputPort debugLed;
        private IRemovable[] storageDevices;
        private MassStorage massStorageDevice;
        private InterruptPort sdCardDetect;
        private GT.StorageDevice storageDevice;
        private SDCardMountedEventHandler mounted;
        private SDCardUnmountedEventHandler unmounted;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public FEZCobraIIEco()
        {
            this.ldr0 = null;
            this.ldr1 = null;
            this.debugLed = null;
            this.storageDevices = new IRemovable[2];
            this.massStorageDevice = null;
            this.storageDevice = null;
            this.sdCardDetect = null;

            Controller.MassStorageConnected += (a, b) =>
            {
                this.massStorageDevice = b;
                this.massStorageDevice.Disconnected += (c, d) => this.UnmountStorageDevice("USB");
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
        }

        /// <summary>
        /// The name of the mainboard.
        /// </summary>
        public override string MainboardName
        {
            get { return "GHI Electronics FEZ Cobra II Eco"; }
        }

        /// <summary>
        /// The current version of the mainboard hardware.
        /// </summary>
        public override string MainboardVersion
        {
            get { return "Rev B"; }
        }

        /// <summary>
        /// The storage device volume names supported by this mainboard.
        /// </summary>
        /// <returns>The volume names.</returns>
        public override string[] GetStorageDeviceVolumeNames()
        {
            return new string[] { "SD", "USB" };
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
                    this.storageDevices[0] = new SDCard();
                    this.storageDevices[0].Mount();

                    break;

                case "USB":
                    if (this.massStorageDevice == null) throw new InvalidOperationException("No USB device is plugged into the device.");

                    this.storageDevices[1] = this.massStorageDevice;
                    this.storageDevices[1].Mount();

                    break;

                default:
                    throw new ArgumentException("volumeName must be present in the array returned by GetStorageDeviceVolumeNames.", "volumeName");
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

                case "USB":
                    if (this.storageDevices[1] == null) throw new InvalidOperationException("This volume is not mounted.");

                    this.storageDevices[1].Unmount();
                    this.storageDevices[1].Dispose();
                    this.storageDevices[1] = null;

                    break;

                default:
                    throw new ArgumentException("volumeName must be present in the array returned by GetStorageDeviceVolumeNames.", "volumeName");
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
            switch (orientationDeg)
            {
                case 0: Display.CurrentRotation = Display.Rotation.Normal; break;
                case 90: Display.CurrentRotation = Display.Rotation.Clockwise90; break;
                case 180: Display.CurrentRotation = Display.Rotation.Half; break;
                case 270: Display.CurrentRotation = Display.Rotation.CounterClockwise90; break;
                default: throw new ArgumentOutOfRangeException("orientationDeg", "orientationDeg must be 0, 90, 180, or 270.");
            }

            Display.Height = (uint)height;
            Display.HorizontalBackPorch = timing.HorizontalBackPorch;
            Display.HorizontalFrontPorch = timing.HorizontalFrontPorch;
            Display.HorizontalSyncPolarity = timing.HorizontalSyncPulseIsActiveHigh;
            Display.HorizontalSyncPulseWidth = timing.HorizontalSyncPulseWidth;
            Display.OutputEnableIsFixed = timing.UsesCommonSyncPin; //not the proper property, but we needed it;
            Display.OutputEnablePolarity = timing.CommonSyncPinIsActiveHigh; //not the proper property, but we needed it;
            Display.PixelClockRateKHz = timing.MaximumClockSpeed;
            Display.PixelPolarity = timing.PixelDataIsValidOnClockRisingEdge;
            Display.VerticalBackPorch = timing.VerticalBackPorch;
            Display.VerticalFrontPorch = timing.VerticalFrontPorch;
            Display.VerticalSyncPolarity = timing.VerticalSyncPulseIsActiveHigh;
            Display.VerticalSyncPulseWidth = timing.VerticalSyncPulseWidth;
            Display.Width = (uint)width;

            if (Display.Save())
            {
                Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
                Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

                Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
            }
        }

        /// <summary>
        /// Ensures that the RGB socket pins are available by disabling the display controller if needed.
        /// </summary>
        public override void EnsureRgbSocketPinsAvailable()
        {
            if (Display.Disable())
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
        /// The InterruptPort object for LDR0.
        /// </summary>
        public InterruptPort LDR0
        {
            get
            {
                if (this.ldr0 == null)
                    this.ldr0 = new InterruptPort(G120.P2_10, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);

                return this.ldr0;
            }
        }

        /// <summary>
        /// The InterruptPort object for LDR1.
        /// </summary>
        public InterruptPort LDR1
        {
            get
            {
                if (this.ldr1 == null)
                    this.ldr1 = new InterruptPort(G120.P0_22, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);

                return this.ldr1;
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

        /// <summary>
        /// Whether or not an SD card is inserted.
        /// </summary>
        public bool IsSDCardInserted
        {
            get
            {
                this.CheckSDCardDetectCreation();

                return !this.sdCardDetect.Read();
            }
        }

        /// <summary>
        /// Whether or not the SD card is mounted.
        /// </summary>
        public bool IsSDCardMounted { get; private set; }

        /// <summary>
        /// The StorageDevice for the currently mounted SD card.
        /// </summary>
        public GT.StorageDevice StorageDevice
        {
            get { return this.storageDevice; }
        }

        /// <summary>
        /// Attempts to mount the file system of the SD card.
        /// </summary>
        public void MountSDCard()
        {
            if (!this.IsSDCardMounted)
                this.IsSDCardMounted = this.MountStorageDevice("SD");
        }

        /// <summary>
        /// Attempts to unmount the file system of the SD card.
        /// </summary>
        public void UnmountSDCard()
        {
            if (this.IsSDCardMounted)
            {
                this.IsSDCardMounted = false;
                this.UnmountStorageDevice("SD");
                this.storageDevice = null;
            }
        }

        private void CheckSDCardDetectCreation()
        {
            if (this.sdCardDetect == null)
            {
                this.sdCardDetect = new InterruptPort(G120.P1_8, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
                this.sdCardDetect.OnInterrupt += this.OnSDCardDetect;
            }
        }

        private void OnSDCardDetect(uint data1, uint data2, DateTime when)
        {
            if (this.IsSDCardInserted)
            {
                this.MountSDCard();
            }
            else
            {
                this.UnmountSDCard();
            }
        }

        private void OnInsert(object sender, MediaEventArgs e)
        {
            if (e.Volume.Name.Length >= 2 && e.Volume.Name.Substring(0, 2) == "SD")
            {
                if (e.Volume.FileSystem != null)
                {
                    this.storageDevice = new GT.StorageDevice(e.Volume);
                    this.OnMounted(this, this.storageDevice);
                }
                else
                {
                    this.UnmountSDCard();
                }
            }

        }

        private void OnEject(object sender, MediaEventArgs e)
        {
            if (e.Volume.Name.Length >= 2 && e.Volume.Name.Substring(0, 2) == "SD")
                this.OnUnmounted(this, null);
        }

        /// <summary>
        /// Represents the delegate that is used for the <see cref="SDCardMounted"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="device">A storage device that can be used to access the SD non-volatile memory card.</param>
        public delegate void SDCardMountedEventHandler(FEZCobraIIEco sender, GT.StorageDevice device);

        /// <summary>
        /// Represents the delegate that is used for the <see cref="SDCardMounted"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void SDCardUnmountedEventHandler(FEZCobraIIEco sender, EventArgs e);

        /// <summary>
        /// Raised when the file system of the SD card is mounted.
        /// </summary>
        public event SDCardMountedEventHandler SDCardMounted
        {
            add
            {
                this.CheckSDCardDetectCreation();

                this.mounted += value;
            }
            remove
            {
                this.mounted -= value;
            }
        }

        /// <summary>
        /// Raised when the file system of the SD card is unmounted.
        /// </summary>
        public event SDCardUnmountedEventHandler SDCardUnmounted
        {
            add
            {
                this.CheckSDCardDetectCreation();

                this.unmounted += value;
            }
            remove
            {
                this.unmounted -= value;
            }
        }

        private SDCardMountedEventHandler onMounted;
        private SDCardUnmountedEventHandler onUnmounted;

        private void OnMounted(FEZCobraIIEco sender, GT.StorageDevice device)
        {
            if (this.onMounted == null)
                this.onMounted = this.OnMounted;

            if (GT.Program.CheckAndInvoke(this.mounted, this.onMounted, sender, device))
                this.mounted(sender, device);
        }

        private void OnUnmounted(FEZCobraIIEco sender, EventArgs e)
        {
            if (this.onUnmounted == null)
                this.onUnmounted = this.OnUnmounted;

            if (GT.Program.CheckAndInvoke(this.unmounted, this.onUnmounted, sender, e))
                this.unmounted(sender, e);
        }
    }
}