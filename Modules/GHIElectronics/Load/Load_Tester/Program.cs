using System.Threading;

using GT = Gadgeteer;

namespace Load_Tester
{
    public partial class Program
    {
        private GT.Timer timer;
        private bool next;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("Load Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.next = false;
            this.timer = new GT.Timer(1000);
            this.timer.Tick += (a) =>
            {
                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("LEDs are now " + (this.next ? "on" : "off"), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);

                this.load1.P1.Write(next);
                this.load1.P2.Write(next);
                this.load1.P3.Write(next);
                this.load1.P4.Write(next);
                this.load1.P5.Write(next);
                this.load1.P6.Write(next);
                this.load1.P7.Write(next);

                this.load2.P1.Write(next);
                this.load2.P2.Write(next);
                this.load2.P3.Write(next);
                this.load2.P4.Write(next);
                this.load2.P5.Write(next);
                this.load2.P6.Write(next);
                this.load2.P7.Write(next);

                this.load3.P1.Write(next);
                this.load3.P2.Write(next);
                this.load3.P3.Write(next);
                this.load3.P4.Write(next);
                this.load3.P5.Write(next);
                this.load3.P6.Write(next);
                this.load3.P7.Write(next);

                this.next = !this.next;
            };
            this.timer.Start();
        }
    }
}
