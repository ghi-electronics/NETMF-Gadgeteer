using NETMF.OpenSource.XBee.Api.Common;

namespace NETMF.OpenSource.XBee.Api.Wpan
{
    /// <summary>
    /// AT commands for IEEE® 802.15.4 RF Modules by Digi International.
    /// </summary>
    /// <remarks>
    /// Based on manual from Digi.
    /// <para>Document name: 90000982_F.</para>
    /// <para>Document version: 1/11/2012.</para>
    /// <para>Firwmare version: v1.xED.</para>
    /// </remarks>
    public enum AtCmd : ushort
    {
        #region Special

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
        /// Restore module parameters to factory defaults.
        /// </summary>
        [AtString("RE")]
        RestoreDefaults = 0x5245,

        /// <summary>
        /// Responds immediately with an OK then performs 
        /// a hard reset ~100ms later.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.x80.
        /// </remarks>
        [AtString("FR")]
        SoftwareReset = 0x4652,

        #endregion

        #region Adressing

        /// <summary>
        /// Set/Read the channel number used for transmitting and 
        /// receiving data between RF modules.
        /// </summary>
        /// <remarks>
        /// <para>Range: 0x0B - 0x1A (XBee), 0x0C - 0x17 (XBee-PRO).</para>
        /// <para>Default: 0x0C (12 dec).</para>
        /// </remarks>
        [AtString("CH")]
        Channel = 0x4348,

        /// <summary>
        /// Set/Read the PAN (Personal Area Network) ID.
        /// </summary>
        /// <remarks>
        /// Use 0xFFFF to broadcast messages to all PANs.
        /// <para>Range: 0 - 0xFFFF.</para>
        /// <para>Default: 0x3332 (13106 dec).</para>
        /// </remarks>
        [AtString("ID")]
        PanId = 0x4944,

        /// <summary>
        /// Set/Read the upper 32 bits of the 64-bit destination address.
        /// </summary>
        /// <remarks>
        /// When combined with DL, it defines the destination address used for transmission. 
        /// To transmit using a 16-bit address, set DH parameter to zero and DL less than 0xFFFF.
        /// 0x000000000000FFFF is the broadcast address for the PAN.
        /// <para>Range: 0 - 0xFFFFFFFF.</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("DH")]
        DestinationAddressHigh = 0x4448,

        /// <summary>
        /// Set/Read the lower 32 bits of the 64-bit destination address.
        /// </summary>
        /// <remarks>
        /// When combined with DH, DL defines the destination address used for transmission. 
        /// To transmit using a 16-bit address, set DH parameter to zero and DL less than 0xFFFF. 
        /// 0x000000000000FFFF is the broadcast address for the PAN.
        /// <para>Range: 0 - 0xFFFFFFFF.</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("DL")]
        DestinationAddressLow = 0x444C,

        /// <summary>
        /// Set/Read the RF module 16-bit source address.
        /// </summary>
        /// <remarks>
        /// Set MY = 0xFFFF to disable reception of packets with 16-bit addresses. 64-bit source 
        /// address (serial number) and broadcast address (0x000000000000FFFF) is always enabled.
        /// <para>Range: 0 - 0xFFFF.</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("MY")]
        SourceAddress = 0x4D59,

        /// <summary>
        /// Read high 32 bits of the RF module's unique IEEE 64-bit address.
        /// </summary>
        /// <remarks>
        /// 64-bit source address is always enabled.
        /// <para>Range: 0 - 0xFFFFFFFF (read-only).</para>
        /// <para>Default: factory-set.</para>
        /// </remarks>
        [AtString("SH")]
        SerialNumberHigh = 0x5348,

        /// <summary>
        /// Read low 32 bits of the RF module's unique IEEE 64-bit address.
        /// </summary>
        /// <remarks>
        /// 64-bit source address is always enabled.
        /// <para>Range: 0 - 0xFFFFFFFF (read-only).</para>
        /// <para>Default: factory-set.</para>
        /// </remarks>
        [AtString("SL")]
        SerialNumberLow = 0x534C,

        /// <summary>
        /// Set/Read the maximum number of retries the module will execute in
        /// addition to the 3 retries provided by the 802.15.4 MAC.
        /// </summary>
        /// <remarks>
        /// For each XBee retry, the 802.15.4 MAC can execute up to 3 retries.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 6.</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("RR")]
        XBeeRetries = 0x5252,

        /// <summary>
        /// Set/Read the minimum value of the back-off exponent in the CSMA-CA 
        /// algorithm that is used for collision avoidance.
        /// </summary>
        /// <remarks>
        /// If RN = 0, collision avoidance is disabled during the first iteration 
        /// of the algorithm (802.15.4 - macMinBE).
        /// <para>Range: 0 - 3 (exponent).</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("RN")]
        RandomDelaySlots = 0x524E,

