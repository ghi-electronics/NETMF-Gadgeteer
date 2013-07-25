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

        void ProgramStarted()
        {
            //GT.Interfaces.InterruptInput interrupt;
			GTM.GHIElectronics.HubAP5 hubAP5 = new GTM.GHIElectronics.HubAP5(2);
			//GTM.GHIElectronics.LED_Strip led_Strip = new GTM.GHIElectronics.LED_Strip(hubAP5.HubSocket1);
			//GTM.GHIElectronics.LED_Strip led_Strip1 = new GTM.GHIElectronics.LED_Strip(hubAP5.HubSocket2);
			//GTM.GHIElectronics.LED_Strip led_Strip2 = new GTM.GHIElectronics.LED_Strip(hubAP5.HubSocket3);
			//GTM.GHIElectronics.LED_Strip led_Strip3 = new GTM.GHIElectronics.LED_Strip(hubAP5.HubSocket4);
			//GTM.GHIElectronics.LED_Strip led_Strip4 = new GTM.GHIElectronics.LED_Strip(hubAP5.HubSocket5);
			//GTM.GHIElectronics.LED_Strip led_Strip5 = new GTM.GHIElectronics.LED_Strip(hubAP5.HubSocket6);
			//GTM.GHIElectronics.LED_Strip led_Strip6 = new GTM.GHIElectronics.LED_Strip(hubAP5.HubSocket7);
			//GTM.GHIElectronics.LED_Strip led_Strip7 = new GTM.GHIElectronics.LED_Strip(hubAP5.HubSocket8);

            //var dOut = new Thread(() =>
            //    {
            //        int next = 0;
            //        while (true)
            //        {
            //            led_Strip.TurnLEDOn(next);
            //            led_Strip1.TurnLEDOn(next);
            //            led_Strip2.TurnLEDOn(next);
            //            led_Strip3.TurnLEDOn(next);
            //            led_Strip4.TurnLEDOn(next);
            //            led_Strip5.TurnLEDOn(next);
            //            led_Strip6.TurnLEDOn(next);
            //            led_Strip7.TurnLEDOn(next);
            //
            //            next += 1;
            //
            //            Thread.Sleep(200);
            //
            //            if (next > 6)
            //            {
            //                next = 0;
            //                for (int i = 0; i < 7; i++)
            //                {
            //                    led_Strip.TurnLEDOff(i);
            //                    led_Strip1.TurnLEDOff(i);
            //                    led_Strip2.TurnLEDOff(i);
            //                    led_Strip3.TurnLEDOff(i);
            //                    led_Strip4.TurnLEDOff(i);
            //                    led_Strip5.TurnLEDOff(i);
            //                    led_Strip6.TurnLEDOff(i);
            //                    led_Strip7.TurnLEDOff(i);
            //                }
            //            }
            //        }
            //    });
            //dOut.Start();


            //var dIn = new Thread(() =>
            //    {
            //        var socket = GT.Socket.GetSocket(hubAP5.HubSocket2, true, null, null);
            //        var inputs = new GT.Interfaces.DigitalInput[] {
            //                                                          new GT.Interfaces.DigitalInput(socket, GT.Socket.Pin.Three, GT.Interfaces.GlitchFilterMode.On, GT.Interfaces.ResistorMode.PullUp, null),
            //                                                          new GT.Interfaces.DigitalInput(socket, GT.Socket.Pin.Four, GT.Interfaces.GlitchFilterMode.On, GT.Interfaces.ResistorMode.PullUp, null),
            //                                                          new GT.Interfaces.DigitalInput(socket, GT.Socket.Pin.Five, GT.Interfaces.GlitchFilterMode.On, GT.Interfaces.ResistorMode.PullUp, null),
            //                                                          new GT.Interfaces.DigitalInput(socket, GT.Socket.Pin.Six, GT.Interfaces.GlitchFilterMode.On, GT.Interfaces.ResistorMode.PullUp, null),
            //                                                          new GT.Interfaces.DigitalInput(socket, GT.Socket.Pin.Seven, GT.Interfaces.GlitchFilterMode.On, GT.Interfaces.ResistorMode.PullUp, null),
            //                                                          new GT.Interfaces.DigitalInput(socket, GT.Socket.Pin.Eight, GT.Interfaces.GlitchFilterMode.On, GT.Interfaces.ResistorMode.PullUp, null),
            //                                                          new GT.Interfaces.DigitalInput(socket, GT.Socket.Pin.Nine, GT.Interfaces.GlitchFilterMode.On, GT.Interfaces.ResistorMode.PullUp, null)
            //                                                      };
            //
            //        while (true)
            //        {
            //            for (int i = 0; i < 7; i++)
            //                if (!inputs[i].Read())
            //                    led_Strip.TurnLEDOn(i);
            //                else
            //                    led_Strip.TurnLEDOff(i);
            //
            //            Thread.Sleep(50);
            //        }
            //    });
            //dIn.Start();


            //var pwmOut = new Thread(() =>
            //{
            //    var socket = GT.Socket.GetSocket(hubAP5.HubSocket6, true, null, null);
            //    var pwms = new GT.Interfaces.PWMOutput[] 
            //    {
            //        new GT.Interfaces.PWMOutput(socket, GT.Socket.Pin.Seven, false, null),
            //        new GT.Interfaces.PWMOutput(socket, GT.Socket.Pin.Eight, false, null),
            //        new GT.Interfaces.PWMOutput(socket, GT.Socket.Pin.Nine, false, null)
            //    };
            //
            //    int next = 400;
            //    double dutyCycle = 0.5;
            //    int i = 0;
            //    while (true)
            //    {
            //        foreach (var pwm in pwms)
            //            pwm.Set(next + i++ * 250, dutyCycle);
            //
            //        i = 0;
            //        next += 10;
            //        if (next > 1000)
            //            next = 400;
            //
            //        Thread.Sleep(25);
            //    }
            //});
            //pwmOut.Start();


            //var aIn = new Thread(() =>
            //{
            //    var ains = new GT.Interfaces.AnalogInput[] 
            //    {
            //        new GT.Interfaces.AnalogInput(GT.Socket.GetSocket(hubAP5.HubSocket1, true, null, null), GT.Socket.Pin.Three, null)/*,
            //        new GT.Interfaces.AnalogInput(GT.Socket.GetSocket(hubAP5.HubSocket1, true, null, null), GT.Socket.Pin.Four, null),
            //        new GT.Interfaces.AnalogInput(GT.Socket.GetSocket(hubAP5.HubSocket1, true, null, null), GT.Socket.Pin.Five, null),
            //        new GT.Interfaces.AnalogInput(GT.Socket.GetSocket(hubAP5.HubSocket2, true, null, null), GT.Socket.Pin.Three, null),
            //        new GT.Interfaces.AnalogInput(GT.Socket.GetSocket(hubAP5.HubSocket2, true, null, null), GT.Socket.Pin.Four, null),
            //        new GT.Interfaces.AnalogInput(GT.Socket.GetSocket(hubAP5.HubSocket2, true, null, null), GT.Socket.Pin.Five, null)*/
            //    };
            //
            //    while (true)
            //    {
            //        string result = "";
            //        foreach (var a in ains)
            //            result += a.ReadVoltage().ToString() + " ";
            //
            //        Debug.Print(result);
            //
            //        Thread.Sleep(1000);
            //    }
            //});
            //aIn.Start();

			//var dOut = new GT.Interfaces.DigitalOutput(GT.Socket.GetSocket(hubAP5.HubSocket1, true, null, null), GT.Socket.Pin.Four, false, null);
			//
			//new Thread(() =>
			//{
			//	while (true)
			//	{
			//		dOut.Write(true);
			//		Thread.Sleep(500);
			//		dOut.Write(false);
			//		Thread.Sleep(500);
			//	}
			//}).Start();
			//
            //interrupt = new GT.Interfaces.InterruptInput(GT.Socket.GetSocket(hubAP5.HubSocket1, true, null, null), GT.Socket.Pin.Three, GT.Interfaces.GlitchFilterMode.Off, GT.Interfaces.ResistorMode.PullUp, GT.Interfaces.InterruptMode.RisingAndFallingEdge, null);
            //interrupt.Interrupt += (sender, state) =>
            //{
            //    Debug.Print(DateTime.Now.ToString() + " " + state.ToString() + " " + Program.CheckAndInvoke(null, null).ToString());
            //};
        }
    }
}
