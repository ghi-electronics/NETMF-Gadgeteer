using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2CBus), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// A 4.3 inch TFT display module with resistive touch for Microsoft .NET Gadgeteer.
    /// </summary>
    public class DisplayT43 : GTM.Module.DisplayModule
    {

        /// <summary>
        /// Constructor for the module if touch is not connected.
        /// </summary>
        /// <remarks>
        /// The ordering of the RGB socket numbers does not matter (socket numbers are autodetected).
        /// </remarks>
        /// <param name="rgbSocketNumber1">The mainboard socket that has the display's R, G, or B socket connected to it.</param>
        /// <param name="rgbSocketNumber2">The mainboard socket that has the display's R, G, or B socket connected to it.</param>
        /// <param name="rgbSocketNumber3">The mainboard socket that has the display's R, G, or B socket connected to it.</param>
        public DisplayT43(int rgbSocketNumber1, int rgbSocketNumber2, int rgbSocketNumber3)
            : this(rgbSocketNumber1, rgbSocketNumber2, rgbSocketNumber3, Socket.Unused)
        {
            // Intentionally empty
        }

        /// <summary>
        /// Constructor used if touch is connected.
        /// </summary>
        /// <remarks>
        /// The ordering of the RGB socket numbers does not matter (socket numbers are autodetected).
        /// </remarks>
        /// <param name="rgbSocketNumber1">The mainboard socket that has the display's R socket connected to it.</param>
        /// <param name="rgbSocketNumber2">The mainboard socket that has the display's G socket connected to it.</param>
        /// <param name="rgbSocketNumber3">The mainboard socket that has the display's B socket connected to it.</param>
        /// <param name="touchSocketNumber">Optional: the mainboard socket that has the display's T socket connected to it. This enables the touch panel capabilities.</param>
        public DisplayT43(int rgbSocketNumber1, int rgbSocketNumber2, int rgbSocketNumber3, int touchSocketNumber)
            :base(WpfMode.PassThrough)
        {
            ReserveLCDPins(rgbSocketNumber1, rgbSocketNumber2, rgbSocketNumber3);
            ConfigureLCD();

            if (touchSocketNumber == Socket.Unused) 
                return;

            ReserveTouchPins(touchSocketNumber);
            GT.Program.BeginInvoke(new NullParamsDelegate(EnableTouchPanel), null);
        }

        private static Socket greenSocket;
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
                    greenSocket = rgbSocket;
                    backlightPin = GTI.DigitalOutputFactory.Create(greenSocket, Socket.Pin.Nine, true, this);
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
                rgbSocket.ReservePin(Socket.Pin.Eight, this);

                if(!rgbSocket.SupportsType('G'))
                    rgbSocket.ReservePin(Socket.Pin.Nine, this);

            }
        }

        private void ConfigureLCD()
        {
            DisplayModule.TimingRequirements lcdConfig = new DisplayModule.TimingRequirements();

            

            lcdConfig.CommonSyncPinIsActiveHigh = false;
            lcdConfig.UsesCommonSyncPin = false;

            // Only use if needed, see documentation.
            lcdConfig.PixelDataIsActiveHigh = false; //not the proper property, but we needed it for PriorityEnable

            lcdConfig.UsesCommonSyncPin = false; //not the proper property, but we needed it for OutputEnableIsFixed
            lcdConfig.CommonSyncPinIsActiveHigh = true; //not the proper property, but we needed it for OutputEnablePolarity

            lcdConfig.HorizontalSyncPulseIsActiveHigh = false;
            lcdConfig.VerticalSyncPulseIsActiveHigh = false;
            lcdConfig.PixelDataIsValidOnClockRisingEdge = false;
            

            lcdConfig.HorizontalSyncPulseWidth = 41;
            lcdConfig.HorizontalBackPorch = 2;
            lcdConfig.HorizontalFrontPorch = 2;
            lcdConfig.VerticalSyncPulseWidth = 10;
            lcdConfig.VerticalBackPorch = 2;
            lcdConfig.VerticalFrontPorch = 2;

            //lcdConfig.PixelClockDivider = 6;
            lcdConfig.MaximumClockSpeed = 20000;

            // Set configs
            base.OnDisplayConnected("Display T43", 480, 272, DisplayOrientation.Normal, lcdConfig);
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

        #region Backlight
        private bool _bBackLightOn = true;

        /// <summary>
        /// Accessor for the state of the backlight
        /// </summary>
        public bool BBackLightOn
        {
            get { return _bBackLightOn; }
            //set { _bBackLightOn = value; }
        }

        private static GTI.DigitalOutput backlightPin;// = new OutputPort(greenSocket.CpuPins[9], true);

        /// <summary>
        /// Sets the backlight to the passed in value.
        /// </summary>
        /// <param name="bOn">Backlight state.</param>
        public void SetBacklight(bool bOn)
        {
            if (greenSocket != null)
            {
                backlightPin.Write(bOn);
                _bBackLightOn = bOn;
            }
            else
            {
                ErrorPrint("Cannot set backlight yet. RGB sockets not yet initialized");
            }
        }
        #endregion

        #region Touch
        private delegate void NullParamsDelegate();
        private bool _touchPanelEnabled;

        private void ReserveTouchPins(int touchSocketNumber)
        {
            Socket tsocket = Socket.GetSocket(touchSocketNumber, true, this, "T");

            tsocket.EnsureTypeIsSupported('T', this);
            tsocket.ReservePin(Socket.Pin.Four, this);
            tsocket.ReservePin(Socket.Pin.Five, this);
            tsocket.ReservePin(Socket.Pin.Six, this);
            tsocket.ReservePin(Socket.Pin.Seven, this);
        }

        private void EnableTouchPanel()
        {
            if (!_touchPanelEnabled)
            {
                // Initialize touch input                
                Microsoft.SPOT.Touch.Touch.Initialize(Application.Current);
                _touchPanelEnabled = true;
            }
        }
        #endregion
    }
}
