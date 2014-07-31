using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using System.Threading;
using GT = Gadgeteer;

namespace DisplayT43_Tester
{
    public partial class Program
    {
        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("DisplayT43 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);
			
            Application.Current.MainWindow = new Window();
            Application.Current.MainWindow.TouchDown += (a, b) =>
            {
                b.Touches[0].X = (int)(b.Touches[0].X / 0.75);
                this.displayT43.SimpleGraphics.DisplayEllipse(GT.Color.Red, 1, GT.Color.Red, b.Touches[0].X - 3, b.Touches[0].Y + 3, 3, 3);
                this.displayT43.SimpleGraphics.DisplayEllipse(GT.Color.Green, 1, GT.Color.Green, b.Touches[0].X + 3, b.Touches[0].Y + 3, 3, 3);
                this.displayT43.SimpleGraphics.DisplayEllipse(GT.Color.Blue, 1, GT.Color.Blue, b.Touches[0].X, b.Touches[0].Y - 3, 3, 3);
            };
        }
    }
}
