using System.Threading;

using GTM = Gadgeteer.Modules;

namespace TestApp
{
	public partial class Program
	{
		void ProgramStarted()
		{
			new Thread(() =>
			{
				bool state = true;
				while (true)
				{
					relay_X1.Enabled = state;
					state = !state;
					Thread.Sleep(1000);
				}
			}).Start();
		}
	}
}
