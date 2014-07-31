using System.Threading;
using GT = Gadgeteer;

namespace TouchL12_Tester
{
    public partial class Program
    {
        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("TouchL12 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.touchL12.SliderPositionChanged += (a, b) =>
            {
                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("Position: " + b.Position.ToString("F2"), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            };
        }
    }
}
