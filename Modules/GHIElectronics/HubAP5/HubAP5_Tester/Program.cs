using System.Threading;
using GT = Gadgeteer;

namespace HubAP5_Tester
{
    public partial class Program
    {
        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("HubAP5 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.button.ButtonPressed += (a, b) =>
            {
                this.button.TurnLedOn();
                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("Button pressed.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            };

            this.button.ButtonReleased += (a, b) =>
            {
                this.button.TurnLedOff();
                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("Button released.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            };
        }
    }
}