        /// <summary>
        /// Set/Read MAC Mode value. MAC Mode enables/disables the
        /// use of a Digi header in the 802.15.4 RF packet.
        /// </summary>
        /// <remarks>
        /// When Modes 1 or 3 are enabled (MM=1,3), duplicate packet detection is enabled 
        /// as well as certain AT commands.Please see the detailed MM description 
        /// on page 47 for additional information. Introduced in firmware v1.x80.
        /// <para>Range: 
        /// 0 (Digi Mode), 
        /// 1 (802.15.4 no ACKs), 
        /// 2 (802.15.4 with ACKs), 
        /// 3 = (Digi Mode no ACKs)
        /// </para>
        /// <para>Default: 0</para>
        /// </remarks>
        [AtString("MM")]
        MacMode = 0x4D4D,

        /// <summary>
        /// Set/Read node string identifier.
        /// </summary>
        /// <remarks>
        /// The register only accepts printable ASCII data. In AT Command Mode, a string 
        /// can not start with a space. A carriage return ends the command. Command will 
        /// automatically end when maximum bytes for the string have been entered. 
        /// This string is returned as part of the <see cref="NodeDiscover"/> command. 
        /// This identifier is also used with the <see cref="DestinationNode"/> command. 
        /// Introduced in firmware v1.x80.
        /// <para>Range: 20-character ASCII string.</para>
        /// <para>Default: empty.</para>
        /// </remarks>
        [AtString("NI")]
        NodeIdentifier = 0x4E49,

        /// <summary>
        /// Discover and report all RF modules found.
        /// </summary>
        /// <remarks>
        /// After (<see cref="NodeDiscoverTime"/> * 100) milliseconds, the command ends 
        /// by returning a carrage return. <see cref="NodeDiscover"/> also accepts a 
        /// <see cref="NodeIdentifier"/> as a parameter (optional). In this case, only a module 
        /// that matches the supplied identifier will respond. If <see cref="NodeDiscover"/> 
        /// is sent through the API, each response is returned as a separate response. 
        /// The data consists of the above listed bytes without the carriage return delimiters. 
        /// The <see cref="NodeIdentifier"/> string will end in a "0x00" null character. 
        /// The radius of the <see cref="NodeDiscover"/> command is set by the BroadcastHops command. 
        /// Introduced in firmware v1.x80.
        /// <para>Range: optional 20-character NI value.</para>
        /// </remarks>
        [AtString("ND")]
        NodeDiscover = 0x4E44,

        /// <summary>
        /// Set/Read the node discovery timeout.
        /// </summary>
        /// <remarks>
        /// When the network discovery (<see cref="NodeDiscover"/>) command is issued, 
        /// the <see cref="NodeDiscoverTime"/> value is included in the transmission to
        /// provide all remote devices with a response timeout. Remote devices wait a random
        /// time, less than NT, before sending their response. Introduced in firmware v1.xA0.
        /// <para>Range: 0x01 - 0xFC (x 100 ms).</para>
        /// <para>Default: 0x19 (25 dec).</para>
        /// </remarks>
        [AtString("NT")]
        NodeDiscoverTime = 0x4E54,

        /// <summary>
        /// Enables node discover self-response on the module.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1xC5.
        /// <para>Range: 0 - 1.</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("NO")]
        NetworkDiscoveryOptions = 0x4E4F,

        /// <summary>
        /// Resolves an <see cref="NodeIdentifier"/> string to a physical address.
        /// </summary>
        /// <remarks>
        /// The following events occur upon successful command execution. <see cref="DestinationAddressHigh"/> 
        /// and <see cref="DestinationAddressLow"/> are set to the address of the module with 
        /// the matching <see cref="NodeIdentifier"/>. "OK" is returned. RF module automatically exits 
        /// AT Command Mode. If there is no response from a module within 200 msec or a parameter is not specified
        /// (left blank), the command is terminated and an "ERROR" message is returned. 
        /// Introduced in firmware v1.x80.
        /// <para>Range: 20-character ASCII string.</para>
        /// <para>Default: empty.</para>
        /// </remarks>
        [AtString("DN")]
        DestinationNode = 0x444E,

        /// <summary>
        /// Set/Read the coordinator setting.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.x80.
        /// <para>Range: 0 (End Device), 1 (Coordinator).</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("CE")]
        CoordinatorEnable = 0x4345,

        /// <summary>
        /// Set/Read list of channels to scan for all Active and Energy Scans as a bitfield.
        /// </summary>
        /// <remarks>
        /// This affects scans initiated in command mode (AS, ED) and during End Device Association 
        /// and Coordinator startup. Introduced in firmware v1.x80.
        /// <para>Range: 0 - 0xFFFF [bitfield] (bits 0, 14, 15 not allowed on the XBee-PRO)</para>
        /// <para>Default: 0x1FFE (all XBee-PRO Channels)</para>
        /// </remarks>
        [AtString("SC")]
        ScanChannels = 0x5343,

