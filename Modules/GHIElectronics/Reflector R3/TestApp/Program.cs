using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace TestApp
{
	public partial class Program
	{
		// This method is run when the mainboard is powered up or reset.   
		void ProgramStarted()
		{
			var reflector = reflector_R3;
			new Thread(() =>
			{
				while (true)
				{
					Debug.Print("Left: " + reflector.Read(Gadgeteer.Modules.GHIElectronics.Reflector_R3.Reflectors.Left).ToString("F3") + " Center: " + reflector.Read(Gadgeteer.Modules.GHIElectronics.Reflector_R3.Reflectors.Center).ToString("F3") + " Right: " + reflector.Read(Gadgeteer.Modules.GHIElectronics.Reflector_R3.Reflectors.Right).ToString("F3"));
					Thread.Sleep(500);
				}
			}).Start();
		}
	}
}
