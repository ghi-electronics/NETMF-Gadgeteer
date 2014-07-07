using Gadgeteer.Modules.GHIElectronics;
using System.Threading;
using GT = Gadgeteer;

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

                        if (this.buttonS7.IsPressed(ButtonS7.Button.Back)) this.Write("Back Pressed.");
                        if (this.buttonS7.IsPressed(ButtonS7.Button.Forward)) this.Write("Forward Pressed.");
                        if (this.buttonS7.IsPressed(ButtonS7.Button.Left)) this.Write("Left Pressed.");
                        if (this.buttonS7.IsPressed(ButtonS7.Button.Right)) this.Write("Right Pressed.");
                        if (this.buttonS7.IsPressed(ButtonS7.Button.Up)) this.Write("Up Pressed.");
                        if (this.buttonS7.IsPressed(ButtonS7.Button.Down)) this.Write("Down Pressed.");
                        if (this.buttonS7.IsPressed(ButtonS7.Button.Enter)) this.Write("Enter Pressed.");

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
