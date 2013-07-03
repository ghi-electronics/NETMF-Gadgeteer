using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;
using Microsoft.SPOT.Hardware;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A Display_CP7 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class Display_CP7 : GTM.Module.DisplayModule
    {
        /// <summary>
        /// The I2C bus that the module will use
        /// </summary>
        public static GT.Interfaces.I2CBus i2cBus;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rgbSocketNumber1">The first R,G,B socket</param>
        /// <param name="rgbSocketNumber2">The second R,G,B socket</param>
        /// <param name="rgbSocketNumber3">The third R,G,B socket</param>
        /// <param name="i2cSocketNumber">The I2C socket</param>
        public Display_CP7(int rgbSocketNumber1, int rgbSocketNumber2, int rgbSocketNumber3, int i2cSocketNumber)
            : base(WPFRenderOptions.Ignore)
        {
            // This finds the Socket instance from the user-specified socket number.  
            // This will generate user-friendly error messages if the socket is invalid.
            // If there is more than one socket on this module, then instead of "null" for the last parameter, 
            // put text that identifies the socket to the user (e.g. "S" if there is a socket type S)
            ReserveLCDPins(rgbSocketNumber1, rgbSocketNumber2, rgbSocketNumber3);
            ConfigureLCD();

            Socket i2cSocket = Socket.GetSocket(i2cSocketNumber, true, this, "i2cSocket");
            i2cBus = new GTI.I2CBus(i2cSocket, 0x38, 400, this);

            // This creates an GTI.InterruptInput interface. The interfaces under the GTI namespace provide easy ways to build common modules.
            // This also generates user-friendly error messages automatically, e.g. if the user chooses a socket incompatible with an interrupt input.
            this.touchInterrupt = new GTI.InterruptInput(i2cSocket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.PullUp, GTI.InterruptMode.RisingAndFallingEdge, this);

            // This registers a handler for the interrupt event of the interrupt input (which is bereleased)
            this.touchInterrupt.Interrupt += new GTI.InterruptInput.InterruptEventHandler(this._input_Interrupt);
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
                }
                else if (!gotB && rgbSocket.SupportsType('B'))
                {
                    gotB = true;
                    //en = new GTI.DigitalOutput(rgbSocket, Socket.Pin.Eight, true, this);
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
                //rgbSocket.ReservePin(Socket.Pin.Eight, this);
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
            lcdConfig.PriorityEnable = true;

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

            // NOTE: This is used for EMX, comment if using ChipworkX.
            //lcdConfig.PixelClockDivider = 6;

            // NOTE: This is used for ChipworkX, comment if using EMX.
            lcdConfig.PixelClockDivider = 4;

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

        private void _input_Interrupt(GTI.InterruptInput input, bool value)
        {
            this.OnTouchEvent(this, null);
        }

        private GTI.InterruptInput touchInterrupt;

        /// <summary>
        /// An enum to describe the possible gestures
        /// </summary>
        public enum Gesture_ID
        {
            /// <summary>
            /// A move up gesture
            /// </summary>
            Move_Up = 0x10,

            /// <summary>
            /// A move left gesture
            /// </summary>
            Move_Left = 0x14,

            /// <summary>
            /// A move down gesture
            /// </summary>
            Move_Down = 0x18,

            /// <summary>
            /// A move right gesture
            /// </summary>
            Move_Right = 0x1C,

            /// <summary>
            /// A zoom in gesture
            /// </summary>
            Zoom_In = 0x48,

            /// <summary>
            /// A zoom out gesture
            /// </summary>
            Zoom_Out = 0x49,

            /// <summary>
            /// No gesture detected
            /// </summary>
            No_Gesture = 0x00
        };

        /// <summary>
        /// Represents the delegate for when the screen is touched
        /// </summary>
        /// <param name="sender">The sending module</param>
        /// <param name="touchStatus">The class that holds all of the information about interaction with the screen</param>
        public delegate void TouchEventHandler(Display_CP7 sender, TouchStatus touchStatus);

        /// <summary>
        /// Represents the delegate for when the screen's home button has been pushed
        /// </summary>
        /// <param name="sender">The sending module</param>
        public delegate void TouchEventHandlerHomeButton(Display_CP7 sender);

        /// <summary>
        /// Represents the delegate for when the screen's menu button has been pressed
        /// </summary>
        /// <param name="sender">The sending module</param>
        public delegate void TouchEventHandlerMenuButton(Display_CP7 sender);

        /// <summary>
        /// Represents the delegate for when the screen's back button has been pressed
        /// </summary>
        /// <param name="sender">The sending module</param>
        public delegate void TouchEventHandlerBackButton(Display_CP7 sender);

        /// <summary>
        /// Represents the delegate for when the screen has been released
        /// </summary>
        /// <param name="sender">The sending module</param>
        public delegate void TouchEventHandlerTouchReleased(Display_CP7 sender);

        /// <summary>
        /// Represents the delegate for when a touch gestur was detected
        /// </summary>
        /// <param name="sender">The sending module</param>
        /// <param name="id">The ID of the detected gesture</param>
        public delegate void TouchGestureDetected(Display_CP7 sender, Gesture_ID id);

        /// <summary>
        /// Raised when the screen detects a touch
        /// </summary>
        public event TouchEventHandler ScreenPressed;

        /// <summary>
        /// Raised when the screen detects a touch on the home button
        /// </summary>
        public event TouchEventHandlerHomeButton homePressed;

        /// <summary>
        /// Raised when the screen detects a touch on the menu button
        /// </summary>
        public event TouchEventHandlerMenuButton menuPressed;

        /// <summary>
        /// Raised when the screen detects a touch on the back button
        /// </summary>
        public event TouchEventHandlerBackButton backPressed;

        /// <summary>
        /// Raised when the screen detects all touches released
        /// </summary>
        public event TouchEventHandlerTouchReleased screenReleased;

        /// <summary>
        /// Raised when the screen detects a touch gesture
        /// </summary>
        public event TouchGestureDetected gestureDetected;

        private TouchEventHandler onTouch;

        private TouchEventHandlerHomeButton onHomePressed;
        private TouchEventHandlerMenuButton onMenuPressed;
        private TouchEventHandlerBackButton onBackPressed;

        private TouchEventHandlerTouchReleased onScreenReleased;

        private TouchGestureDetected onGestureDetected;

        /// <summary>
        /// A class that is responisble for holding information of the current touches on the screen
        /// </summary>
        public class TouchStatus
        {
            /// <summary>
            /// An array of positions, one for each detected touch point
            /// </summary>
            public Finger[] touchPos;

            /// <summary>
            /// Number of current touches
            /// </summary>
            public int numTouches;

            /// <summary>
            /// Constructor
            /// </summary>
            public TouchStatus()
            {
                touchPos = new Finger[5];
            }
        }

        /// <summary>
        /// Structure that represents a single position
        /// </summary>
        public struct Finger
        {
            /// <summary>
            /// X coordinate of the touch position
            /// </summary>
            public int xPos;

            /// <summary>
            /// Y coordinate of the touch position
            /// </summary>
            public int yPos;

            /// <summary>
            /// Determines if this touch is currently in use.
            /// True: this touch is currently registered with the screen
            /// False: this touch is not currently registered with the screen
            /// </summary>
            public bool bActive;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="x">X coordinate</param>
            /// <param name="y">Y coordinate</param>
            /// <param name="active">If the position is active</param>
            public Finger(int x, int y, bool active)
            {
                this.xPos = x;
                this.yPos = y;
                this.bActive = active;
            }
        }

        bool bSentReleased = false;
        /// <summary>
        /// Raises events for both the touch positions and touch gestures
        /// </summary>
        /// <param name="sender">The module that is sending the event</param>
        /// <param name="touchStatus">A class that contains all information about the screen</param>
        protected virtual void OnTouchEvent(Display_CP7 sender, TouchStatus touchStatus)
        {
            int numberOfFingers = (ReadRegister(0x02) & 0xF);

            if (numberOfFingers == 0)
            {
                if (!bSentReleased)
                {
                    if (this.onScreenReleased == null)
                        this.onScreenReleased = new TouchEventHandlerTouchReleased(this.onScreenReleased);

                    if (Program.CheckAndInvoke(screenReleased, this.onScreenReleased, this))
                        this.screenReleased(this);

                    bSentReleased = true;
                }

                return;
            }

            bSentReleased = false;

            int gesture = (ReadRegister(0x01) & 0xFF);

            if (gesture != (int)Gesture_ID.No_Gesture)
            {
                if (this.onGestureDetected == null)
                    this.onGestureDetected = new TouchGestureDetected(this.onGestureDetected);

                if (Program.CheckAndInvoke(gestureDetected, this.onGestureDetected, this, (Gesture_ID)gesture))
                    this.gestureDetected(this, (Gesture_ID)gesture);

                return;
            }
            //Debug.Print("num fingers" + numberOfFingers);

            TouchStatus ts = new TouchStatus();

            ts.numTouches = numberOfFingers;

            for (int i = 0; i < 5; i++)
            {
                if (i < numberOfFingers)
                {
                    int x = ((ReadRegister((byte)(3 + i * 6)) & 0xF) << 8) + ReadRegister((byte)(4 + i * 6));
                    int y = ((ReadRegister((byte)(5 + i * 6)) & 0xF) << 8) + ReadRegister((byte)(6 + i * 6));

                    ts.touchPos[i] = new Finger(x, y, true);

                    //////////////////////////////////////////////////////////////////////
                    // HEY LISTEN
                    // DO THE BUTTON THINGS RIGHT HERE
                    /////////////////////////////////////////////////////////////////////
                    // Check to see if a user has used one of the "Android" buttons
                    if (x > 800)
                    {
                        if (y >= 0 && y <= 50)
                        {
                            // Home
                            if (this.onHomePressed == null)
                                this.onHomePressed = new TouchEventHandlerHomeButton(this.onHomePressed);

                            if (Program.CheckAndInvoke(homePressed, this.onHomePressed, this))
                                this.homePressed(this);
                        }
                        if (y >= 100 && y <= 150)
                        {
                            // Menu
                            if (this.onMenuPressed == null)
                                this.onMenuPressed = new TouchEventHandlerMenuButton(this.onMenuPressed);

                            if (Program.CheckAndInvoke(menuPressed, this.onMenuPressed, this))
                                this.menuPressed(this);
                        }
                        else if (y >= 200 && y <= 250)
                        {
                            // Back
                            if (this.onBackPressed == null)
                                this.onBackPressed = new TouchEventHandlerBackButton(this.onBackPressed);

                            if (Program.CheckAndInvoke(backPressed, this.onBackPressed, this))
                                this.backPressed(this);
                        }
                    }
                }
                else
                {
                    ts.touchPos[i] = new Finger(-1, -1, false);
                }

                //Debug.Print("X: " + x + " Y: " + y);
            }

            if (this.onTouch == null)
            {
                this.onTouch = new TouchEventHandler(this.OnTouchEvent);
            }

            if (Program.CheckAndInvoke(ScreenPressed, this.onTouch, sender, ts))
            {
                this.ScreenPressed(sender, ts);
            }
        }

        static byte ReadRegister(byte Address)
        {
            I2CDevice.I2CTransaction[] xActions = new I2CDevice.I2CTransaction[2];

            // create write buffer (we need one byte)
            byte[] RegisterAddress = new byte[1] { Address };
            xActions[0] = I2CDevice.CreateWriteTransaction(RegisterAddress);
            // create read buffer to read the register
            byte[] RegisterValue = new byte[1];
            xActions[1] = I2CDevice.CreateReadTransaction(RegisterValue);

            if (i2cBus.Execute(xActions, 500) == 0)
            {
                Debug.Print("Failed to perform I2C transaction");
                //i2cBus.Write(null, 1000);
                //RST.Write(true);
                //System.Threading.Thread.Sleep(500);
                //RST.Write(false);                
            }
            else
            {
                //Debug.Print("Register value: " + RegisterValue[0].ToString());
            }

            return RegisterValue[0];
        }
    }
}
