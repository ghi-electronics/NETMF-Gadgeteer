using Microsoft.SPOT;

using GTM = Gadgeteer.Modules;

namespace TestApp
{
    public partial class Program
    {
        GTM.GHIElectronics.Thermocouple therm = new GTM.GHIElectronics.Thermocouple(11);

        void ProgramStarted()
        {
            short c;
            short f;
            byte i;

            c = therm.GetExternalTemp_Celsius();
            f = therm.GetExternalTemp_Fahrenheit();
            i = therm.GetInternalTemp_Celsius();

            Debug.Print("C: " + c);
            Debug.Print("F: " + f);
            Debug.Print("I: " + i);
        }
    }
}
