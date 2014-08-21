using Gadgeteer.Modules.GHIElectronics;
using System.Threading;
using GT = Gadgeteer;

namespace KeypadKP16_Tester
{
    public partial class Program
    {
        private int next;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("KeypadKP16 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            new Thread(() =>
            {
                while (true)
                {
                    this.Clear();

                    if (keypadKP16.IsPressed(KeypadKP16.Key.A)) this.Write("A Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.B)) this.Write("B Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.C)) this.Write("C Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.D)) this.Write("D Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.Pound)) this.Write("Pound Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.Star)) this.Write("Star Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.Zero)) this.Write("Zero Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.One)) this.Write("One Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.Two)) this.Write("Two Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.Three)) this.Write("Three Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.Four)) this.Write("Four Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.Five)) this.Write("Five Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.Six)) this.Write("Six Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.Seven)) this.Write("Seven Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.Eight)) this.Write("Eight Pressed.");
                    if (keypadKP16.IsPressed(KeypadKP16.Key.Nine)) this.Write("Nine Pressed.");

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
