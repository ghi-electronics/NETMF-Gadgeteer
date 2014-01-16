using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A GasSense module for Microsoft .NET Gadgeteer
    /// </summary>
    public class GasSense : GTM.Module
    {
        // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
        // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2C), you must do a few more steps
        // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
        // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
        // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

        GTI.AnalogInput ain;
        GTI.DigitalOutput heatingElementEnable;

        /// <summary>Constructor</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public GasSense(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('A', this);

            ain = new GTI.AnalogInput(socket, Socket.Pin.Three, this);

            heatingElementEnable = new GTI.DigitalOutput(socket, Socket.Pin.Four, false, this);
        }

        /// <summary>
        /// Returns a value describing the reading of the air.
        /// </summary>
        /// <returns>Value between 0.0 and 3.3</returns>
        public double ReadVoltage()
        {
            return (ain.ReadVoltage());
        }

        /// <summary>
        /// Turns the heating element on or off. This may take up to 10 seconds befre a proper reading is taken.
        /// </summary>
        /// <param name="bOn">True to turn the element on, false to turn it off.</param>
        public void SetHeatingElement(bool bOn)
        {
            heatingElementEnable.Write(bOn);
        }
    }
}