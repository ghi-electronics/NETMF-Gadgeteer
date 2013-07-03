using System;

namespace NETMF.OpenSource.XBee.Api
{
    public class XBeeException : Exception
    {
        public XBeeException()
        {
        }

        public XBeeException(string message) : base(message)
        {
        }

        public XBeeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}