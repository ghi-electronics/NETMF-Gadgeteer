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

namespace testapp
{
    public partial class Program
    {
        public static GTM.GHIElectronics.SerCam sercam;

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
            sercam = new GTM.GHIElectronics.SerCam(8);

            sercam.SetImageSize(SerCam.Camera_Resolution.SIZE_QVGA);
            sercam.PictureCaptured += new SerCam.PictureCapturedEventHandler(sercam_PictureCaptured);
            
            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            sercam.TakePicture();
        }

        void sercam_PictureCaptured(GTM.GHIElectronics.SerCam sender, Bitmap picture)
        {
            Debug.GC(true);
            display_T35.SimpleGraphics.DisplayImage(picture, 0, 0);
        }
    }
}
