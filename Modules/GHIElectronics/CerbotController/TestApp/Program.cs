using System.Threading;

namespace TestApp
{
	public partial class Program
	{
		void ProgramStarted()
		{
			cerbotController.StartBuzzer(2000);
			//cerbotController.StartBuzzer(1000, 200);
			//Thread.Sleep(1000);
			//cerbotController.StartBuzzer(10, 200);
			//Thread.Sleep(1000);
			//cerbotController.StartBuzzer(1100, 200);
			//Thread.Sleep(1000);
			//cerbotController.StartBuzzer(5100, 200);
			//Thread.Sleep(1000);
		}
	}
}
