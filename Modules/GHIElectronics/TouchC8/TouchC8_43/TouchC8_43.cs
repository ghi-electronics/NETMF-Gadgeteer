using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A TouchC8 module for Microsoft .NET Gadgeteer
	/// </summary>
	public class TouchC8 : GT.Modules.Module
	{
		private const ushort I2C_ADDRESS = 0x2B;
		private const int I2C_CLOCK_RATE = 100;

		private const byte IRQ_SRC = 0x0;
		private const byte CAP_STAT_MSB = 0x1;
		private const byte CAP_STAT_LSB = 0x2;
		private const byte WHL_POS_MSB = 0x3;
		private const byte WHL_POS_LSB = 0x4;

		private const byte WHEELS = 8;

		private GT.Socket socket;
		private GTI.I2CBus device;
		private GTI.InterruptInput interrupt;
		private GTI.DigitalOutput reset;

		private byte[] readBuffer;
		private byte[] writeBuffer;
		private byte[] addressBuffer;

		private Direction previousWheelDirection;
		private double previousWheelPosition;
		private bool previousWheelTouched;
		private bool previousButton0Touched;
		private bool previousButton1Touched;
		private bool previousButton2Touched;
		private bool previousButton3Touched;

		/// <summary>
		/// Represents the buttons on the sensor.
		/// </summary>
		public enum Button
		{
			/// <summary>
			/// The up button.
			/// </summary>
			Up = 0x2,
			/// <summary>
			/// The middle button.
			/// </summary>
			Middle = 0x4,
			/// <summary>
			/// The down button.
			/// </summary>
			Down = 0x8
		}

		/// <summary>
		/// Represents the direction of motion on the wheel.
		/// </summary>
		public enum Direction
		{
			/// <summary>
			/// Clockwise motion
			/// </summary>
			Clockwise,
			/// <summary>
			/// Counterclockwise motion
			/// </summary>
			Counterclockwise
		}

        /// <summary>
        /// Event arguments for the WheelPositionChanged event.
        /// </summary>
        public class WheelPositionChangedEventArgs : EventArgs
        {
            /// <summary>
            /// The position of the touch on the wheel between 0 and 360 degrees.
            /// </summary>
            public double Position { get; private set; }

            /// <summary>
            /// The direction of movement on the wheel.
            /// </summary>
            public Direction Direction { get; private set; }

            internal WheelPositionChangedEventArgs(Direction direction, double position)
            {
                this.Direction = direction;
                this.Position = position;
            }
        }

        /// <summary>
        /// Event arguments for the ButtonTouched event.
        /// </summary>
        public class ButtonTouchedEventArgs : EventArgs
        {
            /// <summary>
            /// The new state of the button.
            /// </summary>
            public bool State { get; private set; }

            /// <summary>
            /// The button pressed or released.
            /// </summary>
            public Button Button { get; private set; }

            internal ButtonTouchedEventArgs(Button button, bool state)
            {
                this.Button = button;
                this.State = state;
            }
        }

        /// <summary>
        /// Event arguments for the PromixityDetected event.
        /// </summary>
        public class PromixityDetectedEventArgs : EventArgs
        {
            /// <summary>
            /// Whether or not the proximity was detected.
            /// </summary>
            public bool State { get; private set; }

            internal PromixityDetectedEventArgs(bool state)
            {
                this.State = state;
            }
        }

        /// <summary>
        /// Event arguments for the WheelTouched event.
        /// </summary>
        public class WheelTouchedEventArgs : EventArgs
        {
            /// <summary>
            /// Whether or not the wheel was touched.
            /// </summary>
            public bool State { get; private set; }

            internal WheelTouchedEventArgs(bool state)
            {
                this.State = state;
            }
        }

		/// <summary>
		/// Delegate representing the proximity detected event.
		/// </summary>
		/// <param name="sender">The sensor that the detection occured on.</param>
		/// <param name="e">The event arguments.</param>
        public delegate void PromixityDetectedHandler(TouchC8 sender, PromixityDetectedEventArgs e);

		/// <summary>
		/// Delegate representing the button touch event.
		/// </summary>
        /// <param name="sender">The sensor that the event occured on.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void ButtonTouchedHandler(TouchC8 sender, ButtonTouchedEventArgs e);

		/// <summary>
		/// Delegate representing the wheel touch event.
		/// </summary>
        /// <param name="sender">The sensor that the event occured on.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void WheelTouchedHandler(TouchC8 sender, WheelTouchedEventArgs e);

		/// <summary>
		/// Delegate representing the wheel position changed event.
		/// </summary>
        /// <param name="sender">The sensor that the event occured on.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void WheelPositionChangedHandler(TouchC8 sender, WheelPositionChangedEventArgs e);

		/// <summary>
		/// Fires when the proximity sensor detects an object.
		/// </summary>
		public event PromixityDetectedHandler ProximityEnter;

		/// <summary>
		/// Fires when the proximity sensor no longer detects an object after detecting one.
		/// </summary>
		public event PromixityDetectedHandler ProximityExit;

		/// <summary>
		/// Fires when a button is pressed.
		/// </summary>
		public event ButtonTouchedHandler ButtonPressed;

		/// <summary>
		/// Fires when a button is released.
		/// </summary>
		public event ButtonTouchedHandler ButtonReleased;

		/// <summary>
		/// Fires when the wheel is pressed.
		/// </summary>
		public event WheelTouchedHandler WheelPressed;

		/// <summary>
		/// Fires when the wheel is released.
		/// </summary>
		public event WheelTouchedHandler WheelReleased;

		/// <summary>
		/// Fires when the position of the touch on the wheel changes.
		/// </summary>
		public event WheelPositionChangedHandler WheelPositionChanged;

		private PromixityDetectedHandler onProximityDetected;
		private ButtonTouchedHandler onButtonTouched;
		private WheelTouchedHandler onWheelTouched;
		private WheelPositionChangedHandler onWheelPositionChanged;

        private void OnProximityDetected(TouchC8 sender, PromixityDetectedEventArgs e)
		{
			if (this.onProximityDetected == null)
				this.onProximityDetected = this.OnProximityDetected;

			if (Program.CheckAndInvoke(e.State ? this.ProximityEnter : this.ProximityExit, this.onProximityDetected, sender, e))
			{
                if (e.State)
					this.ProximityEnter(sender, e);
				else
					this.ProximityExit(sender, e);
			}
		}

        private void OnButtonTouched(TouchC8 sender, ButtonTouchedEventArgs e)
		{
			if (this.onButtonTouched == null)
				this.onButtonTouched = this.OnButtonTouched;

            if (Program.CheckAndInvoke(e.State ? this.ButtonPressed : this.ButtonReleased, this.onButtonTouched, sender, e))
			{
                if (e.State)
					this.ButtonPressed(sender, e);
				else
					this.ButtonReleased(sender, e);
			}
		}

        private void OnWheelTouched(TouchC8 sender, WheelTouchedEventArgs e)
		{
			if (this.onWheelTouched == null)
				this.onWheelTouched = this.OnWheelTouched;

            if (Program.CheckAndInvoke(e.State ? this.WheelPressed : this.WheelReleased, this.onWheelTouched, sender, e))
			{
                if (e.State)
					this.WheelPressed(sender, e);
				else
					this.WheelReleased(sender, e);
			}
		}

        private void OnWheelPositionChanged(TouchC8 sender, WheelPositionChangedEventArgs e)
		{
			if (this.onWheelPositionChanged == null)
                this.onWheelPositionChanged = this.OnWheelPositionChanged;

            if (Program.CheckAndInvoke(this.WheelPositionChanged, this.onWheelPositionChanged, sender, e))
				this.WheelPositionChanged(sender, e);
		}

		/// <summary>
		/// Constructs a new TouchC8 sensor.
		/// </summary>
		/// <param name="socketNumber">The socket number the sensor is plugged into.</param>
		public TouchC8(int socketNumber)
		{
			this.readBuffer = new byte[1];
			this.writeBuffer = new byte[2];
			this.addressBuffer = new byte[1];

			this.socket = GT.Socket.GetSocket(socketNumber, false, this, "I");
			this.reset = GTI.DigitalOutputFactory.Create(this.socket, GT.Socket.Pin.Six, true, this);

			this.Reset();

			this.device = GTI.I2CBusFactory.Create(this.socket, TouchC8.I2C_ADDRESS, TouchC8.I2C_CLOCK_RATE, this);

			this.interrupt = GTI.InterruptInputFactory.Create(socket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.PullUp, GTI.InterruptMode.FallingEdge, this);
			this.interrupt.Interrupt += this.OnInterrupt;

			this.previousWheelDirection = (Direction)(-1);
			this.previousWheelPosition = 0;
			this.previousWheelTouched = false;
			this.previousButton1Touched = false;
			this.previousButton2Touched = false;
			this.previousButton3Touched = false;

			Thread.Sleep(250);

			this.ConfigureSPM();
		}

		/// <summary>
		/// Gets whether or not the given button is being touched by the user
		/// </summary>
		/// <param name="button">The button to check.</param>
		/// <returns>Whether or not the given button is being touched.</returns>
		public bool IsButtonPressed(Button button)
		{
			return (this.ReadRegister(TouchC8.CAP_STAT_LSB) & (byte)button) != 0;
		}

		/// <summary>
		/// Gets whether or not the wheel is being touched by the user.
		/// </summary>
		/// <returns>Whether or not the wheel is being touched.</returns>
		public bool IsWheelPressed()
		{
			return (this.ReadRegister(TouchC8.CAP_STAT_MSB) & 0x10) != 0;
		}

		/// <summary>
		/// Gets whether or not the proximity detector is detecting a nearby object.
		/// </summary>
		/// <returns>Whether or not the sensor is detecting anything.</returns>
		public bool IsProximityDetected()
		{
			return (this.ReadRegister(TouchC8.CAP_STAT_LSB) & 0x1) != 0;
		}

		/// <summary>
		/// Gets the current position of the user on the wheel in degrees.
		/// </summary>
		/// <returns>The degree measure of the user's location on the wheel between 0 and 360.</returns>
		public double GetWheelPosition()
		{
			ushort whlLsb = this.ReadRegister(TouchC8.WHL_POS_LSB);
			ushort whlMsb = this.ReadRegister(TouchC8.WHL_POS_MSB);
			double wheelPosition = (double)((whlMsb << 8) + whlLsb);
			return wheelPosition * (360.0 / (10.0 * TouchC8.WHEELS));
		}

		/// <summary>
		/// Gets the current direction the user is moving on the wheel.
		/// </summary>
		/// <returns>The direction the user is moving.</returns>
		public Direction GetWheelDirection()
		{
			return (this.ReadRegister(TouchC8.CAP_STAT_MSB) & 0x40) != 0 ? Direction.Clockwise : Direction.Counterclockwise;
		}

		private void Reset()
		{
			Thread.Sleep(100);
			this.reset.Write(false);
			Thread.Sleep(100);
			this.reset.Write(true);
			Thread.Sleep(100);
		}

		private void OnInterrupt(GTI.InterruptInput sender, bool value)
		{
			byte flags = this.ReadRegister(TouchC8.IRQ_SRC);

			if ((flags & 0x8) != 0)
			{
				double wheelPosition = this.GetWheelPosition();
				bool wheelTouched = this.IsWheelPressed();
				Direction wheelDirection = this.GetWheelDirection();

				if (wheelTouched != this.previousWheelTouched)
					this.OnWheelTouched(this, new WheelTouchedEventArgs(wheelTouched));

				if (wheelPosition != this.previousWheelPosition || wheelDirection != this.previousWheelDirection)
                    this.WheelPositionChanged(this, new WheelPositionChangedEventArgs(wheelDirection, wheelPosition));

				this.previousWheelTouched = wheelTouched;
				this.previousWheelPosition = wheelPosition;
				this.previousWheelDirection = wheelDirection;
			}

			if ((flags & 0x4) != 0)
			{
				byte cap = this.ReadRegister(TouchC8.CAP_STAT_LSB);
				bool button0 = (cap & 0x1) != 0; //proximity
				bool button1 = (cap & 0x2) != 0;
				bool button2 = (cap & 0x4) != 0;
				bool button3 = (cap & 0x8) != 0;

				if (button0 != this.previousButton0Touched)
					this.OnProximityDetected(this, new PromixityDetectedEventArgs(button0));

				if (button1 != this.previousButton1Touched)
					this.OnButtonTouched(this, new ButtonTouchedEventArgs(Button.Up, button1));

				if (button2 != this.previousButton2Touched)
					this.OnButtonTouched(this, new ButtonTouchedEventArgs(Button.Middle, button2));

				if (button3 != this.previousButton3Touched)
                    this.OnButtonTouched(this, new ButtonTouchedEventArgs(Button.Down, button3));

				this.previousButton0Touched = button0;
				this.previousButton1Touched = button1;
				this.previousButton2Touched = button2;
				this.previousButton3Touched = button3;
			}
		}

		private void ConfigureSPM()
		{
			//SPM must be written in 8 byte segments, so that is why we have the extra stuff below.

			//0x4 is cap sensitivity mode, 0xFF to 0x55 are the cap type modes, 0x70 is cap 0 and 1 sensitivty, the last 3 0's set cap 2-7 sensitivity.
			this.WriteSPM(0x9, new byte[] { 0x4, 0xFF, 0xFF, 0x55, 0x70, 0x0, 0x0, 0x0 });

			//0x74 is to enable proximity sensing and the remaining values are the default reserved values that we cannot change
			this.WriteSPM(0x70, new byte[] { 0x74, 0x10, 0x45, 0x2, 0xFF, 0xFF, 0xFF, 0xD5 });
		}

		private void WriteSPM(byte address, byte[] values)
		{
			this.WriteRegister(0x0D, 0x10);
			this.WriteRegister(0x0E, address);

			this.addressBuffer[0] = 0;
			this.device.Write(Utility.CombineArrays(this.addressBuffer, values)); //needs to begin with a 0 representing the I2C address of 0.

			this.WriteRegister(0x0D, 0x00);
		}

		private void WriteRegister(byte address, byte value)
		{
			this.writeBuffer[0] = address;
			this.writeBuffer[1] = value;
			this.device.Write(this.writeBuffer);
		}

		private byte ReadRegister(byte address)
		{
			this.addressBuffer[0] = address;
			this.device.WriteRead(this.addressBuffer, this.readBuffer);
			return this.readBuffer[0];
		}
	}
}
