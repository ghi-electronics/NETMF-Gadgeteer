using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A PulseInOut module for Microsoft .NET Gadgeteer
    /// </summary>
    public class PulseInOut : DaisyLinkModule
    {
        private const byte GHI_DAISYLINK_MANUFACTURER = 0x10;
        private const byte GHI_DAISYLINK_TYPE_PULSE = 0x06;
        private const byte GHI_DAISYLINK_VERSION_PULSE = 0x01;

        private const byte REGISTER_OFFSET = 8;
        private const byte REGISTER_CHANNEL_HIGH = PulseInOut.REGISTER_OFFSET;
        private const byte REGISTER_CHANNEL_LOW = PulseInOut.REGISTER_CHANNEL_HIGH + 2 * 8;
        private const byte REGISTER_PERIOD_PWM012_FREQUENCY = PulseInOut.REGISTER_CHANNEL_LOW + 2 * 8;
        private const byte REGISTER_PERIOD_PWM345_FREQUENCY = PulseInOut.REGISTER_PERIOD_PWM012_FREQUENCY + 4 * 1;
        private const byte REGISTER_PERIOD_PWM67_FREQUENCY = PulseInOut.REGISTER_PERIOD_PWM345_FREQUENCY + 4 * 1;
        private const byte REGISTER_PWM_PULSE = PulseInOut.REGISTER_PERIOD_PWM67_FREQUENCY + 4 * 1;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public PulseInOut(int socketNumber) : base(socketNumber, PulseInOut.GHI_DAISYLINK_MANUFACTURER, PulseInOut.GHI_DAISYLINK_TYPE_PULSE, PulseInOut.GHI_DAISYLINK_VERSION_PULSE, PulseInOut.GHI_DAISYLINK_VERSION_PULSE, 50, "PulseInOut")
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported(new char[] { 'X', 'Y' }, this);
        }

        /// <summary>
        /// Reads the current PWM wave on the given input.
        /// </summary>
        /// <param name="input">The input to read from.</param>
        /// <param name="highTime">The amount of time the wave is high in microseconds.</param>
        /// <param name="lowTime">The amount of time the wave is low in microseconds.</param>
        public void ReadChannel(int input, out int highTime, out int lowTime)
        {
            var register = PulseInOut.REGISTER_OFFSET + (input - 1) * 2;

            highTime = this.ReadRegister(register) * 10;
            lowTime = this.ReadRegister(register + 16) * 10;
        }

        /// <summary>
        /// Starts a pulse on the given pwm channel.
        /// </summary>
        /// <param name="pwm">The channel to pulse on.</param>
        /// <param name="period">The period of the pulse.</param>
        /// <param name="highTime">The amount of time for the pin to be high in microseconds.</param>
        public void SetPulse(int pwm, uint period, uint highTime)
        {
            this.WriteRegister(PulseInOut.REGISTER_PERIOD_PWM012_FREQUENCY - PulseInOut.REGISTER_OFFSET + DaisyLinkModule.DaisyLinkOffset, period);
            this.WriteRegister(PulseInOut.REGISTER_PWM_PULSE - PulseInOut.REGISTER_OFFSET + (pwm - 1) * 4 + DaisyLinkModule.DaisyLinkOffset, highTime);
        }

        /// <summary>
        /// Starts a pulse on the given pwm channel.
        /// </summary>
        /// <param name="pwm">The channel to pulse on.</param>
        /// <param name="highTime">The amount of time for the pin to be high in microseconds.</param>
        public void SetPulse(int pwm, uint highTime)
        {
            this.WriteRegister(PulseInOut.REGISTER_PWM_PULSE - PulseInOut.REGISTER_OFFSET + (pwm - 1) * 4 + DaisyLinkModule.DaisyLinkOffset, highTime);
        }

        private void WriteRegister(int address, uint value)
        {
            this.Write((byte)address, (byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24));
        }

        private int ReadRegister(int address)
        {
            return this.Read((byte)address) | (this.Read((byte)(address + 1)) << 8);
        }
    }
}