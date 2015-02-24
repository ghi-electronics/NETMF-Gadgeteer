using System;
using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
	/// A RotaryH1 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class RotaryH1 : GTM.Module
    {
        private byte[] write1;
		private byte[] write2;
		private byte[] read2;
		private byte[] read4;

        private GTI.DigitalOutput enable;
        private GTI.DigitalInput miso;
        private GTI.DigitalOutput mosi;
        private GTI.DigitalOutput clock;
        private GTI.DigitalOutput cs;

		/// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public RotaryH1(int socketNumber)
        {
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('Y', this);

            this.write1 = new byte[1];
            this.write2 = new byte[2];
            this.read2 = new byte[2];
            this.read4 = new byte[4];

            this.cs = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Six, true, this);
            this.miso = GTI.DigitalInputFactory.Create(socket, Socket.Pin.Eight, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, this);
            this.mosi = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Seven, false, this);
            this.clock = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Nine, false, this);
            this.enable = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Five, true, this);
            
			this.Initialize();
		}

		/// <summary>
		/// Gets the current count of the encoder.
		/// </summary>
		/// <returns>An integer representing the count.</returns>
		public int GetCount()
		{
			return this.Read2(Commands.LS7366_READ | Commands.LS7366_CNTR);
		}

		/// <summary>
		/// Gets the current direction that the encoder count is going.
		/// </summary>
		/// <returns>The direction the encoder count is going.</returns>
		public Direction GetDirection()
		{
			return ((this.GetStatus() & 0x02) >> 1) > 0 ? Direction.CounterClockwise : Direction.Clockwise;
		}

        private void Initialize()
        {
            this.Write(Commands.LS7366_CLEAR | Commands.LS7366_MDR0);
            this.Write(Commands.LS7366_CLEAR | Commands.LS7366_MDR1);
            this.Write(Commands.LS7366_CLEAR | Commands.LS7366_STR);
            this.Write(Commands.LS7366_CLEAR | Commands.LS7366_CNTR);
            this.Write(Commands.LS7366_LOAD | Commands.LS7366_OTR);

            this.Write(Commands.LS7366_WRITE | Commands.LS7366_MDR0, MDR0Modes.LS7366_MDR0_QUAD1 | MDR0Modes.LS7366_MDR0_FREER | MDR0Modes.LS7366_MDR0_DIDX | MDR0Modes.LS7366_MDR0_FFAC2);
            this.Write(Commands.LS7366_WRITE | Commands.LS7366_MDR1, MDR1Modes.LS7366_MDR1_2BYTE | MDR1Modes.LS7366_MDR1_ENCNT);
        }

		private byte GetStatus()
		{
            return this.Read1(Commands.LS7366_READ | Commands.LS7366_STR);
		}

        private byte Read1(Commands register)
        {
			this.write1[0] = (byte)register;

			this.WriteRead(this.write1, this.read2);

			return this.read2[1];
        }

        private short Read2(Commands register)
        {
            this.write1[0] = (byte)register;

			this.WriteRead(this.write1, this.read4);

            return (short)((this.read4[1] << 8) | this.read4[2]);
        }

        private void Write(Commands register)
        {
            this.write1[0] = (byte)register;

			this.WriteRead(this.write1, null);
		}

        private void Write(Commands register, MDR0Modes command)
        {
            this.write2[0] = (byte)register;
			this.write2[1] = (byte)command;

			this.WriteRead(this.write2, null);
		}

        private void Write(Commands register, MDR1Modes command)
        {
            this.write2[0] = (byte)register;
            this.write2[1] = (byte)command;

			this.WriteRead(this.write2, null);
        }

        private void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            int writeLength = writeBuffer.Length;
            int readLength = 0;

            if (readBuffer != null)
            {
                readLength = readBuffer.Length;

                for (int i = 0; i < readLength; i++)
                    readBuffer[i] = 0;
            }


            this.cs.Write(false);

            for (int i = 0; i < (writeLength < readLength ? readLength : writeLength); i++)
            {
                byte w = 0;

                if (i < writeLength)
                    w = writeBuffer[i];

                byte mask = 0x80;

                for (int j = 0; j < 8; j++)
                {
                    this.clock.Write(false);

                    this.mosi.Write((w & mask) != 0);

                    this.clock.Write(true);

                    if (readBuffer != null)
                        readBuffer[i] |= (this.miso.Read() ? mask : (byte)0x00);

                    mask >>= 1;
                }

                this.mosi.Write(false);
                this.clock.Write(false);
            }

            Thread.Sleep(20);

            this.cs.Write(true);
        }

		/// <summary>
		/// The direction the encoder is being turned.
		/// </summary>
		public enum Direction : byte
		{
			/// <summary>
			/// The encoder is moving in a counter-clockwise direction.
			/// </summary>
			CounterClockwise,

			/// <summary>
			/// The encoder is moving in a clockwise direction.
			/// </summary>
			Clockwise
		}

        [Flags]
		private enum Commands : byte
		{
			LS7366_CLEAR = 0x00,
			LS7366_READ = 0x40,
			LS7366_WRITE = 0x80,
            LS7366_LOAD = 0xC0,

            LS7366_MDR0 = 0x08,
            LS7366_MDR1 = 0x10,
            LS7366_DTR = 0x18,
            LS7366_CNTR = 0x20,
            LS7366_OTR = 0x28,
            LS7366_STR = 0x30,
		}

        [Flags]
		private enum MDR0Modes : byte
		{
			LS7366_MDR0_QUAD0 = 0x00,
			LS7366_MDR0_QUAD1 = 0x01,
			LS7366_MDR0_QUAD2 = 0x02,
			LS7366_MDR0_QUAD4 = 0x03,
			LS7366_MDR0_FREER = 0x00,
			LS7366_MDR0_SICYC = 0x04,
			LS7366_MDR0_RANGE = 0x08,
			LS7366_MDR0_MODTR = 0x0C,
			LS7366_MDR0_DIDX = 0x00,
			LS7366_MDR0_LDCNT = 0x10,
			LS7366_MDR0_RECNT = 0x20,
			LS7366_MDR0_LDOTR = 0x30,
			LS7366_MDR0_ASIDX = 0x00,
			LS7366_MDR0_SYINX = 0x40,
			LS7366_MDR0_FFAC1 = 0x00,
			LS7366_MDR0_FFAC2 = 0x80,
			LS7366_MDR0_NOFLA = 0x00,
		}

        [Flags]
		private enum MDR1Modes : byte
		{
			LS7366_MDR1_4BYTE = 0x00,
			LS7366_MDR1_3BYTE = 0x01,
			LS7366_MDR1_2BYTE = 0x02,
			LS7366_MDR1_1BYTE = 0x03,
			LS7366_MDR1_ENCNT = 0x00,
			LS7366_MDR1_DICNT = 0x04,
			LS7366_MDR1_FLIDX = 0x10,
            LS7366_MDR1_FLCMP = 0x20,
            LS7366_MDR1_FLBW = 0x40,
            LS7366_MDR1_FLCY = 0x80,
		}
    }
}
