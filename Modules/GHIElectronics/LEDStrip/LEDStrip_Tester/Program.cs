using System.Threading;

using GT = Gadgeteer;

namespace LEDStrip_Tester
{
    public partial class Program
    {
        private GT.Timer timer;
        private bool next;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("LEDStrip Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.next = false;
            this.timer = new GT.Timer(1000);
            this.timer.Tick += (a) =>
            {
                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("LEDs are now " + (this.next ? "on" : "off"), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);

                if (next)
                {
                    this.ledStrip1.TurnAllLedsOn();
                    this.ledStrip2.TurnAllLedsOn();
                    this.ledStrip3.TurnAllLedsOn();
                }
                else
                {
                    this.ledStrip1.TurnAllLedsOff();
                    this.ledStrip2.TurnAllLedsOff();
                    this.ledStrip3.TurnAllLedsOff();
                }

                this.next = !this.next;
            };
            this.timer.Start();
        }
    }
}
