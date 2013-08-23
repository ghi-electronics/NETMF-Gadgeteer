using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;
using GT = Gadgeteer;
using System.Threading;

namespace TestApp
{
	public partial class Program
	{

		void ProgramStarted()
		{
			this.rs485.Initialize();
			this.rs4852.Initialize();

			this.rs485.Port.DataReceived += (sender, data) =>
			{
				if (sender.ReadByte() == (byte)'X')
				{
					this.led_Strip.TurnAllLedsOn();
					Thread.Sleep(1000);
				}

				this.led_Strip.TurnAllLedsOff();
			};

			this.rs4852.Port.DataReceived += (sender, data) =>
			{
				if (sender.ReadByte() == (byte)'Q')
					this.rs4852.Port.Write((byte)'X');
			};
			
			this.button.ButtonReleased += (sender, e) =>
			{
				this.rs485.Port.Write((byte)'Q');
			};
		}
	}
}
