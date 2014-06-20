using System.Threading;
using GT = Gadgeteer;
using Gadgeteer.Modules.GHIElectronics;

namespace DistanceUS3_Tester
{
    public partial class Program
    {
        private GT.Timer timer;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("DistanceUS3 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.timer = new GT.Timer(200);
            this.timer.Tick += (a) =>
            {
                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("Distance: " + this.distanceUS31.GetDistance().ToString() + "cm", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            };
            this.timer.Start();
        }
    }
}
