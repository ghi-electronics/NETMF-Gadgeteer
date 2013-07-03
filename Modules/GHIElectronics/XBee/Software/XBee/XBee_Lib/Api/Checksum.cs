namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public class Checksum
    {
        /// <summary>
        ///  TODO: Update Comments
        ///     
        /// </summary>
        private int _checksum;

        /// <summary>
        /// Don't add Checksum byte when computing checksum!!
        /// </summary>
        /// <param name="value"></param>
        public void AddByte(int value)
        {
            _checksum += value;
        }

        /// <summary>
        /// Computes checksum and stores in checksum instance variable
        /// </summary>
        public int Compute()
        {
            // discard values > 1 byte
            _checksum = 0xff & _checksum;
            // perform 2s complement
            _checksum = 0xff - _checksum;
            return _checksum;
        }

        /// <summary>
        /// First add all relevant bytes, including checksum
        /// </summary>
        /// <returns></returns>
        public bool Verify()
        {
            _checksum = _checksum & 0xff;
            return 0xff == _checksum;
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <returns>
        ///     A byte value...
        /// </returns>
        public byte GetChecksum()
        {
            return (byte)_checksum;
        }

        /***********************************************************************/
        /* Additional methods that could be used instead of the original above */
        /***********************************************************************/

        /// <summary>
        /// Resets the checksum value
        /// </summary>
        public void Clear()
        {
            _checksum = 0;
        }

        /// <summary>
        /// Verify that data contains valid checksum
        /// </summary>
        /// <param name="bytes">Data bytes with checksum</param>
        /// <returns><c>True</c> is checksum is valid, <c>false</c> otherwise</returns>
        public static bool Verify(byte[] bytes)
        {
            return Compute(bytes, 0, bytes.Length - 1) == bytes[bytes.Length-1];
        }

        /// <summary>
        /// Computes checksum for given bytes
        /// </summary>
        /// <param name="bytes">Data bytes</param>
        /// <param name="offset">From where to start</param>
        /// <param name="count">How many bytes to include in checksum</param>
        /// <returns>Calculated checsum</returns>
        public static byte Compute(byte[] bytes, int offset, int count)
        {
            var checksum = 0;

            for (var i = offset; i < offset + count; i++)
                checksum += bytes[i];

            return (byte)(0xFF - checksum);
        }
    }
}