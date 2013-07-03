using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api.Wpan
{
    /// <summary>
    /// Provides access to XBee's 8 Digital (0-7) and 6 Analog (0-5) IO pins.
    /// </summary>
    public class IoSample
    {
        private readonly double[] _analog;
        private readonly ushort _digital;

        public IoSample(double[] analog, ushort digital)
        {
            _analog = analog;
            _digital = digital;
        }

        /// <summary>
        /// Get A/D reading.
        /// </summary>
        /// <param name="pin">Analog pin</param>
        /// <returns>Reading in mV</returns>
        public double GetValue(Pin.Analog pin)
        {
            return _analog[(byte)pin];
        }

        /// <summary>
        /// Get digital pin state.
        /// </summary>
        /// <param name="pin">Digital pin</param>
        /// <returns>Returns <c>true</c> if low, <c>false</c> if high.</returns>
        public bool GetValue(Pin.Digital pin)
        {
            return UshortUtils.GetBit(_digital, (byte)pin);
        }
    }
}