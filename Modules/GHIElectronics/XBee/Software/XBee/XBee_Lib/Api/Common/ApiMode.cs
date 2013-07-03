using System.Collections;

namespace NETMF.OpenSource.XBee.Api.Common
{
    public static class ApiMode
    {
        private static readonly Hashtable ApiModeNames;

        static ApiMode()
        {
            ApiModeNames = new Hashtable
            {
                {ApiModes.Disabled,"Disabled"},
                {ApiModes.Enabled,"Enabled"},
                {ApiModes.EnabledWithEscaped,"EnabledWithEscaped"},
                {ApiModes.Unknown,"Unknown"}
            };
        }

        public static ApiModes Read(XBeeApi xbee)
        {
            var request = xbee.Send(AtCmd.ApiEnable);
            return Parse(request.GetResponse());
        }

        public static ApiModes Read(XBeeApi sender, XBeeAddress remoteXbee)
        {
            var request = sender.Send(AtCmd.ApiEnable).To(remoteXbee);
            return Parse((AtResponse) request.GetResponse());
        }

        public static ApiModes Parse(AtResponse response)
        {
            if (!response.IsOk)
                throw new XBeeException("Attempt to query AP parameter failed");

            return (ApiModes) response.Value[0];
        }

        public static void Write(XBeeApi xbee, ApiModes mode)
        {
            var request = xbee.Send(AtCmd.ApiEnable, new[] {(byte) mode});
            var response = request.GetResponse();

            if (!response.IsOk)
                throw new XBeeException("Failed to write api mode");
        }

        public static void Write(XBeeApi sender, XBeeAddress remoteXbee, ApiModes mode)
        {
            var request = sender.Send(AtCmd.ApiEnable, new[] {(byte) mode}).To(remoteXbee);
            var response = (AtResponse) request.GetResponse();

            if (!response.IsOk)
                throw new XBeeException("Failed to write api mode");
        }

        public static string GetName(ApiModes apiMode)
        {
            return (string)ApiModeNames[apiMode];
        }
    }
}