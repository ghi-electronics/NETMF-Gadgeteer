using GHI.Usb.Host;
using Microsoft.SPOT;
using System;
using System.Threading;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A Camera module for Microsoft .NET Gadgeteer
    /// </summary>
    public class Camera : GTM.Module
    {
        private Webcam camera;
        private DateTime lastStreamingStart;
        private DateTime takePictureLastCalled;
        private bool takePictureFlag;
        private Bitmap targetBitmap;
        private CameraStatus status;
        private Thread workerThread;
        private object syncRoot;
        private bool running;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The mainboard socket that has the camera module plugged into it.</param>
        public Camera(int socketNumber)
        {
            this.running = false;
            this.workerThread = null;
            this.syncRoot = new object();
            this.status = CameraStatus.Disconnected;
            this.takePictureFlag = false;

            this.RestartStreamingTrigger = new TimeSpan(0, 0, 0, 1, 500);
            this.TakePictureStreamTimeout = new TimeSpan(0, 0, 1, 0, 0);

            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('H', this);

            socket.ReservePin(Socket.Pin.Three, this);
            socket.ReservePin(Socket.Pin.Four, this);
            socket.ReservePin(Socket.Pin.Five, this);

            Controller.WebcamConnected += this.OnDeviceConnected;

            this.CurrentPictureResolution = PictureResolution.Resolution320x240;
        }

        private void OnDeviceConnected(object sender, Webcam camera)
        {
            this.camera = camera;
            this.camera.Disconnected += this.OnDeviceDisconnected;

            this.status = CameraStatus.Ready;
            this.running = true;
            this.workerThread = new Thread(this.DoWork);
            this.workerThread.Start();

            this.OnCameraConnected(this, null);
        }

        private void OnDeviceDisconnected(object sender, EventArgs e)
        {
            this.status = CameraStatus.Disconnected;
            this.running = false;
            this.workerThread.Join();
            this.takePictureFlag = false;

            if (this.status == CameraStatus.StreamBitmap)
                this.camera.StopStreaming();

            this.camera = null;

            this.OnCameraDisconnected(this, null);
        }

        /// <summary>
        /// Whether or not the camera is ready to take pictures.
        /// </summary>
        public bool CameraReady { get { return this.status == CameraStatus.Ready; } }

        /// <summary>
        /// The resolution of the pictures to capture.
        /// </summary>
        public PictureResolution CurrentPictureResolution { get; set; }

        /// <summary>
        /// This property is for calibration purposes. Do not change the value if not advised to.
        /// </summary>
        public TimeSpan RestartStreamingTrigger { get; set; }

        /// <summary>
        /// How long to continue streaming for after TakePicture is called.
        /// </summary>
        public TimeSpan TakePictureStreamTimeout { get; set; }

        /// <summary>
        /// Takes a single picture using.
        /// </summary>     
        public void TakePicture()
        {
            lock (this.syncRoot)
            {
                if (this.status == CameraStatus.Disconnected) throw new InvalidOperationException("No camera is connected.");
                if (this.status == CameraStatus.TakePicture) throw new InvalidOperationException("The camera is already busy taking a picture.");
                if (this.status == CameraStatus.StreamBitmap) throw new InvalidOperationException("The camera is already busy streaming a bitmap.");

                if (!this.takePictureFlag)
                    this.camera.StartStreaming(this.GetImageFormat(this.CurrentPictureResolution));

                this.lastStreamingStart = DateTime.Now;
                this.status = CameraStatus.TakePicture;
            }
        }

        /// <summary>
        /// Starts streaming pictures.
        /// </summary>
        public void StartStreaming()
        {
            this.StartStreaming(new Bitmap(this.CurrentPictureResolution.Width, this.CurrentPictureResolution.Height));
        }

        /// <summary>
        /// Starts streaming pictures into the provided bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap to stream into. Its size must patch the selected resolution.</param>
        public void StartStreaming(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");

            lock (this.syncRoot)
            {
                if (this.status == CameraStatus.Disconnected) throw new InvalidOperationException("No camera is connected.");
                if (this.status == CameraStatus.TakePicture) throw new InvalidOperationException("The camera is already busy taking a picture.");
                if (this.CurrentPictureResolution.Height != bitmap.Height || this.CurrentPictureResolution.Width != bitmap.Width) throw new ArgumentException("The camera is already busy streaming a bitmap.");

                if (this.status == CameraStatus.StreamBitmap)
                    this.StopStreaming();
                
                this.takePictureFlag = false;
                this.lastStreamingStart = DateTime.Now;
                this.targetBitmap = bitmap;
                this.status = CameraStatus.StreamBitmap;
                this.camera.StartStreaming(this.GetImageFormat(this.CurrentPictureResolution));
            }
        }

        /// <summary>
        /// Stops streaming the bitmaps.
        /// </summary>
        public void StopStreaming()
        {
            lock (this.syncRoot)
            {
                if (status != CameraStatus.StreamBitmap)
                    return;

                this.takePictureFlag = false;
                this.camera.StopStreaming();
                this.status = CameraStatus.Ready;
            }
        }

        private void DoWork()
        {
            while (this.running)
            {
                lock (this.syncRoot)
                {
                    if (this.status == CameraStatus.TakePicture || this.status == CameraStatus.StreamBitmap)
                    {
                        if (this.camera.IsNewImageAvailable())
                        {
                            this.lastStreamingStart = DateTime.Now;

                            if (status == CameraStatus.TakePicture) 
                                this.targetBitmap = new Bitmap(this.camera.CurrentStreamingFormat.Width, this.camera.CurrentStreamingFormat.Height);

                            this.camera.GetImage(this.targetBitmap);

                            if (this.status == CameraStatus.StreamBitmap)
                            {
                                this.OnBitmapStreamed(this, this.targetBitmap);
                            }
                            else
                            {
                                this.takePictureFlag = true;
                                this.takePictureLastCalled = DateTime.Now;

                                byte[] bmp = GHI.Utilities.Bitmaps.ConvertToFile(this.targetBitmap);
                                this.OnPictureCaptured(this, new Picture(bmp, Picture.PictureEncoding.BMP));
                                this.status = CameraStatus.Ready;
                            }
                        }
                        else
                        {
                            if ((DateTime.Now - this.lastStreamingStart) > this.RestartStreamingTrigger)
                            {
                                this.camera.StopStreaming();
                                this.camera.StartStreaming(this.GetImageFormat(this.CurrentPictureResolution));
                                this.lastStreamingStart = DateTime.Now;
                            }
                        }
                    }
                    if (takePictureFlag && (status != CameraStatus.TakePicture) && ((DateTime.Now - takePictureLastCalled) > this.TakePictureStreamTimeout))
                    {
                        this.takePictureFlag = false;
                        this.camera.StopStreaming();
                    }
                }

                Thread.Sleep(100);
            }
        }

        private Webcam.ImageFormat GetImageFormat(PictureResolution resolution)
        {
            if (this.camera == null) throw new InvalidOperationException("No camera is connected.");

            foreach (var f in this.camera.SupportedFormats)
                if (f.Width == resolution.Width && f.Height == resolution.Height)
                    return f;

            throw new ArgumentException("The connected camera does not supported the specified resolution.");
        }

        private enum CameraStatus
        {
            Disconnected = 0,
            Ready = 1,
            TakePicture = 2,
            StreamBitmap = 3
        }

        /// <summary>
        /// The image resolutions to use when taking a picture.
        /// </summary>
        public class PictureResolution
        {
            /// <summary>
            /// The width.
            /// </summary>
            public int Width { get; private set; }

            /// <summary>
            /// The height.
            /// </summary>
            public int Height { get; private set; }

            /// <summary>
            /// A resolution of 320x240
            /// </summary>
            public static readonly PictureResolution Resolution320x240 = new PictureResolution(320, 240);

            /// <summary>
            /// A resolution of 176x144
            /// </summary>
            public static readonly PictureResolution Resolution176x144 = new PictureResolution(176, 144);

            /// <summary>
            /// A resolution of 160x120
            /// </summary>
            public static readonly PictureResolution Resolution160x120 = new PictureResolution(160, 120);

            /// <summary>
            /// Constructs a new instance.
            /// </summary>
            /// <param name="width">The width in pixels.</param>
            /// <param name="height">The height in pixels.</param>
            public PictureResolution(int width, int height)
            {
                this.Width = width;
                this.Height = height;
            }
        }

        /// <summary>
        /// Represents the delegate that is used for the <see cref="PictureCaptured"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Camera"/> object that raised the event.</param>
        /// <param name="e">A <see cref="T:Gadgeteer.Picture"/> containing the captured image.</param>
        public delegate void PictureCapturedEventHandler(Camera sender, Picture e);

        /// <summary>
        /// Represents the delegate that is used to handle the <see cref="BitmapStreamed"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Camera"/> object that raised the event.</param>
        /// <param name="e">A <see cref="Bitmap"/> containing the captured image.</param>
        public delegate void BitmapStreamedEventHandler(Camera sender, Bitmap e);

        /// <summary>
        /// Represents the delegate to handle the <see cref="CameraConnected"/>.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void CameraEventHandler(Camera sender, EventArgs e);

        /// <summary>
        /// Raised when the camera has completed streaming a bitmap.
        /// </summary>
        public event BitmapStreamedEventHandler BitmapStreamed;

        /// <summary>
        /// Raised when the camera has captured an picture.
        /// </summary>
        public event PictureCapturedEventHandler PictureCaptured;

        /// <summary>
        /// Raised when the camera is connected.
        /// </summary>
        public event CameraEventHandler CameraConnected;

        /// <summary>
        /// Raised when the camera is disconnected.
        /// </summary>
        public event CameraEventHandler CameraDisconnected;

        private PictureCapturedEventHandler onPictureCaptured;
        private BitmapStreamedEventHandler onBitmapStreamed;
        private CameraEventHandler onCameraConnected;
        private CameraEventHandler onCameraDisconnected;

        private void OnPictureCaptured(Camera sender, Picture e)
        {
            if (this.onPictureCaptured == null)
                this.onPictureCaptured = this.OnPictureCaptured;

            if (Program.CheckAndInvoke(this.PictureCaptured, this.onPictureCaptured, sender, e))
                this.PictureCaptured(sender, e);
        }

        private void OnBitmapStreamed(Camera sender, Bitmap e)
        {
            if (this.onBitmapStreamed == null)
                this.onBitmapStreamed = this.OnBitmapStreamed;

            if (Program.CheckAndInvoke(this.BitmapStreamed, this.onBitmapStreamed, sender, e))
                this.BitmapStreamed(sender, e);
        }

        private void OnCameraConnected(Camera sender, EventArgs e)
        {
            if (this.onCameraConnected == null)
                this.onCameraConnected = this.OnCameraConnected;

            if (Program.CheckAndInvoke(this.CameraConnected, this.onCameraConnected, sender, e))
                this.CameraConnected(sender, e);
        }

        private void OnCameraDisconnected(Camera sender, EventArgs e)
        {
            if (this.onCameraDisconnected == null)
                this.onCameraDisconnected = this.OnCameraDisconnected;

            if (Program.CheckAndInvoke(this.CameraDisconnected, this.onCameraDisconnected, sender, e))
                this.CameraDisconnected(sender, e);
        }
    }
}