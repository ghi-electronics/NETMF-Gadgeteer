using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A DL40 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class DL40 : GTM.DaisyLinkModule
    {
        private const byte GHI_DAISYLINK_MANUFACTURER = 0x10;
        private const byte GHI_DAISYLINK_TYPE_GENERIC = 0x01;
        private const byte GHI_DAISYLINK_VERSION_GENERIC = 0x01;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public DL40(int socketNumber) : base(socketNumber, DL40.GHI_DAISYLINK_MANUFACTURER, DL40.GHI_DAISYLINK_TYPE_GENERIC, DL40.GHI_DAISYLINK_VERSION_GENERIC, DL40.GHI_DAISYLINK_VERSION_GENERIC, 50, "DL40")
        {

        }

        /// <summary>
        /// Writes to the byte to the specified address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="value">The value to write.</param>
        public void WriteRegister(byte address, byte value)
        {
            this.Write((byte)(GTM.DaisyLinkModule.DaisyLinkOffset + address), value);
        }

        /// <summary>
        /// Reads a byte at the specified address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <returns>The read value.</returns>
        public byte ReadRegister(byte address)
        {
            return this.Read(address);
        }
    }
}
