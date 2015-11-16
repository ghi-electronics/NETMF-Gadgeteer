using System;
using GHI.OSHW.Hardware;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using FEZCerb_Pins = GHI.Hardware.FEZCerb.Pin;
using GT = Gadgeteer;

namespace GHIElectronics.Gadgeteer {
    /// <summary>
    /// Support class for GHI Electronics FEZCerbuinoNet for Microsoft .NET Gadgeteer
    /// </summary>
    public class FEZCerbuinoNet : GT.Mainboard {
        private bool configSet = false;

        /// <summary>
        /// Instantiates a new FEZCerbuinoNet mainboard
        /// </summary>
        public FEZCerbuinoNet() {
            GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate nativeI2C = new GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate(this.NativeI2CWriteRead);

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
            socket.NativeI2CWriteRead = nativeI2C;

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
            socket.NativeI2CWriteRead = nativeI2C;

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
            socket.AnalogOutput = new FEZCerbuinoNet_AnalogOut(Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0);

            // P
            socket.PWM7 = (Cpu.PWMChannel)14;
            socket.PWM8 = Cpu.PWMChannel.PWM_1;
            socket.PWM9 = (Cpu.PWMChannel)15;

            // Y
            socket.NativeI2CWriteRead = nativeI2C;

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion Socket 3
        }

        private bool NativeI2CWriteRead(GT.Socket socket, GT.Socket.Pin sda, GT.Socket.Pin scl, byte address, byte[] write, int writeOffset, int writeLen, byte[] read, int readOffset, int readLen, out int numWritten, out int numRead) {
            return GHI.OSHW.Hardware.SoftwareI2CBus.DirectI2CWriteRead(socket.CpuPins[(int)scl], socket.CpuPins[(int)sda], 100, address, write, writeOffset, writeLen, read, readOffset, readLen, out numWritten, out numRead);
        }

        private void NativeSPIBitmapPaint(Bitmap bitmap, SPI.Configuration config, int xSrc, int ySrc, int width, int height, GT.Mainboard.BPP bpp) {
            if (bpp != BPP.BPP16_BGR_BE)
                throw new ArgumentException("Invalid BPP");

            if (!this.configSet) {
                Util.SetSpecialDisplayConfig(config, Util.BPP_Type.BPP16_BGR_LE);

                this.configSet = true;
            }

            bitmap.Flush(xSrc, ySrc, width, height);
        }

        private static string[] sdVolumes = new string[] { "SD" };

        /// <summary>
        /// Allows mainboards to support storage device mounting/umounting.  This provides modules with a list of storage device volume names supported by the mainboard. 
        /// </summary>
        public override string[] GetStorageDeviceVolumeNames() {
            return sdVolumes;
        }

        /// <summary>
        /// Functionality provided by mainboard to ummount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
        /// This should result in a Microsoft.SPOT.IO.RemovableMedia.Eject event if successful.
        /// </summary>
        public override bool MountStorageDevice(string volumeName) {
            StorageDev.MountSD();

            return volumeName == "SD";
        }

        /// <summary>
        /// Functionality provided by mainboard to ummount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
        /// This should result in a Microsoft.SPOT.IO.RemovableMedia.Eject event if successful.
        /// </summary>
        public override bool UnmountStorageDevice(string volumeName) {
            StorageDev.UnmountSD();

            return volumeName == "SD";
        }

        /// <summary>
        /// Changes the programming interafces to the one specified
        /// </summary>
        /// <param name="programmingInterface">The programming interface to use</param>
        public override void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface) {
        }

        /// <summary>
        /// This sets the LCD configuration.  If the value GT.Mainboard.LCDConfiguration.HeadlessConfig (=null) is specified, no display support should be active.
        /// If a non-null value is specified but the property LCDControllerEnabled is false, the LCD controller should be disabled if present,
        /// though the Bitmap width/height for WPF should be modified to the Width and Height parameters.  This must reboot if the LCD configuration changes require a reboot.
        /// </summary>
        /// <param name="lcdConfig">The LCD Configuration</param>
        public override void SetLCDConfiguration(GT.Mainboard.LCDConfiguration lcdConfig) {
        }

