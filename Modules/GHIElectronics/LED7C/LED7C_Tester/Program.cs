using Gadgeteer.Modules.GHIElectronics;
using System.Threading;
using GT = Gadgeteer;

namespace LED7C_Tester
{
    public partial class Program
    {
        private GT.Timer timer;

        private void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("LED7C Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.timer = new GT.Timer(1000);
            this.timer.Tick += (a) =>
            {
                this.displayT43.SimpleGraphics.Clear();

                this.Set(LED7C.Color.Red, "red");
                this.Set(LED7C.Color.Green, "green");
                this.Set(LED7C.Color.Blue, "blue");
                this.Set(LED7C.Color.White, "white");
                this.Set(LED7C.Color.Off, "off");
            };
            this.timer.Start();
        }

        private void Set(LED7C.Color color, string colorString)
        {
            this.displayT43.SimpleGraphics.Clear();
            this.displayT43.SimpleGraphics.DisplayText("LEDs are now " + colorString, Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            this.led7C1.SetColor(color);
            this.led7C2.SetColor(color);
            this.led7C3.SetColor(color);
            this.led7C4.SetColor(color);
            this.led7C5.SetColor(color);
            this.led7C6.SetColor(color);
            this.led7C7.SetColor(color);
            this.led7C8.SetColor(color);
            this.led7C9.SetColor(color);
            this.led7C10.SetColor(color);
            Thread.Sleep(750);
        }
    }
}
