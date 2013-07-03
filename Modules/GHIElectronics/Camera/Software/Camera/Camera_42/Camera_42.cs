using System;
using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using System.Threading;

using GHI.Premium.USBHost;
using GHI.Premium.System;

namespace Gadgeteer.Modules.GHIElectronics
{
    // -- CHANGE FOR MICRO FRAMEWORK 4.2 --
    // If you want to use Serial, SPI, or DaisyLink (which includes GTI.SoftwareI2C), you must do a few more steps
    // since these have been moved to separate assemblies for NETMF 4.2 (to reduce the minimum memory footprint of Gadgeteer)
    // 1) add a reference to the assembly (named Gadgeteer.[interfacename])
    // 2) in GadgeteerHardware.xml, uncomment the lines under <Assemblies> so that end user apps using this module also add a reference.

    /// <summary>
    /// Represents a camera module that you can use to capture images.
    /// </summary>
    /// <remarks>
    /// After you create a <see cref="Camera"/> object, the camera initializes asynchronously; 
    /// it takes approximately 400-600 milliseconds before the <see cref="CameraReady"/> property returns <b>true</b>.
    /// </remarks>
    /// <example>
    /// <para>The following example initializes a <see cref="Camera"/> object and the button pressed event delegate in which the camera takes a picture.
    /// Another delegete is initialized to handle the asynchronous PictureCaptured event.  In this method the display module uses the SimpleGraphics class to display
    /// the picture captured by the camera.
    /// </para>
    /// <code>
    /// using System;
    /// using Microsoft.SPOT;
    /// using Microsoft.SPOT.Presentation;
    /// using Microsoft.SPOT.Presentation.Controls;
    /// using Microsoft.SPOT.Presentation.Media;
    ///
    /// using GT = Gadgeteer;
    /// using GTM = Gadgeteer.Modules;
    ///
    /// using Gadgeteer.Modules.GHIElectronics;
    ///
    /// namespace TestApp
    /// {
    ///     public partial class Program
    ///     {
    ///         // This template uses the FEZ Spider mainboard from GHI Electronics
    ///
    ///         // The modules in this demo are plugged into the following socket numbers.        
    ///         // UsbClientDP = 1
    ///         // Button = 4
    ///         // Camera = 3
    ///         // Display_T35 = 12, 13, 14
    ///
    ///         void ProgramStarted()
    ///         {
    ///             // Initialize event handlers here.
    ///             button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
    ///             camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);
    ///
    ///             // Do one-time tasks here
    ///             Debug.Print("Program Started");
    ///         }
    ///
    ///         void camera_PictureCaptured(Camera sender, GT.Picture picture)
    ///         {
    ///             Debug.Print("Picture Captured event.");
    ///             display.SimpleGraphics.DisplayImage(picture, 5, 5);
    ///         }
    ///
    ///         void button_ButtonPressed(Button sender, Button.ButtonState state)
    ///         {
    ///             camera.TakePicture();
    ///         }
    ///     }
    /// }
    /// 
    /// </code>
    /// <para>The following code shows how to start sending bitmaps repeatedly with 
    /// the <see cref="StartStreamingBitmaps"/> method and stop with the <see cref="StopStreamingBitmaps"/> method.</para>
    /// <code> 
    /// void button_ButtonPressed(GTM.GHIElectronics.Button sender, GTM.GHIElectronics.Button.ButtonState state)
    /// {          
    ///     camera.StartStreamingBitmaps(new Bitmap(camera.CurrentPictureResolution.Width, camera.CurrentPictureResolution.Height));
    /// }
    /// 
    /// void camera_BitmapStreamed(GTM.GHIElectronics.Camera sender, Bitmap bitmap)
    /// {            
    ///     display.SimpleGraphics.DisplayImage(bitmap, 10, 10);
    /// }
    /// 
    /// void button_ButtonReleased(GTM.GHIElectronics.Button sender, GTM.GHIElectronics.Button.ButtonState state)
    /// {
    ///     camera.StopStreamingBitmaps();
    /// }
    /// 
    /// </code>
    /// </example>
    public class Camera : GTM.Module
    {
        USBH_Webcam _camera;
        private DateTime _LastTimeStreamStart;
        private DateTime _LastTimeTakePictureCalled;

