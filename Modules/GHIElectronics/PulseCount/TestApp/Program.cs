using System.Threading;
using GTM = Gadgeteer.Modules;

namespace TestApp
{
	public partial class Program
	{
		void ProgramStarted()
		{
			var pulse = new GTM.GHIElectronics.PulseCount(2);
			var input = pulse.CreateInterruptInput(Gadgeteer.Interfaces.GlitchFilterMode.On, Gadgeteer.Interfaces.ResistorMode.PullUp, Gadgeteer.Interfaces.InterruptMode.RisingAndFallingEdge);
			input.Interrupt += (sender, state) =>
			{
				char_Display.SetCursor(1, 0);
				char_Display.PrintString(state.ToString());
			};

			new Thread(() =>
			{
				while (true)
				{
					char_Display.Clear();
					char_Display.CursorHome();
					char_Display.PrintString(pulse.GetCount().ToString());

					Thread.Sleep(500);
				}
			}).Start();
		}
	}
}
