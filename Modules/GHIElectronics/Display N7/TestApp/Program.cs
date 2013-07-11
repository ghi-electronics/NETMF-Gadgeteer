using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace TestApp
{
	public partial class Program
	{  
		void ProgramStarted()
		{
			var display = new GTM.GHIElectronics.Display_N7(14, 13, 12);

			display.SimpleGraphics.DisplayRectangle(GT.Color.Red, 5, GT.Color.Blue, 50, 50, display.Width - 100, display.Height - 100);
			display.SimpleGraphics.Redraw();
		}
	}
}
