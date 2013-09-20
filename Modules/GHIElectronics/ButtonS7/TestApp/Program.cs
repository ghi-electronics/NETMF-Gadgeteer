using System.Threading;
using Microsoft.SPOT;

using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace TestApp
{
	public partial class Program
	{
		void ProgramStarted()
		{
			this.buttonS7.EnterPressed += (sender, state) => { Debug.Print("Enter Pressed"); };
			this.buttonS7.EnterReleased += (sender, state) => { Debug.Print("Enter Released"); };
			
			new Thread(() =>
			{
				while (true)
				{
					if (this.buttonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Back)) Debug.Print("Back");
					if (this.buttonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Forward)) Debug.Print("Forward");
					if (this.buttonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Left)) Debug.Print("Left");
					if (this.buttonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Right)) Debug.Print("Right");
					if (this.buttonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Up)) Debug.Print("Up");
					if (this.buttonS7.IsPressed(GTM.GHIElectronics.ButtonS7.Buttons.Down)) Debug.Print("Down");
					Thread.Sleep(50);
				}
			}).Start();
		}
	}
}
