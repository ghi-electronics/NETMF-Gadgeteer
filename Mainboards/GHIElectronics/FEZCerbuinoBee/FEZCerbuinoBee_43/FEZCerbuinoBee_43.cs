using GHI.IO;
using GHI.IO.Storage;
using GHI.Pins;
using GHI.Processor;
using GHI.Usb;
using GHI.Usb.Host;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.IO;
using System;
using System.Threading;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using FEZCerbuinoBeePins = GHI.Pins.FEZCerbuinoBee;

namespace GHIElectronics.Gadgeteer {
    /// <summary>The mainboard class for the FEZ Cerbuino Bee.</summary>
    public class FEZCerbuinoBee : GT.Mainboard {
        private bool configSet;
        private OutputPort debugLed;
        private IRemovable[] storageDevices;
        private InterruptPort sdCardDetect;
        private GT.StorageDevice sdCardStorageDevice;
        private GT.StorageDevice massStorageDevice;
        private Keyboard connectedKeyboard;
        private Mouse connectedMouse;

        /// <summary>The name of the mainboard.</summary>
        public override string MainboardName {
            get { return "GHI Electronics FEZ Cerbuino Bee"; }
        }

        /// <summary>The current version of the mainboard hardware.</summary>
        public override string MainboardVersion {
            get { return "1.2"; }
        }

