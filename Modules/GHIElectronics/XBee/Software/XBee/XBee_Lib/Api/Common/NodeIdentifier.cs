using NETMF.OpenSource.XBee.Util;

namespace NETMF.OpenSource.XBee.Api.Common
{
    public class NodeIdentifier
    {
        public const byte MaxNodeIdentifierLength = 20;

        public static string Read(XBeeApi xbee)
        {
            var request = xbee.Send(AtCmd.NodeIdentifier);
            return Parse(request.GetResponse());
        }

        public static string Read(XBeeApi sender, XBeeAddress remoteXbee)
        {
            var request = sender.Send(AtCmd.NodeIdentifier).To(remoteXbee);
            return Parse((AtResponse) request.GetResponse());
        }

        private static string Parse(AtResponse response)
        {
            if (!response.IsOk)
                throw new XBeeException("Failed to read node identifier");

            return Arrays.ToString(response.Value);
        }

        public static void Write(XBeeApi xbee, string nodeIdentifier)
        {
            var value = Arrays.ToByteArray(nodeIdentifier, 0, MaxNodeIdentifierLength);
            var response = xbee.Send(AtCmd.NodeIdentifier, value).GetResponse();

            if (!response.IsOk)
                throw new XBeeException("Failed to write node identifier");
        }

        public static void Write(XBeeApi sender, XBeeAddress remoteXbee, string nodeIdentifier)
        {
            var value = Arrays.ToByteArray(nodeIdentifier, 0, MaxNodeIdentifierLength);
            var request = sender.Send(AtCmd.NodeIdentifier, value).To(remoteXbee);
            var response = (AtResponse) request.GetResponse();

            if (!response.IsOk)
                throw new XBeeException("Failed to write node identifier");
        }
    }
}