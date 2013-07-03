using System.Threading;
using GTM = Gadgeteer.Modules;

namespace TestApp
{
    public partial class Program
    {
        GTM.GHIElectronics.RotaryEncoder rotaryEncoder= new GTM.GHIElectronics.RotaryEncoder(4);

        void ProgramStarted()
        {
            new Thread(ThreadTest).Start();
        }
        
        void ThreadTest()
        {
            int reading = 0;
            byte status;
            rotaryEncoder.Initialize();

            while (true)
            {
                reading = rotaryEncoder.ReadEncoders();
                status = rotaryEncoder.ReadStatusReg();

                char_Display.Clear();
                char_Display.CursorHome();
                char_Display.PrintString(rotaryEncoder.ReadDirection() > 0 ? "Up" : "Down");
                char_Display.SetCursor(1, 0);
                char_Display.PrintString(reading.ToString());

                Thread.Sleep(500);
            }
        }
    }
}
