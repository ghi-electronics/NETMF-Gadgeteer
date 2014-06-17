namespace NETMF.OpenSource.XBee.Api.Wpan
{
    /// <summary>
    /// Value returned by WpanAtCmd.AssociationIndication command.
    /// </summary>
    public enum AssociationStatus
    {
        /// <summary>
        /// Coordinator successfully started or End Device association complete.
        /// </summary>
        Success = 0x00,

        /// <summary>
        /// Active Scan Timeout.
        /// </summary>
        ActiveScanTimeout = 0x01,

        /// <summary>
        /// Active Scan found no PANs.
        /// </summary>
        NoPan = 0x02,

        /// <summary>
        /// Active Scan found PAN, but the CoordinatorAllowAssociation bit is not set.
        /// </summary>
        NoAssociation = 0x03,

        /// <summary>
        /// Active Scan found PAN, but Coordinator and End Device are not configured to support beacons.
        /// </summary>
        BeaconsNotSupported = 0x04,

        /// <summary>
        /// Active Scan found PAN, but the Coordinator ID parameter does not match the ID parameter of the End Device.
        /// </summary>
        InvalidPanId = 0x05,

        /// <summary>
        /// Active Scan found PAN, but the Coordinator CH parameter does not match the CH parameter of the End Device.
        /// </summary>
        InvalidChannel = 0x06,

        /// <summary>
        /// Energy Scan Timeout.
        /// </summary>
        EnergyScanTimeout = 0x07,

        /// <summary>
        /// Coordinator start request failed.
        /// </summary>
        StartFailed = 0x08,

        /// <summary>
        /// Coordinator could not start due to invalid parameter.
        /// </summary>
        InvalidParam = 0x09,

        /// <summary>
        /// Coordinator Realignment is in progress.
        /// </summary>
        RealignmentInProgress = 0x0A,

        /// <summary>
        /// Association Request not sent.
        /// </summary>
        RequestNotSent = 0x0B,

        /// <summary>
        /// Association Request timed out - no reply was received.
        /// </summary>
        RequestTimeout = 0x0C,

        /// <summary>
        /// Association Request had an Invalid Parameter.
        /// </summary>
        RequestInvalidParam = 0x0D,

        /// <summary>
        /// Association Request Channel Access Failure. Request was not transmitted - CCA failure.
        /// </summary>
        RequestChannelFailure = 0x0E,

        /// <summary>
        /// Remote Coordinator did not send an ACK after Association Request was sent.
        /// </summary>
        NoAck = 0x0F,

        /// <summary>
        /// Remote Coordinator did not reply to the Association Request, 
        /// but an ACK was received after sending the request.
        /// </summary>
        NoReply = 0x10,

        /// <summary>
        /// Lost synchronization with a Beaconing Coordinator.
        /// </summary>
        SyncLost = 0x12,

        /// <summary>
        /// No longer associated to Coordinator.
        /// </summary>
        Disassociated = 0x13,

        /// <summary>
        /// RF Module is attempting to associate.
        /// </summary>
        AssociationInProgress = 0xFF
    }
}