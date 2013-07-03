using System;
using System.Threading;

using GTI = Gadgeteer.Interfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// An FM Radio module for Microsoft .NET Gadgeteer
    /// </summary>
    public class FMRadio : GTM.Module
    {
        private int currentVolume;
        private bool radioTextWorkerRunning = true;
        private Thread radioTextWorkerThread;
        private string currentRadioText = "N/A";
        private GTI.SoftwareI2C i2cBus;
        private GTI.DigitalOutput resetPin;

        private ushort[] registers = new ushort[16];

        private const byte I2C_ADDRESS = 0x10;

        private const byte REGISTER_DEVICEID = 0x00;
        private const byte REGISTER_CHIPID = 0x01;
        private const byte REGISTER_POWERCFG = 0x02;
        private const byte REGISTER_CHANNEL = 0x03;
        private const byte REGISTER_SYSCONFIG1 = 0x04;
        private const byte REGISTER_SYSCONFIG2 = 0x05;
        private const byte REGISTER_STATUSRSSI = 0x0A;
        private const byte REGISTER_READCHAN = 0x0B;
        private const byte REGISTER_RDSA = 0x0C;
        private const byte REGISTER_RDSB = 0x0D;
        private const byte REGISTER_RDSC = 0x0E;
        private const byte REGISTER_RDSD = 0x0F;

        //Register 0x02 - POWERCFG
        private const byte BIT_SMUTE = 15;
        private const byte BIT_DMUTE = 14;
        private const byte BIT_SKMODE = 10;
        private const byte BIT_SEEKUP = 9;
        private const byte BIT_SEEK = 8;

        //Register 0x03 - CHANNEL
        private const byte BIT_TUNE = 15;

        //Register 0x04 - SYSCONFIG1
        private const byte BIT_RDS = 12;
        private const byte BIT_DE = 11;

        //Register 0x05 - SYSCONFIG2
        private const byte BIT_SPACE1 = 5;
        private const byte BIT_SPACE0 = 4;

        //Register 0x0A - STATUSRSSI
        private const byte BIT_RDSR = 15;
        private const byte BIT_STC = 14;
        private const byte BIT_SFBL = 13;
        private const byte BIT_AFCRL = 12;
        private const byte BIT_RDSS = 11;
        private const byte BIT_STEREO = 8;

        /// <summary>
        /// The maximum Channel the radio and be tuned to.
        /// </summary>
        public const double MAX_CHANNEL = 107.5;

        /// <summary>
        /// The minimum Channel the radio can be tunnel to.
        /// </summary>
        public const double MIN_CHANNEL = 87.5;

        /// <summary>
        /// The Channel returned by <see cref="Seek"/> when no Channel is found.
        /// </summary>
        public const double INVALID_CHANNEL = -1.0;

        /// <summary>
        /// The minimum Volume the device can output.
        /// </summary>
        public const int MIN_VOLUME = 0;

        /// <summary>
        /// The maximum Volume the device can output.
        /// </summary>
        public const int MAX_VOLUME = 15;

        /// <summary>An FM radio module for Microsoft .NET Gadgeteer</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public FMRadio(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported(new char[] { 'Y' }, this);

            this.resetPin = new GTI.DigitalOutput(socket, Socket.Pin.Five, false, this);
            this.i2cBus = new GTI.SoftwareI2C(socket, Socket.Pin.Eight, Socket.Pin.Nine, this);

            this.InitializeDevice();

            this.Channel = FMRadio.MIN_CHANNEL;
            this.Volume = FMRadio.MIN_VOLUME;

            this.radioTextWorkerThread = new Thread(this.RadioTextWorker);
            this.radioTextWorkerThread.Start();
        }

        /// <summary>
        /// The enumeration that determines which direction to Seek when calling Seek(direction);
        /// </summary>
        public enum SeekDirection
        {
            /// <summary>
            /// Seeks for a higher station number.
            /// </summary>
            Forward,

            /// <summary>
            /// Seeks for a lower station number.
            /// </summary>
            Backward
        };

        /// <summary>
        /// Tells the radio to Seek in the given direction until it finds a station.
        /// </summary>
        /// <param name="direction">The direction to Seek the radio.</param>
        /// <remarks>It does wrap around when seeking.</remarks>
        /// <returns>The Channel that was tuned to or <see cref="INVALID_CHANNEL"/> if no Channel was found.</returns>
        public double Seek(SeekDirection direction)
        {
            this.currentRadioText = "N/A";

            if (this.SeekDevice(direction))
                return this.Channel;
            else
                return FMRadio.INVALID_CHANNEL;
        }

        /// <summary>
        /// Increases the Volume by one.
        /// </summary>
        public void IncreaseVolume()
        {
            ++this.Volume;
        }

        /// <summary>
        /// Decreases the Volume by one.
        /// </summary>
        public void DecreaseVolume()
        {
            --this.Volume;
        }

        /// <summary>
        /// Gets or sets the Volume of the radio.
        /// </summary>
        public int Volume
        {
            get
            {
                return this.currentVolume;
            }
            set
            {
                if (value > FMRadio.MAX_VOLUME || value < FMRadio.MIN_VOLUME) return; // throw new ArgumentOutOfRangeException("value", "The Volume provided was outside the allowed range.");

                this.currentVolume = value;
                this.SetDeviceVolume((ushort)value);
            }
        }

        /// <summary>
        /// Gets or sets the Channel of the radio.
        /// </summary>
        public double Channel
        {
            get
            {
                return this.GetDeviceChannel() / 10.0;
            }
            set
            {
                if (value > FMRadio.MAX_CHANNEL || value < FMRadio.MIN_CHANNEL) throw new ArgumentOutOfRangeException("value", "The Channel provided was outside the allowed range.");

                this.SetDeviceChannel((int)(value * 10));
                this.currentRadioText = "N/A";
            }
        }

        /// <summary>
        /// Gets the current Radio Text.
        /// </summary>
        public string RadioText
        {
            get
            {
                return this.currentRadioText;
            }
        }

        /// <summary>
        /// Represents the delegate that is used to handle the <see cref="RadioTextChanged"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="FMRadio"/> that raised the event.</param>
        /// <param name="newRadioText">The new Radio Text.</param>
        public delegate void RadioTextChangedHandler(FMRadio sender, string newRadioText);

        /// <summary>
        /// Raised when new Radio Text is available.
        /// </summary>
        public event RadioTextChangedHandler RadioTextChanged;

        private RadioTextChangedHandler radioTextChangedHandler;

        private void OnRadioTextChanged(FMRadio sender, string newRadioText)
        {
            if (this.radioTextChangedHandler == null)
            {
                this.radioTextChangedHandler = new RadioTextChangedHandler(this.OnRadioTextChanged);
            }

            if (Program.CheckAndInvoke(this.RadioTextChanged, this.radioTextChangedHandler, sender, newRadioText))
            {
                this.RadioTextChanged(sender, newRadioText);
            }
        }

        private void RadioTextWorker()
        {
            const int RADIO_TEXT_GROUP_CODE = 2;
            const int TOGGLE_FLAG_POSITION = 5;
            const int CHARS_PER_SEGMENT = 2;

            const int MAX_MESSAGE_LENGTH = 64;
            const int MAX_SEGMENTS = 16;
            const int MAX_CHARS_PER_GROUP = 4;

            const int VERSION_A_TEXT_SEGMENT_PER_GROUP = 2;
            const int VERSION_B_TEXT_SEGMENT_PER_GROUP = 1;

            char[] currentRadioText = new char[MAX_MESSAGE_LENGTH];
            bool[] isSegmentPresent = new bool[MAX_SEGMENTS];
            int endOfMessage = -1;
            int endSegmentAddress = -1;
            string lastMessage = "";
            int lastTextToggleFlag = -1;
            bool waitForNextMessage = false;

            while (this.radioTextWorkerRunning)
            {
                this.ReadRegisters();
                ushort a = this.registers[FMRadio.REGISTER_RDSA];
                ushort b = this.registers[FMRadio.REGISTER_RDSB];
                ushort c = this.registers[FMRadio.REGISTER_RDSC];
                ushort d = this.registers[FMRadio.REGISTER_RDSD];
                bool ready = (this.registers[FMRadio.REGISTER_STATUSRSSI] & (1 << BIT_RDSR)) != 0;

                if (ready)
                {
                    int programIDCode = a;
                    int groupTypeCode = b >> 12;
                    int versionCode = (b >> 11) & 0x1;
                    int trafficIDCode = (b >> 10) & 0x1;
                    int programTypeCode = (b >> 5) & 0x1F;

                    if (groupTypeCode == RADIO_TEXT_GROUP_CODE)
                    {
                        int textToggleFlag = b & (1 << (TOGGLE_FLAG_POSITION - 1));
                        if (textToggleFlag != lastTextToggleFlag)
                        {
                            currentRadioText = new char[MAX_MESSAGE_LENGTH];
                            lastTextToggleFlag = textToggleFlag;
                            waitForNextMessage = false;
                        }
                        else if (waitForNextMessage)
                        {
                            continue;
                        }

                        int segmentAddress = (b & 0xF);
                        int textAddress = -1;
                        isSegmentPresent[segmentAddress] = true;

                        if (versionCode == 0)
                        {
                            textAddress = segmentAddress * CHARS_PER_SEGMENT * VERSION_A_TEXT_SEGMENT_PER_GROUP;
                            currentRadioText[textAddress] = (char)(c >> 8);
                            currentRadioText[textAddress + 1] = (char)(c & 0xFF);
                            currentRadioText[textAddress + 2] = (char)(d >> 8);
                            currentRadioText[textAddress + 3] = (char)(d & 0xFF);
                        }
                        else
                        {
                            textAddress = segmentAddress * CHARS_PER_SEGMENT * VERSION_B_TEXT_SEGMENT_PER_GROUP;
                            currentRadioText[textAddress] = (char)(d >> 8);
                            currentRadioText[textAddress + 1] = (char)(d & 0xFF);
                        }


                        if (endOfMessage == -1)
                        {
                            for (int i = 0; i < MAX_CHARS_PER_GROUP; ++i)
                            {
                                if (currentRadioText[textAddress + i] == 0xD)
                                {
                                    endOfMessage = textAddress + i;
                                    endSegmentAddress = segmentAddress;
                                }
                            }
                        }

                        if (endOfMessage == -1)
                            continue;

                        bool complete = true;
                        for (int i = 0; i < endSegmentAddress; ++i)
                            if (!isSegmentPresent[i])
                                complete = false;

                        if (!complete)
                            continue;

                        string message = new string(currentRadioText, 0, endOfMessage);
                        if (message == lastMessage)
                        {
                            this.currentRadioText = message;
                            this.OnRadioTextChanged(this, message);
                            waitForNextMessage = true;

                            for (int i = 0; i < endSegmentAddress; ++i)
                                isSegmentPresent[i] = false;

                            endOfMessage = -1;
                            endSegmentAddress = -1;
                        }

                        lastMessage = message;
                    }

                    Thread.Sleep(35);
                }
                else
                {
                    Thread.Sleep(40);
                }
            }
        }

        private void InitializeDevice()
        {
            this.resetPin.Write(true);

            this.ReadRegisters();
            this.registers[0x07] = 0x8100; //Enable the oscillator
            this.UpdateRegisters();

            Thread.Sleep(500); //Wait for clock to settle - from AN230 page 9

            this.ReadRegisters();
            this.registers[FMRadio.REGISTER_POWERCFG] = 0x4001; //Enable the IC
            this.registers[FMRadio.REGISTER_SYSCONFIG1] |= (1 << FMRadio.BIT_RDS); //Enable RDS
            this.registers[FMRadio.REGISTER_SYSCONFIG2] &= 0xFFCF; //Force 200kHz Channel spacing for USA
            this.registers[FMRadio.REGISTER_SYSCONFIG2] &= 0xFFF0; //Clear Volume bits
            this.registers[FMRadio.REGISTER_SYSCONFIG2] |= 0x000F; //Set Volume to lowest
            this.UpdateRegisters();

            Thread.Sleep(110); //Max powerup time, from datasheet page 13
        }

        private void ReadRegisters()
        {
            byte[] data = new byte[32];

            this.i2cBus.Read(FMRadio.I2C_ADDRESS, data, GTI.SoftwareI2C.LengthErrorBehavior.ThrowException);

            for (int i = 0, x = 0xA; i < 12; i += 2, ++x)
                this.registers[x] = (ushort)((data[i] << 8) | (data[i + 1]));

            for (int i = 12, x = 0x0; i < 32; i += 2, ++x)
                this.registers[x] = (ushort)((data[i] << 8) | (data[i + 1]));
        }

        private void UpdateRegisters()
        {
            byte[] data = new byte[12];

            for (int x = 0x02, i = 0; x < 0x08; ++x, i += 2)
            {
                data[i] = (byte)(this.registers[x] >> 8);
                data[i + 1] = (byte)(this.registers[x] & 0x00FF);
            }

            this.i2cBus.Write(FMRadio.I2C_ADDRESS, data, GTI.SoftwareI2C.LengthErrorBehavior.ThrowException);
        }

        private void SetDeviceVolume(ushort Volume)
        {
            this.ReadRegisters();
            this.registers[FMRadio.REGISTER_SYSCONFIG2] &= 0xFFF0; //Clear Volume bits
            this.registers[FMRadio.REGISTER_SYSCONFIG2] |= Volume; //Set Volume to lowest
            this.UpdateRegisters();
        }

        private int GetDeviceChannel()
        {
            this.ReadRegisters();

            int Channel = this.registers[FMRadio.REGISTER_READCHAN] & 0x03FF;

            return Channel * 2 + 875;
        }

        private void SetDeviceChannel(int newChannel)
        {
            newChannel -= 875;
            newChannel /= 2;

            this.ReadRegisters();
            this.registers[FMRadio.REGISTER_CHANNEL] &= 0xFE00; //Clear out the Channel bits
            this.registers[FMRadio.REGISTER_CHANNEL] |= (ushort)newChannel; //Mask in the new Channel
            this.registers[FMRadio.REGISTER_CHANNEL] |= (1 << BIT_TUNE); //Set the TUNE bit to start
            this.UpdateRegisters();

            //Poll to see if STC is set
            while (true)
            {
                this.ReadRegisters();
                if ((this.registers[FMRadio.REGISTER_STATUSRSSI] & (1 << BIT_STC)) != 0)
                    break; //Tuning complete!
            }

            this.ReadRegisters();
            this.registers[FMRadio.REGISTER_CHANNEL] &= 0x7FFF; //Clear the tune after a tune has completed
            this.UpdateRegisters();

            //Wait for the si4703 to clear the STC as well
            while (true)
            {
                this.ReadRegisters();
                if ((this.registers[FMRadio.REGISTER_STATUSRSSI] & (1 << BIT_STC)) == 0)
                    break; //Tuning complete!
            }
        }

        private bool SeekDevice(SeekDirection direction)
        {
            this.ReadRegisters();

            //Set Seek mode wrap bit
            this.registers[FMRadio.REGISTER_POWERCFG] &= 0xFBFF;


            if (direction == SeekDirection.Backward)
                this.registers[FMRadio.REGISTER_POWERCFG] &= 0xFDFF; //Seek down is the default upon reset
            else
                this.registers[FMRadio.REGISTER_POWERCFG] |= 1 << FMRadio.BIT_SEEKUP; //Set the bit to Seek up


            this.registers[FMRadio.REGISTER_POWERCFG] |= (1 << FMRadio.BIT_SEEK); //Start Seek
            this.UpdateRegisters();

            //Poll to see if STC is set
            while (true)
            {
                this.ReadRegisters();
                if ((this.registers[FMRadio.REGISTER_STATUSRSSI] & (1 << FMRadio.BIT_STC)) != 0)
                    break;
            }

            this.ReadRegisters();
            int valueSFBL = this.registers[FMRadio.REGISTER_STATUSRSSI] & (1 << FMRadio.BIT_SFBL);
            this.registers[FMRadio.REGISTER_POWERCFG] &= 0xFEFF; //Clear the Seek bit after Seek has completed
            this.UpdateRegisters();

            //Wait for the si4703 to clear the STC as well
            while (true)
            {
                this.ReadRegisters();
                if ((this.registers[FMRadio.REGISTER_STATUSRSSI] & (1 << FMRadio.BIT_STC)) == 0)
                    break;
            }

            if (valueSFBL > 0) //The bit was set indicating we hit a band limit or failed to find a station
                return false;

            return true;
        }
    }
}
