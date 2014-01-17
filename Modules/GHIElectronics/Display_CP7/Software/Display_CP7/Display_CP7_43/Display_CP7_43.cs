using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

using Microsoft.SPOT.Hardware;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2CBus), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// A 7 inch capacitive touch display module for Microsoft .NET Gadgeteer
    /// </summary>
    /// <example>
    /// <para>The following example uses a <see cref="Display_CP7"/> object to display the picture taken by a camera module. 
    /// First the code initializes a camera object and the button pressed event delegate in which the camera takes a picture.
    /// Then, another delegate is initialized to handle the asynchronous PictureCaptured event.  In this method the display module uses 
    /// the SimpleGraphics class to display the picture captured by the camera.
    /// </para>
    /// <code>
    /// using System;
    /// using Microsoft.SPOT;
    /// using Microsoft.SPOT.Presentation;
    /// using Microsoft.SPOT.Presentation.Controls;
    /// using Microsoft.SPOT.Presentation.Media;
    ///
    /// using GT = Gadgeteer;
    /// using GTM = Gadgeteer.Modules;
    ///
    /// using Gadgeteer.Modules.GHIElectronics;
    ///
    /// namespace TestApp
    /// {
    ///     public partial class Program
    ///     {
    ///         // This template uses the FEZ Spider mainboard from GHI Electronics
    ///
    ///         // Define and initialize GTM.Modules here, specifying their socket numbers.        
    ///         GTM.GHIElectronics.UsbClientDP usbClient = new UsbClientDP(1);
    ///         GTM.GHIElectronics.Button button = new Button(4);
    ///         GTM.GHIElectronics.Camera camera = new Camera(3);
    ///         GTM.GHIElectronics.Display_T35 display = new Display_T35(12, 13, 14);
    ///
    ///         void ProgramStarted()
    ///         {
    ///             // Initialize event handlers here.
    ///             button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
    ///             camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);
    ///
    ///             // Do one-time tasks here
    ///             Debug.Print("Program Started");
    ///         }
    ///
    ///         void camera_PictureCaptured(Camera sender, GT.Picture picture)
    ///         {
    ///             Debug.Print("Picture Captured event.");
    ///             display.SimpleGraphics.DisplayImage(picture, 5, 5);
    ///         }
    ///
    ///         void button_ButtonPressed(Button sender, Button.ButtonState state)
    ///         {
    ///             camera.TakePicture();
    ///         }
    ///     }
    /// }
    /// 
    /// </code>
    /// </example>
    public class Display_CP7 : GTM.Module.DisplayModule
    {
        /// <summary>
        /// The I2C bus that the module will use
        /// </summary>
        public static GT.SocketInterfaces.I2CBus i2cBus;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rgbSocketNumber1">The first R,G,B socket</param>
        /// <param name="rgbSocketNumber2">The second R,G,B socket</param>
        /// <param name="rgbSocketNumber3">The third R,G,B socket</param>
        /// <param name="i2cSocketNumber">The I2C socket</param>
        public Display_CP7(int rgbSocketNumber1, int rgbSocketNumber2, int rgbSocketNumber3, int i2cSocketNumber)
            : base(WpfMode.PassThrough)
        {
            // This finds the Socket instance from the user-specified socket number.  
            // This will generate user-friendly error messages if the socket is invalid.
            // If there is more than one socket on this module, then instead of "null" for the last parameter, 
            // put text that identifies the socket to the user (e.g. "S" if there is a socket type S)
            ReserveLCDPins(rgbSocketNumber1, rgbSocketNumber2, rgbSocketNumber3);
            ConfigureLCD();

            Socket i2cSocket = Socket.GetSocket(i2cSocketNumber, true, this, "i2cSocket");
            i2cBus = GTI.I2CBusFactory.Create(i2cSocket, 0x38, 400, this);

            // This creates an GTI.InterruptInput interface. The interfaces under the GTI namespace provide easy ways to build common modules.
            // This also generates user-friendly error messages automatically, e.g. if the user chooses a socket incompatible with an interrupt input.
            this.touchInterrupt = GTI.InterruptInputFactory.Create(i2cSocket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.PullUp, GTI.InterruptMode.RisingAndFallingEdge, this);

            // This registers a handler for the interrupt event of the interrupt input (which is bereleased)
            this.touchInterrupt.Interrupt += (this._input_Interrupt);
        }

        private bool _bBackLightOn = true;

        /// <summary>
        /// Accessor for the state of the backlight
        /// </summary>
        public bool BBackLightOn
        {
            get { return _bBackLightOn; }
            //set { _bBackLightOn = value; }
        }

        //private bool _bEnable = true;

        ///// <summary>
        ///// Accessor returning if the display is enabled.
        ///// </summary>
        //public bool BEnable
        //{
        //    get { return _bEnable; }
        //    //set { _bEnable = value; }
        //}

        private static GTI.DigitalOutput backlightPin;// = new OutputPort(greenSocket.CpuPins[9], true);
        private static Socket greenSocket;

        //private static OutputPort enablePin;// = new OutputPort(greenSocket.CpuPins[9], true);
        //private static Socket blueSocket;

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

        ///// <summary>
        ///// Sets the backlight to the passed in value.
        ///// </summary>
        ///// <param name="bOn">Backlight state.</param>
        //public void SetEnable(bool bOn)
        //{
        //    if (blueSocket != null)
        //    {
        //        if (bOn)
        //        {
        //            enablePin.Write(true);
        //        }
        //        else
        //        {
        //            enablePin.Write(false);
        //        }
        //    }
        //    else
        //    {
        //        ErrorPrint("Cannot set enable pin yet. RGB sockets not yet initialized");
        //    }
        //}

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

                    //blueSocket = rgbSocket;
                    //enablePin = new OutputPort(blueSocket.CpuPins[8], true);
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
            lcdConfig.PixelDataIsValidOnClockRisingEdge = false;

            lcdConfig.HorizontalSyncPulseWidth = 1;
            lcdConfig.HorizontalBackPorch = 46;
            lcdConfig.HorizontalFrontPorch = 16;
            lcdConfig.VerticalSyncPulseWidth = 1;
            lcdConfig.VerticalBackPorch = 23;
            lcdConfig.VerticalFrontPorch = 7;

            // NOTE: This is used for ChipworkX, comment if using EMX.
			//lcdConfig.PixelClockDivider = 5;
            //dConfig.PixelClockRate = 25000;
            lcdConfig.MaximumClockSpeed = 24000;

            // Set configs
            base.OnDisplayConnected("Display CP7", 800, 480, DisplayOrientation.Normal, lcdConfig);
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
        public event TouchEventHandlerHomeButton HomePressed;

        /// <summary>
        /// Raised when the screen detects a touch on the menu button
        /// </summary>
        public event TouchEventHandlerMenuButton MenuPressed;

        /// <summary>
        /// Raised when the screen detects a touch on the back button
        /// </summary>
        public event TouchEventHandlerBackButton BackPressed;

        /// <summary>
        /// Raised when the screen detects all touches released
        /// </summary>
        public event TouchEventHandlerTouchReleased ScreenReleased;

        /// <summary>
        /// Raised when the screen detects a touch gesture
        /// </summary>
        public event TouchGestureDetected GestureDetected;

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

                    if (Program.CheckAndInvoke(ScreenReleased, this.onScreenReleased, this))
                        this.ScreenReleased(this);

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

                if (Program.CheckAndInvoke(GestureDetected, this.onGestureDetected, this, (Gesture_ID)gesture))
                    this.GestureDetected(this, (Gesture_ID)gesture);

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

                            if (Program.CheckAndInvoke(HomePressed, this.onHomePressed, this))
                                this.HomePressed(this);
                        }
                        if (y >= 100 && y <= 150)
                        {
                            // Menu
                            if (this.onMenuPressed == null)
                                this.onMenuPressed = new TouchEventHandlerMenuButton(this.onMenuPressed);

                            if (Program.CheckAndInvoke(MenuPressed, this.onMenuPressed, this))
                                this.MenuPressed(this);
                        }
                        else if (y >= 200 && y <= 250)
                        {
                            // Back
                            if (this.onBackPressed == null)
                                this.onBackPressed = new TouchEventHandlerBackButton(this.onBackPressed);

                            if (Program.CheckAndInvoke(BackPressed, this.onBackPressed, this))
                                this.BackPressed(this);
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

            if (i2cBus.Execute(xActions) == 0)
            {
                Debug.Print("Failed to perform I2C transaction");
            }
            else
            {
                //Debug.Print("Register value: " + RegisterValue[0].ToString());
            }

            return RegisterValue[0];
        }
    }
}
