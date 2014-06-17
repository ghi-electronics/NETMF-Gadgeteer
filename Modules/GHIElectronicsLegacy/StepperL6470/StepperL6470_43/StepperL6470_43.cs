using System;
using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A StepperL6470 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class StepperL6470 : GTM.Module
    {
        private GTI.Spi spi;
        private GTI.DigitalInput busyPin;
        private GTI.DigitalOutput resetPin;
        private GTI.DigitalOutput stepClock;
        private byte[] send;
        private byte[] receive;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public StepperL6470(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('S', this);

            this.spi = GTI.SpiFactory.Create(socket, new GTI.SpiConfiguration(false, 1000, 1000, true, true, 5000), GTI.SpiSharing.Shared, socket, Socket.Pin.Six, this);
            this.busyPin = GTI.DigitalInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.PullUp, this);
            this.resetPin = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Four, true, this);
            this.stepClock = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Five, false, this);

            this.send = new byte[1];
            this.receive = new byte[1];

            this.InitializeChip();

            this.Reset();
        }

        private void InitializeChip()
        {
            var registers = new Registers();

            /* Acceleration rate settings to 466 steps/s2, range 14.55 to 59590 steps/s2 */
            registers.Acc = AccDecStepsToPar(466);

            /* Deceleration rate settings to 466 steps/s2, range 14.55 to 59590 steps/s2 */
            registers.Dec = AccDecStepsToPar(466);

            /* Maximum speed settings to 488 steps/s, range 15.25 to 15610 steps/s */
            registers.MaxSpeed = MaxSpdStepsToPar(488);

            /* Minimum speed settings to 0 steps/s, range 0 to 976.3 steps/s */
            registers.MinSpeed = MinSpdStepsToPar(0);

            /* Full step speed settings 252 steps/s, range 7.63 to 15625 steps/s */
            registers.FsSpd = FSSpdStepsToPar(252);

            /* Hold duty cycle (torque) settings to 10%, range 0 to 99.6% */
            registers.KValHold = KvalPercToPar(10);

            /* Run duty cycle (torque) settings to 10%, range 0 to 99.6% */
            registers.KValRun = KvalPercToPar(10);

            /* Acceleration duty cycle (torque) settings to 10%, range 0 to 99.6% */
            registers.KValAcc = KvalPercToPar(10);

            /* Deceleration duty cycle (torque) settings to 10%, range 0 to 99.6% */
            registers.KvalDec = KvalPercToPar(10);

            /* Intersect speed settings for BEMF compensation to 200 steps/s, range 0 to 3906 steps/s */
            registers.IntSpd = IntSpdStepsToPar(200);

            /* BEMF start slope settings for BEMF compensation to 0.038% step/s, range 0 to 0.4% s/step */
            registers.StSlp = BemfSlopePercToPar(0.038f);

            /* BEMF final acc slope settings for BEMF compensation to 0.063% step/s, range 0 to 0.4% s/step */
            registers.FnSlpAcc = BemfSlopePercToPar(0.063f);

            /* BEMF final dec slope settings for BEMF compensation to 0.063% step/s, range 0 to 0.4% s/step */
            registers.FnSlpDec = BemfSlopePercToPar(0.063f);

            /* Thermal compensation param settings to 1, range 1 to 1.46875 */
            registers.KTherm = KThermToPar(1);

            /* Overcurrent threshold settings to 1500mA */
            registers.OcdTh = (byte)OvercurrentDetectionThreshold.Thershold1500mA;

            /* Stall threshold settings to 1000mA, range 31.25 to 4000mA */
            registers.StallTh = StallThToPar(1000);

            /* Step mode settings to 128 microsteps */
            registers.StepMode = (byte)StepSelect.Select_1_128;

            /* Alarm settings - all alarms enabled */
            registers.AlarmEn = (byte)AlarmEnable.Overcurrent | (byte)AlarmEnable.ThermalShutdown | (byte)AlarmEnable.ThermalWarning | (byte)AlarmEnable.UnderVoltage | 
                                (byte)AlarmEnable.StallDetectA | (byte)AlarmEnable.StallDetectB | (byte)AlarmEnable.SwTurnOn | (byte)AlarmEnable.WrongNperf;

            /* Internal oscillator, 2MHz OSCOUT clock, supply voltage compensation disabled, *
             * overcurrent shutdown enabled, slew-rate = 290 V/us, PWM frequency = 15.6kHz   */
            registers.Config = (ushort)ConfigOscManagement.Int16MHzOscout2MHz | (ushort)ConfigSwMode.HardStop | (ushort)ConfigEnVscomp.Disable | 
                               (ushort)ConfigOcSd.Enable | (ushort)ConfigPowSr.V290 | (ushort)ConfigFPwmInt.Div2 | (ushort)ConfigFPwmDec.Mul_1;

            /* Program all dSPIN registers */
            this.SetRegisters(registers);
        }

        private byte WriteByte(byte data)
        {
            this.send[0] = data;

            this.spi.WriteRead(this.send, this.receive);

            return this.receive[0];
        }

        /// <summary>
        /// Resets the module
        /// </summary>
        public void Reset()
        {
            this.resetPin.Write(false);
            Thread.Sleep(500);
            
            this.resetPin.Write(true);
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Send NOP operation code to the module
        /// </summary>
        public void Nop()
        {
            this.WriteByte((byte)Command.Nop);
        }

        /// <summary>
        /// Sets a parameter to the passed in value
        /// </summary>
        /// <param name="param">Parameter to set</param>
        /// <param name="value">Value to set</param>
        public void SetParam(RegisterAddress param, uint value)
        {
            this.WriteByte((byte)((byte)Command.SetParam | (byte)param));

            switch (param)
            {
                case RegisterAddress.AbsPos:
                case RegisterAddress.Mark:
                case RegisterAddress.Speed:
                    this.WriteByte((byte)(value >> 16));
                    this.WriteByte((byte)(value >> 8));
                    this.WriteByte((byte)value);

                    break;

                case RegisterAddress.Acc:
                case RegisterAddress.Dec:
                case RegisterAddress.MaxSpeed:
                case RegisterAddress.MinSpeed:
                case RegisterAddress.FsSpd:
                case RegisterAddress.IntSpd:
                case RegisterAddress.Config:
                case RegisterAddress.Status:
                    this.WriteByte((byte)(value >> 8));
                    this.WriteByte((byte)value);

                    break;

                default:
                    this.WriteByte((byte)value);

                    break;
            }
        }

        /// <summary>
        /// Get a value for a given parameter
        /// </summary>
        /// <param name="param">Parameter to retrieve</param>
        /// <returns></returns>
        public uint GetParam(RegisterAddress param)
        {
            uint rx = (uint)this.WriteByte((byte)((byte)Command.GetParam | (byte)param)) << 24;
            
            switch (param)
            {
                case RegisterAddress.AbsPos:
                case RegisterAddress.Mark:
                case RegisterAddress.Speed:
                    rx |= (uint)((this.WriteByte(0x00) << 16) | (this.WriteByte(0x00) << 8) | this.WriteByte(0x00));

                    break;

                case RegisterAddress.Acc:
                case RegisterAddress.Dec:
                case RegisterAddress.MaxSpeed:
                case RegisterAddress.MinSpeed:
                case RegisterAddress.FsSpd:
                case RegisterAddress.IntSpd:
                case RegisterAddress.Config:
                case RegisterAddress.Status:
                    rx |= (uint)((this.WriteByte(0x00) << 8) | this.WriteByte(0x00));

                    break;

                default:
                    rx |= (uint)this.WriteByte(0x00);

                    break;
            }

            return rx;
        }

        /// <summary>
        /// Runs the motor
        /// </summary>
        /// <param name="direction">Direction of the motor</param>
        /// <param name="speed">Speed of the motor</param>
        public void Run(Direction direction, uint speed)
        {
            this.WriteByte((byte)((byte)Command.Run | (byte)direction));
            
            this.WriteByte((byte)(speed >> 16));
            this.WriteByte((byte)(speed >> 8));
            this.WriteByte((byte)(speed));
        }

        /// <summary>
        /// Steps the motor while the stepClock pin is high
        /// </summary>
        /// <param name="direction">Direction of the motor</param>
        public void StepClock(Direction direction)
        {
            this.WriteByte((byte)((byte)(Command.StepClock) | (byte)direction));
        }

        /// <summary>
        /// Moves the motor in the passed in direction for the passed in steps
        /// </summary>
        /// <param name="direction">Direction to move the motor</param>
        /// <param name="nStep">Number of steps to move</param>
        public void Move(Direction direction, uint nStep)
        {
            this.WriteByte((byte)((byte)Command.Move | (byte)direction));
         
            this.WriteByte((byte)(nStep >> 16));
            this.WriteByte((byte)(nStep >> 8));
            this.WriteByte((byte)(nStep));
        }

        /// <summary>
        /// Sets the motor to a specific position
        /// </summary>
        /// <param name="absPos">Absolute position to move to</param>
        public void GoToPosition(uint absPos)
        {
            this.WriteByte((byte)Command.GoTo);

            this.WriteByte((byte)(absPos >> 16));
            this.WriteByte((byte)(absPos >> 8));
            this.WriteByte((byte)absPos);
        }

        /// <summary>
        /// Moves to a specific position, going in the passed in direction
        /// </summary>
        /// <param name="direction">Direction to move</param>
        /// <param name="absPos">Absolute position to move to</param>
        public void GoToDirection(Direction direction, uint absPos)
        {
            this.WriteByte((byte)((byte)Command.GoToDir | (byte)direction));
            
            this.WriteByte((byte)(absPos >> 16));
            this.WriteByte((byte)(absPos >> 8));
            this.WriteByte((byte)absPos);
        }

        /// <summary>
        /// Moves the motor until a specific action happens
        /// </summary>
        /// <param name="action">Action to stop on</param>
        /// <param name="direction">Direction to move</param>
        /// <param name="speed">Speed to move in</param>
        public void GoUntil(Action action, Direction direction, uint speed)
        {
            this.WriteByte((byte)((byte)Command.GoUntil | (byte)action | (byte)direction));

            this.WriteByte((byte)(speed >> 16));
            this.WriteByte((byte)(speed >> 8));
            this.WriteByte((byte)(speed));
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="action"></param>
        /// <param name="direction"></param>
        public void ReleaseSW(Action action, Direction direction)
        {
            this.WriteByte((byte)((byte)Command.ReleaseSw | (byte)action | (byte)direction));
        }

        /// <summary>
        /// Moves the motor to the home position
        /// </summary>
        public void GoHome()
        {
            this.WriteByte((byte)Command.GoHome);
        }

        /// <summary>
        /// Goes the motor to the mark position
        /// </summary>
        public void GoMark()
        {
            this.WriteByte((byte)Command.GoMark);
        }

        /// <summary>
        /// Resets the motor to the position
        /// </summary>
        public void ResetPos()
        {
            this.WriteByte((byte)Command.ResetPos);
        }

        /// <summary>
        /// Resets the device
        /// </summary>
        public void ResetDevice()
        {
            this.WriteByte((byte)Command.ResetDevice);
        }

        /// <summary>
        /// Soft stop operation
        /// </summary>
        public void SoftStop()
        {
            this.WriteByte((byte)Command.SoftStop);
        }

        /// <summary>
        /// Hard stop operation
        /// </summary>
        public void HardStop()
        {
            this.WriteByte((byte)Command.HardStop);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void SoftHiZ()
        {
            this.WriteByte((byte)Command.SoftHiz);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void HardHiZ()
        {
            this.WriteByte((byte)Command.HardHiz);
        }

        /// <summary>
        /// Gets the status
        /// </summary>
        /// <returns>Status</returns>
        public ushort GetStatus()
        {
            this.WriteByte((byte)Command.GetStatus);

            return (ushort)((this.WriteByte((byte)0x00) << 8) | this.WriteByte((byte)0x00));
        }

        /// <summary>
        /// Returns if the hardware is busy
        /// </summary>
        /// <returns>If the hardware is busy</returns>
        public bool BusyHW()
        {
            return !this.busyPin.Read();
        }

        /// <summary>
        /// Returns if the software is busy
        /// </summary>
        /// <returns>If the hardware is busy</returns>
        public byte BusySW()
        {
            return (byte)((this.GetStatus() & (ushort)StatusMask.Busy) == 0 ? 0x01 : 0x00);
        }

        /// <summary>
        /// Sets the registers to the passed in register values
        /// </summary>
        /// <param name="registers">Register values to set the values to</param>
        public void SetRegisters(Registers registers)
        {
            this.SetParam(RegisterAddress.AbsPos, registers.AbsPos);
            this.SetParam(RegisterAddress.ElPos, registers.ElPos);
            this.SetParam(RegisterAddress.Mark, registers.Mark);
            this.SetParam(RegisterAddress.Speed, registers.Speed);
            this.SetParam(RegisterAddress.Acc, registers.Acc);
            this.SetParam(RegisterAddress.Dec, registers.Dec);
            this.SetParam(RegisterAddress.MaxSpeed, registers.MaxSpeed);
            this.SetParam(RegisterAddress.MinSpeed, registers.MinSpeed);
            this.SetParam(RegisterAddress.FsSpd, registers.FsSpd);
            this.SetParam(RegisterAddress.KValHold, registers.KValHold);
            this.SetParam(RegisterAddress.KValRun, registers.KValRun);
            this.SetParam(RegisterAddress.KValAcc, registers.KValAcc);
            this.SetParam(RegisterAddress.KValDev, registers.KvalDec);
            this.SetParam(RegisterAddress.IntSpd, registers.IntSpd);
            this.SetParam(RegisterAddress.StSlp, registers.StSlp);
            this.SetParam(RegisterAddress.FnSlpAcc, registers.FnSlpAcc);
            this.SetParam(RegisterAddress.FnSlpDec, registers.FnSlpDec);
            this.SetParam(RegisterAddress.KTherm, registers.KTherm);
            this.SetParam(RegisterAddress.OcdTh, registers.OcdTh);
            this.SetParam(RegisterAddress.StallTh, registers.StallTh);
            this.SetParam(RegisterAddress.StepMode, registers.StepMode);
            this.SetParam(RegisterAddress.AlarmEn, registers.AlarmEn);
            this.SetParam(RegisterAddress.Config, registers.Config);
        }

		/// <summary>
		/// Converts speed steps to par.
		/// </summary>
		/// <param name="steps">The steps.</param>
		/// <returns>Steps expressed as par.</returns>
        public uint SpeedStepsToPar(uint steps)
        {
            return (uint)(steps * 67.108864 + 0.5);
        }

		/// <summary>
		/// Converts AccDec steps to par.
		/// </summary>
		/// <param name="steps">The steps.</param>
		/// <returns>Steps expressed as par.</returns>
        public ushort AccDecStepsToPar(uint steps)
        {
            return (ushort)(steps * 0.068719476736 + 0.5);
        }

		/// <summary>
		/// Converts max speed steps to par.
		/// </summary>
		/// <param name="steps">The steps.</param>
		/// <returns>Steps expressed as par.</returns>
        public ushort MaxSpdStepsToPar(uint steps)
        {
            return (ushort)(steps * 0.065536 + 0.5);
        }

		/// <summary>
		/// Converts min speed steps to par.
		/// </summary>
		/// <param name="steps">The steps.</param>
		/// <returns>Steps expressed as par.</returns>
        public ushort MinSpdStepsToPar(uint steps)
        {
            return (ushort)(steps * 4.194304 + 0.5);
        }

		/// <summary>
		/// Converts FS speed steps to par.
		/// </summary>
		/// <param name="steps">The steps.</param>
		/// <returns>Steps expressed as par.</returns>
        public ushort FSSpdStepsToPar(uint steps)
        {
            return (ushort)(steps * 0.065536);
        }

		/// <summary>
		/// Converts int speed steps to par.
		/// </summary>
		/// <param name="steps">The steps.</param>
		/// <returns>Steps expressed as par.</returns>
        public ushort IntSpdStepsToPar(uint steps)
        {
            return (ushort)(steps * 4.194304 + 0.5);
        }

		/// <summary>
		/// Converts Kval percent to par.
		/// </summary>
		/// <param name="perc">The percent.</param>
		/// <returns>Percent expressed as par.</returns>
        public byte KvalPercToPar(float perc)
        {
            return (byte)(perc / 0.390625 + 0.5);
        }

		/// <summary>
		/// Converts BEMF slope percent to par.
		/// </summary>
		/// <param name="perc">The percent.</param>
		/// <returns>Percent expressed as par.</returns>
        public byte BemfSlopePercToPar(float perc)
        {
            return (byte)(perc / 0.00156862745098 + 0.5);
        }

		/// <summary>
		/// Converts KTherm to par.
		/// </summary>
		/// <param name="kTherm">The percent.</param>
		/// <returns>KTherms expressed as par.</returns>
        public byte KThermToPar(uint kTherm)
        {
            return (byte)((kTherm - 1) / 0.03125 + 0.5);
        }

		/// <summary>
		/// Converts StallTH to par.
		/// </summary>
		/// <param name="stallTh">The StallTh.</param>
		/// <returns>StallTh expressed as par.</returns>
        public byte StallThToPar(uint stallTh)
        {
            return (byte)((stallTh - 31.25) / 31.25 + 0.5);
        }

        /// <summary>
        /// Avalilable Registers
        /// </summary>
        public class Registers
        {
            /// <summary>
            /// Constructs a new instance.
            /// </summary>
            public Registers()
            {
                this.AbsPos = 0;
                this.ElPos = 0;
                this.Mark = 0;
                this.Speed = 0;
                this.Acc = 0x08A;
                this.Dec = 0x08A;
                this.MaxSpeed = 0x041;
                this.MinSpeed = 0;
                this.FsSpd = 0x027;
                this.KValHold = 0x29;
                this.KValRun = 0x29;
                this.KValAcc = 0x29;
                this.KvalDec = 0x29;
                this.IntSpd = 0x0408;
                this.StSlp = 0x19;
                this.FnSlpAcc = 0x29;
                this.FnSlpDec = 0x29;
                this.KTherm = 0;
                this.OcdTh = 0x8;
                this.StallTh = 0x40;
                this.StepMode = 0x7;
                this.AlarmEn = 0xFF;
                this.Config = 0x2E88;
            }

            /// <summary>
            /// The ABS_POS register.
            /// </summary>
            public uint AbsPos { get; set; }

            /// <summary>
            /// The EL_POS register.
            /// </summary>
            public ushort ElPos { get; set; }

            /// <summary>
            /// The MARK register.
            /// </summary>
            public uint Mark { get; set; }

            /// <summary>
            /// The SPEED register.
            /// </summary>
            public uint Speed { get; set; }

            /// <summary>
            /// The ACC register.
            /// </summary>
            public ushort Acc { get; set; }

            /// <summary>
            /// The DEC register.
            /// </summary>
            public ushort Dec { get; set; }

            /// <summary>
            /// The MAX_SPEED register.
            /// </summary>
            public ushort MaxSpeed { get; set; }

            /// <summary>
            /// The MIN_SPEED register.
            /// </summary>
            public ushort MinSpeed { get; set; }

            /// <summary>
            /// The FS_SPD register.
            /// </summary>
            public ushort FsSpd { get; set; }

            /// <summary>
            /// The KVAL_HOLD register.
            /// </summary>
            public byte KValHold { get; set; }

            /// <summary>
            /// The KVAL_RUN register.
            /// </summary>
            public byte KValRun { get; set; }

            /// <summary>
            /// The KVAL_ACC register.
            /// </summary>
            public byte KValAcc { get; set; }

            /// <summary>
            /// The KVAL_DEC register.
            /// </summary>
            public byte KvalDec { get; set; }

            /// <summary>
            /// The INT_SPD register.
            /// </summary>
            public ushort IntSpd { get; set; }

            /// <summary>
            /// The ST_SLP register.
            /// </summary>
            public byte StSlp { get; set; }

            /// <summary>
            /// The FN_SLP_ACC register.
            /// </summary>
            public byte FnSlpAcc { get; set; }

            /// <summary>
            /// The FN_SLP_DEC register.
            /// </summary>
            public byte FnSlpDec { get; set; }

            /// <summary>
            /// The K_THERM register.
            /// </summary>
            public byte KTherm { get; set; }

            /// <summary>
            /// The ADC_OUT register.
            /// </summary>
            public byte AdcOut { get; set; }

            /// <summary>
            /// The OCD_TH register.
            /// </summary>
            public byte OcdTh { get; set; }

            /// <summary>
            /// The STALL_TH register.
            /// </summary>
            public byte StallTh { get; set; }

            /// <summary>
            /// The STEP_MODE register.
            /// </summary>
            public byte StepMode { get; set; }

            /// <summary>
            /// The ALARM_EN register.
            /// </summary>
            public byte AlarmEn { get; set; }

            /// <summary>
            /// The CONFIG register.
            /// </summary>
            public ushort Config { get; set; }
        }

        /// <summary>
        /// Action Options
        /// </summary>
        public enum Action : byte
        {
            /// <summary>
            /// The reset option.
            /// </summary>
            Reset = 0x00,

            /// <summary>
            /// The copy option.
            /// </summary>
            Copy = 0x01
        };

        /// <summary>
        /// Direction Options
        /// </summary>
        public enum Direction : byte
        {
            /// <summary>
            /// The forward option.
            /// </summary>
            Forward = 0x01,

            /// <summary>
            /// The reverse option.
            /// </summary>
            Reverse = 0x00
        };

        /// <summary>
        /// Status Masks
        /// </summary>
        public enum StatusMask : ushort
        {
            /// <summary>
            /// The HIZ status mask.
            /// </summary>
            Hiz = 0x0001,
            
            /// <summary>
            /// The BUSY status mask.
            /// </summary>
            Busy = 0x0002,
            
            /// <summary>
            /// The SW_F status mask.
            /// </summary>
            SwF = 0x0004,
            
            /// <summary>
            /// The SW_EVN status mask.
            /// </summary>
            SwEvn = 0x0008,
            
            /// <summary>
            /// The DIR status mask.
            /// </summary>
            Dir = 0x0010,
            
            /// <summary>
            /// The MOT_STATUS status mask.
            /// </summary>
            MotStatus = 0x0060,
            
            /// <summary>
            /// The NOTPERF_CMD status mask.
            /// </summary>
            NotPerfCmd = 0x0080,
            
            /// <summary>
            /// The WRONG_CMD status mask.
            /// </summary>
            WrongCmd = 0x0100,
            
            /// <summary>
            /// The OVLO status mask.
            /// </summary>
            Uvlo = 0x0200,
            
            /// <summary>
            /// The TH_WRN status mask.
            /// </summary>
            ThWrn = 0x0400,
            
            /// <summary>
            /// The TH_SD status mask.
            /// </summary>
            ThSd = 0x0800,
            
            /// <summary>
            /// The OCD status mask.
            /// </summary>
            Ocd = 0x1000,
            
            /// <summary>
            /// The STEP_LOSS_a status mask.
            /// </summary>
            StepLossA = 0x2000,
            
            /// <summary>
            /// The STATUS_STEP_LOSS_B status mask.
            /// </summary>
            StepLossB = 0x4000,
            
            /// <summary>
            /// The STATUS_SCK_MOD status mask.
            /// </summary>
            SckMod = 0x8000
        }

        /// <summary>
        /// Registers
        /// </summary>
        public enum RegisterAddress : byte
        {
            /// <summary>
            /// The ABS_POS register.
            /// </summary>
            AbsPos = 0x01,
            
            /// <summary>
            /// The EL_POS register.
            /// </summary>
            ElPos = 0x02,
            
            /// <summary>
            /// The MARK register.
            /// </summary>
            Mark = 0x03,
            
            /// <summary>
            /// The SPEED register.
            /// </summary>
            Speed = 0x04,
            
            /// <summary>
            /// The ACC register.
            /// </summary>
            Acc = 0x05,
            
            /// <summary>
            /// The DEC register.
            /// </summary>
            Dec = 0x06,
            
            /// <summary>
            /// The MAX_SPEED register.
            /// </summary>
            MaxSpeed = 0x07,
            
            /// <summary>
            /// The MIN_SPEED register.
            /// </summary>
            MinSpeed = 0x08,
            
            /// <summary>
            /// The FS_SPD register.
            /// </summary>
            FsSpd = 0x15,
            
            /// <summary>
            /// The KVAL_HOLD register.
            /// </summary>
            KValHold = 0x09,
            
            /// <summary>
            /// The KVAL_RUN register.
            /// </summary>
            KValRun = 0x0A,
            
            /// <summary>
            /// The KVAL_ACC register.
            /// </summary>
            KValAcc = 0x0B,
            
            /// <summary>
            /// The KVAL_DEC register.
            /// </summary>
            KValDev = 0x0C,
            
            /// <summary>
            /// The INT_SPD register.
            /// </summary>
            IntSpd = 0x0D,
            
            /// <summary>
            /// The ST_SLP register.
            /// </summary>
            StSlp = 0x0E,
            
            /// <summary>
            /// The FN_SLP_ACC register.
            /// </summary>
            FnSlpAcc = 0x0F,
            
            /// <summary>
            /// The FN_SLP_DEC register.
            /// </summary>
            FnSlpDec = 0x10,
            
            /// <summary>
            /// The K_THERM register.
            /// </summary>
            KTherm = 0x11,
            
            /// <summary>
            /// The ADC_OUT register.
            /// </summary>
            AdcOut = 0x12,
            
            /// <summary>
            /// The OCD_TH register.
            /// </summary>
            OcdTh = 0x13,
            
            /// <summary>
            /// The STALL_TH register.
            /// </summary>
            StallTh = 0x14,
            
            /// <summary>
            /// The STEP_MODE register.
            /// </summary>
            StepMode = 0x16,
            
            /// <summary>
            /// The ALARM_EN register.
            /// </summary>
            AlarmEn = 0x17,
            
            /// <summary>
            /// The CONFIG register.
            /// </summary>
            Config = 0x18,
            
            /// <summary>
            /// The STATUS register.
            /// </summary>
            Status = 0x19,
            
            /// <summary>
            /// The RESERVED_REG1 register.
            /// </summary>
            ReservedReg1 = 0x1A,
            
            /// <summary>
            /// The RESERVED_REG2 register.
            /// </summary>
            ReservedReg2 = 0x1B
        }

        /// <summary>
        /// Commands
        /// </summary>
        public enum Command : byte
        {
            /// <summary>
            /// The NOP command.
            /// </summary>
            Nop = 0x00,

            /// <summary>
            /// The SET_PARAM command.
            /// </summary>
            SetParam = 0x00,
            
            /// <summary>
            /// The GET_PARAM command.
            /// </summary>
            GetParam = 0x20,
            
            /// <summary>
            /// The RUN command.
            /// </summary>
            Run = 0x50,
            
            /// <summary>
            /// The STEP_CLOCK command.
            /// </summary>
            StepClock = 0x58,
            
            /// <summary>
            /// The MOVE command.
            /// </summary>
            Move = 0x40,
            
            /// <summary>
            /// The GO_TO command.
            /// </summary>
            GoTo = 0x60,
            
            /// <summary>
            /// The GO_TO_DIR command.
            /// </summary>
            GoToDir = 0x68,
            
            /// <summary>
            /// The GO_UNTIL command.
            /// </summary>
            GoUntil = 0x82,
            
            /// <summary>
            /// The RELEASE_SW command.
            /// </summary>
            ReleaseSw = 0x92,
            
            /// <summary>
            /// The GO_HOME command.
            /// </summary>
            GoHome = 0x70,
            
            /// <summary>
            /// The GO_MARK command.
            /// </summary>
            GoMark = 0x78,
            
            /// <summary>
            /// The RESET_POS command.
            /// </summary>
            ResetPos = 0xD8,
            
            /// <summary>
            /// The RESET_DEVICE command.
            /// </summary>
            ResetDevice = 0xC0,
            
            /// <summary>
            /// The SOFT_STOP command.
            /// </summary>
            SoftStop = 0xB0,
            
            /// <summary>
            /// The HARD_STOP command.
            /// </summary>
            HardStop = 0xB8,
            
            /// <summary>
            /// The SOFT_HIZ command.
            /// </summary>
            SoftHiz = 0xA0,
            
            /// <summary>
            /// The HARD_HIZ command.
            /// </summary>
            HardHiz = 0xA8,
            
            /// <summary>
            /// The GET_STATUS command.
            /// </summary>
            GetStatus = 0xD0,
            
            /// <summary>
            /// The RESERVED_CMD1 command.
            /// </summary>
            ReservedCmd1 = 0xEB,
            
            /// <summary>
            /// The RESERVED_CMD2 command.
            /// </summary>
            ReservedCmd2 = 0xF8
        }

        /// <summary>
        /// Overcurrent Detection Threshold Options
        /// </summary>
        public enum OvercurrentDetectionThreshold : byte
        {
            /// <summary>
            /// The 375mA option.
            /// </summary>
            Thershold375mA = 0x00,

            /// <summary>
            /// The 750mA option.
            /// </summary>
            Thershold750mA = 0x01,

            /// <summary>
            /// The 1125mA option.
            /// </summary>
            Thershold1125mA = 0x02,

            /// <summary>
            /// The 1500mA option.
            /// </summary>
            Thershold1500mA = 0x03,

            /// <summary>
            /// The 1875mA option.
            /// </summary>
            Thershold1875mA = 0x04,

            /// <summary>
            /// The 2250mA option.
            /// </summary>
            Thershold2250mA = 0x05,

            /// <summary>
            /// The 2625mA option.
            /// </summary>
            Thershold2625mA = 0x06,

            /// <summary>
            /// The 3000mA option.
            /// </summary>
            Thershold3000mA = 0x07,

            /// <summary>
            /// The 3375mA option.
            /// </summary>
            Thershold3375mA = 0x08,

            /// <summary>
            /// The 3750mA option.
            /// </summary>
            Thershold3750mA = 0x09,

            /// <summary>
            /// The 4125mA option.
            /// </summary>
            Thershold4125mA = 0x0A,

            /// <summary>
            /// The 4500mA option.
            /// </summary>
            Thershold4500mA = 0x0B,

            /// <summary>
            /// The 4875mA option.
            /// </summary>
            Thershold4875mA = 0x0C,

            /// <summary>
            /// The 5250mA option.
            /// </summary>
            Thershold5250mA = 0x0D,

            /// <summary>
            /// The 5625mA option.
            /// </summary>
            Thershold5625mA = 0x0E,

            /// <summary>
            /// The 6000mA option.
            /// </summary>
            Thershold6000mA = 0x0F
        }

        /// <summary>
        /// Step Select Options
        /// </summary>
        public enum StepSelect : byte
        {
            /// <summary>
            /// The step 1 option.
            /// </summary>
            Select_1 = 0x00,

            /// <summary>
            /// The step 2 option.
            /// </summary>
            Select_1_2 = 0x01,

            /// <summary>
            /// The step 4 option.
            /// </summary>
            Select_1_4 = 0x02,

            /// <summary>
            /// The step 8 option.
            /// </summary>
            Select_1_8 = 0x03,

            /// <summary>
            /// The step 16 option.
            /// </summary>
            Select_1_16 = 0x04,

            /// <summary>
            /// The step 32 option.
            /// </summary>
            Select_1_32 = 0x05,

            /// <summary>
            /// The step 64 option.
            /// </summary>
            Select_1_64 = 0x06,

            /// <summary>
            /// The step 128 option.
            /// </summary>
            Select_1_128 = 0x07
        }

        /// <summary>
        /// Alarm Enable Options
        /// </summary>
        public enum AlarmEnable : byte
        {
            /// <summary>
            /// The overcurrent option.
            /// </summary>
            Overcurrent = 0x01,

            /// <summary>
            /// The thermal shutdown option.
            /// </summary>
            ThermalShutdown = 0x02,

            /// <summary>
            /// The thermal warning option.
            /// </summary>
            ThermalWarning = 0x04,

            /// <summary>
            /// The under voltage option.
            /// </summary>
            UnderVoltage = 0x08,

            /// <summary>
            /// The stall detection A option.
            /// </summary>
            StallDetectA = 0x10,

            /// <summary>
            /// The stall detection B option.
            /// </summary>
            StallDetectB = 0x20,

            /// <summary>
            /// The turn on sw option.
            /// </summary>
            SwTurnOn = 0x40,

            /// <summary>
            /// The wrong nperf option.
            /// </summary>
            WrongNperf = 0x80
        }

        /// <summary>
        /// Configuration Register Options
        /// </summary>
        public enum ConfigOscManagement : ushort
        {
            /// <summary>
            /// The 16MHz option.
            /// </summary>
            Int16MHz = 0x0000,

            /// <summary>
            /// The 16MHz OSCOUNT 2MHz option.
            /// </summary>
            Int16MHzOscout2MHz = 0x0008,

            /// <summary>
            /// The 16MHz OSCOUNT 4MHz option.
            /// </summary>
            Int16MHzOscout4MHz = 0x0009,

            /// <summary>
            /// The 16MHz OSCOUNT 8MHz option.
            /// </summary>
            Int16MHzOscout8MHz = 0x000A,

            /// <summary>
            /// The 16MHz OSCOUNT 16MHz option.
            /// </summary>
            Int16MHzOscout16MHz = 0x000B,

            /// <summary>
            /// The 8MHz XTAL drive option.
            /// </summary>
            Ext8MHzXtalDrive = 0x0004,

            /// <summary>
            /// The 16MHz XTAL drive option.
            /// </summary>
            Ext16MHzXtalDrive = 0x0005,

            /// <summary>
            /// The 24MHz XTAL drive option.
            /// </summary>
            Ext24MHzXtalDrive = 0x0006,

            /// <summary>
            /// The 32MHz XTAL drive option.
            /// </summary>
            Ext32MHzXtalDrive = 0x0007,

            /// <summary>
            /// The 8MHz OSCOUNT invert option.
            /// </summary>
            Ext8MHzOscoutInvert = 0x000C,

            /// <summary>
            /// The 16MHz OSCOUNT invert option.
            /// </summary>
            Ext16MHzOscoutInvert = 0x000D,

            /// <summary>
            /// The 24MHz OSCOUNT invert option.
            /// </summary>
            Ext24MHzOscoutInvert = 0x000E,

            /// <summary>
            /// The 32MHz OSCOUNT invert option.
            /// </summary>
            Ext32MHzOscoutInvert = 0x000F
        }

        /// <summary>
        /// The SW Mode configuration.
        /// </summary>
        public enum ConfigSwMode : ushort
        {
            /// <summary>
            /// The hard stop option.
            /// </summary>
            HardStop = 0x0000,

            /// <summary>
            /// The user option.
            /// </summary>
            User = 0x0010
        }

        /// <summary>
        /// The PWM configuration options.
        /// </summary>
        public enum ConfigFPwmDec : ushort
        {
            /// <summary>
            /// The 0-625 option.
            /// </summary>
            Mul_0_625 = 0x00 << 10,

            /// <summary>
            /// The 0-75 option.
            /// </summary>
            Mul_0_75 = 0x01 << 10,

            /// <summary>
            /// The 0-875 option.
            /// </summary>
            Mul_0_875 = 0x02 << 10,

            /// <summary>
            /// The 1 option.
            /// </summary>
            Mul_1 = 0x03 << 10,

            /// <summary>
            /// The 1-25 option.
            /// </summary>
            Mul_1_25 = 0x04 << 10,

            /// <summary>
            /// The 1-5 option.
            /// </summary>
            Mul_1_5 = 0x05 << 10,

            /// <summary>
            /// The 1-75 option.
            /// </summary>
            Mul_1_75 = 0x06 << 10,

            /// <summary>
            /// The 2 option.
            /// </summary>
            Mul_2 = 0x07 << 10
        }

        /// <summary>
        /// The PWM INT configuration options.
        /// </summary>
        public enum ConfigFPwmInt : ushort
        {
            /// <summary>
            /// The DIV 1 option.
            /// </summary>
            Div1 = 0x00 << 13,

            /// <summary>
            /// The DIV 2 option.
            /// </summary>
            Div2 = 0x01 << 13,

            /// <summary>
            /// The DIV 3 option.
            /// </summary>
            Div3 = 0x02 << 13,

            /// <summary>
            /// The DIV 4 option.
            /// </summary>
            Div4 = 0x03 << 13,

            /// <summary>
            /// The DIV 5 option.
            /// </summary>
            Div5 = 0x04 << 13,

            /// <summary>
            /// The DIV 6 option.
            /// </summary>
            Div6 = 0x05 << 13,

            /// <summary>
            /// The DIV 7 option.
            /// </summary>
            Div7 = 0x06 << 13
        }

        /// <summary>
        /// The POW SR configuration options.
        /// </summary>
        public enum ConfigPowSr : ushort
        {
            /// <summary>
            /// The 180V option.
            /// </summary>
            V180 = 0x0000,

            /// <summary>
            /// The 290V option.
            /// </summary>
            V290 = 0x0200,

            /// <summary>
            /// The 530V option.
            /// </summary>
            V530 = 0x0300
        }

        /// <summary>
        /// The EN VSCOMP configuration options.
        /// </summary>
        public enum ConfigEnVscomp : ushort
        {
            /// <summary>
            /// The disable option.
            /// </summary>
            Disable = 0x0000,

            /// <summary>
            /// The enable option.
            /// </summary>
            Enable = 0x0020
        }

        /// <summary>
        /// The OC configuration options.
        /// </summary>
        public enum ConfigOcSd : ushort
        {
            /// <summary>
            /// The disable option.
            /// </summary>
            Disable = 0x0000,

            /// <summary>
            /// The enable option.
            /// </summary>
            Enable = 0x0080
        };
    } 
}
