using Microsoft.SPOT;
using System.Threading;
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
        private Thread worker;

        private const int ID_LENGTH = 12;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public RFIDReader(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('U', this);

            this.port = GTI.SerialFactory.Create(socket, 9600, GTI.SerialParity.None, GTI.SerialStopBits.Two, 8, GTI.HardwareFlowControl.NotRequired, this);
            this.port.Open();

            this.worker = new Thread(DoWork);
            this.worker.Start();
        }

        private byte ASCIIToNumber(char upper, char lower)
        {
            byte high = (byte)(upper - 48 - (upper >= 'A' ? 7 : 0));
            byte low = (byte)(lower - 48 - (lower >= 'A' ? 7 : 0));

            return (byte)((high << 4) | low);
        }

        private void DoWork()
        {
            var id = string.Empty;

            while (true)
            {
                Thread.Sleep(10);

                int available = this.port.BytesToRead;

                if (available <= 0)
                    continue;

                var buffer = new byte[available];

                this.port.Read(buffer, 0, available);

                for (int i = 0; i < buffer.Length; i++)
                {
                    id += (char)buffer[i];

                    if (id.Length < RFIDReader.ID_LENGTH)
                        continue;

                    if (id[0] != 2)
                    {
                        id = string.Empty;

                        this.HandleChecksumError();

                        break;
                    }

                    int cs = 0;
                    for (int x = 1; x < 10; x += 2)
                        cs ^= this.ASCIIToNumber((char)id[x], (char)id[x + 1]);

                    if (cs != id[11])
                    {
                        id = string.Empty;

                        this.HandleChecksumError();

                        break;
                    }

                    this.OnIdReceivedEvent(this, id.Substring(1, 10));

                    Thread.Sleep(100);

                    id = string.Empty;
                }
            }
        }

        private void HandleChecksumError()
        {
            this.port.DiscardInBuffer();

            this.OnBadChecksumReceived(this, null);

            Thread.Sleep(100);
        }

        /// <summary>
        /// The delegate that is used to handle the id received event.
        /// </summary>
        /// <param name="sender">The <see cref="RFIDReader"/> object that raised the event.</param>
        /// <param name="e">The event argument.</param>
        public delegate void IdReceivedEventHandler(RFIDReader sender, string e);

        /// <summary>
        /// The delegate that is used to handle the bad checksum event.
        /// </summary>
        /// <param name="sender">The <see cref="RFIDReader"/> object that raised the event.</param>
        /// <param name="e">The event argument.</param>
        public delegate void BadChecksumReceivedEventHandler(RFIDReader sender, EventArgs e);
        
        /// <summary>
        /// Raised when the module receives an id.
        /// </summary>
        public event IdReceivedEventHandler IdReceived;

        /// <summary>
        /// Raised when the module receives an id with an incorrect checksum.
        /// </summary>
        public event BadChecksumReceivedEventHandler BadChecksumReceived;

        private IdReceivedEventHandler onIdReceived;
        private BadChecksumReceivedEventHandler onBadChecksumReceived;

        private void OnIdReceivedEvent(RFIDReader sender, string e)
        {
            if (this.onIdReceived == null)
                this.onIdReceived = this.OnIdReceivedEvent;

            if (Program.CheckAndInvoke(this.IdReceived, this.onIdReceived, sender, e))
                this.IdReceived(sender, e);
        }

        private void OnBadChecksumReceived(RFIDReader sender, EventArgs e)
        {
            if (this.onBadChecksumReceived == null)
                this.onBadChecksumReceived = this.OnBadChecksumReceived;

            if (Program.CheckAndInvoke(this.BadChecksumReceived, this.onBadChecksumReceived, sender, e))
                this.BadChecksumReceived(sender, e);
        }
    }
}
