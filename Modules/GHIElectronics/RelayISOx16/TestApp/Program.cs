using Microsoft.SPOT;

using GTM = Gadgeteer.Modules;

namespace TestApp
{
    public partial class Program
    {
        GTM.GHIElectronics.RelayISOx16 relay = new GTM.GHIElectronics.RelayISOx16(14);

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
            relay.EnableOutputs();

            relay.EnableRelay(GTM.GHIElectronics.RelayISOx16.Relay.Relay_14);

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }
    }
}
