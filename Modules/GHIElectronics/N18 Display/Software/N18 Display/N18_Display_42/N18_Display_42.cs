using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

using System.Threading;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2C), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// A N18 Display module for Microsoft .NET Gadgeteer
    /// </summary>
    public class N18_Display : GTM.Module
    {
        private GTI.SPI _spi;
        private GTI.SPI.Configuration _spiConfig;
        private GT.Socket _socket;
        private GTI.DigitalOutput _resetPin;
        private GTI.DigitalOutput _backlightPin;
        private GTI.DigitalOutput _rs;

        /// <summary>Constructor</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public N18_Display(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('S', this);

            _socket = socket;

            _resetPin = new GTI.DigitalOutput(socket, Socket.Pin.Three, false, this);
            _backlightPin = new GTI.DigitalOutput(socket, Socket.Pin.Four, true, this);
            _rs = new GTI.DigitalOutput(socket, Socket.Pin.Five, false, this);
        }

        /// <summary>
        /// Initializes the module to use the passed in SPI clock rate in KHz.
        /// </summary>
        /// <param name="spiClockRateKHz">SPI clock rate in KHz.</param>
        public void Initialize(uint spiClockRateKHz)
        {
            _spiConfig = new GTI.SPI.Configuration(false, 0, 0, false, true, spiClockRateKHz);
            _spi = new GTI.SPI(_socket, _spiConfig, GTI.SPI.Sharing.Shared, this);

            Reset();

            WriteCommand(0x11);//Sleep exit 
            Thread.Sleep(120);

            //ST7735R Frame Rate
            WriteCommand(0xB1);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteCommand(0xB2);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteCommand(0xB3);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);
            WriteData(0x01); WriteData(0x2C); WriteData(0x2D);

            WriteCommand(0xB4); //Column inversion 
            WriteData(0x07);

            //ST7735R Power Sequence
            WriteCommand(0xC0);
            WriteData(0xA2); WriteData(0x02); WriteData(0x84);
            WriteCommand(0xC1); WriteData(0xC5);
            WriteCommand(0xC2);
            WriteData(0x0A); WriteData(0x00);
            WriteCommand(0xC3);
            WriteData(0x8A); WriteData(0x2A);
            WriteCommand(0xC4);
            WriteData(0x8A); WriteData(0xEE);

            WriteCommand(0xC5); //VCOM 
            WriteData(0x0E);

            WriteCommand(0x36); //MX, MY, RGB mode 
            WriteData(0xC8);

            //ST7735R Gamma Sequence
            WriteCommand(0xe0);
            WriteData(0x0f); WriteData(0x1a);
            WriteData(0x0f); WriteData(0x18);
            WriteData(0x2f); WriteData(0x28);
            WriteData(0x20); WriteData(0x22);
            WriteData(0x1f); WriteData(0x1b);
            WriteData(0x23); WriteData(0x37); WriteData(0x00);

            WriteData(0x07);
            WriteData(0x02); WriteData(0x10);
            WriteCommand(0xe1);
            WriteData(0x0f); WriteData(0x1b);
            WriteData(0x0f); WriteData(0x17);
            WriteData(0x33); WriteData(0x2c);
            WriteData(0x29); WriteData(0x2e);
            WriteData(0x30); WriteData(0x30);
            WriteData(0x39); WriteData(0x3f);
            WriteData(0x00); WriteData(0x07);
            WriteData(0x03); WriteData(0x10);

            WriteCommand(0x2a);
            WriteData(0x00); WriteData(0x00);
            WriteData(0x00); WriteData(0x7f);
            WriteCommand(0x2b);
            WriteData(0x00); WriteData(0x00);
            WriteData(0x00); WriteData(0x9f);

            WriteCommand(0xF0); //Enable test command  
            WriteData(0x01);
            WriteCommand(0xF6); //Disable ram power save mode 
            WriteData(0x00);

            WriteCommand(0x3A); //65k mode 
            WriteData(0x05);


            WriteCommand(0x29);//Display on
            Thread.Sleep(500);
        }

        public void Reset()
        {
            _resetPin.Write(false);
            Thread.Sleep(300);
            _resetPin.Write(true);
            Thread.Sleep(500);
        }

        private void WriteCommand(byte command)
        {
            _rs.Write(false);
            _spi.Write(new byte[] {command});
        }

        private void WriteData(byte data)
        {
            _rs.Write(true);
            _spi.Write(new byte[] { data });
        }

        private void DataWrite(byte[] data)
        {
            _rs.Write(true);
            _spi.Write(data);
        }

        public void SetClippingArea(int x, int y, int w, int h)
        {
            ushort x_end = (ushort)(x + w);
            ushort y_end = (ushort)(y + h);
            WriteCommand(0x2A);
            WriteData((byte)((x >> 8) & 0xFF));
            WriteData((byte)(x & 0xFF));
            WriteData((byte)((x_end >> 8) & 0xFF));
            WriteData((byte)(x_end & 0xFF));
            WriteCommand(0x2B);
            WriteData((byte)((y >> 8) & 0xFF));
            WriteData((byte)(y & 0xFF));
            WriteData((byte)((y_end >> 8) & 0xFF));
            WriteData((byte)(y_end & 0xFF));
        }

        public void DrawImage(byte[] data)
        {
            WriteCommand(0x2C);
            DataWrite(data);
        }

        public void EnableBacklight(bool bOn)
        {
            _backlightPin.Write(bOn);
        }
    }
}
