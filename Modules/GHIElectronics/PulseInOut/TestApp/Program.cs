using Microsoft.SPOT;

using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;
using System.Threading;

namespace TestApp
{
    public partial class Program
    {
        GTM.GHIElectronics.PulseInOut pio = new GTM.GHIElectronics.PulseInOut(6);

        //GTI.PWMOutput pwm = new GTI.PWMOutput(Gadgeteer.Socket.GetSocket(11, true, null, null), Gadgeteer.Socket.Pin.Seven, false, null);

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
            pio.SetFrequency(50);

            for (; ; )
            {
                //ushort pulseLen = 0;

                for (ushort i = 1100; i < 1900; i++)
                {
                    Debug.Print(i.ToString());
                    pio.SetPulse(1, i);

                    Thread.Sleep(50);
                    int high = 0;
                    int low = 0;

                    pio.ReadChannel(8, out high, out low);


                    //pio.SetPulse(2, i);
                    //pio.SetPulse(3, i);
                    //pio.SetPulse(4, i);
                    //pio.SetPulse(5, i);
                    //pio.SetPulse(6, i);
                    //pio.SetPulse(7, i);
                    //pio.SetPulse(8, i);
                }
            }

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }
    }
}
