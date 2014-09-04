using GHI.Networking;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
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
            Thread.Sleep(2000);

            new Thread(this.DoNetwork).Start();
        }

        private void DoNetwork()
        {
            NetworkChange.NetworkAvailabilityChanged += (a, b) => Debug.Print("SPOT NAVAC " + b.IsAvailable.ToString());
            NetworkChange.NetworkAddressChanged += (a, b) => Debug.Print("SPOT NADRC");
            var socket = GT.Socket.GetSocket(1, true, null, null);
            var result = new byte[8192 * 4];
            var read = 0;

            while (true)
            {
                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("Press LDR0 to begin.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);

                while (Mainboard.LDR0.Read())
                    Thread.Sleep(10);

                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("Testing.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);

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

                    using (var req = HttpWebRequest.Create("http://www.bing.com/robots.txt") as HttpWebRequest)
                    {
                        req.KeepAlive = false;

                        using (var res = req.GetResponse() as HttpWebResponse)
                        {
                            using (var stream = res.GetResponseStream())
                            {
                                do
                                {
                                    read = stream.Read(result, 0, result.Length);

                                    Thread.Sleep(20);
                                } while (read != 0);
                            }
                        }
                    }
                }

                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("Module passes.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.Green, 0, 0);
                Thread.Sleep(2000);
            }
        }
    }
}
