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
        GT.Timer t = new GT.Timer(35);

        GTM.GHIElectronics.PulseCount pulseCount = new GTM.GHIElectronics.PulseCount(9);

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            t.Tick += new GT.Timer.TickEventHandler(t_Tick);
            t.Start();

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        void t_Tick(GT.Timer timer)
        {
            int reading = pulseCount.ReadEncoders();
            Debug.Print(reading.ToString());
        }

    }
}