        /// <summary>
        /// Set/Read the scan duration exponent.
        /// </summary>
        /// <remarks>
        /// Time equals to (2 ^ SD) * 15.36ms. Introduced in firmware v1.x80.
        /// <para>End Device - Duration of Active Scan during Association.</para>
        /// <para>Coordinator - If ReassignPANID option is set on Coordinator (refer to A2 parameter),
        /// SD determines the length of time the Coordinator will scan channels to locate existing
        /// PANs. If ReassignChannel option is set, SD determines how long the Coordinator will
        /// perform an Energy Scan to determine which channel it will operate on. Scan Time is measured 
        /// as (# of channels to scan] * (2 ^ SD) * 15.36ms). The number of channels to scan is set 
        /// by the <see cref="ScanChannels"/> command. The XBee can scan up to 16 channels. 
        /// The XBee PRO can scan up to 13 channels.</para>
        /// <para>Range: 0 - 0x0F [exponent].</para>
        /// <para>Default: 4.</para>
        /// </remarks>
        /// <example>
        /// The values below show results for a 13 channel scan:
        /// <list type="bullet">
        /// <item><term>SD = 0</term><description>time = 0.18 sec</description></item>
        /// <item><term>SD = 2</term><description>time = 0.74 sec</description></item>
        /// <item><term>SD = 4</term><description>time = 2.95 sec</description></item>
        /// <item><term>SD = 6</term><description>time = 11.80 sec</description></item>
        /// <item><term>SD = 8</term><description>time = 47.19 sec</description></item>
        /// <item><term>SD = 10</term><description>time = 3.15 min</description></item>
        /// <item><term>SD = 12</term><description>time = 12.58 min</description></item>
        /// <item><term>SD = 14</term><description>time = 50.33 min</description></item>
        /// </list>
        /// </example>
        [AtString("SD")]
        ScanDuration = 0x5344,

        /// <summary>
        /// Set/Read End Device association options.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.x80.
        /// <list type="bullet">
        /// <item>
        /// <term>bit 0 - ReassignPanIDƒ</term>
        /// <description>
        /// 0 - Will only associate with Coordinator operating on PAN ID that matches module IDƒ. 
        /// 1 - May associate with Coordinator operating on any PAN ID.
        /// </description>
        /// </item>
        /// <item>
        /// <term>bit 1 - ReassignChannel</term>
        /// <description>
        /// 0 - Will only associate with Coordinator operating on matching CH Channel settingƒn. 
        /// 1 - May associate with Coordinator operating on any Channel.
        /// </description>
        /// </item>
        /// <item>
        /// <term>bit 2 - AutoAssociateƒ</term>
        /// <description>
        /// 0 - Device will not attempt Association. 
        /// 1 - Device attempts Association until success. 
        /// This bit is used only for Non-Beacon systems. 
        /// End Devices in Beacon-enabled system must always associate to a Coordinator.
        /// </description>
        /// </item>
        /// <item>
        /// <term>bit 3 - PollCoordOnPinWake</term>
        /// <description>
        /// 0 - Pin Wake will not poll the Coordinator for indirect (pending) data. 
        /// 1 - Pin Wake will send Poll Request to Coordinator to extract any pending data bits 4 - 7 are reserved.
        /// </description>
        /// </item>
        /// </list>
        /// <para>Range: 0 - 0x0F [bitfield].</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("A1")]
        EndDeviceAssociation = 0x4131,

        /// <summary>
        /// Set/Read Coordinator association options.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.x80.
        /// <list type="bullet">
        /// <item>
        /// <term>bit 0 - ReassignPanIDƒ</term>
        /// <description>
        /// 0 - Coordinator will not perform Active Scan to locate available PAN ID. It will operate on ID (PAN ID). 
        /// 1 - Coordinator will perform Active Scan to determine an available ID (PAN ID). If a PAN ID conflict 
        /// is found, the ID parameter will change.
        /// </description>
        /// </item>
        /// <item>
        /// <term>bit 1 - ReassignChannel</term>
        /// <description>
        /// 0 - Coordinator will not perform Energy Scan to determine free channel. It will operate on the channel 
        /// determined by the <see cref="Channel"/> parameter. 
        /// 1 - Coordinator will perform Energy Scan to find a free channel, then operate on that channel.
        /// </description>
        /// </item>
        /// <item>
        /// <term>bit 2 - AllowAssociationƒ</term>
        /// <description>
        /// 0 - Coordinator will not allow any devices to associate to it. 
        /// 1 - Coordinator will allow devices to associate to it.
        /// </description>
        /// </item>
        /// <item>
        /// <term>bits 3 - 7</term>
        /// <description>Reserved.</description>
        /// </item>
        /// </list>
        /// <para>Range: 0 - 7 [bitfield].</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("A2")]
        CoordinatorAssociation = 0x4132,

        /// <summary>
        /// Read errors with the last association request.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.x80.
        /// <para>Range: 0 - 0x13 [read-only].</para>
        /// </remarks>
        /// <returns><see cref="AssociationStatus"/></returns>
        [AtString("AI")]
        AssociationIndication = 0x4149,

        /// <summary>
        /// End Device will immediately disassociate from a Coordinator 
        /// (if associated) and reattempt to associate.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.x80.
        /// </remarks>
        [AtString("DA")]
        ForceDisassociation = 0x4441,

        /// <summary>
        /// Request indirect messages being held by a coordinator.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.x80
        /// </remarks>
        [AtString("FP")]
        ForcePool = 0x4650,

