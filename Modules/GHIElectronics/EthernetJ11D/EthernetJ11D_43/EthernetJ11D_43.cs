using GHI.Networking;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// An EthernetJ11D module for Microsoft .NET Gadgeteer
    /// </summary>
    public class EthernetJ11D : GTM.Module.NetworkModule
    {
        private EthernetBuiltIn networkInterface;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public EthernetJ11D(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('E', this);

            socket.ReservePin(Socket.Pin.Four, this);
            socket.ReservePin(Socket.Pin.Five, this);
            socket.ReservePin(Socket.Pin.Six, this);
            socket.ReservePin(Socket.Pin.Seven, this);
            socket.ReservePin(Socket.Pin.Eight, this);
            socket.ReservePin(Socket.Pin.Nine, this);

            this.networkInterface = new EthernetBuiltIn();

            this.NetworkSettings = this.networkInterface.NetworkInterface;
        }

        /// <summary>
        /// The underlying network interface.
        /// </summary>
        public EthernetBuiltIn NetworkInterface
        {
            get
            {
                return this.networkInterface;
            }
        }

        /// <summary>
        /// Opens the underlying network interface and assigns the NETMF networking stack.
        /// </summary>
        public void UseThisNetworkInterface()
        {
            if (this.networkInterface.Opened)
                return;

            this.networkInterface.Open();
        }

        /// <summary>
        /// Whether or not the cable is inserted into the module. Make sure to also check the NetworkUp property to verify network state.
        /// </summary>
        public override bool IsNetworkConnected
        {
            get
            {
                return this.networkInterface.CableConnected;
            }
        }
    }
}