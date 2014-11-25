using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;
using System;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A RelayISOx16 for Microsoft .NET Gadgeteer
    /// </summary>
    [Obsolete]
    public class RelayISOx16 : GTM.Module
    {
        private GTI.DigitalOutput data;
        private GTI.DigitalOutput clock;
        private GTI.DigitalOutput latch;
        private GTI.DigitalOutput enable;
        private GTI.DigitalOutput clear;

        private ushort regData = 0x0000;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public RelayISOx16(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('Y', this);

            this.data = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Seven, false, this);
            this.clock = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Nine, false, this);
            this.enable = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three, true, this);
            this.latch = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Five, false, this);
            this.clear = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Four, true, this);

            this.DisableAllRelays();

            this.EnableRelay(0);
            
            this.EnableOutputs();
        }

        /// <summary>
        /// Clears all relays.
        /// </summary>
        public void DisableAllRelays()
        {
            regData = 0;
            ushort reg = (ushort)regData;

            for (int i = 0; i < 16; i++)
            {
                if ((reg & 0x1) == 1)
                {
                    data.Write(false);
                }
                else
                {
                    data.Write(true);
                }


                clock.Write(true);
                clock.Write(false);

                reg >>= 1;
            }

            latch.Write(true);
            latch.Write(false);
        }

        /// <summary>
        /// Enables the relay outputs.
        /// </summary>
        private void EnableOutputs()
        {
            this.enable.Write(false);
        }

        /// <summary>
        /// Disables the relay outputs.
        /// </summary>
        private void DisableOutputs()
        {
            this.enable.Write(true);
        }

        /// <summary>
        /// Enables the given relays.
        /// </summary>
        /// <param name="relays">The relays to turn on.</param>
        public void EnableRelay(Relays relays)
        {
            this.regData |= (ushort)relays;

            ushort reg = this.regData;

            for (int i = 0; i < 16; i++)
            {
                this.data.Write((reg & 0x1) == 0);

                this.clock.Write(true);
                this.clock.Write(false);

                reg >>= 1;
            }

            this.latch.Write(true);
            this.latch.Write(false);
        }

        /// <summary>
        /// Disables the given relays.
        /// </summary>
        /// <param name="relays">The relays to turn off.</param>
        public void DisableRelay(Relays relays)
        {
            this.regData &= (ushort)(~relays);

            ushort reg = this.regData;

            for (int i = 0; i < 16; i++)
            {
                this.data.Write((reg & 0x1) == 0);

                this.clock.Write(true);
                this.clock.Write(false);

                reg >>= 1;
            }

            this.latch.Write(true);
            this.latch.Write(false);
        }

        /// <summary>
        /// The relays on the module.
        /// </summary>
        [Flags]
        public enum Relays : ushort
        {
            /// <summary>
            /// Relay 1
            /// </summary>
            Relay1 = 1,

            /// <summary>
            /// Relay 2
            /// </summary>
            Relay2 = 2,

            /// <summary>
            /// Relay 3
            /// </summary>
            Relay3 = 4,

            /// <summary>
            /// Relay 4
            /// </summary>
            Relay4 = 8,

            /// <summary>
            /// Relay 5
            /// </summary>
            Relay5 = 16,

            /// <summary>
            /// Relay 6
            /// </summary>
            Relay6 = 32,

            /// <summary>
            /// Relay 7
            /// </summary>
            Relay7 = 64,

            /// <summary>
            /// Relay 8
            /// </summary>
            Relay8 = 128,

            /// <summary>
            /// Relay 9
            /// </summary>
            Relay9 = 256,

            /// <summary>
            /// Relay 10
            /// </summary>
            Relay10 = 512,

            /// <summary>
            /// Relay 11
            /// </summary>
            Relay11 = 1024,

            /// <summary>
            /// Relay 12
            /// </summary>
            Relay12 = 2048,

            /// <summary>
            /// Relay 13
            /// </summary>
            Relay13 = 4096,

            /// <summary>
            /// Relay 14
            /// </summary>
            Relay14 = 8192,

            /// <summary>
            /// Relay 15
            /// </summary>
            Relay15 = 16384,

            /// <summary>
            /// Relay 16
            /// </summary>
            Relay16 = 32768,
        }
    }
}