        //private TimeSpan delta = new TimeSpan(0, 0, 1);
        private TimeSpan _restartStreamingTriggerTimeSpan;
        private TimeSpan _takePictureStreamingTimeoutTimeSpan;

        private int _restartStreamingTrigger = 1500;
        private int _takePictureStreamingTimeout = 1000 * 60;
        private bool _TakePictureFlag = false;

        /// <summary>
        /// Restart Streaming Trigger is milliseconds. The default value is 1500. This property is for calibration purposes. Do not change the value if not advised to.
        /// </summary>
        public int RestartStreamingTrigger
        {
            get
            {
                return _restartStreamingTrigger;
            }
            set
            {
                _restartStreamingTrigger = value;
            }
        }
        /// <summary>
        /// Camera driver keeps streaming on after calling TakePicture() then it stops streaming if TakePicture() was not called for "TakePictureStreamTimeout" milliseconds. The default value is 60,000 msec.
        /// </summary>
        public int TakePictureStreamTimeout
        {
            get
            {
                return _takePictureStreamingTimeout;
            }
            set
            {
                _takePictureStreamingTimeout = value;
            }
        }
        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="socketNumber">The mainboard socket that has the camera module plugged into it.</param>
        public Camera(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);

            if (socket.SupportsType('H') == false)
            {
                throw new GT.Socket.InvalidSocketException("Socket " + socket +
                    " does not support support Camera modules. Please plug the Camera module into a socket labeled 'H'");
            }

            _runThread = false;

            USBHostController.DeviceConnectedEvent += new USBH_DeviceConnectionEventHandler(USBHostController_DeviceConnectedEvent);
            USBHostController.DeviceDisconnectedEvent += new USBH_DeviceConnectionEventHandler(USBHostController_DeviceDisconnectedEvent);

            // Reserve the pins used by the USBHost interface
            // These calls will throw PinConflictExcpetion if they are already reserved
            try
            {
                socket.ReservePin(Socket.Pin.Three, this);
                socket.ReservePin(Socket.Pin.Four, this);
                socket.ReservePin(Socket.Pin.Five, this);
            }

            catch (Exception e)
            {
                throw new GT.Socket.InvalidSocketException("There is an issue connecting the Camera module to socket " + socketNumber +
                    ". Please check that all modules are connected to the correct sockets or try connecting the Camera to a different 'H' socket", e);
            }

            CurrentPictureResolution = PictureResolution.Resolution320x240;
            _cameraStatus = CameraStatus.Disconnected;

        }

        private void USBHostController_DeviceConnectedEvent(USBH_Device device)
        {
            try
            {
                if (device.TYPE == USBH_DeviceType.Webcamera)
                {
                    _camera = new USBH_Webcam(device);

                    this._cameraThread = new Thread(CameraCommunication);
                    _runThread = true;
                    this._cameraThread.Start();

                    this._cameraStatus = CameraStatus.Ready;
                    DebugPrint("Camera connected.");
                }

            }
            catch
            { }

            if (device.TYPE == USBH_DeviceType.Webcamera)
            {
                CameraConnectedEvent(this);
            }

        }

        private void USBHostController_DeviceDisconnectedEvent(USBH_Device device)
        {
            try
            {
                if (device.TYPE == USBH_DeviceType.Webcamera)
                {
                    _cameraStatus = CameraStatus.Disconnected;
                    _runThread = false;
                    _TakePictureFlag = false;
                    this._camera.StopStreaming();
                    _camera = null;
                    DebugPrint("Camera disconnected.");
                }
            }
            catch
            { }

            if (device.TYPE == USBH_DeviceType.Webcamera)
            {
                OnCameraDisconnectedEvent(this);
            }
        }

