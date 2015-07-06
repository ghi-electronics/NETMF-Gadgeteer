using GHI.IO;
using GHI.IO.Storage;
using GHI.Processor;
using GHI.Usb;
using GHI.Usb.Host;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using G30 = GHI.Pins.G30;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace GHIElectronics.Gadgeteer {
    /// <summary>
    /// The mainboard class for the FEZMedusaII.
    /// </summary>
    public class FEZMedusaII : GT.Mainboard {
        private bool configSet;
        private InterruptPort ldr0;
        private InterruptPort ldr1;
        private OutputPort debugLed;
        private IRemovable[] storageDevices;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public FEZMedusaII() {
            this.configSet = false;
            this.debugLed = null;
            this.storageDevices = new IRemovable[3];

            this.NativeBitmapConverter = this.NativeBitmapConvert;
            this.NativeBitmapCopyToSpi = this.NativeBitmapSpi;

            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
            GT.Socket socket;

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
            socket.SupportedTypes = new char[] { 'D', 'I' };
            socket.CpuPins[3] = G30.Gpio.PB14;
            socket.CpuPins[4] = GT.Socket.UnnumberedPin;
            socket.CpuPins[5] = GT.Socket.UnnumberedPin;
            socket.CpuPins[6] = G30.Gpio.PB13;
            socket.CpuPins[7] = G30.Gpio.PB15;
            socket.CpuPins[8] = G30.Gpio.PB7;
            socket.CpuPins[9] = G30.Gpio.PB6;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
            socket.SupportedTypes = new char[] { 'F', 'Y' };
            socket.CpuPins[3] = G30.Gpio.PC0;
            socket.CpuPins[4] = G30.Gpio.PC8;
            socket.CpuPins[5] = G30.Gpio.PC9;
            socket.CpuPins[6] = G30.Gpio.PD2;
            socket.CpuPins[7] = G30.Gpio.PC10;
            socket.CpuPins[8] = G30.Gpio.PC11;
            socket.CpuPins[9] = G30.Gpio.PC12;
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
            socket.SupportedTypes = new char[] { 'I', 'U', 'X' };
            socket.CpuPins[3] = G30.Gpio.PC1;
            socket.CpuPins[4] = G30.Gpio.PA9;
            socket.CpuPins[5] = G30.Gpio.PA10;
            socket.CpuPins[6] = G30.Gpio.PC2;
            socket.CpuPins[7] = G30.Gpio.PB10;
            socket.CpuPins[8] = G30.Gpio.PB7;
            socket.CpuPins[9] = G30.Gpio.PB6;
            socket.SerialPortName = G30.SerialPort.Com1;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
            socket.SupportedTypes = new char[] { 'A', 'S', 'Y' };
            socket.CpuPins[3] = G30.Gpio.PA7;
            socket.CpuPins[4] = G30.Gpio.PB0;
            socket.CpuPins[5] = G30.Gpio.PB1;
            socket.CpuPins[6] = G30.Gpio.PB12;
            socket.CpuPins[7] = G30.Gpio.PB5;
            socket.CpuPins[8] = G30.Gpio.PB4;
            socket.CpuPins[9] = G30.Gpio.PB3;
            socket.AnalogInput3 = G30.AnalogInput.PA7;
            socket.AnalogInput4 = G30.AnalogInput.PB0;
            socket.AnalogInput5 = G30.AnalogInput.PB1;
            socket.SPIModule = G30.SpiBus.Spi1;
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, G30.SupportedAnalogInputPrecision);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
            socket.SupportedTypes = new char[] { 'A', 'P', 'Y' };
            socket.CpuPins[3] = G30.Gpio.PA4;
            socket.CpuPins[4] = G30.Gpio.PA5;
            socket.CpuPins[5] = G30.Gpio.PA6;
            socket.CpuPins[6] = G30.Gpio.PB2;
            socket.CpuPins[7] = G30.Gpio.PA8;
            socket.CpuPins[8] = G30.Gpio.PB8;
            socket.CpuPins[9] = G30.Gpio.PB9;
            socket.AnalogInput3 = G30.AnalogInput.PA4;
            socket.AnalogInput4 = G30.AnalogInput.PA5;
            socket.AnalogInput5 = G30.AnalogInput.PA6;
            socket.PWM7 = G30.PwmOutput.PA8;
            socket.PWM8 = G30.PwmOutput.PB8;
            socket.PWM9 = G30.PwmOutput.PB9;
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, G30.SupportedAnalogInputPrecision);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
            socket.SupportedTypes = new char[] { 'A', 'K', 'P', 'U', 'Y' };
            socket.CpuPins[3] = G30.Gpio.PC3;
            socket.CpuPins[4] = G30.Gpio.PA2;
            socket.CpuPins[5] = G30.Gpio.PA3;
            socket.CpuPins[6] = G30.Gpio.PA1;
            socket.CpuPins[7] = G30.Gpio.PA0;
            socket.CpuPins[8] = G30.Gpio.PC6;
            socket.CpuPins[9] = G30.Gpio.PC7;
            socket.AnalogInput3 = G30.AnalogInput.PC3;
            socket.AnalogInput4 = G30.AnalogInput.PA2;
            socket.AnalogInput5 = G30.AnalogInput.PA3;
            socket.PWM7 = G30.PwmOutput.PA0;
            socket.PWM8 = G30.PwmOutput.PC6;
            socket.PWM9 = G30.PwmOutput.PC7;
            socket.SerialPortName = G30.SerialPort.Com2;
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, G30.SupportedAnalogInputPrecision);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);
        }

        /// <summary>
        /// The name of the mainboard.
        /// </summary>
        public override string MainboardName {
            get { return "GHI Electronics FEZ Medusa II"; }
        }

        /// <summary>
        /// The current version of the mainboard hardware.
        /// </summary>
        public override string MainboardVersion {
            get { return "1.0"; }
        }

        /// <summary>The InterruptPort object for LDR0.</summary>
        public InterruptPort LDR0 {
            get {
                if (this.ldr0 == null)
                    this.ldr0 = new InterruptPort(G30.Gpio.PA15, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);

                return this.ldr0;
            }
        }

        /// <summary>The InterruptPort object for LDR1.</summary>
        public InterruptPort LDR1 {
            get {
                if (this.ldr1 == null)
                    this.ldr1 = new InterruptPort(G30.Gpio.PC13, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);

                return this.ldr1;
            }
        }

        /// <summary>
        /// The storage device volume names supported by this mainboard.
        /// </summary>
        /// <returns>The volume names.</returns>
        public override string[] GetStorageDeviceVolumeNames() {
            return new string[] { "SD" };
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
                this.debugLed = new OutputPort(G30.Gpio.PC14, on);

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

            GHI.Utilities.Bitmaps.Convert(bitmap, GHI.Utilities.Bitmaps.BitsPerPixel.BPP16_BGR_BE, pixelBytes);
        }

        private void NativeBitmapSpi(Bitmap bitmap, SPI.Configuration config, int xSrc, int ySrc, int width, int height, GT.Mainboard.BPP bpp) {
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE) throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

            if (!this.configSet) {
                Display.Populate(Display.GHIDisplay.DisplayN18);
                Display.Bpp = GHI.Utilities.Bitmaps.BitsPerPixel.BPP16_BGR_BE;
                Display.ControlPin = Cpu.Pin.GPIO_NONE;
                Display.BacklightPin = Cpu.Pin.GPIO_NONE;
                Display.ResetPin = Cpu.Pin.GPIO_NONE;
                Display.ChipSelectPin = config.ChipSelect_Port;
                Display.SpiModule = config.SPI_mod;

                if ((bitmap.Width == 128 || bitmap.Width == 160) && (bitmap.Height == 128 || bitmap.Height == 160))
                    Display.CurrentRotation = bitmap.Width == 128 ? Display.Rotation.Normal : Display.Rotation.Clockwise90;

                Display.Save();

                this.configSet = true;
            }

            bitmap.Flush(xSrc, ySrc, width, height);
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
    }
}
