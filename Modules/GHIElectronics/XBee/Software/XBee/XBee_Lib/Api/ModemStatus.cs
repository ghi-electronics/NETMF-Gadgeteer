namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// TODO: Update comments    
    /// </summary>
    public enum ModemStatus : byte
    {
        /// <summary>
        /// TODO: Update comments    
        /// </summary>
        HardwareReset = 0,

        /// <summary>
        /// TODO: Update comments    
        /// </summary>
        WatchdogTimerReset = 1,

        /// <summary>
        /// Joined network (routers and end devices)
        /// </summary>
        Associated = 2,

        /// <summary>
        /// TODO: Update comments    
        /// </summary>
        Disassociated = 3,

        /// <summary>
        /// TODO: Update comments    
        /// </summary>
        SynchronizationLost = 4,

        /// <summary>
        /// TODO: Update comments    
        /// </summary>
        CoordinatorRealigment = 5,

        /// <summary>
        /// TODO: Update comments    
        /// </summary>
        CoordinatorStarted = 6,

        /// <summary>
        /// Network security key was updated
        /// </summary>
        SecurityKeyUpdated = 7,

        /// <remarks>
        /// Voltage supply limit exceeded (PRO S2B only)
        /// </remarks>
        VoltageLimitExceeded = 0x0D,

        /// <summary>
        /// Modem configuration changed while join in progress
        /// </summary>
        ConfigChangedWhileJoining = 0x11,

        /// <summary>
        /// Stack error for values 0x80+
        /// </summary>
        StackError = 0x80
    }
}