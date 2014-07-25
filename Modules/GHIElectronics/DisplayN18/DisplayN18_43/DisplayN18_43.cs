using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A Display N18 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class DisplayN18 : GTM.Module.DisplayModule
    {
        private GTI.Spi spi;
        private GTI.SpiConfiguration spiConfig;
		private SPI.Configuration netMFSpiConfig;
		private GT.Socket socket;
        private GTI.DigitalOutput resetPin;
        private GTI.DigitalOutput backlightPin;
		private GTI.DigitalOutput rsPin;

		private byte[] byteArray;
		private ushort[] shortArray;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public DisplayN18(int socketNumber) : base(WpfMode.Separate)
        {
			this.byteArray = new byte[1];
			this.shortArray = new ushort[2];

            this.socket = Socket.GetSocket(socketNumber, true, this, null);
            this.socket.EnsureTypeIsSupported('S', this);

			this.resetPin = GTI.DigitalOutputFactory.Create(this.socket, Socket.Pin.Three, false, this);
			this.backlightPin = GTI.DigitalOutputFactory.Create(this.socket, Socket.Pin.Four, true, this);
			this.rsPin = GTI.DigitalOutputFactory.Create(this.socket, Socket.Pin.Five, false, this);

			this.spiConfig = new GTI.SpiConfiguration(false, 0, 0, false, true, 12000);
            this.netMFSpiConfig = new SPI.Configuration(this.socket.CpuPins[6], this.spiConfig.IsChipSelectActiveHigh, this.spiConfig.ChipSelectSetupTime, this.spiConfig.ChipSelectHoldTime, this.spiConfig.IsClockIdleHigh, this.spiConfig.IsClockSamplingEdgeRising, this.spiConfig.ClockRateKHz, this.socket.SPIModule);
			this.spi = GTI.SpiFactory.Create(this.socket, this.spiConfig, GTI.SpiSharing.Shared, this.socket, Socket.Pin.Six, this);

			this.Reset();

			this.ConfigureDisplay();

			this.Clear();

            base.OnDisplayConnected("Display N18", 128, 160, DisplayOrientation.Normal, null);
		}

        /// <summary>
        /// Whether or not the backlight is enabled.
        /// </summary>
        public bool BacklightEnabled
        {
            get
            {
                return this.backlightPin.Read();
            }
            set
            {
                this.backlightPin.Write(value);
            }
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
        /// <param name="bitmap">The bitmap to be drawn to the screen</param>
        public void Draw(Bitmap bitmap)
        {
            this.Draw(bitmap, 0, 0);
        }

		/// <summary>
		/// Draws an image to the screen.
		/// </summary>
		/// <param name="bitmap">The bitmap to be drawn to the screen</param>
		/// <param name="x">Starting X position of the image.</param>
		/// <param name="y">Starting Y position of the image.</param>
        public void Draw(Bitmap bitmap, int x, int y)
		{
			byte[] vram = new byte[bitmap.Width * bitmap.Height * 2];
			GTM.Module.Mainboard.NativeBitmapConverter(bitmap, vram, Mainboard.BPP.BPP16_BGR_BE);
            this.DrawRaw(vram, x, y, bitmap.Width, bitmap.Height);
		}

		/// <summary>
		/// Draws an image to the specified position on the screen.
		/// </summary>
		/// <param name="rawData">Raw bitmap data to be drawn to the screen.</param>
		/// <param name="x">Starting x position of the image.</param>
		/// <param name="y">Starting y position of the image.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
        public void DrawRaw(byte[] rawData, int x, int y, int width, int height)
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
        /// Renders display data on the display device. 
        /// </summary>
        /// <param name="bitmap">The bitmap object to render on the display.</param>
        /// <param name="x">The start x coordinate of the dirty area.</param>
        /// <param name="y">The start y coordinate of the dirty area.</param>
        /// <param name="width">The width of the dirty area.</param>
        /// <param name="height">The height of the dirty area.</param>
		protected override void Paint(Bitmap bitmap, int x, int y, int width, int height)
		{
			try
			{
				if (Mainboard.NativeBitmapCopyToSpi != null)
				{
					this.SetClippingArea(0, 0, bitmap.Width - 1, bitmap.Height - 1);
					this.WriteCommand(0x2C);
					this.rsPin.Write(true);
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

        /// <summary>
        /// Sets the orientation.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        protected override void SetOrientationOverride(Module.DisplayModule.DisplayOrientation orientation) 
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Checks if the orientation is supported.
        /// </summary>
        /// <param name="orientation">The orientation.</param>
        protected override bool SupportsOrientationOverride(Module.DisplayModule.DisplayOrientation orientation)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Swaps the red and blue channels if your display has them reversed.
        /// </summary>
        public void SwapRedBlueChannels()
        {
            this.WriteCommand(0x36); //MX, MY, RGB mode 
            this.WriteData(0xC0);
        }

		private void Reset()
		{
			this.resetPin.Write(false);
			Thread.Sleep(150);
			this.resetPin.Write(true);
		}

        private void ConfigureDisplay()
		{	
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

			this.WriteCommand(0x29); //Display on
        }

        private void SetClippingArea(int x, int y, int width, int height)
		{
			this.shortArray[0] = (ushort)x;
			this.shortArray[1] = (ushort)(x + width);
			this.WriteCommand(0x2A);
			this.WriteData(this.shortArray);

			this.shortArray[0] = (ushort)y;
			this.shortArray[1] = (ushort)(y + height);
			this.WriteCommand(0x2B);
			this.WriteData(this.shortArray);
		}
		
        private void WriteCommand(byte command)
        {
			this.byteArray[0] = command;

			this.rsPin.Write(false);
			this.spi.Write(this.byteArray);
        }

        private void WriteData(byte data)
		{
			this.byteArray[0] = data;
			this.WriteData(this.byteArray);
        }

		private void WriteData(byte[] data)
        {
			this.rsPin.Write(true);
			this.spi.Write(data);
        }

		private void WriteData(ushort[] data)
		{
			this.rsPin.Write(true);
			this.spi.Write(data);
		}
    }
}