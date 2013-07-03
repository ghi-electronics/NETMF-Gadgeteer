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
        GT.Timer t = new GT.Timer(500);
        //Microsoft.SPOT.Hardware.OutputPort _counter_en_pin = new Microsoft.SPOT.Hardware.OutputPort((Microsoft.SPOT.Hardware.Cpu.Pin)7, true);
        //Microsoft.SPOT.Hardware.OutputPort _counter_en_pin = new Microsoft.SPOT.Hardware.OutputPort((Microsoft.SPOT.Hardware.Cpu.Pin)(1*16 +11), true);
        // Y testing
       // GTM.GHIElectronics.RotaryEncoder rotaryEncoder = new GTM.GHIElectronics.RotaryEncoder(5);

        // S testing
        GTM.GHIElectronics.RotaryEncoder rotaryEncoder= new GTM.GHIElectronics.RotaryEncoder(9);

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            //rotaryEncoder.SetCountMode(GTM.GHIElectronics.RotaryEncoder.CountMode.Quad1);

            //display_T35.WPFWindow.TouchDown += new Microsoft.SPOT.Input.TouchEventHandler(WPFWindow_TouchDown);
           // t.Tick += new GT.Timer.TickEventHandler(t_Tick);
           // t.Start();

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
			new Thread(() =>
			{
				while (true)
				{
					char_Display.Clear();
					char_Display.CursorHome();
					char_Display.PrintString(rotaryEncoder.ReadEncoders().ToString());
					char_Display.SetCursor(1, 0);
					char_Display.PrintString(rotaryEncoder.ReadDirection().ToString());
					Thread.Sleep(250);
				}
			}).Start();
        }

		/*
        void WPFWindow_TouchDown(object sender, Microsoft.SPOT.Input.TouchEventArgs e)
        {
            rotaryEncoder.Initialize();
        }
        Font fnt = Resources.GetFont(Resources.FontResources.small);
        Font fntB = Resources.GetFont(Resources.FontResources.NinaB);
        void t_Tick(GT.Timer timer)
        {
            int reading = rotaryEncoder.ReadEncoders();
            byte status = rotaryEncoder.ReadStatusReg();
            byte dir = rotaryEncoder.ReadDirection();
            //char_Display.Clear();
            //char_Display.PrintString(reading.ToString());
            Debug.Print("value: " + reading.ToString() + ", status : " + (status & 0x1) + ", " + dir);
        }
        void ThreadTest()
        {
            display_T35.SimpleGraphics.Clear();
            display_T35.SimpleGraphics.DisplayText("- Potary H1 Module Tester --", fntB, GT.Color.White, 50, 20);
            int reading =0;
             byte status;
             int old_value = 0;
            int old_status = 0;
             rotaryEncoder.Initialize();
            while (true)
            {
                //Thread.Sleep(100);
                if (rotaryEncoder != null)
                {
                  
                    reading = rotaryEncoder.ReadEncoders();
                    if (reading == 0)
                    {
                        rotaryEncoder.Initialize();
                    }
                    status = rotaryEncoder.ReadStatusReg();
                    if (old_value != reading || status != old_status)
                    {
                        old_value = reading;
                        old_status = status;
                        display_T35.SimpleGraphics.DisplayRectangle(Colors.Black, 100, Colors.Black, 10, 100, 310, 20);
                    }
                    if (status == 0)
                    {
                        // rotaryEncoder.Initialize();
                        //display_T35.SimpleGraphics.DisplayRectangle(Colors.Black, 100, Colors.Black, 10, 80, 310, 20);
                        display_T35.SimpleGraphics.DisplayText("Potary disconnected." , fntB, GT.Color.Red, 10, 80);
                        //Thread.Sleep(100);
                        //rotaryEncoder.Initialize();

                    }
                    else
                    {
                        String text_dir = "No direction";
                        if (reading!=0)
                        {
                            if (rotaryEncoder.ReadDirection() > 0) text_dir = "Count up";
                         if (rotaryEncoder.ReadDirection() == 0) text_dir = "Count down";
                        }

                        display_T35.SimpleGraphics.DisplayText("- Value : " + reading + " => " + text_dir , fntB, GT.Color.White, 10, 80);
                    }
                   
                   
                }
                Thread.Sleep(10);
            }
            //display_T35.SimpleGraphics.DisplayText("- Connect SerCam module to extender module 1.", fnt, GT.Color.Blue, 10, 40);
            //display_T35.SimpleGraphics.DisplayText("- Connect extender module 1 to USB Client DP.", fnt, GT.Color.Blue, 10, 60);
            //display_T35.SimpleGraphics.DisplayText("- Connect extender module 1 pin 4, 5 to pin 4, 5", fnt, GT.Color.Blue, 10, 80);
            //display_T35.SimpleGraphics.DisplayText("of extender module 2.", fnt, GT.Color.Blue, 10, 100);
            //display_T35.SimpleGraphics.DisplayText("- Connect extender module 1 pin 4, 5 to pin 4, 5 ", fnt, GT.Color.Blue, 10, 120);
            //display_T35.SimpleGraphics.DisplayText("of extender module 2.", fnt, GT.Color.Blue, 10, 140);
            //display_T35.SimpleGraphics.DisplayText("- Connect extender module 2 to spider socket 8.", fnt, GT.Color.Blue, 10, 160);
            //display_T35.SimpleGraphics.DisplayText("- After 3-4 seconds, LCD screen will show image.", fnt, GT.Color.Blue, 10, 180);

            display_T35.SimpleGraphics.DisplayText("- Connect SerCam module to socket 8.", fnt, GT.Color.Blue, 10, 40);
            int cnt = 0;
        }*/
    }
}
