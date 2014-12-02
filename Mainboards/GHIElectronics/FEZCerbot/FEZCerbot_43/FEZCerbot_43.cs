using GHI.IO;
using GHI.IO.Storage;
using GHI.Pins;
using GHI.Processor;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace GHIElectronics.Gadgeteer
{
    /// <summary>
    /// The mainboard class for the FEZ Cerbot.
    /// </summary>
	[Obsolete]
    public class FEZCerbot : GT.Mainboard
    {
        private const double MOTOR_BASE_FREQUENCY = 100000;

        private GTI.PwmOutput buzzer;
        private GTI.AnalogInput leftSensor;
        private GTI.AnalogInput rightSensor;
        private GTI.DigitalOutput leftIRLED;
        private GTI.DigitalOutput rightIRLED;
        private GTI.PwmOutput enableFaderPin;
        private GTI.Spi forwardLEDs;
        private GTI.PwmOutput leftMotor;
        private GTI.PwmOutput rightMotor;
        private GTI.DigitalOutput leftMotorDirection;
        private GTI.DigitalOutput rightMotorDirection;
        private GTI.PwmOutput servo;

        private ushort ledMask;
        private bool leftMotorInverted;
        private bool rightMotorInverted;
        private uint servoPulseFactor;
        private uint servoMinPulse;
        private uint servoMaxPulse;
        private bool servoConfigured;

        private bool configSet;
        private OutputPort debugLed;
        private IRemovable[] storageDevices;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public FEZCerbot()
        {
            this.configSet = false;
            this.debugLed = null;
            this.storageDevices = new IRemovable[1];

            this.NativeBitmapConverter = this.NativeBitmapConvert;
            this.NativeBitmapCopyToSpi = this.NativeBitmapSpi;

            GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
            GT.Socket socket;


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
            socket.SupportedTypes = new char[] { 'F', 'Y' };
            socket.CpuPins[3] = Generic.GetPin('B', 15);
            socket.CpuPins[4] = Generic.GetPin('C', 8);
            socket.CpuPins[5] = Generic.GetPin('C', 9);
            socket.CpuPins[6] = Generic.GetPin('D', 2);
            socket.CpuPins[7] = Generic.GetPin('C', 10);
            socket.CpuPins[8] = Generic.GetPin('C', 11);
            socket.CpuPins[9] = Generic.GetPin('C', 12);
            socket.I2CBusIndirector = nativeI2C;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
            socket.SupportedTypes = new char[] { 'I', 'X' };
            socket.CpuPins[3] = Generic.GetPin('C', 5);
            socket.CpuPins[4] = Generic.GetPin('A', 10);
            socket.CpuPins[5] = Generic.GetPin('B', 12);
            socket.CpuPins[6] = Generic.GetPin('C', 7);
            socket.CpuPins[8] = Generic.GetPin('B', 7);
            socket.CpuPins[9] = Generic.GetPin('B', 6);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
            socket.SupportedTypes = new char[] { 'S', 'U', 'X' };
            socket.CpuPins[3] = Generic.GetPin('B', 8);
            socket.CpuPins[4] = Generic.GetPin('B', 10);
            socket.CpuPins[5] = Generic.GetPin('B', 11);
            socket.CpuPins[6] = Generic.GetPin('A', 0);
            socket.CpuPins[7] = Generic.GetPin('B', 5);
            socket.CpuPins[8] = Generic.GetPin('B', 4);
            socket.CpuPins[9] = Generic.GetPin('B', 3);
            socket.SerialPortName = "COM3";
            socket.SPIModule = SPI.SPI_module.SPI1;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
            socket.SupportedTypes = new char[] { 'A', 'O', 'I', 'X' };
            socket.CpuPins[3] = Generic.GetPin('C', 0);
            socket.CpuPins[4] = Generic.GetPin('C', 1);
            socket.CpuPins[5] = Generic.GetPin('A', 4);
            socket.CpuPins[6] = Generic.GetPin('A', 1);
            socket.CpuPins[8] = Generic.GetPin('B', 7);
            socket.CpuPins[9] = Generic.GetPin('B', 6);
            socket.AnalogOutput5 = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_1;
            socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_3;
            socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_4;
            socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_5;
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
            GT.Socket.SocketInterfaces.SetAnalogOutputFactors(socket, 3.3, 0, 12);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
            socket.SupportedTypes = new char[] { 'I', 'U' };
            socket.CpuPins[3] = Generic.GetPin('C', 14);
            socket.CpuPins[4] = Generic.GetPin('A', 2);
            socket.CpuPins[5] = Generic.GetPin('A', 3);
            socket.CpuPins[6] = Generic.GetPin('C', 15);
            socket.CpuPins[8] = Generic.GetPin('B', 7);
            socket.CpuPins[9] = Generic.GetPin('B', 6);
            socket.SerialPortName = "COM2";
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
            socket.SupportedTypes = new char[] { 'P' };
            socket.CpuPins[3] = Generic.GetPin('C', 13);
            socket.CpuPins[6] = Generic.GetPin('C', 3);
            socket.CpuPins[7] = Generic.GetPin('A', 9);
            socket.CpuPins[8] = Generic.GetPin('B', 9);
            socket.CpuPins[9] = Generic.GetPin('A', 8);
            socket.PWM7 = (Cpu.PWMChannel)12;
            socket.PWM8 = (Cpu.PWMChannel)15;
            socket.PWM9 = Cpu.PWMChannel.PWM_3;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
            socket.SupportedTypes = new char[] { 'S', 'Y', 'X' };
            socket.CpuPins[3] = Generic.GetPin('B', 13);
            socket.CpuPins[4] = Generic.GetPin('B', 14);
            socket.CpuPins[5] = (Cpu.Pin)(-1);
            socket.CpuPins[6] = Generic.GetPin('B', 2);
            socket.CpuPins[7] = Generic.GetPin('B', 5);
            socket.CpuPins[8] = Generic.GetPin('B', 4);
            socket.CpuPins[9] = Generic.GetPin('B', 3);
            socket.SPIModule = SPI.SPI_module.SPI1;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
            socket.SupportedTypes = new char[] { 'X', 'P' };
            socket.CpuPins[3] = Generic.GetPin('A', 6);
            socket.CpuPins[4] = Generic.GetPin('C', 4);
            socket.CpuPins[5] = (Cpu.Pin)(-1);
            socket.CpuPins[6] = (Cpu.Pin)(-1);
            socket.CpuPins[7] = Generic.GetPin('B', 0);
            socket.CpuPins[8] = Generic.GetPin('B', 1);
            socket.CpuPins[9] = Generic.GetPin('A', 7);
            socket.PWM7 = Cpu.PWMChannel.PWM_4;
            socket.PWM8 = Cpu.PWMChannel.PWM_5;
            socket.PWM9 = Cpu.PWMChannel.PWM_1;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
            socket.SupportedTypes = new char[] { 'A', 'P' };
            socket.CpuPins[3] = Generic.GetPin('A', 5);
            socket.CpuPins[4] = Generic.GetPin('C', 2);
            socket.CpuPins[5] = (Cpu.Pin)(-1);
            socket.CpuPins[6] = (Cpu.Pin)(-1);
            socket.CpuPins[7] = Generic.GetPin('B', 0);
            socket.CpuPins[8] = Generic.GetPin('B', 1);
            socket.CpuPins[9] = (Cpu.Pin)(-1);
            socket.PWM7 = Cpu.PWMChannel.PWM_0;
            socket.PWM8 = (Cpu.PWMChannel)13;
            socket.PWM9 = (Cpu.PWMChannel)(-1);
            socket.AnalogInput3 = (Cpu.AnalogChannel)8;
            socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_6;
            socket.AnalogInput5 = (Cpu.AnalogChannel)(-1);
            GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            this.Initialize();
        }

        /// <summary>
        /// The name of the mainboard.
        /// </summary>
        public override string MainboardName
        {
            get { return "GHI Electronics FEZ Cerbot"; }
        }

        /// <summary>
        /// The current version of the mainboard hardware.
        /// </summary>
        public override string MainboardVersion
        {
            get { return "1.3"; }
        }

        /// <summary>
        /// The storage device volume names supported by this mainboard.
        /// </summary>
        /// <returns>The volume names.</returns>
        public override string[] GetStorageDeviceVolumeNames()
        {
            return new string[] { "SD" };
        }

        /// <summary>
        /// Mounts the device with the given name.
        /// </summary>
        /// <param name="volumeName">The device to mount.</param>
        /// <returns>Whether or not the mount was successful.</returns>
        public override bool MountStorageDevice(string volumeName)
        {
            switch (volumeName)
            {
                case "SD":
                    this.storageDevices[0] = new SDCard();
                    this.storageDevices[0].Mount();

                    break;

                default:
                    throw new ArgumentException("volumeName must be present in the array returned by GetStorageDeviceVolumeNames.", "volumeName");
            }

            return true;
        }

        /// <summary>
        /// Unmounts the device with the given name.
        /// </summary>
        /// <param name="volumeName">The device to unmount.</param>
        /// <returns>Whether or not the unmount was successful.</returns>
        public override bool UnmountStorageDevice(string volumeName)
        {
            switch (volumeName)
            {
                case "SD":
                    if (this.storageDevices[0] == null) throw new InvalidOperationException("This volume is not mounted.");

                    this.storageDevices[0].Unmount();
                    this.storageDevices[0].Dispose();
                    this.storageDevices[0] = null;

                    break;

                default:
                    throw new ArgumentException("volumeName must be present in the array returned by GetStorageDeviceVolumeNames.", "volumeName");
            }

            return true;
        }

        /// <summary>
        /// Configure the onboard display controller to fulfil the requirements of a display using the RGB sockets.
        /// If doing this requires rebooting, then the method must reboot and not return.
        /// If there is no onboard display controller, then NotSupportedException must be thrown.
        /// </summary>
        /// <param name="displayModel">Display model name.</param>
        /// <param name="width">Display physical width in pixels, ignoring the orientation setting.</param>
        /// <param name="height">Display physical height in lines, ignoring the orientation setting.</param>
        /// <param name="orientationDeg">Display orientation in degrees.</param>
        /// <param name="timing">The required timings from an LCD controller.</param>
        protected override void OnOnboardControllerDisplayConnected(string displayModel, int width, int height, int orientationDeg, GTM.Module.DisplayModule.TimingRequirements timing)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Ensures that the RGB socket pins are available by disabling the display controller if needed.
        /// </summary>
        public override void EnsureRgbSocketPinsAvailable()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the state of the debug LED.
        /// </summary>
        /// <param name="on">The new state.</param>
        public override void SetDebugLED(bool on)
        {
            if (this.debugLed == null)
                this.debugLed = new OutputPort(Generic.GetPin('A', 14), on);

            this.debugLed.Write(on);
        }

        /// <summary>
        /// Sets the programming mode of the device.
        /// </summary>
        /// <param name="programmingInterface">The new programming mode.</param>
        public override void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This performs post-initialization tasks for the mainboard.  It is called by Gadgeteer.Program.Run and does not need to be called manually.
        /// </summary>
        public override void PostInit()
        {

        }

        private void NativeBitmapConvert(Bitmap bitmap, byte[] pixelBytes, GT.Mainboard.BPP bpp)
        {
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE) throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

            GHI.Utilities.Bitmaps.Convert(bitmap, GHI.Utilities.Bitmaps.BitsPerPixel.BPP16_BGR_BE, pixelBytes);
        }

        private void NativeBitmapSpi(Bitmap bitmap, SPI.Configuration config, int xSrc, int ySrc, int width, int height, GT.Mainboard.BPP bpp)
		{
            if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE) throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

			if (!this.configSet)
            {
                Display.Populate(Display.GHIDisplay.DisplayN18);
                Display.SpiConfiguration = config;
                Display.Bpp = GHI.Utilities.Bitmaps.BitsPerPixel.BPP16_BGR_BE;
                Display.Save();

				this.configSet = true;
			}

			bitmap.Flush(xSrc, ySrc, width, height);
		}

        private class InteropI2CBus : GT.SocketInterfaces.I2CBus
        {
            public override ushort Address { get; set; }
            public override int Timeout { get; set; }
            public override int ClockRateKHz { get; set; }

            private SoftwareI2CBus softwareBus;

            public InteropI2CBus(GT.Socket socket, GT.Socket.Pin sdaPin, GT.Socket.Pin sclPin, ushort address, int clockRateKHz, GTM.Module module)
            {
                this.Address = address;
                this.ClockRateKHz = clockRateKHz;

                this.softwareBus = new SoftwareI2CBus(socket.CpuPins[(int)sclPin], socket.CpuPins[(int)sdaPin]);
            }

            public override void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int numWritten, out int numRead)
            {
                this.softwareBus.WriteRead((byte)this.Address, writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out numWritten, out numRead);
            }
        }

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

		private void Initialize()
        {
            var spiSocket = GT.Socket.GetSocket(10, true, null, null);
            var pwmSocket = GT.Socket.GetSocket(11, true, null, null);
            var analogSocket = GT.Socket.GetSocket(12, true, null, null);

            this.forwardLEDs = GTI.SpiFactory.Create(spiSocket, new GTI.SpiConfiguration(false, 0, 0, false, true, 2000), GTI.SpiSharing.Shared, spiSocket, GT.Socket.Pin.Six, null);
            this.leftIRLED = GTI.DigitalOutputFactory.Create(spiSocket, GT.Socket.Pin.Three, true, null);
            this.rightIRLED = GTI.DigitalOutputFactory.Create(spiSocket, GT.Socket.Pin.Four, true, null);

            this.leftMotorDirection = GTI.DigitalOutputFactory.Create(pwmSocket, GT.Socket.Pin.Three, false, null);
            this.rightMotorDirection = GTI.DigitalOutputFactory.Create(pwmSocket, GT.Socket.Pin.Four, false, null);
            this.leftMotor = GTI.PwmOutputFactory.Create(pwmSocket, GT.Socket.Pin.Seven, false, null);
            this.rightMotor = GTI.PwmOutputFactory.Create(pwmSocket, GT.Socket.Pin.Eight, false, null);
            this.servo = GTI.PwmOutputFactory.Create(pwmSocket, GT.Socket.Pin.Nine, false, null);

            this.buzzer = GTI.PwmOutputFactory.Create(analogSocket, GT.Socket.Pin.Seven, false, null);
            this.enableFaderPin = GTI.PwmOutputFactory.Create(analogSocket, GT.Socket.Pin.Eight, true, null);
            this.leftSensor = GTI.AnalogInputFactory.Create(analogSocket, GT.Socket.Pin.Three, null);
            this.rightSensor = GTI.AnalogInputFactory.Create(analogSocket, GT.Socket.Pin.Four, null);
            
            this.leftMotor.Set(FEZCerbot.MOTOR_BASE_FREQUENCY, 0);
            this.rightMotor.Set(FEZCerbot.MOTOR_BASE_FREQUENCY, 0);

            this.enableFaderPin.Set(2000, 1.0);
            
            this.leftMotorInverted = false;
            this.rightMotorInverted = false;
            this.servoConfigured = false;

			this.SetLedBitmask(0x00);
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
			this.buzzer.IsActive = false;

			if (frequency <= 0)
				return;

            this.buzzer.Set(frequency, dutyCycle);

			if (duration != 0)
			{
				Thread.Sleep((int)duration);
                this.buzzer.IsActive = false;
			}
		}

		/// <summary>
		/// Stops the buzzer from buzzing.
		/// </summary>
		public void StopBuzzer()
		{
            this.buzzer.IsActive = false;
		}

		/// <summary>
		/// Gets the reading from a reflective sensor between 0 and 100. The higher the number, 
		/// the more reflection that was detected. Nearby objects reflect more than far objects.
		/// </summary>
		/// <param name="sensor">The sensor to read from.</param>
		public double GetReflectiveReading(ReflectiveSensors sensor)
		{
            return 100 * (1 - (sensor == ReflectiveSensors.Left ? this.leftSensor.ReadProportion() : this.rightSensor.ReadProportion()));
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
		public void SetLedIntensity(uint intensity)
		{
			if (intensity > 100 || intensity < 1) throw new ArgumentOutOfRangeException("intensity", "intensity must be between 1 and 100");

			this.enableFaderPin.Set(2000, 2000 * (intensity / 100));
		}

		/// <summary>
		/// Sets the state of the front LEDs using a short where each bit represents one LED.
		/// </summary>
		/// <param name="mask">The mask used to set the LED state.</param>
		/// <remarks>Bit 0 is the leftmost LED, bit 15 is rightmost LED.</remarks>
		public void SetLedBitmask(ushort mask)
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
		public void TurnOnLed(int which)
		{
			if (which < 1 || which > 16)
				throw new ArgumentException("The LED must be between 1 and 16.");

            this.SetLedBitmask((ushort)(this.ledMask | (1 << --which)));
		}

		/// <summary>
		/// Turns off the specified front LED while leaving the others unchanged.
		/// </summary>
		/// <param name="which">The LED number to turn off. Between 1 and 16.</param>
		public void TurnOffLed(int which)
		{
			if (which < 1 || which > 16)
				throw new ArgumentException("The LED must be between 1 and 16.");

            this.SetLedBitmask((ushort)(this.ledMask & ~(1 << --which)));
		}

		/// <summary>
		/// Sets the pulse limits for the servo. You must call this before setting the servo position.
		/// </summary>
		/// <param name="minPulse">The minimum pulse width the servo can handle in microseconds.</param>
		/// <param name="maxPulse">The maximum pulse width the servo can handle in microseconds.</param>
		public void SetServoLimits(uint minPulse, uint maxPulse)
		{
            if (maxPulse < minPulse) throw new ArgumentOutOfRangeException("maxPulse", "maxPulse must be greater than minPulse.");

			this.servoMinPulse = minPulse;
			this.servoMaxPulse = maxPulse;
			this.servoPulseFactor = (maxPulse - minPulse) / 100;

			if (this.servoConfigured)
			{
				this.servo.IsActive = false;
			}

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
            if (position < 0 || position > 100) throw new ArgumentOutOfRangeException("position", "Position must be between 0 and 100.");
            if (!this.servoConfigured) throw new InvalidOperationException("You must call SetServoLimits before calling SetServoPosition.");

            this.servo.Set(50, (this.servoPulseFactor * position + this.servoMinPulse) / 50);
		}

		/// <summary>
		/// If you find that the motors go forward when passed a negative number due to reversed wiring, call this function. 
		/// It will invert the motor direction so that when you pass in a positive speed, it goes forward.
		/// </summary>
		public void SetMotorInversion(bool invertLeft, bool invertRight)
		{
			this.leftMotorInverted = invertLeft;
			this.rightMotorInverted = invertRight;
		}

		/// <summary>
		/// Sets the speed of the motor. -100 is full speed backwards, 100 is full speed forward, and 0 is stopped.
		/// </summary>
		/// <param name="leftSpeed">The new speed of the left motor.</param>
		/// <param name="rightSpeed">The new speed of the right motor.</param>
		public void SetMotorSpeed(int leftSpeed, int rightSpeed)
		{
			if (leftSpeed > 100 || leftSpeed < -100 || rightSpeed > 100 || rightSpeed < -100) new ArgumentOutOfRangeException("motor", "The motor speed must be between -100 and 100");

			if (this.leftMotorInverted)
				leftSpeed *= -1;

			if (this.rightMotorInverted)
				rightSpeed *= -1;

			this.SetSpeed(this.leftMotor, this.leftMotorDirection, leftSpeed, true);
			this.SetSpeed(this.rightMotor, this.rightMotorDirection, rightSpeed, false);
		}

		private void SetSpeed(GTI.PwmOutput motor, GTI.DigitalOutput direction, int speed, bool isLeft)
		{
			if (speed == 0)
			{
				direction.Write(false);
				motor.Set(FEZCerbot.MOTOR_BASE_FREQUENCY, 0.01);
			}
			else if (speed < 0)
			{
				direction.Write(isLeft ? true : false);
                motor.Set(FEZCerbot.MOTOR_BASE_FREQUENCY, speed / -100.0);
			}
			else
			{
				direction.Write(isLeft ? false : true);
                motor.Set(FEZCerbot.MOTOR_BASE_FREQUENCY, speed / 100.0);
			}
		}
    }
}