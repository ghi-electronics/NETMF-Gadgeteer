using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

using GHINet = GHI.Premium.Net;

namespace TestApp
{
    public partial class Program
    {
        public GTM.GHIElectronics.Ethernet_ENC28 ethEnc = new GTM.GHIElectronics.Ethernet_ENC28(6);

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/
            if (!ethEnc.Interface.IsOpen)
                ethEnc.Interface.Open();

            Debug.Print("Subnet Mask: " + ethEnc.Interface.NetworkInterface.SubnetMask);

            if (!ethEnc.Interface.NetworkInterface.IsDhcpEnabled)
                ethEnc.Interface.NetworkInterface.EnableDhcp();

            Debug.Print("Subnet Mask: " + ethEnc.Interface.NetworkInterface.SubnetMask);

            Debug.Print("Assigning the network interface to the module");

            GHINet.NetworkInterfaceExtension.AssignNetworkingStackTo(ethEnc.Interface);

            ethEnc.Interface.CableConnectivityChanged += new GHINet.EthernetENC28J60.CableConnectivityChangedEventHandler(Interface_CableConnectivityChanged);
            ethEnc.Interface.NetworkAddressChanged += new GHINet.NetworkInterfaceExtension.NetworkAddressChangedEventHandler(Interface_NetworkAddressChanged);
            

            
            
            
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        void Interface_NetworkAddressChanged(object sender, EventArgs e)
        {
            Debug.Print("Network Address Changed");
            Debug.Print("SubnetMask: " + ethEnc.Interface.NetworkInterface.SubnetMask);
            Debug.Print("IP Address: " + ethEnc.Interface.NetworkInterface.IPAddress);

        }

        void Interface_CableConnectivityChanged(object sender, GHINet.EthernetENC28J60.CableConnectivityEventArgs e)
        {
            Debug.Print("Cable Connectivity Changed");
            Debug.Print("The cable was " + (e.IsConnected ? "connected" : "removed"));            
        }
    }
}
