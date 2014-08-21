using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A Thermocouple module for Microsoft .NET Gadgeteer
    /// </summary>
    public class Thermocouple : GTM.Module
    {
        private GTI.DigitalInput miso;
        private GTI.DigitalOutput clk;
        private GTI.DigitalOutput cs;

        private const int ERROR_NOCONECT = 0x01;
        private const int ERROR_SHORTGND = 0x02;
        private const int ERROR_SHORTVCC = 0x04;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public Thermocouple(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported(new char[] { 'X', 'Y' }, this);

            this.miso = GTI.DigitalInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.PullUp, this);
            this.clk = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Four, false, this);
            this.cs = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Five, true, this);

            this.Scale = TemperatureScale.Celsius;
        }

        private uint ReadData()
        {
            uint data = 0;

            this.cs.Write(false);

            for (int i = 31; i >= 0; i--)
            {
                this.clk.Write(true);

                if (this.miso.Read())
                    data |= (uint)(1 << i);

                this.clk.Write(false);
            }
            
            this.cs.Write(true);

            return data;
        }

        /// <summary>
        /// The possible temperature scales to return data in.
        /// </summary>
        public enum TemperatureScale
        {
            /// <summary>
            /// Measure in celsius.
            /// </summary>
            Celsius,

            /// <summary>
            /// Measure in fahrenheit.
            /// </summary>
            Fahrenheit
        }

        /// <summary>
        /// The temperature scale to measure in.
        /// </summary>
        public TemperatureScale Scale { get; set; }

        /// <summary>
        /// Reads the external temperature.
        /// </summary>
        /// <returns>The temperature.</returns>
        public int GetExternalTemperature()
        {
            int celsuius = (int)this.ReadData() >> 20;

            if (this.Scale == TemperatureScale.Celsius)
                return celsuius;

            return (int)(celsuius * 1.8) + 32;
        }

        /// <summary>
        /// Reads the internal temperature of the chip.
        /// </summary>
        /// <returns>The temperature.</returns>
        public int GetInternalTemperature()
        {
            int celsuius = (int)(this.ReadData() & 0x0000FF00) >> 8;

            if (this.Scale == TemperatureScale.Celsius)
                return celsuius;

            return (int)(celsuius * 1.8) + 32;
        }
    }
}