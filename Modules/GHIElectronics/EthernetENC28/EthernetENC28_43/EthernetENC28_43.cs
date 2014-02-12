using System;
using System.Threading;

using Microsoft.SPOT.Net.NetworkInformation;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

using GHINet = GHI.Net;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2CBus), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// An Ethernet_ENC28 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class Ethernet_ENC28 : GTM.Module.NetworkModule
    {
        /// <summary>
        /// The class that will be used to interface with the ethernet module. This member will handle everything from initialization to joining networks.
        /// </summary>
        public GHINet.EthernetENC28J60 Interface;
        private GTI.Spi _spi;

        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public Ethernet_ENC28(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('S', this);

            _spi = GTI.SpiFactory.Create(socket, null, GTI.SpiSharing.Exclusive, socket, Socket.Pin.Six, this);

            socket.ReservePin(Socket.Pin.Three, this);
            socket.ReservePin(Socket.Pin.Four, this);
            socket.ReservePin(Socket.Pin.Six, this);

            Interface = new GHINet.EthernetENC28J60(socket.SPIModule, socket.CpuPins[6], socket.CpuPins[3], socket.CpuPins[4], 1000);

            //if (!Interface.IsOpen)
            //{
            //    Interface.Open();
            //}

            //GHINet.NetworkInterfaceExtension.AssignNetworkingStackTo(Interface);

            //Thread.Sleep(500);

            //NetworkSettings = Interface.NetworkInterface;
        }

        ///// <summary>
        ///// Instructs the Mainboard to use this module for all network communication, and assigns the networking stack to this module.
        ///// </summary>
        ///// <remarks>
        ///// This function is only needed if more than one network module is being used simultaneously. If not, this function should not be used.
        ///// </remarks>
        //public void UseThisNetworkInterface()
        //{
        //    GHINet.NetworkInterfaceExtension.AssignNetworkingStackTo(Interface);
        //}

        /// <summary>
        /// Gets a value that indicates whether this ethernet module is physically connected to a network device.
        /// </summary>
        /// <remarks>
        /// <para>
        ///  This property enables you to determine if the <see cref="Ethernet_ENC28"/> module is
        ///  physically connected to a network device, such as a router. 
        ///  When this property is <b>true</b>, it does not necessarily mean that the network connection is usable. 
        ///  You must also check the <see cref="P:Microsoft.Gadgeteer.Modules.NetworkModule.IsNetworkUp"/> property. 
        ///  <see cref="P:Microsoft.Gadgeteer.Modules.NetworkModule.IsNetworkUp"/> returns <b>true</b> 
        ///  if the network connection is both connected and configured for Internet Protocol (IP) communication tasks. 
        /// </para>
        /// <note>
        ///  When <see cref="P:Microsoft.Gadgeteer.Modules.NetworkModule.IsNetworkUp"/> is <b>true</b>, it does not necessarily mean 
        ///  that the network connection is functional. The IP configuration
        ///  for the network connection may be invalid for the network that it is connected to.
        /// </note>
        /// </remarks>
        public override bool IsNetworkConnected
        {
            get
            {
                return Interface.IsCableConnected;
            }
        }

    }
}
