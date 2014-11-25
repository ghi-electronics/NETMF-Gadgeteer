using System;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

using System.Threading;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2C), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// A Distance US3 module for Microsoft .NET Gadgeteer
    /// </summary>
    [Obsolete]
    public class DistanceUS3 : GTM.Module
    {
        private GTI.DigitalInput Echo;
        private GTI.DigitalOutput Trigger;

        private readonly int TicksPerMicrosecond = (int)(TimeSpan.TicksPerMillisecond / 1000);

        /// <summary>
        /// Number of errors that can be accumulated in the GetDistanceInCentimeters function before the function returns an error value;
        /// </summary>
        public int AcceptableErrorRate = 10;

        /// <summary>
        /// Error that will be returned if the sensor fails to get an accurate reading after the AcceptableErrorRate has been reached.
        /// </summary>
        public readonly int SENSOR_ERROR = -1;

        /// <summary>Constructor</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public DistanceUS3(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported(new char[] { 'X', 'Y' }, this);

            Echo = new GTI.DigitalInput(socket, Socket.Pin.Three, GTI.GlitchFilterMode.Off, GTI.ResistorMode.Disabled, this);
            Trigger = new GTI.DigitalOutput(socket, Socket.Pin.Four, false, this);
        }

        /// <summary>
        /// Takes a number of measurements, averaging the distance, and returns the detected distance in centimeters.
        /// </summary>
        /// <param name="numMeasurements">The number of measurements to take and average before returning a value.</param>
        /// <returns>Distance that the module has detected an object in front of it. Will return SENSOR_ERROR if the number of errors 
        /// was reached, which can be caused by an object either being too close or too far from the sensor.</returns>
        public int GetDistanceInCentimeters(int numMeasurements = 1)
        {
            int measuredValue = 0;
            int measuredAverage = 0;
            int errorCount = 0;

            for (int i = 0; i < numMeasurements; i++)
            {
                measuredValue = GetDistanceHelper();

                if (measuredValue != MaxFlag || measuredValue != MinFlag)
                {
                    measuredAverage += measuredValue;
                }
                else
                {
                    errorCount++;
                    i--;

                    if (errorCount > AcceptableErrorRate)
                    {
                        return SENSOR_ERROR;
                    }
                }

            }

            measuredAverage /= numMeasurements;
            return measuredAverage;
        }

        private const int MIN_DISTANCE = 2;
        private const int MAX_DISTANCE = 400;

        private const int MaxFlag = -1;
        private const int MinFlag = -2;

        private int GetDistanceHelper()
        {
            long start = 0;
            int microseconds = 0;
            long time = 0;
            int distance = 0;

            Trigger.Write(true);
            Thread.Sleep(10);
            Trigger.Write(false);

            int error = 0;
            while (!Echo.Read())
            {
                error++;
                if (error > 1000)
                    break;
                Thread.Sleep(0);
            }

            start = System.DateTime.Now.Ticks;

            while (Echo.Read())
                Thread.Sleep(0);

            time = (System.DateTime.Now.Ticks - start);
            microseconds = (int)time / TicksPerMicrosecond;

            distance = (microseconds / 58);
            distance += 2;

            if (distance < MAX_DISTANCE)
            {
                if (distance >= MIN_DISTANCE)
                    return distance;
                else
                    return MinFlag;
            }
            else
            {
                return MaxFlag;
            }
        }
    }
}
