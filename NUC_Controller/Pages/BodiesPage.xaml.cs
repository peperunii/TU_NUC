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
    public partial class BodiesPage : Page
    {
        private static List<NUC> connectedDevices = null;
        private List<Tuple<JointType, JointType>> bones;
        private static Size imagesize = new Size(400, 400);

        public BodiesPage()
        {
            Worker.NewBodyArrived += this.Worker_NewBodyArrived;

            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            InitializeComponent();
        }

        private void Worker_NewBodyArrived(object source, NetworkLib.Events.NewBodyArrivedEventArgs e)
        {
            var bodies = e.BodiesList;
            var deviceID = e.DeviceID;

            var image = this.GetImageChildOfTab(deviceID);
            if (image != null)
            {
                image.Source = this.ToBitmapSource(this.ConvertBodiesToImage(bodies));
            }
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

        private Image<Bgr, Byte> ConvertBodiesToImage(List<Skeleton> bodies)
        {
            var width = (int)imagesize.Width;
            var height = (int)imagesize.Height;

            var image = new Image<Bgr, Byte>(width, height);
            var imgData = image.Data;

            var random = new Random();
            foreach (var body in bodies)
            {
                var b = random.Next(0, 255);
                var g = random.Next(0, 255);
                var r = random.Next(0, 255);

                var bgr = new Bgr(b, g, r);

                foreach (var joint in body.Joints)
                {
                    var point = new System.Drawing.PointF(200 * (joint.Position.X + 1), 200 * (joint.Position.Y + 1));
                    image.Draw(new CircleF(point, 3), bgr, 2);
                }

                foreach (var bone in this.bones)
                {
                    var joint0 = (from t in body.Joints
                                  where t.JointType == bone.Item1
                                  select t).FirstOrDefault();

                    var joint1 = (from t in body.Joints
                                  where t.JointType == bone.Item2
                                  select t).FirstOrDefault();

                    // If we can't find either of these joints, exit
                    if (joint0.TrackingState == TrackingState.NotTracked ||
                        joint1.TrackingState == TrackingState.NotTracked)
                    {
                        continue;
                    }

                    var p1 = new System.Drawing.Point((int)(200 * (joint0.Position.X + 1)), (int)(200 * (joint0.Position.Y + 1)));
                    var p2 = new System.Drawing.Point((int)(200 * (joint1.Position.X + 1)), (int)(200 * (joint1.Position.Y + 1)));

                    // We assume all drawn bones are inferred unless BOTH joints are tracked
                    if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
                    {
                        
                        image.Draw(new LineSegment2D(p1, p2), bgr, 3);
                    }
                    else
                    {
                        image.Draw(new LineSegment2D(p1, p2), bgr, 1);
                    }
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
            buttonCameraStart.HorizontalAlignment = HorizontalAlignment.Right;

            var buttonCameraStop = new Button();
            buttonCameraStop.Name = "buttonCamera_" + device.deviceID;
            buttonCameraStop.Click += this.ButtonCameraStop_Click;
            buttonCameraStop.Width = 80;
            buttonCameraStop.Height = 24;
            buttonCameraStop.Content = "Stop camera";
            buttonCameraStop.IsEnabled = false;
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

            NetworkSettings.tcpClient.Send(new MessageCameraStart(deviceID));
            NetworkSettings.tcpClient.Send(new MessageSkeletonRequest(deviceID));

            (sender as Button).IsEnabled = false;
            this.EnableChildButton(1);
        }

        private void ButtonCameraStop_Click(object sender, RoutedEventArgs e)
        {
            var indexOfSeparator = (sender as Button).Name.IndexOf('_');
            var deviceID_string = (sender as Button).Name.Substring(indexOfSeparator + 1);

            var deviceID = (DeviceID)Enum.Parse(typeof(DeviceID), deviceID_string);
            new Notification(NotificationType.Info, "Stopping camera on device: " + deviceID);

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
            catch(Exception)
            {
            }
        }
    }
}
