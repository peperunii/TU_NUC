using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.IO;

namespace Client.Cameras
{
    public class KinectCamera : Camera
    {
        private KinectSensor kinectSensor = null;

        private ColorFrameReader colorFrameReader = null;
        private DepthFrameReader depthFrameReader = null;
        private InfraredFrameReader irFrameReader = null;
        private BodyFrameReader bodyFrameReader = null;

        private FrameDescription colorFrameDescription;
        private FrameDescription depthFrameDescription;
        private FrameDescription irFrameDescription;

        private WriteableBitmap colorBitmap = null;
        private WriteableBitmap depthBitmap = null;
        private WriteableBitmap irBitmap = null;
        private Body[] bodies = null;

        /*IR settings*/
        private const float InfraredSourceScale = 0.75f;
        private const float InfraredOutputValueMinimum = 0.01f;
        private const float InfraredOutputValueMaximum = 1.0f;
        private const float InfraredSourceValueMaximum = (float)ushort.MaxValue;

        /*Depth settings*/
        private byte[] depthPixels = null;
        private const int MapDepthToByte = 8000 / 256;

        /*Events*/
        public delegate void dOnColorFrameArrived();
        public event dOnColorFrameArrived OnColorFrameArrived;

        public delegate void dOnDepthFrameArrived();
        public event dOnDepthFrameArrived OnDepthFrameArrived;

        public delegate void dOnIRFrameArrived();
        public event dOnIRFrameArrived OnIRFrameArrived;

        public delegate void dOnBodyFrameArrived();
        public event dOnBodyFrameArrived OnBodyFrameArrived;

        public KinectCamera()
        {
            this.kinectSensor = KinectSensor.GetDefault();

            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();
            this.irFrameReader = this.kinectSensor.InfraredFrameSource.OpenReader();
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            this.colorFrameReader.FrameArrived += this.ColorFrameReader_FrameArrived;
            this.depthFrameReader.FrameArrived += this.DepthFrameReader_FrameArrived;
            this.irFrameReader.FrameArrived += this.IrFrameReader_FrameArrived;
            this.bodyFrameReader.FrameArrived += this.BodyFrameReader_FrameArrived;

            this.colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
            this.depthBitmap = new WriteableBitmap(depthFrameDescription.Width, depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);

            this.irFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
            this.irBitmap = new WriteableBitmap(irFrameDescription.Width, irFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray32Float, null);

            this.depthPixels = new byte[this.depthFrameDescription.Width * this.depthFrameDescription.Height];

            this.kinectSensor.Open();
        }


        private void ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                    }

