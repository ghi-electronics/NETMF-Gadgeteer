namespace NETMF.OpenSource.XBee.Api.Common
{
  /// <summary>
  /// Reset Mode
  /// </summary>
  /// <remarks>
  /// TODO: Update  comments    
  /// </remarks>
    public enum ResetMode
    {
        /// <summary>
        /// Reset module.
        /// </summary>
        Software,

        /// <summary>
        /// Reset network layer parameters.
        /// </summary>
        Network,
        
        /// <summary>
        /// Restore module parameters to factory defaults.
        /// </summary>
        RestoreDefaults
    }
}