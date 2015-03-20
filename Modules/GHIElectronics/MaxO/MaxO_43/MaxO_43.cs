using System;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A MaxO module for Microsoft .NET Gadgeteer
    /// </summary>
    public class MaxO : GTM.Module
    {
        private GTI.Spi spi;
        private GTI.DigitalOutput enable;
        private GTI.DigitalOutput clr;
        private byte[] data;
        private int boards;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public MaxO(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('S', this);

            this.spi = GTI.SpiFactory.Create(socket, new GTI.SpiConfiguration(false, 0, 0, false, true, 1000), GTI.SpiSharing.Shared, socket, Socket.Pin.Five, this);
            this.enable = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three, false, this);
            this.clr = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Four, true, this);
            this.boards = 0;
            this.data = null;
        }

        /// <summary>
        /// The number modules chained together.
        /// </summary>
        public int Boards
        {
            get
            {
                return this.boards;
            }
            set
            {
                if (this.data != null) throw new InvalidOperationException("You may only set boards once.");
                if (value <= 0) throw new ArgumentOutOfRangeException("value", "value must be positive.");

                this.boards = value;
                this.data = new byte[value * 4];
            }
        }

        /// <summary>
        /// The size of the array to be filled.
        /// </summary>
        public int ArraySize
        {
            get
            {
                return this.data.Length;
            }
        }

        /// <summary>
        /// Sets the state of the module output.
        /// </summary>
        public bool OutputEnabled
        {
            get
            {
                return !this.enable.Read();
            }
            set
            {
                this.enable.Write(!value);
            }
        }

        /// <summary>
        /// Clears all registers.
        /// </summary>
        public void Clear()
        {
            if (this.data == null) throw new InvalidOperationException("You must set Boards first.");

            this.enable.Write(true);
            this.clr.Write(false);

            Thread.Sleep(10);

            this.spi.Write(new byte[] { 0 });

            this.clr.Write(true);
            this.enable.Write(false);

            for (int i = 0; i < this.data.Length; i++)
                this.data[i] = 0x0;
        }

        /// <summary>
        /// Writes the buffer to the modules.
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        public void Write(byte[] buffer)
        {
            if (this.data == null) throw new InvalidOperationException("You must set Boards first.");
            if (buffer.Length != this.data.Length) throw new ArgumentException("array", "array.Length must be the same size as ArraySize.");

            this.enable.Write(true);

            byte[] reversed = new byte[buffer.Length];
            for (int i = 0; i < reversed.Length; i++)
                reversed[i] = buffer[reversed.Length - i - 1];

            this.spi.Write(reversed);

            if (this.data != buffer)
                Array.Copy(buffer, this.data, buffer.Length);

            this.enable.Write(false);
        }

        /// <summary>
        /// Sets the state of the specified pin on the specified board.
        /// </summary>
        /// <param name="board">The board to write to.</param>
        /// <param name="pin">The pin to write.</param>
        /// <param name="value">The value to write to the pin.</param>
        public void SetPin(int board, int pin, bool value)
        {
            if (this.data == null) throw new InvalidOperationException("You must set Boards first.");
            if (board * 4 > this.data.Length) throw new ArgumentException("board", "The board is out of range.");

            int index = (board - 1) * 4 + pin / 8;

            if (value)
            {
                this.data[index] = (byte)(this.data[index] | (1 << (pin % 8)));
            }
            else
            {
                this.data[index] = (byte)(this.data[index] & ~(1 << (pin % 8)));
            }

            this.Write(this.data);
        }

        /// <summary>
        /// The data currently on the modules.
        /// </summary>
        /// <returns>The data.</returns>
        public byte[] Read()
        {
            if (this.data == null) throw new InvalidOperationException("You must set Boards first.");

            byte[] buffer = new byte[this.data.Length];

            Array.Copy(this.data, buffer, this.data.Length);

            return this.data;
        }
    }
}