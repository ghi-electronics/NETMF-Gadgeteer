using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.SocketInterfaces;

using Microsoft.SPOT.Hardware;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2CBus), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// A module with 7 controllable LEDs for Microsoft .NET Gadgeteer.
    /// </summary>
    /// <example>
    /// <para>The following example uses a <see cref="LED7R"/> object to light each individual LED. 
    /// </para>
    /// <code>
    /// <![CDATA[    
    /// using System;
    /// using System.Collections;
    /// using System.Threading;
    /// using Microsoft.SPOT;
    /// using Microsoft.SPOT.Presentation;
    /// using Microsoft.SPOT.Presentation.Controls;
    /// using Microsoft.SPOT.Presentation.Media;
    /// using Microsoft.SPOT.Touch;
    ///
    /// using Gadgeteer.Networking;
    /// using GT = Gadgeteer;
    /// using GTM = Gadgeteer.Modules;
    /// using Gadgeteer.Modules.GHIElectronics;
    ///
    /// namespace TestApp
    /// {
    ///     public partial class Program
    ///     {
    ///         void ProgramStarted()
    ///         {
    ///             Thread ledThread = new Thread(ledWriteThread);
    ///             ledThread.Start();
    ///         }
    ///
    ///         void ledWriteThread()
    ///         {
    ///             while (true)
    ///             {
    ///                 for (int i = 0; i < 7; i++)
    ///                     led7r.TurnLightOn(i, true);
    ///             }
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public class LED7R : GTM.Module
    {
        private GTI.DigitalOutput[] leds = new GTI.DigitalOutput[7];

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
                leds[i] = GTI.DigitalOutputFactory.Create(socket, Socket.Pin.Three + i, false, this);
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