        /// <summary>
        ///  Delegate method to handle the <see cref="CameraConnectedEvent"/>.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void CameraConnectedEventHandler(Camera sender);

        /// <summary>
        /// Event raised when a Camera is connected to the Mainboard.
        /// </summary>
        public event CameraConnectedEventHandler CameraConnected;

        private CameraConnectedEventHandler _CameraConnected;

        /// <summary>
        /// Raises the <see cref="CameraConnected"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Camera"/> object that raised the event.</param>
        protected virtual void CameraConnectedEvent(Camera sender)
        {
            if (_CameraConnected == null) _CameraConnected = new CameraConnectedEventHandler(CameraConnectedEvent);
            if (Program.CheckAndInvoke(CameraConnected, _CameraConnected, sender))
            {
                CameraConnected(sender);
            }
        }

        /// <summary>
        /// Represents the delegate that is used for the <see cref="CameraDisconnected"/>.
        /// </summary>
        /// <param name="sender">The <see cref="Camera"/> object that raised the event.</param>
        public delegate void CameraDisconnectedEventHandler(Camera sender);

        /// <summary>
        /// Event raised when a Camera is disconnected from the Mainboard.
        /// </summary>
        public event CameraDisconnectedEventHandler CameraDisconnected;

        private CameraDisconnectedEventHandler _CameraDisconnected;

        /// <summary>
        /// Raises the <see cref="CameraDisconnected"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Camera"/> object that raised the event.</param>
        protected virtual void OnCameraDisconnectedEvent(Camera sender)
        {
            if (_CameraDisconnected == null) _CameraDisconnected = new CameraDisconnectedEventHandler(OnCameraDisconnectedEvent);
            if (Program.CheckAndInvoke(CameraDisconnected, _CameraDisconnected, sender))
            {
                CameraDisconnected(sender);
            }
        }

        private CameraStatus _cameraStatus;

        /// <summary>
        /// Gets the ready status of the camera.
        /// </summary>
        /// <remarks>
        /// After you create a <see cref="Camera"/> object, the camera initializes asynchronously; 
        /// it takes approximately 400-600 milliseconds before this property returns <b>true</b>.
        /// </remarks>
        public bool CameraReady { get { return this._cameraStatus == CameraStatus.Ready; } }


        private Thread _cameraThread;
        private object cameraLock = new object();

        private enum CameraStatus
        {
            Disconnected = 0,
            Ready = 1,
            TakePicture = 2,
            StreamBitmap = 3
        }

        ///// <summary>
        ///// Represents the possible status of a camera operation.
        ///// </summary>
        //public enum RequestStatus
        //{
        //    /// <summary>
        //    /// The operation was unsucessful.
        //    /// </summary>
        //    Failed = 0,
        //    /// <summary>
        //    /// The operation completed successfully.
        //    /// </summary>
        //    Acknowledged = 1
        //}

        /// <summary>
        /// Class that specifies the image resolutions supported by the camera.
        /// </summary>
        public class PictureResolution
        {
            /// <summary>
            /// Gets the width of the picture resolution.
            /// </summary>
            public int Width { get; private set; }
            /// <summary>
            /// Gets the height of the picture resolution.
            /// </summary>
            public int Height { get; private set; }

            /// <summary>
            /// Picture resolution 320x240
            /// </summary>
            public static readonly PictureResolution Resolution320x240 = new PictureResolution(320, 240);

            /// <summary>
            /// Picture resolution 176x144
            /// </summary>
            public static readonly PictureResolution Resolution176x144 = new PictureResolution(176, 144);

            /// <summary>
            /// Picture resolution 160x120
            /// </summary>
            public static readonly PictureResolution Resolution160x120 = new PictureResolution(160, 120);

