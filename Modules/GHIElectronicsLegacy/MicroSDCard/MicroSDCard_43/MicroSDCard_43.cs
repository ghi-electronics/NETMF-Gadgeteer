using Microsoft.SPOT.IO;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A MicroSDCard module for Microsoft .NET Gadgeteer
    /// </summary>
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

            IsCardMounted = false;

            this.cardDetect = GTI.InterruptInputFactory.Create(socket, Socket.Pin.Three, GTI.GlitchFilterMode.On, GTI.ResistorMode.PullUp, GTI.InterruptMode.RisingAndFallingEdge, this);
            this.cardDetect.Interrupt += this.OnCardDetect;

            if (this.IsCardInserted)
                this.MountSDCard();
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
        /// The StorageDevice for the currently mounted SD card..
        /// </summary>
        public StorageDevice StorageDevice
        {
            get { return this.device; }
        }

        /// <summary>
        /// Attempts to mount the file system of the SD card.
        /// </summary>
        public void MountSDCard()
        {
            if (!this.IsCardMounted)
            {
                try
                {
                    Mainboard.MountStorageDevice("SD");
                    this.IsCardMounted = true;
                    Thread.Sleep(500);
                }
                catch
                {
                    ErrorPrint("Error mounting SD card - no card detected.");
                }
            }
        }

        /// <summary>
        /// Attempts to unmount the file system of the SD card.
        /// </summary>
        public void UnmountSDCard()
        {
            if (this.IsCardMounted)
            {
                try
                {
                    this.IsCardMounted = false;
                    Mainboard.UnmountStorageDevice("SD");
                    Thread.Sleep(500);
                }
                catch
                {
                    this.ErrorPrint("Unable to unmount SD card - no card detected.");
                }

                this.device = null;
            }
        }

        private void OnCardDetect(GTI.InterruptInput sender, bool value)
        {
            Thread.Sleep(500);

            if (this.IsCardInserted)
            {
                this.MountSDCard();
            }
            else
            {
                this.UnmountSDCard();
            }
        }

        private void OnInsert(object sender, MediaEventArgs e)
        {
            if (e.Volume.Name.Length >= 2 && e.Volume.Name.Substring(0, 2) == "SD")
            {
                if (e.Volume.FileSystem != null)
                {
                    this.device = new StorageDevice(e.Volume);
                    this.OnMounted(this, this.device);
                }
                else
                {
                    this.ErrorPrint("Unable to mount SD card. Is card formatted as FAT32?");
                    this.UnmountSDCard();
                }
            }

        }

        private void OnEject(object sender, MediaEventArgs e)
        {
            if (e.Volume.Name.Length >= 2 && e.Volume.Name.Substring(0, 2) == "SD")
                this.OnUnmounted(this);
        }

        /// <summary>
        /// Represents the delegate that is used for the <see cref="Mounted"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="MicroSDCard"/> object that raised the event.</param>
        /// <param name="SDCard">A storage device that can be used to access the SD non-volatile memory card.</param>
        public delegate void MountedEventHandler(MicroSDCard sender, StorageDevice SDCard);

        /// <summary>
        /// Represents the delegate that is used for the <see cref="Mounted"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="MicroSDCard"/> object that raised the event.</param>
        public delegate void UnmountedEventHandler(MicroSDCard sender);

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

        private void OnUnmounted(MicroSDCard sender)
        {
            if (this.onUnmounted == null)
                this.onUnmounted = this.OnUnmounted;

            if (Program.CheckAndInvoke(this.Unmounted, this.onUnmounted, sender))
                this.Unmounted(sender);
        }
    }
}