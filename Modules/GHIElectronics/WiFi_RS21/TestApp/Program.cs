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

using System.Text;
using System.Net;

namespace TestApp
{
    public partial class Program
    {
        public GTM.GHIElectronics.WiFi_RS21 wifi = new GTM.GHIElectronics.WiFi_RS21(9);

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
            if(!wifi.Interface.IsOpen)
                wifi.Interface.Open();

            //wifi.GHI_Wifi.NetworkInterface.EnableStaticIP("192.168.1.160", "255.255.255.0", "192.168.1.1");

            Debug.Print("SubnetMask: " + wifi.Interface.NetworkInterface.SubnetMask);

            if (!wifi.Interface.NetworkInterface.IsDhcpEnabled)
                wifi.Interface.NetworkInterface.EnableDhcp();

            Debug.Print("SubnetMask: " + wifi.Interface.NetworkInterface.SubnetMask);

            GHINet.NetworkInterfaceExtension.AssignNetworkingStackTo(wifi.Interface);

            wifi.Interface.NetworkAddressChanged += new GHI.Premium.Net.NetworkInterfaceExtension.NetworkAddressChangedEventHandler(GHI_Wifi_NetworkAddressChanged);
            wifi.Interface.WirelessConnectivityChanged += new GHI.Premium.Net.WiFiRS9110.WirelessConnectivityChangedEventHandler(GHI_Wifi_WirelessConnectivityChanged);

            Debug.Print("Scanning for wifi networks");
            GHINet.WiFiNetworkInfo[] wifiInfo = wifi.Interface.Scan();

            int temp = 2;

            wifi.Interface.Join(wifiInfo[temp], "1a2b3c4d5e");

            Debug.Print("waiting for DHCP lease");
            while (true)
            {
                IPAddress ip = IPAddress.GetDefaultLocalAddress();
                Debug.Print(ip.ToString());

                if (ip != IPAddress.Any) break;

                Thread.Sleep(1000);
            }

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        void GHI_Wifi_WirelessConnectivityChanged(object sender, GHI.Premium.Net.WiFiRS9110.WirelessConnectivityEventArgs e)
        {
            Debug.Print("Wireless Connectivity Changed");
            Debug.Print("Connected: " + (e.IsConnected ? "true" : "false"));

            Debug.Print("SSID: " + e.NetworkInformation.SSID);
            Debug.Print("Network type: " + e.NetworkInformation.networkType.ToString());
            //Debug.Print("IP Address: " + wifi.GHI_Wifi.NetworkInterface.IPAddress);
        }

        void GHI_Wifi_NetworkAddressChanged(object sender, EventArgs e)
        {
            Debug.Print("Network Address Changed");
            Debug.Print("SubnetMask: " + wifi.Interface.NetworkInterface.SubnetMask);
            Debug.Print("IP Address: " + wifi.Interface.NetworkInterface.IPAddress);
        }
    }
}
