using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using System;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
	/// <summary>
	/// An MicroSDCard module for Microsoft .NET Gadgeteer
	/// </summary>
	[Obsolete]
	public class MicroSDCard : GTM.Module
	{
		private GTI.InterruptInput cardDetect;
		private StorageDevice device;

		/// <summary>Constructs a new instance.</summary>
		/// <param name="socketNumber">The mainboard socket that has the module plugged into it.</param>
		public MicroSDCard(int socketNumber)
		{
			Socket socket = Socket.GetSocket(socketNumber, true, this, null);
			socket.EnsureTypeIsSupported('F', this);

			socket.ReservePin(Socket.Pin.Four, this);
			socket.ReservePin(Socket.Pin.Five, this);
			socket.ReservePin(Socket.Pin.Six, this);
			socket.ReservePin(Socket.Pin.Seven, this);
			socket.ReservePin(Socket.Pin.Eight, this);
			socket.ReservePin(Socket.Pin.Nine, this);

			RemovableMedia.Insert += this.OnInsert;
			RemovableMedia.Eject += this.OnEject;

			this.IsCardMounted = false;

			this.cardDetect = GTI.InterruptInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.On, GTI.ResistorMode.PullUp, GTI.InterruptMode.RisingAndFallingEdge, this);
			this.cardDetect.Interrupt += this.OnCardDetect;

			if (this.IsCardInserted)
				this.Mount();
		}

		/// <summary>
		/// Whether or not an SD card is inserted.
		/// </summary>
		public bool IsCardInserted
		{
			get { return !this.cardDetect.Read(); }
		}

		/// <summary>
		/// Whether or not the SD card is mounted.
		/// </summary>
		public bool IsCardMounted { get; private set; }

		/// <summary>
		/// The StorageDevice for the currently mounted SD card.
		/// </summary>
		public StorageDevice StorageDevice
		{
			get { return this.device; }
		}

		/// <summary>
		/// Attempts to mount the card.
		/// </summary>
		/// <returns>Whether or not the card was successfully mounted.</returns>
		public bool Mount()
		{
			if (this.IsCardMounted) throw new InvalidOperationException("The card is already mounted.");

			return Mainboard.MountStorageDevice("SD");
		}

		/// <summary>
		/// Attempts to unmount the card.
		/// </summary>
		/// <returns>Whether or not the card was successfully unmounted.</returns>
		public bool Unmount()
		{
			if (!this.IsCardMounted) throw new InvalidOperationException("The card is already unmounted.");

			return !Mainboard.UnmountStorageDevice("SD");
		}

		private void OnCardDetect(GTI.InterruptInput sender, bool value)
		{
			Thread.Sleep(500);

			if (this.IsCardInserted && !this.IsCardMounted)
				this.Mount();

			if (!this.IsCardInserted && this.IsCardMounted)
				this.Unmount();
		}

		private void OnInsert(object sender, MediaEventArgs e)
		{
			if (string.Compare(e.Volume.Name, "SD") == 0)
			{
				if (e.Volume.FileSystem != null)
				{
					this.device = new StorageDevice(e.Volume);
					this.IsCardMounted = true;
					this.OnMounted(this, this.device);
				}
				else
				{
					this.device = null;
					this.IsCardMounted = false;
					Mainboard.UnmountStorageDevice("SD");
					this.ErrorPrint("The SD card does not have a valid filesystem.");
				}
			}
		}

		private void OnEject(object sender, MediaEventArgs e)
		{
			if (string.Compare(e.Volume.Name, "SD") == 0)
			{
				this.device = null;
				this.IsCardMounted = false;
				this.OnUnmounted(this, null);
			}
		}

		/// <summary>
		/// Represents the delegate that is used for the Mounted event.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="device">A storage device that can be used to access the SD card.</param>
		public delegate void MountedEventHandler(MicroSDCard sender, StorageDevice device);

		/// <summary>
		/// Represents the delegate that is used for the Unmounted event.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The event arguments.</param>
		public delegate void UnmountedEventHandler(MicroSDCard sender, EventArgs e);

		/// <summary>
		/// Raised when the file system of the SD card is mounted.
		/// </summary>
		public event MountedEventHandler Mounted;

		/// <summary>
		/// Raised when the file system of the SD card is unmounted.
		/// </summary>
		public event UnmountedEventHandler Unmounted;

		private MountedEventHandler onMounted;
		private UnmountedEventHandler onUnmounted;

		private void OnMounted(MicroSDCard sender, StorageDevice device)
		{
			if (this.onMounted == null)
				this.onMounted = this.OnMounted;

			if (Program.CheckAndInvoke(this.Mounted, this.onMounted, sender, device))
				this.Mounted(sender, device);
		}

		private void OnUnmounted(MicroSDCard sender, EventArgs e)
		{
			if (this.onUnmounted == null)
				this.onUnmounted = this.OnUnmounted;

			if (Program.CheckAndInvoke(this.Unmounted, this.onUnmounted, sender, e))
				this.Unmounted(sender, e);
		}
	}
}