using System.IO;

namespace NETMF.OpenSource.XBee.Util
{
    public class OutputStream : IOutputStream
    {
        private readonly Stream _stream;

        public OutputStream()
        {
            _stream = new MemoryStream();
        }

        #region IOutputStream Members

        public void Write(byte data)
        {
            _stream.WriteByte(data);
        }

        public void Write(int data)
        {
            _stream.WriteByte((byte) data);
        }

        public void Write(ushort data)
        {
            Write(Arrays.ToByteArray(data));
        }

        public void Write(string data)
        {
            Write(Arrays.ToByteArray(data));
        }

        public void Write(byte[] data)
        {
            _stream.Write(data, 0, data.Length);
        }

        public byte[] ToArray()
        {
            var result = new byte[(int)(_stream.Length)];
            _stream.Position = 0;
            _stream.Read(result, 0, result.Length);
            return result;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        #endregion
    }
}