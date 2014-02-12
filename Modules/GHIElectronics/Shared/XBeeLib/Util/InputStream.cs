using System;
using System.IO;

namespace NETMF.OpenSource.XBee.Util
{
    public class InputStream : IInputStream
    {
        private readonly Stream _stream;

        public InputStream(Stream stream)
        {
            _stream = stream;
            _stream.Position = 0;
        }

        public InputStream(byte[] source)
        {
            _stream = new MemoryStream(source);
        }

        #region IInputStream Members

        public byte Read()
        {
            var result = _stream.ReadByte();

            if (result == -1)
                throw new InvalidOperationException("end of input stream");

            return (byte) result;
        }

        public byte Read(string message)
        {
            return Read();
        }

        /// <summary>
        /// Reads <paramref name="count"/> bytes from the input stream and returns the bytes in an array
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] Read(int count)
        {
            var block = new byte[count];
            _stream.Read(block, 0, count);
            return block;
        }

        public void Dispose()
        {
            if (_stream != null)
                _stream.Dispose();
        }

        #endregion
    }
}