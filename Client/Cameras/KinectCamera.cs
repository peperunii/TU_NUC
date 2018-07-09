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
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Network.Logger;

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
        
        public Body[] bodies = null;

        private Image<Bgr, Byte> colorImage = null;
        private Image<Gray, ushort> depthImage = null;
        private Image<Gray, ushort> irImage = null;

        public byte[] colorImageByteArr = null;
        public ushort[] depthImageUshortArr = null;
        public ushort[] irImageUshortArr = null;

        /*IR settings*/
        private const float InfraredSourceScale = 0.75f;
        private const float InfraredOutputValueMinimum = 0.01f;
        private const float InfraredOutputValueMaximum = 1.0f;
        private const float InfraredSourceValueMaximum = (float)ushort.MaxValue;

        /*Depth settings*/
        private const int MapDepthToByte = 8000 / 256;
        private ushort MinReliableDepth = ushort.MinValue;
        private ushort MaxReliableDepth = ushort.MaxValue;

        /*Resolution Scale settings*/
        private double colorScale = 1.0;
        private double depthScale = 1.0;
        private double irScale = 1.0;

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
            this.colorImage = new Image<Bgr, byte>(colorFrameDescription.Width, colorFrameDescription.Height);

            this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
            this.depthImage = new Image<Gray, ushort>(depthFrameDescription.Width, depthFrameDescription.Height);

            this.irFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
            this.irImage = new Image<Gray, ushort>(irFrameDescription.Width, irFrameDescription.Height);
        }

        public void Start()
        {
            this.kinectSensor.Open();
        }

        public void Stop()
        {
            this.kinectSensor.Close();
        }

        public void SetScaleFactor(CameraDataType type, double scale)
        {
            switch(type)
            {
                case CameraDataType.Color:
                    this.colorScale = scale;
                    break;

                case CameraDataType.Depth:
                    this.depthScale = scale;
                    break;

                case CameraDataType.IR:
                    this.irScale = scale;
                    break;
            }
        }

        public void SetDepthMinReliable(ushort minReliableDepth)
        {
            this.MinReliableDepth = minReliableDepth;
        }

        public void SetDepthMaxReliable(ushort maxReliableDepth)
        {
            this.MaxReliableDepth = maxReliableDepth;
        }

        private unsafe void ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        // verify data and write the new color frame data to the display bitmap

                        var dataSize = this.colorFrameDescription.Width * this.colorFrameDescription.Height * 2;
                        this.colorImageByteArr = new byte[dataSize];

                        //colorFrame.CopyConvertedFrameDataToArray(byteArr, ColorImageFormat.Bgra);
                        //var multiByteArr = this.ConvertSingleToMultiArr(byteArr, this.colorFrameDescription.Width, this.colorFrameDescription.Height);

                        fixed (byte* frameData = &this.colorImageByteArr[0])
                        {
                            IntPtr ptr = (IntPtr)frameData;

                            colorFrame.CopyRawFrameDataToIntPtr(ptr, (uint)dataSize);
                        }
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

        private byte[,,] ConvertSingleToMultiArr(byte[] byteArr, int width, int height)
        {
            var multiByteArr = new byte[height, width, 3];

            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    var index = (i * width + j) * 4;
                    multiByteArr[i, j, 0] = byteArr[index];
                    multiByteArr[i, j, 1] = byteArr[index + 1];
                    multiByteArr[i, j, 2] = byteArr[index + 2];
                }
            }

            return multiByteArr;
        }

        private unsafe void DepthFrameReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {

                        // Note: In order to see the full range of depth (including the less reliable far field depth)
                        // we are setting maxDepth to the extreme potential depth threshold
                        
                        ushort* frameData = (ushort*)depthBuffer.UnderlyingBuffer;
                        var dataSize = depthFrameDescription.Width * depthFrameDescription.Height;

                        IntPtr ptr = (IntPtr)frameData;
                        depthFrame.CopyFrameDataToIntPtr(ptr, (uint)dataSize);

                        this.depthImage = new Image<Gray, ushort>(depthFrameDescription.Width, depthFrameDescription.Height, depthFrameDescription.Width, ptr);
                        if (this.depthScale != 1)
                        {
                            this.depthImage = this.depthImage.Resize(this.depthScale, Inter.Cubic);
                        }

                        if(this.MaxReliableDepth != ushort.MaxValue)
                        {
                            this.ConvertDepthImageToMaxDistance();
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
        }

        private unsafe void IrFrameReader_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            // InfraredFrame is IDisposable
            using (InfraredFrame infraredFrame = e.FrameReference.AcquireFrame())
            {
                if (infraredFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer infraredbuffer = infraredFrame.LockImageBuffer())
                    {
                        ushort* frameData = (ushort*)infraredbuffer.UnderlyingBuffer;
                        var dataSize = irFrameDescription.Width * irFrameDescription.Height;

                        IntPtr ptr = (IntPtr)frameData;
                        infraredFrame.CopyFrameDataToIntPtr(ptr, (uint)dataSize);

                        this.irImage = new Image<Gray, ushort>(irFrameDescription.Width, irFrameDescription.Height, irFrameDescription.Width, ptr);
                        if (this.irScale != 1)
                        {
                            this.irImage = this.irImage.Resize(this.irScale, Inter.Cubic);
                        }

                        this.ConvertIRImage();
                    }

                    //fire event
                    dOnDepthFrameArrived lEvent = OnDepthFrameArrived;
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
        private void ConvertDepthImageToMaxDistance()
        {
            var data = this.depthImage.Data;
            var width = this.depthImage.Width;
            var height = this.depthImage.Height;

            for(int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var depth = data[i, j, 0];
                    data[i, j, 0] = (depth >= this.MinReliableDepth && depth <= this.MaxReliableDepth ? depth : (ushort)0);
                }
            }
        }


        private void ConvertIRImage()
        {
            var data = this.irImage.Data;
            var width = this.irImage.Width;
            var height = this.irImage.Height;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var ir = data[i, j, 0];
                    data[i, j, 0] = 
                        (ushort)Math.Min(
                            InfraredOutputValueMaximum, 
                            (((float)ir / InfraredSourceValueMaximum * InfraredSourceScale) * (1.0f - InfraredOutputValueMinimum)) + InfraredOutputValueMinimum);
                }
            }
        }

        
        private unsafe byte[] GetColorImageByteArr()
        {
            return this.colorImageByteArr;

            if (this.colorImage != null)
            {
                var byteArr = this.colorImage.Data;
                byte[] baData = new byte[byteArr.Length];

                Buffer.BlockCopy(byteArr, 0, baData, 0, byteArr.Length);

                return baData;
            }
            else return null;
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
                    return this.Serialize(this.colorImage, type);

                case CameraDataType.Depth:
                    return this.Serialize(this.depthImage, type);

                case CameraDataType.IR:
                    return this.Serialize(this.irImage, type);

                case CameraDataType.Body:
                    return this.Serialize(this.bodies, type);

                default:
                    return null;
            }
        }
    }
}
