using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A DisplayN7 module for Microsoft .NET Gadgeteer.</summary>
	[Obsolete]
	public class DisplayN7 : GTM.Module.DisplayModule {
		private GTI.DigitalOutput backlightPin;

		/// <summary>Whether or not the backlight is enabled.</summary>
		public bool BacklightEnabled {
			get {
				return this.backlightPin.Read();
			}

			set {
				this.backlightPin.Write(value);
			}
		}

		/// <summary>Constructs a new instance.</summary>
		/// <param name="rSocketNumber">The mainboard socket that has the display's R socket connected to it.</param>
		/// <param name="gSocketNumber">The mainboard socket that has the display's G socket connected to it.</param>
		/// <param name="bSocketNumber">The mainboard socket that has the display's B socket connected to it.</param>
		public DisplayN7(int rSocketNumber, int gSocketNumber, int bSocketNumber)
			: base(WpfMode.PassThrough) {
			var config = new DisplayModule.TimingRequirements() {
				UsesCommonSyncPin = true, //not the proper property, but we needed it for OutputEnableIsFixed
				CommonSyncPinIsActiveHigh = true, //not the proper property, but we needed it for OutputEnablePolarity
				HorizontalSyncPulseIsActiveHigh = true,
				VerticalSyncPulseIsActiveHigh = true,
				PixelDataIsValidOnClockRisingEdge = false,
				HorizontalSyncPulseWidth = 1,
				HorizontalBackPorch = 46,
				HorizontalFrontPorch = 16,
				VerticalSyncPulseWidth = 1,
				VerticalBackPorch = 23,
				VerticalFrontPorch = 7,
				MaximumClockSpeed = 24000,
			};

			base.OnDisplayConnected("Display N7", 800, 480, DisplayOrientation.Normal, config);

			var rSocket = Socket.GetSocket(rSocketNumber, true, this, null);
			var gSocket = Socket.GetSocket(gSocketNumber, true, this, null);
			var bSocket = Socket.GetSocket(bSocketNumber, true, this, null);

			rSocket.EnsureTypeIsSupported('R', this);
			gSocket.EnsureTypeIsSupported('G', this);
			bSocket.EnsureTypeIsSupported('B', this);

			this.backlightPin = GTI.DigitalOutputFactory.Create(gSocket, Socket.Pin.Nine, true, this);

			rSocket.ReservePin(Socket.Pin.Three, this);
			rSocket.ReservePin(Socket.Pin.Four, this);
			rSocket.ReservePin(Socket.Pin.Five, this);
			rSocket.ReservePin(Socket.Pin.Six, this);
			rSocket.ReservePin(Socket.Pin.Seven, this);
			rSocket.ReservePin(Socket.Pin.Eight, this);
			rSocket.ReservePin(Socket.Pin.Nine, this);

			gSocket.ReservePin(Socket.Pin.Three, this);
			gSocket.ReservePin(Socket.Pin.Four, this);
			gSocket.ReservePin(Socket.Pin.Five, this);
			gSocket.ReservePin(Socket.Pin.Six, this);
			gSocket.ReservePin(Socket.Pin.Seven, this);
			gSocket.ReservePin(Socket.Pin.Eight, this);

			bSocket.ReservePin(Socket.Pin.Three, this);
			bSocket.ReservePin(Socket.Pin.Four, this);
			bSocket.ReservePin(Socket.Pin.Five, this);
			bSocket.ReservePin(Socket.Pin.Six, this);
			bSocket.ReservePin(Socket.Pin.Seven, this);
			bSocket.ReservePin(Socket.Pin.Eight, this);
			bSocket.ReservePin(Socket.Pin.Nine, this);
		}

		/// <summary>Renders display data on the display device.</summary>
		/// <param name="bitmap">The <see cref="T:Microsoft.SPOT.Bitmap" /> object to render on the display.</param>
		/// <param name="x">The start x coordinate of the dirty area.</param>
		/// <param name="y">The start y coordinate of the dirty area.</param>
		/// <param name="width">The width of the dirty area.</param>
		/// <param name="height">The height of the dirty area.</param>
		protected override void Paint(Bitmap bitmap, int x, int y, int width, int height) {
			try {
				bitmap.Flush(x, y, width, height);
			}
			catch {
				ErrorPrint("Painting error");
			}
		}
	}
}