using System.Threading;

using GT = Gadgeteer;

namespace DisplayN18_Tester
{
    public partial class Program
    {
        private GT.Timer timer;
        private int next;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("DisplayN18 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.next = 0;
            this.timer = new GT.Timer(750);
            this.timer.Tick += (a) =>
            {
                this.displayT43.SimpleGraphics.Clear();
                switch (this.next)
                {
                    case 0: this.Show(GT.Color.Red, "Red"); break;
                    case 1: this.Show(GT.Color.Green, "Green"); break;
                    case 2: this.Show(GT.Color.Blue, "Blue"); break;
                    case 3: this.Show(GT.Color.White, "White"); break;
                    case 4: this.Show(GT.Color.Black, "Black"); break;
                    case 5: this.Show(GT.Color.Yellow, "Yellow"); break;
                    case 6: this.Show(GT.Color.Purple, "Purple"); break;
                    case 7: this.Show(GT.Color.Orange, "Orange"); break;
                }

                this.next = (++this.next) % 8;
            };
            this.timer.Start();
        }

        private void Show(GT.Color c, string s)
        {
            this.displayT43.SimpleGraphics.DisplayText("Screens are now " + s.ToLower(), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            this.displayN181.SimpleGraphics.DisplayRectangle(c, 1, c, 0, 0, 128, 160);
            this.displayN182.SimpleGraphics.DisplayRectangle(c, 1, c, 0, 0, 128, 160);
            this.displayN183.SimpleGraphics.DisplayRectangle(c, 1, c, 0, 0, 128, 160);
        }
    }
}
