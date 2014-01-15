#define USE_SOFTWARE_SPI

using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.Interfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
	/// A RotaryH1 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class RotaryH1 : GTM.Module
	{
		private byte[] write1 = new byte[1];
		private byte[] write2 = new byte[2];
		private byte[] read2 = new byte[2];
		private byte[] read4 = new byte[4];

        private GTI.DigitalOutput Enable;

#if USE_SOFTWARE_SPI
        private GTI.DigitalInput MISO;
        private GTI.DigitalOutput MOSI;
        private GTI.DigitalOutput CLOCK;
        private GTI.DigitalOutput CS;
#else
        private readonly GTI.SPI.Configuration config;
        private readonly GTI.SPI spi;
#endif

		/// <summary>Constructs a new instance of the RotaryH1 module.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public RotaryH1(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

#if USE_SOFTWARE_SPI
            socket.EnsureTypeIsSupported('Y', this);

			this.CS = new GTI.DigitalOutput(socket, Socket.Pin.Six, true, this);
			this.MISO = new GTI.DigitalInput(socket, Socket.Pin.Eight, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, this);
			this.MOSI = new GTI.DigitalOutput(socket, Socket.Pin.Seven, false, this);
			this.CLOCK = new GTI.DigitalOutput(socket, Socket.Pin.Nine, false, this);
#else
            socket.EnsureTypeIsSupported('S', this);

            this.config = new GTI.SPI.Configuration(false, 0, 0, false, true, 1000);
			this.spi = new GTI.SPI(socket, this.config, GTI.SPI.Sharing.Shared, socket, GT.Socket.Pin.Six, this);
#endif

            this.Enable = new GTI.DigitalOutput(socket, Socket.Pin.Five, true, this);
            
			this.Initialize();
		}

		/// <summary>
		/// Gets the current count of the encoder.
		/// </summary>
		/// <returns>An integer representing the count.</returns>
		public int GetCount()
		{
			int count = this.Read2((byte)Commands.LS7366_READ | (byte)Registers.LS7366_CNTR);

			if ((this.ReadStatusReg() & 0x1) > 0) // native number
			{
				count = ~count;
				count &= 0x7FFF;
				count *= (-1);
			}

			return count;
		}

		/// <summary>
		/// Gets the current direction that the encoder count is going.
		/// </summary>
		/// <returns>The direction the encoder count is going.</returns>
		public Direction GetDirection()
		{
			return ((this.ReadStatusReg() & 0x2) >> 1) > 0 ? Direction.CounterClockwise : Direction.Clockwise;
		}

        private void Initialize()
        {
            this.Write((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_MDR0);
			this.Write((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_MDR1);
			this.Write((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_STR);
			this.Write((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_CNTR);
			this.Write((byte)Commands.LS7366_LOAD | (byte)Registers.LS7366_OTR);

            this.Write((byte)Commands.LS7366_WRITE | (byte)Registers.LS7366_MDR0,
                               (byte)MDR0Mode.LS7366_MDR0_QUAD1   // none quadrature mode
                             | (byte)MDR0Mode.LS7366_MDR0_FREER   // modulo-n counting 
                             | (byte)MDR0Mode.LS7366_MDR0_DIDX
                             | (byte)MDR0Mode.LS7366_MDR0_FFAC2);

			this.Write((byte)Commands.LS7366_WRITE | (byte)Registers.LS7366_MDR1,
                               (byte)MDR1Mode.LS7366_MDR1_2BYTE     // 2 byte counter mode
                             | (byte)MDR1Mode.LS7366_MDR1_ENCNT);   // enable counting
        }

		private byte ReadStatusReg()
		{
			return this.Read1((byte)((byte)Commands.LS7366_READ | (byte)Registers.LS7366_STR));
		}

        private byte Read1(byte register)
        {
			write1[0] = register;

#if USE_SOFTWARE_SPI
            this.SoftwareSPI_WriteRead(write1, read2);
#else
			this.spi.WriteRead(write1, read2);
#endif
			return read2[1];
        }

		private short Read2(byte register)
        {
			write1[0] = register;

#if USE_SOFTWARE_SPI
            this.SoftwareSPI_WriteRead(write1, read4);
#else
			this.spi.WriteRead(write1, read4);
#endif

			return (short)((read4[1] << 8) + read4[2]);
        }

        private void Write(byte register)
        {
			write1[0] = register;

#if USE_SOFTWARE_SPI
            this.SoftwareSPI_WriteRead(write1, null);
#else
			this.spi.Write(write1);
#endif
		}

        private void Write(byte register, byte command)
        {
            write2[0] = register;
			write2[1] = command;

#if USE_SOFTWARE_SPI
            this.SoftwareSPI_WriteRead(write2, null);
#else
			this.spi.Write(write2);
#endif
		}

#if USE_SOFTWARE_SPI
        private void SoftwareSPI_WriteRead(byte[] write, byte[] read)
        {
            int writeLen = write.Length;
            int readLen = 0;

            if (read != null)
            {
                readLen = read.Length;

                for (int i = 0; i < readLen; i++)
                {
                    read[i] = 0;
                }
            }

            int loopLen = (writeLen < readLen ? readLen : writeLen);

            byte w = 0;

            CS.Write(false);

            // per byte
            for (int len = 0; len < loopLen; len++)
            {
                if (len < writeLen)
                    w = write[len];

                byte mask = 0x80;

                // per bit
                for (int i = 0; i < 8; i++)
                {
                    CLOCK.Write(false);

                    if ((w & mask) == mask)
                        MOSI.Write(true);
                    else
                        MOSI.Write(false);

                    CLOCK.Write(true);

                    if (true == MISO.Read())
                        if (read != null)
                            read[len] |= mask;

                    mask >>= 1;
                }

                MOSI.Write(false);
                CLOCK.Write(false);
            }

            Thread.Sleep(20);
            CS.Write(true);
        }
#endif

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

		private enum Commands : byte
		{
			LS7366_CLEAR = 0x00, // clear register
			LS7366_READ = 0x40, // read register
			LS7366_WRITE = 0x80, // write register
			LS7366_LOAD = 0xC0, // load register
		}

		private enum Registers : byte
		{
			LS7366_MDR0 = 0x08, // select MDR0
			LS7366_MDR1 = 0x10, // select MDR1
			LS7366_DTR = 0x18, // select DTR
			LS7366_CNTR = 0x20, // select CNTR
			LS7366_OTR = 0x28, // select OTR
			LS7366_STR = 0x30, // select STR
		}

		private enum MDR0Mode : byte
		{
			LS7366_MDR0_QUAD0 = 0x00, // none quadrature mode
			LS7366_MDR0_QUAD1 = 0x01, // quadrature x1 mode
			LS7366_MDR0_QUAD2 = 0x02, // quadrature x2 mode
			LS7366_MDR0_QUAD4 = 0x03, // quadrature x4 mode

			LS7366_MDR0_FREER = 0x00, // free run mode
			LS7366_MDR0_SICYC = 0x04, // single cycle count mode
			LS7366_MDR0_RANGE = 0x08, // range limit count mode (0-DTR-0)
			// counting freezes at limits but 
			// resumes on direction reverse
			LS7366_MDR0_MODTR = 0x0C, // modulo-n count (n=DTR both dirs)

			LS7366_MDR0_DIDX = 0x00, // disable index
			LS7366_MDR0_LDCNT = 0x10, // config IDX as load DTR to CNTR
			LS7366_MDR0_RECNT = 0x20, // config IDX as reset CNTR (=0)
			LS7366_MDR0_LDOTR = 0x30, // config IDX as load CNTR to OTR  

			LS7366_MDR0_ASIDX = 0x00, // asynchronous index
			LS7366_MDR0_SYINX = 0x40, // synchronous IDX (if !NQUAD)

			LS7366_MDR0_FFAC1 = 0x00, // filter clock division factor=1
			LS7366_MDR0_FFAC2 = 0x80, // filter clock division factor=2

			LS7366_MDR0_NOFLA = 0x00, // no flags
		}

		private enum CountMode : byte
		{
			NoneQuad = 0x00, // none quadrature mode
			Quad1 = 0x01, // quadrature x1 mode
			Quad2 = 0x02, // quadrature x2 mode
			Quad4 = 0x03, // quadrature x4 mode
		}

		private enum MDR1Mode : byte
		{
			LS7366_MDR1_4BYTE = 0x00, // 4 byte counter mode
			LS7366_MDR1_3BYTE = 0x01, // 3 byte counter mode
			LS7366_MDR1_2BYTE = 0x02, // 2 byte counter mode
			LS7366_MDR1_1BYTE = 0x03, // 1 byte counter mode
			LS7366_MDR1_ENCNT = 0x00, // enable counting
			LS7366_MDR1_DICNT = 0x04, // disable counting
			LS7366_MDR1_FLIDX = 0x20, // FLAG on IDX (index)
			LS7366_MDR1_FLCMP = 0x40, // FLAG on CMP (compare)
			LS7366_MDR1_FLCY = 0x80, // FLAG on CY (carry)
		}
    }
}
