﻿using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using Network.Messages;
using System;
using System.Collections.Generic;

namespace Client.Cameras
{
    public class KinectCamera : Camera
    {
        private KinectSensor kinectSensor = null;

        private ColorFrameReader colorFrameReader = null;
        private DepthFrameReader depthFrameReader = null;
        private InfraredFrameReader irFrameReader = null;
        private BodyFrameReader bodyFrameReader = null;

        public FrameDescription colorFrameDescription;
        public FrameDescription depthFrameDescription;
        public FrameDescription irFrameDescription;
        
        public Body[] bodies = null;

        private Image<Bgr, Byte> colorImage = null;
        private Image<Gray, ushort> depthImage = null;
        private Image<Gray, ushort> irImage = null;

        public byte[] colorImageByteArr = null;
        public ushort[] depthImageUshortArr = null;
        public byte[] irImageByteArr = null;

        /*IR settings*/
        private const float InfraredSourceScale = 0.75f;
        private const float InfraredOutputValueMinimum = 0.01f;
        private const float InfraredOutputValueMaximum = 1.0f;
        private const float InfraredSourceValueMaximum = (float)4096;// ushort.MaxValue;

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

        internal bool IsTrackedBodyFound()
        {
            var result = false;

            foreach(var body in this.bodies)
            {
                if(body.IsTracked == true)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public void Start()
        {
            this.kinectSensor.Open();
        }

        public void Stop()
        {
            this.kinectSensor.Close();
        }

        public List<Skeleton> GetConvertedBodyArr()
        {
            var listSkeletons = new List<Skeleton>();

            foreach(var body in this.bodies)
            {
                listSkeletons.Add(new Skeleton(body));
            }

            return listSkeletons;
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
                        var dataSize = this.depthFrameDescription.Width * this.depthFrameDescription.Height * 2;
                        this.depthImageUshortArr = new ushort[dataSize];

                        fixed (ushort* frameData = &this.depthImageUshortArr[0])
                        {
                            IntPtr ptr = (IntPtr)frameData;

                            depthFrame.CopyFrameDataToIntPtr(ptr, (uint)dataSize);
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

        private void IrFrameReader_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
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
                        this.ProcessIRFrame(infraredbuffer.UnderlyingBuffer, infraredbuffer.Size/ 2);
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

        private unsafe void ProcessIRFrame(IntPtr underlyingBuffer, uint dataSize)
        {
            ushort* frameData = (ushort*)underlyingBuffer;
            
            this.irImageByteArr = new byte[dataSize];

            try
            {
                float min = 10000;
                float max = -1110;

                for (int i = 0; i < dataSize; i++)
                {
                    var calc = 255 * Math.Min(
                            InfraredOutputValueMaximum,
                            (((float)frameData[i] / InfraredSourceValueMaximum * InfraredSourceScale) * (1.0f - InfraredOutputValueMinimum)) + InfraredOutputValueMinimum);
                    if (calc > max) max = calc;
                    if (calc < min) min = calc;
                     calc = calc > 255 ? 255 : calc;
                    this.irImageByteArr[i] = (byte)calc;
                }
                
            }
            catch (Exception ex) { Console.WriteLine(ex); }
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
        }

        private unsafe byte[] GetDepthImageByteArr()
        {
            byte[] result = new byte[this.depthImageUshortArr.Length * sizeof(ushort)];
            Buffer.BlockCopy(this.depthImageUshortArr, 0, result, 0, result.Length);

            return result;
        }

        private unsafe byte[] GetIRImageByteArr()
        {
            return this.irImageByteArr;
            //byte[] result = new byte[this.irImageByteArr.Length * sizeof(ushort)];
            //Buffer.BlockCopy(this.irImageByteArr, 0, result, 0, result.Length);
            //
            //Console.WriteLine("IR frame converted");
            //return result;
        }

        private byte [] Serialize(object objToSerialize, CameraDataType type)
        {
            byte[] result = null;

            switch(type)
            {
                case CameraDataType.Color:
                    result = this.GetColorImageByteArr();
                    break;

                case CameraDataType.Depth:
                    result = this.GetDepthImageByteArr();
                    break;

                case CameraDataType.IR:
                    result = this.GetIRImageByteArr();
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
