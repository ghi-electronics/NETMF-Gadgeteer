using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A PIR module for Microsoft .NET Gadgeteer
    /// </summary>
    [Obsolete]
    public class PIR : GTM.Module
    {
		private GTI.InterruptInput interrupt;
		private MotionEventHandler onMotionSensed;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public PIR(int socketNumber)
        {
            var socket = Socket.GetSocket(socketNumber, true, this, null);

			this.onMotionSensed = this.OnMotionSensed;

            this.interrupt = GTI.InterruptInputFactory.Create(socket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.On, GTI.ResistorMode.PullUp, GTI.InterruptMode.RisingAndFallingEdge, this);
			this.interrupt.Interrupt += (a, b) => this.OnMotionSensed(this, b ? State.Ready : State.Busy);
        }

        /// <summary>
        /// Whether or not the sensor is still high after detecthing motion.
        /// </summary>
        public State SensorState
        {
            get
            {
                return this.interrupt.Read() ? State.Busy : State.Ready;
            }
        }

        /// <summary>
        /// Represents the state of the sensor.
        /// </summary>
        public enum State
        {
            /// <summary>
            /// The sensor is ready to begin detecting motion.
            /// </summary>
            Ready = 0,

			/// <summary>
			/// The sensor is not ready to begin detecting motion.
            /// </summary>
            Busy = 1
        }

        /// <summary>
        /// Represents the delegate that is used to handle the <see cref="MotionSensed"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="PIR"/> object that raised the event.</param>
        /// <param name="state">The state of the Motion_Sensor</param>
        public delegate void MotionEventHandler(PIR sender, State state);

        /// <summary>
        /// Raised when the state of <see cref="PIR"/> is high.
        /// </summary>
        /// <remarks>
        /// Implement this event handler when you want to provide an action associated with Motion_Sensor activity.
        /// The state of the Motion_Sensor is passed to the <see cref="MotionEventHandler"/> delegate,
        /// so you can use the same event handler for both Motion_Sensor states.
        /// </remarks>
        public event MotionEventHandler MotionSensed;

        private void OnMotionSensed(PIR sender, State state)
        {
			if (Program.CheckAndInvoke(this.MotionSensed, this.onMotionSensed, sender, state))
				this.MotionSensed(sender, state);
        }
    }
}