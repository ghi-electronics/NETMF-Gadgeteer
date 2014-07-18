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

        private Hashtable mice;
        private Hashtable keyboards;
        private Hashtable storageDevices;

        static USBHost()
        {
            USBHost.firstHostModule = true;
        }

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The mainboard socket that has the module plugged into it.</param>
        public USBHost(int socketNumber)
        {
            if (!USBHost.firstHostModule) throw new Exception("Only one USB host module may be connected in the designer at a time. If you have multiple host modules, just connect one of the modules in the designer. It will receive the events for both modules.");
            
            USBHost.firstHostModule = false;

            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('H', this);

            socket.ReservePin(Socket.Pin.Three, this);
            socket.ReservePin(Socket.Pin.Four, this);
            socket.ReservePin(Socket.Pin.Five, this);

            this.mice = new Hashtable();
            this.keyboards = new Hashtable();
            this.storageDevices = new Hashtable();

            RemovableMedia.Insert += OnInsert;
            RemovableMedia.Eject += OnEject;

            Controller.DeviceConnected += OnDeviceConnected;
        }

        private void OnDeviceConnected(object sender, Controller.DeviceConnectedEventArgs e)
        {
            var device = e.Device;
            device.Disconnected += OnDeviceDisconnected;

            switch (device.Type)
            {
                case Device.DeviceType.MassStorage:
                    lock (this.storageDevices)
                    {
                        var ps = new UsbMassStorage(device);
                        ps.Mount();
                        this.storageDevices.Add(device.Id, ps);
                    }

                    break;

                case Device.DeviceType.Mouse:
                    lock (this.mice)
                    {
                        var mouse = new Mouse(device);
                        mouse.SetCursorBounds(new Position() { X = int.MinValue, Y = int.MinValue }, new Position() { X = int.MaxValue, Y = int.MaxValue });
                        this.mice.Add(device.Id, mouse);
                        this.OnMouseConnected(this, mouse);
                    }

                    break;

                case Device.DeviceType.Keyboard:
                    lock (this.keyboards)
                    {
                        var keyboard = new Keyboard(device);
                        this.keyboards.Add(device.Id, keyboard);
                        this.OnKeyboardConnected(this, keyboard);
                    }

                    break;

                case Device.DeviceType.Webcam:
                    this.DebugPrint("Use GTM.GHIElectronics.Camera for USB WebCamera support.");
                    break;

                default:
                    DebugPrint("USB device is not supported by the Gadgeteer driver. More devices are supported by the GHI USB Host driver. Remove the USB Host from the designer, and proceed without using Gadgeteer code.");
                    break;
            }
        }

        private void OnDeviceDisconnected(Device device, EventArgs e)
        {
            switch (device.Type)
            {
                case Device.DeviceType.MassStorage:
                    lock (this.storageDevices)
                    {
                        if (!this.storageDevices.Contains(device.Id))
                            return;

                        var ps = (GHI.IO.Storage.Removable)this.storageDevices[device.Id];
                        ps.Unmount();
                        ps.Dispose();
                        this.storageDevices.Remove(device.Id);
                    }

                    break;

                case Device.DeviceType.Mouse:
                    lock (this.mice)
                    {
                        if (!this.mice.Contains(device.Id))
                            return;

                        var mouse = (Mouse)this.mice[device.Id];
                        this.OnMouseDisconnected(this, mouse);
                        this.mice.Remove(device.Id);
                    }

                    break;

                case Device.DeviceType.Keyboard:
                    lock (this.keyboards)
                    {
                        if (!this.keyboards.Contains(device.Id))
                            return;

                        var keyboard = (Keyboard)this.keyboards[device.Id];
                        this.OnKeyboardDisconnected(this, keyboard);
                        this.keyboards.Remove(device.Id);
                    }

                    break;
            }
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
            {
                this.OnMassStorageDisconnected(this);
            }
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
        public delegate void MassStorageDisconnectedEventHandler(USBHost sender);

        /// <summary>
        /// Represents the delegate that is used for the <see cref="MouseConnected"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="USBHost"/> object that raised the event.</param>
        /// <param name="mouse">The <see cref="Mouse"/> object associated with the event.</param>
        public delegate void MouseConnectedEventHandler(USBHost sender, Mouse mouse);

        /// <summary>
        /// Represents the delegate that is used for the <see cref="MouseDisconnected"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="USBHost"/> object that raised the event.</param>
        /// <param name="mouse">The <see cref="Mouse"/> object associated with the event.</param>
        public delegate void MouseDisconnectedEventHandler(USBHost sender, Mouse mouse);

        /// <summary>
        /// Represents the delegate that is used to handle the <see cref="KeyboardConnected"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="USBHost"/> object that raised the event.</param>
        /// <param name="keyboard">The <see cref="Keyboard"/> object associated with the event.</param>
        public delegate void KeyboardConnectedEventHandler(USBHost sender, Keyboard keyboard);

        /// <summary>
        /// Represents the delegate that is used for the <see cref="KeyboardDisconnected"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="USBHost"/> object that raised the event.</param>
        /// <param name="keyboard">The <see cref="Keyboard"/> object associated with the event.</param>
        public delegate void KeyboardDisconnectedEventHandler(USBHost sender, Keyboard keyboard);

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
        /// Raised when a USB mouse is disconnected.
        /// </summary>
        public event MouseDisconnectedEventHandler MouseDisconnected;

        /// <summary>
        /// Raised when a USB keyboard is connected.
        /// </summary>
        public event KeyboardConnectedEventHandler KeyboardConnected;

        /// <summary>
        /// Raised when a USB keyboard is disconnected.
        /// </summary>
        public event KeyboardDisconnectedEventHandler KeyboardDisconnected;

        private MassStorageConnectedEventHandler onMassStorageConnected;
        private MassStorageDisconnectedEventHandler onMassStorageDisconnected;
        private MouseConnectedEventHandler onMouseConnected;
        private MouseDisconnectedEventHandler onMouseDisconnected;
        private KeyboardConnectedEventHandler onKeyboardConnected;
        private KeyboardDisconnectedEventHandler onKeyboardDisconnected;

        private void OnMassStorageConnected(USBHost sender, StorageDevice storageDevice)
        {
            if (this.onMassStorageConnected == null) 
                this.onMassStorageConnected = this.OnMassStorageConnected;

            if (Program.CheckAndInvoke(this.MassStorageConnected, this.onMassStorageConnected, sender, storageDevice))
                this.MassStorageConnected(sender, storageDevice);
        }

        private void OnMassStorageDisconnected(USBHost sender)
        {
            if (this.onMassStorageDisconnected == null)
                this.onMassStorageDisconnected = this.OnMassStorageDisconnected;

            if (Program.CheckAndInvoke(this.MassStorageDisconnected, this.onMassStorageDisconnected, sender))
                this.MassStorageDisconnected(sender);
        }

        private void OnMouseConnected(USBHost sender, Mouse mouse)
        {
            if (this.onMouseConnected == null)
                this.onMouseConnected = this.OnMouseConnected;

            if (Program.CheckAndInvoke(this.MouseConnected, this.onMouseConnected, sender, mouse))
                this.MouseConnected(sender, mouse);
        }

        private void OnMouseDisconnected(USBHost sender, Mouse mouse)
        {
            if (this.onMouseDisconnected == null)
                this.onMouseDisconnected = this.OnMouseDisconnected;

            if (Program.CheckAndInvoke(this.MouseDisconnected, this.onMouseDisconnected, sender, mouse))
                this.MouseDisconnected(sender, mouse);
        }

        private void OnKeyboardConnected(USBHost sender, Keyboard keyboard)
        {
            if (this.onKeyboardConnected == null)
                this.onKeyboardConnected = this.OnKeyboardConnected;

            if (Program.CheckAndInvoke(this.KeyboardConnected, this.onKeyboardConnected, sender, keyboard))
                this.KeyboardConnected(sender, keyboard);
        }

        private void OnKeyboardDisconnected(USBHost sender, Keyboard keyboard)
        {
            if (this.onKeyboardDisconnected == null)
                this.onKeyboardDisconnected = this.OnKeyboardDisconnected;

            if (Program.CheckAndInvoke(this.KeyboardDisconnected, this.onKeyboardDisconnected, sender, keyboard))
                this.KeyboardDisconnected(sender, keyboard);
        }
    }
}