        /// <summary>
        /// Configures rotation in the LCD controller. This must reboot if performing the LCD rotation requires a reboot.
        /// </summary>
        /// <param name="rotation">The LCD rotation to use</param>
        /// <returns>true if the rotation is supported</returns>
        public override bool SetLCDRotation(GT.Modules.Module.DisplayModule.LCDRotation rotation) {
            return false;
        }

        private const Cpu.Pin DebugLedPin = FEZCerb_Pins.PB2;

        private Microsoft.SPOT.Hardware.OutputPort debugled = new OutputPort(DebugLedPin, false);
        /// <summary>
        /// Turns the debug LED on or off
        /// </summary>
        /// <param name="on">True if the debug LED should be on</param>
        public override void SetDebugLED(bool on) {
            debugled.Write(on);
        }

        /// <summary>
        /// This performs post-initialization tasks for the mainboard.  It is called by Gadgeteer.Program.Run and does not need to be called manually.
        /// </summary>
        public override void PostInit() {
            return;
        }

        /// <summary>
        /// The mainboard name, which is printed at startup in the debug window
        /// </summary>
        public override string MainboardName {
            get { return "GHI Electronics FEZCerbuinoNet"; }
        }

        /// <summary>
        /// The mainboard version, which is printed at startup in the debug window
        /// </summary>
        public override string MainboardVersion {
            get { return "1.0"; }
        }

        private void BitmapConverter(byte[] bitmapBytes, byte[] pixelBytes, GT.Mainboard.BPP bpp) {
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE)
                throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

            GHI.OSHW.Hardware.Util.BitmapConvertBPP(bitmapBytes, pixelBytes, Util.BPP_Type.BPP16_BGR_BE);
        }
    }

    internal class FEZCerbuinoNet_AnalogOut : GT.Socket.SocketInterfaces.AnalogOutput {
        private AnalogOutput aout = null;

        Cpu.AnalogOutputChannel pin;
        const double MIN_VOLTAGE = 0;
        const double MAX_VOLTAGE = 3.3;

        public FEZCerbuinoNet_AnalogOut(Cpu.AnalogOutputChannel pin) {
            this.pin = pin;
        }

        public double MinOutputVoltage {
            get {
                return FEZCerbuinoNet_AnalogOut.MIN_VOLTAGE;
            }
        }

        public double MaxOutputVoltage {
            get {
                return FEZCerbuinoNet_AnalogOut.MAX_VOLTAGE;
            }
        }

        public bool Active {
            get {
                return this.aout != null;
            }
            set {
                if (value == this.Active)
                    return;

                if (value) {
                    this.aout = new AnalogOutput(this.pin, 1 / FEZCerbuinoNet_AnalogOut.MAX_VOLTAGE, 0, 10);
                    this.SetVoltage(FEZCerbuinoNet_AnalogOut.MIN_VOLTAGE);
                }
                else {
                    this.aout.Dispose();
                    this.aout = null;
                }
            }
        }

        public void SetVoltage(double voltage) {
            this.Active = true;

            if (voltage < FEZCerbuinoNet_AnalogOut.MIN_VOLTAGE)
                throw new ArgumentOutOfRangeException("The minimum voltage of the analog output interface is " + FEZCerbuinoNet_AnalogOut.MIN_VOLTAGE.ToString() + "V");

            if (voltage > FEZCerbuinoNet_AnalogOut.MAX_VOLTAGE)
                throw new ArgumentOutOfRangeException("The maximum voltage of the analog output interface is " + FEZCerbuinoNet_AnalogOut.MAX_VOLTAGE.ToString() + "V");

            this.aout.Write(voltage);
        }
    }
}
