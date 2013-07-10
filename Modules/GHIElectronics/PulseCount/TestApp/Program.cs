using System.Threading;
using GTM = Gadgeteer.Modules;

namespace TestApp
{
	public partial class Program
	{
		void ProgramStarted()
		{
			var pulse = new GTM.GHIElectronics.PulseCount(2);

			new Thread(() =>
			{
				while (true)
				{
					char_Display.Clear();
					char_Display.CursorHome();
					char_Display.PrintString(pulse.GetValue().ToString());

					Thread.Sleep(500);
				}
			}).Start();
		}
	}
}
