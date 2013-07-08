using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace TestApp
{
	public partial class Program
	{
		// This method is run when the mainboard is powered up or reset.   
		void ProgramStarted()
		{
			new Thread(() =>
			{
				int i = 0;
				bool next = false;
				while (true)
				{
					led_Strip[i++] = next;

					if (i >= led_Strip.LedCount)
					{
						i = 0;
						next = !next;
					}

					Thread.Sleep(250);
				}
			}).Start();
		}
	}
}
