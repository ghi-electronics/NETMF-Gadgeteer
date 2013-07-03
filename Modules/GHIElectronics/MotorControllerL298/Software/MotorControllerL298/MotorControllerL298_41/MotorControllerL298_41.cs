using System;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

using Microsoft.SPOT.Hardware;
using System.Threading;
using Microsoft.SPOT;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A MotorControllerL298 module for Microsoft .NET Gadgeteer
    /// </summary>
    /// <example>
    /// <para>The following example shows the use of the motor controller</para>
    /// <code>
    ///using System;
    ///using Microsoft.SPOT;
    ///using Microsoft.SPOT.Presentation;
    ///using Microsoft.SPOT.Presentation.Controls;
    ///using Microsoft.SPOT.Presentation.Media;
    ///
    ///using Microsoft.SPOT.Hardware;
    ///using System.Threading;
    ///
    ///using GT = Gadgeteer;
    ///using GTM = Gadgeteer.Modules;
    ///using GTI = Gadgeteer.Interfaces;
    ///using Gadgeteer.Modules.GHIElectronics;
    ///
    /// namespace TestApp
    /// {
    ///     public partial class Program
    ///     {
    ///         // This template uses the FEZ Spider mainboard from GHI Electronics
    ///
    ///         // Define and initialize GTM.Modules here, specifying their socket numbers.        
    ///         MotorControllerL298 motorController = new MotorControllerL298(11);
    ///
    ///         void ProgramStarted()
    ///         {
    ///             // Set the motor speeds to 0
    ///             motorController.MoveMotor(MotorControllerL298.Motor.Motor1, 0);
    ///             motorController.MoveMotor(MotorControllerL298.Motor.Motor2, 0);
    ///
    ///             // Switch this to test the threaded movement
    ///             // true = threaded
    ///             bool bThreaded = false;
    ///             
    ///             if(bThreaded)
    ///             {
    ///                 // Start one motor on its own thread
    ///                 motorController.MoveMotorRampNB(MotorControllerL298.Motor.Motor1, 100, 5000);
    ///
    ///                 // Wait so we can see the other start
    ///                 Thread.Sleep(500);
    ///
    ///                 // Start the other motor on its own thread
    ///                 motorController.MoveMotorRampNB(MotorControllerL298.Motor.Motor2, -100, 5000);
    ///             }
    ///             else
    ///             {
    ///                 motorController.MoveMotorRamp(MotorControllerL298.Motor.Motor1, 100, 5000);
    ///                 Debug.Print("going down");
    ///                 motorController.MoveMotorRamp(MotorControllerL298.Motor.Motor1, 0, 5000);
    ///                 Debug.Print("going more down");
    ///                 motorController.MoveMotorRamp(MotorControllerL298.Motor.Motor1, -100, 5000);
    ///                 Debug.Print("going up");
    ///                 motorController.MoveMotorRamp(MotorControllerL298.Motor.Motor1, 0, 5000);
    ///                 Debug.Print("more up");
    ///                 motorController.MoveMotorRamp(MotorControllerL298.Motor.Motor1, 100, 5000);
    ///                 
    ///                 Debug.Print("OTHER SIDE");
    ///                 
    ///                 motorController.MoveMotorRamp(MotorControllerL298.Motor.Motor2, 100, 5000);
    ///                 Debug.Print("going down");
    ///                 motorController.MoveMotorRamp(MotorControllerL298.Motor.Motor2, 0, 5000);
    ///                 Debug.Print("going more down");
    ///                 motorController.MoveMotorRamp(MotorControllerL298.Motor.Motor2, -100, 5000);
    ///                 Debug.Print("going up");
    ///                 motorController.MoveMotorRamp(MotorControllerL298.Motor.Motor2, 0, 5000);
    ///                 Debug.Print("more up");
    ///                 motorController.MoveMotorRamp(MotorControllerL298.Motor.Motor2, 100, 5000);
    ///             }
    ///             
    ///             Debug.Print("Program Started");
    ///         }
    ///     }
    /// }
    /// 
    /// </code>
    /// </example>
    public class MotorControllerL298 : GTM.Module
    {
        GTI.PWMOutput m_Pwm1;
        GTI.PWMOutput m_Pwm2;

        OutputPort m_Direction1;
        OutputPort m_Direction2;

        int m_lastSpeed1 = 0;
        int m_lastSpeed2 = 0;

        // This example implements  a driver in managed code for a simple Gadgeteer module.  The module uses a 
        // single GTI.InterruptInput to interact with a sensor that can be in either of two states: low or high.
        // The example code shows the recommended code pattern for exposing the property (IsHigh). 
        // The example also uses the recommended code pattern for exposing two events: MotorControllerL298High, MotorControllerL298Low. 
        // The triple-slash "///" comments shown will be used in the build process to create an XML file named
        // GTM.GHIElectronics.MotorControllerL298. This file will provide Intellisense and documention for the
        // interface and make it easier for developers to use the MotorControllerL298 module.        

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public MotorControllerL298(int socketNumber)
        {
            // This finds the Socket instance from the user-specified socket number.  
            // This will generate user-friendly error messages if the socket is invalid.
            // If there is more than one socket on this module, then instead of "null" for the last parameter, 
            // put text that identifies the socket to the user (e.g. "S" if there is a socket type S)
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported(new char[] { 'P' }, this);

            // Set up the PWM pins
            m_Pwm1 = new GTI.PWMOutput(socket, Socket.Pin.Seven, this);
            m_Pwm2 = new GTI.PWMOutput(socket, Socket.Pin.Eight, this);

            m_Direction1 = new OutputPort(socket.CpuPins[9], false);
            m_Direction2 = new OutputPort(socket.CpuPins[6], false);

            m_Pwm1.Set(1000, 0);
            m_Pwm2.Set(1000, 0);

            // This creates an GTI.InterruptInput interface. The interfaces under the GTI namespace provide easy ways to build common modules.
            // This also generates user-friendly error messages automatically, e.g. if the user chooses a socket incompatible with an interrupt input.
            this.input = new GTI.InterruptInput(socket, GT.Socket.Pin.Three, GTI.GlitchFilterMode.On, GTI.ResistorMode.PullUp, GTI.InterruptMode.RisingAndFallingEdge, this);

            // This registers a handler for the interrupt event of the interrupt input (which is below)
            this.input.Interrupt += new GTI.InterruptInput.InterruptEventHandler(this._input_Interrupt);
        }

        private void _input_Interrupt(GTI.InterruptInput input, bool value)
        {
            this.OnMotorControllerL298Event(this, value ? MotorControllerL298State.Low : MotorControllerL298State.High);
        }

        private GTI.InterruptInput input;

        /// <summary>
        /// Gets a value that indicates whether the state of this MotorControllerL298 is high.
        /// </summary>
        public bool IsHigh
        {
            get
            {
                return this.input.Read();
            }
        }

        /// <summary>
        /// Represents the state of the <see cref="MotorControllerL298"/> object.
        /// </summary>
        public enum MotorControllerL298State
        {
            /// <summary>
            /// The state of MotorControllerL298 is low.
            /// </summary>
            Low = 0,
            /// <summary>
            /// The state of MotorControllerL298 is high.
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
            Motor1 = 1,

            /// <summary>
            /// The right motor, marked M2, is the one in use.
            /// </summary>
            Motor2 = 0,
        }

        /// <summary>
        /// Used to set a motor's speed.
        /// <param name="_motorSide">The motor <see cref="Motor"/> you are setting the speed for.</param>
        /// <param name="_newSpeed"> The new speed that you want to set the current motor to.</param>
        /// </summary>
        public void MoveMotor(Motor _motorSide, int _newSpeed)
        {
#if DEBUG
            Debug.Print(_newSpeed.ToString());
#endif
            // Make sure the speed is within an acceptable range.
            if (_newSpeed > 100 || _newSpeed < -100)
                new ArgumentException("New motor speed outside the acceptable range (-100-100)", "_newSpeed");

            //////////////////////////////////////////////////////////////////////////////////
            // Motor1
            //////////////////////////////////////////////////////////////////////////////////
            if (_motorSide == Motor.Motor1)
            {
                // Save our speed
                m_lastSpeed1 = _newSpeed;

                // Determine the direction we are going to go.
                if (_newSpeed < 0)
                {
                    // Set direction and power.
                    m_Direction1.Write(true);
                    m_Pwm1.Set(1000, (byte)(100 - System.Math.Abs(_newSpeed)));
                }
                else
                {
                    // Set direction and power.
                    m_Direction1.Write(false);
                    m_Pwm1.Set(1000, (byte)_newSpeed);
                }
            }
            //////////////////////////////////////////////////////////////////////////////////
            // Motor2
            //////////////////////////////////////////////////////////////////////////////////
            else
            {
                // Save our speed
                m_lastSpeed2 = _newSpeed;

                // Determine the direction we are going to go.
                if (_newSpeed < 0)
                {
                    // Set direction and power.
                    m_Direction2.Write(true);
                    m_Pwm2.Set(1000, (byte)(100 - System.Math.Abs(_newSpeed)));
                }
                else
                {
                    // Set direction and power.
                    m_Direction2.Write(false);
                    m_Pwm2.Set(1000, (byte)_newSpeed);
                }
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
            if (_motorSide == Motor.Motor1)
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
        /// Used to set a the values for a motor's movement to be used by <see cref="MoveMotorRampNB"/> 
        /// to give a motor ramping acceleration asynchronously.
        /// </summary>
        private struct tMovement
        {
            public Motor m_motorSide;
            public int m_newSpeed;
            public int m_rampingDelayMilli;
        }

        /// <summary>
        /// Used to set a motor's speed with a ramping acceleration asynchronously. <see cref="MoveMotor"/>
        /// <param name="_motorSide">The motor <see cref="Motor"/> you are setting the speed for.</param>
        /// <param name="_newSpeed"> The new speed that you want to set the current motor to.</param>
        /// <param name="_rampingDelayMilli"> The time in which you want the motor to reach the new speed (in milliseconds).</param>
        /// </summary>
        public void MoveMotorRampNB(Motor _motorSide, int _newSpeed, int _rampingDelayMilli)
        {
            movement = new tMovement();
            movement.m_motorSide = _motorSide;
            movement.m_newSpeed = _newSpeed;
            movement.m_rampingDelayMilli = _rampingDelayMilli;

            Thread moveThread = new Thread(new ThreadStart(this.MoveMotorRampThreadStart));
            moveThread.Start();
        }


        private tMovement movement;// = new tMovement();

        /// <summary>
        /// Represents the function that is used to start a thread to asynchronously move a motor.
        /// </summary>
        private void MoveMotorRampThreadStart()
        {
            MoveMotorRamp(movement.m_motorSide, movement.m_newSpeed, movement.m_rampingDelayMilli);
        }

        /// <summary>
        /// Represents the delegate that is used to handle the <see cref="MotorControllerL298High"/>
        /// and <see cref="MotorControllerL298Low"/> events.
        /// </summary>
        /// <param name="sender">The <see cref="MotorControllerL298"/> object that raised the event.</param>
        /// <param name="state">The state of the MotorControllerL298</param>
        public delegate void MotorControllerL298EventHandler(MotorControllerL298 sender, MotorControllerL298State state);

        /// <summary>
        /// Raised when the state of <see cref="MotorControllerL298"/> is high.
        /// </summary>
        /// <remarks>
        /// Implement this event handler and the <see cref="MotorControllerL298Low"/> event handler
        /// when you want to provide an action associated with MotorControllerL298 activity.
        /// The state of the MotorControllerL298 is passed to the <see cref="MotorControllerL298EventHandler"/> delegate,
        /// so you can use the same event handler for both MotorControllerL298 states.
        /// </remarks>
        public event MotorControllerL298EventHandler MotorControllerL298High;

        /// <summary>
        /// Raised when the state of <see cref="MotorControllerL298"/> is low.
        /// </summary>
        /// <remarks>
        /// Implement this event handler and the <see cref="MotorControllerL298High"/> event handler
        /// when you want to provide an action associated with MotorControllerL298 activity.
        /// Since the state of the MotorControllerL298 is passed to the <see cref="MotorControllerL298EventHandler"/> delegate,
        /// you can use the same event handler for both MotorControllerL298 states.
        /// </remarks>
        public event MotorControllerL298EventHandler MotorControllerL298Low;

        private MotorControllerL298EventHandler onMotorControllerL298;

        /// <summary>
        /// Raises the <see cref="MotorControllerL298High"/> or <see cref="MotorControllerL298Low"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="MotorControllerL298"/> that raised the event.</param>
        /// <param name="MotorControllerL298State">The state of the MotorControllerL298.</param>
        protected virtual void OnMotorControllerL298Event(MotorControllerL298 sender, MotorControllerL298State MotorControllerL298State)
        {
            if (this.onMotorControllerL298 == null)
            {
                this.onMotorControllerL298 = new MotorControllerL298EventHandler(this.OnMotorControllerL298Event);
            }

            if (Program.CheckAndInvoke((MotorControllerL298State == MotorControllerL298State.High ? this.MotorControllerL298High : this.MotorControllerL298Low), this.onMotorControllerL298, sender, MotorControllerL298State))
            {
                switch (MotorControllerL298State)
                {
                    case MotorControllerL298State.High:
                        this.MotorControllerL298High(sender, MotorControllerL298State);
                        break;
                    case MotorControllerL298State.Low:
                        this.MotorControllerL298Low(sender, MotorControllerL298State);
                        break;
                }
            }
        }
    }
}