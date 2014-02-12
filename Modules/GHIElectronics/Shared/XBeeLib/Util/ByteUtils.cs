using System;

namespace NETMF.OpenSource.XBee.Util
{
    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public class ByteUtils
    {
        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        private const string Hex = "0123456789ABCDEF";

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="value" type="byte[]">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <returns>
        ///     A string value...
        /// </returns>
        public static string ToBase16(byte[] value)
        {
            var result = "";

            foreach (var b in value)
                result += ToBase16(b);

            return result;
        }


        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="b" type="ushort">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <returns>
        ///     A string value...
        /// </returns>
        public static string ToBase16(ushort b)
        {
            return b <= byte.MaxValue
                       ? new string(new[] {Hex[b >> 4], Hex[b & 0x0F]})
                       : new string(new[] {Hex[b >> 12], Hex[(b >> 8) & 0x0F], Hex[(b >> 4) & 0x00F], Hex[b & 0x0000F]});
        }

        /// <summary>
        /// Taken from here
        /// http://code.tinyclr.com/project/100/another-fast-hex-string-to-byte-conversion/
        /// </summary>
        public static byte FromBase16(string hexNumber)
        {
            var lowDigit = 0;
            var highDigit = 0;

            if (hexNumber.Length > 2)
                throw new InvalidCastException("The number to convert is too large for a byte, or not hexadecimal");

            switch (hexNumber.Length)
            {
                case 1:
                    lowDigit = hexNumber[0] - '0';
                    break;
                case 2:
                    lowDigit = hexNumber[1] - '0';
                    highDigit = hexNumber[0] - '0';
                    break;
            }

            if (lowDigit > 9) lowDigit -= 7;
            if (lowDigit > 15) lowDigit -= 32;
            
            if (lowDigit > 15) 
                throw new InvalidCastException("The number to convert is not hexadecimal");
            
            if (highDigit > 9) highDigit -= 7;
            if (highDigit > 15) highDigit -= 32;
            
            if (highDigit > 15) 
                throw new InvalidCastException("The number to convert is not hexadecimal");

            return (byte) (lowDigit + (highDigit << 4));
        }


        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="value" type="byte">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <param name="bit" type="int">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <returns>
        ///     A bool value...
        /// </returns>
        public static bool GetBit(byte value, int bit)
        {
            if (bit > 7)
                throw new IndexOutOfRangeException("Bit value range is: 0 (lsb) - 7 (msb)");

            return ((value >> bit) & 1) == 1;
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="value" type="byte">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <returns>
        ///     A string value...
        /// </returns>
        public static string FormatByte(byte value)
        {
            return "0x" + ToBase16(value);
        }
    }
}