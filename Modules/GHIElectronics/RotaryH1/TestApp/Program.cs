using System.Threading;
using GTM = Gadgeteer.Modules;

namespace TestApp
{
    public partial class Program
    {
        void ProgramStarted()
		{
			var rotary = new GTM.GHIElectronics.RotaryH1(2);

			new Thread(() =>
			{
				while (true)
				{
					char_Display.Clear();
					char_Display.CursorHome();
					char_Display.PrintString(rotary.GetDirection() == GTM.GHIElectronics.RotaryH1.Direction.Up ? "Up" : "Down");
					char_Display.SetCursor(1, 0);
					char_Display.PrintString(rotary.GetCount().ToString());

					Thread.Sleep(500);
				}
			}).Start();
        }
    }
}
