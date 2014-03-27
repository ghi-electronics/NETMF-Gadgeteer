using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A FEZCerbot Control module for Microsoft .NET Gadgeteer
	/// </summary>
	public class CerbotController : GTM.Module
	{
		private static double MOTOR_BASE_FREQUENCY = 100000;

		private PWM buzzer;

		private AnalogInput leftSensor;
		private AnalogInput rightSensor;
		private OutputPort leftIRLED;
		private OutputPort rightIRLED;

		private ushort ledMask;
		private PWM enableFaderPin;
		private GTI.Spi forwardLEDs;
		private GTI.SpiConfiguration spiConfig;

		private bool leftInverted;
		private bool rightInverted;
		private PWM leftMotor;
		private PWM rightMotor;
		private OutputPort leftMotorDirection;
		private OutputPort rightMotorDirection;

		private uint servoPulseFactor;
		private uint servoMinPulse;
		private uint servoMaxPulse;
		private bool servoConfigured;
		private PWM servo;

		/// <summary>
		/// The reflective sensors on the Cerbot.
		/// </summary>
		public enum ReflectiveSensors
		{
			/// <summary>
			/// Represents the left sensor.
			/// </summary>
			Left,

			/// <summary>
			/// Represents the right sensor.
			/// </summary>
			Right
		}

		/// <summary>
		/// Creates a CerbotController.
		/// </summary>
		public CerbotController()
		{
			this.buzzer = new PWM(Cpu.PWMChannel.PWM_0, 1000, 0, false);

			this.leftSensor = new AnalogInput((Cpu.AnalogChannel)8);
			this.rightSensor = new AnalogInput(Cpu.AnalogChannel.ANALOG_6);
			this.leftIRLED = new OutputPort(GHI.Pins.Generic.GetPin('B', 13), true);
            this.rightIRLED = new OutputPort(GHI.Pins.Generic.GetPin('B', 14), true);

			this.leftInverted = false;
			this.rightInverted = false;
			this.leftMotor = new PWM(Cpu.PWMChannel.PWM_4, CerbotController.MOTOR_BASE_FREQUENCY, 0, false);
			this.rightMotor = new PWM(Cpu.PWMChannel.PWM_5, CerbotController.MOTOR_BASE_FREQUENCY, 0, false);
            this.leftMotorDirection = new OutputPort(GHI.Pins.Generic.GetPin('A', 6), false);
            this.rightMotorDirection = new OutputPort(GHI.Pins.Generic.GetPin('C', 4), false);

			this.servoConfigured = false;

            var spiSocket = GT.Socket.GetSocket(3, true, null, null);
            var tempSocket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
            tempSocket.SupportedTypes = new char[] { 'S' };
            tempSocket.CpuPins[3] = GHI.Pins.Generic.GetPin('B', 2);
            tempSocket.CpuPins[4] = GHI.Pins.Generic.GetPin('B', 2);
            tempSocket.CpuPins[5] = GHI.Pins.Generic.GetPin('B', 2);
            tempSocket.CpuPins[6] = GHI.Pins.Generic.GetPin('B', 2);
            tempSocket.CpuPins[7] = spiSocket.CpuPins[7];
            tempSocket.CpuPins[8] = spiSocket.CpuPins[8];
            tempSocket.CpuPins[9] = spiSocket.CpuPins[9];
            tempSocket.SPIModule = spiSocket.SPIModule;
            GT.Socket.SocketInterfaces.RegisterSocket(tempSocket);

            this.spiConfig = new GTI.SpiConfiguration(false, 0, 0, false, true, 2000);
            this.forwardLEDs = GTI.SpiFactory.Create(tempSocket, this.spiConfig, GTI.SpiSharing.Shared, tempSocket, Socket.Pin.Six, null);

			this.enableFaderPin = new PWM((Cpu.PWMChannel)13, 500, 500, PWM.ScaleFactor.Microseconds, true);
			this.enableFaderPin.Start();
			this.SetLEDBitmask(0x0);
		}

		/// <summary>
		/// Sets the frequency and duration that the buzzer will buzz for.
		/// </summary>
		/// <param name="frequency">The frequency that the buzzer will buzz in hertz.</param>
		/// <param name="duration">The duration the buzzer will buzz for in milliseconds.</param>
		/// <param name="dutyCycle">The duty cycle for the buzzer.</param>
		/// <remarks>If duration is 0, the buzzer will buzz indefinitely. If it is non-zero, then this call will block for as many milliseconds as specified in duration, then return.</remarks>
		public void StartBuzzer(double frequency, uint duration = 0, double dutyCycle = 0.5)
		{
			this.buzzer.Stop();

			if (frequency <= 0)
				return;

			this.buzzer.Frequency = frequency;
            this.buzzer.DutyCycle = dutyCycle;

            this.buzzer.Start();

			if (duration != 0)
			{
				Thread.Sleep((int)duration);
				this.buzzer.Stop();
			}
		}

		/// <summary>
		/// Stops the buzzer from buzzing.
		/// </summary>
		public void StopBuzzer()
		{
			this.buzzer.Stop();
		}

		/// <summary>
		/// Gets the reading from a reflective sensor between 0 and 100. The higher the number, 
		/// the more reflection that was detected. Nearby objects reflect more than far objects.
		/// </summary>
		/// <param name="sensor">The sensor to read from.</param>
		public double GetReflectiveReading(ReflectiveSensors sensor)
		{
			return 100 * (1 - (sensor == ReflectiveSensors.Left ? this.leftSensor.Read() : this.rightSensor.Read()));
		}

		/// <summary>
		/// Turns the reflective sensors on or off to save power (true = on, false = off). They are on by default.
		/// </summary>
		public void SetReflectiveSensorState(bool state)
		{
			this.leftIRLED.Write(state);
			this.rightIRLED.Write(state);
		}

		/// <summary>
		/// Sets the intensity of every front LED.
		/// </summary>
		/// <param name="intensity">The intensity between 0 and 100 to set the LEDs to. The higher the number, the brighter the LED.</param>
		public void SetLEDIntensity(uint intensity)
		{
			if (intensity > 100 || intensity < 1)
				throw new ArgumentException("Intensity must be between 1 and 100");

			enableFaderPin.Duration = 5 * intensity;
		}

		/// <summary>
		/// Sets the state of the front LEDs using a short where each bit represents one LED.
		/// </summary>
		/// <param name="mask">The mask used to set the LED state.</param>
		/// <remarks>Bit 0 is the leftmost LED, bit 15 is rightmost LED.</remarks>
		public void SetLEDBitmask(ushort mask)
		{
			this.ledMask = mask;

			byte[] toSend = new byte[2];
			toSend[0] = (byte)(mask >> 8);
			toSend[1] = (byte)(mask & 0xFF);

			this.forwardLEDs.Write(toSend);
		}

		/// <summary>
		/// Turns on the specified front LED while leaving the others unchanged.
		/// </summary>
		/// <param name="which">The LED number to turn on. Between 1 and 16.</param>
		public void TurnOnLED(int which)
		{
			if (which < 1 || which > 16)
				throw new ArgumentException("The LED must be between 1 and 16.");

			this.SetLEDBitmask((ushort)(this.ledMask | (1 << --which)));
		}

		/// <summary>
		/// Turns off the specified front LED while leaving the others unchanged.
		/// </summary>
		/// <param name="which">The LED number to turn off. Between 1 and 16.</param>
		public void TurnOffLED(int which)
		{
			if (which < 1 || which > 16)
				throw new ArgumentException("The LED must be between 1 and 16.");

			this.SetLEDBitmask((ushort)(this.ledMask & ~(1 << --which)));
		}

		/// <summary>
		/// Sets the pulse limits for the servo. You must call this before setting the servo position.
		/// </summary>
		/// <param name="minPulse">The minimum pulse width the servo can handle in microseconds.</param>
		/// <param name="maxPulse">The maximum pulse width the servo can handle in microseconds.</param>
		public void SetServoLimits(uint minPulse, uint maxPulse)
		{
			if (maxPulse < minPulse)
				throw new ArgumentException("Max pulse must be greater than min pulse.");

			this.servoMinPulse = minPulse;
			this.servoMaxPulse = maxPulse;
			this.servoPulseFactor = (maxPulse - minPulse) / 100;

			if (this.servoConfigured)
			{
				this.servo.Stop();
				this.servo = null;
			}

			this.servo = new PWM(Cpu.PWMChannel.PWM_1, 20000, this.servoMinPulse, PWM.ScaleFactor.Microseconds, false);
			this.servoConfigured = true;
		}

		/// <summary>
		/// Sets the position of the servo if one is present. Make sure to call SetServoLimits(min, max) before using this function.
		/// </summary>
		/// <param name="position">
		/// A number between 0 and 100 that represents the position of the servo.
		/// 0 means the servo will be sent the minimum pulse, 100 means it will be sent
		/// the maximum pulse, and number in between scale between the minimum and maximum.
		/// </param>
		public void SetServoPosition(double position)
		{
			if (position < 0 || position > 100)
				throw new ArgumentException("Position must be between 0 and 100.");

			if (!this.servoConfigured)
				throw new ArgumentException("You must call setServoConfig() before calling setServoPosition().");

			this.servo.Duration = (uint)(this.servoPulseFactor * position + this.servoMinPulse);
			this.servo.Start();
		}

		/// <summary>
		/// If you find that the motors go forward when passed a negative number due to reversed wiring, call this function. 
		/// It will invert the motor direction so that when you pass in a positive speed, it goes forward.
		/// </summary>
		public void SetMotorInversion(bool invertLeft, bool invertRight)
		{
			this.leftInverted = invertLeft;
			this.rightInverted = invertRight;
		}

		/// <summary>
		/// Sets the speed of the motor. -100 is full speed backwards, 100 is full speed forward, and 0 is stopped.
		/// </summary>
		/// <param name="leftSpeed">The new speed of the left motor.</param>
		/// <param name="rightSpeed">The new speed of the right motor.</param>
		public void SetMotorSpeed(int leftSpeed, int rightSpeed)
		{
			if (leftSpeed > 100 || leftSpeed < -100 || rightSpeed > 100 || rightSpeed < -100)
				new ArgumentException("The motor speed must be between -100 and 100");

			if (this.leftInverted)
				leftSpeed *= -1;

			if (this.rightInverted)
				rightSpeed *= -1;

			this.SetSpeed(this.leftMotor, this.leftMotorDirection, leftSpeed, true);
			this.SetSpeed(this.rightMotor, this.rightMotorDirection, rightSpeed, false);
		}

		private void SetSpeed(PWM motor, OutputPort direction, int speed, bool isLeft)
		{
			if (speed == 0)
			{
				direction.Write(false);
				motor.DutyCycle = 0.01;
			}
			else if (speed < 0)
			{
				direction.Write(isLeft ? true : false);
				motor.DutyCycle = speed / -100.0;
			}
			else
			{
				direction.Write(isLeft ? false : true);
				motor.DutyCycle = speed / 100.0;
			}

			motor.Start();
		}
	}
}