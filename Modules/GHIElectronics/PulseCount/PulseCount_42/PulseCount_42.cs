#define USE_SOFTWARE_SPI

using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.Interfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A PulseCount module for Microsoft .NET Gadgeteer
    /// </summary>
    public class PulseCount : GTM.Module
    {
		private byte[] write1 = new byte[1];
		private byte[] write2 = new byte[2];
		private byte[] read5 = new byte[5];
		private CountMode mode;
		private Socket socket;

#if USE_SOFTWARE_SPI
        private GTI.DigitalInput MISO;
        private GTI.DigitalOutput MOSI;
        private GTI.DigitalOutput CLOCK;
        private GTI.DigitalOutput CS;
#else
        private GTI.SPI.Configuration config;
        private GTI.SPI spi;
#endif

        /// <summary>Constructs a new PulseCount instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public PulseCount(int socketNumber)
        {
            this.socket = Socket.GetSocket(socketNumber, true, this, null);

#if USE_SOFTWARE_SPI
			this.socket.EnsureTypeIsSupported('Y', this);

			this.CS = new GTI.DigitalOutput(this.socket, Socket.Pin.Six, true, this);
			this.MISO = new GTI.DigitalInput(this.socket, Socket.Pin.Eight, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, this);
			this.MOSI = new GTI.DigitalOutput(this.socket, Socket.Pin.Seven, false, this);
			this.CLOCK = new GTI.DigitalOutput(this.socket, Socket.Pin.Nine, false, this);
#else
            socket.EnsureTypeIsSupported('S', this);

            this.config = new GTI.SPI.Configuration(false, 0, 0, false, true, 1000);
			this.spi = new GTI.SPI(socket, this.config, GTI.SPI.Sharing.Shared, socket, GT.Socket.Pin.Six, this);
#endif

			this.Initialize();
        }

		/// <summary>
		/// Creates an input on the input port on  the module.
		/// </summary>
		/// <param name="glitchFilterMode">The glitch filter mode for the input.</param>
		/// <param name="resistorMode">The resistor mode for the input.</param>
		/// <returns>The new input.</returns>
		public GTI.DigitalInput CreateInput(GTI.GlitchFilterMode glitchFilterMode, GTI.ResistorMode resistorMode)
		{
			return new GTI.DigitalInput(this.socket, Socket.Pin.Three, glitchFilterMode, resistorMode, this);
		}

		/// <summary>
		/// Creates an interrrupt input on the input port on  the module.
		/// </summary>
		/// <param name="glitchFilterMode">The glitch filter mode for the input.</param>
		/// <param name="resistorMode">The resistor mode for the input.</param>
		/// <param name="interruptMode">The interrupt mode for the input.</param>
		/// <returns>The new input.</returns>
		public GTI.InterruptInput CreateInterruptInput(GTI.GlitchFilterMode glitchFilterMode, GTI.ResistorMode resistorMode, GTI.InterruptMode interruptMode)
		{
			return new GTI.InterruptInput(this.socket, Socket.Pin.Three, glitchFilterMode, resistorMode, interruptMode, this);
		}

		private void Initialize()
		{
			this.Write((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_MDR0);
			this.Write((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_MDR1);
			this.Write((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_STR);
			this.Write((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_CNTR);
			this.Write((byte)Commands.LS7366_LOAD | (byte)Registers.LS7366_OTR);

			this.Mode = CountMode.Quad1;

			this.Write((byte)Commands.LS7366_WRITE | (byte)Registers.LS7366_MDR1, (byte)MDR1Mode.LS7366_MDR1_4BYTE | (byte)MDR1Mode.LS7366_MDR1_ENCNT);
		}

        /// <summary>
        /// Gets the current count from the module.
        /// </summary>
        /// <returns>The current count on the module.</returns>
        public int GetCount()
		{
			this.Write((byte)Commands.LS7366_LOAD | (byte)Registers.LS7366_OTR);
            return this.Read4((byte)Commands.LS7366_READ | (byte)Registers.LS7366_OTR);
        }

		/// <summary>
		/// Resets the count on the module to 0.
		/// </summary>
		public void ResetCount()
		{
			this.Write((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_CNTR);
		}

		/// <summary>
		/// Sets or gets the count mode the module uses.
		/// </summary>
		public CountMode Mode
		{
			get
			{
				return this.mode;
			}
			set
			{
				if (this.mode == value)
					return;

				this.mode = value;

				byte command = (byte)MDR0Mode.LS7366_MDR0_FREER | (byte)MDR0Mode.LS7366_MDR0_DIDX | (byte)MDR0Mode.LS7366_MDR0_FFAC2;

				switch (this.mode)
				{
					case CountMode.NoneQuad: command |= (byte)MDR0Mode.LS7366_MDR0_QUAD0; break;
					case CountMode.Quad1: command |= (byte)MDR0Mode.LS7366_MDR0_QUAD1; break;
					case CountMode.Quad2: command |= (byte)MDR0Mode.LS7366_MDR0_QUAD2; break;
					case CountMode.Quad4: command |= (byte)MDR0Mode.LS7366_MDR0_QUAD4; break;
				}

				this.Write((byte)Commands.LS7366_WRITE | (byte)Registers.LS7366_MDR0, command);
			}
		}

		private int Read4(byte register)
		{
			write1[0] = register;

#if USE_SOFTWARE_SPI
			SoftwareSPI_WriteRead(write1, read5);
#else
			this.spi.WriteRead(write1, read5);
#endif

			return (read5[1] << 24) + (read5[2] << 16) + (read5[3] << 8) + read5[4];
		}

        private void Write(byte register)
        {
            write1[0] = register;

#if USE_SOFTWARE_SPI
            SoftwareSPI_WriteRead(write1, null);
#else
			this.spi.Write(write1);
#endif
        }

        private void Write(byte register, byte command)
        {
            write2[0] = register;
			write2[1] = command;

#if USE_SOFTWARE_SPI
            SoftwareSPI_WriteRead(write2, null);
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
		/// The possible count modes the modules can use.
		/// </summary>
		public enum CountMode
		{
			/// <summary>
			/// Non-quadrature mode.
			/// </summary>
			NoneQuad,
			/// <summary>
			/// 1x quadrature mode.
			/// </summary>
			Quad1,
			/// <summary>
			/// 2x quadrature mode.
			/// </summary>
			Quad2,
			/// <summary>
			/// 4x quadrature mode.
			/// </summary>
			Quad4
		}

		private enum Commands : byte
		{
			LS7366_CLEAR = 0x00, // clear registerister
			LS7366_READ = 0x40, // read registerister
			LS7366_WRITE = 0x80, // write registerister
			LS7366_LOAD = 0xC0, // load registerister
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

		private enum MDR1Mode : byte
		{
			LS7366_MDR1_4BYTE = 0x00, // 4 byte counter mode
			LS7366_MDR1_3BYTE = 0x01, // 3 byte counter mode
			LS7366_MDR1_2BYTE = 0x02, // 2 byte counter mode
			LS7366_MDR1_1BYTE = 0x03, // 1 byte counter mode
			LS7366_MDR1_ENCNT = 0x00, // enable counting
            LS7366_MDR1_DICNT = 0x04, // disable counting
            LS7366_MDR1_FLIDX = 0x10,
            LS7366_MDR1_FLCMP = 0x20,
            LS7366_MDR1_FLBW = 0x40,
            LS7366_MDR1_FLCY = 0x80,
		}
    }
}
