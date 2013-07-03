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
        public static GTM.GHIElectronics.FEZtive feztive;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            // Create an instance of the module (ONLY DO THIS IF YOU ARE NOT USING THE DESIGNER)
            feztive = new GTM.GHIElectronics.FEZtive(9); 
            
            // Initialize the module to use 80 LEDs, with an SPI clock rate of 4Mhz
            feztive.Initialize(80, 1000);

            // Set every LED to Black (off)
            feztive.SetAll(feztive.Black);

            // Set every LED to Red
            feztive.SetAll(feztive.Red);
            
            // Set every LED to Green
            feztive.SetAll(feztive.Green);
            
            // Set every LED to Blue
            feztive.SetAll(feztive.Blue);
            
            // Set every LED to White
            feztive.SetAll(feztive.White);

            // Set every LED to Black (off)
            feztive.SetAll(feztive.Black);



            // Set LED 42 (43rd LED) to Blue
            feztive.SetLED(feztive.Blue, 42);

            // Set LED 75 (76th LED) to Green
            feztive.SetLED(feztive.Green, 75);

            // Set LED 12 (13th LED) to Red
            feztive.SetLED(feztive.Red, 12);

            // Set LED 1 (2nd LED) to White
            feztive.SetLED(feztive.White, 1);

            // Set LED 79 (80th and last LED) to Blue
            feztive.SetLED(feztive.Blue, 79);

            // Set LED 57 (58th LED) to Blue
            feztive.SetLED(feztive.Green, 57);

            // Create your own colors
            GTM.GHIElectronics.Color blue = new GTM.GHIElectronics.Color(0, 0, 127);
            GTM.GHIElectronics.Color green = new GTM.GHIElectronics.Color(0, 127, 0);

            // Set LED 10 (11th LED) to your created color
            feztive.SetLED(green, 10);
            feztive.SetLED(blue, 10);


            // Retrieves the array of Color structures representing the current display
            GTM.GHIElectronics.Color[] curr = feztive.GetCurrentColors();

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }
    }
}
