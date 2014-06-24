using System.Threading;
using GT = Gadgeteer;

namespace LightSense_Tester
{
    public partial class Program
    {
        private GT.Timer timer;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("LightSense Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.timer = new GT.Timer(100);
            this.timer.Tick += (a) =>
                {
                    this.displayT43.SimpleGraphics.Clear();

                    this.displayT43.SimpleGraphics.DisplayText("Socket  2: " + (this.lightSense1.ReadProportion() * 100).ToString("f2"), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
                    this.displayT43.SimpleGraphics.DisplayText("Socket 13: " + (this.lightSense2.ReadProportion() * 100).ToString("f2"), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 15);
                    this.displayT43.SimpleGraphics.DisplayText("Socket 18: " + (this.lightSense3.ReadProportion() * 100).ToString("f2"), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 30);
                };
            this.timer.Start();
        }
    }
}