        /// <summary>
        /// Send Beacon Request to Broadcast Address (0xFFFF) and Broadcast PAN (0xFFFF) on every channel.
        /// </summary>
        /// <remarks>
        /// The parameter determines the time the radio will listen for Beacons on each channel. 
        /// A PanDescriptor is created and returned for every Beacon received from the scan. 
        /// The Active Scan is capable of returning up to 5 PanDescriptors in a scan. 
        /// The actual scan time on each channel is measured as Time = [(2 ^SD PARAM) * 15.36] ms. 
        /// Note the total scan time is this time multiplied by the number of channels to be scanned 
        /// (16 for the XBee and 13 for the XBee-PRO). Also refer to <see cref="ScanDuration"/> command description. 
        /// Introduced in firmware v1.x80.
        /// <para>Range: 0 - 6.</para>
        /// </remarks>
        [AtString("AS")]
        ActiveScan = 0x4153,

        /// <summary>
        /// Send an Energy Detect Scan.
        /// </summary>
        /// <remarks>
        /// The parameter determines the length of scan on each channel. The maximal energy on each channel 
        /// is returned and each value is followed by a carriage return. Values returned represent detected 
        /// energy levels in units of -dBm. Actual scan time on each channel is measured as Time = [(2 ^ SD) * 15.36] ms. 
        /// Total scan time is this time multiplied by the number of channels to be scanned. Introduced in firmware v1.x80
        /// <para>Range: 0 - 6.</para>
        /// </remarks>
        [AtString("ED")]
        EnergyScan = 0x4544,

        /// <summary>
        /// Disable/Enable 128-bit AES encryption support. 
        /// Use in conjunction with the <see cref="AesEncryptionKey"/> command.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 1.</para>
        /// <para>Default: 0 (disabled).</para>
        /// </remarks>
        [AtString("EE")]
        AesEncryptionEnable = 0x4545,

        /// <summary>
        /// Set the 128-bit AES (Advanced Encryption Standard) key for encrypting/decrypting data. 
        /// The <see cref="AesEncryptionKey"/> register cannot be read.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.xA0
        /// <para>Range: 0 - (any 16-Byte value).</para>
        /// </remarks>
        [AtString("KY")]
        AesEncryptionKey = 0x4B59,

        #endregion

        #region RF Interfacing

        /// <summary>
        /// Select/Read the power level at which the RF module transmits conducted power.
        /// </summary>
        /// <remarks>
        /// <para>Range: 0 - 4.</para>
        /// <para>Default: 4.</para>
        /// </remarks>
        /// <returns><see cref="PowerLevel"/></returns>
        [AtString("PL")]
        PowerLevel = 0x504C,

        /// <summary>
        /// Set/read the CCA (Clear Channel Assessment) threshold.
        /// </summary>
        /// <remarks>
        /// Prior to transmitting a packet, a CCA is performed to detect energy on the channel. 
        /// If the detected energy is above the CCA Threshold, the module will not transmit the packet.
        /// Introduced in firmware v1.x80
        /// <para>Range: 0x24 - 0x50 [-dBm].</para>
        /// <para>Default: 0x2C (-44 dBm).</para>
        /// </remarks>
        [AtString("CA")]
        CcaThreshold = 0x4341,

        #endregion

        #region Sleep (Low Power)

        /// <summary>
        /// SM.
        /// <para>Set/Read Sleep Mode configurations.</para>
        /// </summary>
        /// <remarks>
        /// <para>Range: 0 - 5.</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        /// <returns><see cref="SleepMode"/></returns>
        [AtString("SM")]
        SleepMode = 0x534D,

        /// <summary>
        /// Set/Read the sleep mode options.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><term>Bit 0 - Poll wakeup disable.</term>
        /// <description>
        /// 0 - Normal operations. A module configured for cyclic sleep will poll for data on waking. 
        /// 1 - Disable wakeup poll. A module configured for cyclic sleep will not poll for data on waking.
        /// </description></item>
        /// <item><term>Bit 1 - ADC/DIO wakeup sampling disable.</term>
        /// <description>
        /// 0 - Normal operations. A module configured in a sleep mode with ADC/DIO sampling enabled will 
        /// automatically perform a sampling on wakeup. 1 - Suppress sample on wakeup. A module configured 
        /// in a sleep mode with ADC/DIO sampling enabled will not automatically sample on wakeup.
        /// </description></item>
        /// </list>
        /// <para>Range: 0 - 4.</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("SO")]
        SleepOptions = 0x534F,

        /// <summary>
        /// Set/Read time period of inactivity (no serial or RF data 
        /// is sent or received) before activating Sleep Mode
        /// </summary>
        /// <remarks>
        /// ST parameter is only valid with Cyclic Sleep settings (<see cref="SleepMode"/> = 4 - 5).
        /// Coordinator and End Device ST values must be equal.
        /// Also note, the GT parameter value must always be less than the ST value. (If GT > ST,
        /// the configuration will render the module unable to enter into command mode.) 
        /// If the ST parameter is modified, also modify the GT parameter accordingly.
        /// <para>Range: 1 - 0xFFFF [x 1 ms].</para>
        /// <para>Default: 0x1388 (5000 dec).</para>
        /// </remarks>
        [AtString("ST")]
        TimeBeforeSleep = 0x5354,

