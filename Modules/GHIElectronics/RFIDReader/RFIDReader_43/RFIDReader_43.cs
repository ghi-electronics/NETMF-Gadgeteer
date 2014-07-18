using Microsoft.SPOT;
using System.Text;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// An RFIDReader module for Microsoft .NET Gadgeteer
    /// </summary>
    public class RFIDReader : GTM.Module
    {
        private GT.SocketInterfaces.Serial port;
        private GT.Timer timer;
        private byte[] buffer;
        private int read;
        private int checksum;

        private const int MESSAGE_LENGTH = 13;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public RFIDReader(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('U', this);

            this.buffer = new byte[RFIDReader.MESSAGE_LENGTH];
            this.read = 0;
            this.checksum = 0;

            this.port = GTI.SerialFactory.Create(socket, 9600, GTI.SerialParity.None, GTI.SerialStopBits.Two, 8, GTI.HardwareFlowControl.NotRequired, this);
            this.port.ReadTimeout = 10;
            this.port.Open();

            this.timer = new GT.Timer(100);
            this.timer.Tick += this.DoWork;
            this.timer.Start();
        }

        private int ASCIIToNumber(byte upper, byte lower)
        {
            var high = upper - 48 - (upper >= 'A' ? 7 : 0);
            var low = lower - 48 - (lower >= 'A' ? 7 : 0);

            return (high << 4) | low;
        }

        private void DoWork(object o)
        {
            this.read += this.port.Read(this.buffer, this.read, RFIDReader.MESSAGE_LENGTH - this.read);

            if (this.read != RFIDReader.MESSAGE_LENGTH)
                return;

            for (int i = 1; i < 10; i += 2)
                this.checksum ^= this.ASCIIToNumber(this.buffer[i], this.buffer[i + 1]);

            if (this.buffer[0] == 0x02 && this.buffer[12] == 0x03 && this.checksum == this.buffer[11])
            {
                this.OnIdReceived(this, new string(Encoding.UTF8.GetChars(this.buffer, 1, 10)));
            }
            else
            {
                this.port.DiscardInBuffer();

                this.OnMalformedIdReceived(this, null);
            }

            this.read = 0;
            this.checksum = 0;
        }

        /// <summary>
        /// The delegate that is used to handle the id received event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void IdReceivedEventHandler(RFIDReader sender, string e);

        /// <summary>
        /// The delegate that is used to handle the bad checksum event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void MalformedIdReceivedEventHandler(RFIDReader sender, EventArgs e);
        
        /// <summary>
        /// Raised when the module receives an id.
        /// </summary>
        public event IdReceivedEventHandler IdReceived;

        /// <summary>
        /// Raised when the module receives an id with an incorrect checksum.
        /// </summary>
        public event MalformedIdReceivedEventHandler MalformedIdReceived;

        private IdReceivedEventHandler onIdReceived;
        private MalformedIdReceivedEventHandler onMalformedIdReceived;

        private void OnIdReceived(RFIDReader sender, string e)
        {
            if (this.onIdReceived == null)
                this.onIdReceived = this.OnIdReceived;

            if (Program.CheckAndInvoke(this.IdReceived, this.onIdReceived, sender, e))
                this.IdReceived(sender, e);
        }

        private void OnMalformedIdReceived(RFIDReader sender, EventArgs e)
        {
            if (this.onMalformedIdReceived == null)
                this.onMalformedIdReceived = this.OnMalformedIdReceived;

            if (Program.CheckAndInvoke(this.MalformedIdReceived, this.onMalformedIdReceived, sender, e))
                this.MalformedIdReceived(sender, e);
        }
    }
}
