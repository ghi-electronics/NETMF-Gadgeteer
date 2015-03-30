using Microsoft.SPOT;
using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A PIR module for Microsoft .NET Gadgeteer</summary>
	[Obsolete]
	public class PIR : GTM.Module {
		private GTI.InterruptInput interrupt;
		private MotionEventHandler onMotionSensed;

		/// <summary>Represents the delegate that is used to handle the <see cref="MotionSensed" /> event.</summary>
		/// <param name="sender">The <see cref="PIR" /> object that raised the event.</param>
		/// <param name="e">The event arguments.</param>
		public delegate void MotionEventHandler(PIR sender, EventArgs e);

		/// <summary>Raised when the state of <see cref="PIR" /> is high.</summary>
		public event MotionEventHandler MotionSensed;

		/// <summary>Whether or not the sensor is still high after detecthing motion.</summary>
		public bool SensorStillActive {
			get {
				return this.interrupt.Read();
			}
		}

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public PIR(int socketNumber) {
			var socket = Socket.GetSocket(socketNumber, true, this, null);

			this.onMotionSensed = this.OnMotionSensed;

			this.interrupt = GTI.InterruptInputFactory.Create(socket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.On, GTI.ResistorMode.PullUp, GTI.InterruptMode.RisingAndFallingEdge, this);
			this.interrupt.Interrupt += (a, b) => {
				if (!b)
					this.OnMotionSensed(this, null);
			};
		}

		private void OnMotionSensed(PIR sender, EventArgs e) {
			if (Program.CheckAndInvoke(this.MotionSensed, this.onMotionSensed, sender, e))
				this.MotionSensed(sender, e);
		}
	}
}