            /// <summary>
            /// Initializes a new <see cref="PictureResolution"/> object.
            /// </summary>
            /// <param name="width">Width supported resolution in pixels.</param>
            /// <param name="height">Height of supported resolution in pixels.</param>
            public PictureResolution(int width, int height)
            {
                Width = width;
                Height = height;
            }

            /// <summary>
            /// Initializes a new <see cref="PictureResolution"/> object from a member of the <see cref="DefaultResolutions"/> enumeration.
            /// </summary>
            /// <param name="resolution">A member of the <see cref="DefaultResolutions"/> enumeration.</param>
            public PictureResolution(DefaultResolutions resolution)
            {
                switch (resolution)
                {
                    case DefaultResolutions._320x240:
                        Width = 320;
                        Height = 240;
                        break;
                    case DefaultResolutions._160x120:
                        Width = 160;
                        Height = 120;
                        break;
                    case DefaultResolutions._176x144:
                        Width = 176;
                        Height = 144;
                        break;
                }
            }

            /// <summary>
            /// Enumeration of supported resolutions.
            /// </summary>
            public enum DefaultResolutions
            {
                /// <summary>
                /// Width 320, height 240.
                /// </summary>
                _320x240,
                /// <summary>
                /// Width 176, height 144.
                /// </summary>
                _176x144,
                /// <summary>
                /// Width 160, height 120.
                /// </summary>
                _160x120
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PictureResolution"/> enumeration.
        /// </summary>
        public PictureResolution CurrentPictureResolution { get; set; }

        private USBH_Webcam.ImageFormat GetImageFormat(PictureResolution pictureResolution)
        {
            USBH_Webcam.ImageFormat imageFormat = null;
            USBH_Webcam.ImageFormat[] imageFormats;

            if (_camera != null)
            {
                try
                {
                    imageFormats = _camera.GetSupportedFormats();
                    for (int i = 0; i < imageFormats.Length; i++)
                    {
                        if (imageFormats[i].Width == pictureResolution.Width && imageFormats[i].Height == pictureResolution.Height)
                        {
                            imageFormat = imageFormats[i];
                        }
                    }
                }
                catch
                {
                    throw new Exception("Unable to get supported image formats from camera.");
                }
            }
            else
            {
                throw new Exception("Camera must be connected to be able to get valid image formats.");
            }

            if (imageFormat != null)
            {
                return imageFormat;
            }
            else
            {
                throw new ArgumentException("No valid image formats were found for the specified PictureResolution.");
            }

        }


        /// <summary>
        /// Takes a picture using the <see cref="Camera"/> object.
        /// </summary>
        /// <remarks>
        /// Taking picture involves start streaming which might take several seconds to stabilize. Camera driver keeps the stream on then it stops streaming internally if it was idle for more than 1 minute by default. This time out can be changed through <see cref="TakePictureStreamTimeout"/>
        /// </remarks>     
        public void TakePicture()
        {
            lock (this.cameraLock)
            {
                if (this._cameraStatus == CameraStatus.Disconnected)
                {
                    ErrorPrint("Unable to take picture. Camera is not ready. Is the Camera connected?");
                }
                else if (this._cameraStatus == CameraStatus.TakePicture)
                {
                    ErrorPrint("Unable to take picture. The camera is already busy taking a picture.");
                }
                else if (this._cameraStatus == CameraStatus.StreamBitmap)
                {
                    ErrorPrint("Unable to start streaming. The camera is already busy streaming a bitmap. Call StopStreamingBitmaps to cancel that process.");
                }
                else if (this._cameraStatus == CameraStatus.Ready)
                {

                    if (!_TakePictureFlag)
                    {
                        _camera.StartStreaming(GetImageFormat(CurrentPictureResolution));
                    }

                    _LastTimeStreamStart = DateTime.Now;
                    this._cameraStatus = CameraStatus.TakePicture;
                }
            }
        }

        private Bitmap _targetBitmap;

