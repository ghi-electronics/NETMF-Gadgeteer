using System;

namespace NETMF.OpenSource.XBee.Api.Wpan
{
    public abstract class RxResponseBase : XBeeResponse
    {
        [Flags]
        public enum Options
        {
            AddressBroadcast = 2,
            PanBroadcast = 4
        }

        public int Rssi { get; set; }
        public Options Option { get; set; }
        public XBeeAddress Source { get; set; }

        public override void Parse(IPacketParser parser)
        {
            Rssi = -1 * parser.Read("RSSI");
            Option = (Options)parser.Read("Options");
        }

        public string GetOptionName(Options option)
        {
            var isAddressBroadcast = (option & Options.AddressBroadcast) != 0;
            var isPanBroadcast = (option & Options.PanBroadcast) != 0;

            if (isAddressBroadcast && isPanBroadcast)
                return "AddressBroadcast, PanBroadcast";

            if (isAddressBroadcast)
                return "AddressBroadcast";

            if (isPanBroadcast)
                return "PanBroadcast";

            return "Unicast";
        }

        public override string ToString()
        {
            return base.ToString()
                   + ",rssi=" + Rssi + "dBi"
                   + ",option=" + GetOptionName(Option)
                   + ",source=" + Source;
        }
    }
}