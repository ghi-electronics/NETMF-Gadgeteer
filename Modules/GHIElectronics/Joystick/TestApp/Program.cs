using System.Threading;
using Microsoft.SPOT;

namespace TestApp
{
	public partial class Program
	{
		void ProgramStarted()
		{
			joystick.Calibrate();
			joystick.JoystickPressed += (sender, state) => { Debug.Print("Pressed!"); };
			joystick.JoystickReleased += (sender, state) => { Debug.Print("Released!"); };

			Thread joystickThread = new Thread(joystickReadThread);
			joystickThread.Start();
		}

		void joystickReadThread()
		{
			double xPos = 0.0;
			double yPos = 0.0;

			while (true)
			{
				xPos = joystick.GetPosition().X;
				yPos = joystick.GetPosition().Y;
				Debug.Print("X: " + xPos + " Y: " + yPos);
				Thread.Sleep(100);
			}
		}
	}
}
