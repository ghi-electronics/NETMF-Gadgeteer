using System;
using System.Collections;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A IO60P16 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class IO60P16 : GTM.Module
    {
        private const byte INPUT_PORT_0_REGISTER = 0x00;
        private const byte OUTPUT_PORT_0_REGISTER = 0x08;
        private const byte INTERRUPT_PORT_0_REGISTER = 0x10;
        private const byte PORT_SELECT_REGISTER = 0x18;
        private const byte INTERRUPT_MASK_REGISTER = 0x19;

        private const byte PIN_DIRECTION_REGISTER = 0x1C;
        private const byte ENABLE_PWM_REGISTER = 0x1A;
        private const byte PWM_SELECT_REGISTER = 0x28;
        private const byte PWM_CONFIG = 0x29;
        private const byte PERIOD_REGISTER = 0x2A;
        private const byte PULSE_WIDTH_REGISTER = 0x2B;

        private const byte CLOCK_SOURCE = 0x3;

        private GTI.I2CBus io60Chip;
        private GTI.InterruptInput interrupt;
        private ArrayList interruptHandlers;
        private byte[] write2;
        private byte[] write1;
        private byte[] read1;
        private byte[] pwms;

        /// <summary>
        /// The delegate that represents interrupt events.
        /// </summary>
        /// <param name="state">The pin state.</param>
        public delegate void InterruptHandler(bool state);

        private class InterruptRegistraton
        {
            public GTI.InterruptMode mode;
            public InterruptHandler handler;
            public byte pin;
        }

        /// <summary>
        /// The possible IOStates a pin can be in.
        /// </summary>
        public enum IOState
        {
            /// <summary>
            /// The pin is an interrupt input.
            /// </summary>
            InputInterrupt,
            /// <summary>
            /// The pin is an input.
            /// </summary>
            Input,
            /// <summary>
            /// The pin is an output.
            /// </summary>
            Output,
            /// <summary>
            /// The pin is a pwm output.
            /// </summary>
            Pwm
        }

        /// <summary>
        /// The possible resistor modes for a pin.
        /// </summary>
        public enum ResistorMode
        {
            /// <summary>
            /// Pull up.
            /// </summary>
            ResistivePullUp = 0x1D,
            /// <summary>
            /// Pull down.
            /// </summary>
            ResistivePullDown = 0x1E,
            /// <summary>
            /// Open drain high.
            /// </summary>
            OpenDrainHigh = 0x1F,
            /// <summary>
            /// Open drain low.
            /// </summary>
            OpenDrainLow = 0x20,
            /// <summary>
            /// Strong drive.
            /// </summary>
            StrongDrive = 0x21,
            /// <summary>
            /// Slow strong drive.
            /// </summary>
            SlowStrongDrive = 0x22,
            /// <summary>
            /// High impedance.
            /// </summary>
            HighImpedence = 0x23
        }

        private byte GetPort(byte pin)
        {
            return (byte)(pin >> 4);
        }

        private byte GetMask(byte pin)
        {
            return (byte)(1 << (pin & 0x0F));
        }

        private void WriteRegister(byte register, byte value)
        {
            lock (this.io60Chip)
            {
                write2[0] = register;
                write2[1] = value;
                this.io60Chip.Write(write2);
            }
        }

        private byte ReadRegister(byte register)
        {
            byte result;

            lock (this.io60Chip)
            {
                write1[0] = register;
                this.io60Chip.WriteRead(write1, read1);
                result = read1[0];
            }

            return result;
        }

        private byte[] ReadRegisters(byte register, uint count)
        {
            byte[] result = new byte[count];

            lock (this.io60Chip)
            {
                write1[0] = register;
                this.io60Chip.WriteRead(write1, result);
            }

            return result;
        }

        private void OnInterrupt(GTI.InterruptInput sender, bool value)
        {
            ArrayList interruptedPins = new ArrayList();

            byte[] intPorts = this.ReadRegisters(IO60P16.INTERRUPT_PORT_0_REGISTER, 8);
            for (byte i = 0; i < 8; i++)
                for (int j = 1, k = 0; j <= 128; j <<= 1, k++)
                    if ((intPorts[i] & j) != 0)
                        interruptedPins.Add((i << 4) | k);

            foreach (int pin in interruptedPins)
            {
                lock (this.interruptHandlers)
                {
                    foreach (InterruptRegistraton reg in this.interruptHandlers)
                    {
                        if (reg.pin == pin)
                        {
                            bool val = this.ReadDigital((byte)pin);
                            if ((reg.mode == GTI.InterruptMode.RisingEdge && val) || (reg.mode == GTI.InterruptMode.FallingEdge && !val) || reg.mode == GTI.InterruptMode.RisingAndFallingEdge)
                                reg.handler(val);
                        }
                    }
                }
            }
        }

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public IO60P16(int socketNumber)
        {
            var socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('X', this);

            this.interruptHandlers = new ArrayList();
            this.write2 = new byte[2];
            this.write1 = new byte[1];
            this.read1 = new byte[1];
            this.pwms = new byte[30] { 0x60, 0, 0x61, 1, 0x62, 2, 0x63, 3, 0x64, 4, 0x65, 5, 0x66, 6, 0x67, 7, 0x70, 8, 0x71, 9, 0x72, 10, 0x73, 11, 0x74, 12, 0x75, 13, 0x76, 14 };

            this.io60Chip = new GTI.SoftwareI2CBus(socket, Socket.Pin.Five, Socket.Pin.Four, 0x20, 400, this);

            this.interrupt = GTI.InterruptInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.On, GTI.ResistorMode.Disabled, GTI.InterruptMode.RisingEdge, null);
            this.interrupt.Interrupt += this.OnInterrupt;
        }

        /// <summary>
        /// Creates a digital input on the given pin.
        /// </summary>
        /// <param name="port">The port to create the interface on.</param>
        /// <param name="pin">The pin to create the interface on.</param>
        /// <param name="glitchFilterMode">The glitch filter mode for the interface.</param>
        /// <param name="resistorMode">The resistor mode for the interface.</param>
        /// <returns>The new interface.</returns>
        public GTI.DigitalInput CreateDigitalInput(int port, int pin, GTI.GlitchFilterMode glitchFilterMode, GTI.ResistorMode resistorMode)
        {
            if (port < 0 || port > 7) throw new ArgumentOutOfRangeException("port", "port must be between 0 and 7.");
            if (pin < 0 || pin > 7) throw new ArgumentOutOfRangeException("pin", "pin must be between 0 and 7.");

            return new DigitalInput((byte)((port << 4) | pin), glitchFilterMode, resistorMode, this);
        }

        /// <summary>
        /// Creates a digital output on the given pin.
        /// </summary>
        /// <param name="port">The port to create the interface on.</param>
        /// <param name="pin">The pin to create the interface on.</param>
        /// <param name="initialState">The initial state for the interface.</param>
        /// <returns>The new interface.</returns>
        public GTI.DigitalOutput CreateDigitalOutput(int port, int pin, bool initialState)
        {
            if (port < 0 || port > 7) throw new ArgumentOutOfRangeException("port", "port must be between 0 and 7.");
            if (pin < 0 || pin > 7) throw new ArgumentOutOfRangeException("pin", "pin must be between 0 and 7.");

            return new DigitalOutput((byte)((port << 4) | pin), initialState, this);
        }

        /// <summary>
        /// Creates a digital input/output on the given pin.
        /// </summary>
        /// <param name="port">The port to create the interface on.</param>
        /// <param name="pin">The pin to create the interface on.</param>
        /// <param name="initialState">The initial state for the interface.</param>
        /// <param name="glitchFilterMode">The glitch filter mode for the interface.</param>
        /// <param name="resistorMode">The resistor mode for the interface.</param>
        /// <returns>The new interface.</returns>
        public GTI.DigitalIO CreateDigitalIO(int port, int pin, bool initialState, GTI.GlitchFilterMode glitchFilterMode, GTI.ResistorMode resistorMode)
        {
            if (port < 0 || port > 7) throw new ArgumentOutOfRangeException("port", "port must be between 0 and 7.");
            if (pin < 0 || pin > 7) throw new ArgumentOutOfRangeException("pin", "pin must be between 0 and 7.");

            return new DigitalIO((byte)((port << 4) | pin), initialState, glitchFilterMode, resistorMode, this);
        }

        /// <summary>
        /// Creates an interrupt input on the given pin.
        /// </summary>
        /// <param name="port">The port to create the interface on.</param>
        /// <param name="pin">The pin to create the interface on.</param>
        /// <param name="glitchFilterMode">The glitch filter mode for the interface.</param>
        /// <param name="resistorMode">The resistor mode for the interface.</param>
        /// <param name="interruptMode">The interrupt mode for the interface.</param>
        /// <returns>The new interface.</returns>
        public GTI.InterruptInput CreateInterruptInput(int port, int pin, GTI.GlitchFilterMode glitchFilterMode, GTI.ResistorMode resistorMode, GTI.InterruptMode interruptMode)
        {
            if (port < 0 || port > 7) throw new ArgumentOutOfRangeException("port", "port must be between 0 and 7.");
            if (pin < 0 || pin > 7) throw new ArgumentOutOfRangeException("pin", "pin must be between 0 and 7.");

            return new InterruptInput((byte)((port << 4) | pin), glitchFilterMode, resistorMode, interruptMode, this);
        }

        /// <summary>
        /// Creates a pwm output on the given pin.
        /// </summary>
        /// <param name="pwm">The pwm to create the interface on.</param>
        /// <returns>The new interface.</returns>
        public GTI.PwmOutput CreatePwmOutput(int pwm)
        {
            if (pwm < 0 || pwm > 15) throw new ArgumentOutOfRangeException("port", "port must be between 0 and 15.");

            byte pin = 0;
            for (var i = 1; i < 30; i += 2)
                if (this.pwms[i] == pwm)
                    pin = this.pwms[i - 1];

            return new PwmOutput(pin, false, this);
        }

        private void RegisterInterruptHandler(byte pin, GTI.InterruptMode mode, InterruptHandler handler)
        {
            InterruptRegistraton reg = new InterruptRegistraton();
            reg.handler = handler;
            reg.mode = mode;
            reg.pin = pin;

            lock (this.interruptHandlers)
                this.interruptHandlers.Add(reg);
        }

        private void SetIOMode(byte pin, IOState state, GTI.ResistorMode resistorMode)
        {
            switch (resistorMode)
            {
                case GTI.ResistorMode.Disabled: this.SetIOMode(pin, state, ResistorMode.HighImpedence); break;
                case GTI.ResistorMode.PullDown: this.SetIOMode(pin, state, ResistorMode.ResistivePullDown); break;
                case GTI.ResistorMode.PullUp: this.SetIOMode(pin, state, ResistorMode.ResistivePullUp); break;
            }
        }

        private void SetIOMode(byte pin, IOState state, ResistorMode resistorMode)
        {
            this.WriteRegister(IO60P16.PORT_SELECT_REGISTER, this.GetPort(pin));

            byte mask = this.GetMask(pin);
            byte val = this.ReadRegister(IO60P16.ENABLE_PWM_REGISTER);

            if (state == IOState.Pwm)
            {
                this.WriteRegister(IO60P16.ENABLE_PWM_REGISTER, (byte)(val | mask));

                this.WriteDigital(pin, true);

                byte pwm = 255;
                for (var i = 0; i < 30; i += 2)
                    if (this.pwms[i] == pin)
                        pwm = this.pwms[i + 1];

                this.WriteRegister(IO60P16.PWM_SELECT_REGISTER, pwm);
                this.WriteRegister(IO60P16.PWM_CONFIG, IO60P16.CLOCK_SOURCE); //93.75KHz clock

                val = this.ReadRegister((byte)IO60P16.ResistorMode.StrongDrive);
                this.WriteRegister((byte)IO60P16.ResistorMode.StrongDrive, (byte)(val | mask));
            }
            else
            {
                this.WriteRegister(IO60P16.ENABLE_PWM_REGISTER, (byte)(val & ~mask));
                val = this.ReadRegister(IO60P16.PIN_DIRECTION_REGISTER);

                if (state == IOState.Output)
                {
                    this.WriteRegister(IO60P16.PIN_DIRECTION_REGISTER, (byte)(val & ~mask));

                    val = this.ReadRegister((byte)IO60P16.ResistorMode.StrongDrive);
                    this.WriteRegister((byte)IO60P16.ResistorMode.StrongDrive, (byte)(val | mask));
                }
                else
                {
                    this.WriteRegister(IO60P16.PIN_DIRECTION_REGISTER, (byte)(val | mask));

                    val = this.ReadRegister((byte)resistorMode);
                    this.WriteRegister((byte)resistorMode, (byte)(val | mask));
                }
            }

            val = this.ReadRegister(IO60P16.INTERRUPT_MASK_REGISTER);
            if (state == IOState.InputInterrupt)
                this.WriteRegister(IO60P16.INTERRUPT_MASK_REGISTER, (byte)(val & ~mask));
            else
                this.WriteRegister(IO60P16.INTERRUPT_MASK_REGISTER, (byte)(val | mask));
        }

        //We're using the 93.75KHz clock source because it gives a good resolution around the 1KHz frequency
        //while still allowing the user to select frequencies such as 10KHz, but with reduced duty cycle
        //resolution.
        private void SetPWM(byte pin, double frequency, double dutyCycle)
        {
            byte pwm = 255;
            for (var i = 0; i < 30; i += 2)
                if (this.pwms[i] == pin)
                    pwm = this.pwms[i + 1];

            this.WriteRegister((byte)(IO60P16.PWM_SELECT_REGISTER), pwm);

            byte period = (byte)(93750 / frequency);

            this.WriteRegister(IO60P16.PERIOD_REGISTER, period);
            this.WriteRegister((byte)(IO60P16.PULSE_WIDTH_REGISTER), (byte)(period * dutyCycle));
        }

        private bool ReadDigital(byte pin)
        {
            byte b = this.ReadRegister((byte)(IO60P16.INPUT_PORT_0_REGISTER + this.GetPort(pin)));

            return (b & this.GetMask(pin)) != 0;
        }

        private void WriteDigital(byte pin, bool value)
        {
            byte b = this.ReadRegister((byte)(IO60P16.OUTPUT_PORT_0_REGISTER + this.GetPort(pin)));

            if (value)
                b |= this.GetMask(pin);
            else
                b = (byte)(b & ~this.GetMask(pin));

            this.WriteRegister((byte)(IO60P16.OUTPUT_PORT_0_REGISTER + this.GetPort(pin)), b);
        }

        private class DigitalInput : GTI.DigitalInput
        {
            private IO60P16 io60;
            private byte pin;

            public DigitalInput(byte pin, GTI.GlitchFilterMode glitchFilter, GTI.ResistorMode resistorMode, IO60P16 io60)
            {
                this.io60 = io60;
                this.pin = pin;

                this.io60.SetIOMode(this.pin, IO60P16.IOState.Input, resistorMode);
            }

            public override bool Read()
            {
                return this.io60.ReadDigital(this.pin);
            }
        }

        private class DigitalOutput : GTI.DigitalOutput
        {
            private IO60P16 io60;
            private byte pin;

            public DigitalOutput(byte pin, bool initialState, IO60P16 io60)
            {
                this.io60 = io60;
                this.pin = pin;

                this.io60.SetIOMode(this.pin, IO60P16.IOState.Output, GTI.ResistorMode.Disabled);
                this.Write(initialState);
            }

            public override bool Read()
            {
                return this.io60.ReadDigital(this.pin);
            }

            public override void Write(bool state)
            {
                this.io60.WriteDigital(this.pin, state);
            }
        }

        private class DigitalIO : GTI.DigitalIO
        {
            private IO60P16 io60;
            private byte pin;
            private GTI.IOMode mode;
            private GTI.ResistorMode resistorMode;

            public DigitalIO(byte pin, bool initialState, GTI.GlitchFilterMode glitchFilter, GTI.ResistorMode resistorMode, IO60P16 io60)
            {
                this.io60 = io60;
                this.pin = pin;
                this.resistorMode = resistorMode;
                this.Mode = GTI.IOMode.Input;
                this.io60.WriteDigital(this.pin, initialState);
            }

            public override bool Read()
            {
                this.Mode = GTI.IOMode.Input;
                return this.io60.ReadDigital(this.pin);
            }

            public override void Write(bool state)
            {
                this.Mode = GTI.IOMode.Output;
                this.io60.WriteDigital(this.pin, state);
            }

            public override GTI.IOMode Mode
            {
                get
                {
                    return this.mode;
                }
                set
                {
                    this.mode = value;
                    this.io60.SetIOMode(this.pin, this.mode == GTI.IOMode.Input ? IO60P16.IOState.Input : IO60P16.IOState.Output, this.resistorMode);
                }
            }
        }

        private class PwmOutput : GTI.PwmOutput
        {
            private IO60P16 io60;
            private byte pin;
            private bool isActive;

            public PwmOutput(byte pin, bool invert, IO60P16 io60)
            {
                this.io60 = io60;
                this.pin = pin;
                this.isActive = false;
            }

            public override void Set(double frequency, double dutyCycle)
            {
                if (frequency <= 0) throw new ArgumentOutOfRangeException("frequency", "frequency must be positive.");
                if (dutyCycle < 0 || dutyCycle > 1) throw new ArgumentOutOfRangeException("dutyCycle", "dutyCycle must be between zero and one.");

                this.IsActive = true;

                this.io60.SetPWM(this.pin, frequency, dutyCycle);
            }

            public override void Set(uint period, uint highTime, GTI.PwmScaleFactor factor)
            {
                if (period == 0) throw new ArgumentOutOfRangeException("period", "pedior must be positive.");
                if (highTime > period) throw new ArgumentOutOfRangeException("highTime", "highTime must be no more than period.");

                double frequency = 0;

                switch (factor)
                {
                    case GTI.PwmScaleFactor.Milliseconds: frequency = 1000 / period; break;
                    case GTI.PwmScaleFactor.Microseconds: frequency = 1000000 / period; break;
                    case GTI.PwmScaleFactor.Nanoseconds: frequency = 1000000000 / period; break;
                }

                this.Set(frequency, (double)highTime / (double)period);
            }

            public override bool IsActive
            {
                get
                {
                    return this.isActive;
                }
                set
                {
                    if (this.isActive == value)
                        return;

                    this.isActive = !this.isActive;

                    if (this.isActive)
                    {
                        this.io60.SetIOMode(this.pin, IO60P16.IOState.Pwm, GTI.ResistorMode.Disabled);
                        this.io60.SetPWM(this.pin, 1, 0.5);
                    }
                    else
                    {
                        this.io60.SetIOMode(this.pin, IO60P16.IOState.Input, GTI.ResistorMode.Disabled);
                    }
                }
            }
        }

        private class InterruptInput : GTI.InterruptInput
        {
            private IO60P16 io60;
            private byte pin;

            protected override void OnInterruptFirstSubscribed()
            {

            }

            protected override void OnInterruptLastUnsubscribed()
            {

            }

            public InterruptInput(byte pin, GTI.GlitchFilterMode glitchFilter, GTI.ResistorMode resistorMode, GTI.InterruptMode interruptMode, IO60P16 io60)
            {
                this.io60 = io60;
                this.pin = pin;

                this.io60.SetIOMode(this.pin, IO60P16.IOState.InputInterrupt, resistorMode);
                this.io60.RegisterInterruptHandler(this.pin, interruptMode, this.RaiseInterrupt);
            }

            public override bool Read()
            {
                return this.io60.ReadDigital(this.pin);
            }
        }
    }
}
