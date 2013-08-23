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
		void ProgramStarted()
		{
			display_N181.SimpleGraphics.DisplayEllipse(GT.Color.Red, 10, 10, 10, 10);
			display_N182.SimpleGraphics.DisplayEllipse(GT.Color.Blue, 10, 10, 10, 10);

			for (uint x = 10; x <= 118; x++)
			{
				display_N183.SimpleGraphics.ClearNoRedraw();
				display_N183.SimpleGraphics.DisplayEllipse(GT.Color.Green, x, 10, 10, 10);
			}
		}
	}
}
