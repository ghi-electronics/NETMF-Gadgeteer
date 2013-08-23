using System;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Gadgeteer.Modules.GHIElectronics
{

    /// <summary>
    /// A Display N18 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class Display_N18 : GTM.Module.DisplayModule
    {
        private GTI.SPI spi;
        private GTI.SPI.Configuration spiConfig;
		private SPI.Configuration netMFSpiConfig;
		private GT.Socket socket;
        private GTI.DigitalOutput resetPin;
        private GTI.DigitalOutput backlightPin;
		private GTI.DigitalOutput rs;

		private byte[] byteArray;
		private ushort[] shortArray;

		/// <summary>
		/// Gets the width of the display.
		/// </summary>
		/// <remarks>
		/// This property always returns 128.
		/// </remarks>
		public override uint Width { get { return 128; } }

		/// <summary>
		/// Gets the height of the display.
		/// </summary>
		/// <remarks>
		/// This property always returns 160.
		/// </remarks>
		public override uint Height { get { return 160; } }

        /// <summary>Constructor</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public Display_N18(int socketNumber) : base(WPFRenderOptions.Intercept)
        {
			this.byteArray = new byte[1];
			this.shortArray = new ushort[2];

            this.socket = Socket.GetSocket(socketNumber, true, this, null);
            this.socket.EnsureTypeIsSupported('S', this);

			this.resetPin = new GTI.DigitalOutput(this.socket, Socket.Pin.Three, false, this);
			this.backlightPin = new GTI.DigitalOutput(this.socket, Socket.Pin.Four, false, this);
			this.rs = new GTI.DigitalOutput(this.socket, Socket.Pin.Five, false, this);

			this.spiConfig = new GTI.SPI.Configuration(false, 0, 0, false, true, 12000);
			this.netMFSpiConfig = new SPI.Configuration(this.socket.CpuPins[6], this.spiConfig.ChipSelectActiveState, this.spiConfig.ChipSelectSetupTime, this.spiConfig.ChipSelectHoldTime, this.spiConfig.ClockIdleState, this.spiConfig.ClockEdge, this.spiConfig.ClockRateKHz, this.socket.SPIModule);
			this.spi = new GTI.SPI(this.socket, this.spiConfig, GTI.SPI.Sharing.Shared, this.socket, Socket.Pin.Six, this);

			this.Reset();

			this.ConfigureDisplay();

			this.Clear();

			this.SetBacklight(true);
		}

		/// <summary>
		/// Enables or disables the display backlight.
		/// </summary>
		/// <param name="state">The state to set the backlight to.</param>
		public void SetBacklight(bool state)
		{
			this.backlightPin.Write(state);
		}

		/// <summary>
		/// Clears the display.
		/// </summary>
		public void Clear()
		{
			byte[] data = new byte[64 * 80 * 2]; //zero-init'd by default

			this.DrawRaw(data, 64, 80, 0, 0);
			this.DrawRaw(data, 64, 80, 64, 0);
			this.DrawRaw(data, 64, 80, 0, 80);
			this.DrawRaw(data, 64, 80, 64, 80);
		}

		/// <summary>
		/// Draws an image to the screen.
		/// </summary>
		/// <param name="bmp">The bitmap to be drawn to the screen</param>
		/// <param name="x">Starting X position of the image.</param>
		/// <param name="y">Starting Y position of the image.</param>
		public void Draw(Bitmap bmp, uint x = 0, uint y = 0)
		{
			byte[] vram = new byte[bmp.Width * bmp.Height * 2];
			GTM.Module.Mainboard.NativeBitmapConverter(bmp.GetBitmap(), vram, Mainboard.BPP.BPP16_BGR_BE);
			this.DrawRaw(vram, (uint)bmp.Width, (uint)bmp.Height, x, y);
		}

		/// <summary>
		/// Draws an image to the specified position on the screen.
		/// </summary>
		/// <param name="rawData">Raw Bitmap data to be drawn to the screen.</param>
		/// <param name="x">Starting X position of the image.</param>
		/// <param name="y">Starting Y position of the image.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		public void DrawRaw(byte[] rawData, uint width, uint height, uint x, uint y)
		{
			if (x > this.Width || y > this.Height)
				return;

			if (x + width > this.Width)
				width = this.Width - x;
			if (y + height > this.Height)
				height = this.Height - y;

			this.SetClippingArea(x, y, width - 1, height - 1);
			this.WriteCommand(0x2C);
			this.WriteData(rawData);
		}

		/// <summary>
		/// Renders Bitmap data on the display device. 
		/// </summary>
		/// <param name="bitmap">The <see cref="T:Microsoft.SPOT.Bitmap"/> object to render on the display.</param>
		protected override void Paint(Bitmap bitmap)
		{
			try
			{
				if (Mainboard.NativeBitmapCopyToSpi != null)
				{
					this.SetClippingArea(0, 0, (uint)bitmap.Width - 1, (uint)bitmap.Height - 1);
					this.WriteCommand(0x2C);
					this.rs.Write(true);
					Mainboard.NativeBitmapCopyToSpi(bitmap, this.netMFSpiConfig, 0, 0, bitmap.Width, bitmap.Height, GT.Mainboard.BPP.BPP16_BGR_BE);
				}
				else
				{
					this.Draw(bitmap);
				}
			}
			catch
			{
				this.ErrorPrint("Painting error");
			}
		}

		private void Reset()
		{
			this.resetPin.Write(false);
			Thread.Sleep(150);
			this.resetPin.Write(true);
		}

        private void ConfigureDisplay()
		{
			Mainboard.LCDConfiguration lcdConfig = new Mainboard.LCDConfiguration();

			lcdConfig.LCDControllerEnabled = false;
			lcdConfig.Width = Width;
			lcdConfig.Height = Height;

			DisplayModule.SetLCDConfig(lcdConfig);
	
			this.WriteCommand(0x11);//Sleep exit 
            Thread.Sleep(120);

            //ST7735R Frame Rate
            this.WriteCommand(0xB1);
            this.WriteData(0x01); this.WriteData(0x2C); this.WriteData(0x2D);
            this.WriteCommand(0xB2);
            this.WriteData(0x01); this.WriteData(0x2C); this.WriteData(0x2D);
            this.WriteCommand(0xB3);
            this.WriteData(0x01); this.WriteData(0x2C); this.WriteData(0x2D);
            this.WriteData(0x01); this.WriteData(0x2C); this.WriteData(0x2D);

            this.WriteCommand(0xB4); //Column inversion 
            this.WriteData(0x07);

            //ST7735R Power Sequence
            this.WriteCommand(0xC0);
            this.WriteData(0xA2); this.WriteData(0x02); this.WriteData(0x84);
            this.WriteCommand(0xC1); this.WriteData(0xC5);
            this.WriteCommand(0xC2);
            this.WriteData(0x0A); this.WriteData(0x00);
            this.WriteCommand(0xC3);
            this.WriteData(0x8A); this.WriteData(0x2A);
            this.WriteCommand(0xC4);
            this.WriteData(0x8A); this.WriteData(0xEE);

            this.WriteCommand(0xC5); //VCOM 
            this.WriteData(0x0E);

            this.WriteCommand(0x36); //MX, MY, RGB mode 
            this.WriteData(0xC8);

            //ST7735R Gamma Sequence
            this.WriteCommand(0xe0);
            this.WriteData(0x0f); this.WriteData(0x1a);
            this.WriteData(0x0f); this.WriteData(0x18);
            this.WriteData(0x2f); this.WriteData(0x28);
            this.WriteData(0x20); this.WriteData(0x22);
            this.WriteData(0x1f); this.WriteData(0x1b);
            this.WriteData(0x23); this.WriteData(0x37); this.WriteData(0x00);

            this.WriteData(0x07);
            this.WriteData(0x02); this.WriteData(0x10);
            this.WriteCommand(0xe1);
            this.WriteData(0x0f); this.WriteData(0x1b);
            this.WriteData(0x0f); this.WriteData(0x17);
            this.WriteData(0x33); this.WriteData(0x2c);
            this.WriteData(0x29); this.WriteData(0x2e);
            this.WriteData(0x30); this.WriteData(0x30);
            this.WriteData(0x39); this.WriteData(0x3f);
            this.WriteData(0x00); this.WriteData(0x07);
            this.WriteData(0x03); this.WriteData(0x10);

            this.WriteCommand(0x2a);
            this.WriteData(0x00); this.WriteData(0x00);
            this.WriteData(0x00); this.WriteData(0x7f);
            this.WriteCommand(0x2b);
            this.WriteData(0x00); this.WriteData(0x00);
            this.WriteData(0x00); this.WriteData(0x9f);

            this.WriteCommand(0xF0); //Enable test command  
            this.WriteData(0x01);
            this.WriteCommand(0xF6); //Disable ram power save mode 
            this.WriteData(0x00);

            this.WriteCommand(0x3A); //65k mode 
            this.WriteData(0x05);

			this.WriteCommand(0x29);//Display on
        }

		private void SetClippingArea(uint x, uint y, uint w, uint h)
		{
			this.shortArray[0] = (ushort)x;
			this.shortArray[1] = (ushort)(x + w);
			this.WriteCommand(0x2A);
			this.WriteData(this.shortArray);

			this.shortArray[0] = (ushort)y;
			this.shortArray[1] = (ushort)(y + h);
			this.WriteCommand(0x2B);
			this.WriteData(this.shortArray);
		}
		
        private void WriteCommand(byte command)
        {
			this.byteArray[0] = command;

			this.rs.Write(false);
			this.spi.Write(this.byteArray);
        }

        private void WriteData(byte data)
		{
			this.byteArray[0] = data;
			this.WriteData(this.byteArray);
        }

		private void WriteData(byte[] data)
        {
			this.rs.Write(true);
			this.spi.Write(data);
        }

		private void WriteData(ushort[] data)
		{
			this.rs.Write(true);
			this.spi.Write(data);
		}
    }
}