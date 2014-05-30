using GHI.Networking;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using System;
using System.Net;
using System.Threading;
using GT = Gadgeteer;

namespace EthernetENC28_Tester
{
    public partial class Program
    {
        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("EthernetENC28 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);

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

                using (var netif = new EthernetENC28J60(socket.SPIModule, socket.CpuPins[6], socket.CpuPins[3], socket.CpuPins[4]))
                {
                    netif.Open();
                    netif.EnableDhcp();
                    netif.EnableDynamicDns();

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

                        var s = end.AddMilliseconds(-20 * count) - start;
                        var ms = s.Minutes * 60000 + s.Seconds * 1000 + s.Milliseconds;
                        this.displayT43.SimpleGraphics.DisplayText(i.ToString() + " " + ((total / 1024.0) / (ms / 1000.0)).ToString("N2") + "    " + total.ToString() + "    " + ms.ToString(), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 15);

                        Thread.Sleep(100);
                    }
                }

                this.displayT43.SimpleGraphics.DisplayText("Module passes.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.Green, 0, 30);
            }
        }
    }
}
