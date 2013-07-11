using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;
using GT = Gadgeteer;

namespace TestApp
{
	public partial class Program
	{
		RS485 socket2 = new RS485(2);
		RS485 socket6 = new RS485(6);

		void ProgramStarted()
		{
			socket2.Initialize();
			socket6.Initialize();

			socket6.Port.DataReceived += (GT.Interfaces.Serial sender, System.IO.Ports.SerialData data) =>
			{
				byte[] received = new byte[socket6.Port.BytesToRead];

				socket6.Port.Read(received, 0, received.Length);

				string dataString = "";
				for (int i = 0; i < received.Length; i++)
					dataString += (char)received[i];

				Debug.Print(dataString);
			};

			GT.Timer timer = new GT.Timer(1000);
			timer.Tick += (sender) =>
			{
				socket2.Port.WriteLine("Hello, World!");
			};
			timer.Start();
		}
	}
}
