namespace NETMF.OpenSource.XBee.Util
{
    /// <summary>
    ///  TODO: Update comments
    ///     
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public static class AdcHelper
    {
        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="adcReading" type="ushort">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <returns>
        ///     A double value...
        /// </returns>
        public static double ToMilliVolts(ushort adcReading)
        {
            return adcReading * 1200 / 1023.0;
        }
    }
}