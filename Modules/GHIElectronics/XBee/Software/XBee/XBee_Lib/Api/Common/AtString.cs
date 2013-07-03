using System;

namespace NETMF.OpenSource.XBee.Api.Common
{
    internal class AtString : Attribute
    {
        public string Value { get; set; }

        public AtString(string value)
        {
            Value = value;
        }
    }
}
