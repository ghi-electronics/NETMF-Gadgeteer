using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A XBeeAdapter module for Microsoft .NET Gadgeteer
    /// </summary>
    public class XBeeAdapter : GTM.Module
    {
        private GT.SocketInterfaces.Serial serialPort;
        private Socket socket;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public XBeeAdapter(int socketNumber)
        {
            this.socket = Socket.GetSocket(socketNumber, true, this, null);
            this.socket.EnsureTypeIsSupported('U', this);
            this.serialPort = null;
        }

        /// <summary>
        /// Initializes the serial port with the parameters 115200 baud, 8N1, with no flow control.
        /// </summary>
        public void Configure()
        {
            this.Configure(115200, GTI.SerialParity.None, GTI.SerialStopBits.One, 8, GTI.HardwareFlowControl.NotRequired);
        }

        /// <summary>
        /// Initializes the serial port with the given parameters.
        /// </summary>
        /// <param name="baudRate">The baud rate to use.</param>
        /// <param name="parity">The parity to use.</param>
        /// <param name="stopBits">The stop bits to use.</param>
        /// <param name="dataBits">The number of data bits to use.</param>
        /// <param name="flowControl">The flow control to use.</param>
        public void Configure(int baudRate, GTI.SerialParity parity, GTI.SerialStopBits stopBits, int dataBits, GTI.HardwareFlowControl flowControl)
        {
            if (this.serialPort != null) throw new InvalidOperationException("Configure can only be called once.");

            this.serialPort = GTI.SerialFactory.Create(socket, baudRate, parity, stopBits, dataBits, flowControl, this);
            this.serialPort.Open();
        }

        /// <summary>
        /// The underlying serial port object.
        /// </summary>
        public GT.SocketInterfaces.Serial Port
        {
            get
            {
                if (this.serialPort == null) throw new InvalidOperationException("You must call Configure first.");

                return this.serialPort;
            }
        }
    }
}