        /// <summary>
        /// Set/Read sleep period for cyclic sleeping remotes.
        /// </summary>
        /// <remarks>
        /// Coordinator and End Device SP values should always be equal. 
        /// To send Direct Messages, set SP = 0.
        /// <para>
        /// End Device - SP determines the sleep period for cyclic sleeping remotes. 
        /// Maximum sleep period is 268 seconds (0x68B0).
        /// </para>
        /// <para>
        /// Coordinator - If non-zero, SP determines the time to hold an indirect message before
        /// discarding it. A Coordinator will discard indirect messages after a period of (2.5 * SP).
        /// </para>
        /// <para>Range: 0 - 0x68B0 [x 10 ms].</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("SP")]
        CyclicSleepPeriod = 0x5350,

        /// <summary>
        /// Set/Read time period of sleep for cyclic sleeping remotes that are 
        /// configured for Association but are not associated to a Coordinator.
        /// </summary>
        /// <remarks>
        /// If a device is configured to associate, configured as a Cyclic Sleep remote, 
        /// but does not find a Coordinator, it will sleep for DP time before reattempting association. 
        /// Maximum sleepperiod is 268 seconds (0x68B0). DP should be > 0 for NonBeacon systems.
        /// Introduced in firmware v1.x80.
        /// <para>Range: 0 - 0x68B0 [x 10 ms].</para>
        /// <para>Default: 0x3E8 (1000 dec).</para>
        /// </remarks>
        [AtString("DP")]
        DisassociatedCyclicSleepPeriod = 0x4450,

        #endregion

        #region Serial Interfacing

        /// <summary>
        /// Set/Read the serial interface data rate for communication between
        /// the module serial port and host.
        /// </summary>
        /// <remarks>
        /// Request non-standard baud rates with values above 0x80 using a terminal window. 
        /// Read the BD register to find actual baud rate achieved.
        /// <para>Range: 
        /// 0 - 7 (standard baud rates), 
        /// 0x80 (non-standard baud rates up to 250 Kbps).
        /// </para>
        /// <para>Default: 3 (9600 Kbps).</para>
        /// </remarks>
        [AtString("BD")]
        InterfaceDataRate = 0x4244,

        /// <summary>
        /// Set/Read number of character times of inter-character delay required before transmission.
        /// </summary>
        /// <remarks>
        /// Set to zero to transmit characters as they arrive instead of buffering them into one RF packet.
        /// <para>Range: 0 - 0xFF [x character times]</para>
        /// <para>Default: 3.</para>
        /// </remarks>
        [AtString("RO")]
        PacketizationTimeout = 0x524F,

        /// <summary>
        /// Disable/Enable API Mode.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.x80.
        /// <para>Range: 0-2.</para>
        /// <para>Default: 0 (Disabled).</para>
        /// </remarks>
        [AtString("AP")]
        ApiEnable = 0x4150,

        /// <summary>
        /// Set/Read parity settings.
        /// </summary>
        /// <remarks>
        /// <para>Range: 0 - 4.</para>
        /// <para>Default: 0 (8-bit no parity).</para>
        /// </remarks>
        [AtString("NB")]
        Parity = 0x4E42,

        /// <summary>
        /// Set/Read bitfield to configure internal pull-up resistor status for I/O lines.
        /// </summary>
        /// <remarks>
        /// Bit set to '1' specifies pull-up enabled, '0' specifies no pull-up.
        /// Bitfield Map:
        /// bit 0 - AD4/DIO4 (pin11)
        /// bit 1 - AD3 / DIO3 (pin17)
        /// bit 2 - AD2/DIO2 (pin18)
        /// bit 3 - AD1/DIO1 (pin19)
        /// bit 4 - AD0 / DIO0 (pin20)
        /// bit 5 - RTS / AD6 / DIO6 (pin16)
        /// bit 6 - DTR / SLEEP_RQ / DI8 (pin9)
        /// bit 7 - DIN/CONFIG (pin3)
        /// Introduced in firmware v1.x80.
        /// <para>Range: 0 - 0xFF.</para>
        /// <para>Default: 0xFF.</para>
        /// </remarks>
        [AtString("PR")]
        PullUpResistorEnable = 0x5052,

        #endregion

        #region IO Settings

        /// <summary>
        /// Select/Read options for the DI8 line (pin 9) of the RF module.
        /// </summary>
        /// <remarks>
        /// <para>Range: 0 (Disabled), 3 (DI).</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("D8")]
        DIO8Config = 0x4438,

        /// <summary>
        /// Select/Read settings for the DIO7 line (pin 12) of the RF module.
        /// </summary>
        /// <remarks>
        /// Options include CTS flow control and I/O line settings.
        /// Introduced in firmware v1.x80.
        /// <para>Range: 
        /// 0 (Disabled), 
        /// 1 (CTS Flow Controlƒ), 
        /// 3 (DI), 
        /// 4 (DO low), 
        /// 5 (DO high), 
        /// 6 (RS485 Tx Enable Low), 
        /// 7 (RS485 Tx Enable High).
        /// </para>
        /// <para>Default: 1.</para>
        /// </remarks>
        [AtString("D7")]
        DIO7Config = 0x4437,

