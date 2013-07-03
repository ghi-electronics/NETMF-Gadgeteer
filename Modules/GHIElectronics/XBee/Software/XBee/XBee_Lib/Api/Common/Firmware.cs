using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api.Common
{
    public static class Firmware
    {
        public static string Read(XBeeApi xbee)
        {
            var request = xbee.Send(AtCmd.FirmwareVersion);
            return Parse(request.GetResponse());
        }

        public static string Read(XBeeApi sender, XBeeAddress remoteXbee)
        {
            var request = sender.Send(AtCmd.FirmwareVersion).To(remoteXbee);
            return Parse((AtResponse) request.GetResponse());
        }

        public static string Parse(AtResponse response)
        {
            if (!response.IsOk)
                throw new XBeeException("Attempt to query HV parameter failed");

            return ByteUtils.ToBase16(response.Value);
        }
    }
}