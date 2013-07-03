using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using System;

namespace TestApp
{
    public partial class Program
    {
        GTM.GHIElectronics.FLASH flash = new GTM.GHIElectronics.FLASH(9);

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
            byte[] buffer = new byte[1024 * 4];
            SetArray(buffer, 0x00);

            Debug.Print("Get Identify...");
            //flash.EraseBlock(63, 1);
            flash.EraseChip();
            flash.EraseSector(start_sec, 1);
            SetArray(buffer, 0x00);
            buffer = flash.ReadData((start_sec * 1024 * 4), buffer.Length);
            for (int j = 0; j < buffer.Length; j++)
            {
                if (buffer[j] != 0xFF)
                {
                    throw new Exception("Read whole chip fail  ");
                }
            }
            SetArray(buffer, 0x10);
            flash.WriteData(start_sec * 1024 * 4, buffer);
            SetArray(buffer, 0x00);
            buffer = flash.ReadData((start_sec * 1024 * 4), buffer.Length);
            for (int j = 0; j < buffer.Length; j++)
            {
                if (buffer[j] != 0x10)
                {
                    throw new Exception("Read whole chip fail  ");
                }
            }

            Debug.Print("Write / Read last sector is good");

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        int start_sec = 0;

        public void SetArray(byte[] data, byte value)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = value;
            }
        }
    }
}
