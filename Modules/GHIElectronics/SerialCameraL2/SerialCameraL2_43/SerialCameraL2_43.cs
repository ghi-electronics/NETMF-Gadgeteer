using Microsoft.SPOT;
using System;
using System.Threading;
using GTI = Gadgeteer.SocketInterfaces;
using GTM = Gadgeteer.Modules;

namespace Gadgeteer.Modules.GHIElectronics
{
    /// <summary>
    /// A SerialCameraL2 module for Microsoft .NET Gadgeteer
    /// </summary>
    public class SerialCameraL2 : GTM.Module
    {
        private const int CMD_DELAY_TIME = 50;
        private const int RESET_DELAY_TIME = 1000;
        private const int POWERUP_DELAY_TIME = 2000;

        private GTI.Serial port;
        private bool newImageReady;
        private bool updateStreaming;
        private bool paused;
        private byte[] imageData;
        private int dataSize;
        private Thread worker;
        private Resolution resolution;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public SerialCameraL2(int socketNumber)
        {
            Socket socket = Socket.GetSocket(socketNumber, true, this, null);
            socket.EnsureTypeIsSupported('U', this);

            this.port = GTI.SerialFactory.Create(socket, 115200, GTI.SerialParity.None, GTI.SerialStopBits.One, 8, GTI.HardwareFlowControl.NotRequired, this);
            this.port.ReadTimeout = 500;
            this.port.WriteTimeout = 500;
            this.port.Open();
            this.ResetCamera();

            this.newImageReady = false;
            this.updateStreaming = false;
            this.paused = false;
        }

        /// <summary>
        /// The possible resolutions.
        /// </summary>
        public enum Resolution
        {
            /// <summary>
            /// 640x480
            /// </summary>
            VGA = 0x00,

            /// <summary>
            /// 320x240
            /// </summary>
            QVGA = 0x11,

            /// <summary>
            /// 160x120
            /// </summary>
            QQVGA = 0x22,
        }

        /// <summary>
        /// Whether or not a new image is ready.
        /// </summary>
        public bool NewImageReady
        {
            get
            {
                return this.newImageReady;
            }
        }

        private void DiscardBuffers()
        {
            this.port.DiscardInBuffer();
            this.port.DiscardOutBuffer();
        }

        private byte[] ReadBytes()
        {
            int available = this.port.BytesToRead;

            if (available <= 0)
                return null;

            byte[] buffer = new byte[available];
            this.ReadBytes(buffer, 0, buffer.Length);
            return buffer;
        }

        private void ReadBytes(byte[] buffer, int offset, int count)
        {
            int attempts = 0;

            do
            {
                int read = this.port.Read(buffer, offset, count);

                offset += read;
                count -= read;

                if (read == 0)
                {
                    this.port.DiscardInBuffer();

                    if (attempts++ > 1)
                        throw new InvalidOperationException("Failed to read all of the bytes from the port.");

                    continue;
                }

                Thread.Sleep(1);
            } while (count > 0);
        }

        private bool ResetCamera()
        {
            this.port.Write(0x56, 0x00, 0x26, 0x00);

            Thread.Sleep(SerialCameraL2.RESET_DELAY_TIME);

            var received = this.ReadBytes();

            return received != null && received.Length > 0;
        }

        private bool StopFrameBufferControl()
        {
            this.DiscardBuffers();

            this.port.Write(0x56, 0x00, 0x36, 0x01, 0x00);

            Thread.Sleep(SerialCameraL2.CMD_DELAY_TIME);

            byte[] received = this.ReadBytes();

            return received != null && received.Length >= 4 && received[0] == 0x76 && received[1] == 0 && received[2] == 0x36 && received[3] == 0 && received[4] == 0;
        }

        private void ResumeToNextFrame()
        {
            this.DiscardBuffers();

            this.port.Write(0x56, 0x00, 0x36, 0x01, 0x03);

            this.ReadBytes(new byte[5], 0, 5);
        }

        private int GetFrameBufferLength()
        {
            this.DiscardBuffers();

            this.port.Write(0x56, 0x00, 0x34, 0x01, 0x00);

            Thread.Sleep(SerialCameraL2.CMD_DELAY_TIME);

            int size = 0;
            byte[] receive = this.ReadBytes();

            if (receive != null && receive.Length >= 9 && receive[0] == 0x76 && receive[1] == 0 && receive[2] == 0x34 && receive[3] == 0 && receive[4] == 0x4)
                size = receive[5] << 24 | receive[6] << 16 | receive[7] << 8 | receive[8];

            return size;
        }

        private bool ReadFrameBuffer()
        {
            this.DiscardBuffers();

            this.port.Write(0x56, 0x00, 0x32, 0x0C, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x00, (byte)(this.dataSize >> 24), (byte)(this.dataSize >> 16), (byte)(this.dataSize >> 8), (byte)(this.dataSize), 0x10, 0x00);

            Thread.Sleep(10);

            try
            {
                byte[] header = new byte[5];

                this.ReadBytes(header, 0, 5);
                this.ReadBytes(this.imageData, 0, this.dataSize);
                this.ReadBytes(header, 0, 5);
            }
            catch
            {
                this.imageData = null;

                return false;
            }

            if (this.imageData[0] == 0xFF && this.imageData[1] == 0xD8 && this.imageData[this.dataSize - 2] == 0xFF && this.imageData[this.dataSize - 1] == 0xD9)
                return true;

            this.imageData = null;

            return false;
        }

