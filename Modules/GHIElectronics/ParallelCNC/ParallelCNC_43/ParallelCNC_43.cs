using System;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A ParallelCNC module for Microsoft .NET Gadgeteer
    /// </summary>
    [Obsolete]
    public class ParallelCNC : GTM.DaisyLinkModule
    {
        private enum Registers : byte
        {
            ACTIVATE = 0, STATUS,
            XSTEP1, XSTEP2, XSTEP3, XSTEP4,
            YSTEP1, YSTEP2, YSTEP3, YSTEP4,
            ZSTEP1, ZSTEP2, ZSTEP3, ZSTEP4,
            DIRECTION, ENABLE, VALUE, LED,
            X_ENABLE, X_DIR, X_STEP,
            Y_ENABLE, Y_DIR, Y_STEP,
            Z_ENABLE, Z_DIR, Z_STEP,
            A_ENABLE, A_DIR, A_STEP,
            B_ENABLE, B_DIR, B_STEP,
        };

        /// <summary>
        /// Lists the axises that this module can control
        /// </summary>
        public enum Axis
        { 
            /// <summary>
            /// The X-axis
            /// </summary>
            X = 0, 
            
            /// <summary>
            /// The Y-axis
            /// </summary>
            Y, 
            
            /// <summary>
            /// The Z-axis
            /// </summary>
            Z, 
            
            /// <summary>
            /// The A-axis
            /// </summary>
            A, 
            
            /// <summary>
            /// The B-Axis
            /// </summary>
            B 
        };

        /// <summary>
        /// A mask used when controlling motors.
        /// </summary>
        public enum axisMask : byte 
        {
            /// <summary>
            /// Mask for the X-axis
            /// </summary>
            X = 32, 
            
            /// <summary>
            /// Mask for the Y-axis
            /// </summary>
            Y = 16, 
            
            /// <summary>
            /// Mask for the Z-axis
            /// </summary>
            Z = 8, 
            
            /// <summary>
            /// Mask for the A-axis
            /// </summary>
            A = 4, 
            
            /// <summary>
            /// Mask for the B-axis
            /// </summary>
            B = 2, 
        };

        private const byte GHI_DAISYLINK_MANUFACTURER = 0x10;
        private const byte GHI_DAISYLINK_TYPE_PARALLELCNC = 0x03;
        private const byte GHI_DAISYLINK_VERSION_PARALLELCNC = 0x01;

        byte[] throwAway = new byte[] { 1 };

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public ParallelCNC(int socketNumber)
            : base(socketNumber, GHI_DAISYLINK_MANUFACTURER, GHI_DAISYLINK_TYPE_PARALLELCNC, GHI_DAISYLINK_VERSION_PARALLELCNC, GHI_DAISYLINK_VERSION_PARALLELCNC, 50, "ParallelCNC")
        {
            this.DaisyLinkInterrupt += new DaisyLinkInterruptEventHandler(ParallelCNC_DaisyLinkInterrupt);
        }

        void ParallelCNC_DaisyLinkInterrupt(DaisyLinkModule sender)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Enumerates the pins on the parallel port to their function.
        /// </summary>
        /// <param name="xEnable">The pin on the parallel port that handles this function.</param>
        /// <param name="xDir">The pin on the parallel port that handles this function.</param>
        /// <param name="xStep">The pin on the parallel port that handles this function.</param>
        /// <param name="yEnable">The pin on the parallel port that handles this function.</param>
        /// <param name="yDir">The pin on the parallel port that handles this function.</param>
        /// <param name="yStep">The pin on the parallel port that handles this function.</param>
        /// <param name="zEnable">The pin on the parallel port that handles this function.</param>
        /// <param name="zDir">The pin on the parallel port that handles this function.</param>
        /// <param name="zStep">The pin on the parallel port that handles this function.</param>
        /// <param name="aEnable">The pin on the parallel port that handles this function.</param>
        /// <param name="aDir">The pin on the parallel port that handles this function.</param>
        /// <param name="aStep">The pin on the parallel port that handles this function.</param>
        /// <param name="bEnable">The pin on the parallel port that handles this function.</param>
        /// <param name="bDir">The pin on the parallel port that handles this function.</param>
        /// <param name="bStep">The pin on the parallel port that handles this function.</param>
        public void ConfigurePins(byte xEnable, byte xDir, byte xStep,
            byte yEnable, byte yDir, byte yStep,
            byte zEnable, byte zDir, byte zStep,
            byte aEnable, byte aDir, byte aStep,
            byte bEnable, byte bDir, byte bStep)
        {
            WriteRegister((byte)Registers.X_ENABLE, xEnable);
            WriteRegister((byte)Registers.X_DIR, xDir);
            WriteRegister((byte)Registers.X_STEP, xStep);

            WriteRegister((byte)Registers.Y_ENABLE, yEnable);
            WriteRegister((byte)Registers.Y_DIR, yDir);
            WriteRegister((byte)Registers.Y_STEP, yStep);

            WriteRegister((byte)Registers.Z_ENABLE, zEnable);
            WriteRegister((byte)Registers.Z_DIR, zDir);
            WriteRegister((byte)Registers.Z_STEP, zStep);

            WriteRegister((byte)Registers.A_ENABLE, aEnable);
            WriteRegister((byte)Registers.A_DIR, aDir);
            WriteRegister((byte)Registers.A_STEP, aStep);

            WriteRegister((byte)Registers.B_ENABLE, bEnable);
            WriteRegister((byte)Registers.B_DIR, bDir);
            WriteRegister((byte)Registers.B_STEP, bStep);
        }

        /// <summary>
        /// Writes to the daisylink register specified by the address. Does not allow writing to the reserved registers.
        /// </summary>
        /// <param name="address">Address of the register.</param>
        /// <param name="writebuffer">Byte to write.</param>
        public void WriteRegister(byte address, byte writebuffer)
        {
            WriteParams((byte)(DaisyLinkOffset + address), (byte)writebuffer);
        }

        /// <summary>
        /// Reads a byte from the specified register. Allows reading of reserved registers.
        /// </summary>
        /// <param name="memoryaddress">Address of the register.</param>
        /// <returns></returns>
        public byte ReadRegister(byte memoryaddress)
        {
            return Read((byte)(DaisyLinkOffset + memoryaddress));
        }

        /// <summary>
        /// Moves the motor associated with the specified axis.
        /// </summary>
        /// <remarks>Calling this function on a motor that is already moving will cancel its current movement and set a new target position.</remarks>
        /// <param name="axis">The axis to move.</param>
        /// <param name="steps">The number of steps to move.</param>
        /// <param name="positiveDirection">Determines the direction of the motor.</param>
        public void MoveMotor(Axis axis, int steps, bool positiveDirection)
        {
            byte status;
            byte direction;

            // read the direction
            direction = ReadRegister((byte)Registers.DIRECTION);

            switch (axis)
            {
                case Axis.X:
                    {
                        // read status and see if this motor is stopped.
                        status = ReadRegister((byte)Registers.STATUS);

                        //if ((status & (byte)axisMask.X) == status)
                        //    throw new Exception("Motor is currently moving. Cannot set new steps at this time");

                        // Write the steps value to the register
                        WriteRegister((byte)Registers.XSTEP1, (byte)(steps & 0xFF));
                        WriteRegister((byte)Registers.XSTEP2, (byte)((steps >> 8) & 0xFF));
                        WriteRegister((byte)Registers.XSTEP3, (byte)((steps >> 16) & 0xFF));
                        WriteRegister((byte)Registers.XSTEP4, (byte)((steps >> 24) & 0xFF));

                        if (positiveDirection)
                        {
                            direction |= (byte)axisMask.X;
                        }
                        else
                        {
                            byte mask = (byte)axisMask.X;
                            direction &= (byte)(~mask);
                        }

                        // mark the motor is ready to move
                        status |= (byte)axisMask.X;

                        WriteRegister((byte)Registers.DIRECTION, direction);
                        WriteRegister((byte)Registers.STATUS, status);

                        break;
                    }
                case Axis.Y:
                    {
                        // read status and see if this motor is stopped.
                        status = ReadRegister((byte)Registers.STATUS);

                        //if ((status & (byte)axisMask.Y) == status)
                        //    throw new Exception("Motor is currently moving. Cannot set new steps at this time");

                        // Write the steps value to the register
                        WriteRegister((byte)Registers.YSTEP1, (byte)(steps & 0xFF));
                        WriteRegister((byte)Registers.YSTEP2, (byte)((steps >> 8) & 0xFF));
                        WriteRegister((byte)Registers.YSTEP3, (byte)((steps >> 16) & 0xFF));
                        WriteRegister((byte)Registers.YSTEP4, (byte)((steps >> 24) & 0xFF));

                        if (positiveDirection)
                        {
                            direction |= (byte)axisMask.Y;
                        }
                        else
                        {
                            byte mask = (byte)axisMask.Y;
                            direction &= (byte)(~mask);
                        }

                        // mark the motor is ready to move
                        status |= (byte)axisMask.Y;

                        WriteRegister((byte)Registers.DIRECTION, direction);
                        WriteRegister((byte)Registers.STATUS, status);

                        break;
                    }
                case Axis.Z:
                    {
                        // read status and see if this motor is stopped.
                        status = ReadRegister((byte)Registers.STATUS);

                        //if ((status & (byte)axisMask.Z) == status)
                        //    throw new Exception("Motor is currently moving. Cannot set new steps at this time");

                        // Write the steps value to the register
                        WriteRegister((byte)Registers.ZSTEP1, (byte)(steps & 0xFF));
                        WriteRegister((byte)Registers.ZSTEP2, (byte)((steps >> 8) & 0xFF));
                        WriteRegister((byte)Registers.ZSTEP3, (byte)((steps >> 16) & 0xFF));
                        WriteRegister((byte)Registers.ZSTEP4, (byte)((steps >> 24) & 0xFF));

                        if (positiveDirection)
                        {
                            direction |= (byte)axisMask.Z;
                        }
                        else
                        {
                            byte mask = (byte)axisMask.Z;
                            direction &= (byte)(~mask);
                        }

                        // mark the motor is ready to move
                        status |= (byte)axisMask.Z;

                        WriteRegister((byte)Registers.DIRECTION, direction);
                        WriteRegister((byte)Registers.STATUS, status);

                        break;
                    }
                case Axis.A:
                    {
                        throw new Exception("This direction not yet supproted!");
                        //break;
                    }
                case Axis.B:
                    {
                        throw new Exception("This direction not yet supproted!");
                        //break;
                    }
            }
        }

        /// <summary>
        /// Sets all motors to the passed in numbers of steps. Not yet implemented.
        /// </summary>
        /// <param name="xSteps">Number of steps in the X direction.</param>
        /// <param name="ySteps">Number of steps in the Y direction.</param>
        /// <param name="zSteps">Number of steps in the Z direction.</param>
        /// <param name="aSteps">Number of steps in the A direction.</param>
        /// <param name="bSteps">Number of steps in the B direction.</param>
        public void SetAllMotors(int xSteps, int ySteps, int zSteps, int aSteps, int bSteps)
        {
            // read the status and make sure they are all stopped, if not, throw an exception

        }

        /// <summary>
        /// Enable the motor associated with the specified axis.
        /// </summary>
        /// <param name="axis">The axis to enable.</param>
        public void EnableMotor(axisMask axis)
        {
            byte enable = ReadRegister((byte)Registers.ENABLE);

            enable |= (byte)((byte)axis * 2);

            WriteRegister((byte)Registers.ENABLE, 1);
        }

        /// <summary>
        /// Disable the motor associated with the specified axis.
        /// </summary>
        /// <param name="axis">The axis to diable.</param>
        public void DisableMotor(axisMask axis)
        {
            //WRONG WriteRegister((byte)Registers.ACTIVATE, 0);
        }

        /// <summary>
        /// Stops all motors and clears all step counters.
        /// </summary>
        public void TerminateMovements()
        {
            //TODO
        }

        /// <summary>
        /// Immediately stops all motors.
        /// </summary>
        /// <param name="lockMotorsInPlace">If true, on stop the motors will lock into place.</param>
        public void EmergencyStop(bool lockMotorsInPlace)
        {
            // Read the current status register.
            byte status = ReadRegister((byte)Registers.STATUS);

            // Turn on the flag to stop all motors.
            status |= 128;

            // TODO: handle bool
            if (lockMotorsInPlace)
            {
                status |= 1;
            }
            else
            {
                byte mask = 1;
                status &= (byte)(~mask);
            }

            // Write the new status register back.
            WriteRegister((byte)Registers.STATUS, status);
        }
    }
}