        /// <summary>
        /// Select/Read settings for the DIO6 line (pin 16) of the RF module.
        /// </summary>
        /// <remarks>
        /// Options include RTS flow control and I/O line settings.
        /// Introduced in firmware v1.x80.
        /// <para>Range: 
        /// 0 (Disabled), 
        /// 1 (RTS Flow Controlƒ), 
        /// 3 (DI), 
        /// 4 (DO low), 
        /// 5 (DO high).
        /// </para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("D6")]
        DIO6Config = 0x4436,

        /// <summary>
        /// Configure settings for the DIO5 line (pin 15) of the RF module.
        /// </summary>
        /// <remarks>
        /// Options include Associated LED indicator (blinks when associated) and I/O line settings.
        /// Introduced in firmware v1.x80.
        /// <para>Range: 
        /// 0 (Disabled), 
        /// 1 (Associated indicatorƒ), 
        /// 2 (ADC), 
        /// 3 (DI), 
        /// 4 (DO low), 
        /// 5 (DO high).
        /// </para>
        /// <para>Default: 1.</para>
        /// </remarks>
        [AtString("D5")]
        DIO5Config = 0x4435,

        /// <summary>
        /// Select/Read settings for the AD4/DIO4 (pin 11).
        /// </summary>
        /// <remarks>
        /// Options include: Analog-to-digital converter, Digital Input and Digital Output.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 
        /// 0 (Disabled), 
        /// 2 (ADC), 
        /// 3 (DI), 
        /// 4 (DO low), 
        /// 5 (DO high).
        /// </para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("D4")]
        DIO4Config = 0x4434,

        /// <summary>
        /// Select/Read settings for the AD3/DIO3 (pin 17).
        /// </summary>
        /// <remarks>
        /// Options include: Analog-to-digital converter, Digital Input and Digital Output.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 
        /// 0 (Disabled), 
        /// 2 (ADC), 
        /// 3 (DI), 
        /// 4 (DO low), 
        /// 5 (DO high).
        /// </para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("D3")]
        DIO3Config = 0x4433,

        /// <summary>
        /// Select/Read settings for the AD2/DIO2 (pin 18).
        /// </summary>
        /// <remarks>
        /// Options include: Analog-to-digital converter, Digital Input and Digital Output.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 
        /// 0 (Disabled), 
        /// 2 (ADC), 
        /// 3 (DI), 
        /// 4 (DO low), 
        /// 5 (DO high).
        /// </para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("D2")]
        DIO2Config = 0x4432,

        /// <summary>
        /// Select/Read settings for the AD1/DIO1 (pin 19).
        /// </summary>
        /// <remarks>
        /// Options include: Analog-to-digital converter, Digital Input and Digital Output.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 
        /// 0 (Disabled), 
        /// 2 (ADC), 
        /// 3 (DI), 
        /// 4 (DO low), 
        /// 5 (DO high).
        /// </para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("D1")]
        DIO1Config = 0x4431,

        /// <summary>
        /// Select/Read settings for the AD0/DIO0 (pin 20).
        /// </summary>
        /// <remarks>
        /// Options include: Analog-to-digital converter, Digital Input and Digital Output.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 
        /// 0 (Disabled), 
        /// 2 (ADC), 
        /// 3 (DI), 
        /// 4 (DO low), 
        /// 5 (DO high).
        /// </para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("D0")]
        DIO0Config = 0x4430,

        /// <summary>
        /// Disables/Enables I/O data received to be sent out UART.
        /// </summary>
        /// <remarks>
        /// The data is sent using an API frame regardless of the current AP parameter value.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 (Disabled), 1 (Enabled).</para>
        /// <para>Default: 1.</para>
        /// </remarks>
        [AtString("IU")]
        IOOutputEnable = 0x4955,

        /// <summary>
        /// Set/Read the number of samples to collect before transmitting data.
        /// </summary>
        /// <remarks>
        /// Maximum number of samples is dependent upon the number of enabled inputs.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 1 - 0xFF.</para>
        /// <para>Default: 1.</para>
        /// </remarks>
        [AtString("IT")]
        SamplesBeforeTx = 0x4954,

        /// <summary>
        /// Force a read of all enabled inputs (DI or ADC). Data is returned through the UART.
        /// </summary>
        /// <remarks>
        /// If no inputs are defined (DI or ADC), this command will return error.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 8-bit map (each bit represents the level of an I/O line setup as an output).</para>
        /// </remarks>
        [AtString("IS")]
        ForceSample = 0x4953,

        /// <summary>
        /// Set digital output level to allow DIO lines that are setup as 
        /// outputs to be changed through Command Mode.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.xA0.
        /// </remarks>
        [AtString("IO")]
        DigitalOutputLevel = 0x494F,

        /// <summary>
        /// Set/Read bitfield values for change detect monitoring.
        /// </summary>
        /// <remarks>
        /// Each bit enables monitoring of DIO0 - DIO7 for changes. If detected, data is transmitted with 
        /// DIO data only. Any samples queued waiting for transmission will be sent first.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFF [bitfield].</para>
        /// <para>Default: 0 (disabled).</para>
        /// </remarks>
        [AtString("IC")]
        DIOChangeDetect = 0x4943,

        /// <summary>
        /// Set/Read sample rate.
        /// </summary>
        /// <remarks>
        /// When set, this parameter causes the module to sample all enabled inputs at a specified interval.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFFFF [x 1 msec].</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("IR")]
        SampleRate = 0x4952,

