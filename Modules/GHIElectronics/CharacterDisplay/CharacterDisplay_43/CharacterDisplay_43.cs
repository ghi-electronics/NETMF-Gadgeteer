using System.Threading;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A CharacterDisplay module for Microsoft .NET Gadgeteer
    /// </summary>
    public class CharacterDisplay : GTM.Module
    {
        private GTI.DigitalOutput lcdRS;
        private GTI.DigitalOutput lcdE;

        private GTI.DigitalOutput lcdD4;
        private GTI.DigitalOutput lcdD5;
        private GTI.DigitalOutput lcdD6;
        private GTI.DigitalOutput lcdD7;

        private GTI.DigitalOutput backlight;

        private static byte[] ROW_OFFSETS = new byte[4] { 0x00, 0x40, 0x14, 0x54 };
        private const byte DISP_ON = 0x0C;
        private const byte CLR_DISP = 1;
        private const byte CUR_HOME = 2;
        private const byte SET_CURSOR = 0x80;

        private void WriteNibble(byte b)
        {
            this.lcdD7.Write((b & 0x8) != 0);
            this.lcdD6.Write((b & 0x4) != 0);
            this.lcdD5.Write((b & 0x2) != 0);
            this.lcdD4.Write((b & 0x1) != 0);

            this.lcdE.Write(true); 
            this.lcdE.Write(false);

            Thread.Sleep(1);
        }

        private void SendCommand(byte command)
        {
            this.lcdRS.Write(false);

            this.WriteNibble((byte)(command >> 4));
            this.WriteNibble(command);

            Thread.Sleep(2);

            this.lcdRS.Write(true);
        }

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public CharacterDisplay(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('Y', this);

            this.lcdRS = GTI.DigitalOutputFactory.Create(socket, GT.Socket.Pin.Four, false, null);
            this.lcdE = GTI.DigitalOutputFactory.Create(socket, GT.Socket.Pin.Three, false, null);
            this.lcdD4 = GTI.DigitalOutputFactory.Create(socket, GT.Socket.Pin.Five, false, null);
            this.lcdD5 = GTI.DigitalOutputFactory.Create(socket, GT.Socket.Pin.Seven, false, null);
            this.lcdD6 = GTI.DigitalOutputFactory.Create(socket, GT.Socket.Pin.Nine, false, null);
            this.lcdD7 = GTI.DigitalOutputFactory.Create(socket, GT.Socket.Pin.Six, false, null);
            
            this.backlight = GTI.DigitalOutputFactory.Create(socket, GT.Socket.Pin.Eight, true, null);

            this.SendCommand(0x33);
            this.SendCommand(0x32);
            this.SendCommand(CharacterDisplay.DISP_ON);
            this.SendCommand(CharacterDisplay.CLR_DISP);

            Thread.Sleep(3);
        }

        /// <summary>
        /// Prints the passed in string to the screen at the current cursor position.
        /// </summary>
        /// <param name="value">The string to print.</param>
        public void Print(string value)
        {
            for (int i = 0; i < value.Length; i++)
                this.Print(value[i]);
        }

        /// <summary>
        /// Prints a character to the screen at the current cursor position.
        /// </summary>
        /// <param name="value">The character to display.</param>
        public void Print(char value)
        {
            this.WriteNibble((byte)(value >> 4));
            this.WriteNibble((byte)value);
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        public void Clear()
        {
            this.SendCommand(CharacterDisplay.CLR_DISP);
            
            Thread.Sleep(2);
        }

        /// <summary>
        /// Places the cursor at the top left of the screen.
        /// </summary>
        public void CursorHome()
        {
            this.SendCommand(CharacterDisplay.CUR_HOME);
            
            Thread.Sleep(2);
        }

        /// <summary>
        /// Moves the cursor to given position.
        /// </summary>
        /// <param name="row">The new row.</param>
        /// <param name="column">The new column.</param>
        public void SetCursorPosition(byte row, byte column)
        {
            this.SendCommand((byte)(CharacterDisplay.SET_CURSOR | CharacterDisplay.ROW_OFFSETS[row] | column));
        }

        /// <summary>
        /// Whether or not the backlight is enabled.
        /// </summary>
        public bool BacklightEnabled
        {
            get
            {
                return this.backlight.Read();
            }
            set
            {
                this.backlight.Write(value);
            }
        }
    }
}
