using System.Threading;

using GT = Gadgeteer;
using Gadgeteer.Modules.GHIElectronics;

namespace Tunes_Tester
{
    public partial class Program
    {
        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("Tunes Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(1000);

            this.displayT43.SimpleGraphics.Clear();
            this.displayT43.SimpleGraphics.DisplayText("Playing the tone now", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);

            this.tunes.Play(800);
        }
    }
}
