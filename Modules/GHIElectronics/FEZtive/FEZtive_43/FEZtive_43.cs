using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

using Microsoft.SPOT.Presentation;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2C), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// A FEZtive module for Microsoft .NET Gadgeteer
    /// </summary>
    [Obsolete]
    public class FEZtive : GTM.Module
    {
        // Gadgeteer module driver variables
        private GT.Socket _socket;
        private GTI.SPI _spi;
        private GTI.SPI.Configuration _spiConfig;

        // Array of color structures that represent the LEDs connected to the module
        // One Color object represents one LED on the strip
        private Color[] LEDs;
        private byte[] _zeros;

        // Defines of basic colors for quick color definitions
        #region Basic Color Definitions

        /// <summary>
        /// Predefined Red values
        /// </summary>
        public Color Red
        {
            get;
            protected set;
        }

        /// <summary>
        /// Predefined Blue values
        /// </summary>
        public Color Blue
        {
            get;
            protected set;
        }

        /// <summary>
        /// Predefined Green values
        /// </summary>
        public Color Green
        {
            get;
            protected set;
        }

        /// <summary>
        /// Predefined White values
        /// </summary>
        public Color White
        {
            get;
            protected set;
        }

        /// <summary>
        /// Predefined Black values
        /// </summary>
        public Color Black
        {
            get;
            protected set;
        }

        #endregion

        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public FEZtive(int socketNumber)
        {
            _socket = Socket.GetSocket(socketNumber, true, this, null);

            _socket.EnsureTypeIsSupported('S', this);

            Red = new Color(127, 0, 0);
            Blue = new Color(0, 0, 127);
            Green = new Color(0, 127, 0);
            White = new Color(127, 127, 127);
            Black = new Color(0, 0, 0);
        }

        /// <summary>
        /// Initializes the module for the passed in clock rate and the length of the LED strip
        /// </summary>
        /// <param name="numLEDS">Number of LEDs to control. Default as provided with the module is 80 per strip.</param>
        /// <param name="spiClockRateKHZ">The SPI clock rate in KHz.</param>
        public void Initialize(int numLEDS = 80, uint spiClockRateKHZ = 1000)
        {
            _spiConfig = new GTI.SPI.Configuration(true, 0, 0, false, true, spiClockRateKHZ);

            _spi = new GTI.SPI(_socket, _spiConfig, GTI.SPI.Sharing.Shared, this);

            LEDs = new Color[numLEDS];

            for (int i = 0; i < LEDs.Length; i++)
            {
                LEDs[i] = new Color(0, 0, 0);
            }

            _zeros = new byte[3 * ((numLEDS + 63) / 64)];
        }

        /// <summary>
        /// Sets all LEDs to the passed in Color structure
        /// </summary>
        /// <param name="color">Color to set all LEDs to. Color values must be between 0-127.</param>
        public void SetAll(Color color)
        {
            //Clear();

            _spi.Write(_zeros);

            for (int i = 0; i < LEDs.Length; i += 2)
            {
                LEDs[i] = color;
                LEDs[i + 1] = color;
                
                _spi.Write(LEDs[i].GetForRender());
                _spi.Write(LEDs[i + 1].GetForRender());
            }

            _spi.Write(_zeros);
        }

        /// <summary>
        /// Sets all LEDs to the passed in array of Color structures and resends the colors
        /// </summary>
        /// <param name="colorArr">An array of Color structures to set every LED to. Color values must be between 0-127.</param>
        public void SetAll(Color[] colorArr)
        {
            if (colorArr.Length > LEDs.Length)
                throw new ArgumentOutOfRangeException("colorArr", "is larger than expected!");

            for (int i = 0; i < LEDs.Length; i += 2)
            {
                SetLED(colorArr[i], i);
                SetLED(colorArr[i + 1], i + 1);
                
                _spi.Write(LEDs[i].GetForRender());
                _spi.Write(LEDs[i + 1].GetForRender());
            }

            _spi.Write(_zeros);
        }

        /// <summary>
        /// Sets the specified LED to the passed in color and resends the colors
        /// </summary>
        /// <param name="color">Color to set the LED to. Color values must be between 0-127.</param>
        /// <param name="numLED">The LED to set the color of</param>
        public void SetLED(Color color, int numLED)
        {
            LEDs[numLED] = color;

            Redraw();
        }

        /// <summary>
        /// Returns an array of the current colors displayed by this module
        /// </summary>
        /// <returns>Array of Color structures that the current module is holding</returns>
        public Color[] GetCurrentColors()
        {
            return LEDs;
        }

        /// <summary>
        /// Turns all LEDs off (Black)
        /// </summary>
        public void Clear()
        {
            SetAll(Black);
        }

        /// <summary>
        /// Redraws all of the colors. Only to be used after a change was made to the Color array.
        /// </summary>
        public void Redraw()
        {
            _spi.Write(_zeros);

            for (int i = 0; i < LEDs.Length; i += 2)
            {
                _spi.Write(LEDs[i].GetForRender());
                _spi.Write(LEDs[i + 1].GetForRender());
            }

            _spi.Write(_zeros);
        }
    }

    /// <summary>
    /// Class that holds information describing a color for one LED on this module
    /// </summary>
    [Obsolete]
    public class Color
    {
        private byte red;
        private byte green;
        private byte blue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="r">Red value. Must be between 0-127.</param>
        /// <param name="g">Green value. Must be between 0-127.</param>
        /// <param name="b">Blue value. Must be between 0-127.</param>
        public Color(byte r, byte g, byte b)
        {
            this.red = r;
            this.green = g;
            this.blue = b;
        }

        /// <summary>
        /// Sets the color to the passed in RGB color values
        /// </summary>
        /// <param name="r">Red value. Must be between 0-127.</param>
        /// <param name="g">Green value. Must be between 0-127.</param>
        /// <param name="b">Blue value. Must be between 0-127.</param>
        public void Set(byte r, byte g, byte b)
        {
            this.red = r;
            this.green = g;
            this.blue = b;

            //this.red = (byte)(0x80 | r);
            //this.green = (byte)(0x80 | g);
            //this.blue = (byte)(0x80 | b);
        }

        /// <summary>
        /// Sets the color to the passed in Color structure
        /// </summary>
        /// <param name="color">Color structure to be used. Color values must be between 0-127.</param>
        public void Set(Color color)
        {
            this.Set(color.red, color.green, color.blue);
        }

        /// <summary>
        /// Returns a byte[] containing the current colors for rendering. Do not use to change the colors.
        /// </summary>
        /// <returns>A byte[] containing the current color </returns>
        public byte[] GetForRender()
        {
            return new byte[] { (byte)(0x80 | green), (byte)(0x80 | red), (byte)(0x80 | blue) };
        }
    };

}