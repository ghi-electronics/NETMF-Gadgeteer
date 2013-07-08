using System.Threading;
using Microsoft.SPOT;

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
					Debug.Print(lightSensor.ReadLightSensorPercentage().ToString());
					Thread.Sleep(250);
				}
			}).Start();
        }
    }
}