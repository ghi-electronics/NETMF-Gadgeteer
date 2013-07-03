using GTM = Gadgeteer.Modules;

using Microsoft.SPOT.Hardware;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A LED7R module for Microsoft .NET Gadgeteer
    /// </summary>
    public class LED7R : GTM.Module
    {
        private OutputPort[] leds = new OutputPort[7];

        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public LED7R(int socketNumber)
        {
            // This finds the Socket instance from the user-specified socket number.  
            // This will generate user-friendly error messages if the socket is invalid.
            // If there is more than one socket on this module, then instead of "null" for the last parameter, 
            // put text that identifies the socket to the user (e.g. "S" if there is a socket type S)
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('Y', this);

            for (int i = 0; i < leds.Length; i++)
            {
                leds[i] = new OutputPort(socket.CpuPins[i + 3], false);
            }
        }

        /// <summary>
        /// Turns a light on.
        /// </summary>
        /// <param name="light">The light to turn on, as marked on the board.</param>
        /// <param name="onlyLight">Set to true if the passed in light is the only one that should be on. Defaulted to false.</param>
        public void TurnLightOn(int light, bool onlyLight = false)
        {
            light--;

            if (light < 0 || light > 6)
                return;

            if (onlyLight)
            {
                for (int i = 0; i < leds.Length; i++)
                {
                    if (light == i)
                    {
                        leds[i].Write(true);
                    }
                    else
                    {
                        leds[i].Write(false);
                    }
                }
            }
            else
            {
                leds[light].Write(true);
            }
        }

        /// <summary>
        /// Turns a light off.
        /// </summary>
        /// <param name="light">The light to turn off, as marked on the board.</param>
        public void TurnLightOff(int light)
        {
            light--;

            if (light < 0 || light > 6)
                return;

            leds[light].Write(false);
        }

        /// <summary>
        /// Animates the lights on the board, according to the passed in values.
        /// </summary>
        /// <param name="switchTime">Time between operation of each light in milliseconds</param>
        /// <param name="clockwise">True if the animation should play in a clockwise motion.</param>
        /// <param name="on">True if the animation should turn the lights on, false if the lights should be turned off.</param>
        /// <param name="remainOn">True if a light should remain on when another one is lit, false if only one light should be lit at a time.</param>
        public void Animate(int switchTime, bool clockwise, bool on, bool remainOn)
        {
            int length = leds.Length - 1;
            int i;
            int terminate;
            int dir;

            if (clockwise)
            {
                i = 0;
                terminate = length;
                dir = 1;
            }
            else
            {
                i = length - 1;
                terminate = -1;
                dir = -1;
            }

            for (; i != terminate; i += dir)
            {
                if (on)
                {
                    TurnLightOn(i + 1, !remainOn);
                }
                else
                {
                    TurnLightOff(i + 1);
                }

                System.Threading.Thread.Sleep(switchTime);
            }
        }
    }
}
