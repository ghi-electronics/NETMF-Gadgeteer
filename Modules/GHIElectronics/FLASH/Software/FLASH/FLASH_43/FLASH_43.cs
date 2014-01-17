using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;
using System;
using System.Threading;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A FLASH module for Microsoft .NET Gadgeteer
    /// </summary>
    /// <remarks>
    /// Chip Details:
    /// 
    /// <list type="bullet">
    /// <item>64 total blocks</item>
    /// <item>1024 sectors</item>
    /// <item>4Kb for each sector</item>
    /// <item>1024 sectrors * 4kb = 4096Kb total memory</item>
    /// <item>256 bytes mamximum for each transfer</item>
    /// </list>
    /// </remarks>
    public class FLASH : GTM.Module
    {
        // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
        // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2CBus), you must do a few more steps
        // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
        // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
        // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

        ///////////////////////////////////////////////////////////////////////
        // Constants
        ///////////////////////////////////////////////////////////////////////
        private const int MAX_ADDRESS = 0x400000;
        private const byte CMD_GET_IDENTIFICATION = 0x9F;
        private const byte CMD_ERASE_SECTOR = 0x20;
        private const byte CMD_ERASE_BLOCK = 0xD8;
        private const byte CMD_ERASE_CHIP = 0xC7;
        private const byte CMD_WRITE_SECTOR = 0x2;
        private const byte CMD_WRITE_ENABLE = 0x6;
        private const byte CMD_READ_STATUS = 0x5;

        private const int SectoreSize = 4 * 1024;
        private const int BlockSize = 64 * 1024;
        private const int PageSize = 256; // Max is 256 byte for each block transfer

        const byte ID_MANUFACTURE = 0xC2;
        const byte ID_DEVICE_0 = 0x20;
        const byte ID_DEVICE_1 = 0x16;

        const byte DUMMY_BYTE = 0x00;
        ///////////////////////////////////////////////////////////////////////

        GTI.Spi spi;
        GTI.SpiConfiguration spiConfig;
        GTI.DigitalOutput statusLED;

        byte[] writeData;
        byte[] readData;

        /// <summary>Constructor</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public FLASH(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('S', this);

            statusLED = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Five, false, this);

            Initialize(socket);
        }

        private void Initialize(GT.Socket socket)
        {
            spiConfig = new GTI.SpiConfiguration(false, 0, 0, false, true, 4000);
            spi = GTI.SpiFactory.Create(socket, spiConfig, GTI.SpiSharing.Shared, socket, Socket.Pin.Six, this);

            ClearBuffers();
        }

        /// <summary>
        /// Attempts to enable writing to the flash, and returns the result.
        /// </summary>
        /// <returns>True is writing is enabled, false if it is not</returns>
        public bool WriteEnable()
        {
            ClearBuffers();

            writeData[0] = CMD_WRITE_ENABLE;

            statusLED.Write(true);

            spi.Write(writeData);

            writeData[0] = CMD_READ_STATUS;

            spi.WriteRead(writeData, readData);
            
            statusLED.Write(false);

            return ((readData[1] & 0x2) != 0);
        }

        /// <summary>
        /// Returns the ID of the chip.
        /// </summary>
        /// <returns>A byte[] containing the chip ID.</returns>
        public byte[] GetIdentification()
        {
            writeData = new byte[1];
            readData = new byte[4];
            writeData[0] = CMD_GET_IDENTIFICATION;
            
            statusLED.Write(true);

            spi.WriteRead(writeData, readData);

            if ((readData[1] == 0xFF && readData[2] == 0xFF && readData[3] == 0xFF) || (readData[1] == 0 && readData[2] == 0 && readData[3] == 0))
            {
                throw new Exception("Can not initialize flash");
            }

            statusLED.Write(false);

            return readData;
        }
        
        /// <summary>
        /// Returns if a write is in progress.
        /// </summary>
        /// <returns>True is there is a operation write in progress. False is there is no write operation in progress.</returns>
        public bool WriteInProgress()
        {
            writeData = new byte[] { 0 };
            readData = new byte[] { 0, 0 };

            statusLED.Write(true);

            readData[1] = 1;
            writeData[0] = CMD_READ_STATUS;

            spi.WriteRead(writeData, readData);

            statusLED.Write(false);

            return ((readData[1] & 0x1) != 0);
        }

        /// <summary>
        /// Erases the entire chip. Blocking function.
        /// </summary>
        public void EraseChip()
        {
            while (WriteEnable() == false)
                Thread.Sleep(0);

            statusLED.Write(true);

            writeData = new byte[1];
            writeData[0] = CMD_ERASE_CHIP;

            spi.Write(writeData);

            statusLED.Write(false);

            while (WriteInProgress() == true)
                Thread.Sleep(0);
        }

        /// <summary>
        /// Erases blocks, starting at the passed in block, for the passed in number of blocks. Blocking function.
        /// </summary>
        /// <param name="block">The block to begin erasing at.</param>
        /// <param name="num">The number of blocks to erase.</param>
        /// <returns>If the erase was successful.</returns>
        public bool EraseBlock(int block, int num)
        {
            if ((block + num) * BlockSize > MAX_ADDRESS)
            {
                throw new Exception("Invalid params");
            }
         
            int address = block * BlockSize;
            int i = 0;

            statusLED.Write(true);

            for (i = 0; i < num; i++)
            {
                while (WriteEnable() == false)
                    Thread.Sleep(0);

                writeData = new byte[4];
                writeData[0] = CMD_ERASE_BLOCK;
                writeData[1] = (byte)(address >> 16);
                writeData[2] = (byte)(address >> 8);
                writeData[3] = (byte)(address >> 0);

                spi.Write(writeData);
                
                address += BlockSize;

                while (WriteInProgress() == true)
                    Thread.Sleep(0);
            }

            statusLED.Write(false);

            return i == num;
        }

        /// <summary>
        /// Erases sectors, starting at the passed in sector, for the passed in number of sectors. Blocking function.
        /// </summary>
        /// <param name="sector">Sector to begin at.</param>
        /// <param name="num">Number of sectors to erase.</param>
        /// <returns>If the erase was successful.</returns>
        public bool EraseSector(int sector, int num)
        {
            if ((sector + num) * SectoreSize > MAX_ADDRESS)
            {
                throw new Exception("Invalid params");
            }

            int address = sector * SectoreSize;
            int i = 0;

            statusLED.Write(true);

            for (i = 0; i < num; i++)
            {
                while (WriteEnable() == false)
                    Thread.Sleep(0);

                writeData = new byte[4];
                writeData[0] = CMD_ERASE_SECTOR;
                writeData[1] = (byte)(address >> 16);
                writeData[2] = (byte)(address >> 8);
                writeData[3] = (byte)(address >> 0);
                
                spi.Write(writeData);
                
                address += SectoreSize;

                while (WriteInProgress() == true)
                    Thread.Sleep(0);
            }

            statusLED.Write(false);

            return i == num;
        }

        /// <summary>
        /// Writes data to a specific address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="buffer">The data to write.</param>
        /// <returns>If the write was successful.</returns>
        public bool WriteData(int address, byte[] buffer)
        {
            if (buffer.Length + address > MAX_ADDRESS)
            {
                throw new Exception("Invalid params");
            }
           
            int block = buffer.Length / PageSize;
            int length = buffer.Length;
            int i = 0;

            statusLED.Write(true);

            if (block > 0)
            {
                for (i = 0; i < block; i++)
                {
                    while (WriteEnable() == false)
                        Thread.Sleep(0);

                    writeData = new byte[PageSize + 4];
                    writeData[0] = CMD_WRITE_SECTOR;
                    writeData[1] = (byte)(address >> 16);
                    writeData[2] = (byte)(address >> 8);
                    writeData[3] = (byte)(address >> 0);
                    
                    Array.Copy(buffer, i * PageSize, writeData, 4, PageSize);
                    
                    spi.Write(writeData);

                    while (WriteInProgress() == true)
                        Thread.Sleep(0);

                    address += PageSize;
                    length -= PageSize;
                }
            }

            if (length > 0)
            {
                while (WriteEnable() == false)
                    Thread.Sleep(0);

                writeData = new byte[length + 4];
                writeData[0] = CMD_WRITE_SECTOR;
                writeData[1] = (byte)(address >> 16);
                writeData[2] = (byte)(address >> 8);
                writeData[3] = (byte)(address >> 0);
                
                Array.Copy(buffer, i * PageSize, writeData, 4, length);
                
                spi.Write(writeData);

                while (WriteInProgress() == true)
                    Thread.Sleep(0);
                
                address += length;
                length -= length;
            }

            statusLED.Write(false);

            return length == 0;
        }

        /// <summary>
        /// Reads data from a specific address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="length">The length to read.</param>
        /// <returns>The data that was read.</returns>
        public byte[] ReadData(int address, int length)
        {
            if (length + address > MAX_ADDRESS)
            {
                throw new Exception("Invalid params");
            }

            statusLED.Write(true);

            byte[] buffer = new byte[length];

            while (WriteEnable() == false)
                Thread.Sleep(0);
            
            writeData = new byte[4];
            readData = new byte[length + 4];

            writeData[0] = 0x3;
            writeData[1] = (byte)(address >> 16);
            writeData[2] = (byte)(address >> 8);
            writeData[3] = (byte)(address >> 0);
            
            spi.WriteRead(writeData, readData);
            Array.Copy(readData, 4, buffer, 0, length);

            statusLED.Write(false);

            return buffer;
        }
        /// <summary>
        /// Reads data from a specific address in fast mode.
        /// </summary>
        /// <param name="address">Address to read from.</param>
        /// <param name="length">The length to read.</param>
        /// <returns>The data that was read.</returns>
        public byte[] ReadData_FastMode(uint address, int length)
        {
            if (length + address > MAX_ADDRESS)
            {
                throw new Exception("Invalid params");
            }

            statusLED.Write(true);

            byte[] buffer = new byte[length];

            while (WriteEnable() == false)
                Thread.Sleep(0);
            
            writeData = new byte[5];
            readData = new byte[length + 5];

            writeData[0] = 0x3;
            writeData[1] = (byte)(address >> 16);
            writeData[2] = (byte)(address >> 8);
            writeData[3] = (byte)(address >> 0);
            writeData[4] = DUMMY_BYTE;
            
            spi.WriteRead(writeData, readData);
            Array.Copy(readData, 5, buffer, 0, length);

            statusLED.Write(false);

            return buffer;
        }

        private void ClearBuffers()
        {
            writeData = new byte[] { 0 };
            readData = new byte[] { 0, 0 };
        }
    }
}