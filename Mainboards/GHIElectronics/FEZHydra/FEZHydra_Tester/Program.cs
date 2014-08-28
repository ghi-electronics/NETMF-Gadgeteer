using GHI.Pins;
using Microsoft.SPOT.Hardware;
using System.Collections;
using GT = Gadgeteer;

namespace FEZHydra_Tester
{
    public partial class Program
    {
        private ArrayList outputs;
        private GT.Timer timer;
        private bool next;

        void ProgramStarted()
        {
            this.timer = new GT.Timer(500);
            this.outputs = new ArrayList();
            this.next = false;

            this.outputs.Add(new OutputPort(Generic.GetPin('B', 8), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 9), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 12), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 13), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 2), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 11), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 12), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 14), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 9), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 22), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 21), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 10), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 23), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 24), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 17), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 13), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 14), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 29), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 30), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 19), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 6), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 7), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 20), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 14), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 15), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 16), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 11), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 0), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 3), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 1), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 4), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 5), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 2), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 9), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 10), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 12), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 1), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 3), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 4), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 2), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 22), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 23), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 24), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 25), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 20), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 4), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 5), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 15), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 16), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 17), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 18), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 19), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 21), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 3), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 9), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 10), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 11), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 12), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 13), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 7), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('C', 6), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 6), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 20), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 18), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 1), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 28), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 26), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 29), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('D', 7), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 19), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('A', 17), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 0), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 30), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 31), !this.next));
            this.outputs.Add(new OutputPort(Generic.GetPin('B', 27), !this.next));

            this.timer.Tick += (a) =>
            {
                Mainboard.SetDebugLED(this.next);

                foreach (OutputPort i in this.outputs)
                    i.Write(this.next);

                this.next = !this.next;
            };

            this.timer.Start();
        }
    }
}
