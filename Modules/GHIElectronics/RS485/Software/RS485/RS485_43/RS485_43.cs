using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// A RS485 module for Microsoft .NET Gadgeteer
	/// </summary>
	public class RS485 : GTM.Module
	{
		private GTI.Serial port;
		private GT.Socket socket;

		/// <summary>Constructs a new RS485 instance.</summary>
		/// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public RS485(int socketNumber)
		{
			this.socket = Socket.GetSocket(socketNumber, true, this, null);

			this.socket.EnsureTypeIsSupported('U', this);
		}

        /// <summary>
        /// Initializes the serial port to the passed in parameters. If no parameters are passed, the defaults are used.
        /// </summary>
        /// <param name="baudRate">The baud rate for the serail port. Defaulted to 38400.</param>
        /// <param name="parity">Specifies the parity bit for the serial port. Defaulted to none.</param>
        /// <param name="stopBits">Specifies the number of stop bits used on the serial port. Defaulted to one.</param>
        /// <param name="dataBits">The number of data bits. Defaulted to 8.</param>
        public GTI.Serial Initialize(int baudRate = 38400, GTI.SerialParity parity = GTI.SerialParity.None, GTI.SerialStopBits stopBits = GTI.SerialStopBits.One, int dataBits = 8)
        {
            this.port = GTI.SerialFactory.Create(this.socket, baudRate, parity, stopBits, dataBits, GTI.HardwareFlowControl.NotRequired, this);
            this.port.Open();
			return this.port;
        }

		/// <summary>
		/// The serial port the module provides.
		/// </summary>
		public GTI.Serial Port
		{
			get
			{
				  return this.port;
			}
		}
	}
}
