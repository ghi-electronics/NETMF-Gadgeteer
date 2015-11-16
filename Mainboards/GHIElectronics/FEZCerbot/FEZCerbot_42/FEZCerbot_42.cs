using GHI.OSHW.Hardware;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Threading;
using FEZCerberus = GHI.Hardware.FEZCerb;
using GT = Gadgeteer;
using GTI = Gadgeteer.Interfaces;

namespace GHIElectronics.Gadgeteer
{
	/// <summary>
	/// Support class for GHI Electronics FEZCerbot for Microsoft .NET Gadgeteer
	/// </summary>
	public class FEZCerbot : GT.Mainboard
    {
        private const int MOTOR_BASE_FREQUENCY = 100000;

        private GTI.PWMOutput buzzer;
        private GTI.AnalogInput leftSensor;
        private GTI.AnalogInput rightSensor;
        private GTI.DigitalOutput leftIRLED;
        private GTI.DigitalOutput rightIRLED;
        private GTI.PWMOutput enableFaderPin;
        private GTI.SPI forwardLEDs;
        private GTI.PWMOutput leftMotor;
        private GTI.PWMOutput rightMotor;
        private GTI.DigitalOutput leftMotorDirection;
        private GTI.DigitalOutput rightMotorDirection;
        private GTI.PWMOutput servo;

        private ushort ledMask;
        private bool leftMotorInverted;
        private bool rightMotorInverted;
        private uint servoPulseFactor;
        private uint servoMinPulse;
        private uint servoMaxPulse;
        private bool servoConfigured;

		private bool configSet = false;
		// The mainboard constructor gets called before anything else in Gadgeteer (module constructors, etc), 
		// so it can set up fields in Gadgeteer.dll specifying socket types supported, etc.

