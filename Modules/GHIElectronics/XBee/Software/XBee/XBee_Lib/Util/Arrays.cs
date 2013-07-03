using System;
using System.Text;

namespace NETMF.OpenSource.XBee.Util
{
    public static class Arrays
    {
        public static bool AreEqual(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (var i = 0; i < array1.Length; i++)
                if (array1[i] != array2[i])
                    return false;

            return true;
        }

        public static int HashCode(byte[] array)
        {
            var result = array.Length;

            for (var i = 0; i < array.Length; i++)
                if (array[i] > 0)
                    result *= (array[i] << i);

            return result;
        }

        public static byte[] ToByteArray(string message, int offset = 0, int count = int.MaxValue)
        {
            var byteArray = Encoding.UTF8.GetBytes(message);

            if (offset == 0 && byteArray.Length <= count)
                return byteArray;
        
            var result = new byte[Math.Min(byteArray.Length, count)];

            for (var i = 0; i < result.Length; i++)
                result[i] = byteArray[offset++];

            return result;
        }

        public static byte[] ToByteArray(ushort value)
        {
            return new[] { UshortUtils.Msb(value), UshortUtils.Lsb(value) };
        }

        public static byte[] ToByteArray(ulong value)
        {
            var result = new byte[8];
            var index = result.Length;

            while (index > 0)
            {
                result[--index] = (byte)value;
                value >>= 8;
            }

            return result;
        }

        public static string ToString(byte[] array)
        {
            return new string(Encoding.UTF8.GetChars(array));
        }
    }
}