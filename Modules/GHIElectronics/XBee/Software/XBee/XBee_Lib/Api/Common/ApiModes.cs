namespace NETMF.OpenSource.XBee.Api.Common
{
  /// <summary>
  /// API Modes   
  /// </summary>
  /// <remarks>
  /// TODO: Update  comments        
  /// </remarks>
    public enum ApiModes
    {
        /// <summary>
        /// Disabled (Transparent operation)
        /// </summary>
        Disabled = 0x00,

        /// <summary>
        /// API enabled
        /// </summary>
        Enabled = 0x01,

        /// <summary>
        /// API enabled (with escaped characters)
        /// </summary>
        EnabledWithEscaped = 0x02,

        /// <summary>
        /// The undefined value.
        /// </summary>
        Unknown = 0xff 
    }
}