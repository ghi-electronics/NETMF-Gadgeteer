using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;
using GT = Gadgeteer;
using System.Threading;

namespace TestApp
{
	public partial class Program
	{
		RS485 socket4 = new RS485(4);
		RS485 socket8 = new RS485(8);

		void ProgramStarted()
		{
			this.socket4.Initialize();
			this.socket8.Initialize();

			this.socket4.Port.DataReceived += (sender, data) =>
			{
				if (sender.ReadByte() == (byte)'X')
				{
					this.led7c.SetColor(GT.Modules.GHIElectronics.LED7C.LEDColor.White);
					Thread.Sleep(1000);
				}

				this.led7c.SetColor(GT.Modules.GHIElectronics.LED7C.LEDColor.Off);
			};

			this.socket8.Port.DataReceived += (sender, data) =>
			{
				if (sender.ReadByte() == (byte)'Q')
					this.socket8.Port.Write((byte)'X');
			};
			
			this.button.ButtonReleased += (sender, e) =>
			{
				this.socket4.Port.Write((byte)'Q');
			};
		}
	}
}
