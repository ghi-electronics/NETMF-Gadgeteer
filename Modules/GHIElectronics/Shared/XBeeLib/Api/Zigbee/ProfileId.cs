namespace NETMF.OpenSource.XBee.Api.Zigbee
{
    /// <summary>
    /// Application profiles provide messaging schemes allowing to communicate with connected device.
    /// </summary>
    public enum ProfileId : ushort
    {
        /// <summary>
        /// Zigbee Device Profile (ZDP).
        /// </summary>
        ZigbeeDevice = 0x0000,

        /// <summary>
        /// Smart homes (HA).
        /// </summary>
        HomeAutomation = 0x0104,

        /// <summary>
        /// Value-added services (TA).
        /// </summary>
        TelecomServices = 0x0107,
        
        /// <summary>
        /// Health and fitness monitoring (HC).
        /// </summary>
        HealthCare = 0x0108,

        /// <summary>
        /// Home energy savings (SE). 
        /// </summary>
        SmartEnergy = 0x0109,

        /// <summary>
        /// Digi private profile called Drop-In-Networking.
        /// </summary>
        Digi = 0xC105
    }
}