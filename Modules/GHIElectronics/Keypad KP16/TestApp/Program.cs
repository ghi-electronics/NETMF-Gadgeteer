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
using Gadgeteer.Modules.GHIElectronics;

namespace TestApp
{
	public partial class Program
	{
		void ProgramStarted()
		{
			new Thread(() =>
			{
				while (true)
				{
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.A)) Debug.Print("A");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.B)) Debug.Print("B");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.C)) Debug.Print("C");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.D)) Debug.Print("D");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.Pound)) Debug.Print("#");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.Star)) Debug.Print("*");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.Zero)) Debug.Print("0");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.One)) Debug.Print("1");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.Two)) Debug.Print("2");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.Three)) Debug.Print("3");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.Four)) Debug.Print("4");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.Five)) Debug.Print("5");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.Six)) Debug.Print("6");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.Seven)) Debug.Print("7");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.Eight)) Debug.Print("8");
					if (keypad_KP16.IsKeyPressed(GTM.GHIElectronics.Keypad_KP16.Key.Nine)) Debug.Print("9");

					Thread.Sleep(100);
				}
			}).Start();
		}
	}
}
