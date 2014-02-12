namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// TODO: Update comments    
    /// </summary>
    public enum AtResponseStatus
    {
        /// <summary>
        /// TODO: Update comments    
        /// </summary>
        Ok = 0,

        /// <summary>
        /// TODO: Update comments    
        /// </summary>
        Error = 1,

        /// <summary>
        /// TODO: Update comments    
        /// </summary>
        InvalidCommand = 2,

        /// <summary>
        /// TODO: Update comments    
        /// </summary>
        IvalidParameter = 3,

        /// <summary>
        /// Series 1 remote AT only according to spec.
        /// Also series 2 in 2x64 zb pro firmware
        /// </summary>
        TransmissionFailed = 4
    }
}