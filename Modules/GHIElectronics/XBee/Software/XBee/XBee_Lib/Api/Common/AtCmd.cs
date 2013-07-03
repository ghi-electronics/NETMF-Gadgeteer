namespace NETMF.OpenSource.XBee.Api.Common
{
    /// <summary>
    /// Common AT commands.
    /// </summary>
    /// <remarks>
    /// Summary descriptions of these commands are copied from Digi manuals
    /// </remarks>
    public enum AtCmd : ushort
    {
        /// <summary>
        /// Read the high 32 bits of the module's unique 64-bit address.
        /// </summary>
        /// <remarks>
        /// <para>Range: 0 - 0xFFFFFFFF (read-only).</para>
        /// <para>Default: factory-set.</para>
        /// </remarks>
        [AtString("SH")]
        SerialNumberHigh = 0x5348,

        /// <summary>
        /// Read the low 32 bits of the module's unique 64-bit address.
        /// </summary>
        /// <remarks>
        /// <para>Range: 0 - 0xFFFFFFFF ƒ[read-only].</para>
        /// <para>Default: factory-set.</para>
        /// </remarks>
        [AtString("SL")]
        SerialNumberLow = 0x534C,

        /// <summary>
        /// Set/Read node string identifier.
        /// </summary>
        [AtString("NI")]
        NodeIdentifier = 0x4E49,

        /// <summary>
        /// Set/Read the node discovery timeout.
        /// </summary>
        [AtString("NT")]
        NodeDiscoverTimeout = 0x4E54,

        /// <summary>
        /// Discovers and reports all RF modules found.
        /// </summary>
        [AtString("ND")]
        NodeDiscover = 0x4E44,

        /// <summary>
        /// Write parameter values to non-volatile memory so that parameter 
        /// modifications persist through subsequent power-up or reset.
        /// </summary>
        /// <remarks>
        /// Once WR is issued, no additional characters should be sent to the 
        /// module until after the response "OK\r" is received.
        /// </remarks>
        [AtString("WR")]
        Write = 0x5752,

        /// <summary>
        /// Read firmware version of the RF module.
        /// </summary>
        /// <remarks>
        /// <para>Range: 0 - 0xFFFF [read-only].</para>
        /// <para>Default: Factory-set.</para>
        /// </remarks>
        [AtString("VR")]
        FirmwareVersion = 0x5652,

        /// <summary>
        /// Read hardware version of the RF module.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.x80.
        /// <para>Range: 0 - 0xFFFF [read-only].</para>
        /// <para>Default: Factory-set.</para>
        /// </remarks>
        [AtString("HV")]
        HardwareVersion = 0x4856,

        /// <summary>
        /// Disable/Enable API Mode.
        /// </summary>
        [AtString("AP")]
        ApiEnable = 0x4150,

        /// <summary>
        /// Forces a read of all enabled digital and analog input lines.
        /// </summary>
        [AtString("IS")]
        ForceSample = 0x4953,

        /// <summary>
        /// Read the network address of the module.
        /// </summary>
        [AtString("MY")]
        NetworkAddress = 0x4D59,

        /// <summary>
        /// Restore module parameters to factory defaults.
        /// </summary>
        [AtString("RE")]
        RestoreDefaults = 0x5245,

        /// <summary>
        /// Reset module.
        /// </summary>
        [AtString("FR")]
        SoftwareReset = 0x4652,

        /// <summary>
        /// Reset network layer parameters.
        /// </summary>
        [AtString("NR")]
        NetworkReset = 0x4E52,

        /// <summary>
        /// Applies changes to all command registers causing queued command 
        /// register values to be applied.
        /// </summary>
        [AtString("AC")]
        ApplyChanges = 0x4143
    }
}