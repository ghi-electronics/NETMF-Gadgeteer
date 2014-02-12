namespace NETMF.OpenSource.XBee.Api.Common
{
  /// <summary>
  ///  TODO: Update comments
  ///     
  /// </summary>
  /// <remarks>
  ///     
  /// </remarks>
    public class NodeInfo
    {
      /// <summary>
      ///   TODO: Update Comments
      ///     
      /// </summary>
      /// <value>
      ///     <para>
      ///         
      ///     </para>
      /// </value>
      /// <remarks>
      ///     
      /// </remarks>
        public XBeeAddress16 NetworkAddress { get; set; }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <value>
        ///     <para>
        ///         
        ///     </para>
        /// </value>
        /// <remarks>
        ///     
        /// </remarks>
        public XBeeAddress64 SerialNumber { get; set; }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <value>
        ///     <para>
        ///         
        ///     </para>
        /// </value>
        /// <remarks>
        ///     
        /// </remarks>
        public string NodeIdentifier { get; set; }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        public NodeInfo()
            : this(XBeeAddress64.Broadcast, XBeeAddress16.Unknown)
        {
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="serialNumber" type="NETMF.OpenSource.XBee.Api.XBeeAddress64">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <param name="networkAddress" type="NETMF.OpenSource.XBee.Api.XBeeAddress16">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public NodeInfo(XBeeAddress64 serialNumber, XBeeAddress16 networkAddress)
        {
            NetworkAddress = networkAddress;
            SerialNumber = serialNumber;
            NodeIdentifier = string.Empty;
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <returns>
        ///     A string value...
        /// </returns>
        public override string ToString()
        {
            return "S/N=" + SerialNumber
                   + ", address=" + NetworkAddress
                   + ", id='" + NodeIdentifier + "'";
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="n1" type="NETMF.OpenSource.XBee.Api.Common.NodeInfo">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <param name="n2" type="object">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public static bool operator ==(NodeInfo n1, object n2)
        {
            if (ReferenceEquals(null, n1) && ReferenceEquals(null, n2))
                return true;

            return !ReferenceEquals(null, n1) && n1.Equals(n2);
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="n1" type="NETMF.OpenSource.XBee.Api.Common.NodeInfo">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <param name="n2" type="object">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public static bool operator !=(NodeInfo n1, object n2)
        {
            return !(n1 == n2);
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="n1" type="NETMF.OpenSource.XBee.Api.Common.NodeInfo">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <param name="n2" type="NETMF.OpenSource.XBee.Api.Common.NodeInfo">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public static bool operator ==(NodeInfo n1, NodeInfo n2)
        {
            if (ReferenceEquals(null, n1) && ReferenceEquals(null, n2))
                return true;

            return !ReferenceEquals(null, n1) && n1.Equals(n2);
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="n1" type="NETMF.OpenSource.XBee.Api.Common.NodeInfo">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <param name="n2" type="NETMF.OpenSource.XBee.Api.Common.NodeInfo">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        public static bool operator !=(NodeInfo n1, NodeInfo n2)
        {
            return !(n1 == n2);
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="other" type="NETMF.OpenSource.XBee.Api.Common.NodeInfo">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <returns>
        ///     A bool value...
        /// </returns>
        public bool Equals(NodeInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.NetworkAddress, NetworkAddress) 
                && Equals(other.SerialNumber, SerialNumber);
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <param name="obj" type="object">
        ///     <para>
        ///         
        ///     </para>
        /// </param>
        /// <returns>
        ///     A bool value...
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof (NodeInfo) 
                && Equals((NodeInfo) obj);
        }

        /// <summary>
        ///   TODO: Update Comments
        ///     
        /// </summary>
        /// <returns>
        ///     A int value...
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (NetworkAddress.GetHashCode() * 397) ^ SerialNumber.GetHashCode();
            }
        }
    }
}