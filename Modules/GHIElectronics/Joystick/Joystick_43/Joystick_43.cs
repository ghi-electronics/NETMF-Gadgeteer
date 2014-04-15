using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A Joystick module for .NET Gadgeteer.
    /// </summary>
    public class Joystick : GTM.Module
    {
        private GTI.AnalogInput inputX;
        private GTI.AnalogInput inputY;
        private GTI.InterruptInput input;
		private double offsetX;
		private double offsetY;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The mainboard socket that has the module plugged into it.</param>
        public Joystick(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported(new char[] { 'X', 'Y' }, this);

            this.inputX = GTI.AnalogInputFactory.Create(socket, Socket.Pin.Four, this);
            this.inputY = GTI.AnalogInputFactory.Create(socket, Socket.Pin.Five, this);
            this.input = GTI.InterruptInputFactory.Create(socket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.On, GTI.ResistorMode.PullUp, GTI.InterruptMode.RisingAndFallingEdge, this);
            this.input.Interrupt += (a, b) => this.OnJoystickEvent(this, b ? ButtonState.Released : ButtonState.Pressed);

			this.offsetX = 0;
			this.offsetY = 0;
        }

        /// <summary>
        /// Whether or not the joystick is pressed.
        /// </summary>
        public bool IsPressed
        {
            get
            {
                return !this.input.Read();
            }
        }

        /// <summary>
        /// Represents the state of the <see cref="Joystick"/> object.
        /// </summary>
        public enum ButtonState
        {
            /// <summary>
            /// The state of Joystick is Pressed.
            /// </summary>
            Pressed = 0,
            /// <summary>
            /// The state of Joystick is Released.
            /// </summary>
            Released = 1
        }

        /// <summary>
        /// Represents the delegate that is used to handle the <see cref="JoystickReleased"/> and <see cref="JoystickPressed"/> events.
        /// </summary>
        /// <param name="sender">The <see cref="Joystick"/> object that raised the event.</param>
        /// <param name="state">The state of the Joystick.</param>
        public delegate void JoystickEventHandler(Joystick sender, ButtonState state);

        /// <summary>
        /// Raised when the joystick is released.
        /// </summary>
        public event JoystickEventHandler JoystickReleased;

        /// <summary>
        /// Raised when the joystick is pressed.
        /// </summary>
        public event JoystickEventHandler JoystickPressed;

        private JoystickEventHandler onJoystickEvent;

        private void OnJoystickEvent(Joystick sender, ButtonState state)
        {
            if (this.onJoystickEvent == null)
                this.onJoystickEvent = this.OnJoystickEvent;

            if (Program.CheckAndInvoke(state == ButtonState.Released ? this.JoystickReleased : this.JoystickPressed, this.onJoystickEvent, sender, state))
            {
                switch (state)
                {
                    case ButtonState.Released: this.JoystickReleased(sender, state); break;
                    case ButtonState.Pressed: this.JoystickPressed(sender, state); break;
                }

            }
        }

        /// <summary>
        /// Gets position of the joystick.
        /// </summary>
        /// <returns>The position.</returns>
        public Position GetPosition()
        {
            double x = this.Read(this.inputX);
            double y = this.Read(this.inputY);

            return new Position()
            {
                X = x * 2 - 1 - this.offsetX,
                Y = (1 - y) * 2 - 1 - this.offsetY
            };
        }

		/// <summary>
		/// Calibrates the joystick such that the current position is interpreted as 0.
		/// </summary>
		public void Calibrate()
		{
			this.offsetX = this.Read(this.inputX) * 2 - 1;
			this.offsetY = (1 - this.Read(this.inputY)) * 2 - 1;
		}

        /// <summary>
        /// Structure that contains the X and Y position of the joystick from -1.0 to 1.0 (0.0 means centered).
        /// </summary>
        public struct Position
        {
            /// <summary>
            /// The X coordinate of the joystick from -1.0 to 1.0 (0.0 means centered).
            /// </summary>
            public double X { get; set; }

            /// <summary>
            /// The Y coordinate of the joystick from -1.0 to 1.0 (0.0 means centered).
            /// </summary>
            public double Y { get; set; }
        }

		private double Read(GTI.AnalogInput input)
		{
			double total = 0;

			for (int i = 0; i < 10; i++)
				total += input.ReadProportion();

			return total / 10;
		}
    }
}