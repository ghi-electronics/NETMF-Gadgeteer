using System.Threading;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace TestApp
{
    public partial class Program
    {
        void ProgramStarted()
		{
			var rotary = rotaryH1;

			new Thread(() =>
			{
				while (true)
				{
					char_Display.Clear();
					char_Display.CursorHome();
					char_Display.PrintString(rotary.GetDirection() == GTM.GHIElectronics.RotaryH1.Direction.CounterClockwise ? "CounterClockwise" : "Clockwise");
					char_Display.SetCursor(1, 0);
					char_Display.PrintString(rotary.GetCount().ToString());

					Thread.Sleep(500);
				}
			}).Start();
        }
    }
}