        /// <summary>Constructs a new instance.</summary>
        public FEZCerbuinoBee() {
            this.configSet = false;
            this.debugLed = null;
            this.storageDevices = new IRemovable[2];
            this.sdCardStorageDevice = null;

            RemovableMedia.Insert += this.OnInsert;
            RemovableMedia.Eject += this.OnEject;

            Controller.MouseConnected += (a, b) => {
                this.connectedMouse = b;
                this.OnMouseConnected(this, b);

                b.Disconnected += (c, d) => this.connectedMouse = null;
            };

            Controller.KeyboardConnected += (a, b) => {
                this.connectedKeyboard = b;
                this.OnKeyboardConnected(this, b);

                b.Disconnected += (c, d) => this.connectedKeyboard = null;
            };

            Controller.MassStorageConnected += (a, b) => {
                this.IsMassStorageConnected = true;

                this.MountStorageDevice("USB");

                b.Disconnected += (c, d) => {
                    this.IsMassStorageConnected = false;

                    if (this.IsMassStorageMounted)
                        this.UnmountStorageDevice("USB");
                };
            };

            this.IsSDCardMounted = false;
            this.IsMassStorageConnected = false;
            this.IsMassStorageMounted = false;

            this.sdCardDetect = new InterruptPort(FEZCerbuinoBeePins.SdCardDetect, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
            this.sdCardDetect.OnInterrupt += this.OnSDCardDetect;

            if (this.IsSDCardInserted)
                this.MountStorageDevice("SD");

            Controller.Start();

            this.NativeBitmapConverter = this.NativeBitmapConvert;
            this.NativeBitmapCopyToSpi = this.NativeBitmapSpi;

            #region Sockets
            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
            GT.Socket socket;

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
            socket.SupportedTypes = new char[] { 'P', 'S', 'U', 'Y' };
            socket.CpuPins[3] = FEZCerbuinoBeePins.Socket1.Pin3;
            socket.CpuPins[4] = FEZCerbuinoBeePins.Socket1.Pin4;
            socket.CpuPins[5] = FEZCerbuinoBeePins.Socket1.Pin5;
            socket.CpuPins[6] = FEZCerbuinoBeePins.Socket1.Pin6;
            socket.CpuPins[7] = FEZCerbuinoBeePins.Socket1.Pin7;
            socket.CpuPins[8] = FEZCerbuinoBeePins.Socket1.Pin8;
            socket.CpuPins[9] = FEZCerbuinoBeePins.Socket1.Pin9;
            socket.I2CBusIndirector = nativeI2C;
            socket.PWM7 = Cpu.PWMChannel.PWM_6;
            socket.PWM8 = Cpu.PWMChannel.PWM_7;
            socket.PWM9 = (Cpu.PWMChannel)8;
            socket.SPIModule = SPI.SPI_module.SPI1;
            socket.SerialPortName = "COM3";
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
            socket.SupportedTypes = new char[] { 'A', 'I', 'K', 'U', 'Y' };
            socket.CpuPins[3] = FEZCerbuinoBeePins.Socket2.Pin3;
            socket.CpuPins[4] = FEZCerbuinoBeePins.Socket2.Pin4;
            socket.CpuPins[5] = FEZCerbuinoBeePins.Socket2.Pin5;
            socket.CpuPins[6] = FEZCerbuinoBeePins.Socket2.Pin6;
            socket.CpuPins[7] = FEZCerbuinoBeePins.Socket2.Pin7;
            socket.CpuPins[8] = FEZCerbuinoBeePins.Socket2.Pin8;
            socket.CpuPins[9] = FEZCerbuinoBeePins.Socket2.Pin9;
            socket.I2CBusIndirector = nativeI2C;
            socket.SerialPortName = "COM2";
            socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_0;
            socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_1;
            socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_2;
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
            socket.SupportedTypes = new char[] { 'A', 'O', 'P', 'Y' };
            socket.CpuPins[3] = FEZCerbuinoBeePins.Socket3.Pin3;
            socket.CpuPins[4] = FEZCerbuinoBeePins.Socket3.Pin4;
            socket.CpuPins[5] = FEZCerbuinoBeePins.Socket3.Pin5;
            socket.CpuPins[6] = FEZCerbuinoBeePins.Socket3.Pin6;
            socket.CpuPins[7] = FEZCerbuinoBeePins.Socket3.Pin7;
            socket.CpuPins[8] = FEZCerbuinoBeePins.Socket3.Pin8;
            socket.CpuPins[9] = FEZCerbuinoBeePins.Socket3.Pin9;
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

            #endregion Sockets
        }

        /// <summary>The storage device volume names supported by this mainboard.</summary>
        /// <returns>The volume names.</returns>
        public override string[] GetStorageDeviceVolumeNames() {
            return new string[] { "SD", "USB" };
        }

        /// <summary>Mounts the device with the given name.</summary>
        /// <param name="volumeName">The device to mount.</param>
        /// <returns>Whether or not the mount was successful.</returns>
        public override bool MountStorageDevice(string volumeName) {
            try {
                if (volumeName == "SD" && this.storageDevices[0] == null) {
                    this.storageDevices[0] = new SDCard();
                    this.storageDevices[0].Mount();

                    return true;
                }
                else if (volumeName == "USB" && this.storageDevices[1] == null) {
                    foreach (BaseDevice dev in Controller.GetConnectedDevices()) {
                        if (dev.GetType() == typeof(MassStorage)) {
                            this.storageDevices[1] = (MassStorage)dev;
                            this.storageDevices[1].Mount();

                            return true;
                        }
                    }
                }
            }
            catch {
            }

            return false;
        }

        /// <summary>Unmounts the device with the given name.</summary>
        /// <param name="volumeName">The device to unmount.</param>
        /// <returns>Whether or not the unmount was successful.</returns>
        public override bool UnmountStorageDevice(string volumeName) {
            if (volumeName == "SD" && this.storageDevices[0] != null) {
                this.storageDevices[0].Dispose();
                this.storageDevices[0] = null;
            }
            else if (volumeName == "USB" && this.storageDevices[1] != null) {
                this.storageDevices[1].Dispose();
                this.storageDevices[1] = null;
            }
            else {
                return false;
            }

            return true;
        }

        /// <summary>Ensures that the RGB socket pins are available by disabling the display controller if needed.</summary>
        public override void EnsureRgbSocketPinsAvailable() {
            throw new NotSupportedException();
        }

        /// <summary>Sets the state of the debug LED.</summary>
        /// <param name="on">The new state.</param>
        public override void SetDebugLED(bool on) {
            if (this.debugLed == null)
                this.debugLed = new OutputPort(FEZCerbuinoBeePins.DebugLed, on);

            this.debugLed.Write(on);
        }

        /// <summary>Sets the programming mode of the device.</summary>
        /// <param name="programmingInterface">The new programming mode.</param>
        public override void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface) {
            throw new NotSupportedException();
        }

