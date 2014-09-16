using GHI.Networking;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using System.Net;
using System.Threading;
using GT = Gadgeteer;

namespace WiFiRS21_Tester
{
    public partial class Program
    {
        void ProgramStarted()
        {
            this.Write("WiFiRS21 Tester.", GT.Color.White);
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
                this.Write("Press LDR0 to begin.", GT.Color.White);

                while (Mainboard.LDR0.Read())
                    Thread.Sleep(10);

                this.Write("Configuring.", GT.Color.White);

                using (var netif = new WiFiRS9110(socket.SPIModule, socket.CpuPins[6], socket.CpuPins[3], socket.CpuPins[4]))
                {
                    netif.Open();
                    netif.EnableDhcp();
                    netif.EnableDynamicDns();

                    this.Write("Joining.", GT.Color.White);
                    netif.Join("", "");
                    this.Write("Joined.", GT.Color.White);

                    while (netif.IPAddress == "0.0.0.0")
                    {
                        Debug.Print("Waiting DHCP");
                        Thread.Sleep(250);
                    }

                    this.Write("Connecting.", GT.Color.White);

                    using (var req = HttpWebRequest.Create("http://ghielectronics.com/downloads/") as HttpWebRequest)
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

                this.Write("Module passes.", GT.Color.Green);
                Thread.Sleep(2000);
            }
        }

        private void Write(string text, GT.Color color)
        {
            this.displayT43.SimpleGraphics.Clear();
            this.displayT43.SimpleGraphics.DisplayText(text, Resources.GetFont(Resources.FontResources.NinaB), color, 0, 0);
        }
    }
}
