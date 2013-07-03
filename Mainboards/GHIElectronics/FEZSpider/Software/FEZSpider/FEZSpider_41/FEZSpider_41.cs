using System;

using GT = Gadgeteer;
using Microsoft.SPOT.Hardware;

using EMX = GHIElectronics.NETMF.Hardware.EMX;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.System;
using GHIElectronics.NETMF.IO;

namespace GHIElectronics.Gadgeteer
{
    /// <summary>
    /// Support class for GHI Electronics FEZSpider Mainboard for Microsoft .NET Gadgeteer
    /// </summary>
    public class FEZSpider : GT.Mainboard
    {
        bool NativeI2CWriteRead(GT.Socket socket, GT.Socket.Pin sda, GT.Socket.Pin scl, byte address, byte[] write, int writeOffset, int writeLen, byte[] read, int readOffset, int readLen, out int numWritten, out int numRead)
        {
            return SoftwareI2CBus.DirectI2CWriteRead(socket.CpuPins[(int)scl], socket.CpuPins[(int)sda], 100, address, write, writeOffset, writeLen, read, readOffset, readLen, out numWritten, out numRead);
        }

        // docs auto-generated for constructor
        /// <summary>
        /// </summary>
        public FEZSpider()
        {
            this.NativeBitmapConverter = new BitmapConvertBPP(BitmapConverter);

            /////////////////////////////////////////////////////////////////
            // NEW STUFF GOES HERE
            /////////////////////////////////////////////////////////////////
            this.GetStorageDeviceVolumeNames = new GetStorageDeviceVolumeNamesDelegate(_GetStorageDeviceVolumeNames);
            this.MountStorageDevice = new StorageDeviceDelegate(_MountStorageDevice);
            this.UnmountStorageDevice = new StorageDeviceDelegate(_UnmountStorageDevice);
            /////////////////////////////////////////////////////////////////

            GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate nativeI2C = new GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate(NativeI2CWriteRead);

            #region Socket Setup
            // Create sockets.  Use the same socket variable to avoid copy-paste errors that often happen if we use socket1, socket2, etc.
            GT.Socket socket;

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
            socket.SupportedTypes = new char[] { 'D', 'I' };
            socket.CpuPins[3] = EMX.Pin.IO21;
            socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_DM;
            socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_DP;
            socket.CpuPins[6] = EMX.Pin.IO19;
            socket.CpuPins[7] = EMX.Pin.IO75;
            socket.CpuPins[8] = EMX.Pin.IO12;
            socket.CpuPins[9] = EMX.Pin.IO11;
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
            socket.CpuPins[3] = EMX.Pin.IO1;
            socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBH_DM;
            socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBH_DP;
            socket.CpuPins[6] = EMX.Pin.IO0;
            socket.CpuPins[8] = EMX.Pin.IO12;
            socket.CpuPins[9] = EMX.Pin.IO11;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
            socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
            socket.CpuPins[3] = EMX.Pin.IO33;
            socket.CpuPins[4] = EMX.Pin.IO37;
            socket.CpuPins[5] = EMX.Pin.IO32;
            socket.CpuPins[6] = EMX.Pin.IO31;
            socket.CpuPins[7] = EMX.Pin.IO34;
            socket.CpuPins[8] = EMX.Pin.IO12;
            socket.CpuPins[9] = EMX.Pin.IO11;
            socket.NativeI2CWriteRead = nativeI2C;
            socket.SerialPortName = "COM2";
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
            socket.SupportedTypes = new char[] { 'F', 'Y' };
            socket.CpuPins[3] = EMX.Pin.IO23;
            socket.CpuPins[4] = EMX.Pin.IO43;
            socket.CpuPins[5] = EMX.Pin.IO41;
            socket.CpuPins[6] = EMX.Pin.IO44;
            socket.CpuPins[7] = EMX.Pin.IO40;
            socket.CpuPins[8] = EMX.Pin.IO39;
            socket.CpuPins[9] = EMX.Pin.IO42;
            socket.NativeI2CWriteRead = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
            socket.SupportedTypes = new char[] { 'C', 'S', 'Y' };
            socket.CpuPins[3] = EMX.Pin.IO18;
            socket.CpuPins[4] = EMX.Pin.IO20;
            socket.CpuPins[5] = EMX.Pin.IO22;
            socket.CpuPins[6] = EMX.Pin.IO10;
            socket.CpuPins[7] = EMX.Pin.IO36;
            socket.CpuPins[8] = EMX.Pin.IO38;
            socket.CpuPins[9] = EMX.Pin.IO35;
            socket.NativeI2CWriteRead = nativeI2C;
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
            socket.CpuPins[3] = EMX.Pin.IO30;
            socket.CpuPins[4] = EMX.Pin.IO29;
            socket.CpuPins[5] = EMX.Pin.IO28;
            socket.CpuPins[6] = EMX.Pin.IO16;
            socket.CpuPins[7] = EMX.Pin.IO74;
            socket.CpuPins[8] = EMX.Pin.IO48;
            socket.CpuPins[9] = EMX.Pin.IO49;
            socket.NativeI2CWriteRead = nativeI2C;
            socket.SerialPortName = "COM3";
            socket.PWM7 = new EMX_PWM(PWM.Pin.PWM5);
            socket.PWM8 = new EMX_PWM(PWM.Pin.PWM4);
            socket.PWM9 = new EMX_PWM(PWM.Pin.PWM3);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
            socket.SupportedTypes = new char[] { 'A', 'O', 'S', 'U', 'Y' };
            socket.CpuPins[3] = EMX.Pin.IO46;
            socket.CpuPins[4] = EMX.Pin.IO6;
            socket.CpuPins[5] = EMX.Pin.IO7;
            socket.CpuPins[6] = EMX.Pin.IO15;
            socket.CpuPins[7] = EMX.Pin.IO24;
            socket.CpuPins[8] = EMX.Pin.IO25;
            socket.CpuPins[9] = EMX.Pin.IO27;
            socket.NativeI2CWriteRead = nativeI2C;
            socket.SerialPortName = "COM4";
            socket.SPIModule = SPI.SPI_module.SPI1;
            socket.AnalogOutput = new EMX_AnalogOut(AnalogOut.Pin.Aout0);
            socket.AnalogInput3 = new EMX_AIN(AnalogIn.Pin.Ain7);
            socket.AnalogInput4 = new EMX_AIN(AnalogIn.Pin.Ain2);
            socket.AnalogInput5 = new EMX_AIN(AnalogIn.Pin.Ain3);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
            socket.SupportedTypes = new char[] { 'A', 'I', 'T', 'X' };
            socket.CpuPins[3] = EMX.Pin.IO45;
            socket.CpuPins[4] = EMX.Pin.IO5;
            socket.CpuPins[5] = EMX.Pin.IO8;
            socket.CpuPins[6] = EMX.Pin.IO73;
            socket.CpuPins[7] = EMX.Pin.IO72;
            socket.CpuPins[8] = EMX.Pin.IO12;
            socket.CpuPins[9] = EMX.Pin.IO11;
            socket.NativeI2CWriteRead = nativeI2C;
            socket.AnalogInput3 = new EMX_AIN(AnalogIn.Pin.Ain6);
            socket.AnalogInput4 = new EMX_AIN(AnalogIn.Pin.Ain1);
            socket.AnalogInput5 = new EMX_AIN(AnalogIn.Pin.Ain0);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
            socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
            socket.CpuPins[3] = EMX.Pin.IO26;
            socket.CpuPins[4] = EMX.Pin.IO3;
            socket.CpuPins[5] = EMX.Pin.IO2;
            socket.CpuPins[6] = EMX.Pin.IO9;
            socket.CpuPins[7] = EMX.Pin.IO14;
            socket.CpuPins[8] = EMX.Pin.IO13;
            socket.CpuPins[9] = EMX.Pin.IO50;
            socket.NativeI2CWriteRead = nativeI2C;
            socket.SerialPortName = "COM1";
            socket.PWM7 = new EMX_PWM(PWM.Pin.PWM1);
            socket.PWM8 = new EMX_PWM(PWM.Pin.PWM0);
            socket.PWM9 = new EMX_PWM(PWM.Pin.PWM2);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
            socket.SupportedTypes = new char[] { 'B', 'Y' };
            socket.CpuPins[3] = EMX.Pin.IO70;
            socket.CpuPins[4] = EMX.Pin.IO57;
            socket.CpuPins[5] = EMX.Pin.IO58;
            socket.CpuPins[6] = EMX.Pin.IO59;
            socket.CpuPins[7] = EMX.Pin.IO60;
            socket.CpuPins[8] = EMX.Pin.IO63;
            socket.CpuPins[9] = EMX.Pin.IO61;
            socket.NativeI2CWriteRead = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(13);
            socket.SupportedTypes = new char[] { 'G' };
            socket.CpuPins[3] = EMX.Pin.IO51;
            socket.CpuPins[4] = EMX.Pin.IO52;
            socket.CpuPins[5] = EMX.Pin.IO53;
            socket.CpuPins[6] = EMX.Pin.IO54;
            socket.CpuPins[7] = EMX.Pin.IO55;
            socket.CpuPins[8] = EMX.Pin.IO56;
            socket.CpuPins[9] = EMX.Pin.IO17;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(14);
            socket.SupportedTypes = new char[] { 'R', 'Y' };
            socket.CpuPins[3] = EMX.Pin.IO69;
            socket.CpuPins[4] = EMX.Pin.IO65;
            socket.CpuPins[5] = EMX.Pin.IO66;
            socket.CpuPins[6] = EMX.Pin.IO67;
            socket.CpuPins[7] = EMX.Pin.IO68;
            socket.CpuPins[8] = EMX.Pin.IO62;
            socket.CpuPins[9] = EMX.Pin.IO64;
            socket.NativeI2CWriteRead = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion
        }

