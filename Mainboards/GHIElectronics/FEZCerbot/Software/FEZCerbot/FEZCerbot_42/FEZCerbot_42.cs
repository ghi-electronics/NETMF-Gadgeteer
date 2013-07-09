using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GT = Gadgeteer;

using GHI.OSHW.Hardware;
using FEZCerberus = GHI.Hardware.FEZCerb;

namespace GHIElectronics.Gadgeteer
{
	/// <summary>
	/// Support class for GHI Electronics FEZCerbot for Microsoft .NET Gadgeteer
	/// </summary>
	public class FEZCerbot : GT.Mainboard
	{
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
		}

		bool NativeI2CWriteRead(GT.Socket socket, GT.Socket.Pin sda, GT.Socket.Pin scl, byte address, byte[] write, int writeOffset, int writeLen, byte[] read, int readOffset, int readLen, out int numWritten, out int numRead)
		{
			return GHI.OSHW.Hardware.SoftwareI2CBus.DirectI2CWriteRead(socket.CpuPins[(int)scl], socket.CpuPins[(int)sda], 100, address, write, writeOffset, writeLen, read, readOffset, readLen, out numWritten, out numRead);
		}

		void BitmapConverter(byte[] bitmapBytes, byte[] pixelBytes, GT.Mainboard.BPP bpp)
		{
			if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE)
				throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_LE supported");

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
				throw new ArgumentOutOfRangeException("The minimum voltage of the analog output interface is " + FEZCerbot_AnalogOut.MAX_VOLTAGE.ToString() + "V");

			this.aout.Write(voltage);
		}
	}
}