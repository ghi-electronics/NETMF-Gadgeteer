using Microsoft.SPOT;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A 7 inch display module for Microsoft .NET Gadgeteer
	/// </summary>
	public class Display_N7 : GTM.Module.DisplayModule
	{
		private GTI.DigitalOutput backlightPin;
		private bool backlightState;

		/// <summary>
		/// Creates a new DisplayN7 instance.
		/// </summary>
		/// <param name="rgbSocketNumber1">The first R,G,B socket</param>
		/// <param name="rgbSocketNumber2">The second R,G,B socket</param>
		/// <param name="rgbSocketNumber3">The third R,G,B socket</param>
		public Display_N7(int rgbSocketNumber1, int rgbSocketNumber2, int rgbSocketNumber3) : base(WpfMode.PassThrough)
		{
			this.backlightState = false;
			this.ReserveLCDPins(rgbSocketNumber1, rgbSocketNumber2, rgbSocketNumber3);
			this.ConfigureLCD();
		}

		/// <summary>
		/// Gets or sets the state of the backlight.
		/// </summary>
		public bool BackLight
		{
			get 
			{
				return this.backlightState;
			}
			set 
			{
				this.backlightState = value;
				this.backlightPin.Write(this.backlightState);
			}
		}


		private void ReserveLCDPins(int rgbSocketNumber1, int rgbSocketNumber2, int rgbSocketNumber3)
		{
			bool gotR = false, gotG = false, gotB = false;
			Socket[] rgbSockets = new Socket[3] { Socket.GetSocket(rgbSocketNumber1, true, this, "rgbSocket1"), Socket.GetSocket(rgbSocketNumber2, true, this, "rgbSocket2"), Socket.GetSocket(rgbSocketNumber3, true, this, "rgbSocket3") };

			foreach (var rgbSocket in rgbSockets)
			{
				if (!gotR && rgbSocket.SupportsType('R'))
				{
					gotR = true;
				}
				else if (!gotG && rgbSocket.SupportsType('G'))
				{
					gotG = true;

					backlightPin = GTI.DigitalOutputFactory.Create(rgbSocket, Socket.Pin.Nine, true, this);
				}
				else if (!gotB && rgbSocket.SupportsType('B'))
				{
					gotB = true;
				}
				else
				{
					throw new GT.Socket.InvalidSocketException("Socket " + rgbSocket + " is not an R, G or B socket, as required for the LCD module.");
				}

				rgbSocket.ReservePin(Socket.Pin.Three, this);
				rgbSocket.ReservePin(Socket.Pin.Four, this);
				rgbSocket.ReservePin(Socket.Pin.Five, this);
				rgbSocket.ReservePin(Socket.Pin.Six, this);
				rgbSocket.ReservePin(Socket.Pin.Seven, this);

				if (!rgbSocket.SupportsType('G'))
					rgbSocket.ReservePin(Socket.Pin.Nine, this);
			}
		}

		private void ConfigureLCD()
		{
			DisplayModule.TimingRequirements lcdConfig = new DisplayModule.TimingRequirements();

			

			lcdConfig.CommonSyncPinIsActiveHigh = false;
			lcdConfig.UsesCommonSyncPin = false;

            // Only use if needed, see documentation.
            lcdConfig.PixelDataIsActiveHigh = true; //not the proper property, but we needed it for PriorityEnable

            lcdConfig.UsesCommonSyncPin = true; //not the proper property, but we needed it for OutputEnableIsFixed
            lcdConfig.CommonSyncPinIsActiveHigh = true; //not the proper property, but we needed it for OutputEnablePolarity

			lcdConfig.HorizontalSyncPulseIsActiveHigh = true;
            lcdConfig.VerticalSyncPulseIsActiveHigh = true;
            lcdConfig.PixelDataIsValidOnClockRisingEdge = true;

			lcdConfig.HorizontalSyncPulseWidth = 1;
			lcdConfig.HorizontalBackPorch = 46;
			lcdConfig.HorizontalFrontPorch = 16;
			lcdConfig.VerticalSyncPulseWidth = 1;
			lcdConfig.VerticalBackPorch = 23;
			lcdConfig.VerticalFrontPorch = 7;

			// NOTE: This is used for ChipworkX, comment if using EMX.
			//lcdConfig.PixelClockDivider = 5;
			lcdConfig.MaximumClockSpeed = 24000;

			// Set configs
			base.OnDisplayConnected("Display N7", 800, 480, DisplayOrientation.Normal, lcdConfig);
		}

		/// <summary>
		/// Renders display data on the display device. 
		/// </summary>
        /// <param name="bitmap">The <see cref="T:Microsoft.SPOT.Bitmap"/> object to render on the display.</param>
        /// <param name="x">The start x coordinate of the dirty area.</param>
        /// <param name="y">The start y coordinate of the dirty area.</param>
        /// <param name="width">The width of the dirty area.</param>
        /// <param name="height">The height of the dirty area.</param>
        protected override void Paint(Bitmap bitmap, int x, int y, int width, int height)
		{
			try
			{
				bitmap.Flush(x, y, width, height);
			}
			catch
			{
				ErrorPrint("Painting error");
			}
		}
	}
}
