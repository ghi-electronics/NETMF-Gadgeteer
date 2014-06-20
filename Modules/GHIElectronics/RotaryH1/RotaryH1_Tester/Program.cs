using System.Threading;
using GT = Gadgeteer;

namespace RotaryH1_Tester
{
    public partial class Program
    {
        private GT.Timer timer;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("RotaryH1 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.timer = new GT.Timer(150);
            this.timer.Tick += (c) =>
                {
                    var str = "";

                    str += "Socket 1: " + this.rotaryH11.GetCount().ToString() + " " + this.rotaryH11.GetDirection().ToString() + "        ";
                    str += "Socket 9: " + this.rotaryH12.GetCount().ToString() + " " + this.rotaryH12.GetDirection().ToString() + "        ";
                    str += "Socket 18: " + this.rotaryH13.GetCount().ToString() + " " + this.rotaryH13.GetDirection().ToString();

                    this.displayT43.SimpleGraphics.Clear();
                    this.displayT43.SimpleGraphics.DisplayText(str, Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
                };
            this.timer.Start();
        }
    }
}
