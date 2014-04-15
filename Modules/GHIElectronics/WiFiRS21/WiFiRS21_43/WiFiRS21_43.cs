using GHI.Networking;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A WiFiRS21 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class WiFiRS21 : GTM.Module.NetworkModule
    {
        private WiFiRS9110 networkInterface;
        private GTI.Spi spi;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public WiFiRS21(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('S', this);

            socket.ReservePin(Socket.Pin.Three, this);
            socket.ReservePin(Socket.Pin.Four, this);
            socket.ReservePin(Socket.Pin.Six, this);

            this.spi = GTI.SpiFactory.Create(socket, null, GTI.SpiSharing.Exclusive, socket, Socket.Pin.Six, this);
            this.networkInterface = new WiFiRS9110(socket.SPIModule, socket.CpuPins[6], socket.CpuPins[3], socket.CpuPins[4], 4000);

            if (BaseInterface.ActiveInterface == null)
                this.networkInterface.Open();

            Thread.Sleep(500);

            this.NetworkSettings = this.networkInterface.NetworkInterface;
        }

        /// <summary>
        /// The underlying network interface.
        /// </summary>
        public WiFiRS9110 NetworkInterface
        {
            get
            {
                return this.networkInterface;
            }
        }

        /// <summary>
        /// Opens the underlying network interface and assigns the NETMF networking stack to it if it is not already so.
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