        #endregion

        #region I/O Line Passing

        /// <summary>
        /// Set/Read addresses of module to which outputs are bound.
        /// </summary>
        /// <remarks>
        /// Setting all bytes to 0xFF will not allow any received I/O packet to change outputs. 
        /// Setting address to 0xFFFF will allow any received I/O packet to change outputs.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFFFFFFFFFFFFFFFF.</para>
        /// <para>Default: 0xFFFFFFFFFFFFFFFF.</para>
        /// </remarks>
        [AtString("IA")]
        IOInputAddress = 0x4941,

        /// <summary>
        /// Set/Read Output timeout values for corresponding digital line.
        /// </summary>
        /// <remarks>
        /// When output is set (due to I/O line passing) to a nondefault level, a timer is started which when 
        /// expired will set the output to it default level. The timer is reset when a valid I/O packet is received.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFF [x 100 ms].</para>
        /// <para>Default: 0xFF.</para>
        /// </remarks>
        [AtString("T0")]
        DIO0OutputTimeout = 0x5430,

        /// <summary>
        /// Set/Read Output timeout values for corresponding digital line.
        /// </summary>
        /// <remarks>
        /// When output is set (due to I/O line passing) to a nondefault level, a timer is started which when 
        /// expired will set the output to it default level. The timer is reset when a valid I/O packet is received.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFF [x 100 ms].</para>
        /// <para>Default: 0xFF.</para>
        /// </remarks>
        [AtString("T1")]
        DIO1OutputTimeout = 0x5431,

        /// <summary>
        /// Set/Read Output timeout values for corresponding digital line.
        /// </summary>
        /// <remarks>
        /// When output is set (due to I/O line passing) to a nondefault level, a timer is started which when 
        /// expired will set the output to it default level. The timer is reset when a valid I/O packet is received.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFF [x 100 ms].</para>
        /// <para>Default: 0xFF.</para>
        /// </remarks>
        [AtString("T2")]
        DIO2OutputTimeout = 0x5432,

        /// <summary>
        /// Set/Read Output timeout values for corresponding digital line.
        /// </summary>
        /// <remarks>
        /// When output is set (due to I/O line passing) to a nondefault level, a timer is started which when 
        /// expired will set the output to it default level. The timer is reset when a valid I/O packet is received.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFF [x 100 ms].</para>
        /// <para>Default: 0xFF.</para>
        /// </remarks>
        [AtString("T3")]
        DIO3OutputTimeout = 0x5433,

        /// <summary>
        /// Set/Read Output timeout values for corresponding digital line.
        /// </summary>
        /// <remarks>
        /// When output is set (due to I/O line passing) to a nondefault level, a timer is started which when 
        /// expired will set the output to it default level. The timer is reset when a valid I/O packet is received.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFF [x 100 ms].</para>
        /// <para>Default: 0xFF.</para>
        /// </remarks>
        [AtString("T4")]
        DIO4OutputTimeout = 0x5434,

        /// <summary>
        /// Set/Read Output timeout values for corresponding digital line.
        /// </summary>
        /// <remarks>
        /// When output is set (due to I/O line passing) to a nondefault level, a timer is started which when 
        /// expired will set the output to it default level. The timer is reset when a valid I/O packet is received.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFF [x 100 ms].</para>
        /// <para>Default: 0xFF.</para>
        /// </remarks>
        [AtString("T5")]
        DIO5OutputTimeout = 0x5435,

        /// <summary>
        /// Set/Read Output timeout values for corresponding digital line.
        /// </summary>
        /// <remarks>
        /// When output is set (due to I/O line passing) to a nondefault level, a timer is started which when 
        /// expired will set the output to it default level. The timer is reset when a valid I/O packet is received.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFF [x 100 ms].</para>
        /// <para>Default: 0xFF.</para>
        /// </remarks>
        [AtString("T6")]
        DIO6OutputTimeout = 0x5436,

        /// <summary>
        /// Set/Read Output timeout values for corresponding digital line.
        /// </summary>
        /// <remarks>
        /// When output is set (due to I/O line passing) to a nondefault level, a timer is started which when 
        /// expired will set the output to it default level. The timer is reset when a valid I/O packet is received.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFF [x 100 ms].</para>
        /// <para>Default: 0xFF.</para>
        /// </remarks>
        [AtString("T7")]
        DIO7OutputTimeout = 0x5437,

        /// <summary>
        /// Select/Read function for PWM0 pin.
        /// </summary>
        /// <remarks>
        /// <para>Range: 0 (Disabled), 1 (RSSI), 2 (PWM Output).</para>
        /// <para>Default: 1.</para>
        /// </remarks>
        [AtString("P0")]
        PWM0Config = 0x5030,

        /// <summary>
        /// Select/Read function for PWM0 pin.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 (Disabled), 1 (RSSI), 2 (PWM Output).</para>
        /// <para>Default: 0.</para>
        /// </remarks>
        [AtString("P1")]
        PWM1Config = 0x5031,

        /// <summary>
        /// Set/Read the PWM 0 output level.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0x03FF.</para>
        /// </remarks>
        [AtString("M0")]
        PWM0OutputLevel = 0x4D30,

