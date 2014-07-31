using Gadgeteer.Modules.GHIElectronics;
using System.Threading;
using GT = Gadgeteer;

namespace TouchC8_Tester
{
    public partial class Program
    {
        private GT.Timer timer;
        private int next;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("TouchC8 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.timer = new GT.Timer(30);
            this.timer.Tick += (a) =>
            {
                this.next = 0;
                this.displayT43.SimpleGraphics.Clear();

                if (this.touchC8.IsButtonPressed(TouchC8.Button.Up)) this.displayT43.SimpleGraphics.DisplayText("Button 1 pressed.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, this.next++ * 15);
                if (this.touchC8.IsButtonPressed(TouchC8.Button.Middle)) this.displayT43.SimpleGraphics.DisplayText("Button 2 pressed.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, this.next++ * 15);
                if (this.touchC8.IsButtonPressed(TouchC8.Button.Down)) this.displayT43.SimpleGraphics.DisplayText("Button 3 pressed.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, this.next++ * 15);

                this.displayT43.SimpleGraphics.DisplayText("Wheel position: " + this.touchC8.GetWheelPosition().ToString("F0"), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, this.next++ * 15);
            };
            this.timer.Start();
        }
    }
}
