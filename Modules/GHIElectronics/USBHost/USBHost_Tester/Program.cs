using System.Threading;
using GT = Gadgeteer;

namespace USBHost_Tester
{
    public partial class Program
    {
        private GT.Timer timer;
        private GHI.Usb.Host.Mouse mouse;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("USBHost Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.timer = new GT.Timer(25);
            this.timer.Tick += (a) =>
                {
                    this.displayT43.SimpleGraphics.Clear();
                    this.displayT43.SimpleGraphics.DisplayText("X: " + this.mouse.CursorPosition.X.ToString() + " Y: " + this.mouse.CursorPosition.Y.ToString(), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
                };

            this.usbHost.MouseConnected += (a, b) =>
                {
                    this.mouse = b;
                    this.timer.Start();
                };

            this.usbHost.MouseDisconnected += (a, b) =>
                {
                    this.timer.Stop();

                    this.displayT43.SimpleGraphics.Clear();
                    this.displayT43.SimpleGraphics.DisplayText("Plug in a mouse.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
                };

            this.displayT43.SimpleGraphics.Clear();
            this.displayT43.SimpleGraphics.DisplayText("Plug in a mouse.", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
        }
    }
}
