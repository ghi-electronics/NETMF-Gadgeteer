using Microsoft.SPOT;
using System;
using GT = Gadgeteer;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A GPS module for Microsoft .NET Gadgeteer
    /// </summary>
    public class GPS : GTM.Module
    {
        private GTI.DigitalOutput powerControl;
        private GTI.DigitalOutput gpsExtInt;
        private GTI.Serial serial;
        private TimeSpan lastPositionReceived;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public GPS(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('U', this);

            this.lastPositionReceived = TimeSpan.MinValue;
            this.LastPosition = null;

            this.powerControl = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three, true, this);
            this.gpsExtInt = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Six, true, this);

            this.serial = GTI.SerialFactory.Create(socket, 9600, GTI.SerialParity.None, GTI.SerialStopBits.One, 8, GTI.HardwareFlowControl.NotRequired, this);
            this.serial.NewLine = "\r\n";
            this.serial.LineReceived += this.OnLineReceived;
        }

        /// <summary>
        /// Enables or disables the GPS. 
        /// </summary>
        public bool Enabled
        {
            get
            {
                return !this.powerControl.Read();
            }
            set
            {
                this.powerControl.Write(!value);

                if (!value) 
                    serial.Close();

                if (value) 
                    serial.Open();
            }
        }

        /// <summary>
        /// The last valid position received.
        /// </summary>
        public Position LastPosition { get; private set; }

        /// <summary>
        /// The time elapsed since the last position was received.
        /// </summary>
        public TimeSpan LastValidPositionAge
        {
            get
            {
                if (this.lastPositionReceived == TimeSpan.MinValue) 
                    return TimeSpan.MaxValue;

                return GT.Timer.GetMachineTime() - this.lastPositionReceived;
            }
        }

        private void OnLineReceived(GTI.Serial sender, string line)
        {
            try
            {
                if (line != null && line.Length > 0) 
                    this.OnNmeaSentenceReceived(this, line);

                if (line.Substring(0, 7) != "$GPRMC,") 
                    return;
                
                string[] tokens = line.Split(',');
                if (tokens.Length != 13)
                {
                    this.DebugPrint("RMC NMEA line does not have 13 tokens, ignoring");

                    return;
                }

                if (tokens[2] != "A")
                {
                    this.OnInvalidPositionReceived(this, null);

                    return;
                }

                double timeRawDouble = Double.Parse(tokens[1]);

                int timeRaw = (int)timeRawDouble;
                int hours = timeRaw / 10000;
                int minutes = (timeRaw / 100) % 100;
                int seconds = timeRaw % 100;
                int milliseconds = (int)((timeRawDouble - timeRaw) * 1000.0);
                int dateRaw = Int32.Parse(tokens[9]);
                int days = dateRaw / 10000;
                int months = (dateRaw / 100) % 100;
                int years = 2000 + (dateRaw % 100);

                Position position = new Position();

                position.FixTimeUtc = new DateTime(years, months, days, hours, minutes, seconds, milliseconds);
                position.LatitudeString = tokens[3] + " " + tokens[4];
                position.LongitudeString = tokens[5] + " " + tokens[6];

                double latitudeRaw = double.Parse(tokens[3]);
                int latitudeDegreesRaw = ((int)latitudeRaw) / 100;
                double latitudeMinutesRaw = latitudeRaw - (latitudeDegreesRaw * 100);
                position.Latitude = latitudeDegreesRaw + (latitudeMinutesRaw / 60.0);

                if (tokens[4] == "S") 
                    position.Latitude = -position.Latitude;

                double longitudeRaw = double.Parse(tokens[5]);
                int longitudeDegreesRaw = ((int)longitudeRaw) / 100;
                double longitudeMinutesRaw = longitudeRaw - (longitudeDegreesRaw * 100);
                position.Longitude = longitudeDegreesRaw + (longitudeMinutesRaw / 60.0);

                if (tokens[6] == "W") 
                    position.Longitude = -position.Longitude;

                position.SpeedKnots = 0;
                if (tokens[7] != "") 
                    position.SpeedKnots = Double.Parse(tokens[7]);

                position.CourseDegrees = 0;
                if (tokens[8] != "") 
                    position.CourseDegrees = Double.Parse(tokens[8]);

                this.lastPositionReceived = GT.Timer.GetMachineTime();
                this.LastPosition = position;
                this.OnPositionReceived(this, position);
            }
            catch
            {
                this.DebugPrint("Error parsing RMC NMEA message");
            }
        }

        /// <summary>
        /// Represents a GPS position.
        /// </summary>
        public class Position
        {
            /// <summary>
            /// The latitude.
            /// </summary>
            public double Latitude { get; set; }

            /// <summary>
            /// The longitude.
            /// </summary>
            public double Longitude { get; set; }

            /// <summary>
            /// A string representing the latitude, in the format ddmm.mmmm H, where dd = degrees, mm.mmm = minutes and fractional minutes, and H = hemisphere (N/S).
            /// </summary>
            public string LatitudeString { get; set; }

            /// <summary>
            /// A string representing the longitude, in the format ddmm.mmmm H, where dd = degrees, mm.mmm = minutes and fractional minutes, and H = hemisphere (E/W).
            /// </summary>
            public string LongitudeString { get; set; }

            /// <summary>
            /// Speed over the ground in knots.
            /// </summary>
            public double SpeedKnots { get; set; }

            /// <summary>
            /// Course over the ground in degrees.
            /// </summary>
            public double CourseDegrees { get; set; }

            /// <summary>
            /// The Universal Coordinated Time (UTC) time of the fix.
            /// </summary>
            public DateTime FixTimeUtc { get; set; }

            /// <summary>
            /// Provides a formatted string for this Position.
            /// </summary>
            /// <returns>The formatted string.</returns>
            public override string ToString()
            {
                return "Lat " + Latitude + ", Long " + Longitude + ", Speed " + SpeedKnots + ", Course " + CourseDegrees + ", FixTime " + FixTimeUtc.ToString();
            }
        }

        /// <summary>
        /// Represents the delegate that is used to handle the PositionReceived event
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void PositionReceivedHandler(GPS sender, Position e);

        /// <summary>
        /// Represents the delegate that is used to handle the NMEASentenceReceived event
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void NmeaSentenceReceivedHandler(GPS sender, string e);

        /// <summary>
        /// Represents the delegate that is used to handle the InvalidPositionReceived event
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void InvalidPositionReceivedHandler(GPS sender, EventArgs e);

        /// <summary>
        /// Raised when a valid position is received.
        /// </summary>
        public event PositionReceivedHandler PositionReceived;

        /// <summary>
        /// Raised when an NMEA sentence is received.  This is for advanced users who want to parse the NMEA sentences themselves.  
        /// </summary>
        public event NmeaSentenceReceivedHandler NmeaSentenceReceived;

        /// <summary>
        /// Raised when an invalid position is received.
        /// </summary>
        public event InvalidPositionReceivedHandler InvalidPositionReceived;

        private PositionReceivedHandler onPositionReceived;
        private NmeaSentenceReceivedHandler onNmeaSentenceReceived;
        private InvalidPositionReceivedHandler onInvalidPositionReceived;

        private void OnPositionReceived(GPS sender, Position e)
        {
            if (this.onPositionReceived == null)
                this.onPositionReceived = this.OnPositionReceived;

            if (Program.CheckAndInvoke(this.PositionReceived, this.onPositionReceived, sender, e))
                this.PositionReceived(sender, e);
        }

        private void OnNmeaSentenceReceived(GPS sender, string e)
        {
            if (this.onNmeaSentenceReceived == null)
                this.onNmeaSentenceReceived = this.OnNmeaSentenceReceived;

            if (Program.CheckAndInvoke(this.NmeaSentenceReceived, this.onNmeaSentenceReceived, sender, e))
                this.NmeaSentenceReceived(sender, e);
        }

        private void OnInvalidPositionReceived(GPS sender, EventArgs e)
        {
            if (this.onInvalidPositionReceived == null)
                this.onInvalidPositionReceived = this.OnInvalidPositionReceived;

            if (Program.CheckAndInvoke(this.InvalidPositionReceived, this.onInvalidPositionReceived, sender, e))
                this.InvalidPositionReceived(sender, e);
        }
    }
}
