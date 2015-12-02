using GHI.IO;
using GHI.IO.Storage;
using GHI.Processor;
using GHI.Usb;
using GHI.Usb.Host;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.IO;
using System;
using System.Threading;
using G120 = GHI.Pins.G120;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace GHIElectronics.Gadgeteer {
	/// <summary>The mainboard class for the FEZ Cobra II Eco.</summary>
	public class FEZCobraIIEcoExtender : GT.Mainboard {
		private InterruptPort ldr0;
		private InterruptPort ldr1;
		private OutputPort debugLed;
		private IRemovable[] storageDevices;
		private InputPort sdCardDetect;
		private GT.StorageDevice sdCardStorageDevice;
		private GT.StorageDevice massStorageDevice;
		private Keyboard connectedKeyboard;
		private Mouse connectedMouse;

		/// <summary>The name of the mainboard.</summary>
		public override string MainboardName {
			get { return "GHI Electronics FEZ Cobra II Eco Extender"; }
		}

		/// <summary>The current version of the mainboard hardware.</summary>
		public override string MainboardVersion {
			get { return "Rev B"; }
		}

		/// <summary>The InterruptPort object for LDR0.</summary>
		public InterruptPort LDR0 {
			get {
				if (this.ldr0 == null)
					this.ldr0 = new InterruptPort(G120.P2_10, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);

				return this.ldr0;
			}
		}

		/// <summary>The InterruptPort object for LDR1.</summary>
		public InterruptPort LDR1 {
			get {
				if (this.ldr1 == null)
					this.ldr1 = new InterruptPort(G120.P0_22, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);

				return this.ldr1;
			}
		}

		/// <summary>Constructs a new instance.</summary>
		public FEZCobraIIEcoExtender() {
			this.ldr0 = null;
			this.ldr1 = null;
			this.debugLed = null;
			this.storageDevices = new IRemovable[2];
			this.sdCardStorageDevice = null;

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

				this.MountStorageDevice("USB");

				b.Disconnected += (c, d) => {
					this.IsMassStorageConnected = false;

					if (this.IsMassStorageMounted)
						this.UnmountStorageDevice("USB");
				};
			};

			this.IsSDCardMounted = false;
			this.IsMassStorageConnected = false;
			this.IsMassStorageMounted = false;

			this.sdCardDetect = new InputPort(G120.P1_8, false, Port.ResistorMode.PullUp);

			if (this.IsSDCardInserted)
				this.MountStorageDevice("SD");

			Controller.Start();

			this.NativeBitmapConverter = this.BitmapConverter;

			#region Sockets
			GT.SocketInterfaces.I2CBusIndirector nativeI2C = (s, sdaPin, sclPin, address, clockRateKHz, module) => new InteropI2CBus(s, sdaPin, sclPin, address, clockRateKHz, module);
			GT.Socket socket;

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(1);
			socket.SupportedTypes = new char[] { 'B', 'Y' };
			socket.CpuPins[3] = G120.P2_13;
			socket.CpuPins[4] = G120.P1_26;
			socket.CpuPins[5] = G120.P1_27;
			socket.CpuPins[6] = G120.P1_28;
			socket.CpuPins[7] = G120.P1_29;
			socket.CpuPins[8] = G120.P2_4;
			socket.CpuPins[9] = G120.P2_2;
			socket.I2CBusIndirector = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(2);
			socket.SupportedTypes = new char[] { 'G' };
			socket.CpuPins[3] = G120.P1_20;
			socket.CpuPins[4] = G120.P1_21;
			socket.CpuPins[5] = G120.P1_22;
			socket.CpuPins[6] = G120.P1_23;
			socket.CpuPins[7] = G120.P1_24;
			socket.CpuPins[8] = G120.P1_25;
			socket.CpuPins[9] = G120.P1_19;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(3);
			socket.SupportedTypes = new char[] { 'R', 'Y' };
			socket.CpuPins[3] = G120.P2_12;
			socket.CpuPins[4] = G120.P2_6;
			socket.CpuPins[5] = G120.P2_7;
			socket.CpuPins[6] = G120.P2_8;
			socket.CpuPins[7] = G120.P2_9;
			socket.CpuPins[8] = G120.P2_3;
			socket.CpuPins[9] = G120.P2_5;
			socket.I2CBusIndirector = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(4);
			socket.SupportedTypes = new char[] { 'A', 'I', 'T', 'X' };
			socket.CpuPins[3] = G120.P0_25;
			socket.CpuPins[4] = G120.P0_24;
			socket.CpuPins[5] = G120.P0_23;
			socket.CpuPins[6] = G120.P1_0;
			socket.CpuPins[7] = G120.P1_1;
			socket.CpuPins[8] = G120.P0_27;
			socket.CpuPins[9] = G120.P0_28;
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_2;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_1;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_0;
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(5);
			socket.SupportedTypes = new char[] { 'U', 'X' };
			socket.CpuPins[3] = G120.P0_13;
			socket.CpuPins[4] = G120.P0_2;
			socket.CpuPins[5] = G120.P0_3;
			socket.CpuPins[6] = G120.P1_4;
			socket.SerialPortName = "COM1";
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(6);
			socket.SupportedTypes = new char[] { 'S', 'X' };
			socket.CpuPins[3] = G120.P2_21;
			socket.CpuPins[4] = G120.P1_14;
			socket.CpuPins[5] = G120.P1_16;
			socket.CpuPins[6] = G120.P1_17;
			socket.CpuPins[7] = GT.Socket.UnnumberedPin;
			socket.CpuPins[8] = GT.Socket.UnnumberedPin;
			socket.CpuPins[9] = GT.Socket.UnnumberedPin;
			socket.SPIModule = SPI.SPI_module.SPI2;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(7);
			socket.SupportedTypes = new char[] { 'P', 'U', 'Y' };
			socket.CpuPins[3] = G120.P0_4;
			socket.CpuPins[4] = G120.P4_28;
			socket.CpuPins[5] = G120.P4_29;
			socket.CpuPins[6] = G120.P1_30;
			socket.CpuPins[7] = G120.P3_26;
			socket.CpuPins[8] = G120.P3_25;
			socket.CpuPins[9] = G120.P3_24;
			socket.SerialPortName = "COM4";
			socket.PWM7 = (Cpu.PWMChannel)8;
			socket.PWM8 = Cpu.PWMChannel.PWM_7;
			socket.PWM9 = Cpu.PWMChannel.PWM_6;
			socket.I2CBusIndirector = nativeI2C;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(8);
			socket.SupportedTypes = new char[] { 'I', 'K', 'U', 'X' };
			socket.CpuPins[3] = G120.P0_10;
			socket.CpuPins[4] = G120.P2_0;
			socket.CpuPins[5] = G120.P0_16;
			socket.CpuPins[6] = G120.P0_6;
			socket.CpuPins[7] = G120.P0_17;
			socket.CpuPins[8] = G120.P0_27;
			socket.CpuPins[9] = G120.P0_28;
			socket.SerialPortName = "COM2";
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(9);
			socket.SupportedTypes = new char[] { 'A', 'O', 'S', 'X' };
			socket.CpuPins[3] = G120.P0_12;
			socket.CpuPins[4] = G120.P1_31;
			socket.CpuPins[5] = G120.P0_26;
			socket.CpuPins[6] = G120.P1_5;
			socket.CpuPins[7] = G120.P0_18;
			socket.CpuPins[8] = G120.P0_17;
			socket.CpuPins[9] = G120.P0_15;
			socket.SPIModule = SPI.SPI_module.SPI1;
			socket.AnalogOutput5 = Cpu.AnalogOutputChannel.ANALOG_OUTPUT_0;
			socket.AnalogInput3 = Cpu.AnalogChannel.ANALOG_6;
			socket.AnalogInput4 = Cpu.AnalogChannel.ANALOG_5;
			socket.AnalogInput5 = Cpu.AnalogChannel.ANALOG_3;
			GT.Socket.SocketInterfaces.SetAnalogInputFactors(socket, 3.3, 0, 12);
			GT.Socket.SocketInterfaces.SetAnalogOutputFactors(socket, 3.3, 0, 10);
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			socket = GT.Socket.SocketInterfaces.CreateNumberedSocket(10);
			socket.SupportedTypes = new char[] { 'C', 'I', 'X' };
			socket.CpuPins[3] = G120.P0_11;
			socket.CpuPins[4] = G120.P0_1;
			socket.CpuPins[5] = G120.P0_0;
			socket.CpuPins[6] = G120.P0_5;
			socket.CpuPins[8] = G120.P0_27;
			socket.CpuPins[9] = G120.P0_28;
			GT.Socket.SocketInterfaces.RegisterSocket(socket);

			#endregion Sockets
		}

		/// <summary>The storage device volume names supported by this mainboard.</summary>
		/// <returns>The volume names.</returns>
		public override string[] GetStorageDeviceVolumeNames() {
			return new string[] { "SD", "USB" };
		}

		/// <summary>Mounts the device with the given name.</summary>
		/// <param name="volumeName">The device to mount.</param>
		/// <returns>Whether or not the mount was successful.</returns>
		public override bool MountStorageDevice(string volumeName) {
			try {
				if (volumeName == "SD" && this.storageDevices[0] == null) {
					this.storageDevices[0] = new SDCard();
					this.storageDevices[0].Mount();

					return true;
				}
				else if (volumeName == "USB" && this.storageDevices[1] == null) {
					foreach (BaseDevice dev in Controller.GetConnectedDevices()) {
						if (dev.GetType() == typeof(MassStorage)) {
							this.storageDevices[1] = (MassStorage)dev;
							this.storageDevices[1].Mount();

							return true;
						}
					}
				}
			}
			catch {
			}

			return false;
		}

		/// <summary>Unmounts the device with the given name.</summary>
		/// <param name="volumeName">The device to unmount.</param>
		/// <returns>Whether or not the unmount was successful.</returns>
		public override bool UnmountStorageDevice(string volumeName) {
			if (volumeName == "SD" && this.storageDevices[0] != null) {
				this.storageDevices[0].Dispose();
				this.storageDevices[0] = null;
			}
			else if (volumeName == "USB" && this.storageDevices[1] != null) {
				this.storageDevices[1].Dispose();
				this.storageDevices[1] = null;
			}
			else {
				return false;
			}

			return true;
		}

		/// <summary>Ensures that the RGB socket pins are available by disabling the display controller if needed.</summary>
		public override void EnsureRgbSocketPinsAvailable() {
			if (Display.Disable()) {
				Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
				Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

				Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
			}
		}

		/// <summary>Sets the state of the debug LED.</summary>
		/// <param name="on">The new state.</param>
		public override void SetDebugLED(bool on) {
			if (this.debugLed == null)
				this.debugLed = new OutputPort(G120.P1_15, on);

			this.debugLed.Write(on);
		}

		/// <summary>Sets the programming mode of the device.</summary>
		/// <param name="programmingInterface">The new programming mode.</param>
		public override void SetProgrammingMode(GT.Mainboard.ProgrammingInterface programmingInterface) {
			throw new NotSupportedException();
		}

		/// <summary>This performs post-initialization tasks for the mainboard. It is called by Gadgeteer.Program.Run and does not need to be called manually.</summary>
		public override void PostInit() {
		}

		/// <summary>
		/// Configure the onboard display controller to fulfil the requirements of a display using the RGB sockets. If doing this requires rebooting, then the method must reboot and not return. If
		/// there is no onboard display controller, then NotSupportedException must be thrown.
		/// </summary>
		/// <param name="displayModel">Display model name.</param>
		/// <param name="width">Display physical width in pixels, ignoring the orientation setting.</param>
		/// <param name="height">Display physical height in lines, ignoring the orientation setting.</param>
		/// <param name="orientationDeg">Display orientation in degrees.</param>
		/// <param name="timing">The required timings from an LCD controller.</param>
		protected override void OnOnboardControllerDisplayConnected(string displayModel, int width, int height, int orientationDeg, GTM.Module.DisplayModule.TimingRequirements timing) {
			switch (orientationDeg) {
				case 0: Display.CurrentRotation = Display.Rotation.Normal; break;
				case 90: Display.CurrentRotation = Display.Rotation.Clockwise90; break;
				case 180: Display.CurrentRotation = Display.Rotation.Half; break;
				case 270: Display.CurrentRotation = Display.Rotation.CounterClockwise90; break;
				default: throw new ArgumentOutOfRangeException("orientationDeg", "orientationDeg must be 0, 90, 180, or 270.");
			}

			Display.Height = height;
			Display.HorizontalBackPorch = timing.HorizontalBackPorch;
			Display.HorizontalFrontPorch = timing.HorizontalFrontPorch;
			Display.HorizontalSyncPolarity = timing.HorizontalSyncPulseIsActiveHigh;
			Display.HorizontalSyncPulseWidth = timing.HorizontalSyncPulseWidth;
			Display.OutputEnableIsFixed = timing.UsesCommonSyncPin; //not the proper property, but we needed it;
			Display.OutputEnablePolarity = timing.CommonSyncPinIsActiveHigh; //not the proper property, but we needed it;
			Display.PixelClockRateKHz = (int)timing.MaximumClockSpeed;
			Display.PixelPolarity = timing.PixelDataIsValidOnClockRisingEdge;
			Display.VerticalBackPorch = timing.VerticalBackPorch;
			Display.VerticalFrontPorch = timing.VerticalFrontPorch;
			Display.VerticalSyncPolarity = timing.VerticalSyncPulseIsActiveHigh;
			Display.VerticalSyncPulseWidth = timing.VerticalSyncPulseWidth;
			Display.Width = width;

			if (Display.Save()) {
				Debug.Print("Updating display configuration. THE MAINBOARD WILL NOW REBOOT.");
				Debug.Print("To continue debugging, you will need to restart debugging manually (Ctrl-Shift-F5)");

				Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
			}
		}

		private void BitmapConverter(Bitmap bitmap, byte[] pixelBytes, GT.Mainboard.BPP bpp) {
			if (bpp != GT.Mainboard.BPP.BPP16_BGR_BE) throw new ArgumentOutOfRangeException("bpp", "Only BPP16_BGR_BE supported");

			GHI.Utilities.Bitmaps.Convert(bitmap, GHI.Utilities.Bitmaps.Format.Bpp16BgrBe, pixelBytes);
		}

		private void OnInsert(object sender, MediaEventArgs e) {
			if (string.Compare(e.Volume.Name, "USB") == 0) {
				if (e.Volume.FileSystem != null) {
					this.massStorageDevice = new GT.StorageDevice(e.Volume);
					this.IsMassStorageMounted = true;
					this.OnMassStorageMounted(this, this.massStorageDevice);
				}
				else {
					this.massStorageDevice = null;
					this.IsMassStorageMounted = false;
					this.UnmountStorageDevice("USB");
					Debug.Print("The mass storage device does not have a valid filesystem.");
				}
			}
			else if (string.Compare(e.Volume.Name, "SD") == 0) {
				if (e.Volume.FileSystem != null) {
					this.sdCardStorageDevice = new GT.StorageDevice(e.Volume);
					this.IsSDCardMounted = true;
					this.OnSDCardMounted(this, this.sdCardStorageDevice);
				}
				else {
					this.sdCardStorageDevice = null;
					this.IsSDCardMounted = false;
					this.UnmountStorageDevice("SD");
					Debug.Print("The SD card does not have a valid filesystem.");
				}
			}
		}

		private void OnEject(object sender, MediaEventArgs e) {
			if (string.Compare(e.Volume.Name, "USB") == 0) {
				this.massStorageDevice = null;
				this.IsMassStorageMounted = false;
				this.OnMassStorageUnmounted(this, null);
			}
			else if (string.Compare(e.Volume.Name, "SD") == 0) {
				this.sdCardStorageDevice = null;
				this.IsSDCardMounted = false;
				this.OnSDCardUnmounted(this, null);
			}
		}
		private class InteropI2CBus : GT.SocketInterfaces.I2CBus {
			private SoftwareI2CBus softwareBus;

			public override ushort Address { get; set; }

			public override int Timeout { get; set; }

			public override int ClockRateKHz { get; set; }

			public InteropI2CBus(GT.Socket socket, GT.Socket.Pin sdaPin, GT.Socket.Pin sclPin, ushort address, int clockRateKHz, GTM.Module module) {
				this.Address = address;
				this.ClockRateKHz = clockRateKHz;

				this.softwareBus = new SoftwareI2CBus(socket.CpuPins[(int)sclPin], socket.CpuPins[(int)sdaPin]);
			}

			public override void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int numWritten, out int numRead) {
				this.softwareBus.WriteRead((byte)this.Address, writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out numWritten, out numRead);
			}
		}

		#region SDCard
		private SDCardMountedEventHandler onSDCardMounted;

		private SDCardUnmountedEventHandler onSDCardUnmounted;

		/// <summary>Represents the delegate that is used for the Mounted event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="device">A storage device that can be used to access the SD card.</param>
		public delegate void SDCardMountedEventHandler(FEZCobraIIEcoExtender sender, GT.StorageDevice device);

		/// <summary>Represents the delegate that is used for the Unmounted event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The event arguments.</param>
		public delegate void SDCardUnmountedEventHandler(FEZCobraIIEcoExtender sender, EventArgs e);

		/// <summary>Raised when the file system of the SD card is mounted.</summary>
		public event SDCardMountedEventHandler SDCardMounted;

		/// <summary>Raised when the file system of the SD card is unmounted.</summary>
		public event SDCardUnmountedEventHandler SDCardUnmounted;

		/// <summary>Whether or not an SD card is inserted. Since the SD card detect pin is not interrupt capable, you must manually poll this property then call MountStorageDevice.</summary>
		public bool IsSDCardInserted {
			get {
				return !this.sdCardDetect.Read();
			}
		}

		/// <summary>Whether or not the SD card is mounted.</summary>
		public bool IsSDCardMounted { get; private set; }

		/// <summary>The StorageDevice for the currently mounted SD card.</summary>
		public GT.StorageDevice SDCardStorageDevice {
			get { return this.sdCardStorageDevice; }
		}

		private void OnSDCardMounted(FEZCobraIIEcoExtender sender, GT.StorageDevice device) {
			if (this.onSDCardMounted == null)
				this.onSDCardMounted = this.OnSDCardMounted;

			if (GT.Program.CheckAndInvoke(this.SDCardMounted, this.onSDCardMounted, sender, device))
				this.SDCardMounted(sender, device);
		}

		private void OnSDCardUnmounted(FEZCobraIIEcoExtender sender, EventArgs e) {
			if (this.onSDCardUnmounted == null)
				this.onSDCardUnmounted = this.OnSDCardUnmounted;

			if (GT.Program.CheckAndInvoke(this.SDCardUnmounted, this.onSDCardUnmounted, sender, e))
				this.SDCardUnmounted(sender, e);
		}

		#endregion SDCard

		#region USBHost
		private MassStorageMountedEventHandler onMassStorageMounted;

		private MassStorageUnmountedEventHandler onMassStorageUnmounted;

		private MouseConnectedEventHandler onMouseConnected;

		private KeyboardConnectedEventHandler onKeyboardConnected;

		/// <summary>Represents the delegate that is used for the MassStorageMounted event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="device">A storage device that can be used to access the SD card.</param>
		public delegate void MassStorageMountedEventHandler(FEZCobraIIEcoExtender sender, GT.StorageDevice device);

		/// <summary>Represents the delegate that is used for the MassStorageUnmounted event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The event arguments.</param>
		public delegate void MassStorageUnmountedEventHandler(FEZCobraIIEcoExtender sender, EventArgs e);

		/// <summary>Represents the delegate that is used for the MouseConnected event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="mouse">The object associated with the event.</param>
		public delegate void MouseConnectedEventHandler(FEZCobraIIEcoExtender sender, Mouse mouse);

		/// <summary>Represents the delegate that is used to handle the KeyboardConnected event.</summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="keyboard">The object associated with the event.</param>
		public delegate void KeyboardConnectedEventHandler(FEZCobraIIEcoExtender sender, Keyboard keyboard);

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
		public GT.StorageDevice MassStorageDevice {
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

		private void OnMassStorageMounted(FEZCobraIIEcoExtender sender, GT.StorageDevice device) {
			if (this.onMassStorageMounted == null)
				this.onMassStorageMounted = this.OnMassStorageMounted;

			if (GT.Program.CheckAndInvoke(this.MassStorageMounted, this.onMassStorageMounted, sender, device))
				this.MassStorageMounted(sender, device);
		}

		private void OnMassStorageUnmounted(FEZCobraIIEcoExtender sender, EventArgs e) {
			if (this.onMassStorageUnmounted == null)
				this.onMassStorageUnmounted = this.OnMassStorageUnmounted;

			if (GT.Program.CheckAndInvoke(this.MassStorageUnmounted, this.onMassStorageUnmounted, sender, e))
				this.MassStorageUnmounted(sender, e);
		}

		private void OnMouseConnected(FEZCobraIIEcoExtender sender, Mouse mouse) {
			if (this.onMouseConnected == null)
				this.onMouseConnected = this.OnMouseConnected;

			if (GT.Program.CheckAndInvoke(this.MouseConnected, this.onMouseConnected, sender, mouse))
				this.MouseConnected(sender, mouse);
		}

		private void OnKeyboardConnected(FEZCobraIIEcoExtender sender, Keyboard keyboard) {
			if (this.onKeyboardConnected == null)
				this.onKeyboardConnected = this.OnKeyboardConnected;

			if (GT.Program.CheckAndInvoke(this.KeyboardConnected, this.onKeyboardConnected, sender, keyboard))
				this.KeyboardConnected(sender, keyboard);
		}

		#endregion USBHost
	}
}