                    //fire event
                    dOnColorFrameArrived lEvent = OnColorFrameArrived;
                    if (lEvent != null)
                    {
                        lEvent();
                    }
                }
            }
        }

        private void DepthFrameReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            bool depthFrameProcessed = false;

            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                        if (((this.depthFrameDescription.Width * this.depthFrameDescription.Height) == (depthBuffer.Size / this.depthFrameDescription.BytesPerPixel)) &&
                            (this.depthFrameDescription.Width == this.depthBitmap.PixelWidth) && (this.depthFrameDescription.Height == this.depthBitmap.PixelHeight))
                        {
                            // Note: In order to see the full range of depth (including the less reliable far field depth)
                            // we are setting maxDepth to the extreme potential depth threshold
                            ushort maxDepth = ushort.MaxValue;

                            // If you wish to filter by reliable depth distance, uncomment the following line:
                            //// maxDepth = depthFrame.DepthMaxReliableDistance

                            this.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);
                            depthFrameProcessed = true;
                        }
                    }

                    //fire event
                    dOnDepthFrameArrived lEvent = OnDepthFrameArrived;
                    if (lEvent != null)
                    {
                        lEvent();
                    }
                }
            }

            if (depthFrameProcessed)
            {
                this.RenderDepthPixels();
            }
        }

        private void IrFrameReader_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            // InfraredFrame is IDisposable
            using (InfraredFrame infraredFrame = e.FrameReference.AcquireFrame())
            {
                if (infraredFrame != null)
                {
                    // the fastest way to process the infrared frame data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer infraredBuffer = infraredFrame.LockImageBuffer())
                    {
                        // verify data and write the new infrared frame data to the display bitmap
                        if (((this.irFrameDescription.Width * this.irFrameDescription.Height) == (infraredBuffer.Size / this.irFrameDescription.BytesPerPixel)) &&
                            (this.irFrameDescription.Width == this.irBitmap.PixelWidth) && (this.irFrameDescription.Height == this.irBitmap.PixelHeight))
                        {
                            this.ProcessInfraredFrameData(infraredBuffer.UnderlyingBuffer, infraredBuffer.Size);
                        }
                    }

                    //fire event
                    dOnIRFrameArrived lEvent = OnIRFrameArrived;
                    if (lEvent != null)
                    {
                        lEvent();
                    }
                }
            }
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);

                    //fire event
                    dOnBodyFrameArrived lEvent = OnBodyFrameArrived;
                    if (lEvent != null)
                    {
                        lEvent();
                    }
                }
            }
        }

        #region Helpers
        private unsafe void ProcessInfraredFrameData(IntPtr infraredFrameData, uint infraredFrameDataSize)
        {
            // infrared frame data is a 16 bit value
            ushort* frameData = (ushort*)infraredFrameData;

            // lock the target bitmap
            this.irBitmap.Lock();

            // get the pointer to the bitmap's back buffer
            float* backBuffer = (float*)this.irBitmap.BackBuffer;

            // process the infrared data
            for (int i = 0; i < (int)(infraredFrameDataSize / this.irFrameDescription.BytesPerPixel); ++i)
            {
                // since we are displaying the image as a normalized grey scale image, we need to convert from
                // the ushort data (as provided by the InfraredFrame) to a value from [InfraredOutputValueMinimum, InfraredOutputValueMaximum]
                backBuffer[i] = Math.Min(InfraredOutputValueMaximum, (((float)frameData[i] / InfraredSourceValueMaximum * InfraredSourceScale) * (1.0f - InfraredOutputValueMinimum)) + InfraredOutputValueMinimum);
            }

            // mark the entire bitmap as needing to be drawn
            this.irBitmap.AddDirtyRect(new Int32Rect(0, 0, this.irBitmap.PixelWidth, this.irBitmap.PixelHeight));

            // unlock the bitmap
            this.irBitmap.Unlock();
        }

        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
        }

        private void RenderDepthPixels()
        {
            this.depthBitmap.WritePixels(
                new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                this.depthPixels,
                this.depthBitmap.PixelWidth,
                0);
        }

        private unsafe byte[] GetColorImageByteArr()
        {
            var byteArr = new byte[this.colorFrameDescription.Width * this.colorFrameDescription.Height * 3];
            var dataPointer = (byte *)this.colorBitmap.BackBuffer;
            var totalBufferSize = this.colorFrameDescription.Width * this.colorFrameDescription.Height * 4;
            var curIndex = 0;

            for (int i = 0; i < totalBufferSize; ++i)
            {
                if (i % 3 == 0) continue;
                byteArr[curIndex++] = dataPointer[i];
            }

            return byteArr;
        }

        private byte [] Serialize(object objToSerialize, CameraDataType type)
        {
            byte[] result = null;

            switch(type)
            {
                case CameraDataType.Color:
                    result = this.GetColorImageByteArr();
                    break;

                default:
                    break;
            }

            return result;
        }

        #endregion

        public override object GetData(CameraDataType type)
        {
            switch(type)
            {
                case CameraDataType.Color:
                    return this.Serialize(this.colorBitmap, type);

                case CameraDataType.Depth:
                    return this.Serialize(this.depthBitmap, type);

                case CameraDataType.IR:
                    return this.Serialize(this.irBitmap, type);

                case CameraDataType.Body:
                    return this.Serialize(this.bodies, type);

                default:
                    return null;
            }
        }
    }
}
