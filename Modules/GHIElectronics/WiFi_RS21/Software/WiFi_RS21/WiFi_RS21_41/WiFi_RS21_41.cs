using System;

using Microsoft.SPOT.Net.NetworkInformation;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

using GHINet = GHIElectronics.NETMF.Net;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A WiFi RS21 Gadgeteer module
    /// </summary>
    public class WiFi_RS21 : GTM.Module.NetworkModule
    {

        private Interfaces.SPI _spi;

        /// <summary>
        /// WiFi Security Mode
        /// </summary>
        public enum SecurityMode
        {
            /// <summary>
            /// Open, No security or encryption
            /// </summary>
            Open = 0,
            /// <summary>
            /// WPA, Wi-Fi Protected Access II 
            /// </summary>
            WPA = 1,
            /// <summary>
            /// WPA, Wi-Fi Protected Access II 
            /// </summary>
            WPA2 = 2,
            /// <summary>
            /// WEP, Wired Equivalent Privacy 
            /// </summary>
            WEP = 3
        }

        /// <summary>
        /// WiFi Netwrok Type
        /// </summary>
        public enum NetworkType
        {
            /// <summary>
            /// Ad-Hoc (IBSS).
            /// </summary>
            AdHoc = 0,
            /// <summary>
            /// Wireless Access Point (Infrastructure).
            /// </summary>
            AccessPoint = 1
        }

        /// <summary>
        /// WiFi Network Information. 
        /// </summary>
        public class WiFiNetworkInfo
        {
            /// <summary>
            /// Channel Number.
            /// </summary>
            public uint ChannelNumber;
            /// <summary>
            /// Security Mode
            /// </summary>
            public SecurityMode SecMode;
            /// <summary>
            /// Received signal strength indication in -dB unit
            /// </summary>
            public int RSSI;
            /// <summary>
            /// Service set identifier (the wifi netwrok name)
            /// </summary>
            public String SSID;
            /// <summary>
            /// Netwrok Type
            /// </summary>
            public NetworkType networkType;
            /// <summary>
            /// Base station's MAC address.
            /// </summary>
            public byte[] PhysicalAddress;
            /// <summary>
            /// AccessPointInfo Constructor.
            /// </summary>
            public WiFiNetworkInfo()
            {
                ChannelNumber = 0;
                SecMode = SecurityMode.Open;
                networkType = NetworkType.AdHoc;
                SSID = string.Empty;
                PhysicalAddress = new byte[6];
            }

            /// <summary>
            /// Copy the object.
            /// </summary>
            /// <param name="info">WiFiNetworkInfo to be copied</param>
            public WiFiNetworkInfo(WiFiNetworkInfo info)
            {
                ChannelNumber = info.ChannelNumber;
                SecMode = info.SecMode;
                RSSI = info.RSSI;
                SSID = info.SSID;
                networkType = info.networkType;
                PhysicalAddress = new byte[6];
                Array.Copy(info.PhysicalAddress, PhysicalAddress, info.PhysicalAddress.Length);

            }

            internal GHINet.WiFiNetworkInfo GHIWiFiNetworkInfo
            {
                get
                {
                    GHINet.WiFiNetworkInfo _info = new GHINet.WiFiNetworkInfo();
                    _info.ChannelNumber = ChannelNumber;
                    _info.SecMode = (GHINet.SecurityMode)SecMode;
                    _info.RSSI = RSSI;
                    _info.SSID = SSID;
                    _info.networkType = (GHINet.NetworkType)networkType;
                    Array.Copy(PhysicalAddress, _info.PhysicalAddress, _info.PhysicalAddress.Length);
                    return _info;

                }
            }
            /// <summary>
            /// ToString()
            /// </summary>
            /// <returns>AccessPointInfo in a string</returns>
            public override string ToString()
            {
                string str;
                str = "SSID: " + SSID + "\n";
                str += "Channel Number: " + ChannelNumber + "\n";
                str += "RSSI: -" + RSSI + "dB" + "\n";
                str += "Security Mode: ";
                switch (SecMode)
                {
                    case SecurityMode.Open:
                        str += "Open";
                        break;
                    case SecurityMode.WEP:
                        str += "WEP";
                        break;
                    case SecurityMode.WPA:
                        str += "WPA";
                        break;
                    case SecurityMode.WPA2:
                        str += "WPA2";
                        break;
                }
                str += "\n";
                str += "Network Type: ";
                switch (networkType)
                {
                    case NetworkType.AccessPoint:
                        str += "Access Point";
                        break;
                    case NetworkType.AdHoc:
                        str += "AdHoc";
                        break;
                }
                str += "\n";
                str += "BS MAC: " + ByteToHex(PhysicalAddress[0]) + "-"
                                    + ByteToHex(PhysicalAddress[1]) + "-"
                                    + ByteToHex(PhysicalAddress[2]) + "-"
                                    + ByteToHex(PhysicalAddress[3]) + "-"
                                    + ByteToHex(PhysicalAddress[4]) + "-"
                                    + ByteToHex(PhysicalAddress[5]) + "\n";
                return str;
            }

            /// <summary>
            /// Convert Byte to HEX string.
            /// </summary>
            /// <param name="number">number</param>
            /// <returns>HEX in a string</returns>
            private static string ByteToHex(byte number)
            {
                string hex = "0123456789ABCDEF";
                return new string(new char[] { hex[(number & 0xF0) >> 4], hex[number & 0x0F] });
            }

        };


        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="socketNumber">The mainboard socket that has the module plugged into it.</param>
        public WiFi_RS21(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            if (!socket.SupportsType('S'))
            {
                throw new GT.Socket.InvalidSocketException("Socket " + socket +
                    " does not support support WiFi RS21 modules. Please plug the WiFi RS21 module into a socket labeled 'S'");
            }

            // Since the Configuration parameter is null, this just reserves the pins and sets _spi.SPIModule but doesnt set up SPI (which the WiFi driver does itself)
            _spi = new Interfaces.SPI(socket, null, Interfaces.SPI.Sharing.Exclusive, this);

            // Make sure that the INT pin gets reserved. This is used internally by WiFi driver
            socket.ReservePin(Socket.Pin.Three, this);
            socket.ReservePin(Socket.Pin.Four, this);
            socket.ReservePin(Socket.Pin.Six, this);


            if (!GHINet.WiFi.IsEnabled)
            {
                try
                {
                    GHINet.WiFi.Enable(GHINet.WiFi.HardwareModule.RS9110_N_11_21_1_Compatible, _spi.SPIModule, socket.CpuPins[6], socket.CpuPins[3], socket.CpuPins[4]);
                }
                catch (GHINet.WiFi.WiFiException e)
                {
                    if (e.errorCode == GHINet.WiFi.WiFiException.ErrorCode.HardwareCommunicationTimeout || e.errorCode == GHINet.WiFi.WiFiException.ErrorCode.HardwareFirmwareVersionMismatch)
                    {
                        try
                        {
                            GHINet.WiFi.UpdateFirmware(GHINet.WiFi.HardwareModule.RS9110_N_11_21_1_Compatible, _spi.SPIModule, socket.CpuPins[6], socket.CpuPins[3], socket.CpuPins[4]);
                            GHINet.WiFi.Enable(GHINet.WiFi.HardwareModule.RS9110_N_11_21_1_Compatible, _spi.SPIModule, socket.CpuPins[6], socket.CpuPins[3], socket.CpuPins[4]);
                        }
                        catch (Exception ee)
                        {
                            throw new ApplicationException("Unable to enable WiFi module.", ee);
                        }
                    }
                    else
                    {
                        throw new ApplicationException("Unable to enable WiFi module.", e);
                    }

                }

            }
            NetworkInterface[] netInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            if (netInterfaces == null || netInterfaces[0] == null)
            {
                throw new Exception("Unable to configure the mainboard's Ethernet interface");
            }

            //// There is not need for the following becaue WiFi RS21 connection is dynamic and it uses interface[0] resources anyway.
            //for (int i = 0; i < netInterfaces.Length; i++)
            //{
            //    if (netInterfaces[i].NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            //    {
            //        NetworkSettings = netInterfaces[0];
            //        break;
            //    }
            //}

            //if (null == NetworkSettings)
            //{
            //    throw new Exception("Unable to find an Ethernet interface on the mainboard.");
            //}
            NetworkSettings = netInterfaces[0];

        }

        /// <summary>
        /// This function scans for all access points in range with any channel.
        /// </summary>
        /// <returns>An array of the found access point.</returns>
        public WiFiNetworkInfo[] Scan()
        {
            GHINet.WiFiNetworkInfo[] ScanResp = GHINet.WiFi.Scan();
            if (ScanResp != null)
            {
                WiFiNetworkInfo[] GScanResp = new WiFiNetworkInfo[ScanResp.Length];
                for (int i = 0; i < ScanResp.Length; i++)
                {
                    GScanResp[i] = new WiFiNetworkInfo();
                    GScanResp[i].ChannelNumber = ScanResp[i].ChannelNumber;
                    GScanResp[i].SecMode = (SecurityMode)ScanResp[i].SecMode;
                    GScanResp[i].RSSI = ScanResp[i].RSSI;
                    GScanResp[i].SSID = ScanResp[i].SSID;
                    GScanResp[i].networkType = (NetworkType)ScanResp[i].networkType;
                    Array.Copy(ScanResp[i].PhysicalAddress, GScanResp[i].PhysicalAddress, GScanResp[i].PhysicalAddress.Length);

                }
                return GScanResp;
            }
            else
                return null;
        }

        /// <summary>
        /// Search for a specific BSSID 
        /// </summary>
        /// <param name="SSID">The service set identifier (or network name) of the wireless network to connect to.</param>
        /// <returns>BSSID Information object. it returns null if the network not found.</returns>
        public WiFiNetworkInfo Search(string SSID)
        {
            WiFiNetworkInfo[] ScanResp = Scan();

            for (int i = 0; i < ScanResp.Length; i++)
            {
                if (string.Compare(ScanResp[i].SSID, SSID) == 0)
                {

                    return new WiFiNetworkInfo(ScanResp[i]);
                }
            }

            return null;
        }


        /// <summary>
        /// Join open (non secured) wireless network
        /// </summary>
        /// <param name="bssid">Wireless network information (usually it is generated by <see cref="M:Scan()"/> or  <see cref="M:Search"/> method)</param>
        public void Join(WiFiNetworkInfo bssid)
        {

            GHINet.WiFi.Join(bssid.GHIWiFiNetworkInfo, "");
        }

        /// <summary>
        /// Join wireless network
        /// </summary>
        /// <param name="bssid">Wireless network information (usually it is generated by <see cref="M:Scan"/> or  <see cref="M:Search"/> method)</param>
        /// <param name="PassPhrase">equals "" for open networks. <br/>
        /// With WPA or WPA2, it is a plain string Pass phrase. For example:"password"<br/>
        /// With WEP, Key1 should be use in HEX format in a string. For example: if the Key 1 is 0xE8430A5EDB then use this string "E8430A5EDB".<br/></param>
        public void Join(WiFiNetworkInfo bssid, string PassPhrase)
        {
            GHINet.WiFi.Join(bssid.GHIWiFiNetworkInfo, PassPhrase);
        }

        /// <summary>
        /// Disconnect WiFi connection.
        /// </summary>
        public void Disconnect()
        {
            GHINet.WiFi.Disconnect();
        }
        /// <summary>
        /// Gets a value that indicates whether this wi-fi module has established a media link.
        /// </summary>
        /// <remarks>
        /// <para>
        ///  This property enables you to determine if the WiFi module has
        ///  established a link with a network device, such as a wireless router. 
        ///  When this property is <b>true</b>, it does not necessarily mean that the network connection is functional. 
        ///  You must also check the <see cref="P:Gadgeteer.Modules.NetworkModule.IsNetworkUp"/> property. 
        ///  <see cref="P:Gadgeteer.Modules.NetworkModule.IsNetworkUp"/> returns <b>true</b> 
        ///  if the network connection is both connected and configured for Internet Proctocol (IP) communication tasks. 
        /// </para>
        /// <note>
        ///  When <see cref="P:Gadgeteer.Modules.NetworkModule.IsNetworkUp"/> is <b>true</b>, it does not necessarily mean 
        ///  that the network connection is functional. The IP configuration
        ///  for the network connection may be invalid for the network that it is connected to.
        /// </note>
        /// </remarks>
        public override bool IsNetworkConnected
        {
            get
            {
                return GHINet.WiFi.IsLinkConnected;
            }
        }

    }
}