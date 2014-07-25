using Microsoft.SPOT;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A DisplayTE35 module for Microsoft .NET Gadgeteer.
    /// </summary>
    public class DisplayTE35 : GTM.Module.DisplayModule
    {
        private delegate void NullParamsDelegate();
        private GTI.DigitalOutput backlightPin;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="rSocketNumber">The mainboard socket that has the display's R socket connected to it.</param>
        /// <param name="gSocketNumber">The mainboard socket that has the display's G socket connected to it.</param>
        /// <param name="bSocketNumber">The mainboard socket that has the display's B socket connected to it.</param>
        public DisplayTE35(int rSocketNumber, int gSocketNumber, int bSocketNumber) : this(rSocketNumber, gSocketNumber, bSocketNumber, Socket.Unused)
        {

        }

        /// <summary>Constructs a new instance.</summary>
        /// <param name="rSocketNumber">The mainboard socket that has the display's R socket connected to it.</param>
        /// <param name="gSocketNumber">The mainboard socket that has the display's G socket connected to it.</param>
        /// <param name="bSocketNumber">The mainboard socket that has the display's B socket connected to it.</param>
        /// <param name="tSocketNumber">The mainboard socket that has the display's T socket connected to it.</param>
        public DisplayTE35(int rSocketNumber, int gSocketNumber, int bSocketNumber, int tSocketNumber) : base(WpfMode.PassThrough)
        {
            var config = new DisplayModule.TimingRequirements()
            {
                UsesCommonSyncPin = true, //not the proper property, but we needed it for OutputEnableIsFixed
                CommonSyncPinIsActiveHigh = true, //not the proper property, but we needed it for OutputEnablePolarity
                HorizontalSyncPulseIsActiveHigh = false,
                VerticalSyncPulseIsActiveHigh = false,
                PixelDataIsValidOnClockRisingEdge = true,
                HorizontalSyncPulseWidth = 41,
                HorizontalBackPorch = 29,
                HorizontalFrontPorch = 51,
                VerticalSyncPulseWidth = 10,
                VerticalBackPorch = 3,
                VerticalFrontPorch = 16,
                MaximumClockSpeed = 15000
            };

            base.OnDisplayConnected("Display TE35", 320, 240, DisplayOrientation.Normal, config);

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

            if (tSocketNumber == Socket.Unused)
                return;

            var tSocket = Socket.GetSocket(tSocketNumber, true, this, null);

            tSocket.EnsureTypeIsSupported('T', this);

            tSocket.ReservePin(Socket.Pin.Four, this);
            tSocket.ReservePin(Socket.Pin.Five, this);
            tSocket.ReservePin(Socket.Pin.Six, this);
            tSocket.ReservePin(Socket.Pin.Seven, this);

            GT.Program.BeginInvoke(new NullParamsDelegate(() => Microsoft.SPOT.Touch.Touch.Initialize(Application.Current)), null);
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
                bitmap.Flush(x, y, width, height);
            }
            catch
            {
                this.ErrorPrint("Painting error");
            }
        }
    }
}
