using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;
using Microsoft.SPOT.Touch;
using System.Threading;
using GT = Gadgeteer;

namespace DisplayTE35_Tester
{
    public partial class Program
    {
        void ProgramStarted()
        {
            this.displayTE35.SimpleGraphics.DisplayText("DisplayTE35 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            Touch.Initialize(new TouchConnection(this.displayTE35));

            TouchCollectorConfiguration.CollectionMode = CollectionMode.InkOnly;
            TouchCollectorConfiguration.CollectionMethod = CollectionMethod.Native;
        }
    }

    public class TouchConnection : IEventListener
    {
        private DisplayTE35 displayTE35;

        public TouchConnection(DisplayTE35 displayTE35)
        {
            this.displayTE35 = displayTE35;
        }

        public void InitializeForEventSource() 
        { 

        }

        public bool OnEvent(BaseEvent baseEvent)
        {
            if (baseEvent is TouchEvent && baseEvent.EventMessage == 1)
            {
                var e = (TouchEvent)baseEvent;
                this.displayTE35.SimpleGraphics.DisplayEllipse(GT.Color.Red, 1, GT.Color.Red, e.Touches[0].X - 3, e.Touches[0].Y + 3, 3, 3);
                this.displayTE35.SimpleGraphics.DisplayEllipse(GT.Color.Green, 1, GT.Color.Green, e.Touches[0].X + 3, e.Touches[0].Y + 3, 3, 3);
                this.displayTE35.SimpleGraphics.DisplayEllipse(GT.Color.Blue, 1, GT.Color.Blue, e.Touches[0].X, e.Touches[0].Y - 3, 3, 3);
            }

            return true;
        }
    }
}
