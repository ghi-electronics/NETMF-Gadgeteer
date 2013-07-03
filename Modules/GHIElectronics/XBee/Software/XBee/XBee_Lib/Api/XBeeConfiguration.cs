using NETMF.OpenSource.XBee.Api.Common;

namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// Holds basic information about XBee module and allows to set it's properties using AT commands.
    /// </summary>
    public class XBeeConfiguration
    {
        private readonly XBeeApi _xbee;
        private readonly XBeeAddress _remoteXbee;

        public HardwareVersions HardwareVersion { get; private set; }
        public string Firmware { get; private set; }
        public XBeeAddress64 SerialNumber { get; private set; }
        public ApiModes ApiMode { get; private set; }
        public string NodeIdentifier { get; private set; }

        private XBeeConfiguration(XBeeApi xbee, XBeeAddress remoteXbee = null)
        {
            _xbee = xbee;
            _remoteXbee = remoteXbee;
        }

        /// <summary>
        /// Reads module basic information
        /// </summary>
        /// <param name="xbee">XBee module to read data from</param>
        /// <returns>XBee basic information</returns>
        public static XBeeConfiguration Read(XBeeApi xbee)
        {
            return new XBeeConfiguration(xbee)
            {
                ApiMode = Common.ApiMode.Read(xbee),
                HardwareVersion = Common.HardwareVersion.Read(xbee),
                Firmware = Common.Firmware.Read(xbee),
                SerialNumber = Common.SerialNumber.Read(xbee),
                NodeIdentifier = Common.NodeIdentifier.Read(xbee)
            };
        }

        /// <summary>
        /// Reads remote module basic information
        /// </summary>
        /// <param name="sender">XBee module that will send AT command to remote target</param>
        /// <param name="remoteXbee">XBee module which infomation will be retrieved</param>
        /// <returns>Remote XBee basic infomation</returns>
        public static XBeeConfiguration Read(XBeeApi sender, XBeeAddress remoteXbee)
        {
            return new XBeeConfiguration(sender, remoteXbee)
            {
                ApiMode = Common.ApiMode.Read(sender, remoteXbee),
                HardwareVersion = Common.HardwareVersion.Read(sender, remoteXbee),
                Firmware = Common.Firmware.Read(sender, remoteXbee),
                SerialNumber = Common.SerialNumber.Read(sender, remoteXbee),
                NodeIdentifier = Common.NodeIdentifier.Read(sender, remoteXbee)
            };
        }

        public void SetApiMode(ApiModes apiMode)
        {
            if (_remoteXbee != null)
            {
                Common.ApiMode.Write(_xbee, _remoteXbee, apiMode);
            }
            else
            {
                Common.ApiMode.Write(_xbee, apiMode);
            }

            ApiMode = apiMode;
        }

        public void SetNodeIdentifier(string nodeIdentifier)
        {
            if (_remoteXbee != null)
            {
                Common.NodeIdentifier.Write(_xbee, _remoteXbee, nodeIdentifier);
            }
            else
            {
                Common.NodeIdentifier.Write(_xbee, nodeIdentifier);
            }

            NodeIdentifier = nodeIdentifier;
        }

        /// <summary>
        /// Saves changes permanently in module flash memory.
        /// </summary>
        public void Save()
        {
            if (_remoteXbee != null)
            {
                SaveSettings.Write(_xbee, _remoteXbee);
            }
            else
            {
                SaveSettings.Write(_xbee);   
            }
        }

        public bool IsSeries1()
        {
            switch (HardwareVersion)
            {
                case HardwareVersions.Series1:
                case HardwareVersions.Series1Pro:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsSeries2()
        {
            switch (HardwareVersion)
            {
                case HardwareVersions.Series2:
                case HardwareVersions.Series2Pro:
                case HardwareVersions.Series2BPro:
                    return true;
                default:
                    return false;
            }
        }

        public override string ToString()
        {
            return "ApiMode: " + Common.ApiMode.GetName(ApiMode)
                   + ", HardwareVersion: " + Common.HardwareVersion.GetName(HardwareVersion)
                   + ", Firmware: " + Firmware
                   + ", SerialNumber: " + SerialNumber
                   + ", NodeIdentifier: '" + NodeIdentifier + "'";
        }
    }
}