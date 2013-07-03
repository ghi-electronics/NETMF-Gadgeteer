//#define USE_SOFTWARE_SPI

using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;
using System.Threading;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2C), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// A PulseCount module for Microsoft .NET Gadgeteer
    /// </summary>
    public class PulseCount : GTM.Module
    {
        byte LS7366_1B_rd; // write byte lenght = 1

#if USE_SOFTWARE_SPI

        private GTI.DigitalInput MISO;
        private GTI.DigitalOutput MOSI;
        private GTI.DigitalOutput CLOCK;
        private GTI.DigitalOutput CS;
#else
        private readonly GTI.SPI.Configuration _spiConfig1;
        private readonly GTI.SPI _spi1;
#endif

        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public PulseCount(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
#if USE_SOFTWARE_SPI
            socket.EnsureTypeIsSupported('Y', this);

            CS = new GTI.DigitalOutput(socket, Socket.Pin.Six, true, this);
            MISO = new GTI.DigitalInput(socket, Socket.Pin.Eight, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, this);
            MOSI = new GTI.DigitalOutput(socket, Socket.Pin.Seven, false, this);
            CLOCK = new GTI.DigitalOutput(socket, Socket.Pin.Nine, false, this);

#else
            socket.EnsureTypeIsSupported('S', this);

            _spiConfig1 = new GTI.SPI.Configuration(false, 0, 0, false, true, 1000);
            _spi1 = new GTI.SPI(socket, _spiConfig1, GTI.SPI.Sharing.Shared, socket, GT.Socket.Pin.Six, this);
#endif
            #region Clear registers
            // Clear MDR0 register
            Write1Byte((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_MDR0);

            // Clear MDR1 register
            Write1Byte((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_MDR1);

            // Clear STR register
            Write1Byte((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_STR);

            // Clear CNTR
            Write1Byte((byte)Commands.LS7366_CLEAR | (byte)Registers.LS7366_CNTR);

            // Clear ORT (write CNTR into OTR)
            Write1Byte((byte)Commands.LS7366_LOAD | (byte)Registers.LS7366_OTR);
            #endregion Clear registers

            #region Configure MDR0 and MDR1 registers
            // Configure MDR0 register
            Write2Bytes((byte)Commands.LS7366_WRITE        // write command
                             | (byte)Registers.LS7366_MDR0,       // to MDR0
                               (byte)MDR0Mode.LS7366_MDR0_QUAD1   // none quadrature mode
                             | (byte)MDR0Mode.LS7366_MDR0_FREER   // modulo-n counting 
                             | (byte)MDR0Mode.LS7366_MDR0_DIDX
                //| (byte)MDR0Mode.LS7366_MDR0_LDOTR
                             | (byte)MDR0Mode.LS7366_MDR0_FFAC2);

            // Configure MDR1 register
            Write2Bytes((byte)Commands.LS7366_WRITE        // write command
                             | (byte)Registers.LS7366_MDR1,       // to MDR1
                               (byte)MDR1Mode.LS7366_MDR1_2BYTE   // 2 byte counter mode
                             | (byte)MDR1Mode.LS7366_MDR1_ENCNT);   // enable counting
            #endregion Configure MDR0 and MDR1 registers

            // Set the count mode to 1, for rotary encoder.
            Write2Bytes((byte)Commands.LS7366_WRITE | (byte)Registers.LS7366_MDR0, (byte)CountMode.NoneQuad);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ReadEncoders()
        {
            //LS7366_1B_rd = Return1Byte((byte)Commands.LS7366_LOAD | (byte)Registers.LS7366_OTR);
            //int retVal = Return2Bytes((byte)Commands.LS7366_READ | (byte)Registers.LS7366_OTR);

            //return retVal;

            LS7366_1B_rd = Return1Byte((byte)Commands.LS7366_READ | (byte)Registers.LS7366_STR);
            return Return2Bytes((byte)Commands.LS7366_READ | (byte)Registers.LS7366_CNTR);

        }

        // Return one byte from a register
        private byte Return1Byte(byte reg)
        {
            byte b = new byte();
            LS7366_1B_wr[0] = reg;

#if USE_SOFTWARE_SPI

            SoftwareSPI_WriteRead(LS7366_1B_wr, LS7366_2B_rd);
#else
            _spi1.WriteRead(LS7366_1B_wr, LS7366_2B_rd);
#endif
            b = LS7366_2B_rd[1];

            return b;
        }

        // Return integer in 2 byte mode
        private int Return2Bytes(byte reg)
        {
            int result = 0;
            LS7366_1B_wr[0] = reg;

#if USE_SOFTWARE_SPI
            SoftwareSPI_WriteRead(LS7366_1B_wr, LS7366_4B_rd);
#else
            _spi1.WriteRead(LS7366_1B_wr, LS7366_4B_rd);
#endif
            result = (LS7366_4B_rd[1] * 256) + LS7366_4B_rd[2];

            return result;
        }

        // Write one byte to register
        private void Write1Byte(byte reg)
        {
            LS7366_1B_wr[0] = reg;

#if USE_SOFTWARE_SPI
            SoftwareSPI_WriteRead(LS7366_1B_wr, null);
#else
            _spi1.Write(LS7366_1B_wr);
#endif
        }

        // Write two bytes to a register
        private void Write2Bytes(byte reg, byte cmd)
        {
            LS7366_2B_wr[0] = reg;
            LS7366_2B_wr[1] = cmd;
#if USE_SOFTWARE_SPI
            SoftwareSPI_WriteRead(LS7366_2B_wr, null);
#else
            _spi1.Write(LS7366_2B_wr);
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

        #region Registers, commands and modes

        // LS7366 Commands
        public enum Commands : byte
        {
            LS7366_CLEAR = 0x00, // clear register
            LS7366_READ = 0x40, // read register
            LS7366_WRITE = 0x80, // ,write register
            LS7366_LOAD = 0xC0, // load register
        }

        // LS7366 Registers
        public enum Registers : byte
        {
            LS7366_MDR0 = 0x08, // select MDR0
            LS7366_MDR1 = 0x10, // select MDR1
            LS7366_DTR = 0x18, // select DTR
            LS7366_CNTR = 0x20, // select CNTR
            LS7366_OTR = 0x28, // select OTR
            LS7366_STR = 0x30, // select STR
        }

        // LS7366 MDR0 Counter modes
        public enum MDR0Mode : byte
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

        /// <summary>
        /// Set the count mode
        /// </summary>
        public enum CountMode : byte
        {
            NoneQuad = 0x00, // none quadrature mode
            Quad1 = 0x01, // quadrature x1 mode
            Quad2 = 0x02, // quadrature x2 mode
            Quad4 = 0x03, // quadrature x4 mode
        }

        // LS7366 MDR1 counter modes
        public enum MDR1Mode : byte
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
        #endregion Registers, commands and modes

        #region Buffers
        // Read/write buffers
        byte[] LS7366_1B_wr = new byte[1]; // write one byte
        byte[] LS7366_2B_wr = new byte[2]; // write two bytes
        byte[] LS7366_3B_wr = new byte[5]; // write three bytes
        byte[] LS7366_2B_rd = new byte[2]; // read two bytes
        byte[] LS7366_4B_rd = new byte[4]; // read four bytes
        #endregion Buffers
    }
}
