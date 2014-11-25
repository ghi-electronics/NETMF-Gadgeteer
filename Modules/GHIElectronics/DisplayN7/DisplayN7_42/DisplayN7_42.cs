using Microsoft.SPOT;
using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.Interfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A 7 inch display module for Microsoft .NET Gadgeteer
	/// </summary>
	[Obsolete]
	public class DisplayN7 : GTM.Module.DisplayModule
	{
		private GTI.DigitalOutput backlightPin;
		private bool backlightState;

		/// <summary>
		/// Creates a new DisplayN7 instance.
		/// </summary>
		/// <param name="rgbSocketNumber1">The first R,G,B socket</param>
		/// <param name="rgbSocketNumber2">The second R,G,B socket</param>
		/// <param name="rgbSocketNumber3">The third R,G,B socket</param>
		public DisplayN7(int rgbSocketNumber1, int rgbSocketNumber2, int rgbSocketNumber3) : base(WPFRenderOptions.Ignore)
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

					backlightPin = new GTI.DigitalOutput(rgbSocket, Socket.Pin.Nine, true, this);
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
			Mainboard.LCDConfiguration lcdConfig = new Mainboard.LCDConfiguration();

			lcdConfig.LCDControllerEnabled = true;

			lcdConfig.Width = Width;
			lcdConfig.Height = Height;

			// Only use if needed, see documentation.
			//lcdConfig.PriorityEnable = true;

			lcdConfig.OutputEnableIsFixed = true;
			lcdConfig.OutputEnablePolarity = true;

			lcdConfig.HorizontalSyncPolarity = true;
			lcdConfig.VerticalSyncPolarity = true;
			lcdConfig.PixelPolarity = false;

			lcdConfig.HorizontalSyncPulseWidth = 1;
			lcdConfig.HorizontalBackPorch = 46;
			lcdConfig.HorizontalFrontPorch = 16;
			lcdConfig.VerticalSyncPulseWidth = 1;
			lcdConfig.VerticalBackPorch = 23;
			lcdConfig.VerticalFrontPorch = 7;

			// NOTE: This is used for ChipworkX, comment if using EMX.
			lcdConfig.PixelClockDivider = 5;
			//lcdConfig.PixelClockRate = 25000;

			// Set configs
			DisplayModule.SetLCDConfig(lcdConfig);
		}

		/// <summary>
		/// Gets the width of the display.
		/// </summary>
		/// <remarks>
		/// This property always returns 800.
		/// </remarks>
		public override uint Width { get { return 800; } }

		/// <summary>
		/// Gets the height of the display.
		/// </summary>
		/// <remarks>
		/// This property always returns 480.
		/// </remarks>
		public override uint Height { get { return 480; } }

		/// <summary>
		/// Renders display data on the display device. 
		/// </summary>
		/// <param name="bitmap">The <see cref="T:Microsoft.SPOT.Bitmap"/> object to render on the display.</param>
		protected override void Paint(Bitmap bitmap)
		{
			try
			{
				bitmap.Flush();
			}
			catch
			{
				ErrorPrint("Painting error");
			}
		}
	}
}
