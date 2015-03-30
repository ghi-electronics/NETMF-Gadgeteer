using GHI.IO.Storage;
using GHI.Usb;
using GHI.Usb.Host;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using System;
using System.Collections;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics {
	/// <summary>A USBHost module for Microsoft .NET Gadgeteer</summary>
	public class USBHost : GTM.Module {
		private static bool firstHostModule;

		private StorageDevice massStorageDevice;
		private Keyboard connectedKeyboard;
		private Mouse connectedMouse;

		private MassStorageMountedEventHandler onMassStorageMounted;

		private MassStorageUnmountedEventHandler onMassStorageUnmounted;

		private MouseConnectedEventHandler onMouseConnected;

		private KeyboardConnectedEventHandler onKeyboardConnected;

		/// <summary>Represents the delegate that is used for the MassStorageMounted event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="device">A storage device that can be used to access the SD card.</param>
		public delegate void MassStorageMountedEventHandler(USBHost sender, StorageDevice device);

		/// <summary>Represents the delegate that is used for the MassStorageUnmounted event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The event arguments.</param>
		public delegate void MassStorageUnmountedEventHandler(USBHost sender, EventArgs e);

		/// <summary>Represents the delegate that is used for the MouseConnected event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="mouse">The object associated with the event.</param>
		public delegate void MouseConnectedEventHandler(USBHost sender, Mouse mouse);

		/// <summary>Represents the delegate that is used to handle the KeyboardConnected event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="keyboard">The object associated with the event.</param>
		public delegate void KeyboardConnectedEventHandler(USBHost sender, Keyboard keyboard);

		/// <summary>Raised when the file system of the mass storage device is mounted.</summary>
		public event MassStorageMountedEventHandler MassStorageMounted;

		/// <summary>Raised when the file system of the mass storage device is unmounted.</summary>
		public event MassStorageUnmountedEventHandler MassStorageUnmounted;

		/// <summary>Raised when a mouse is connected.</summary>
		public event MouseConnectedEventHandler MouseConnected;

		/// <summary>Raised when a keyboard is connected.</summary>
		public event KeyboardConnectedEventHandler KeyboardConnected;

		/// <summary>The current connected keyboard.</summary>
		public Keyboard ConnectedKeyboard {
			get { return this.connectedKeyboard; }
		}

		/// <summary>The current connected mouse.</summary>
		public Mouse ConnectedMouse {
			get { return this.connectedMouse; }
		}

		/// <summary>The StorageDevice for the currently mounted mass storage device.</summary>
		public StorageDevice MassStorageDevice {
			get { return this.massStorageDevice; }
		}

		/// <summary>Whether or not the keyboard is connected.</summary>
		public bool IsKeyboardConnected { get { return this.connectedKeyboard != null; } }

		/// <summary>Whether or not the mouse is connected.</summary>
		public bool IsMouseConnected { get { return this.connectedMouse != null; } }

		/// <summary>Whether or not the mass storage device is connected.</summary>
		public bool IsMassStorageConnected { get; private set; }

		/// <summary>Whether or not the mass storage device is mounted.</summary>
		public bool IsMassStorageMounted { get; private set; }

		static USBHost() {
			USBHost.firstHostModule = true;
		}

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The mainboard socket that has the module plugged into it.</param>
		public USBHost(int socketNumber) {
			if (!USBHost.firstHostModule) throw new InvalidOperationException("Only one USB host module may be connected in the designer at a time. If you have multiple host modules, just connect one of the modules in the designer. It will receive the events for both modules.");

			USBHost.firstHostModule = false;

			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('H', this);

			socket.ReservePin(Socket.Pin.Three, this);
			socket.ReservePin(Socket.Pin.Four, this);
			socket.ReservePin(Socket.Pin.Five, this);

			this.IsMassStorageConnected = false;
			this.IsMassStorageMounted = false;

			RemovableMedia.Insert += this.OnInsert;
			RemovableMedia.Eject += this.OnEject;

			Controller.MouseConnected += (a, b) => {
				this.connectedMouse = b;
				this.OnMouseConnected(this, b);

				b.Disconnected += (c, d) => this.connectedMouse = null;
			};

			Controller.KeyboardConnected += (a, b) => {
				this.connectedKeyboard = b;
				this.OnKeyboardConnected(this, b);

				b.Disconnected += (c, d) => this.connectedKeyboard = null;
			};

			Controller.MassStorageConnected += (a, b) => {
				this.IsMassStorageConnected = true;

				if (!this.IsMassStorageMounted)
					this.MountMassStorage();

				b.Disconnected += (c, d) => {
					this.IsMassStorageConnected = false;

					if (this.IsMassStorageMounted)
						this.UnmountMassStorage();
				};
			};
		}

		/// <summary>Attempts to mount the mass storage device.</summary>
		/// <returns>Whether or not the mass storage device was successfully mounted.</returns>
		public bool MountMassStorage() {
			if (this.IsMassStorageMounted) throw new InvalidOperationException("The mass storage is already mounted.");
			if (!this.IsMassStorageConnected) throw new InvalidOperationException("There is no mass storage device connected.");

			return Mainboard.MountStorageDevice("USB");
		}

		/// <summary>Attempts to unmount the mass storage device.</summary>
		/// <returns>Whether or not the mass storage device was successfully unmounted.</returns>
		public bool UnmountMassStorage() {
			if (!this.IsMassStorageMounted) throw new InvalidOperationException("The mass storage is not mounted.");

			return Mainboard.UnmountStorageDevice("USB");
		}

		private void OnInsert(object sender, MediaEventArgs e) {
			if (string.Compare(e.Volume.Name, "USB") == 0) {
				if (e.Volume.FileSystem != null) {
					this.massStorageDevice = new StorageDevice(e.Volume);
					this.IsMassStorageMounted = true;
					this.OnMassStorageMounted(this, this.massStorageDevice);
				}
				else {
					this.massStorageDevice = null;
					this.IsMassStorageMounted = false;
					Mainboard.UnmountStorageDevice("USB");
					this.ErrorPrint("The mass storage device does not have a valid filesystem.");
				}
			}
		}

		private void OnEject(object sender, MediaEventArgs e) {
			if (string.Compare(e.Volume.Name, "USB") == 0) {
				this.massStorageDevice = null;
				this.IsMassStorageMounted = false;
				this.OnMassStorageUnmounted(this, null);
			}
		}

		private void OnMassStorageMounted(USBHost sender, StorageDevice device) {
			if (this.onMassStorageMounted == null)
				this.onMassStorageMounted = this.OnMassStorageMounted;

			if (Program.CheckAndInvoke(this.MassStorageMounted, this.onMassStorageMounted, sender, device))
				this.MassStorageMounted(sender, device);
		}

		private void OnMassStorageUnmounted(USBHost sender, EventArgs e) {
			if (this.onMassStorageUnmounted == null)
				this.onMassStorageUnmounted = this.OnMassStorageUnmounted;

			if (Program.CheckAndInvoke(this.MassStorageUnmounted, this.onMassStorageUnmounted, sender, e))
				this.MassStorageUnmounted(sender, e);
		}

		private void OnMouseConnected(USBHost sender, Mouse mouse) {
			if (this.onMouseConnected == null)
				this.onMouseConnected = this.OnMouseConnected;

			if (Program.CheckAndInvoke(this.MouseConnected, this.onMouseConnected, sender, mouse))
				this.MouseConnected(sender, mouse);
		}

		private void OnKeyboardConnected(USBHost sender, Keyboard keyboard) {
			if (this.onKeyboardConnected == null)
				this.onKeyboardConnected = this.OnKeyboardConnected;

			if (Program.CheckAndInvoke(this.KeyboardConnected, this.onKeyboardConnected, sender, keyboard))
				this.KeyboardConnected(sender, keyboard);
		}
	}
}