        /// <summary>
        /// Starts streaming the bitmap identified by the bitmap parameter.
        /// </summary>
        /// <param name="bitmap">Bitmap of the same dimensions as the <see cref="CurrentPictureResolution"/> property.</param>
        public void StartStreamingBitmaps(Bitmap bitmap)
        {
            lock (this.cameraLock)
            {
                if (this._cameraStatus == CameraStatus.Disconnected)
                {
                    ErrorPrint("Unable to take picture. Camera is not ready. Is the Camera connected?");
                }
                else if (this._cameraStatus == CameraStatus.TakePicture)
                {
                    ErrorPrint("Unable to take picture. The camera is already busy taking a picture.");
                }
                else if (this._cameraStatus == CameraStatus.StreamBitmap)
                {
                    StopStreamingBitmaps();
                }

                if (CurrentPictureResolution.Height != bitmap.Height || CurrentPictureResolution.Width != bitmap.Width)
                {
                    throw new ArgumentException("Bitmap is not the same size as the current PictureResoltion", "bitmap");
                }

                if (this._cameraStatus == CameraStatus.Ready)
                {
                    _TakePictureFlag = false;
                    _LastTimeStreamStart = DateTime.Now;
                    _targetBitmap = bitmap;
                    _camera.StartStreaming(GetImageFormat(CurrentPictureResolution));
                    this._cameraStatus = CameraStatus.StreamBitmap;

                }
            }
        }

        /// <summary>
        /// Stops streaming of bitmaps by the <see cref="Camera"/> object.
        /// </summary>
        public void StopStreamingBitmaps()
        {
            lock (this.cameraLock)
            {
                if (_cameraStatus == CameraStatus.StreamBitmap)
                {
                    _TakePictureFlag = false;
                    _camera.StopStreaming();
                    _cameraStatus = CameraStatus.Ready;
                }
            }
        }

        private bool _runThread;

        private void CameraCommunication()
        {
            while (_runThread)
            {
                try
                {
                    lock (cameraLock)
                    {

                        if (_cameraStatus == CameraStatus.TakePicture || _cameraStatus == CameraStatus.StreamBitmap)
                        {
                            if (_camera.IsNewImageReady())
                            {

                                _LastTimeStreamStart = DateTime.Now;
                                if (_cameraStatus == CameraStatus.TakePicture) _targetBitmap = new Bitmap(_camera.CurrentFormat.Width, _camera.CurrentFormat.Height);

                                _camera.DrawImage(_targetBitmap, 0, 0, _camera.CurrentFormat.Width, _camera.CurrentFormat.Height);

                                if (_cameraStatus == CameraStatus.StreamBitmap)
                                {
                                    OnBitmapStreamedEvent(this, _targetBitmap);
                                }
                                else if (_cameraStatus == CameraStatus.TakePicture)
                                {
                                    //_camera.StopStreaming();
                                    _TakePictureFlag = true;
                                    _takePictureStreamingTimeoutTimeSpan = new TimeSpan(10 * 1000 * _takePictureStreamingTimeout);
                                    _LastTimeTakePictureCalled = DateTime.Now;

                                    byte[] bmpFile = new byte[_targetBitmap.Width * _targetBitmap.Height * 3 + 54];
                                    Util.BitmapToBMPFile(_targetBitmap.GetBitmap(), _targetBitmap.Width, _targetBitmap.Height, bmpFile);
                                    Picture picture = new Picture(bmpFile, Picture.PictureEncoding.BMP);
                                    OnPictureCapturedEvent(this, picture);
                                    _cameraStatus = CameraStatus.Ready;
                                }
                            }
                            else
                            {
                                _restartStreamingTriggerTimeSpan = new TimeSpan(10 * 1000 * _restartStreamingTrigger);
                                if ((DateTime.Now - _LastTimeStreamStart) > _restartStreamingTriggerTimeSpan)
                                {
                                    DebugPrint("Synchronizing..");

                                    _camera.StopStreaming();
                                    _camera.StartStreaming(GetImageFormat(CurrentPictureResolution));
                                    _LastTimeStreamStart = DateTime.Now;
                                }
                            }


                        }
                        if (_TakePictureFlag // If a picture has been taken recently using TakePicture()
                            && (_cameraStatus != CameraStatus.TakePicture) // If the system is not trying to aquire a picture for TakePicture() currently
                            && ((DateTime.Now - _LastTimeTakePictureCalled) > _takePictureStreamingTimeoutTimeSpan))// The Stream has been on abd idle for a while for TakePicture().
                        {

                            _TakePictureFlag = false;
                            _camera.StopStreaming();

                        }
                    }
                }
                catch
                {
                    ErrorPrint("Unable to get picture from camera");
                }
                Thread.Sleep(100);
            }
        }


