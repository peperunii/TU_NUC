using Emgu.CV.Structure;
using Network;
using Network.Devices;
using Network.Messages;
using NUC_Controller.NetworkWorker;
using NUC_Controller.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Emgu.CV;
using System.Runtime.InteropServices;

namespace NUC_Controller.Pages
{
    /// <summary>
    /// Interaction logic for BodiesPage.xaml
    /// </summary>
    public partial class IRsPage : Page
    {
        private static List<NUC> connectedDevices = null;
        private static Size imagesize = new Size(512, 424);


        public IRsPage()
        {
            Worker.NewIRArrived += this.Worker_NewIRArrived;
            
            InitializeComponent();
        }

        private void Worker_NewIRArrived(object source, NetworkLib.Events.NewIRArrivedEventArgs e)
        {
            var irMessage = e.Message;
            var deviceID = irMessage.deviceID;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var image = this.GetImageChildOfTab(deviceID);
                if (image != null)
                {
                    image.Source = this.ToBitmapSource(this.ConvertMessageToImage(irMessage));
                }
            }));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            connectedDevices = Worker.GetConnectedDevices();

            if (connectedDevices != null)
            {
                var sortedDevices = connectedDevices.OrderBy(o => o.deviceID).ToList();
                var lastSelectedIndex = this.tabDevicesList.SelectedIndex;

                this.tabDevicesList.Items.Clear();
                foreach (var device in sortedDevices)
                {
                    if (device.deviceID != DeviceID.TU_SERVER)
                        this.TabCreator(device);
                }

                this.tabDevicesList.SelectedIndex = lastSelectedIndex != -1 ? lastSelectedIndex : connectedDevices.Count > 0 ? 0 : -1;
            }
            else
            {
                var textBox = new TextBox();
                textBox.Text = "Not connected to Server";
                this.tabDevicesList.Items.Add(textBox);
                this.tabDevicesList.SelectedIndex = 0;
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }

        private Image GetImageChildOfTab(DeviceID deviceID)
        {
            var tab = (from t in this.tabDevicesList.Items.Cast<TabItem>()
                       where (t.Header as string).Contains(deviceID.ToString())
                       select t).FirstOrDefault();
            if (tab != null)
            {
                var datagrid = tab.Content as Grid;
                return datagrid.Children[1] as Image;
            }
            else
            {
                return null;
            }
        }

        private Image<Bgr, ushort> ConvertMessageToImage(MessageIRFrame message)
        {
            var data = message.info as byte[];

            var width = (int)imagesize.Width;
            var height = (int)imagesize.Height;

            var image = new Image<Bgr, ushort>(width, height);
            var imgData = image.Data;

            ushort[] result = new ushort[data.Length /2];
            Buffer.BlockCopy(data, 0, result, 0, result.Length);

            for(int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    imgData[i, j, 0] = result[i * width + j];
                }
            }

            return image;
        }

        private void TabCreator(NUC device)
        {
            /*Do not visualize configuration of controller*/
            if (device.deviceID == DeviceID.Controller) return;

            var tab = new TabItem();
            tab.Header = device.deviceID;
            this.tabDevicesList.Items.Add(tab);

            var image = new Image();
            image.Width = imagesize.Width;
            image.Height = imagesize.Height;

            var datagrid = new Grid();
            datagrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            datagrid.RowDefinitions.Add(new RowDefinition());

            var dockPanel = new DockPanel();

            var textblockSocket = new TextBlock();
            textblockSocket.Margin = new Thickness(5);
            textblockSocket.Height = 30;
            textblockSocket.Text = device.ip.ToString();

            var dockPanelButtons = new DockPanel();
            dockPanelButtons.HorizontalAlignment = HorizontalAlignment.Right;
            dockPanelButtons.VerticalAlignment = VerticalAlignment.Center;

            var buttonCameraStart = new Button();
            buttonCameraStart.Name = "buttonCamera_" + device.deviceID;
            buttonCameraStart.Click += this.ButtonCameraStart_Click;
            buttonCameraStart.Width = 80;
            buttonCameraStart.Height = 24;
            buttonCameraStart.Content = "Start camera";
            buttonCameraStart.IsEnabled = !device.isCameraStarted;
            buttonCameraStart.HorizontalAlignment = HorizontalAlignment.Right;

            var buttonCameraStop = new Button();
            buttonCameraStop.Name = "buttonCamera_" + device.deviceID;
            buttonCameraStop.Click += this.ButtonCameraStop_Click;
            buttonCameraStop.Width = 80;
            buttonCameraStop.Height = 24;
            buttonCameraStop.Content = "Stop camera";
            buttonCameraStop.IsEnabled = device.isCameraStarted;
            buttonCameraStop.HorizontalAlignment = HorizontalAlignment.Right;
            buttonCameraStop.Margin = new Thickness(5, 0, 5, 0);

            dockPanelButtons.Children.Add(buttonCameraStart);
            dockPanelButtons.Children.Add(buttonCameraStop);

            dockPanel.Children.Add(textblockSocket);
            dockPanel.Children.Add(dockPanelButtons);

            //var textBoxConfig = new TextBox();
            //textBoxConfig.Text = device.config;

            Grid.SetRow(dockPanel, 0);
            Grid.SetRow(image, 1);

            datagrid.Children.Add(dockPanel);
            datagrid.Children.Add(image);
            tab.Content = datagrid;
        }


        private void ButtonCameraStart_Click(object sender, RoutedEventArgs e)
        {
            var indexOfSeparator = (sender as Button).Name.IndexOf('_');
            var deviceID_string = (sender as Button).Name.Substring(indexOfSeparator + 1);

            var deviceID = (DeviceID)Enum.Parse(typeof(DeviceID), deviceID_string);
            new Notification(NotificationType.Info, "Starting camera on device: " + deviceID);

            var device = (from t in connectedDevices
                          where t.deviceID == deviceID
                          select t).FirstOrDefault();
            device.isCameraStarted = true;

            NetworkSettings.tcpClient.Send(new MessageCameraStart(deviceID));

            if (!device.isIRStreamEnabled)
            {
                NetworkSettings.tcpClient.Send(new MessageIRFrameRequest(deviceID));
                device.isIRStreamEnabled = true;
            }
            (sender as Button).IsEnabled = false;
            this.EnableChildButton(1);
        }

        private void ButtonCameraStop_Click(object sender, RoutedEventArgs e)
        {
            var indexOfSeparator = (sender as Button).Name.IndexOf('_');
            var deviceID_string = (sender as Button).Name.Substring(indexOfSeparator + 1);

            var deviceID = (DeviceID)Enum.Parse(typeof(DeviceID), deviceID_string);
            new Notification(NotificationType.Info, "Stopping camera on device: " + deviceID);

            var device = (from t in connectedDevices
                          where t.deviceID == deviceID
                          select t).FirstOrDefault();
            device.isCameraStarted = false;

            NetworkSettings.tcpClient.Send(new MessageCameraStop(deviceID));
            (sender as Button).IsEnabled = false;
            this.EnableChildButton(0);
        }

        private void buttonRefreshDevices_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.buttonRefreshDevices.IsEnabled = false;

                NetworkSettings.tcpClient.Send(new MessageGetConnectedClients());


                this.Page_Loaded(null, null);
                this.buttonRefreshDevices.IsEnabled = true;
            }
            catch (Exception)
            {
                this.buttonRefreshDevices.IsEnabled = true;
            }
        }

        private void EnableChildButton(int childIndex)
        {
            try
            {
                var tab = this.tabDevicesList.SelectedItem as TabItem;
                if (tab != null)
                {
                    var datagrid = tab.Content as Grid;
                    var button = (((datagrid.Children[0] as DockPanel).Children[1] as DockPanel).Children[childIndex] as Button);

                    button.IsEnabled = true;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
