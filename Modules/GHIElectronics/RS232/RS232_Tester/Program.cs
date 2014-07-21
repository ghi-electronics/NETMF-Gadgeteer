using System.Threading;
using GT = Gadgeteer;

namespace RS232_Tester
{
    public partial class Program
    {
        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("RS232 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(500);

            this.rs2321.Configure();
            this.rs2322.Configure();

            this.rs2322.Port.LineReceived += (a, b) => this.rs2322.Port.WriteLine(b);

            this.rs2321.Port.LineReceived += (a, b) =>
            {
                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText(b == "Hello, World!" ? "Passed" : "Failed", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            };

            this.rs2321.Port.WriteLine("Hello, World!");
        }
    }
}
