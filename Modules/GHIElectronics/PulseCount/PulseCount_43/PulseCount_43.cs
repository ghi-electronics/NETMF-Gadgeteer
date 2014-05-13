using System;
using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A PulseCount module for Microsoft .NET Gadgeteer
    /// </summary>
    public class PulseCount : GTM.Module
    {
        private delegate void SPIWriteRead(byte[] writeBuffer, byte[] readBuffer);

		private byte[] write1;
		private byte[] write2;
		private byte[] read5;

        private Socket socket;
        private GTI.DigitalInput miso;
        private GTI.DigitalOutput mosi;
        private GTI.DigitalOutput clock;
        private GTI.DigitalOutput cs;
        private GTI.DigitalOutput enable;
        private GTI.Spi spi;
        private SPIWriteRead spiWriteRead;
        private CountMode mode;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public PulseCount(int socketNumber)
        {
            this.socket = Socket.GetSocket(socketNumber, true, this, null);

            this.write1 = new byte[1];
            this.write2 = new byte[2];
            this.read5 = new byte[5];

            if (this.socket.SupportsType('S'))
            {
                this.spi = GTI.SpiFactory.Create(this.socket, new GTI.SpiConfiguration(false, 0, 0, false, true, 1000), GTI.SpiSharing.Shared, socket, GT.Socket.Pin.Six, this);
                this.spiWriteRead = this.HardwareWriteRead;
            }
            else
            {
                this.socket.EnsureTypeIsSupported('Y', this);

                this.cs = GTI.DigitalOutputFactory.Create(this.socket, Socket.Pin.Six, true, this);
                this.miso = GTI.DigitalInputFactory.Create(this.socket, Socket.Pin.Eight, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, this);
                this.mosi = GTI.DigitalOutputFactory.Create(this.socket, Socket.Pin.Seven, false, this);
                this.clock = GTI.DigitalOutputFactory.Create(this.socket, Socket.Pin.Nine, false, this);

                this.spiWriteRead = this.SoftwareWriteRead;
            }

            this.enable = GTI.DigitalOutputFactory.Create(this.socket, Socket.Pin.Five, true, this);

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
			return GTI.DigitalInputFactory.Create(this.socket, Socket.Pin.Three, glitchFilterMode, resistorMode, this);
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
			return GTI.InterruptInputFactory.Create(this.socket, Socket.Pin.Three, glitchFilterMode, resistorMode, interruptMode, this);
		}

		private void Initialize()
		{
            this.Write(Commands.LS7366_CLEAR | Commands.LS7366_MDR0);
            this.Write(Commands.LS7366_CLEAR | Commands.LS7366_MDR1);
            this.Write(Commands.LS7366_CLEAR | Commands.LS7366_STR);
            this.Write(Commands.LS7366_CLEAR | Commands.LS7366_CNTR);
            this.Write(Commands.LS7366_LOAD | Commands.LS7366_OTR);

			this.Mode = CountMode.Quad1;

            this.Write(Commands.LS7366_WRITE | Commands.LS7366_MDR1, MDR1Modes.LS7366_MDR1_4BYTE | MDR1Modes.LS7366_MDR1_ENCNT);
		}

        /// <summary>
        /// Gets the current count from the module.
        /// </summary>
        /// <returns>The current count on the module.</returns>
        public int GetCount()
		{
            this.Write(Commands.LS7366_LOAD | Commands.LS7366_OTR);
            return this.Read4(Commands.LS7366_READ | Commands.LS7366_OTR);
        }

		/// <summary>
		/// Resets the count on the module to 0.
		/// </summary>
		public void ResetCount()
		{
            this.Write(Commands.LS7366_CLEAR | Commands.LS7366_CNTR);
		}

		/// <summary>
		/// The count mode the module uses.
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

                MDR0Modes command = MDR0Modes.LS7366_MDR0_FREER | MDR0Modes.LS7366_MDR0_DIDX | MDR0Modes.LS7366_MDR0_FFAC2;

				switch (this.mode)
				{
                    case CountMode.NoneQuad: command |= MDR0Modes.LS7366_MDR0_QUAD0; break;
                    case CountMode.Quad1: command |= MDR0Modes.LS7366_MDR0_QUAD1; break;
                    case CountMode.Quad2: command |= MDR0Modes.LS7366_MDR0_QUAD2; break;
                    case CountMode.Quad4: command |= MDR0Modes.LS7366_MDR0_QUAD4; break;
				}

                this.Write(Commands.LS7366_WRITE | Commands.LS7366_MDR0, command);
			}
		}

        private int Read4(Commands register)
        {
            this.write1[0] = (byte)register;

            this.spiWriteRead(this.write1, this.read5);

            return (this.read5[1] << 24) + (this.read5[2] << 16) + (this.read5[3] << 8) + this.read5[4];
        }

        private void Write(Commands register)
        {
            this.write1[0] = (byte)register;

            this.spiWriteRead(this.write1, null);
        }

        private void Write(Commands register, MDR0Modes command)
        {
            this.write2[0] = (byte)register;
            this.write2[1] = (byte)command;

            this.spiWriteRead(this.write2, null);
        }

        private void Write(Commands register, MDR1Modes command)
        {
            this.write2[0] = (byte)register;
            this.write2[1] = (byte)command;

            this.spiWriteRead(this.write2, null);
        }

        private void HardwareWriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            if (readBuffer == null)
                this.spi.Write(writeBuffer);
            else
                this.spi.WriteRead(writeBuffer, readBuffer);
        }

        private void SoftwareWriteRead(byte[] writeBuffer, byte[] readBuffer)
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

        /// <summary>
        /// The possible count modes the module can use.
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
    }
}
