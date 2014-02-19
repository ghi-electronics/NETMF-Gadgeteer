using System;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

using System.Threading;
using Microsoft.SPOT;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A motor controller used to control two different motors.
	/// </summary>
    public class MotorDriverL298 : GTM.Module
    {
        GTI.PWMOutput m_Pwm1;
        GTI.PWMOutput m_Pwm2;

        GTI.DigitalOutput m_Direction1;
        GTI.DigitalOutput m_Direction2;

		/// <summary>
		/// Used to set the PWM frequency for the motors because some motors require a 
		/// certain frequency in order to operate properly. It defaults to 25KHz (25000).
		/// </summary>
        public int Frequency { get; set; }

        int m_lastSpeed1 = 0;
        int m_lastSpeed2 = 0;

        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public MotorDriverL298(int socketNumber)
        {
			this.Frequency = 25000;

            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported(new char[] { 'P' }, this);

            m_Pwm1 = new GTI.PWMOutput(socket, Socket.Pin.Seven, false, this);
            m_Pwm2 = new GTI.PWMOutput(socket, Socket.Pin.Eight, false, this);

            m_Direction1 = new GTI.DigitalOutput(socket, Socket.Pin.Nine, false, this);
            m_Direction2 = new GTI.DigitalOutput(socket, Socket.Pin.Six, false, this);

            m_Pwm1.Set(Frequency, 0);
            m_Pwm2.Set(Frequency, 0);
        }

        /// <summary>
        /// Represents the state of the <see cref="MotorDriverL298"/> object.
        /// </summary>
        public enum MotorControllerL298State
        {
            /// <summary>
            /// The state of MotorDriverL298 is low.
            /// </summary>
            Low = 0,
            /// <summary>
            /// The state of MotorDriverL298 is high.
            /// </summary>
            High = 1
        }

        /// <summary>
        /// Represents the desired motor to use.
        /// </summary>
        public enum Motor
        {
            /// <summary>
            /// The left motor, marked M1, is the one in use.
            /// </summary>
            Motor1 = 0,

            /// <summary>
            /// The right motor, marked M2, is the one in use.
            /// </summary>
            Motor2 = 1,
        }

        /// <summary>
        /// Used to set a motor's speed.
        /// <param name="_motorSide">The motor <see cref="Motor"/> you are setting the speed for.</param>
        /// <param name="_newSpeed"> The new speed that you want to set the current motor to.</param>
        /// </summary>
        public void MoveMotor(Motor _motorSide, int _newSpeed)
        {
            // Make sure the speed is within an acceptable range.
            if (_newSpeed > 100 || _newSpeed < -100)
                new ArgumentException("New motor speed outside the acceptable range (-100-100)", "_newSpeed");

            //////////////////////////////////////////////////////////////////////////////////
            // Motor1
            //////////////////////////////////////////////////////////////////////////////////
            if (_motorSide == Motor.Motor2)
            {
                // Determine the direction we are going to go.
                if (_newSpeed == 0)
                {
                    //if (m_lastSpeed1 == 0)
                    m_Direction1.Write(false);
                    m_Pwm1.Set(Frequency, 0.01);
                }
                else if (_newSpeed < 0)
                {
                    // Set direction and power.
                    m_Direction1.Write(true);

                    /////////////////////////////////////////////////////////////////////////////
                    // Quick fix for current PWM issue
                    double fix = (double)((100 - System.Math.Abs(_newSpeed)) / 100.0);
                    if (fix >= 1.0)
                        fix = 0.99;
                    if (fix <= 0.0)
                        fix = 0.01;
                    /////////////////////////////////////////////////////////////////////////////

                    m_Pwm1.Set(Frequency, fix);
                }
                else
                {
                    // Set direction and power.
                    m_Direction1.Write(false);

                    /////////////////////////////////////////////////////////////////////////////
                    // Quick fix for current PWM issue
                    double fix = (double)(_newSpeed / 100.0);
                    if (fix >= 1.0)
                        fix = 0.99;
                    if (fix <= 0.0)
                        fix = 0.01;
                    /////////////////////////////////////////////////////////////////////////////

                    m_Pwm1.Set(Frequency, fix);
                }

                // Save our speed
                m_lastSpeed1 = _newSpeed;
            }
            //////////////////////////////////////////////////////////////////////////////////
            // Motor2
            //////////////////////////////////////////////////////////////////////////////////
            else
			{
                // Determine the direction we are going to go.
                if (_newSpeed == 0)
                {
                    //if( m_lastSpeed2 == 0)
                    m_Direction2.Write(false);
                    m_Pwm2.Set(Frequency, 0.01);
                }
                else if (_newSpeed < 0)
                {
                    // Set direction and power.
                    m_Direction2.Write(true);

                    /////////////////////////////////////////////////////////////////////////////
                    // Quick fix for current PWM issue
                    double fix = (double)((100 - System.Math.Abs(_newSpeed)) / 100.0);
                    if (fix >= 1.0)
                        fix = 0.99;
                    if (fix <= 0.0)
                        fix = 0.01;
                    /////////////////////////////////////////////////////////////////////////////

                    m_Pwm2.Set(Frequency, fix);
                }
                else
                {
                    // Set direction and power.
                    m_Direction2.Write(false);

                    /////////////////////////////////////////////////////////////////////////////
                    // Quick fix for current PWM issue
                    double fix = (double)(_newSpeed / 100.0);
                    if (fix >= 1.0)
                        fix = 0.99;
                    if (fix <= 0.0)
                        fix = 0.01;
                    /////////////////////////////////////////////////////////////////////////////

                    m_Pwm2.Set(Frequency, fix);
                }

                // Save our speed
                m_lastSpeed2 = _newSpeed;

            }
            //////////////////////////////////////////////////////////////////////////////////
        }

        /// <summary>
        /// Used to set a motor's speed with a ramping acceleration. <see cref="MoveMotor"/>
        /// <param name="_motorSide">The motor <see cref="Motor"/> you are setting the speed for.</param>
        /// <param name="_newSpeed"> The new speed that you want to set the current motor to.</param>
        /// <param name="_rampingDelayMilli"> The time in which you want the motor to reach the new speed (in milliseconds).</param>
        /// </summary>
        public void MoveMotorRamp(Motor _motorSide, int _newSpeed, int _rampingDelayMilli)
        {
            int temp_speed;
            int startSpeed;
            int lastSpeed;

            int timeStep;
            int deltaTime = 0;

            // Determine which motor we are going to change.
            if (_motorSide == Motor.Motor2)
            {
                temp_speed = m_lastSpeed1;
                startSpeed = m_lastSpeed1;
                lastSpeed = m_lastSpeed1;
            }
            else
            {
                temp_speed = m_lastSpeed2;
                startSpeed = m_lastSpeed2;
                lastSpeed = m_lastSpeed2;
            }

            // Determine how long we need to wait between move calls.
            // Make sure we dont divied by 0
            if (_newSpeed == lastSpeed)
                return;

            timeStep = _rampingDelayMilli / (_newSpeed - lastSpeed);

            ////////////////////////////////////////////////////////////////
            // Ramp
            ////////////////////////////////////////////////////////////////
            while (_newSpeed != temp_speed)
            {
                // If we have been updating for the passed in length of time, exit the loop.
                if (deltaTime >= _rampingDelayMilli)
                    break;

                // If we are slowing the motor down.
                if (temp_speed > _newSpeed)
                {
                    temp_speed += ((startSpeed - _newSpeed) / timeStep);
                }
                // If we are speeding the motor up.
                if (temp_speed < _newSpeed)
                {
                    temp_speed -= ((startSpeed - _newSpeed) / timeStep);
                }

                // Set our motor speed to our new values.
                MoveMotor(_motorSide, temp_speed);

                // Increase our timer.
                deltaTime += System.Math.Abs(timeStep);

                // Wait until we can move again.
                Thread.Sleep(System.Math.Abs(timeStep));
            }
            ////////////////////////////////////////////////////////////////
        }

        /// <summary>
        /// Used to set a motor's speed with a ramping acceleration asynchronously. <see cref="MoveMotor"/>
        /// <param name="_motorSide">The motor <see cref="Motor"/> you are setting the speed for.</param>
        /// <param name="_newSpeed"> The new speed that you want to set the current motor to.</param>
        /// <param name="_rampingDelayMilli"> The time in which you want the motor to reach the new speed (in milliseconds).</param>
        /// </summary>
        public void MoveMotorRampNonBlocking(Motor _motorSide, int _newSpeed, int _rampingDelayMilli)
		{
			new Thread(() => this.MoveMotorRamp(_motorSide, _newSpeed, _rampingDelayMilli)).Start();
        }
    }
}