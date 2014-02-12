using System.Collections;

namespace NETMF.OpenSource.XBee.Api.Common
{
    public static class HardwareVersion
    {
        private static readonly Hashtable HardwareVersionNames;

        static HardwareVersion()
        {
            HardwareVersionNames = new Hashtable
            {
                {HardwareVersions.Unknown,"Unknown"},
                {HardwareVersions.Series1,"Series 1"},
                {HardwareVersions.Series1Pro,"Series 1 Pro"},
                {HardwareVersions.Series2,"Series 2"},
                {HardwareVersions.Series2Pro,"Series 2 Pro"},
                {HardwareVersions.Series2BPro,"Series 2B Pro"},
                {HardwareVersions.Series6,"Series 6"}
            };
        }

        public static HardwareVersions Read(XBeeApi xbee)
        {
            var request = xbee.Send(AtCmd.HardwareVersion);
            return Parse(request.GetResponse());
        }

        public static HardwareVersions Read(XBeeApi sender, XBeeAddress remoteXBee)
        {
            var request = sender.Send(AtCmd.HardwareVersion).To(remoteXBee);
            return Parse((AtResponse) request.GetResponse());
        }

        public static HardwareVersions Parse(AtResponse response)
        {
            if (!response.IsOk)
                throw new XBeeException("Attempt to query remote HV parameter failed");

            return (HardwareVersions)response.Value[0];
        }

        public static string GetName(HardwareVersions radiotype)
        {
            return (string) HardwareVersionNames[radiotype];
        }
    }
}