using System;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
    /// <summary>A CurrentACS712 module for Microsoft .NET Gadgeteer</summary>
    public class CurrentACS712 : GTM.Module {
        private GTI.AnalogInput input;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public CurrentACS712(int socketNumber) {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('A', this);

            this.input = GTI.AnalogInputFactory.Create(socket, Socket.Pin.Five, this);
        }

        /// <summary>Reads the alternating current value.</summary>
        /// <returns>The AC reading.</returns>
        public double ReadACCurrent() {
            return this.ReadACCurrent(25);
        }

        /// <summary>Reads the alternating current value using an average of multiple samples.</summary>
        /// <param name="samples">The number of times to sample the sensor.</param>
        /// <returns>The AC reading.</returns>
        public double ReadACCurrent(int samples) {
            if (samples < 1) throw new ArgumentOutOfRangeException("samples", "samples must be at least one.");

            var sum = 0.0;

            for (int i = 0; i < samples; i++)
                sum += Math.Abs(this.input.ReadVoltage() - 2.083);

            sum /= samples;
            sum /= 0.116;

            return sum;
        }

        /// <summary>Reads the direct current value.</summary>
        /// <returns>The DC reading.</returns>
        public double ReadDCCurrent() {
            return this.ReadDCCurrent(25);
        }

        /// <summary>Reads the direct current value using an average of multiple samples.</summary>
        /// <param name="samples">The number of times to sample the sensor.</param>
        /// <returns>The DC reading.</returns>
        public double ReadDCCurrent(int samples) {
            if (samples < 1) throw new ArgumentOutOfRangeException("samples", "samples must be at least one.");

            var sum = 0.0;

            for (var i = 0; i < samples; i++) {
                sum += this.input.ReadVoltage();

                Thread.Sleep(2);
            }

            sum /= samples;

            sum -= 2.083;
            sum /= 0.116;

            return sum;
        }
    }
}