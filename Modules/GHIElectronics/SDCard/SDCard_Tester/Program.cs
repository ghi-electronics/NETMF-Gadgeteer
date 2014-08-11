using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;
using System;
using System.Text;
using System.Threading;
using GT = Gadgeteer;

namespace SDCard_Tester
{
    public partial class Program
    {
        private bool first;
        private string str;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("SDCard Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(500);

            this.first = true;

            if (this.sdCard.IsCardMounted)
                this.sdCard_Mounted(this.sdCard, this.sdCard.StorageDevice);

            this.sdCard.Mounted += this.sdCard_Mounted;
            this.sdCard.Unmounted += this.sdCard_Unmounted;
        }

        void sdCard_Unmounted(SDCard sender, EventArgs e)
        {
            if (this.first)
            {
                this.first = false;

                this.sdCard.Mount();
            }
            else
            {
                this.first = true;
            }
        }

        private void sdCard_Mounted(SDCard sender, GT.StorageDevice e)
        {
            if (this.first)
            {
                this.displayT43.SimpleGraphics.Clear();
                this.displayT43.SimpleGraphics.DisplayText("Inserted", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);

                this.str = DateTime.UtcNow.ToString();

                e.WriteFile("Test.txt", Encoding.UTF8.GetBytes(this.str));
            }
            else
            {
                if (new string(Encoding.UTF8.GetChars(e.ReadFile("Test.txt"))) == this.str)
                {
                    this.displayT43.SimpleGraphics.Clear();
                    this.displayT43.SimpleGraphics.DisplayText("Passed", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
                }

                e.Delete("Test.txt");
            }

            this.sdCard.Unmount();
        }
    }
}