        private PersistentStorage _storage;

        string[] _GetStorageDeviceVolumeNames()
        {
            return new string[] { "SD", "USB Mass Storage" };
        }

        void _MountStorageDevice(string volumeName)
        {
            _storage = new PersistentStorage(volumeName);
            _storage.MountFileSystem();

            //make sure to send event
        }

        void _UnmountStorageDevice(string volumeName)
        {
            _storage.UnmountFileSystem();
            _storage.Dispose();

            //make sure to send event
        }

        void BitmapConverter(byte[] bitmapBytes, byte[] pixelBytes, GT.Mainboard.BPP bpp)
        {
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE) throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_LE supported");
            Util.BitmapConvertBPP(bitmapBytes, pixelBytes, Util.BPP_Type.BPP16_BGR_BE);
        }

        /// <summary>
        /// Changes the programming interafces to the one specified
        /// </summary>
        /// <param name="programmingInterface">The programming interface to use</param>
        override public void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface)
        {
            // not supported
            return;
        }

        /// <summary>
        /// This sets the LCD configuration.  If the value GT.Mainboard.LCDConfiguration.HeadlessConfig (=null) is specified, no display support should be active.
        /// If a non-null value is specified but the property LCDControllerEnabled is false, the LCD controller should be disabled if present,
        /// though the Bitmap width/height for WPF should be modified to the Width and Height parameters.
        /// </summary>
        /// <param name="lcdConfig">The LCD Configuration</param>
        override public void SetLCD(GT.Mainboard.LCDConfiguration lcdConfig)
        {
            if (lcdConfig.LCDControllerEnabled == false)
            {
                //Configuration.LCD.Set(Configuration.LCD.HeadlessConfig);
                var config = new Configuration.LCD.Configurations();
                config.Width = lcdConfig.Width;
                config.Height = lcdConfig.Height;
                config.PixelClockDivider = 0xFF;
                Configuration.LCD.Set(config);
            }
            else
            {
                var config = new Configuration.LCD.Configurations();

                //if (lcdConfig.LCDControllerEnabled == false)
                //{
                //    // EMX firmware has PixelClockDivider 0xFF as special value meaning "don't run"
                //    config.PixelClockDivider = 0xff;
                //}

                config.Height = lcdConfig.Height;
                config.HorizontalBackPorch = lcdConfig.HorizontalBackPorch;
                config.HorizontalFrontPorch = lcdConfig.HorizontalFrontPorch;
                config.HorizontalSyncPolarity = lcdConfig.HorizontalSyncPolarity;
                config.HorizontalSyncPulseWidth = lcdConfig.HorizontalSyncPulseWidth;
                config.OutputEnableIsFixed = lcdConfig.OutputEnableIsFixed;
                config.OutputEnablePolarity = lcdConfig.OutputEnablePolarity;
                config.PixelClockDivider = lcdConfig.PixelClockDivider;
                config.PixelPolarity = lcdConfig.PixelPolarity;
                config.PriorityEnable = lcdConfig.PriorityEnable;
                config.VerticalBackPorch = lcdConfig.VerticalBackPorch;
                config.VerticalFrontPorch = lcdConfig.VerticalFrontPorch;
                config.VerticalSyncPolarity = lcdConfig.VerticalSyncPolarity;
                config.VerticalSyncPulseWidth = lcdConfig.VerticalSyncPulseWidth;
                config.Width = lcdConfig.Width;

                Configuration.LCD.Set(config);
            }
        }

        private Microsoft.SPOT.Hardware.OutputPort debugled = new OutputPort(EMX.Pin.IO47, false);

        /// <summary>
        /// Turns the debug LED on or off
        /// </summary>
        /// <param name="on">True if the debug LED should be on</param>
        override public void SetDebugLED(bool on)
        {
            debugled.Write(on);
        }

        /// <summary>
        /// This performs post-initialization tasks for the mainboard.  It is called by Gadgeteer.Program.Run and does not need to be called manually.
        /// </summary>
        override public void PostInit()
        {
            return;
        }

        /// <summary>
        /// The mainboard name, which is printed at startup in the debug window
        /// </summary>
        override public string MainboardName
        {
            get { return "GHIElectronics-FEZSpider"; }
        }

        /// <summary>
        /// The mainboard version, which is printed at startup in the debug window
        /// </summary>
        override public string MainboardVersion
        {
            get { return "1.0"; }
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


    internal class EMX_PWM : GT.Socket.SocketInterfaces.PWM
    {
        PWM.Pin pin;
        PWM pwm = null;

        public EMX_PWM(PWM.Pin pin)
        {
            this.pin = pin;
        }

        public bool Active
        {
            get
            {
                return pwm != null;
            }
            set
            {
                if (value == Active) return;
                if (value)
                {
                    pwm = new PWM(pin);
                }
                else
                {
                    pwm.Dispose();
                    pwm = null;
                }

            }
        }

        public void Set(int frequency, byte dutyCycle)
        {
            Active = true;
            pwm.Set(frequency, dutyCycle);
        }

        public void SetPulse(uint period_ns, uint highTime_ns)
        {
            Active = true;
            pwm.SetPulse(period_ns, highTime_ns);
        }
    }

    internal class EMX_AnalogOut : GT.Socket.SocketInterfaces.AnalogOutput
    {
        private AnalogOut aout;
        private AnalogOut.Pin pin;
        private readonly double _minVoltage = 0;
        private readonly double _maxVoltage = 3.3;
        private int lastSet = 0;

        public double MinOutputVoltage
        {
            get
            {
                return _minVoltage;
            }
        }

        public double MaxOutputVoltage
        {
            get
            {
                return _maxVoltage;
            }
        }

        public bool Active
        {
            get
            {
                return aout != null;
            }

            set
            {
                if (value == Active) return;
                if (value)
                {
                    aout = new AnalogOut(pin);

                    // This analog output has 10-bit resolution (0-1024) and voltage range (0-3.3V). 
                    // Map minVoltage and maxVoltage to 0-1024 range: Each increment in range represents (3.3/1024) volts.
                    aout.SetLinearScale(0, 1024);
                    aout.Set(lastSet);
                }
                else
                {
                    aout.Dispose();
                    aout = null;
                }
            }
        }

        public EMX_AnalogOut(AnalogOut.Pin pin)
        {
            this.pin = pin;
        }

        public void SetVoltage(double voltage)
        {
            Active = true;

            // Check that voltage does not fall outside of mix/max range
            if (voltage < _minVoltage)
                throw new ArgumentOutOfRangeException("The minimum voltage of the analog output interface is 0.0V");

            if (voltage > _maxVoltage)
                throw new ArgumentOutOfRangeException("The maximum voltage of the analog output interface is 3.3V");

            // Convert voltage to 0-1024 unit scale
            int voltageInUnits = (int)((double)((voltage / _maxVoltage)) * 1024);
            aout.Set(voltageInUnits);
            lastSet = voltageInUnits;
        }
    }

    internal class EMX_AIN : GT.Socket.SocketInterfaces.AnalogInput
    {
        private AnalogIn.Pin pin;
        private AnalogIn ain;

        public EMX_AIN(AnalogIn.Pin pin)
        {
            this.pin = pin;
        }

        #region AnalogInput Members

        public double ReadVoltage()
        {
            Active = true;
            return ((double)ain.Read()) * 3.3 / 1024.0;
        }

        public bool Active
        {
            get
            {
                return ain != null;
            }

            set
            {
                if (Active == value) return;
                if (value)
                {
                    ain = new AnalogIn(pin);
                    ain.SetLinearScale(0, 1024);
                }
                else
                {
                    ain.Dispose();
                    ain = null;
                }
            }
        }

        #endregion
    }
}