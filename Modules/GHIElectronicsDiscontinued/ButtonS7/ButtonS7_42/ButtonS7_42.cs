using GT = Gadgeteer;
using GTI = Gadgeteer.Interfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A ButtonS7 module for Microsoft .NET Gadgeteer
	/// </summary>
	public class ButtonS7 : GTM.Module
	{
		private GTI.InterruptInput enter;
		private GTI.DigitalInput[] buttons;
		private EnterEventHandler enterEvent;

		/// <summary>Constructs a new ButtonS7 instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public ButtonS7(int socketNumber)
		{
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('Y', this);

			this.buttons = new GTI.DigitalInput[6];
			for (int i = 0; i < 6; i++)
				this.buttons[i] = new GTI.DigitalInput(socket, (Socket.Pin)(i + 4), GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, this);

			this.enter = new GTI.InterruptInput(socket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.On, GTI.ResistorMode.Disabled, GTI.InterruptMode.RisingAndFallingEdge, this);
			this.enter.Interrupt += this.OnInterrupt;
		}

		private void OnInterrupt(GTI.InterruptInput input, bool value)
		{
			this.OnEnterEvent(this, value ? EnterStates.Released : EnterStates.Pressed);
		}

		/// <summary>
		/// Gets a value that indicates whether the given button of the ButtonS7 is pressed.
		/// </summary>
		public bool this[Buttons button]
		{
			get
			{
				if (button == Buttons.Enter)
					return this.enter.Read();

				return this.buttons[(int)button].Read();
			}
		}

		/// <summary>
		/// Gets a value that indicates whether the given button of the ButtonS7 is pressed.
		/// </summary>
		public bool IsPressed(Buttons button)
		{
			if (button == Buttons.Enter)
				return !this.enter.Read();

			return !this.buttons[(int)button].Read();
		}

		/// <summary>
		/// Represents the buttons of the <see cref="ButtonS7"/>.
		/// </summary>
		public enum Buttons
		{
			/// <summary>
			/// The back button.
			/// </summary>
			Back = 0,
			/// <summary>
			/// The left button.
			/// </summary>
			Left,
			/// <summary>
			/// The up button.
			/// </summary>
			Up,
			/// <summary>
			/// The down button.
			/// </summary>
			Down,
			/// <summary>
			/// The right button.
			/// </summary>
			Right,
			/// <summary>
			/// The forward button.
			/// </summary>
			Forward,
			/// <summary>
			/// The enter button.
			/// </summary>
			Enter
		}

		/// <summary>
		/// Represents the state of the enter button of the <see cref="ButtonS7"/>.
		/// </summary>
		public enum EnterStates
		{
			/// <summary>
			/// The button is released.
			/// </summary>
			Released = 0,
			/// <summary>
			/// The button is pressed.
			/// </summary>
			Pressed = 1
		}

		/// <summary>
		/// Represents the delegate that is used to handle the <see cref="EnterPressed"/>
		/// and <see cref="EnterReleased"/> events.
		/// </summary>
		/// <param name="sender">The <see cref="ButtonS7"/> object that raised the event.</param>
		/// <param name="state">The state of the button of the <see cref="ButtonS7"/></param>
		public delegate void EnterEventHandler(ButtonS7 sender, EnterStates state);

		/// <summary>
		/// Raised when the button of the <see cref="ButtonS7"/> is pressed.
		/// </summary>
		/// <remarks>
		/// Implement this event handler and/or the <see cref="EnterReleased"/> event handler
		/// when you want to provide an action associated with button events.
		/// Since the state of the button is passed to the <see cref="EnterEventHandler"/> delegate,
		/// so you can use the same event handler for both button states.
		/// </remarks>
		public event EnterEventHandler EnterPressed;

		/// <summary>
		/// Raised when the button of the <see cref="ButtonS7"/> is released.
		/// </summary>
		/// <remarks>
		/// Implement this event handler and/or the <see cref="EnterPressed"/> event handler
		/// when you want to provide an action associated with button events.
		/// Since the state of the button is passed to the <see cref="EnterEventHandler"/> delegate,
		/// you can use the same event handler for both button states.
		/// </remarks>
		public event EnterEventHandler EnterReleased;

		/// <summary>
		/// Raises the <see cref="EnterPressed"/> or <see cref="EnterReleased"/> event.
		/// </summary>
		/// <param name="sender">The <see cref="ButtonS7"/> that raised the event.</param>
		/// <param name="buttonState">The state of the button.</param>
		protected virtual void OnEnterEvent(ButtonS7 sender, EnterStates buttonState)
		{
			if (this.enterEvent == null)
				this.enterEvent = new EnterEventHandler(this.OnEnterEvent);

			if (buttonState == EnterStates.Pressed)
			{
				if (Program.CheckAndInvoke(this.EnterPressed, this.enterEvent, sender, buttonState))
					this.EnterPressed(sender, buttonState);
			}
			else
			{
				if (Program.CheckAndInvoke(this.EnterReleased, this.enterEvent, sender, buttonState))
					this.EnterReleased(sender, buttonState);
			}
		}
	}
}
