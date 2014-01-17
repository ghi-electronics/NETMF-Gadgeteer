using System;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A module used to read and send remote control signals, with 8 PWM in and 8 PWM out for Microsoft .NET Gadgeteer
    /// </summary>
    /// <example>
    /// <para>The following example uses a <see cref="PulseInOut"/> object to write to a few of the 8 available PWM outputs to control servos.</para>
    /// <code>
    /// <![CDATA[    
    /// using System;
    /// using System.Collections;
    /// using System.Threading;
    /// using Microsoft.SPOT;
    /// using Microsoft.SPOT.Presentation;
    /// using Microsoft.SPOT.Presentation.Controls;
    /// using Microsoft.SPOT.Presentation.Media;
    /// using Microsoft.SPOT.Touch;
    ///
    /// using Gadgeteer.Networking;
    /// using GT = Gadgeteer;
    /// using GTM = Gadgeteer.Modules;
    /// using Gadgeteer.Modules.GHIElectronics;
    ///
    /// namespace TestApp
    /// {
    ///     public partial class Program
    ///     {
    ///         void ProgramStarted()
    ///         {
    ///             // Set the frequency to 50Hz, so we have a pulse every 20ms
    ///             pulseInOut.SetFrequency(50);
    ///
    ///             // Make every servo turn from its low limit to its high limit
    ///             for (ushort i = 1100; i < 1900; i += 8)
    ///             {
    ///                 pulseInOut.SetPulse(1, i);
    ///                 pulseInOut.SetPulse(2, i);
    ///                 pulseInOut.SetPulse(3, i);
    ///             }
    ///
    ///             Debug.Print("Program Started");
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>    
    public class PulseInOut : GTM.DaisyLinkModule
    {
        // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
        // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2CBus), you must do a few more steps
        // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
        // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
        // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

        private const byte GHI_DAISYLINK_MANUFACTURER = 0x10;
        private const byte GHI_DAISYLINK_TYPE_PULSE = 0x06;
        private const byte GHI_DAISYLINK_VERSION_PULSE = 0x01;

        const int REGISTER_OFFSET = 8;
        const int REGISTER_CHANNEL_HIGH = REGISTER_OFFSET;
        const int REGISTER_CHANNEL_LOW = REGISTER_CHANNEL_HIGH + 2 * 8;
        const int REGISTER_PERIOD_PWM012_FREQUENCY = REGISTER_CHANNEL_LOW + 2 * 8;

        const int REGISTER_PERIOD_PWM345_FREQUENCY = REGISTER_PERIOD_PWM012_FREQUENCY + 4 * 1;
        const int REGISTER_PERIOD_PWM67_FREQUENCY = REGISTER_PERIOD_PWM345_FREQUENCY + 4 * 1;
        const int REGISTER_PWM_PULSE = REGISTER_PERIOD_PWM67_FREQUENCY + 4 * 1;

        /// <summary>Constructor</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public PulseInOut(int socketNumber)
            : base(socketNumber, GHI_DAISYLINK_MANUFACTURER, GHI_DAISYLINK_TYPE_PULSE, GHI_DAISYLINK_VERSION_PULSE, GHI_DAISYLINK_VERSION_PULSE, 50, "PulseIO")
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            char[] types = { 'X', 'Y' };
            socket.EnsureTypeIsSupported(types, this);
        }

        /// <summary>
        /// Reads the current PWM wave from Inputs 1-8
        /// </summary>
        /// <param name="input_id">The input to read from. 1-8.</param>
        /// <param name="high_time">The amount of time the wave is high in microseconds.</param>
        /// <param name="low_time">The amount of time the wave is low in microseconds.</param>
        public void ReadChannel(int input_id, out int high_time, out int low_time)
        {
            // As a user, channel will start from 1....8
            byte register = (byte)(REGISTER_OFFSET + (input_id - 1) * 2);
            high_time = (ushort)(ReadRegister(register) | (ReadRegister((byte)(register + 1)) << 8));
            low_time = (ushort)(ReadRegister((byte)(register + 16)) | (ReadRegister((byte)(register + 16 + 1)) << 8));

            high_time *= 10;
            low_time *= 10;
        }

        //public void SetPulse(uint period_nanosecond, uint highTime_nanosecond);
        /// <summary>
        /// Sets a PWM pulse on the passed in pin.
        /// </summary>
        /// <param name="pwm_id">The pin to set.</param>
        /// <param name="period_microsec">Length of the perion in microseconds.</param>
        /// <param name="highTime_microse">The length of time for the wave to be high, in microseconds.</param>
        public void SetPulse(int pwm_id, uint period_microsec, uint highTime_microse)
        {
            byte register_period = (byte)(REGISTER_PERIOD_PWM012_FREQUENCY - REGISTER_OFFSET);
            byte register_highTime = (byte)(REGISTER_PWM_PULSE - REGISTER_OFFSET + (pwm_id - 1) * 4);

            Write((byte)(register_period + 0 + DaisyLinkOffset), (byte)period_microsec,
                (byte)(period_microsec >> 8),
                (byte)(period_microsec >> 16),
                (byte)(period_microsec >> 24));
            Write((byte)(register_highTime + 0 + DaisyLinkOffset), (byte)highTime_microse,
               (byte)(highTime_microse >> 8),
               (byte)(highTime_microse >> 16),
               (byte)(highTime_microse >> 24));

        }
        /// <summary>
        /// Sets an output pin's PWM 1-8.
        /// </summary>
        /// <param name="pwm_id">The PWM channel to set. 1-8.</param>
        /// <param name="highTime_microse">The ammount of time for the pin to be high, in microseconds.</param>
        public void SetPulse(int pwm_id, ushort highTime_microse)
        {
            // As a user side, PWM_ID will start from 1....8
            byte register = (byte)(REGISTER_PWM_PULSE - REGISTER_OFFSET + (pwm_id - 1) * 4);
            //WriteRegister((byte)(register + 0), (byte)pulse_microsec);
            //WriteRegister((byte)(register + 1), (byte)(pulse_microsec >> 8));
            //WriteRegister((byte)(register + 2), (byte)(pulse_microsec >> 16));
            //WriteRegister((byte)(register + 3), (byte)(pulse_microsec >> 24));

            Write((byte)(register + 0 + DaisyLinkOffset), (byte)highTime_microse,
                (byte)(highTime_microse >> 8),
                (byte)(highTime_microse >> 16),
                (byte)(highTime_microse >> 24));
        }

        ///// <summary>
        ///// Sets the frequency for the PWM pins.
        ///// </summary>
        ///// <param name="freqHz">Frequency in Hz.</param>
        //public void SetFrequency(uint freqHz)
        //{
        //    UInt32 period;
        //    if (freqHz != 0) period = 1000000 / freqHz;
        //    else period = 0;
        //    byte register = (byte)(REGISTER_PERIOD_PWM012_FREQUENCY - REGISTER_OFFSET);
        //    WriteRegister((byte)(register + 0), (byte)period);
        //    WriteRegister((byte)(register + 1), (byte)(period >> 8));
        //    WriteRegister((byte)(register + 2), (byte)(period >> 16));
        //    WriteRegister((byte)(register + 3), (byte)(period >> 24));
        //}

        #region Generic Daisylink Functions
        /// <summary>
        /// Writes to the daisylink register specified by the address. Does not allow writing to the reserved registers.
        /// </summary>
        /// <param name="address">Address of the register.</param>
        /// <param name="writebuffer">Byte to write.</param>
        private void WriteRegister(byte address, byte writebuffer)
        {
            Write((byte)(DaisyLinkOffset + address), (byte)writebuffer);
        }

        /// <summary>
        /// Reads a byte from the specified register. Allows reading of reserved registers.
        /// </summary>
        /// <param name="memoryaddress">Address of the register.</param>
        /// <returns></returns>
        private byte ReadRegister(byte memoryaddress)
        {
            return Read(memoryaddress);
        }
        #endregion
    }
}