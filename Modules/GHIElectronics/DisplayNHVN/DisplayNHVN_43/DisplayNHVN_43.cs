using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A DisplayNHVN module for Microsoft .NET Gadgeteer.</summary>
	public class DisplayNHVN : GTM.Module.DisplayModule {
		private GTI.DigitalOutput backlightPin;
		private GTI.InterruptInput touchInterrupt;
		private GTI.I2CBus i2cBus;
		private I2CDevice.I2CTransaction[] transactions;
		private byte[] addressBuffer;
		private byte[] resultBuffer;

		private delegate void NullParamsDelegate();

		/// <summary>The delegate that is used to handle the capacitive touch events.</summary>
		/// <param name="sender">The DisplayNHVN object that raised the event.</param>
		/// <param name="e">The event arguments.</param>
		public delegate void CapacitiveTouchEventHandler(DisplayNHVN sender, TouchEventArgs e);

		/// <summary>Raised when the module detects a capacitive press.</summary>
		public event CapacitiveTouchEventHandler CapacitiveScreenPressed;

		/// <summary>Raised when the module detects a capacitive release.</summary>
		public event CapacitiveTouchEventHandler CapacitiveScreenReleased;

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
		public DisplayNHVN(int rSocketNumber, int gSocketNumber, int bSocketNumber)
			: this(rSocketNumber, gSocketNumber, bSocketNumber, Socket.Unused, Socket.Unused) {

		}

		/// <summary>Constructs a new instance.</summary>
		/// <param name="rSocketNumber">The mainboard socket that has the display's R socket connected to it.</param>
		/// <param name="gSocketNumber">The mainboard socket that has the display's G socket connected to it.</param>
		/// <param name="bSocketNumber">The mainboard socket that has the display's B socket connected to it.</param>
		/// <param name="tSocketNumber">The mainboard socket that has the display's T socket connected to it.</param>
		/// <param name="iSocketNumber">The mainboard socket that has the display's I socket connected to it.</param>
		public DisplayNHVN(int rSocketNumber, int gSocketNumber, int bSocketNumber, int tSocketNumber, int iSocketNumber)
			: base(WpfMode.PassThrough) {
			if (tSocketNumber != Socket.Unused && iSocketNumber != Socket.Unused) throw new InvalidOperationException("The T and I sockets may not be connected at the same time.");

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

			if (tSocketNumber != Socket.Unused) {
				var tSocket = Socket.GetSocket(tSocketNumber, true, this, null);

				tSocket.EnsureTypeIsSupported('T', this);

				tSocket.ReservePin(Socket.Pin.Four, this);
				tSocket.ReservePin(Socket.Pin.Five, this);
				tSocket.ReservePin(Socket.Pin.Six, this);
				tSocket.ReservePin(Socket.Pin.Seven, this);

				GT.Program.BeginInvoke(new NullParamsDelegate(() => Microsoft.SPOT.Touch.Touch.Initialize(Application.Current)), null);
			}
			else if (iSocketNumber != Socket.Unused) {
				var iSocket = Socket.GetSocket(iSocketNumber, true, this, null);

				this.transactions = new I2CDevice.I2CTransaction[2];
				this.resultBuffer = new byte[1];
				this.addressBuffer = new byte[1];
				this.i2cBus = GTI.I2CBusFactory.Create(iSocket, 0x38, 400, this);
				this.touchInterrupt = GTI.InterruptInputFactory.Create(iSocket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, GTI.InterruptMode.RisingAndFallingEdge, this);
				this.touchInterrupt.Interrupt += (a, b) => this.OnTouchEvent();
			}
		}

		/// <summary>Renders display data on the display device.</summary>
		/// <param name="bitmap">The bitmap object to render on the display.</param>
		/// <param name="x">The start x coordinate of the dirty area.</param>
		/// <param name="y">The start y coordinate of the dirty area.</param>
		/// <param name="width">The width of the dirty area.</param>
		/// <param name="height">The height of the dirty area.</param>
		protected override void Paint(Bitmap bitmap, int x, int y, int width, int height) {
			try {
				bitmap.Flush(x, y, width, height);
			}
			catch {
				this.ErrorPrint("Painting error");
			}
		}

		/// <summary>
		/// Configures the module to use the 7" display.
		/// </summary>
		public void Configure7InchDisplay() {
			var config = new DisplayModule.TimingRequirements() {
				UsesCommonSyncPin = false, //not the proper property, but we needed it for OutputEnableIsFixed
				CommonSyncPinIsActiveHigh = true, //not the proper property, but we needed it for OutputEnablePolarity
				PixelDataIsValidOnClockRisingEdge = false,
				MaximumClockSpeed = 20000,
				HorizontalSyncPulseIsActiveHigh = false,
				HorizontalSyncPulseWidth = 48,
				HorizontalBackPorch = 88,
				HorizontalFrontPorch = 40,
				VerticalSyncPulseIsActiveHigh = false,
				VerticalSyncPulseWidth = 3,
				VerticalBackPorch = 32,
				VerticalFrontPorch = 13,
			};

			base.OnDisplayConnected("Display NHVN", 800, 480, DisplayOrientation.Normal, config);
		}

		/// <summary>
		/// Configures the module to use the 4.3" display.
		/// </summary>
		public void Configure43InchDisplay() {
			var config = new DisplayModule.TimingRequirements() {
				UsesCommonSyncPin = false, //not the proper property, but we needed it for OutputEnableIsFixed
				CommonSyncPinIsActiveHigh = true, //not the proper property, but we needed it for OutputEnablePolarity
				PixelDataIsValidOnClockRisingEdge = false,
				MaximumClockSpeed = 20000,
				HorizontalSyncPulseIsActiveHigh = false,
				HorizontalSyncPulseWidth = 41,
				HorizontalBackPorch = 2,
				HorizontalFrontPorch = 2,
				VerticalSyncPulseIsActiveHigh = false,
				VerticalSyncPulseWidth = 10,
				VerticalBackPorch = 2,
				VerticalFrontPorch = 2,
			};

			base.OnDisplayConnected("Display NHVN", 480, 272, DisplayOrientation.Normal, config);
		}

		private void OnTouchEvent() {
			for (var i = 0; i < 5; i++) {
				var first = this.ReadRegister((byte)(3 + i * 6));
				var x = ((first & 0x0F) << 8) + this.ReadRegister((byte)(4 + i * 6));
				var y = ((this.ReadRegister((byte)(5 + i * 6)) & 0x0F) << 8) + this.ReadRegister((byte)(6 + i * 6));

				if (x == 4095 && y == 4095)
					break;

				if (((first & 0xC0) >> 6) == 1) {
					this.CapacitiveScreenReleased(this, new TouchEventArgs(x, y));
				}
				else {
					this.CapacitiveScreenPressed(this, new TouchEventArgs(x, y));
				}
			}
		}

		private byte ReadRegister(byte address) {
			this.addressBuffer[0] = address;

			this.transactions[0] = I2CDevice.CreateWriteTransaction(this.addressBuffer);
			this.transactions[1] = I2CDevice.CreateReadTransaction(this.resultBuffer);

			this.i2cBus.Execute(this.transactions);

			return this.resultBuffer[0];
		}

		/// <summary>
		/// Event arguments for the capacitive touch events.
		/// </summary>
		public class TouchEventArgs : EventArgs {
			/// <summary>
			/// The X coordinate of the touch event.
			/// </summary>
			public int X { get; set; }

			/// <summary>
			/// The Y coordinate of the touch event.
			/// </summary>
			public int Y { get; set; }

			internal TouchEventArgs(int x, int y) {
				this.X = x;
				this.Y = y;
			}
		}
	}
}