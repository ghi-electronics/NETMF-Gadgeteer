using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A DisplayCP7 module for Microsoft .NET Gadgeteer.
    /// </summary>
    public class DisplayCP7 : GTM.Module.DisplayModule
    {
        private GTI.DigitalOutput backlightPin;
        private GTI.InterruptInput touchInterrupt;
        private GTI.I2CBus i2cBus;
        private I2CDevice.I2CTransaction[] transactions;
        private byte[] addressBuffer;
        private byte[] resultBuffer;
        private bool releaseSent;

        private TouchEventHandler onScreenPressed;
        private EventHandler onScreenReleased;
        private EventHandler onHomePressed;
        private EventHandler onMenuPressed;
        private EventHandler onBackPressed;
        private GestureDetectedEventHandler onGestureDetected;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="rSocketNumber">The mainboard socket that has the display's R socket connected to it.</param>
        /// <param name="gSocketNumber">The mainboard socket that has the display's G socket connected to it.</param>
        /// <param name="bSocketNumber">The mainboard socket that has the display's B socket connected to it.</param>
        public DisplayCP7(int rSocketNumber, int gSocketNumber, int bSocketNumber) : this(rSocketNumber, gSocketNumber, bSocketNumber, Socket.Unused)
        {

        }

        /// <summary>Constructs a new instance.</summary>
        /// <param name="rSocketNumber">The mainboard socket that has the display's R socket connected to it.</param>
        /// <param name="gSocketNumber">The mainboard socket that has the display's G socket connected to it.</param>
        /// <param name="bSocketNumber">The mainboard socket that has the display's B socket connected to it.</param>
        /// <param name="i2cSocketNumber">The mainboard socket that has the display's I socket connected to it.</param>
        public DisplayCP7(int rSocketNumber, int gSocketNumber, int bSocketNumber, int i2cSocketNumber) : base(WpfMode.PassThrough)
        {
            var config = new DisplayModule.TimingRequirements()
            {
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

            base.OnDisplayConnected("Display CP7", 800, 480, DisplayOrientation.Normal, config);

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
            
             if (i2cSocketNumber == Socket.Unused)
                return;
            
            this.onScreenPressed = this.OnScreenPressed;
            this.onScreenReleased = this.OnScreenReleased;
            this.onHomePressed = this.OnHomePressed;
            this.onMenuPressed = this.OnMenuPressed;
            this.onBackPressed = this.OnBackPressed;
            this.onGestureDetected = this.OnGestureDetected;

            Socket i2cSocket = Socket.GetSocket(i2cSocketNumber, true, this, null);

            this.releaseSent = false;
            this.transactions = new I2CDevice.I2CTransaction[2];
            this.resultBuffer = new byte[1];
            this.addressBuffer = new byte[1];
            this.i2cBus = GTI.I2CBusFactory.Create(i2cSocket, 0x38, 400, this);
            this.touchInterrupt = GTI.InterruptInputFactory.Create(i2cSocket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.PullUp, GTI.InterruptMode.RisingAndFallingEdge, this);
            this.touchInterrupt.Interrupt += (a, b) => this.OnTouchEvent();
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
                this.ErrorPrint("Painting error");
            }
        }

        /// <summary>
        /// The possible gestures.
        /// </summary>
        public enum GestureType
        {
            /// <summary>
            /// A move up gesture
            /// </summary>
            MoveUp = 0x10,

            /// <summary>
            /// A move left gesture
            /// </summary>
            MoveLeft = 0x14,

            /// <summary>
            /// A move down gesture
            /// </summary>
            MoveDown = 0x18,

            /// <summary>
            /// A move right gesture
            /// </summary>
            MoveRight = 0x1C,

            /// <summary>
            /// A zoom in gesture
            /// </summary>
            ZoomIn = 0x48,

            /// <summary>
            /// A zoom out gesture
            /// </summary>
            ZoomOut = 0x49,

            /// <summary>
            /// No gesture detected
            /// </summary>
            None = 0x00
        }

        /// <summary>
        /// Represents a single position
        /// </summary>
        public class Position
        {
            /// <summary>
            /// The x coordinate of the position.
            /// </summary>
            public int X { get; set; }

            /// <summary>
            /// The y coordinate of the position.
            /// </summary>
            public int Y { get; set; }

            /// <summary>
            /// Constructs a new instance.
            /// </summary>
            /// <param name="x">The x coordinate</param>
            /// <param name="y">The y coordinate</param>
            public Position(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        /// <summary>
        /// Event arguments for the ScreenPressed event.
        /// </summary>
        public class TouchEventArgs : EventArgs
        {
            /// <summary>
            /// the detected touches.
            /// </summary>
            public Position[] TouchPoints { get; private set; }

            /// <summary>
            /// The number of touches
            /// </summary>
            public int TouchCount { get; private set; }

            internal TouchEventArgs(int touchCount)
            {
                this.TouchCount = touchCount;
                this.TouchPoints = new Position[touchCount];
            }
        }

        /// <summary>
        /// Event arguments for the GestureDetected event.
        /// </summary>
        public class GestureDetectedEventArgs : EventArgs
        {
            /// <summary>
            /// The detected gesture.
            /// </summary>
            public GestureType Gesture { get; private set; }

            internal GestureDetectedEventArgs(GestureType type)
            {
                this.Gesture = type;
            }
        }

        /// <summary>
        /// The delegate that is used to handle the button pressed and screen released events.
        /// </summary>
        /// <param name="sender">The <see cref="DisplayCP7"/> object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void EventHandler(DisplayCP7 sender, EventArgs e);

        /// <summary>
        /// The delegate that is used to handle the touch events.
        /// </summary>
        /// <param name="sender">The <see cref="DisplayCP7"/> object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void TouchEventHandler(DisplayCP7 sender, TouchEventArgs e);

        /// <summary>
        /// The delegate that is used to handle the GestureDetected event.
        /// </summary>
        /// <param name="sender">The <see cref="DisplayCP7"/> object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void GestureDetectedEventHandler(DisplayCP7 sender, GestureDetectedEventArgs e);

        /// <summary>
        /// Raised when the screen detects a press.
        /// </summary>
        public event TouchEventHandler ScreenPressed;

        /// <summary>
        /// Raised when the screen detects that all touches were released.
        /// </summary>
        public event EventHandler ScreenReleased;

        /// <summary>
        /// Raised when the screen detects a touch on the home button.
        /// </summary>
        public event EventHandler HomePressed;

        /// <summary>
        /// Raised when the screen detects a touch on the menu button.
        /// </summary>
        public event EventHandler MenuPressed;

        /// <summary>
        /// Raised when the screen detects a touch on the back button.
        /// </summary>
        public event EventHandler BackPressed;

        /// <summary>
        /// Raised when the screen detects a gesture.
        /// </summary>
        public event GestureDetectedEventHandler GestureDetected;

        private void OnScreenPressed(DisplayCP7 sender, TouchEventArgs e)
        {
            if (Program.CheckAndInvoke(this.ScreenPressed, this.onScreenPressed, sender, e))
                this.ScreenPressed(sender, e);
        }

        private void OnScreenReleased(DisplayCP7 sender, EventArgs e)
        {
            if (Program.CheckAndInvoke(this.ScreenReleased, this.onScreenReleased, sender, e))
                this.ScreenReleased(sender, e);
        }

        private void OnHomePressed(DisplayCP7 sender, EventArgs e)
        {
            if (Program.CheckAndInvoke(this.HomePressed, this.onHomePressed, sender, e))
                this.HomePressed(sender, e);
        }

        private void OnMenuPressed(DisplayCP7 sender, EventArgs e)
        {
            if (Program.CheckAndInvoke(this.MenuPressed, this.onMenuPressed, sender, e))
                this.MenuPressed(sender, e);
        }

        private void OnBackPressed(DisplayCP7 sender, EventArgs e)
        {
            if (Program.CheckAndInvoke(this.BackPressed, this.onBackPressed, sender, e))
                this.BackPressed(sender, e);
        }

        private void OnGestureDetected(DisplayCP7 sender, GestureDetectedEventArgs e)
        {
            if (Program.CheckAndInvoke(this.GestureDetected, this.onGestureDetected, sender, e))
                this.GestureDetected(sender, e);
        }

        private void OnTouchEvent()
        {
            int numberOfTouches = this.ReadRegister(0x02) & 0x0F;
            if (numberOfTouches == 0)
            {
                if (!this.releaseSent)
                {
                    this.OnScreenReleased(this, new EventArgs());

                    this.releaseSent = true;
                }

                return;
            }

            this.releaseSent = false;

            var gesture = (GestureType)(this.ReadRegister(0x01) & 0xFF);
            if (gesture != GestureType.None)
            {
                this.OnGestureDetected(this, new GestureDetectedEventArgs(gesture));

                return;
            }

            var positions = new Position[numberOfTouches];
            var actualTouchCount = 0;

            for (int i = 0; i < numberOfTouches; i++)
            {
                int x = ((this.ReadRegister((byte)(3 + i * 6)) & 0x0F) << 8) + this.ReadRegister((byte)(4 + i * 6));
                int y = ((this.ReadRegister((byte)(5 + i * 6)) & 0x0F) << 8) + this.ReadRegister((byte)(6 + i * 6));

                if (x > 800)
                {
                    if (y >= 0 && y <= 50)
                    {
                        this.OnHomePressed(this, new EventArgs());
                    }
                    else if (y >= 100 && y <= 150)
                    {
                        this.OnMenuPressed(this, new EventArgs());
                    }
                    else if (y >= 200 && y <= 250)
                    {
                        this.OnBackPressed(this, new EventArgs());
                    }
                }
                else
                {
                    actualTouchCount++;
                    positions[i] = new Position(x, y);
                }
            }

            TouchEventArgs e = new TouchEventArgs(actualTouchCount);
            Array.Copy(positions, e.TouchPoints, actualTouchCount);
            this.OnScreenPressed(this, e);
        }

        private byte ReadRegister(byte address)
        {
            this.addressBuffer[0] = address;

            this.transactions[0] = I2CDevice.CreateWriteTransaction(this.addressBuffer);
            this.transactions[1] = I2CDevice.CreateReadTransaction(this.resultBuffer);

            if (this.i2cBus.Execute(this.transactions) == 0)
                Debug.Print("Failed to perform I2C transaction");

            return this.resultBuffer[0];
        }
    }
}
