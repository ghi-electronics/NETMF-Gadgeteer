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
    /// A light sensor module for Microsoft .NET Gadgeteer.
    /// </summary>
    /// <example>
    /// <para>The following example uses a <see cref="LightSensor"/> object to read and display the current amount of light to the output window. 
    /// </para>
    /// <code>
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
    ///             double lightPercent = lightSensor.ReadLightSensorPercentage();
    ///
    ///             Debug.Print(lightPercent.ToString());
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class LightSensor : GTM.Module
    {
        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public LightSensor(int socketNumber)
        {
            // This finds the Socket instance from the user-specified socket number.  
            // This will generate user-friendly error messages if the socket is invalid.
            // If there is more than one socket on this module, then instead of "null" for the last parameter, 
            // put text that identifies the socket to the user (e.g. "S" if there is a socket type S)
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            socket.EnsureTypeIsSupported('A', this);

            this.input = new GTI.AnalogInput(socket, Socket.Pin.Three, this);
        }

        private GTI.AnalogInput input;

        /// <summary>
        /// Returns the current voltage reading of the light sensor
		/// </summary>
		/// <returns>A voltage reading between 0 and 3.3.</returns>
        public double ReadLightSensorVoltage()
        {
            return input.ReadVoltage();
        }

        /// <summary>
        ///  Returns the current strength of the light relative to its maximum: range 0.0 to 100.0
        /// </summary>
		/// <returns>A percentage between 0 and 100.</returns>
        public double ReadLightSensorPercentage()
        {
            return (input.ReadProportion() * 100);
        }

		/// <summary>
		/// Returns the current sensor reading in lux.
		/// </summary>
		/// <returns>A reading in lux between 0 and MAX_ILLUMINANCE.</returns>
		public double GetIlluminance()
		{
			return this.input.ReadProportion() * LightSensor.MAX_ILLUMINANCE;
		}

		/// <summary>
		/// The maximum amount of lux the sensor can detect before becoming saturated.
		/// </summary>
		public const double MAX_ILLUMINANCE = 750;
    }
}
