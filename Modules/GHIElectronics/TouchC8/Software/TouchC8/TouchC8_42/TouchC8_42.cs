using System.Threading;
using Microsoft.SPOT.Hardware;
using GT = Gadgeteer;
using GTI = Gadgeteer.Interfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A multi-function touch sensor for .Net Gadgeteer.
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
		public enum Buttons
		{
			/// <summary>
			/// Button A
			/// </summary>
			A = 0x2,
			/// <summary>
			/// Button B
			/// </summary>
			B = 0x4,
			/// <summary>
			/// Button C
			/// </summary>
			C = 0x8
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
		/// Delegate representing the proximity detected event.
		/// </summary>
		/// <param name="sender">The sensor that the detection occured on.</param>
		/// <param name="state">Whether or not an object is near the sensor.</param>
		public delegate void PromixityDetectedHandler(TouchC8 sender, bool state);

		/// <summary>
		/// Delegate representing the button touch event.
		/// </summary>
		/// <param name="sender">The sensor that the event occured on.</param>
		/// <param name="button">The button that the event occured on.</param>
		/// <param name="state">Whether or not the button was pressed or released.</param>
		public delegate void ButtonTouchHandler(TouchC8 sender, Buttons button, bool state);

		/// <summary>
		/// Delegate representing the wheel touch event.
		/// </summary>
		/// <param name="sender">The sensor that the event occured on.</param>
		/// <param name="state">Whether or not the wheel was pressed or released.</param>
		public delegate void WheelTouchHandler(TouchC8 sender, bool state);

		/// <summary>
		/// Delegate representing the wheel position changed event.
		/// </summary>
		/// <param name="sender">The sensor that the event occured on.</param>
		/// <param name="position">The position of the touch on the wheel.</param>
		/// <param name="direction">The direction of the touch on the wheel.</param>
		public delegate void WheelPositionChangedHandler(TouchC8 sender, double position, Direction direction);

		/// <summary>
		/// Fires when the proximity sensor detects an object.
		/// </summary>
		public event PromixityDetectedHandler OnProximityEnter;

		/// <summary>
		/// Fires when the proximity sensor no longer detects an object after detecting one.
		/// </summary>
		public event PromixityDetectedHandler OnProximityExit;

		/// <summary>
		/// Fires when a button is pressed.
		/// </summary>
		public event ButtonTouchHandler OnButtonPressed;

		/// <summary>
		/// Fires when a button is released.
		/// </summary>
		public event ButtonTouchHandler OnButtonReleased;

		/// <summary>
		/// Fires when the wheel is pressed.
		/// </summary>
		public event WheelTouchHandler OnWheelPressed;

		/// <summary>
		/// Fires when the wheel is released.
		/// </summary>
		public event WheelTouchHandler OnWheelReleased;

		/// <summary>
		/// Fires when the position of the touch on the wheel changes.
		/// </summary>
		public event WheelPositionChangedHandler OnWheelPositionChanged;

		private PromixityDetectedHandler OnProximity;
		private ButtonTouchHandler OnButton;
		private WheelTouchHandler OnWheel;
		private WheelPositionChangedHandler OnWheelPosition;

		private void OnProximityEvent(TouchC8 sender, bool state)
		{
			if (this.OnProximity == null)
				this.OnProximity = new PromixityDetectedHandler(this.OnProximityEvent);

			if (Program.CheckAndInvoke(state ? this.OnProximityEnter : this.OnProximityExit, this.OnProximity, sender, state))
			{
				if (state)
					this.OnProximityEnter(sender, state);
				else
					this.OnProximityExit(sender, state);
			}
		}

		private void OnButtonEvent(TouchC8 sender, Buttons button, bool state)
		{
			if (this.OnButton == null)
				this.OnButton = new ButtonTouchHandler(this.OnButtonEvent);

			if (Program.CheckAndInvoke(state ? this.OnButtonPressed : this.OnButtonReleased, this.OnButton, sender, button, state))
			{
				if (state)
					this.OnButtonPressed(sender, button, state);
				else
					this.OnButtonReleased(sender, button, state);
			}
		}

		private void OnWheelEvent(TouchC8 sender, bool state)
		{
			if (this.OnWheel == null)
				this.OnWheel = new WheelTouchHandler(this.OnWheelEvent);

			if (Program.CheckAndInvoke(state ? this.OnWheelPressed : this.OnWheelReleased, this.OnWheel, sender, state))
			{
				if (state)
					this.OnWheelPressed(sender, state);
				else
					this.OnWheelReleased(sender, state);
			}
		}

		private void OnWheelPositionEvent(TouchC8 sender, double position, Direction direction)
		{
			if (this.OnWheelPosition == null)
				this.OnWheelPosition = new WheelPositionChangedHandler(this.OnWheelPositionEvent);

			if (Program.CheckAndInvoke(this.OnWheelPositionChanged, this.OnWheelPosition, sender, position, direction))
				this.OnWheelPositionChanged(sender, position, direction);
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

			Thread.Sleep(1000);

			this.reset = new GTI.DigitalOutput(this.socket, GT.Socket.Pin.Six, true, this);

			Thread.Sleep(1000);

			this.Reset();

			Thread.Sleep(1000);
			this.device = new GTI.I2CBus(this.socket, TouchC8.I2C_ADDRESS, TouchC8.I2C_CLOCK_RATE, this);

			Thread.Sleep(1000);

			this.interrupt = new GTI.InterruptInput(socket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.PullUp, GTI.InterruptMode.FallingEdge, this);
			this.interrupt.Interrupt += new GTI.InterruptInput.InterruptEventHandler(OnInterrupt);

			this.previousWheelDirection = (Direction)(-1);
			this.previousWheelPosition = 0;
			this.previousWheelTouched = false;
			this.previousButton1Touched = false;
			this.previousButton2Touched = false;
			this.previousButton3Touched = false;

			Thread.Sleep(1000);

			this.ConfigureSPM();
		}

		/// <summary>
		/// Gets whether or not the given button is being touched by the user
		/// </summary>
		/// <param name="button">The button to check.</param>
		/// <returns>Whether or not the given button is being touched.</returns>
		public bool IsButtonPressed(Buttons button)
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
					this.OnWheelEvent(this, wheelTouched);

				if (wheelPosition != this.previousWheelPosition || wheelDirection != this.previousWheelDirection)
					this.OnWheelPositionEvent(this, wheelPosition, wheelDirection);

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
					this.OnProximityEvent(this, button0);

				if (button1 != this.previousButton1Touched)
					this.OnButtonEvent(this, Buttons.A, button1);

				if (button2 != this.previousButton2Touched)
					this.OnButtonEvent(this, Buttons.B, button2);

				if (button3 != this.previousButton3Touched)
					this.OnButtonEvent(this, Buttons.C, button3);

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
			this.device.Write(Utility.CombineArrays(this.addressBuffer, values), 100); //needs to begin with a 0 representing the I2C address of 0.

			this.WriteRegister(0x0D, 0x00);
		}

		private void WriteRegister(byte address, byte value)
		{
			this.writeBuffer[0] = address;
			this.writeBuffer[1] = value;
			this.device.Write(this.writeBuffer, 100);
		}

		private byte ReadRegister(byte address)
		{
			this.addressBuffer[0] = address;
			this.device.WriteRead(this.addressBuffer, this.readBuffer, 100);
			return this.readBuffer[0];
		}
	}
}