        /// <summary>
        /// Set/Read the PWM 1 output level.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0x03FF.</para>
        /// </remarks>
        [AtString("M1")]
        PWM1OutputLevel = 0x4D31,

        /// <summary>
        /// Set/Read output timeout value for both PWM outputs.
        /// </summary>
        /// <remarks>
        /// When PWM is set to a non-zero value: Due to I/O line passing, a time is started which when 
        /// expired will set the PWM output to zero. The timer is reset when a valid I/O packet is received.
        /// Introduced in firmware v1.xA0.
        /// <para>Range: 0 - 0xFF [x 100 ms].</para>
        /// <para>Default: 0xFF.</para>
        /// </remarks>
        [AtString("PT")]
        PWMOutputTimeout = 0x5054,

        /// <summary>
        /// Set/Read PWM timer register.
        /// </summary>
        /// <remarks>
        /// Set the duration of PWM (pulse width modulation) signal output on the RSSI pin. 
        /// The signal duty cycle is updated with each received packet and is shut off when the timer expires.
        /// <para>Range: 0 - 0xFF [x 100 ms].</para>
        /// <para>Default: 0x28 (40 dec).</para>
        /// </remarks>
        [AtString("RP")]
        RssiPwmTimer = 0x5250,

        #endregion

        #region Diagnostics

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
        /// Read detailed version information (including application build date, MAC, PHY and bootloader versions).
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.x80. It has been deprecated in version 10C9. 
        /// It is not supported in firmware versions after 10C8.
        /// </remarks>
        [AtString("VL")]
        FirmwareVersionVerbose = 0x564C,

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
        /// Read signal level [in dB] of last good packet received (RSSI).
        /// </summary>
        /// <remarks>
        /// Absolute value is reported. (For example: 0x58 = -88 dBm) Reported value 
        /// is accurate between -40 dBm and RX sensitivity.
        /// <para>Range: 0x17-0x5C (XBee), 0x24-0x64 (XBee-PRO) [read-only].</para>
        /// </remarks>
        [AtString("DB")]
        ReceivedSignalStrength = 0x4442,

        /// <summary>
        /// Reset/Read count of CCA (Clear Channel Assessment) failures.
        /// </summary>
        /// <remarks>
        /// This parameter value increments when the module does not transmit a packet because it
        /// detected energy above the CCA threshold level set with CA command. This count saturates 
        /// at its maximum value. Set count to "0" to reset count. Introduced in firmware v1.x80.
        /// <para>Range: 0 - 0xFFFF.</para>
        /// </remarks>
        [AtString("EC")]
        CcaFailures = 0x4543,

        /// <summary>
        /// EC.
        /// <para>Reset/Read count of acknowledgment failures.</para>
        /// </summary>
        /// <remarks>
        /// This parameter value increments when the module expires its transmission retries without 
        /// receiving an ACK on a packet transmission. This count saturates at its maximum value. 
        /// Set the parameter to "0" to reset count. Introduced in firmware v1.x80.
        /// <para>Range: 0 - 0xFFFF.</para>
        /// </remarks>
		[AtString("EA")]
        AckFailures = 0x4541,

        //*********************************************
        // ED command is duplicated in Adressing region
        //*********************************************

        #endregion

        #region AT Command Options

        /// <summary>
        /// Set/Read the period of inactivity (no valid commands received) 
        /// after which the RF module automatically exits AT Command Mode and returns
        /// to Idle Mode.
        /// </summary>
        /// <remarks>
        /// <para>Range: 2 - 0xFFFF [x 100 ms].</para>
        /// <para>Default: 0x64 (100 dec).</para>
        /// </remarks>
        [AtString("CT")]
        CommandModeTimeout = 0x4354,

        /// <summary>
        /// Explicitly exit the module from AT Command Mode.
        /// </summary>
        [AtString("CN")]
        ExitCommandMode = 0x434E,

        /// <summary>
        /// Explicitly apply changes to queued parameter value(s) and reinitialize module.
        /// </summary>
        /// <remarks>
        /// Introduced in firmware v1.xA0.
        /// </remarks>
        [AtString("AC")]
        ApplyChanges = 0x4143,

        /// <summary>
        /// Set required period of silence before and after the Command Sequence
        /// Characters of the AT Command Mode Sequence (GT+ CC + GT).
        /// </summary>
        /// <remarks>
        /// The period of silence is used to prevent inadvertent entrance into AT Command Mode.
        /// <para>Range: 2 - 0x0CE4 [x 1 ms].</para>
        /// <para>Default: 0x3E8 (1000 dec).</para>
        /// </remarks>
        [AtString("GT")]
        GuardTimes = 0x4754,

        /// <summary>
        /// Set/Read the ASCII character value to be used between Guard Times of the AT Command Mode Sequence (GT+CC+GT).
        /// </summary>
        /// <remarks>
        /// The AT Command Mode Sequence enters the RF module into AT Command Mode.
        /// <para>Range: 0 - 0xFF.</para>
        /// <para>Default: 0x2B ('+' in ASCII).</para>
        /// </remarks>
        [AtString("CC")]
        CommandSequenceCharacter = 0x4343,

        #endregion
    }
}