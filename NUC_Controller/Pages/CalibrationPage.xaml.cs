using NUC_Controller.NetworkWorker;
using System.Windows.Controls;
using NetworkLib.Events;
using System;
using System.Windows;
using Network.Devices;
using Network.Messages;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using Network;
using System.Linq;

namespace NUC_Controller.Pages
{
    /// <summary>
    /// Interaction logic for AnomaliesPage.xaml
    /// </summary>
    /// 
    public enum ImageType
    {
        Color,
        Depth
    }

    public partial class CalibrationPage : Page
    {
        private Dictionary<DeviceID, Dictionary<ImageType, Emgu.CV.IImage>> dictNUCCalibration;
        
        public CalibrationPage()
        {
            this.dictNUCCalibration = new Dictionary<DeviceID, Dictionary<ImageType, IImage>>();
            Worker.NewCalibrationArrived += this.Worker_NewCalibrationFramesArrived;

            InitializeComponent();
        }

        private Image<Gray, byte> ConvertDepthMessageToImage(MessageDepthFrame message)
        {
            var data = message.info as byte[];

            var width = message.Width;
            var height = message.Height;

            var image = new Image<Gray, byte>(width, height);
            var imgData = image.Data;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    imgData[i, j, 0] = data[i * width + j];
                }
            }

            return image;
        }

        private Image<Bgr, byte> ConvertColorMessageToImage(MessageColorFrame message)
        {
            var data = message.info as byte[];

            var width = message.Width;
            var height = message.Height;

            var image = new Image<Bgr, byte>(width, height);
            var imgData = image.Data;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    imgData[i, j, 0] = data[3 * (i * width + j)];
                    imgData[i, j, 1] = data[3 * (i * width + j) + 1];
                    imgData[i, j, 2] = data[3 * (i * width + j) + 2];
                }
            }

            return image;
        }

        private void Worker_NewCalibrationFramesArrived(object source, NewCalibrationArrivedEventArgs e)
        {
            var calibrationMessage = e.messageCalibration;
            var deviceID = calibrationMessage.deviceId;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    var colorImage = this.ConvertColorMessageToImage(calibrationMessage.colorFrame);
                    var depthImage = this.ConvertDepthMessageToImage(calibrationMessage.depthFrame);

                    this.dictNUCCalibration.Add(deviceID, new Dictionary<ImageType, IImage>());
                    this.dictNUCCalibration[deviceID].Add(ImageType.Color, colorImage);
                    this.dictNUCCalibration[deviceID].Add(ImageType.Depth, depthImage);

                    this.CalibrationCheck();
                }
                catch (Exception) { }
            }));
        }

        private void CalibrationCheck()
        {
            //If a calibration message was received from all connected devices
            if(this.dictNUCCalibration.Count == Worker.GetConnectedDevices().Count)
            {
                //TODO: Perform frame processing and calibration here
                var colorFrames = from t in this.dictNUCCalibration
                                  from a in t.Value
                                  where a.Key == ImageType.Color
                                  select a.Value;

                var depthFrames = from t in this.dictNUCCalibration
                                  from a in t.Value
                                  where a.Key == ImageType.Depth
                                  select a.Value;
            }
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Ask for Calibration
            foreach (var device in Worker.GetConnectedDevices())
            {
                NetworkSettings.tcpClient.Send(new MessageCalibrationRequest(device.deviceID));
            }
        }
    }
}
