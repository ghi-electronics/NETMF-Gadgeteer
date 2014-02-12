using System;

namespace NETMF.OpenSource.XBee.Util
{
    public interface IInputStream : IDisposable
    {
        byte Read();
        byte Read(string message);
        byte[] Read(int count);
    }
}