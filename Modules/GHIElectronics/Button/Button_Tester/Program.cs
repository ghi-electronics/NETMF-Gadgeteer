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

namespace Button_Tester
{
    public partial class Program
    {
        private GT.Timer timer;
        private int next;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("Button Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.Setup(this.button1);
            this.Setup(this.button2);
            this.Setup(this.button3);
            this.Setup(this.button4);
            this.Setup(this.button5);
            this.Setup(this.button6);
            this.Setup(this.button7);
            this.Setup(this.button8);
            this.Setup(this.button9);
            this.Setup(this.button10);

            this.timer = new GT.Timer(50);
            this.timer.Tick += (a) =>
                {
                    this.Clear();

                    if (this.button1.Pressed) this.Write("Button 1 Pressed.");
                    if (this.button2.Pressed) this.Write("Button 2 Pressed.");
                    if (this.button3.Pressed) this.Write("Button 3 Pressed.");
                    if (this.button4.Pressed) this.Write("Button 4 Pressed.");
                    if (this.button5.Pressed) this.Write("Button 9 Pressed.");
                    if (this.button6.Pressed) this.Write("Button 10 Pressed.");
                    if (this.button7.Pressed) this.Write("Button 11 Pressed.");
                    if (this.button8.Pressed) this.Write("Button 12 Pressed.");
                    if (this.button9.Pressed) this.Write("Button 13 Pressed.");
                    if (this.button10.Pressed) this.Write("Button 18 Pressed.");
                };
            this.timer.Start();
        }

        private void Setup(Button button)
        {
            button.ButtonPressed += (a, b) => button.TurnLedOn();
            button.ButtonReleased += (a, b) => button.TurnLedOff();
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
