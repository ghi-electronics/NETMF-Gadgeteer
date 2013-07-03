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

namespace TestApp_42
{
    public partial class Program
    {
        public static GTM.GHIElectronics.Stepper_L6470 stepper;// = new GTM.GHIElectronics.Stepper_L6470(6);

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
            stepper = new GTM.GHIElectronics.Stepper_L6470(6);

            UInt32 test = stepper.GetParam(GTM.GHIElectronics.Stepper_L6470.Registers.CONFIG);
            if (test != 0x2E88)
            {
                throw new Exception("Check wires");
            }


            stepper.Move(GTM.GHIElectronics.Stepper_L6470.Direction.FWD, 1000);

            stepper.Run(GTM.GHIElectronics.Stepper_L6470.Direction.FWD, uint.MaxValue);

            stepper.HardStop();

            stepper.GoMark();

            stepper.Run(GTM.GHIElectronics.Stepper_L6470.Direction.FWD, uint.MaxValue);

            stepper.ResetPos();

            stepper.SoftStop();

            stepper.GoHome();

            stepper.NOP();

            stepper.GoToPosition(1000);

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }
    }
}
