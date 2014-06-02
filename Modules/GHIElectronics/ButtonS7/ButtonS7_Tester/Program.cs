using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace ButtonS7_Tester
{
    public partial class Program
    {
        private int next;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("ButtonS7 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            new Thread(() =>
                {
                    while (true)
                    {
                        this.Clear();

                        if (buttonS7.IsPressed(ButtonS7.Button.Back)) this.Write("Back Pressed.");
                        if (buttonS7.IsPressed(ButtonS7.Button.Forward)) this.Write("Forward Pressed.");
                        if (buttonS7.IsPressed(ButtonS7.Button.Left)) this.Write("Left Pressed.");
                        if (buttonS7.IsPressed(ButtonS7.Button.Right)) this.Write("Right Pressed.");
                        if (buttonS7.IsPressed(ButtonS7.Button.Up)) this.Write("Up Pressed.");
                        if (buttonS7.IsPressed(ButtonS7.Button.Down)) this.Write("Down Pressed.");
                        if (buttonS7.IsPressed(ButtonS7.Button.Enter)) this.Write("Enter Pressed.");

                        Thread.Sleep(15);
                    }
                }).Start();
        }

        private void Write(string text)
        {
            this.displayT43.SimpleGraphics.DisplayText(text, Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 15 * this.next++);
        }

        private void Clear()
        {
            this.next = 0;
            this.displayT43.SimpleGraphics.Clear();
        }
    }
}
