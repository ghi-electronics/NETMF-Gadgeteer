using System.Threading;
using Microsoft.SPOT;

using GTM = Gadgeteer.Modules;

namespace TestApp
{
	public partial class Program
	{
		void ProgramStarted()
		{
			this.ButtonS7.EnterPressed += (sender, state) => { Debug.Print("Enter Pressed"); };
			this.ButtonS7.EnterReleased += (sender, state) => { Debug.Print("Enter Released"); };

			new Thread(() =>
			{
				while (true)
				{
					if (this.ButtonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Back)) Debug.Print("Back");
					if (this.ButtonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Forward)) Debug.Print("Forward");
					if (this.ButtonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Left)) Debug.Print("Left");
					if (this.ButtonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Right)) Debug.Print("Right");
					if (this.ButtonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Up)) Debug.Print("Up");
					if (this.ButtonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Down)) Debug.Print("Down");
					Thread.Sleep(50);
				}
			}).Start();
		}
	}
}