        ///// <summary>
        ///// Gets or sets the size of the image.
        ///// </summary>
        ///// <remarks>
        ///// Set this property to the desired image size before calling the <see cref="TakePicture"/> or <see cref="StreamPicture"/> method.
        ///// </remarks>
        //public ImageSize CameraImageSize { get; set; }

        /// <summary>
        /// Represents the delegate that is used for the <see cref="PictureCaptured"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Camera"/> object that raised the event.</param>
        /// <param name="picture">A <see cref="T:Gadgeteer.Picture"/> containing the captured image.</param>
        public delegate void PictureCapturedEventHandler(Camera sender, Picture picture);

        /// <summary>
        /// Event raised when the <see cref="Camera"/> has completed an image capture.
        /// </summary>
        /// <remarks>
        /// Handle the <see cref="PictureCaptured"/> event to process image data
        /// after you call the <see cref="TakePicture"/> or <see cref="StartStreamingBitmaps"/>
        /// methods. These methods process the image data from the camera asynchronously, 
        /// and raise this event when the processing is complete.
        /// </remarks>
        public event PictureCapturedEventHandler PictureCaptured;

        private PictureCapturedEventHandler OnPictureCaptured;

        /// <summary>
        /// Raises the <see cref="PictureCaptured"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Camera"/> that raised the event.</param>
        /// <param name="picture">A <see cref="T:Gadgeteer.Picture"/> containing the captured image.</param>
        protected virtual void OnPictureCapturedEvent(Camera sender, Picture picture)
        {
            if (OnPictureCaptured == null) OnPictureCaptured = new PictureCapturedEventHandler(OnPictureCapturedEvent);
            if (Program.CheckAndInvoke(PictureCaptured, OnPictureCaptured, sender, picture))
            {
                PictureCaptured(sender, picture);
            }
        }

        /// <summary>
        /// Represents the delegate that is used to handle the <see cref="BitmapStreamed"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Camera"/> object that raised the event.</param>
        /// <param name="bitmap">A <see cref="Bitmap"/> containing the captured image.</param>
        public delegate void BitmapStreamedEventHandler(Camera sender, Bitmap bitmap);

        /// <summary>
        /// Event raised when the camera has completed streaming a bitmap.
        /// </summary>
        public event BitmapStreamedEventHandler BitmapStreamed;

        private BitmapStreamedEventHandler OnBitmapStreamed;

        /// <summary>
        /// Raises the <see cref="BitmapStreamed"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Camera"/> that raised the event.</param>
        /// <param name="bitmap">The <see cref="Bitmap"/> that contains the bitmap from the camera.</param>
        protected virtual void OnBitmapStreamedEvent(Camera sender, Bitmap bitmap)
        {
            if (OnBitmapStreamed == null) OnBitmapStreamed = new BitmapStreamedEventHandler(OnBitmapStreamedEvent);
            if (Program.CheckAndInvoke(BitmapStreamed, OnBitmapStreamed, sender, bitmap))
            {
                BitmapStreamed(sender, bitmap);
            }
        }

    }
}
