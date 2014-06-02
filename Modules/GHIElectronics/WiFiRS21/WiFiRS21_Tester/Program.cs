using GHI.Networking;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using System;
using System.Net;
using System.Threading;
using GT = Gadgeteer;

namespace WiFiRS21_Tester
{
    public partial class Program
    {
        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("WiFiRS21 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);

            new Thread(this.DoNetwork).Start();
        }

        private void DoNetwork()
        {
            NetworkChange.NetworkAvailabilityChanged += (a, b) => Debug.Print("SPOT NAVAC " + b.IsAvailable.ToString());
            NetworkChange.NetworkAddressChanged += (a, b) => Debug.Print("SPOT NADRC");
            var socket = GT.Socket.GetSocket(1, true, null, null);

            while (true)
            {
                Thread.Sleep(5000);
                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("Press LD0 to begin.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
                while (Mainboard.LDR0.Read())
                    Thread.Sleep(250);

                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("Beginning test.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);

                using (var netif = new WiFiRS9110(socket.SPIModule, socket.CpuPins[6], socket.CpuPins[3], socket.CpuPins[4]))
                {
                    netif.Open();
                    netif.EnableStaticIP("192.168.0.225", "255.255.255.0", "192.168.0.1");
                    netif.EnableStaticDns(new string[] { "192.168.0.1" });

                    Debug.Print("Joining");
                    netif.Join("GHI Production", "48755981");
                    Debug.Print("Joined");

                    while (netif.IPAddress == "0.0.0.0")
                    {
                        Debug.Print("Waiting DHCP");
                        Thread.Sleep(250);
                    }

                    Debug.Print(netif.IPAddress);

                    byte[] result = new byte[8192 * 4];

                    for (int i = 0; i < 10; i++)
                    {
                        DateTime start, end;
                        int read = 0, total = 0, count = 0;

                        using (var req = HttpWebRequest.Create("http://www.bing.com/robots.txt") as HttpWebRequest)
                        {
                            req.KeepAlive = false;
                            start = DateTime.Now;

                            using (var res = req.GetResponse() as HttpWebResponse)
                            {
                                using (var stream = res.GetResponseStream())
                                {
                                    do
                                    {
                                        read = stream.Read(result, 0, result.Length);

                                        total += read;
                                        count++;

                                        Thread.Sleep(20);
                                    } while (read != 0);

                                    end = DateTime.Now;
                                }
                            }
                        }

                        Thread.Sleep(100);
                    }
                }

                this.displayT43.SimpleGraphics.DisplayText("Module passes.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.Green, 0, 15);
            }
        }
    }
}