        /// <summary>This performs post-initialization tasks for the mainboard. It is called by Gadgeteer.Program.Run and does not need to be called manually.</summary>
        public override void PostInit() {
        }

        /// <summary>
        /// Configure the onboard display controller to fulfil the requirements of a display using the RGB sockets. If doing this requires rebooting, then the method must reboot and not return. If
        /// there is no onboard display controller, then NotSupportedException must be thrown.
        /// </summary>
        /// <param name="displayModel">Display model name.</param>
        /// <param name="width">Display physical width in pixels, ignoring the orientation setting.</param>
        /// <param name="height">Display physical height in lines, ignoring the orientation setting.</param>
        /// <param name="orientationDeg">Display orientation in degrees.</param>
        /// <param name="timing">The required timings from an LCD controller.</param>
        protected override void OnOnboardControllerDisplayConnected(string displayModel, int width, int height, int orientationDeg, GTM.Module.DisplayModule.TimingRequirements timing) {
            throw new NotSupportedException();
        }

        private void NativeBitmapConvert(Bitmap bitmap, byte[] pixelBytes, GT.Mainboard.BPP bpp) {
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE) throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

            GHI.Utilities.Bitmaps.Convert(bitmap, GHI.Utilities.Bitmaps.Format.Bpp16BgrBe, pixelBytes);
        }

        private void NativeBitmapSpi(Bitmap bitmap, SPI.Configuration config, int xSrc, int ySrc, int width, int height, GT.Mainboard.BPP bpp) {
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE) throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

			if (!this.configSet) {
				Display.BitmapFormat = GHI.Utilities.Bitmaps.Format.Bpp16BgrBe;
				Display.CurrentRotation = Display.Rotation.Normal;
				Display.Type = Display.DisplayType.Spi;
				Display.ControlPin = Cpu.Pin.GPIO_NONE;
				Display.BacklightPin = Cpu.Pin.GPIO_NONE;
				Display.ResetPin = Cpu.Pin.GPIO_NONE;
				Display.SpiConfiguration = config;
				Display.Width = bitmap.Width;
				Display.Height = bitmap.Height;

				Display.Save();

				this.configSet = true;
			}

