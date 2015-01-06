using System;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// An LEDMatrix module for .NET Gadgeteer
    /// </summary>
    [Obsolete]
    public class LEDMatrix : GTM.DaisyLinkModule
    {
        private const byte GHI_DAISYLINK_MANUFACTURER = 0x10;
        private const byte GHI_DAISYLINK_TYPE_LEDMATRIX = 0x02;
        private const byte GHI_DAISYLINK_VERSION_LEDMATRIX = 0x01;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
		public LEDMatrix(int socketNumber) : base(socketNumber, LEDMatrix.GHI_DAISYLINK_MANUFACTURER, LEDMatrix.GHI_DAISYLINK_TYPE_LEDMATRIX, LEDMatrix.GHI_DAISYLINK_VERSION_LEDMATRIX, LEDMatrix.GHI_DAISYLINK_VERSION_LEDMATRIX, 50, "LEDMatrix")
        {

        }

        /// <summary>
        /// Draws an 8x8 bitmap to the module.
        /// </summary>
        /// <param name="bitmap">The array of 8 bytes to display on the LED Matrix.</param>
        public void DrawBitmap(byte[] bitmap)
        {
			if (bitmap == null) throw new ArgumentNullException("bitmap");
			if (bitmap.Length != 8) throw new ArgumentException("bitmap.Length must be 8.", "bitmap");

			for (int i = 0; i < 8; i++)
				this.Write((byte)(LEDMatrix.DaisyLinkOffset + i), (byte)bitmap[i]);
        }
    }
}
