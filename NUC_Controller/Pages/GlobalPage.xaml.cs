using Network.Devices;
using Network.Messages;
using NUC_Controller.Windows;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace NUC_Controller.Pages
{
    /// <summary>
    /// Interaction logic for GlobalPage.xaml
    /// </summary>
    /// 

    public class GlobalSettigns
    {
        public bool CameraStart = false;
        public bool allStreamsEnabled = false;
        public bool colorStreamEnabled = false;
        public bool depthStreamEnabled = false;
        public bool irStreamEnabled = false;
        public bool bodyStreamEnabled = false;

        public GlobalSettigns()
        {

        }
    }

    public partial class GlobalPage : Page
    {
        private List<NUC> connectedDevices;

        private static GlobalSettigns settings = new GlobalSettigns();

        public GlobalPage()
        {
            this.connectedDevices = new List<NUC>();
            InitializeComponent();
        }

        private void buttonRestartApps_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectedDevices.Count > 0)
            {
                foreach (var connectedDevice in this.connectedDevices)
                {
                    if (connectedDevice.deviceID != Network.DeviceID.TU_SERVER)
                        NetworkWorker.Worker.SendMessage(new MessageRestartClientApp(connectedDevice.deviceID));
                }

                var msgWindow = new MessageViewer("Warning", "Restart also the Server app?");
                msgWindow.ShowDialog();

                if (msgWindow.result == MessageBoxResult.Yes)
                {
                    NetworkWorker.Worker.SendMessage(new MessageRestartServerApp());
                }
            }
        }

        private void buttonRestartDevices_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectedDevices.Count > 0)
            {
                foreach (var connectedDevice in this.connectedDevices)
                {
                    if (connectedDevice.deviceID != Network.DeviceID.TU_SERVER)
                        NetworkWorker.Worker.SendMessage(new MessageRestartClientDevice(connectedDevice.deviceID));
                }

                var msgWindow = new MessageViewer("Warning", "Restart also the Server device?");
                msgWindow.ShowDialog();

                if (msgWindow.result == MessageBoxResult.Yes)
                {
                    NetworkWorker.Worker.SendMessage(new MessageRestartServerDevice());
                }
            }
        }

        private void buttonShutdownDevices_Click(object sender, RoutedEventArgs e)
        {
            if (this.connectedDevices.Count > 0)
            {
                foreach (var connectedDevice in this.connectedDevices)
                {
                    if (connectedDevice.deviceID != Network.DeviceID.TU_SERVER)
                        NetworkWorker.Worker.SendMessage(new MessageShutdownDevice(connectedDevice.deviceID));
                }

                var msgWindow = new MessageViewer("Warning", "Shutdown also the Server device?");
                msgWindow.ShowDialog();

                if (msgWindow.result == MessageBoxResult.Yes)
                {
                    NetworkWorker.Worker.SendMessage(new MessageShutdownDevice(Network.DeviceID.TU_SERVER));
                }
            }
        }

        private void buttonCamerasStartStop_Click(object sender, RoutedEventArgs e)
        {
            settings.CameraStart = !settings.CameraStart;

            var button = (sender as Button);
            if (button.Content as string == "Start")
            {
                button.Content = "Stop";

                if (this.connectedDevices.Count > 0)
                {
                    foreach (var connectedDevice in this.connectedDevices)
                    {
                        if (!connectedDevice.isCameraStarted)
                        {
                            if (connectedDevice.deviceID != Network.DeviceID.TU_SERVER)
                            {
                                NetworkWorker.Worker.SendMessage(new MessageCameraStart(connectedDevice.deviceID));
                                connectedDevice.isCameraStarted = !connectedDevice.isCameraStarted;
                            }
                        }
                    }
                }
            }
            else
            {
                button.Content = "Start";

                if (this.connectedDevices.Count > 0)
                {
                    foreach (var connectedDevice in this.connectedDevices)
                    {
                        if (connectedDevice.isCameraStarted)
                        {
                            if (connectedDevice.deviceID != Network.DeviceID.TU_SERVER)
                            {
                                NetworkWorker.Worker.SendMessage(new MessageCameraStop(connectedDevice.deviceID));
                                connectedDevice.isCameraStarted = !connectedDevice.isCameraStarted;
                            }
                        }
                    }
                }
            }
        }

        private void buttonStreamsAll_Click(object sender, RoutedEventArgs e)
        {
            settings.allStreamsEnabled = !settings.allStreamsEnabled;

            var button = (sender as Button);
            if (button.Content as string == "Enable")
            {
                button.Content = "Disable";

                if (this.connectedDevices.Count > 0)
                {
                    foreach (var connectedDevice in this.connectedDevices)
                    {
                        if (connectedDevice.deviceID != Network.DeviceID.TU_SERVER)
                        {
                            if (!connectedDevice.isColorStreamEnabled)
                            {
                                NetworkWorker.Worker.SendMessage(new MessageColorFrameRequest(connectedDevice.deviceID));
                                connectedDevice.isColorStreamEnabled = !connectedDevice.isColorStreamEnabled;
                            }
                            if (!connectedDevice.isDepthStreamEnabled)
                            {
                                NetworkWorker.Worker.SendMessage(new MessageDepthFrameRequest(connectedDevice.deviceID));
                                connectedDevice.isDepthStreamEnabled = !connectedDevice.isDepthStreamEnabled;
                            }
                            if (!connectedDevice.isIRStreamEnabled)
                            {
                                NetworkWorker.Worker.SendMessage(new MessageIRFrameRequest(connectedDevice.deviceID));
                                connectedDevice.isIRStreamEnabled = !connectedDevice.isIRStreamEnabled;
                            }
                            if (!connectedDevice.isBodyStreamEnabled)
                            {
                                NetworkWorker.Worker.SendMessage(new MessageSkeletonRequest(connectedDevice.deviceID));
                                connectedDevice.isBodyStreamEnabled = !connectedDevice.isBodyStreamEnabled;
                            }
                        }
                    }
                }
            }
            else
            {
                button.Content = "Enable";

                if (this.connectedDevices.Count > 0)
                {
                    foreach (var connectedDevice in this.connectedDevices)
                    {
                        if (connectedDevice.deviceID != Network.DeviceID.TU_SERVER)
                        {
                            if (connectedDevice.isColorStreamEnabled)
                            {
                                NetworkWorker.Worker.SendMessage(new MessageColorFrameRequest(connectedDevice.deviceID));
                                connectedDevice.isColorStreamEnabled = !connectedDevice.isColorStreamEnabled;
                            }
                            if (connectedDevice.isDepthStreamEnabled)
                            {
                                NetworkWorker.Worker.SendMessage(new MessageDepthFrameRequest(connectedDevice.deviceID));
                                connectedDevice.isDepthStreamEnabled = !connectedDevice.isDepthStreamEnabled;
                            }
                            if (connectedDevice.isIRStreamEnabled)
                            {
                                NetworkWorker.Worker.SendMessage(new MessageIRFrameRequest(connectedDevice.deviceID));
                                connectedDevice.isIRStreamEnabled = !connectedDevice.isIRStreamEnabled;
                            }
                            if (connectedDevice.isBodyStreamEnabled)
                            {
                                NetworkWorker.Worker.SendMessage(new MessageSkeletonRequest(connectedDevice.deviceID));
                                connectedDevice.isBodyStreamEnabled = !connectedDevice.isBodyStreamEnabled;
                            }
                        }
                    }
                }
            }
        }

        private void buttonStreamsColor_Click(object sender, RoutedEventArgs e)
        {
            settings.colorStreamEnabled = !settings.colorStreamEnabled;

            var button = (sender as Button);
            if (button.Content as string == "Enable")
            {
                button.Content = "Disable";
            }
            else
            {
                button.Content = "Enable";
            }

            foreach (var connectedDevice in this.connectedDevices)
            {
                if (connectedDevice.deviceID != Network.DeviceID.TU_SERVER)
                {
                    connectedDevice.isColorStreamEnabled = !connectedDevice.isColorStreamEnabled;
                    NetworkWorker.Worker.SendMessage(new MessageColorFrameRequest(connectedDevice.deviceID));
                }
            }
        }

        private void buttonStreamDepth_Click(object sender, RoutedEventArgs e)
        {
            settings.depthStreamEnabled = !settings.depthStreamEnabled;

            var button = (sender as Button);
            if (button.Content as string == "Enable")
            {
                button.Content = "Disable";
            }
            else
            {
                button.Content = "Enable";
            }

            foreach (var connectedDevice in this.connectedDevices)
            {
                if (connectedDevice.deviceID != Network.DeviceID.TU_SERVER)
                {
                    connectedDevice.isDepthStreamEnabled = !connectedDevice.isDepthStreamEnabled;
                    NetworkWorker.Worker.SendMessage(new MessageDepthFrameRequest(connectedDevice.deviceID));
                }
            }
        }

        private void buttonStreamIR_Click(object sender, RoutedEventArgs e)
        {
            settings.irStreamEnabled = !settings.irStreamEnabled;

            var button = (sender as Button);
            if (button.Content as string == "Enable")
            {
                button.Content = "Disable";
            }
            else
            {
                button.Content = "Enable";
            }

            foreach (var connectedDevice in this.connectedDevices)
            {
                if (connectedDevice.deviceID != Network.DeviceID.TU_SERVER)
                {
                    connectedDevice.isIRStreamEnabled = !connectedDevice.isIRStreamEnabled;
                    NetworkWorker.Worker.SendMessage(new MessageIRFrameRequest(connectedDevice.deviceID));
                }
            }
        }

        private void buttonStreamBody_Click(object sender, RoutedEventArgs e)
        {
            settings.bodyStreamEnabled = !settings.bodyStreamEnabled;

            var button = (sender as Button);
            if (button.Content as string == "Enable")
            {
                button.Content = "Disable";
            }
            else
            {
                button.Content = "Enable";
            }

            foreach (var connectedDevice in this.connectedDevices)
            {
                if (connectedDevice.deviceID != Network.DeviceID.TU_SERVER)
                {
                    connectedDevice.isBodyStreamEnabled = !connectedDevice.isBodyStreamEnabled;
                    NetworkWorker.Worker.SendMessage(new MessageSkeletonRequest(connectedDevice.deviceID));
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var devices = NetworkWorker.Worker.GetConnectedDevices();
            if(devices != null)
            {
                this.connectedDevices = devices;
                this.gridDevicesSettings.IsEnabled = true;

                this.buttonCamerasStartStop.Content = settings.CameraStart ? "Stop" : "Start";
                this.buttonStreamsAll.Content = settings.allStreamsEnabled ? "Disable" : "Enable";
                this.buttonStreamsColor.Content = settings.colorStreamEnabled ? "Disable" : "Enable";
                this.buttonStreamDepth.Content = settings.depthStreamEnabled ? "Disable" : "Enable";
                this.buttonStreamIR.Content = settings.irStreamEnabled ? "Disable" : "Enable";
                this.buttonStreamBody.Content = settings.bodyStreamEnabled ? "Disable" : "Enable";
            }
            else
            {
                this.gridDevicesSettings.IsEnabled = false;
            }
        }

        private void buttonRefresDevices_Click(object sender, RoutedEventArgs e)
        {
            NetworkWorker.Worker.SendMessage(new MessageGetConnectedClients());
            Thread.Sleep(1);
            this.Page_Loaded(null, null);
        }
    }
}
