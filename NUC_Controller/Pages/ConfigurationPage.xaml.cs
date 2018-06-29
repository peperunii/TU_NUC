using Network;
using Network.Devices;
using Network.Messages;
using NUC_Controller.DB;
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

namespace NUC_Controller.Pages
{
    /// <summary>
    /// Interaction logic for ConfigurationPage.xaml
    /// </summary>
    public partial class ConfigurationPage : Page
    {
        private static List<NUC> connectedDevices = null;
        public ConfigurationPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.buttonRefresh.ToolTip = string.Empty;
            connectedDevices = Worker.GetConnectedDevices();

            if (connectedDevices != null)
            {
                var lastSelectedIndex = this.tabDevicesList.SelectedIndex;

                this.tabDevicesList.Items.Clear();
                foreach (var device in connectedDevices)
                {
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

        private void TabCreator(NUC device)
        {
            var tab = new TabItem();
            tab.Header = device.deviceID;
            this.tabDevicesList.Items.Add(tab);
            
            var datagrid = new Grid();
            datagrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            datagrid.RowDefinitions.Add(new RowDefinition());

            var dockPanel = new DockPanel();
            
            var textblockSocket = new TextBlock();
            textblockSocket.Margin = new Thickness(5);
            textblockSocket.Height = 30;
            textblockSocket.Text = device.ip.ToString();

            var buttonRestart = new Button();
            buttonRestart.Name = "buttonRestart_" + device.deviceID;
            buttonRestart.Click += this.ButtonRestart_Click;
            buttonRestart.Width = 80;
            buttonRestart.Height = 24;
            buttonRestart.Content = "Restart App";
            buttonRestart.HorizontalAlignment = HorizontalAlignment.Right;

            dockPanel.Children.Add(textblockSocket);
            dockPanel.Children.Add(buttonRestart);

            var textBoxConfig = new TextBox();
            textBoxConfig.Text = device.config;

            Grid.SetRow(dockPanel, 0);
            Grid.SetRow(textBoxConfig, 1);

            datagrid.Children.Add(dockPanel);
            datagrid.Children.Add(textBoxConfig);
            tab.Content = datagrid;
        }

        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure?", "Warning", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                var indexOfSeparator = (sender as Button).Name.IndexOf('_');
                var deviceID_string = (sender as Button).Name.Substring(indexOfSeparator + 1);

                var deviceID = (DeviceID)Enum.Parse(typeof(DeviceID), deviceID_string);
                new Notification(NotificationType.Info, "Restarting " + deviceID);

                if (deviceID_string.ToLower().Contains("server"))
                {
                    NetworkSettings.tcpClient.Send(new MessageRestartServerApp());
                }
                else
                {
                    NetworkSettings.tcpClient.Send(new MessageRestartClientApp(deviceID));
                }
            }
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            var currentConnectedDevices = Worker.GetConnectedDevices();

            if (currentConnectedDevices != null)
            {
                this.buttonRefresh.IsEnabled = false;

                for (int i = 0; i < currentConnectedDevices.Count; i++)
                {
                    var deviceID = currentConnectedDevices[i].deviceID;

                    NetworkSettings.tcpClient.Send(new MessageGetConfigurationPerClient(deviceID));

                }
                this.Page_Loaded(null, null);

                this.buttonRefresh.IsEnabled = true;
            }
            else
            {
                this.buttonRefresh.ToolTip = "Not connected to Server";
            }
        }
    }
}
