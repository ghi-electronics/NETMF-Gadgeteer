using System;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A MotorDriverL298 module for Microsoft .NET Gadgeteer
	/// </summary>
    public class MotorDriverL298 : GTM.Module
    {
        private const int STEP_FACTOR = 1000;

        private GTI.PwmOutput[] pwms;
        private GTI.DigitalOutput[] directions;
        private double[] lastSpeeds;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public MotorDriverL298(int socketNumber)
        {

            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('P', this);

            this.pwms = new GTI.PwmOutput[2]
            {
                GTI.PwmOutputFactory.Create(socket, Socket.Pin.Eight, false, this),
                GTI.PwmOutputFactory.Create(socket, Socket.Pin.Seven, false, this)
            };

            this.directions = new GTI.DigitalOutput[2]
            {
                GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Six, false, this),
                GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Nine, false, this)
            };

            this.lastSpeeds = new double[2] {0, 0};

            this.Frequency = 25000;

            this.StopAll();
        }

        /// <summary>
        /// The possible motors.
        /// </summary>
        public enum Motor
        {
            /// <summary>
            /// The motor marked M1.
            /// </summary>
            Motor1 = 0,

            /// <summary>
            /// The motor marked M2.
            /// </summary>
            Motor2 = 1,
        }

        /// <summary>
        /// Used to set the PWM frequency for the motors because some motors require a 
        /// certain frequency in order to operate properly. It defaults to 25KHz (25000).
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// Stops all motors.
        /// </summary>
        public void StopAll()
        {
            this.SetSpeed(Motor.Motor1, 0);
            this.SetSpeed(Motor.Motor2, 0);
        }

        /// <summary>
        /// Sets the given motor's speed.
        /// </summary>
        /// <param name="motor">The motor to set the speed for.</param>
        /// <param name="speed">The desired speed of the motor.</param>
        public void SetSpeed(Motor motor, double speed)
        {
            if (speed > 1 || speed < -1) new ArgumentOutOfRangeException("speed", "speed must be between -1 and 1.");
            if (motor != Motor.Motor1 && motor != Motor.Motor2) throw new ArgumentException("motor", "You must specify a valid motor.");

            this.directions[(int)motor].Write(speed < 0);
            this.pwms[(int)motor].Set(this.Frequency, speed < 0 ? 1 - speed : speed);
            this.lastSpeeds[(int)motor] = speed;
        }

        /// <summary>
        /// Sets the given motor's speed.
        /// </summary>
        /// <param name="motor">The motor to set the speed for.</param>
        /// <param name="speed">The desired speed of the motor.</param>
        /// <param name="time">How many milliseconds the motor should take to reach the specified speed.</param>
        public void SetSpeed(Motor motor, double speed, int time)
        {
            if (speed > 1 || speed < -1) new ArgumentOutOfRangeException("speed", "speed must be between -1  and 1.");
            if (motor != Motor.Motor1 && motor != Motor.Motor2) throw new ArgumentException("motor", "You must specify a valid motor.");

            double currentSpeed = this.lastSpeeds[(int)motor];

            if (currentSpeed == speed)
                return;

            double sleep = time / ((speed - currentSpeed) * MotorDriverL298.STEP_FACTOR);
            double step = 1 / MotorDriverL298.STEP_FACTOR;

            while (Math.Abs(speed - currentSpeed) >= 0.01)
            {
                currentSpeed += step;

                this.SetSpeed(motor, currentSpeed);

                Thread.Sleep((int)sleep);
            }
        }
    }
}