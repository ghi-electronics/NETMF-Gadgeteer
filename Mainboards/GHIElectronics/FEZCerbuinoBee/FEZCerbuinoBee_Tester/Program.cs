using GHI.IO.Storage;
using GHI.Pins;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.IO;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

namespace FEZCerbuinoBee_Tester
{
    public class Program
    {
        private static InterruptPort sdCardDetect;
        private static OutputPort debugLed;
        private static AutoResetEvent sdEvt;
        private static ArrayList outputs;
        private static Thread worker;
        private static Thread timer;
        private static bool sdSuccess;

        public static void Main()
        {
            sdCardDetect = new InterruptPort(Generic.GetPin('C', 2), true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
            sdEvt = new AutoResetEvent(false);

            RemovableMedia.Insert += (e, f) => { Debug.Print("Inserted"); sdEvt.Set(); };
            RemovableMedia.Eject += (e, f) => { Debug.Print("Ejected"); sdEvt.Reset(); };

            debugLed = new OutputPort(Generic.GetPin('B', 2), false);

            sdSuccess = false;

            outputs = new ArrayList();

            outputs.Add(new OutputPort(Generic.GetPin('A', 14), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 10), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 11), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 13), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 5), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 4), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 3), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 6), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 2), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 3), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 1), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 0), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 7), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 6), false));
            outputs.Add(new OutputPort(Generic.GetPin('C', 0), false));
            outputs.Add(new OutputPort(Generic.GetPin('C', 1), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 4), false));
            outputs.Add(new OutputPort(Generic.GetPin('C', 5), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 8), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 7), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 9), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 12), false));
            outputs.Add(new OutputPort(Generic.GetPin('C', 14), false));
            outputs.Add(new OutputPort(Generic.GetPin('C', 15), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 8), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 10), false));
            outputs.Add(new OutputPort(Generic.GetPin('C', 4), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 13), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 9), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 15), false));
            outputs.Add(new OutputPort(Generic.GetPin('B', 1), false));
            outputs.Add(new OutputPort(Generic.GetPin('A', 5), false));
            outputs.Add(new OutputPort(Generic.GetPin('C', 3), false));

            timer = new Thread(() =>
            {
                while (true)
                {
                    Debug.GC(true);

                    foreach (OutputPort i in outputs)
                        i.Write(true);

                    Thread.Sleep(125);

                    foreach (OutputPort i in outputs)
                        i.Write(false);

                    Thread.Sleep(125);

                    debugLed.Write(sdSuccess);
                }
            });
            timer.Start();

            worker = new Thread(() =>
            {
                while (!sdSuccess)
                {
                    if (!sdSuccess && !sdCardDetect.Read())
                    {
                        Thread.Sleep(1000);

                        var str = DateTime.UtcNow.ToString();

                        using (var rs = new SDCard())
                        {
                            rs.Mount();

                            sdEvt.WaitOne();

                            using (var fs = new FileStream("\\SD\\Test.txt", FileMode.OpenOrCreate))
                            {
                                fs.Position = 0;
                                fs.Write(Encoding.UTF8.GetBytes(str), 0, str.Length);
                            }

                            rs.Unmount();

                            rs.Mount();

                            sdEvt.WaitOne();

                            using (var fs = new FileStream("\\SD\\Test.txt", FileMode.Open))
                            {
                                var buffer = new byte[str.Length];
                                fs.Read(buffer, 0, str.Length);
                                sdSuccess = new string(Encoding.UTF8.GetChars(buffer)) == str;
                            }

                            rs.Unmount();
                        }
                    }

                    Thread.Sleep(100);
                }

                worker = null;
            });
            worker.Start();
        }
    }
}