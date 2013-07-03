namespace NETMF.OpenSource.XBee.Api.Zigbee
{
  /// <summary>
  /// Association Status  
  /// </summary>
  /// <remarks>
  /// TODO: Update  comments    
  /// </remarks>
    public enum AssociationStatus
    {
        /// <summary>
        /// Successfully formed or joined a network. 
        /// </summary>
        /// <remarks>
        /// Coordinators form a network, routers and end devices join a network.
        /// </remarks>
        Success = 0,

        /// <summary>
        /// Scan found no PANs
        /// </summary>
	      NoPan = 0x21,

	      /// <summary>
	      /// Scan found no valid PANs based on current SC and ID settings.
	      /// </summary>
        NoValidPan = 0x22,

	      /// <summary>
	      /// Valid Coordinator or Routers found, but they are not allowing joining (NJ expired).
	      /// </summary>
        NodeJoiningExpired = 0x23,

        /// <summary>
        /// No joinable beacons were found.
        /// </summary>
        NoJoinableBeacons = 0x24,

        /// <summary>
        /// Node should not be attempting to join at this time.
        /// </summary>
        UnexpectedState = 0x25,

	      /// <summary>
	      /// Node Joining attempt failed (typically due to incompatible security settings).
	      /// </summary>
        JoiningFailed = 0x27,

        /// <summary>
        /// Coordinator Start attempt failed.
        /// </summary>
	      CoordinatorStartFailed = 0x2A,

        /// <summary>
        /// Checking for an existing coordinator.
        /// </summary>
        CheckingForCoordinator = 0x2B,

        /// <summary>
        /// Attempt to leave the network failed.
        /// </summary>
        FailedToLeaveNetwork = 0x2C,

        /// <summary>
        /// Attempted to join a device that did not respond.
        /// </summary>
        FailedToJoinDevice = 0xAB,

        /// <summary>
        /// Network security key received unsecured.
        /// </summary>
        ReceivedUnsecuredKey = 0xAC,

        /// <summary>
        /// Network security key not received.
        /// </summary>
        KeyNotReceived = 0xAD,

        /// <summary>
        /// Joining device does not have the right preconfigured link key.
        /// </summary>
        MissingKey = 0xAF,

        /// <summary>
        /// Scanning for a ZigBee network (routers and end devices).
        /// </summary>
        ScanningInProgress = 0xFF,
    }
}