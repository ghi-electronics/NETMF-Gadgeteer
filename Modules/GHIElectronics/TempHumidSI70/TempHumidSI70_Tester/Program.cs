using Microsoft.SPOT;
using System.Threading;
using GT = Gadgeteer;
using Gadgeteer.Modules.GHIElectronics;

namespace TempHumidSI70_Tester
{
    public partial class Program
    {
        private GT.Timer timer;
        private Font font;
        private string measurement;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("TempHumidSI70 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.font = Resources.GetFont(Resources.FontResources.NinaB);
            this.measurement = string.Empty;
            this.timer = new GT.Timer(100);
            this.timer.Tick += (a) =>
            {
                this.measurement = this.tempHumidSI70.TakeMeasurement().ToString();

                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText(this.measurement, this.font, GT.Color.White, 0, 0);
            };
            this.timer.Start();
        }
    }
}
