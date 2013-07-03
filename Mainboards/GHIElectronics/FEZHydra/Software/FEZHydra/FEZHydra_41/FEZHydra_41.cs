using System;
using Microsoft.SPOT.Hardware;

using GT = Gadgeteer;

using GHIOSH = GHIElectronics.OSH.NETMF.Hardware;

namespace GHIElectronics.Gadgeteer
{
    #region FEZHydra
    /// <summary>
    /// Support class for GHI Electronics FEZHydra for Microsoft .NET Gadgeteer
    /// </summary>
    public class FEZHydra : GT.Mainboard
    {
        // The mainboard constructor gets called before anything else in Gadgeteer (module constructors, etc), 
        // so it can set up fields in Gadgeteer.dll specifying socket types supported, etc.

        /// <summary>
        /// Instantiates a new FEZHydra mainboard
        /// </summary>
        public FEZHydra()
        {
            this.NativeBitmapConverter = new BitmapConvertBPP(BitmapConverter);

            this.GetStorageDeviceVolumeNames = new GetStorageDeviceVolumeNamesDelegate(_GetStorageDeviceVolumeNames);
            this.MountStorageDevice = new StorageDeviceDelegate(_MountStorageDevice);
            this.UnmountStorageDevice = new StorageDeviceDelegate(_UnmountStorageDevice);

            GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate nativeI2C =
                new GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate(NativeI2CWriteRead);

            // Create sockets.  Use the same socket variable to avoid copy-paste errors that often happen if we use socket1, socket2, etc
            GT.Socket socket;

            #region Socket_Setup
            // For each socket on the mainboard, create, configure and register a Socket object with Gadgeteer.dll
            #region Socket1 (Z)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
            socket.SupportedTypes = new char[] { 'Z' };
            socket.CpuPins[3] = (Cpu.Pin)SpecialPurposePin.RESET;
            socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.TCK;
            socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.RTC_BATT;
            socket.CpuPins[6] = (Cpu.Pin)SpecialPurposePin.TDO;
            socket.CpuPins[7] = (Cpu.Pin)SpecialPurposePin.TRST;
            socket.CpuPins[8] = (Cpu.Pin)SpecialPurposePin.TMS;
            socket.CpuPins[9] = (Cpu.Pin)SpecialPurposePin.TDI;

            // Set special properties
            // none

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket2 (D)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
            socket.SupportedTypes = new char[] { 'D' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB19;
            socket.CpuPins[4] = (Cpu.Pin)SpecialPurposePin.USBD_DM;
            socket.CpuPins[5] = (Cpu.Pin)SpecialPurposePin.USBD_DP;
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB18;
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB22;
            //socket.CpuPins[8] = (Cpu.Pin)Unused???;
            //socket.CpuPins[9] = (Cpu.Pin)Unused???;

            // Set special properties
            // none

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket3 (S,Y)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
            socket.SupportedTypes = new char[] { 'S', 'Y' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB8; //PWM0
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB9; //PWM1
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB12;
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB13;
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA26; //MOSI
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA25; //MISO
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA27; //SCK

            // Set special properties
            socket.SPIModule = SPI.SPI_module.SPI1; //S
            socket.NativeI2CWriteRead = nativeI2C; //Y

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket4 (S,U,Y)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
            socket.SupportedTypes = new char[] { 'S', 'U', 'Y' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB2;
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA11; //TXD1
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA12; //RXD1
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB14;
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA26; //MOSI
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA25; //MISO
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA27; //SCK

            // Set special properties
            socket.SPIModule = SPI.SPI_module.SPI1; //S
            socket.SerialPortName = "COM3"; //U
            socket.NativeI2CWriteRead = nativeI2C; //Y

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket5 (I,U,X)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
            socket.SupportedTypes = new char[] { 'I', 'U', 'X' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA9; //RTS0
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA22; //DTXD
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA21; //DRXD
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA10; //CTS0
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.GPIO_NONE; // Unused
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA23; // I2C_SDA
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA24; // I2C_SCL

            // Set special properties
            socket.SerialPortName = "COM1";
            socket.NativeI2CWriteRead = nativeI2C; //X/Y

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket6 (I,K,U,X)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
            socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD17;
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA13; //TXD2
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA14; //RXD2
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA29; //TXD2
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA30; //CTS2
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA23; //I2C_SDA
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA24; //I2C_SCL

            // Set special properties
            socket.NativeI2CWriteRead = nativeI2C;
            socket.SerialPortName = "COM4";

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket7 (P,U,Y)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(7);
            socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD19;
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA6; //TXD0
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA7; //RXD0
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD20;
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD14; //PWM0
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD15; //PWM1
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD16; //PWM2

            // Set special properties
            socket.PWM7 = new FEZHydra_PWM(GHIOSH.PWM.Pin.PWM0); //P
            socket.PWM8 = new FEZHydra_PWM(GHIOSH.PWM.Pin.PWM1); //P
            socket.PWM9 = new FEZHydra_PWM(GHIOSH.PWM.Pin.PWM2); //P
            socket.SerialPortName = "COM2"; //U
            socket.NativeI2CWriteRead = nativeI2C; //Y

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket8 (F,Y)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(8);
            socket.SupportedTypes = new char[] { 'F', 'Y' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD11;
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA0; //MC_DA0
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA3; //MC_DA1
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA1; //MC_CDA
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA4; //MC_DA2
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA5; //MC_DA3
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA2; //MC_CK

            // Set special properties
            socket.NativeI2CWriteRead = nativeI2C; //Y

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket9 (Y)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
            socket.SupportedTypes = new char[] { 'Y' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD9;
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD10;
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD12; //PCK1
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD1; //AC97_FS
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD3; //AC97_TX
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD4; //AC97_RX
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD2; //AC97_CK

            // Set special properties
            socket.NativeI2CWriteRead = nativeI2C; //Y

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket10 (R,Y)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
            socket.SupportedTypes = new char[] { 'R', 'Y' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC22;  //LCD_R0
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC23; //LCD_R1
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC24; //LCD_R2
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC25;  //LCD_R3
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC20;  //LCD_R4
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC4;  //LCD_VSYNC
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC5;  //LCD_HSYNC

            // Set special properties
            socket.NativeI2CWriteRead = nativeI2C; //Y

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket11 (G,Y)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
            socket.SupportedTypes = new char[] { 'G', 'Y' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC15; //LCD_G0
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC16; //LCD_G1
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC17; //LCD_G2
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC18;  //LCD_G3
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC19;  //LCD_G4
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC21;  //LCD_G5
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC3;  //LCD_PWM

            // Set special properties
            socket.NativeI2CWriteRead = nativeI2C; //Y

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket12 (B,Y)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
            socket.SupportedTypes = new char[] { 'B', 'Y' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC9; //LCD_B0
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC10; //LCD_B1
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC11; //LCD_B2
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC12;  //LCD_B3
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC13;  //LCD_B4
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC7;  //LCD_EN
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PC6;  //LCD_CLK

            // Set special properties
            socket.NativeI2CWriteRead = nativeI2C; //Y

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket13 (A,T,Y)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(13);
            socket.SupportedTypes = new char[] { 'A', 'T', 'Y' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD6; //GPAD4
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA20; //AD3YM
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA18; //AD1XM
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB1; //RXD3
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB28;
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB26;
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB29;

            // Set special properties
            socket.AnalogInput3 = new FEZHydra_AIN(GHIOSH.AnalogIn.Pin.Ain4); //A
            socket.AnalogInput4 = new FEZHydra_AIN(GHIOSH.AnalogIn.Pin.Ain3); //A
            socket.AnalogInput5 = new FEZHydra_AIN(GHIOSH.AnalogIn.Pin.Ain1); //A
            socket.NativeI2CWriteRead = nativeI2C; //Y


            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion

            #region Socket14 (A,Y)
            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(14);
            socket.SupportedTypes = new char[] { 'A', 'Y' };
            socket.CpuPins[3] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD7; //GPAD5
            socket.CpuPins[4] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA19; //AD2YP
            socket.CpuPins[5] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PA17; //AD0XP
            socket.CpuPins[6] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB0; //TXD3
            socket.CpuPins[7] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB30;
            socket.CpuPins[8] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB31;
            socket.CpuPins[9] = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PB27;

            // Set special properties
            socket.AnalogInput3 = new FEZHydra_AIN(GHIOSH.AnalogIn.Pin.Ain5); //A
            socket.AnalogInput4 = new FEZHydra_AIN(GHIOSH.AnalogIn.Pin.Ain2); //A
            socket.AnalogInput5 = new FEZHydra_AIN(GHIOSH.AnalogIn.Pin.Ain0); //A
            socket.NativeI2CWriteRead = nativeI2C; //Y

            GT.Socket.SocketInterfaces.RegisterSocket(socket);
            #endregion
            #endregion //Socket_Setup
        }

        //private GHIOSH.StorageDev _storage;

        string[] _GetStorageDeviceVolumeNames()
        {
            return new string[] { "SD" };
        }

        void _MountStorageDevice(string volumeName)
        {
            //_storage = new GHIOSH.StorageDev;
            //_storage.MountFileSystem();
            GHIOSH.StorageDev.MountSD();

            //make sure to send event
        }

        void _UnmountStorageDevice(string volumeName)
        {
            //_storage.UnmountFileSystem();
            //_storage.Dispose();
            GHIOSH.StorageDev.UnmountSD();

            //make sure to send event
        }

        bool NativeI2CWriteRead(GT.Socket socket, GT.Socket.Pin sda, GT.Socket.Pin scl, byte address, byte[] write, int writeOffset, int writeLen, byte[] read, int readOffset, int readLen, out int numWritten, out int numRead)
        {
            //throw new NotImplementedException();

            return GHIOSH.SoftwareI2CBus.DirectI2CWriteRead(socket.CpuPins[(int)scl], socket.CpuPins[(int)sda], 100, address, write, writeOffset, writeLen, read, readOffset, readLen, out numWritten, out numRead);
        }

        /// <summary>
        /// Changes the programming interafce to the one specified
        /// </summary>
        /// <param name="programmingInterface">The programming interface to use</param>
        public override void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface)
        {
            // Change the reflashing interface to the one specified, if possible.
            // This is an advanced API that we don't expect people to call much.
            throw new Exception("Not yet implemented");
        }

        void BitmapConverter(byte[] bitmapBytes, byte[] pixelBytes, GT.Mainboard.BPP bpp)
        {
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE)
                throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_LE supported");

            //Util.BitmapConvertBPP(bitmapBytes, pixelBytes, Util.BPP_Type.BPP16_BGR_BE);

            int bitmapSize = bitmapBytes.Length;

            int x = 0;
            for (int i = 0; i < bitmapSize; i += 4)
            {
                byte R = bitmapBytes[i + 0];
                byte G = bitmapBytes[i + 1];
                byte B = bitmapBytes[i + 2];

                pixelBytes[x] = (byte)((R & 0xE0) | (G >> 5));
                pixelBytes[x + 1] = (byte)(B >> 3);
                x += 2;
            }
        }

        /// <summary>
        /// This sets the LCD configuration.  If the value GT.Mainboard.LCDConfiguration.HeadlessConfig (=null) is specified, no display support should be active.
        /// If a non-null value is specified but the property LCDControllerEnabled is false, the LCD controller should be disabled if present,
        /// though the Bitmap width/height for WPF should be modified to the Width and Height parameters.
        /// </summary>
        /// <param name="lcdConfig">The LCD Configuration</param>
        public override void SetLCD(GT.Mainboard.LCDConfiguration lcdConfig)
        {
            if (lcdConfig.LCDControllerEnabled == false)
            {
                GHIOSH.LCDController.Configurations config = new GHIOSH.LCDController.Configurations();
                config.Width = lcdConfig.Width;
                config.Height = lcdConfig.Height;
                config.PixelClockDivider = 0xFF;
                GHIOSH.LCDController.Set(config);
            }
            else
            {
                GHIOSH.LCDController.Configurations config = new GHIOSH.LCDController.Configurations();

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

                GHIOSH.LCDController.Set(config);
            }
        }

        // change the below to the debug led pin on this mainboard
        private const Cpu.Pin DebugLedPin = (Cpu.Pin)GHIOSH.FEZHydra.Pin.PD18;

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
            get { return "GHIElectronics-FEZHydra"; }
        }

        /// <summary>
        /// The mainboard version, which is printed at startup in the debug window
        /// </summary>
        public override string MainboardVersion
        {
            get { return "1.2"; }
        }

        #region Special Purpose Pins
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
        #endregion
    }
    #endregion

    #region PWM
    internal class FEZHydra_PWM : GT.Socket.SocketInterfaces.PWM
    {
        GHIOSH.PWM.Pin pin;
        GHIOSH.PWM pwm = null;

        public FEZHydra_PWM(GHIOSH.PWM.Pin pin)
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
                    pwm = new GHIOSH.PWM(pin);
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
    #endregion

    //#region Analog Out
    //internal class FEZHydra_AnalogOut : GT.Socket.SocketInterfaces.AnalogOutput
    //{
    //    private AnalogOut aout;
    //    private AnalogOut.Pin pin;
    //    private readonly double _minVoltage = 0;
    //    private readonly double _maxVoltage = 3.3;
    //    private int lastSet = 0;

    //    public FEZHydra_AnalogOut(AnalogOut.Pin pin)
    //    {
    //        this.pin = pin;
    //    }

    //    public double MinOutputVoltage
    //    {
    //        get
    //        {
    //            return _minVoltage;
    //        }
    //    }

    //    public double MaxOutputVoltage
    //    {
    //        get
    //        {
    //            return _maxVoltage;
    //        }
    //    }

    //    public bool Active
    //    {
    //        get
    //        {
    //            return aout != null;
    //        }

    //        set
    //        {
    //            if (value == Active) return;
    //            if (value)
    //            {
    //                aout = new AnalogOut(pin);

    //                // This analog output has 10-bit resolution (0-1024) and voltage range (0-3.3V). 
    //                // Map minVoltage and maxVoltage to 0-1024 range: Each increment in range represents (3.3/1024) volts.
    //                aout.SetLinearScale(0, 1024);
    //                aout.Set(lastSet);
    //            }
    //            else
    //            {
    //                aout.Dispose();
    //                aout = null;
    //            }
    //        }
    //    }

    //    public void SetVoltage(double voltage)
    //    {
    //        Active = true;

    //        // Check that voltage does not fall outside of mix/max range
    //        if (voltage < _minVoltage)
    //            throw new ArgumentOutOfRangeException("The minimum voltage of the analog output interface is 0.0V");

    //        if (voltage > _maxVoltage)
    //            throw new ArgumentOutOfRangeException("The maximum voltage of the analog output interface is 3.3V");

    //        // Convert voltage to 0-1024 unit scale
    //        int voltageInUnits = (int)((double)((voltage / _maxVoltage)) * 1024);
    //        aout.Set(voltageInUnits);
    //        lastSet = voltageInUnits;
    //    }
    //}
    //#endregion

    #region Analog In
    internal class FEZHydra_AIN : GT.Socket.SocketInterfaces.AnalogInput
    {
        private GHIOSH.AnalogIn.Pin pin;
        private GHIOSH.AnalogIn ain;

        public FEZHydra_AIN(GHIOSH.AnalogIn.Pin pin)
        {
            this.pin = pin;
        }

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
                    ain = new GHIOSH.AnalogIn(pin);
                    ain.SetLinearScale(0, 1024);
                }
                else
                {
                    ain.Dispose();
                    ain = null;
                }
            }
        }
    }
    #endregion
}