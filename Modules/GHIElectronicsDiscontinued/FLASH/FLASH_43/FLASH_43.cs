using System;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A FLASH module for Microsoft .NET Gadgeteer
    /// </summary>
    public class FLASH : GTM.Module
    {
        private const int MAX_ADDRESS = 0x400000;
        private const byte CMD_GET_IDENTIFICATION = 0x9F;
        private const byte CMD_ERASE_SECTOR = 0x20;
        private const byte CMD_ERASE_BLOCK = 0xD8;
        private const byte CMD_ERASE_CHIP = 0xC7;
        private const byte CMD_WRITE_SECTOR = 0x2;
        private const byte CMD_WRITE_ENABLE = 0x6;
        private const byte CMD_READ_STATUS = 0x5;

        private const int SECTOR_SIZE = 4 * 1024;
        private const int BLOCK_SIZE = 64 * 1024;
        private const int PAGE_SIZE = 256;

        private const byte ID_MANUFACTURER = 0xC2;
        private const byte ID_DEVICE_0 = 0x20;
        private const byte ID_DEVICE_1 = 0x16;

        private GTI.Spi spi;
        private GTI.DigitalOutput statusLED;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public FLASH(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('S', this);

            this.statusLED = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Five, false, this);
            this.spi = GTI.SpiFactory.Create(socket, new GTI.SpiConfiguration(false, 0, 0, false, true, 4000), GTI.SpiSharing.Shared, socket, Socket.Pin.Six, this);
        }

        /// <summary>
        /// Attempts to enable writing to the flash.
        /// </summary>
        /// <returns>If writing is enabled or not.</returns>
        public bool WriteEnable()
        {
            var writeData = new byte[1];
            var readData = new byte[2];

            this.statusLED.Write(true);

            writeData[0] = FLASH.CMD_WRITE_ENABLE;
            this.spi.Write(writeData);

            writeData[0] = FLASH.CMD_READ_STATUS;
            this.spi.WriteRead(writeData, readData);
            
            this.statusLED.Write(false);

            return ((readData[1] & 0x2) != 0);
        }

        /// <summary>
        /// Gets the ID of the chip.
        /// </summary>
        /// <returns>The chip ID.</returns>
        public byte[] GetIdentification()
        {
            var writeData = new byte[1];
            var readData = new byte[4];
            
            this.statusLED.Write(true);

            writeData[0] = FLASH.CMD_GET_IDENTIFICATION;
            this.spi.WriteRead(writeData, readData);

            if ((readData[1] == 0xFF && readData[2] == 0xFF && readData[3] == 0xFF) || (readData[1] == 0x00 && readData[2] == 0x00 && readData[3] == 0x00))
            {
                throw new Exception("The module could not initialize.");
            }

            this.statusLED.Write(false);

            return readData;
        }
        
        /// <summary>
        /// Checks if a write is in progress.
        /// </summary>
        /// <returns>Whether or not a write is in progress.</returns>
        public bool IsWriteInProgress()
        {
            var writeData = new byte[1];
            var readData = new byte[2];
            
            this.statusLED.Write(true);

            readData[1] = 1;
            writeData[0] = FLASH.CMD_READ_STATUS;

            this.spi.WriteRead(writeData, readData);

            this.statusLED.Write(false);

            return ((readData[1] & 0x1) != 0);
        }

        /// <summary>
        /// Erases the entire chip.
        /// </summary>
        public void EraseChip()
        {
            while (!this.WriteEnable())
                Thread.Sleep(1);

            this.statusLED.Write(true);

            var writeData = new byte[1];

            writeData[0] = FLASH.CMD_ERASE_CHIP;
            this.spi.Write(writeData);

            this.statusLED.Write(false);

            while (this.IsWriteInProgress())
                Thread.Sleep(1);
        }

        /// <summary>
        /// Erases the specified blocks.
        /// </summary>
        /// <param name="block">The block to begin erasing at.</param>
        /// <param name="count">The number of blocks to erase.</param>
        public void EraseBlock(int block, int count)
        {
            if (block < 0) throw new ArgumentOutOfRangeException("block", "block must not be negative.");
            if (count <= 0) throw new ArgumentOutOfRangeException("count", "count must be positive.");
            if ((block + count) * FLASH.BLOCK_SIZE > FLASH.MAX_ADDRESS) throw new ArgumentOutOfRangeException("block", "block + count must be less than the total number of blocks.");

            int address = block * FLASH.BLOCK_SIZE;
            int i = 0;

            this.statusLED.Write(true);

            var writeData = new byte[4];
            for (i = 0; i < count; i++)
            {
                while (!this.WriteEnable())
                    Thread.Sleep(1);

                writeData[0] = FLASH.CMD_ERASE_BLOCK;
                writeData[1] = (byte)(address >> 16);
                writeData[2] = (byte)(address >> 8);
                writeData[3] = (byte)(address >> 0);

                this.spi.Write(writeData);

                address += FLASH.BLOCK_SIZE;

                while (this.IsWriteInProgress())
                    Thread.Sleep(1);
            }

            this.statusLED.Write(false);
        }

        /// <summary>
        /// Erases the specified sectors.
        /// </summary>
        /// <param name="sector">The sector to begin erasing at.</param>
        /// <param name="count">The number of sectors to erase.</param>
        public void EraseSector(int sector, int count)
        {
            if (sector < 0) throw new ArgumentOutOfRangeException("sector", "sector must not be negative.");
            if (count <= 0) throw new ArgumentOutOfRangeException("count", "count must be positive.");
            if ((sector + count) * FLASH.SECTOR_SIZE > FLASH.MAX_ADDRESS) throw new ArgumentOutOfRangeException("sector", "sector + count must be less than the total number of blocks.");

            int address = sector * FLASH.SECTOR_SIZE;
            int i = 0;

            this.statusLED.Write(true);

            var writeData = new byte[4];
            for (i = 0; i < count; i++)
            {
                while (!this.WriteEnable())
                    Thread.Sleep(1);

                writeData[0] = FLASH.CMD_ERASE_SECTOR;
                writeData[1] = (byte)(address >> 16);
                writeData[2] = (byte)(address >> 8);
                writeData[3] = (byte)(address >> 0);

                this.spi.Write(writeData);

                address += FLASH.SECTOR_SIZE;

                while (this.IsWriteInProgress())
                    Thread.Sleep(1);
            }

            this.statusLED.Write(false);
        }

        /// <summary>
        /// Writes data to a specific address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="buffer">The data to write.</param>
        public void Write(int address, byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

            this.Write(address, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes data to a specific address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="buffer">The data to write.</param>
        /// <param name="offset">The offset to begin writing from.</param>
        /// <param name="count">The nubmer of bytes to write.</param>
        public void Write(int address, byte[] buffer, int offset, int count)
        {
            if (address < 0) throw new ArgumentOutOfRangeException("address", "address must not be negative.");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must not be negative.");
            if (count <= 0) throw new ArgumentOutOfRangeException("count", "count must be positive.");
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (count + address > FLASH.MAX_ADDRESS) throw new ArgumentOutOfRangeException("address", "address + buffer.Length must be less than the total number of blocks.");

            int block = count / FLASH.PAGE_SIZE;
            int length = count;
            int i = 0;

            this.statusLED.Write(true);

            if (block > 0)
            {
                var writeData = new byte[FLASH.PAGE_SIZE + 4];
                for (i = 0; i < block; i++)
                {
                    while (!this.WriteEnable())
                        Thread.Sleep(1);

                    writeData[0] = FLASH.CMD_WRITE_SECTOR;
                    writeData[1] = (byte)(address >> 16);
                    writeData[2] = (byte)(address >> 8);
                    writeData[3] = (byte)(address >> 0);

                    Array.Copy(buffer, i * FLASH.PAGE_SIZE + offset, writeData, 4, FLASH.PAGE_SIZE);

                    this.spi.Write(writeData);

                    while (this.IsWriteInProgress())
                        Thread.Sleep(1);

                    address += FLASH.PAGE_SIZE;
                    length -= FLASH.PAGE_SIZE;
                }
            }

            if (length > 0)
            {
                var writeData = new byte[length + 4];
                while (!this.WriteEnable())
                    Thread.Sleep(1);

                writeData[0] = FLASH.CMD_WRITE_SECTOR;
                writeData[1] = (byte)(address >> 16);
                writeData[2] = (byte)(address >> 8);
                writeData[3] = (byte)(address >> 0);

                Array.Copy(buffer, i * FLASH.PAGE_SIZE + offset, writeData, 4, length);

                this.spi.Write(writeData);

                while (this.IsWriteInProgress())
                    Thread.Sleep(1);
                
                address += length;
                length -= length;
            }

            this.statusLED.Write(false);
        }

        /// <summary>
        /// Reads data from the specific address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The data that was read.</returns>
        public byte[] Read(int address, int length)
        {
            if (address < 0) throw new ArgumentOutOfRangeException("address", "address must not be negative.");
            if (length <= 0) throw new ArgumentOutOfRangeException("length", "length must be positive.");
            if (length + address > FLASH.MAX_ADDRESS) throw new ArgumentOutOfRangeException("address", "address + length must be less than the total number of blocks.");

            this.statusLED.Write(true);

            byte[] buffer = new byte[length];

            while (!this.WriteEnable())
                Thread.Sleep(1);
            
            var writeData = new byte[4];
            var readData = new byte[length + 4];

            writeData[0] = 0x03;
            writeData[1] = (byte)(address >> 16);
            writeData[2] = (byte)(address >> 8);
            writeData[3] = (byte)(address >> 0);

            this.spi.WriteRead(writeData, readData);
            Array.Copy(readData, 4, buffer, 0, length);

            this.statusLED.Write(false);

            return buffer;
        }

        /// <summary>
        /// Reads data from a specific address in fast mode.
        /// </summary>
        /// <param name="address">Address to read from.</param>
        /// <param name="length">The length to read.</param>
        /// <returns>The data that was read.</returns>
        public byte[] ReadFast(uint address, int length)
        {
            if (address < 0) throw new ArgumentOutOfRangeException("address", "address must not be negative.");
            if (length <= 0) throw new ArgumentOutOfRangeException("length", "length must be positive.");
            if (length + address > FLASH.MAX_ADDRESS) throw new ArgumentOutOfRangeException("address", "address + length must be less than the total number of blocks.");

            this.statusLED.Write(true);

            byte[] buffer = new byte[length];

            while (!this.WriteEnable())
                Thread.Sleep(1);
            
            var writeData = new byte[5];
            var readData = new byte[length + 5];

            writeData[0] = 0x03;
            writeData[1] = (byte)(address >> 16);
            writeData[2] = (byte)(address >> 8);
            writeData[3] = (byte)(address >> 0);
            writeData[4] = 0x00;

            this.spi.WriteRead(writeData, readData);
            Array.Copy(readData, 5, buffer, 0, length);

            this.statusLED.Write(false);

            return buffer;
        }
    }
}