            bitmap.Flush(xSrc, ySrc, width, height);
        }

        private void OnInsert(object sender, MediaEventArgs e) {
            if (string.Compare(e.Volume.Name, "USB") == 0) {
                if (e.Volume.FileSystem != null) {
                    this.massStorageDevice = new GT.StorageDevice(e.Volume);
                    this.IsMassStorageMounted = true;
                    this.OnMassStorageMounted(this, this.massStorageDevice);
                }
                else {
                    this.massStorageDevice = null;
                    this.IsMassStorageMounted = false;
                    this.UnmountStorageDevice("USB");
                    Debug.Print("The mass storage device does not have a valid filesystem.");
                }
            }
            else if (string.Compare(e.Volume.Name, "SD") == 0) {
                if (e.Volume.FileSystem != null) {
                    this.sdCardStorageDevice = new GT.StorageDevice(e.Volume);
                    this.IsSDCardMounted = true;
                    this.OnSDCardMounted(this, this.sdCardStorageDevice);
                }
                else {
                    this.sdCardStorageDevice = null;
                    this.IsSDCardMounted = false;
                    this.UnmountStorageDevice("SD");
                    Debug.Print("The SD card does not have a valid filesystem.");
                }
            }
        }

        private void OnEject(object sender, MediaEventArgs e) {
            if (string.Compare(e.Volume.Name, "USB") == 0) {
                this.massStorageDevice = null;
                this.IsMassStorageMounted = false;
                this.OnMassStorageUnmounted(this, null);
            }
            else if (string.Compare(e.Volume.Name, "SD") == 0) {
                this.sdCardStorageDevice = null;
                this.IsSDCardMounted = false;
                this.OnSDCardUnmounted(this, null);
            }
        }

        private class InteropI2CBus : GT.SocketInterfaces.I2CBus {
            private SoftwareI2CBus softwareBus;

            public override ushort Address { get; set; }

            public override int Timeout { get; set; }

            public override int ClockRateKHz { get; set; }

            public InteropI2CBus(GT.Socket socket, GT.Socket.Pin sdaPin, GT.Socket.Pin sclPin, ushort address, int clockRateKHz, GTM.Module module) {
                this.Address = address;
                this.ClockRateKHz = clockRateKHz;

                this.softwareBus = new SoftwareI2CBus(socket.CpuPins[(int)sclPin], socket.CpuPins[(int)sdaPin]);
            }

            public override void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int numWritten, out int numRead) {
                this.softwareBus.WriteRead((byte)this.Address, writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out numWritten, out numRead);
            }
        }

        #region SDCard
        private SDCardMountedEventHandler onSDCardMounted;
        private SDCardUnmountedEventHandler onSDCardUnmounted;

        /// <summary>Represents the delegate that is used for the Mounted event.</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="device">A storage device that can be used to access the SD card.</param>
        public delegate void SDCardMountedEventHandler(FEZCerbuinoBee sender, GT.StorageDevice device);

        /// <summary>Represents the delegate that is used for the Unmounted event.</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void SDCardUnmountedEventHandler(FEZCerbuinoBee sender, EventArgs e);

        /// <summary>Raised when the file system of the SD card is mounted.</summary>
        public event SDCardMountedEventHandler SDCardMounted;

        /// <summary>Raised when the file system of the SD card is unmounted.</summary>
        public event SDCardUnmountedEventHandler SDCardUnmounted;

        /// <summary>Whether or not an SD card is inserted.</summary>
        public bool IsSDCardInserted {
            get {
                return !this.sdCardDetect.Read();
            }
        }

        /// <summary>Whether or not the SD card is mounted.</summary>
        public bool IsSDCardMounted { get; private set; }

        /// <summary>The StorageDevice for the currently mounted SD card.</summary>
        public GT.StorageDevice SDCardStorageDevice {
            get { return this.sdCardStorageDevice; }
        }

        private void OnSDCardDetect(uint data1, uint data2, DateTime when) {
            Thread.Sleep(500);

            if (this.IsSDCardInserted && !this.IsSDCardMounted)
                this.MountStorageDevice("SD");

            if (!this.IsSDCardInserted && this.IsSDCardMounted)
                this.UnmountStorageDevice("SD");
        }

        private void OnSDCardMounted(FEZCerbuinoBee sender, GT.StorageDevice device) {
            if (this.onSDCardMounted == null)
                this.onSDCardMounted = this.OnSDCardMounted;

            if (GT.Program.CheckAndInvoke(this.SDCardMounted, this.onSDCardMounted, sender, device))
                this.SDCardMounted(sender, device);
        }

        private void OnSDCardUnmounted(FEZCerbuinoBee sender, EventArgs e) {
            if (this.onSDCardUnmounted == null)
                this.onSDCardUnmounted = this.OnSDCardUnmounted;

            if (GT.Program.CheckAndInvoke(this.SDCardUnmounted, this.onSDCardUnmounted, sender, e))
                this.SDCardUnmounted(sender, e);
        }

        #endregion SDCard

        #region USBHost
        private MassStorageMountedEventHandler onMassStorageMounted;
        private MassStorageUnmountedEventHandler onMassStorageUnmounted;
        private MouseConnectedEventHandler onMouseConnected;
        private KeyboardConnectedEventHandler onKeyboardConnected;

        /// <summary>Represents the delegate that is used for the MassStorageMounted event.</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="device">A storage device that can be used to access the SD card.</param>
        public delegate void MassStorageMountedEventHandler(FEZCerbuinoBee sender, GT.StorageDevice device);

        /// <summary>Represents the delegate that is used for the MassStorageUnmounted event.</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void MassStorageUnmountedEventHandler(FEZCerbuinoBee sender, EventArgs e);

        /// <summary>Represents the delegate that is used for the MouseConnected event.</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="mouse">The object associated with the event.</param>
        public delegate void MouseConnectedEventHandler(FEZCerbuinoBee sender, Mouse mouse);

        /// <summary>Represents the delegate that is used to handle the KeyboardConnected event.</summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="keyboard">The object associated with the event.</param>
        public delegate void KeyboardConnectedEventHandler(FEZCerbuinoBee sender, Keyboard keyboard);

        /// <summary>Raised when the file system of the mass storage device is mounted.</summary>
        public event MassStorageMountedEventHandler MassStorageMounted;

        /// <summary>Raised when the file system of the mass storage device is unmounted.</summary>
        public event MassStorageUnmountedEventHandler MassStorageUnmounted;

        /// <summary>Raised when a mouse is connected.</summary>
        public event MouseConnectedEventHandler MouseConnected;

        /// <summary>Raised when a keyboard is connected.</summary>
        public event KeyboardConnectedEventHandler KeyboardConnected;

        /// <summary>The current connected keyboard.</summary>
        public Keyboard ConnectedKeyboard {
            get { return this.connectedKeyboard; }
        }

        /// <summary>The current connected mouse.</summary>
        public Mouse ConnectedMouse {
            get { return this.connectedMouse; }
        }

        /// <summary>The StorageDevice for the currently mounted mass storage device.</summary>
        public GT.StorageDevice MassStorageDevice {
            get { return this.massStorageDevice; }
        }

        /// <summary>Whether or not the keyboard is connected.</summary>
        public bool IsKeyboardConnected { get { return this.connectedKeyboard != null; } }

        /// <summary>Whether or not the mouse is connected.</summary>
        public bool IsMouseConnected { get { return this.connectedMouse != null; } }

        /// <summary>Whether or not the mass storage device is connected.</summary>
        public bool IsMassStorageConnected { get; private set; }

        /// <summary>Whether or not the mass storage device is mounted.</summary>
        public bool IsMassStorageMounted { get; private set; }

        private void OnMassStorageMounted(FEZCerbuinoBee sender, GT.StorageDevice device) {
            if (this.onMassStorageMounted == null)
                this.onMassStorageMounted = this.OnMassStorageMounted;

            if (GT.Program.CheckAndInvoke(this.MassStorageMounted, this.onMassStorageMounted, sender, device))
                this.MassStorageMounted(sender, device);
        }

        private void OnMassStorageUnmounted(FEZCerbuinoBee sender, EventArgs e) {
            if (this.onMassStorageUnmounted == null)
                this.onMassStorageUnmounted = this.OnMassStorageUnmounted;

            if (GT.Program.CheckAndInvoke(this.MassStorageUnmounted, this.onMassStorageUnmounted, sender, e))
                this.MassStorageUnmounted(sender, e);
        }

        private void OnMouseConnected(FEZCerbuinoBee sender, Mouse mouse) {
            if (this.onMouseConnected == null)
                this.onMouseConnected = this.OnMouseConnected;

            if (GT.Program.CheckAndInvoke(this.MouseConnected, this.onMouseConnected, sender, mouse))
                this.MouseConnected(sender, mouse);
        }

        private void OnKeyboardConnected(FEZCerbuinoBee sender, Keyboard keyboard) {
            if (this.onKeyboardConnected == null)
                this.onKeyboardConnected = this.OnKeyboardConnected;

            if (GT.Program.CheckAndInvoke(this.KeyboardConnected, this.onKeyboardConnected, sender, keyboard))
                this.KeyboardConnected(sender, keyboard);
        }

        #endregion USBHost
    }
}