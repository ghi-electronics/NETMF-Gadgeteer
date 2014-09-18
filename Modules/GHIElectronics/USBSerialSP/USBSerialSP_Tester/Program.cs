using System.Threading;

using GT = Gadgeteer;

namespace USBSerialSP_Tester
{
    public partial class Program
    {
        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("USBSerialSP Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);

            var buffer = new byte[512];

            this.usbSerialSP.Configure();
            this.usbSerialSP.Port.DataReceived += a =>
            {
                var read = a.Read(buffer, 0, buffer.Length);
                a.Write(buffer, 0, read);
            };
        }
    }
}
