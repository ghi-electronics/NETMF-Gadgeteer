using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A DistanceUS3 module for Microsoft .NET Gadgeteer
	/// </summary>
	[Obsolete]
	public class DistanceUS3 : GTM.Module
	{
		private GTI.DigitalInput echo;
		private GTI.DigitalOutput trigger;

		private const int MIN_DISTANCE = 2;
		private const int MAX_DISTANCE = 400;
		private const int MIN_FLAG = -2;
		private const int MAX_FLAG = -1;

		/// <summary>
		/// The value that will be returned when the sensor failed to take an accurate reading.
		/// </summary>
		public static int SensorError { get { return -1; } }

		/// <summary>
		/// The number of errors to encounter before returning SENSOR_ERROR.
		/// </summary>
		public int AcceptableErrors { get; set; }

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public DistanceUS3(int socketNumber)
		{
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported(new char[] { 'X', 'Y' }, this);

			this.echo = GTI.DigitalInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, this);
			this.trigger = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Four, false, this);

			this.AcceptableErrors = 10;
		}

		/// <summary>
		/// Takes a number of measurements and returns the average in centimeters.
		/// </summary>
		/// <returns>The averaged distance or SensorError.</returns>
		public double GetDistance()
		{
			return this.GetDistance(1);
		}

		/// <summary>
		/// Takes a number of measurements and returns the average in centimeters.
		/// </summary>
		/// <param name="measurements">The number of measurements to take and average.</param>
		/// <returns>The averaged distance or SensorError.</returns>
		public double GetDistance(int measurements)
		{
			var sum = 0.0;
			var errorCount = 0;

			for (var i = 0; i < measurements; i++)
			{
				var value = this.GetDistanceHelper();

				if (value >= DistanceUS3.MIN_DISTANCE && value <= DistanceUS3.MAX_DISTANCE)
				{
					sum += value;
				}
				else
				{
					errorCount++;
					i--;

					if (errorCount > this.AcceptableErrors)
						return DistanceUS3.SensorError;
				}
			}

			return sum / measurements;
		}

		private double GetDistanceHelper()
		{
			this.trigger.Write(false);
			Thread.Sleep(1);

			this.trigger.Write(true);
			Thread.Sleep(1);

			this.trigger.Write(false);

			var error = 0;
			while (!this.echo.Read())
				if (error++ > 1000)
					return DistanceUS3.SensorError;

			var start = Utility.GetMachineTime().Ticks;

			error = 0;
			while (this.echo.Read())
				if (error++ > 1000)
					return DistanceUS3.SensorError;

			var end = Utility.GetMachineTime().Ticks;

			return (end - start) / (TimeSpan.TicksPerMillisecond / 1000.0) / 58.0;
		}
	}
}