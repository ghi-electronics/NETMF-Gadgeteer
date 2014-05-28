using System.Threading;
using GT = Gadgeteer;

namespace RelayX1_Tester
{
    public partial class Program
    {
        private GT.Timer timer;
        private bool next;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("RelayX1 Tester", Resources.GetFont(Resources.FontResources.small), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.next = false;
            this.timer = new GT.Timer(1000);
            this.timer.Tick += (a) =>
                {
                    this.displayT43.SimpleGraphics.Clear();
                    this.displayT43.SimpleGraphics.DisplayText("Relays " + (this.next ? "on" : "off"), Resources.GetFont(Resources.FontResources.small), GT.Color.White, 0, 0);

                    this.relayX11.Enabled = this.next;
                    this.relayX12.Enabled = this.next;
                    this.relayX13.Enabled = this.next;
                    this.relayX14.Enabled = this.next;
                    this.relayX15.Enabled = this.next;
                    this.relayX16.Enabled = this.next;
                    this.relayX17.Enabled = this.next;
                    this.relayX18.Enabled = this.next;
                    this.relayX19.Enabled = this.next;
                    this.relayX110.Enabled = this.next;

                    this.next = !this.next;
                };
            this.timer.Start();
        }
    }
}
