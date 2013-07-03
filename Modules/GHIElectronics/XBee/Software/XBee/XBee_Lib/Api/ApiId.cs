namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// In API frame <see cref="ApiId"/> indicates which API message will be contained in the packet payload.
    /// </summary>
    public enum ApiId
    {
        /// <summary>
        /// API ID: 0x00
        /// <para>Transmit data as an RF Packet to 64-bit address destination.</para>
        /// </summary>
        TxRequest64 = 0x00,

        /// <summary>
        /// API ID: 0x01
        /// <para>Transmit data as an RF Packet to 16-bit address destination.</para>
        /// </summary>
        TxRequest16 = 0x01,

        /// <summary>
        /// API ID: 0x08
        /// <para>Allows for module parameters to be queried or set.</para>
        /// </summary>
        /// <remarks>
        /// When using this command ID, new parameter values are applied immediately. 
        /// This includes any register set with the <see cref="AtCommandQueue"/> API type.
        /// </remarks>
        AtCommand = 0x08,

        /// <summary>
        /// API ID: 0x09
        /// <para>Allows for module parameters to be queried or set.</para>
        /// </summary>
        /// <remarks>
        /// In contrast to the <see cref="AtCommand"/> API type, new parameter values 
        /// are queued and not applied until either the <see cref="AtCommand"/> 
        /// API type or the <see cref="Common.AtCmd.ApplyChanges"/> command is issued. 
        /// Register queries (reading parameter values) are returned immediately
        /// </remarks>
        AtCommandQueue = 0x09,

        /// <summary>
        /// API ID: 0x10
        /// <para>Send data as an Zigbee RF packet to the specified destination.</para>
        /// </summary>
        /// <remarks>
        /// The 64-bit destination address should be set to 0x000000000000FFFF for a broadcast transmission (to all devices). 
        /// The coordinator can be addressed by either setting the 64-bit address to all 0x00s and the 16-bit address to 0xFFFE, 
        /// OR by setting the 64-bit address to the coordinator's 64-bit address and the 16-bit address to 0x0000. 
        /// For all other transmissions, setting the 16-bit address to the correct 16-bit address can help improve performance 
        /// when transmitting to multiple destinations. If a 16-bit address is not known, this field should be set to 0xFFFE (unknown). 
        /// A <see cref="TxStatusResponse"/> packet will indicate the discovered 16-bit address, if successful.
        /// </remarks>
        ZnetTxRequest = 0x10,

        /// <summary>
        /// API ID: 0x11
        /// <para>Allows ZigBee application layer fields (endpoint and cluster ID) to be specified for a data transmission.</para>
        /// </summary>
        /// <remarks>
        /// Similar to <see cref="ZnetTxRequest"/>, but also requires ZigBee application layer addressing fields to be
        /// specified (endpoints, cluster ID, profile ID). An <see cref="ZnetExplicitTxRequest"/> frame causes the module to
        /// send data as an RF packet to the specified destination, using the specified source and destination endpoints,
        /// cluster ID, and profile ID.
        /// </remarks>
        ZnetExplicitTxRequest = 0x11,

        /// <summary>
        /// API ID: 0x17
        /// <para>Allows for remote module parameters to be queried or set.</para>
        /// </summary>
        /// <remarks>
        /// For parameter changes on the remote device to take effect, changes must be applied, either by setting the 
        /// apply changes options bit, or by sending an <see cref="Common.AtCmd.ApplyChanges"/> command to the remote.
        /// </remarks>
        RemoteAtCommand = 0x17,

        /// <summary>
        /// API ID: 0x21
        /// <para>Creates a source route in the module.</para>
        /// </summary>
        /// <remarks>
        /// <para>A source route specifies the complete route a packet should traverse to get from source to destination. 
        /// Source routing should be used with many-to-one routing for best results.</para>
        /// <para>Note: Both the 64-bit and 16-bit destination addresses are required when creating a source route. 
        /// These are obtained when a <see cref="RouteRecord"/> frame is received.</para>
        /// </remarks>
        CreateSourceRoute = 0x21,

        /// <summary>
        /// API ID: 0x24
        /// <para>Registers a new device into the trust center's key table.</para>
        /// </summary>
        /// <remarks>
        /// A KY command can be used to set the new device’s initial link key.
        /// </remarks>
        RegisterDevice = 0x24,

        /// <summary>
        /// API ID: 0x80
        /// <para>RF packet received from 64-bit address sender.</para>
        /// </summary>
        Rx64Response = 0x80,

        /// <summary>
        /// API ID: 0x81
        /// <para>RF packet received from 16-bit address sender.</para>
        /// </summary>
        Rx16Response = 0x81,

        /// <summary>
        /// API ID: 0x82
        /// <para>I/O data received from 64-bit sender.</para>
        /// </summary>
        Rx64IoResponse = 0x82,

        /// <summary>
        /// API ID: 0x83
        /// <para>I/O data received from 16-bit sender.</para>
        /// </summary>
        Rx16IoResponse = 0x83,

        /// <summary>
        /// API ID: 0x88
        /// <para>Response to previous <see cref="AtCommand"/> command.</para>
        /// </summary>
        /// <remarks>
        /// In response to a <see cref="AtCommand"/>, the module will send a <see cref="AtResponse"/>.
        /// Some commands will send back multiple frames (<see cref="Common.AtCmd.NodeDiscover"/> or <see cref="Wpan.AtCmd.ActiveScan"/>). 
        /// These commands will end by sending a frame with <see cref="Api.AtResponse.IsOk"/> set to <c>true</c> and empty payload.
        /// </remarks>
        AtResponse = 0x88,

        /// <summary>
        /// API ID: 0x89
        /// <para>Result of sending an RF packet.</para>
        /// </summary>
        /// <remarks>
        /// When <see cref="TxRequest16"/> or <see cref="TxRequest64"/> is completed, 
        /// the module sends a <see cref="TxStatusResponse"/>. This message will indicate 
        /// if the packet was transmitted successfully or if there was a failure.
        /// </remarks>
        TxStatusResponse = 0x89,

        /// <summary>
        /// API ID: 0x8A
        /// <para>Module current status.</para>
        /// </summary>
        /// <remarks>
        /// RF module status messages are sent from the module in response to specific conditions like reboot.
        /// </remarks>
        ModemStatusResponse = 0x8A,

        /// <summary>
        /// API ID: 0x8B
        /// <para>Result of sending an Zigbee RF packet.</para>
        /// </summary>
        /// <remarks>
        /// When <see cref="ZnetTxRequest"/> or <see cref="ZnetExplicitTxRequest"/> is completed, 
        /// the module sends a <see cref="ZnetTxStatusResponse"/>. This message will indicate 
        /// if the packet was transmitted successfully or if there was a failure.
        /// </remarks>
        ZnetTxStatusResponse = 0x8B,

        /// <summary>
        /// API ID: 0x90
        /// <para>Zigbee RF packet received from remote sender.</para>
        /// </summary>
        ZnetRxResponse = 0x90,

        /// <summary>
        /// API ID: 0x91
        /// <para>Explicit Zigbee RF packet received from remote sender.</para>
        /// </summary>
        ZnetExplicitRxResponse = 0x91,

        /// <summary>
        /// API ID: 0x92
        /// <para>I/O packet received from remote sender.</para>
        /// </summary>
        ZnetIoSampleResponse = 0x92,

        /// <summary>
        /// API ID: 0x94
        /// <para>Zigbee sensor packet from a Digi 1-wire sensor adapter.</para>
        /// </summary>
        ZnetSensorResponse = 0x94,

        /// <summary>
        /// API ID: 0x95
        /// <para>Received when a module transmits a node identification message to identify itself.</para>
        /// </summary>
        /// <remarks>
        /// The data portion of this frame is similar to <see cref="Common.AtCmd.NodeDiscover"/> frame.
        /// </remarks>
        ZnetNodeIdentifierResponse = 0x95,

        /// <summary>
        /// API ID: 0x97
        /// <para>Response to previous <see cref="RemoteAtCommand"/> command.</para>
        /// </summary>
        RemoteAtResponse = 0x97,

        /// <summary>
        /// API ID: 0xA0
        /// <para>Status indication of Over-the-Air firmware update transmission attempt.</para>
        /// </summary>
        FirwareUpdateStatus = 0xA0,

        /// <summary>
        /// API ID: 0xA1
        /// <para>Received whenever a device sends a ZigBee route record command.</para>
        /// </summary>
        /// <remarks>
        /// This is used with many-to-one routing to create source routes for devices in a network.
        /// </remarks>
        RouteRecord = 0xA1,

        /// <summary>
        /// API ID: 0xA2
        /// <para>Received whenever a device is authenticated by Trust Center.</para>
        /// </summary>
        /// <remarks>
        /// This frame is sent out the UART of the Trust Center when a new device 
        /// is authenticated on a Smart Energy network.
        /// </remarks>
        DeviceAuthenticatedIndicator = 0xA2,

        /// <summary>
        /// API ID: 0xA3
        /// <para>Many-to-one route request indicator frame.</para>
        /// </summary>
        ManyToOneRouteRequest = 0xA3,

        /// <summary>
        /// API ID: 0xA4
        /// <para>Received whenever a device is authenticated by Trust Center.</para>
        /// </summary>
        /// <remarks>
        /// This frame is sent out the UART of the Trust Center when a new device 
        /// is authenticated on a Smart Energy network.
        /// </remarks>
        RegisterJoiningDeviceStatus = 0xA4,

        /// <summary>
        /// API ID: 0xA5
        /// <para>Received whenever a device attempts to join, rejoin, or leave the network.</para>
        /// </summary>
        /// <remarks>
        /// This frame is sent out the UART of the Trust Center when a device attempts to join, rejoin, or leave the network. 
        /// It is enabled by setting bit 0x02 in the DO register.
        /// </remarks>
        JoinNotificationStatus = 0xA5,

        /// <summary>
        /// Unknown API ID value.
        /// </summary>
        /// <remarks>
        /// Indicates that we've parsed a packet for which we didn't 
        /// know how to handle the API type. This will be parsed 
        /// into a GenericResponse   
        /// </remarks>
        Unknown = 0xFF,

        /// <summary>
        /// Error occured while parsing packet.
        /// </summary>
        /// <remarks>
        /// This is returned if an error occurs during packet parsing
        /// and does not correspond to a XBee API ID.
        /// </remarks>
        ErrorResponse = -1
    }
}