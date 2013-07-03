using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2C), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// A LED Strip module for Microsoft .NET Gadgeteer
    /// </summary>
    public class LED_Strip : GTM.Module
    {
        private GTI.DigitalOutput[] LEDs = new GTI.DigitalOutput[7];

        public readonly uint MAX_VALUE = 0x7F;

        /*/// <summary>
        /// Enum for the types of bitwise operations that can be performed on this module.
        /// </summary>
        public enum Bitwise_Operation
        {
            /// <summary>
            /// A bitwise AND takes two binary representations of equal length and 
            /// performs the logical AND operation on each pair of corresponding bits.
            /// </summary>
            Bitwise_AND = 0, 

            /// <summary>
            /// A bitwise OR takes two bit patterns of equal length and performs the 
            /// logical inclusive OR operation on each pair of corresponding bits.
            /// </summary>
            Bitwise_OR = 1,

            /// <summary>
            /// The bitwise NOT, or complement, is an unary operation that performs 
            /// logical negation on each bit, forming the ones' complement of the given 
            /// binary value. Bits that are 0 become 1, and those that are 1 become 0.
            /// </summary>
            Bitwise_NOT = 2,

            Bitwise_XOR = 3
        }*/

        /// <summary>Constructor</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public LED_Strip(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('Y', this);

            for (int i = 0; i < 7; i++)
            {
                LEDs[i] = new GTI.DigitalOutput(socket, Socket.Pin.Three + i, false, this);
            }
        }

        /// <summary>
        /// Turns a specified LED on, indicated by the numbering on the PCB.
        /// </summary>
        /// <param name="led">LED to turn on.</param>
        public void TurnLEDOn(int led)
        {
            if (led > 6)
                throw new ArgumentOutOfRangeException();

            LEDs[led].Write(true);
        }

        /// <summary>
        /// Turns a specified LED off, indicated by the numbering on the PCB.
        /// </summary>
        /// <param name="led">LED to turn off.</param>
        public void TurnLEDOff(int led)
        {
            if (led > 6)
                throw new ArgumentOutOfRangeException();

            LEDs[led].Write(false);
        }

        /// <summary>
        /// Sets an LED to the specified state.
        /// </summary>
        /// <param name="led">LED to change. Indicated be the numbering on the PCB.</param>
        /// <param name="state">State to set. True is on. False is off.</param>
        public void SetLED(int led, bool state)
        {
            if (led > 6)
                throw new ArgumentOutOfRangeException();

            LEDs[led].Write(state);
        }

        /// <summary>
        /// Sets the LEDs on the module to the value passed in.
        /// </summary>
        /// <param name="mask">The bit mask to set the LEDs to.</param>
        public void SetBitmask(uint mask)
        {
            if (mask > MAX_VALUE)
                throw new ArgumentOutOfRangeException();

            uint value = 1;

            for (uint i = 0; i < 7; i++)
            {
                if ((mask & value) == value)
                    LEDs[i].Write(true);
                else
                    LEDs[i].Write(false);

                value = value << 1;
            }
        }

        /*/// <summary>
        /// Sets the LEDs on the module depending on the current state of the module and the operation passed in.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="op"></param>
        public void SetBitmask(uint mask, Bitwise_Operation op)
        {
            if (mask > MAX_VALUE)
                throw new ArgumentOutOfRangeException();
        }*/
    }
}