        /// <summary>
        /// The image resolution
        /// </summary>
        public Resolution ImageResolution
        {
            get
            {
                return this.resolution;
            }
            set
            {
                this.port.Write(0x56, 0x00, 0x31, 0x05, 0x04, 0x01, 0x00, 0x19, (byte)value);

                Thread.Sleep(SerialCameraL2.CMD_DELAY_TIME);

                byte[] received = this.ReadBytes();

                if (!(received != null && received[0] == 0x76 && received[1] == 0x00 && received[2] == 0x31 && received[3] == 0x00 && received[4] == 0x00))
                    throw new InvalidOperationException("The resolution failed to set.");

                this.resolution = value;
            }
        }

        /// <summary>
        /// Sets the ratio.
        /// </summary>
        /// <param name="ratio">The ratio.</param>
        /// <returns>Whether it was successful or not.</returns>
        public bool SetRatio(byte ratio)
        {
            this.DiscardBuffers();

            this.port.Write(0x56, 0x00, 0x31, 0x05, 0x04, 0x01, 0x00, 0x1A, ratio);

            Thread.Sleep(SerialCameraL2.CMD_DELAY_TIME);

            byte[] received = this.ReadBytes();
            if (received != null && received.Length >= 5 && received[0] == 0x76 && received[1] == 0 && received[2] == 0x31 && received[3] == 0 && received[4] == 0x0)
            {
                this.ResetCamera();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Starts streaming from the device.
        /// </summary>
        public void StartStreaming()
        {
            this.StopStreaming();

            this.updateStreaming = true;
            this.newImageReady = false;
            this.paused = false;

            this.worker = new Thread(this.UpdateStreaming);
            this.worker.Start();
        }

        /// <summary>
        /// Stops streaming from the device.
        /// </summary>
        public void StopStreaming()
        {
            this.updateStreaming = false;
            this.newImageReady = false;

            if (this.worker != null)
            {
                while (this.worker.IsAlive)
                    Thread.Sleep(SerialCameraL2.CMD_DELAY_TIME);

                this.worker = null;
            }
        }

        /// <summary>
        /// Pauses streaming from the device.
        /// </summary>
        public void PauseStreaming()
        {
            this.paused = true;
        }

        /// <summary>
        /// Resumes streaming from the device.
        /// </summary>
        public void ResumeStreaming()
        {
            this.paused = false;
        }

        private void UpdateStreaming()
        {
            while (this.updateStreaming)
            {
                if (this.paused)
                {
                    Thread.Sleep(10);
                    continue;
                }

                this.StopFrameBufferControl();
                this.dataSize = this.GetFrameBufferLength();

                if (this.dataSize > 0)
                {
                    this.newImageReady = false;

                    this.imageData = null;
                    this.imageData = new byte[dataSize];

                    this.newImageReady = this.ReadFrameBuffer();
                    if (!this.newImageReady)
                    {
                        this.ResetCamera();
                    }
                    else
                    {
                        this.ResumeToNextFrame();
                    }
                }
            }
        }

        /// <summary>
        /// Returns the image data.
        /// </summary>
        /// <returns>The image data.</returns>
        public byte[] GetImageData()
        {
            if (this.newImageReady && this.imageData != null && this.imageData.Length > 0)
            {
                this.newImageReady = false;

                return this.imageData;
            }

            return null;
        }

        /// <summary>
        /// Returns the image.
        /// </summary>
        /// <returns>The image.</returns>
        public Bitmap GetImage()
        {
            Bitmap bmp = null;

            switch (this.resolution)
            {
                case Resolution.VGA: bmp = new Bitmap(640, 480); break;
                case Resolution.QVGA: bmp = new Bitmap(320, 240); break;
                case Resolution.QQVGA: bmp = new Bitmap(160, 120); break;
                default: throw new InvalidOperationException("Invalid resolution specified.");
            }

            this.DrawImage(bmp);

            return bmp;
        }

        /// <summary>
        /// Draws the image onto the bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap to draw.</param>
        public void DrawImage(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");
            if (bitmap.Width <= 0) throw new ArgumentOutOfRangeException("bitmap", "bitmap.Width must be positive.");
            if (bitmap.Height <= 0) throw new ArgumentOutOfRangeException("bitmap", "bitmap.Height must be positive.");

            if (this.newImageReady && this.imageData != null && this.imageData.Length > 0)
            {
                this.newImageReady = false;
                this.PauseStreaming();

                var image = new Bitmap(this.imageData, Bitmap.BitmapImageType.Jpeg);
                bitmap.DrawImage(0, 0, image, 0, 0, bitmap.Width, bitmap.Height);

                this.ResumeStreaming();
            }
        }

        /// <summary>
        /// Draws the image onto the bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap to draw.</param>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="width">The width of the image to draw.</param>
        /// <param name="height">The height of the image to draw.</param>
        public void DrawImage(Bitmap bitmap, int x, int y, int width, int height)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");
            if (x < 0) throw new ArgumentOutOfRangeException("x", "x must be non-negative.");
            if (y < 0) throw new ArgumentOutOfRangeException("y", "y must be non-negative.");
            if (width <= 0) throw new ArgumentOutOfRangeException("width", "width must be positive.");
            if (height <= 0) throw new ArgumentOutOfRangeException("height", "height must be positive.");
            if (x + width > bitmap.Width) throw new ArgumentOutOfRangeException("bitmap", "x + width must be no more than bitmap.Width.");
            if (y + height > bitmap.Height) throw new ArgumentOutOfRangeException("bitmap", "x + height must be no more than bitmap.Height.");

            if (this.newImageReady && this.imageData != null && this.imageData.Length > 0)
            {
                this.newImageReady = false;
                this.PauseStreaming();

                var image = new Bitmap(this.imageData, Bitmap.BitmapImageType.Jpeg);
                bitmap.DrawImage(x, y, image, 0, 0, width, height);

                this.ResumeStreaming();
            }
        }
    }
}
