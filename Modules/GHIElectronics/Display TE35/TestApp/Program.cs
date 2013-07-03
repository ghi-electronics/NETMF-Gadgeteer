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

namespace TestApp42
{
    public partial class Program
    {
        GTM.GHIElectronics.Display_TE35 display = new GTM.GHIElectronics.Display_TE35(12, 13, 14, 10);

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/
            Bitmap HydraLCD = new Bitmap(SystemMetrics.ScreenWidth, SystemMetrics.ScreenHeight);
        
            int maxX = (SystemMetrics.ScreenWidth - 1);
            int maxY = (SystemMetrics.ScreenHeight - 1);
            //for(int y = 0; y < SystemMetrics.ScreenHeight; y++)
            //    HydraLCD.DrawLine(Color.White, 1, 0, y, maxX, y);

            HydraLCD.DrawLine(Color.White, 1, 0, 0, maxX, 0);
            HydraLCD.DrawLine(Color.White, 1, 0, 0, 0, maxY);


            HydraLCD.DrawLine(Color.White, 1, maxX, 0, maxX, maxY);
            HydraLCD.DrawLine(Color.White, 1, 0, maxY, maxX, maxY); // bottom

            HydraLCD.Flush();


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }
    }
}
