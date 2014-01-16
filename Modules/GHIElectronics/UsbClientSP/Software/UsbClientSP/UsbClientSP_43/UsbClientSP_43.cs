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
    /// Represents a USB Device module that allows the mainboard to be programmed over USB.  
    /// Red USB Device modules also apply USB power (5V) to the mainboard and peripherals. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="UsbClientSP"/> class has no programmable functionality other than internally reserving 
    /// the pins used by the USB Device module, which allows programming of the mainboard.  
    /// Red USB Device modules also provide power to the mainboard and peripherals from the USB power source.  
    /// Important: only plug in one Red module at a time.
    /// </para>
    /// <para>
    /// You may also be interested in other USB module types listed below.
    /// </para>
    /// <list type="bullet">
    ///  <item><see cref="T:Gadgeteer.Modules.USBSerial"/></item>
    ///  <item><see cref="T:Gadgeteer.Modules.USBHost"/></item>
    /// </list>
    /// </remarks>
    public class UsbClientSP : GTM.Module
    {
        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public UsbClientSP(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('D', this);
            socket.ReservePin(Socket.Pin.Three, this);
            socket.ReservePin(Socket.Pin.Four, this);
            socket.ReservePin(Socket.Pin.Five, this);
            socket.ReservePin(Socket.Pin.Six, this);
            socket.ReservePin(Socket.Pin.Seven, this);
        }
    }
}
