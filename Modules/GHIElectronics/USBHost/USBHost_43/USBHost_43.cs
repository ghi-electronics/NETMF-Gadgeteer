using GHI.IO.Storage;
using GHI.Usb;
using GHI.Usb.Host;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using System;
using System.Collections;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A USBHost module for Microsoft .NET Gadgeteer
    /// </summary>
    public class USBHost : GTM.Module
    {
        private static bool firstHostModule;

        static USBHost()
        {
            USBHost.firstHostModule = true;
        }

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The mainboard socket that has the module plugged into it.</param>
        public USBHost(int socketNumber)
        {
            if (!USBHost.firstHostModule) throw new InvalidOperationException("Only one USB host module may be connected in the designer at a time. If you have multiple host modules, just connect one of the modules in the designer. It will receive the events for both modules.");

            USBHost.firstHostModule = false;

            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('H', this);

            socket.ReservePin(Socket.Pin.Three, this);
            socket.ReservePin(Socket.Pin.Four, this);
            socket.ReservePin(Socket.Pin.Five, this);

            RemovableMedia.Insert += this.OnInsert;
            RemovableMedia.Eject += this.OnEject;

            Controller.MouseConnected += (a, b) => this.OnMouseConnected(this, b);
            Controller.KeyboardConnected += (a, b) => this.OnKeyboardConnected(this, b);
            Controller.MassStorageConnected += (a, b) => b.Mount();
            Controller.Start();
        }

        private void OnInsert(object sender, MediaEventArgs e)
        {
            if (e.Volume.Name.Length >= 3 && e.Volume.Name.Substring(0, 3) == "USB")
            {
                if (e.Volume.FileSystem != null)
                {
                    this.OnMassStorageConnected(this, new StorageDevice(e.Volume));
                }
                else
                {
                    this.ErrorPrint("Unable to mount the USB drive. Is it formatted as FAT32?");
                }
            }
        }

        private void OnEject(object sender, MediaEventArgs e)
        {
            if (e.Volume.Name.Length >= 3 && e.Volume.Name.Substring(0, 3) == "USB")
                this.OnMassStorageDisconnected(this, null);
        }

        /// <summary>
        /// Represents the delegate that is used for the <see cref="MassStorageConnected"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="USBHost"/> object that raised the event.</param>
        /// <param name="storageDevice">The <see cref="T:Microsoft.Gadgeteer.StorageDevice"/> object associated with the connected USB drive.</param>
        public delegate void MassStorageConnectedEventHandler(USBHost sender, StorageDevice storageDevice);

        /// <summary>
        /// Represents the delegate that is used for the <see cref="MassStorageDisconnected"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="USBHost"/> object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void MassStorageDisconnectedEventHandler(USBHost sender, EventArgs e);

        /// <summary>
        /// Represents the delegate that is used for the <see cref="MouseConnected"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="USBHost"/> object that raised the event.</param>
        /// <param name="mouse">The <see cref="Mouse"/> object associated with the event.</param>
        public delegate void MouseConnectedEventHandler(USBHost sender, Mouse mouse);

        /// <summary>
        /// Represents the delegate that is used to handle the <see cref="KeyboardConnected"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="USBHost"/> object that raised the event.</param>
        /// <param name="keyboard">The <see cref="Keyboard"/> object associated with the event.</param>
        public delegate void KeyboardConnectedEventHandler(USBHost sender, Keyboard keyboard);

        /// <summary>
        /// Raised when a USB drive is connected.
        /// </summary>
        public event MassStorageConnectedEventHandler MassStorageConnected;

        /// <summary>
        /// Raised when a USB drive is disconnected.
        /// </summary>
        public event MassStorageDisconnectedEventHandler MassStorageDisconnected;

        /// <summary>
        /// Raised when a USB mouse is connected.
        /// </summary>
        public event MouseConnectedEventHandler MouseConnected;

        /// <summary>
        /// Raised when a USB keyboard is connected.
        /// </summary>
        public event KeyboardConnectedEventHandler KeyboardConnected;

        private MassStorageConnectedEventHandler onMassStorageConnected;
        private MassStorageDisconnectedEventHandler onMassStorageDisconnected;
        private MouseConnectedEventHandler onMouseConnected;
        private KeyboardConnectedEventHandler onKeyboardConnected;

        private void OnMassStorageConnected(USBHost sender, StorageDevice storageDevice)
        {
            if (this.onMassStorageConnected == null)
                this.onMassStorageConnected = this.OnMassStorageConnected;

            if (Program.CheckAndInvoke(this.MassStorageConnected, this.onMassStorageConnected, sender, storageDevice))
                this.MassStorageConnected(sender, storageDevice);
        }

        private void OnMassStorageDisconnected(USBHost sender, EventArgs e)
        {
            if (this.onMassStorageDisconnected == null)
                this.onMassStorageDisconnected = this.OnMassStorageDisconnected;

            if (Program.CheckAndInvoke(this.MassStorageDisconnected, this.onMassStorageDisconnected, sender, e))
                this.MassStorageDisconnected(sender, e);
        }

        private void OnMouseConnected(USBHost sender, Mouse mouse)
        {
            if (this.onMouseConnected == null)
                this.onMouseConnected = this.OnMouseConnected;

            if (Program.CheckAndInvoke(this.MouseConnected, this.onMouseConnected, sender, mouse))
                this.MouseConnected(sender, mouse);
        }

        private void OnKeyboardConnected(USBHost sender, Keyboard keyboard)
        {
            if (this.onKeyboardConnected == null)
                this.onKeyboardConnected = this.OnKeyboardConnected;

            if (Program.CheckAndInvoke(this.KeyboardConnected, this.onKeyboardConnected, sender, keyboard))
                this.KeyboardConnected(sender, keyboard);
        }
    }
}