		/// <summary>
		/// Instantiates a new FEZCerbot mainboard
		/// </summary>
		public FEZCerbot()
		{
			// uncomment the following if you support NativeI2CWriteRead for faster DaisyLink performance
			// otherwise, the DaisyLink I2C interface will be supported in Gadgeteer.dll in managed code.
			GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate nativeI2C = new GT.Socket.SocketInterfaces.NativeI2CWriteReadDelegate(NativeI2CWriteRead);

			this.NativeBitmapConverter = new BitmapConvertBPP(BitmapConverter);
			this.NativeBitmapCopyToSpi = this.NativeSPIBitmapPaint;

			GT.Socket socket;

			#region examples
			// For each socket on the mainboard, create, configure and register a Socket object with Gadgeteer.dll
			// This specifies:
			// - the SupportedTypes character array matching the list on the mainboard
			// - the CpuPins array (indexes [3] to [9].  [1,2,10] are constant (3.3V, 5V, GND) and [0] is unused.  This is normally based on an enumeration supplied in the NETMF port used.
			// - for other functionality, e.g. UART, SPI, etc, properties in the Socket class are set as appropriate to enable Gadgeteer.dll to access this functionality.
			// See the Mainboard Builder's Guide and specifically the Socket Types specification for more details
			// The two examples below are not realistically implementable sockets, but illustrate how to initialize a wide range of socket functionality.

			// This example socket 1 supports many types
			// Type 'D' - no additional action
			// Type 'I' - I2C pins must be used for the correct CpuPins
			// Type 'K' and 'U' - UART pins and UART handshaking pins must be used for the correct CpuPins, and the SerialPortName property must be set.
			// Type 'S' - SPI pins must be used for the correct CpuPins, and the SPIModule property must be set 
			// Type 'X' - the NativeI2CWriteRead function pointer is set (though by default "nativeI2C" is null) 
			//socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			//socket.SupportedTypes = new char[] { 'D', 'I', 'K', 'S', 'U', 'X' };
			//socket.CpuPins[3] = (Cpu.Pin)1;
			//socket.CpuPins[4] = (Cpu.Pin)52;
			//socket.CpuPins[5] = (Cpu.Pin)23;
			//socket.CpuPins[6] = (Cpu.Pin)12;
			//socket.CpuPins[7] = (Cpu.Pin)34;
			//socket.CpuPins[8] = (Cpu.Pin)5;
			//socket.CpuPins[9] = (Cpu.Pin)7;
			//socket.NativeI2CWriteRead = nativeI2C;
			//socket.SerialPortName = "COM1";
			//socket.SPIModule = SPI.SPI_module.SPI1;
			//GT.Socket.SocketInterfaces.RegisterSocket(socket);

			// This example socket 2 supports many types
			// Type 'A' - AnalogInput3-5 properties are set and GT.Socket.SocketInterfaces.SetAnalogInputFactors call is made
			// Type 'O' - AnalogOutput property is set
			// Type 'P' - PWM7-9 properties are set
			// Type 'Y' - the NativeI2CWriteRead function pointer is set (though by default "nativeI2C" is null) 
			//socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			//socket.SupportedTypes = new char[] { 'A', 'O', 'P', 'Y' };
			//socket.CpuPins[3] = (Cpu.Pin)11;
			//socket.CpuPins[4] = (Cpu.Pin)5;
			//socket.CpuPins[5] = (Cpu.Pin)3;
			//socket.CpuPins[6] = (Cpu.Pin)66;
			// Pin 7 not connected on this socket, so it is left unspecified
			//socket.CpuPins[8] = (Cpu.Pin)59;
			//socket.CpuPins[9] = (Cpu.Pin)18;
			//socket.NativeI2CWriteRead = nativeI2C;
			//socket.AnalogOutput = new FEZCerbot_AnalogOut((Cpu.Pin)14);
			//GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 1, 2, 10);
			//socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_2;
			//socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_3;
			//socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_1;
			//socket.PWM7 = Cpu.PWMChannel.PWM_3;
			//socket.PWM8 = Cpu.PWMChannel.PWM_0;
			//socket.PWM9 = Cpu.PWMChannel.PWM_2;
			//GT.Socket.SocketInterfaces.RegisterSocket(socket);

			#endregion

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'F', 'Y' };
			socket.CpuPins[3] = FEZCerberus.Pin.PB15;
			socket.CpuPins[4] = FEZCerberus.Pin.PC8;
			socket.CpuPins[5] = FEZCerberus.Pin.PC9;
			socket.CpuPins[6] = FEZCerberus.Pin.PD2;
			socket.CpuPins[7] = FEZCerberus.Pin.PC10;
			socket.CpuPins[8] = FEZCerberus.Pin.PC11;
			socket.CpuPins[9] = FEZCerberus.Pin.PC12;
			socket.NativeI2CWriteRead = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'I', 'X' };
			socket.CpuPins[3] = FEZCerberus.Pin.PC5;
			socket.CpuPins[4] = FEZCerberus.Pin.PA10;
			socket.CpuPins[5] = FEZCerberus.Pin.PB12;
			socket.CpuPins[6] = FEZCerberus.Pin.PC7;
			socket.CpuPins[8] = FEZCerberus.Pin.PB7;
			socket.CpuPins[9] = FEZCerberus.Pin.PB6;
			socket.NativeI2CWriteRead = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
			socket.SupportedTypes = new char[] { 'U', 'S', 'X' };
			socket.CpuPins[3] = FEZCerberus.Pin.PB8;
			socket.CpuPins[4] = FEZCerberus.Pin.PB10;
			socket.CpuPins[5] = FEZCerberus.Pin.PB11;
			socket.CpuPins[6] = FEZCerberus.Pin.PA0;
			socket.CpuPins[7] = FEZCerberus.Pin.PB5;
			socket.CpuPins[8] = FEZCerberus.Pin.PB4;
			socket.CpuPins[9] = FEZCerberus.Pin.PB3;
			socket.NativeI2CWriteRead = nativeI2C;
			socket.SerialPortName = "COM3";
			socket.SPIModule = SPI.SPI_module.SPI1;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
			socket.SupportedTypes = new char[] { 'A', 'O', 'I', 'X' };
			socket.CpuPins[3] = FEZCerberus.Pin.PC0;
			socket.CpuPins[4] = FEZCerberus.Pin.PC1;
			socket.CpuPins[5] = FEZCerberus.Pin.PA4;
			socket.CpuPins[6] = FEZCerberus.Pin.PA1;
			socket.CpuPins[8] = FEZCerberus.Pin.PB7;
			socket.CpuPins[9] = FEZCerberus.Pin.PB6;
			socket.NativeI2CWriteRead = nativeI2C;
			socket.AnalogOutput = new FEZCerbot_AnalogOut(Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0);
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_3;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_4;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_5;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
			socket.SupportedTypes = new char[] { 'U', 'I' };
			socket.CpuPins[3] = FEZCerberus.Pin.PC14;
			socket.CpuPins[4] = FEZCerberus.Pin.PA2;
			socket.CpuPins[5] = FEZCerberus.Pin.PA3;
			socket.CpuPins[6] = FEZCerberus.Pin.PC15;
			socket.CpuPins[8] = FEZCerberus.Pin.PB7;
			socket.CpuPins[9] = FEZCerberus.Pin.PB6;
			socket.NativeI2CWriteRead = nativeI2C;
			socket.SerialPortName = "COM2";
			GT.Socket.SocketInterfaces.RegisterSocket(socket);


			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
			socket.SupportedTypes = new char[] { 'P' };
			socket.CpuPins[3] = FEZCerberus.Pin.PC13;
			socket.CpuPins[6] = FEZCerberus.Pin.PC3;
			socket.CpuPins[7] = FEZCerberus.Pin.PA9;
			socket.CpuPins[8] = FEZCerberus.Pin.PB9;
			socket.CpuPins[9] = FEZCerberus.Pin.PA8;
			socket.PWM7 = (Cpu.PWMChannel)12;
			socket.PWM8 = (Cpu.PWMChannel)15;
			socket.PWM9 = Cpu.PWMChannel.PWM_3;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);




            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
            socket.SupportedTypes = new char[] { 'S', 'Y', 'X' };
            socket.CpuPins[3] = FEZCerberus.Pin.PB13;
            socket.CpuPins[4] = FEZCerberus.Pin.PB14;
            socket.CpuPins[5] = (Cpu.Pin)(-1);
            socket.CpuPins[6] = FEZCerberus.Pin.PB2;
            socket.CpuPins[7] = FEZCerberus.Pin.PB5;
            socket.CpuPins[8] = FEZCerberus.Pin.PB4;
            socket.CpuPins[9] = FEZCerberus.Pin.PB3;
            socket.SPIModule = SPI.SPI_module.SPI1;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(11);
            socket.SupportedTypes = new char[] { 'X', 'P' };
            socket.CpuPins[3] = FEZCerberus.Pin.PA6;
            socket.CpuPins[4] = FEZCerberus.Pin.PC4;
            socket.CpuPins[5] = (Cpu.Pin)(-1);
            socket.CpuPins[6] = (Cpu.Pin)(-1);
            socket.CpuPins[7] = FEZCerberus.Pin.PB0;
            socket.CpuPins[8] = FEZCerberus.Pin.PB1;
            socket.CpuPins[9] = FEZCerberus.Pin.PA7;
            socket.PWM7 = Cpu.PWMChannel.PWM_4;
            socket.PWM8 = Cpu.PWMChannel.PWM_5;
            socket.PWM9 = Cpu.PWMChannel.PWM_1;
            GT.Socket.SocketInterfaces.RegisterSocket(socket);


            socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(12);
            socket.SupportedTypes = new char[] { 'A', 'P' };
            socket.CpuPins[3] = FEZCerberus.Pin.PA5;
            socket.CpuPins[4] = FEZCerberus.Pin.PC2;
            socket.CpuPins[5] = (Cpu.Pin)(-1);
            socket.CpuPins[6] = (Cpu.Pin)(-1);
            socket.CpuPins[7] = FEZCerberus.Pin.PB0;
            socket.CpuPins[8] = FEZCerberus.Pin.PB1;
            socket.CpuPins[9] = (Cpu.Pin)(-1);
            socket.PWM7 = Cpu.PWMChannel.PWM_0;
            socket.PWM8 = (Cpu.PWMChannel)13;
            socket.PWM9 = (Cpu.PWMChannel)(-1);
            socket.AnalogInput3 = (Cpu.AnalogChannel)8;
            socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_6;
            socket.AnalogInput5 = (Cpu.AnalogChannel)(-1);
            GT.Socket.SocketInterfaces.RegisterSocket(socket);

            this.Initialize();
		}

		bool NativeI2CWriteRead(GT.Socket socket, GT.Socket.Pin sda, GT.Socket.Pin scl, byte address, byte[] write, int writeOffset, int writeLen, byte[] read, int readOffset, int readLen, out int numWritten, out int numRead)
		{
			return GHI.OSHW.Hardware.SoftwareI2CBus.DirectI2CWriteRead(socket.CpuPins[(int)scl], socket.CpuPins[(int)sda], 100, address, write, writeOffset, writeLen, read, readOffset, readLen, out numWritten, out numRead);
		}

		private void NativeSPIBitmapPaint(Bitmap bitmap, SPI.Configuration config, int xSrc, int ySrc, int width, int height, GT.Mainboard.BPP bpp)
		{
			if (bpp != BPP.BPP16_BGR_BE)
				throw new ArgumentException("Invalid BPP");

			if (!this.configSet)
			{
				Util.SetSpecialDisplayConfig(config, Util.BPP_Type.BPP16_BGR_LE);

				this.configSet = true;
			}

			bitmap.Flush(xSrc, ySrc, width, height);
		}

		void BitmapConverter(byte[] bitmapBytes, byte[] pixelBytes, GT.Mainboard.BPP bpp)
		{
			if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE)
				throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

			Util.BitmapConvertBPP(bitmapBytes, pixelBytes, Util.BPP_Type.BPP16_BGR_BE);
		}

		private static string[] sdVolumes = new string[] { "SD" };

		/// <summary>
		/// Allows mainboards to support storage device mounting/umounting.  This provides modules with a list of storage device volume names supported by the mainboard. 
		/// </summary>
		public override string[] GetStorageDeviceVolumeNames()
		{
			return sdVolumes;
		}

		/// <summary>
		/// Functionality provided by mainboard to mount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a <see cref="Microsoft.SPOT.IO.RemovableMedia.Insert"/> event if successful.
		/// </summary>
		public override bool MountStorageDevice(string volumeName)
		{
			StorageDev.MountSD();

			return volumeName == "SD";
		}

		/// <summary>
		/// Functionality provided by mainboard to ummount storage devices, given the volume name of the storage device (see <see cref="GetStorageDeviceVolumeNames"/>).
		/// This should result in a <see cref="Microsoft.SPOT.IO.RemovableMedia.Eject"/> event if successful.
		/// </summary>
		public override bool UnmountStorageDevice(string volumeName)
		{
			StorageDev.UnmountSD();

			return volumeName == "SD";
		}

		/// <summary>
		/// Changes the programming interafces to the one specified
		/// </summary>
		/// <param name="programmingInterface">The programming interface to use</param>
		public override void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface)
		{
			// Change the reflashing interface to the one specified, if possible.
			// This is an advanced API that we don't expect people to call much.
		}

		/// <summary>
		/// This sets the LCD configuration.  If the value GT.Mainboard.LCDConfiguration.HeadlessConfig (=null) is specified, no display support should be active.
		/// If a non-null value is specified but the property LCDControllerEnabled is false, the LCD controller should be disabled if present,
		/// though the Bitmap width/height for WPF should be modified to the Width and Height parameters.  This must reboot if the LCD configuration changes require a reboot.
		/// </summary>
		/// <param name="lcdConfig">The LCD Configuration</param>
		public override void SetLCDConfiguration(GT.Mainboard.LCDConfiguration lcdConfig)
		{
		}

		/// <summary>
		/// Configures rotation in the LCD controller. This must reboot if performing the LCD rotation requires a reboot.
		/// </summary>
		/// <param name="rotation">The LCD rotation to use</param>
		/// <returns>true if the rotation is supported</returns>
		public override bool SetLCDRotation(GT.Modules.Module.DisplayModule.LCDRotation rotation)
		{
			return false;
		}

		// change the below to the debug led pin on this mainboard
		private const Cpu.Pin DebugLedPin = FEZCerberus.Pin.PA14;

		private Microsoft.SPOT.Hardware.OutputPort debugled = new OutputPort(DebugLedPin, false);
		/// <summary>
		/// Turns the debug LED on or off
		/// </summary>
		/// <param name="on">True if the debug LED should be on</param>
		public override void SetDebugLED(bool on)
		{
			debugled.Write(on);
		}

		/// <summary>
		/// This performs post-initialization tasks for the mainboard.  It is called by Gadgeteer.Program.Run and does not need to be called manually.
		/// </summary>
		public override void PostInit()
		{
			return;
		}

		/// <summary>
		/// The mainboard name, which is printed at startup in the debug window
		/// </summary>
		public override string MainboardName
		{
			get { return "GHI Electronics FEZCerbot"; }
		}

		/// <summary>
		/// The mainboard version, which is printed at startup in the debug window
		/// </summary>
		public override string MainboardVersion
		{
			get { return "1.0"; }
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

            this.forwardLEDs = new GTI.SPI(spiSocket, new GTI.SPI.Configuration(false, 0, 0, false, true, 2000), GTI.SPI.Sharing.Shared, spiSocket, GT.Socket.Pin.Six, null);
            this.leftIRLED = new GTI.DigitalOutput(spiSocket, GT.Socket.Pin.Three, true, null);
            this.rightIRLED = new GTI.DigitalOutput(spiSocket, GT.Socket.Pin.Four, true, null);

            this.leftMotorDirection = new GTI.DigitalOutput(pwmSocket, GT.Socket.Pin.Three, false, null);
            this.rightMotorDirection = new GTI.DigitalOutput(pwmSocket, GT.Socket.Pin.Four, false, null);
            this.leftMotor = new GTI.PWMOutput(pwmSocket, GT.Socket.Pin.Seven, false, null);
            this.rightMotor = new GTI.PWMOutput(pwmSocket, GT.Socket.Pin.Eight, false, null);
            this.servo = new GTI.PWMOutput(pwmSocket, GT.Socket.Pin.Nine, false, null);

            this.buzzer = new GTI.PWMOutput(analogSocket, GT.Socket.Pin.Seven, false, null);
            this.enableFaderPin = new GTI.PWMOutput(analogSocket, GT.Socket.Pin.Eight, true, null);
            this.leftSensor = new GTI.AnalogInput(analogSocket, GT.Socket.Pin.Three, null);
            this.rightSensor = new GTI.AnalogInput(analogSocket, GT.Socket.Pin.Four, null);

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
        public void StartBuzzer(int frequency, uint duration = 0, double dutyCycle = 0.5)
        {
            this.buzzer.Active = false;

            if (frequency <= 0)
                return;

            this.buzzer.Set(frequency, dutyCycle);

            if (duration != 0)
            {
                Thread.Sleep((int)duration);
                this.buzzer.Active = false;
            }
        }

        /// <summary>
        /// Stops the buzzer from buzzing.
        /// </summary>
        public void StopBuzzer()
        {
            this.buzzer.Active = false;
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
                this.servo.Active = false;
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

        private void SetSpeed(GTI.PWMOutput motor, GTI.DigitalOutput direction, int speed, bool isLeft)
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

	internal class FEZCerbot_AnalogOut : GT.Socket.SocketInterfaces.AnalogOutput
	{
		private AnalogOutput aout = null;

		Cpu.AnalogOutputChannel pin;
		const double MIN_VOLTAGE = 0;
		const double MAX_VOLTAGE = 3.3;

		public FEZCerbot_AnalogOut(Cpu.AnalogOutputChannel pin)
		{
			this.pin = pin;
		}

		public double MinOutputVoltage
		{
			get
			{
				return FEZCerbot_AnalogOut.MIN_VOLTAGE;
			}
		}

		public double MaxOutputVoltage
		{
			get
			{
				return FEZCerbot_AnalogOut.MAX_VOLTAGE;
			}
		}

		public bool Active
		{
			get
			{
				return this.aout != null;
			}
			set
			{
				if (value == this.Active)
					return;

				if (value)
				{
					this.aout = new AnalogOutput(this.pin, 1 / FEZCerbot_AnalogOut.MAX_VOLTAGE, 0, 12);
					this.SetVoltage(FEZCerbot_AnalogOut.MIN_VOLTAGE);
				}
				else
				{
					this.aout.Dispose();
					this.aout = null;
				}
			}
		}

		public void SetVoltage(double voltage)
		{
			this.Active = true;

			if (voltage < FEZCerbot_AnalogOut.MIN_VOLTAGE)
				throw new ArgumentOutOfRangeException("The minimum voltage of the analog output interface is " + FEZCerbot_AnalogOut.MIN_VOLTAGE.ToString() + "V");

			if (voltage > FEZCerbot_AnalogOut.MAX_VOLTAGE)
				throw new ArgumentOutOfRangeException("The maximum voltage of the analog output interface is " + FEZCerbot_AnalogOut.MAX_VOLTAGE.ToString() + "V");

			this.aout.Write(voltage);
		}
	}
}