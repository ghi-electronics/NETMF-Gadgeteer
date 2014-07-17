using System.Threading;
using GT = Gadgeteer;

namespace AccelG248_Tester
{
    public partial class Program
    {
        private static int GRAPH_HEIGHT = 80;
        private static int GRAPH_SPACING = 8;
        private static int TEXT_HEIGHT = 15;

        private int xCoord;
        private GT.Timer timer;
        private GT.Color[] colors;
        private double[] last;

        void ProgramStarted()
        {
            this.displayT43.SimpleGraphics.DisplayText("AccelG248 Tester", Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, 0);
            Thread.Sleep(2000);

            this.xCoord = int.MaxValue;
            this.last = new double[] { 0, 0, 0 };
            this.colors = new GT.Color[] { GT.Color.Red, GT.Color.Green, GT.Color.Blue };

            this.timer = new GT.Timer(25);
            this.timer.Tick += (a) =>
            {
                var acc = this.accelG248.GetAcceleration();

                this.displayT43.SimpleGraphics.DisplayRectangle(GT.Color.Black, 1, GT.Color.Black, 0, this.displayT43.Height - Program.TEXT_HEIGHT, this.displayT43.Width, Program.TEXT_HEIGHT);
                this.displayT43.SimpleGraphics.DisplayText(acc.ToString(), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, 0, this.displayT43.Height - Program.TEXT_HEIGHT);

                this.CheckReset();
                this.Draw(0, acc.X);
                this.Draw(1, acc.Y);
                this.Draw(2, acc.Z);
                this.xCoord++;
            };
            this.timer.Start();
        }

        private void DrawAxes()
        {
            for (int i = 0; i < 3; i++)
            {
                this.displayT43.SimpleGraphics.DisplayLine(GT.Color.White, 1, 0, i * (Program.GRAPH_HEIGHT + Program.GRAPH_SPACING), 0, i * (Program.GRAPH_HEIGHT + Program.GRAPH_SPACING) + Program.GRAPH_HEIGHT);
                this.displayT43.SimpleGraphics.DisplayLine(GT.Color.White, 1, 0, i * (Program.GRAPH_HEIGHT + Program.GRAPH_SPACING) + Program.GRAPH_HEIGHT / 2, this.displayT43.Width, i * (Program.GRAPH_HEIGHT + Program.GRAPH_SPACING) + Program.GRAPH_HEIGHT / 2);
            }
        }

        private void Draw(int axis, double value)
        {
            if (value < -1.0)
                value = -1.0;

            if (value > 1.0)
                value = 1.0;

            value += 1;
            value /= 2;
            value = 1 - value;
            value *= Program.GRAPH_HEIGHT;

            this.displayT43.SimpleGraphics.DisplayLine(this.colors[axis], 1, this.xCoord, axis * (Program.GRAPH_HEIGHT + Program.GRAPH_SPACING) + (int)this.last[axis], this.xCoord + 1, axis * (Program.GRAPH_HEIGHT + Program.GRAPH_SPACING) + (int)value);

            this.last[axis] = value;
        }

        private void CheckReset()
        {
            if (this.xCoord > this.displayT43.Width)
            {
                this.xCoord = 1;
                this.displayT43.SimpleGraphics.Clear();
                this.DrawAxes();
            }
        }
    }
}
