using Network;
using Network.Logger;
using Network.Messages;
using NUC_Controller.DB;
using NUC_Controller.NetworkWorker;
using NUC_Controller.Notifications;
using NUC_Controller.Users;
using NUC_Controller.Windows;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NUC_Controller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private bool isWindowLoaded = false;
        private TextBox lastClickedNav;
        private IDictionary<ActionType, IEnumerable<FrameworkElement>> mapPermissionControls;
        public Dictionary<int, TextBox> tabOrder;
        public UInt16 lastSelectedTab;
        private UInt16 initialSelectedTab;
        private Dictionary<TextBox, Uri> navigationsLinks;

        private UInt16 fontSizeInitialValueOnLostFocus = 12;
        private UInt16 fontSizeIncreaseValueOnFocus = 14;
        private UInt16 borderSizeIncreaseValueOnFocus = 2;
        
        #region Notifications
        public void NotificationSetter(string str1, string str2)
        {
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (this.staticTextNotifications != null)
                {
                    this.staticTextNotifications.Text = str1;
                    this.textNotifications.Text = str2;
                }
            }));
        }

        public void NotificationChecker()
        {
            var lastNotification = NotificationsContainer.GetLastNotification();
            if (lastNotification != null)
            {
                if ((DateTime.Now - lastNotification.startTime).Seconds > MonitoringConfiguration.MAX_TIME_FOR_NOTIFICATION_SEC)
                {
                    this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.staticTextNotifications.Text = string.Empty;
                        this.textNotifications.Text = string.Empty;
                    }));
                }
            }
        }
        
        public void NetworkInit()
        {
            var networkThread = new Thread(Worker.StartNetworkClient);
            networkThread.IsBackground = true;
            networkThread.Name = "NetworkInit";
            networkThread.Start();
        }
        #endregion

        #region Initializers
        private void InitializeNavLinks()
        {
            this.navigationsLinks = new Dictionary<TextBox, Uri>()
            {
                {this.navGlobal, new Uri("Pages/GlobalPage.xaml", UriKind.RelativeOrAbsolute)},
                {this.navEvents, new Uri("Pages/EventsPage.xaml", UriKind.RelativeOrAbsolute)},
                {this.navConfiguration, new Uri("Pages/ConfigurationPage.xaml", UriKind.RelativeOrAbsolute)},
                {this.navCalibration, new Uri("Pages/CalibrationPage.xaml", UriKind.RelativeOrAbsolute)},
                {this.navBodies, new Uri("Pages/BodiesPage.xaml", UriKind.RelativeOrAbsolute)},
                {this.navIRs, new Uri("Pages/IRsPage.xaml", UriKind.RelativeOrAbsolute)},
                { this.navUsers, new Uri("Pages/UsersPage.xaml", UriKind.RelativeOrAbsolute)}
            };
        }

        private void InitializePermissionsControlsMapping()
        {
            this.mapPermissionControls = new Dictionary<ActionType, IEnumerable<FrameworkElement>>()
            {
                { ActionType.readGlobals, new List<FrameworkElement>(){ this.navGlobal } },
                { ActionType.ReadEvents, new List<FrameworkElement>(){ this.navEvents } },
                { ActionType.ReadConfig, new List<FrameworkElement>(){ this.navConfiguration } },
                { ActionType.PerformCalibration, new List<FrameworkElement>(){ this.navCalibration } },
                { ActionType.ReadBodies, new List<FrameworkElement>(){ this.navBodies, this.navIRs } },
                { ActionType.ReadUsers, new List<FrameworkElement>(){ this.navUsers } }
            };
        }
        #endregion
        #region Constructor_WindowEvents
        public MainWindow()
        {
            Configuration.DeviceID = DeviceID.Controller;
            /* Setting up the Log manager */
            LogManager.SetConsole(Configuration.logInConsole);
            LogManager.SetDB(Configuration.logInDB);
            LogManager.SetFileOutput(Configuration.logFilename, Configuration.logInFile);

            var loginWindow = new LoginWindow();
            loginWindow.ShowDialog();

            this.NetworkInit();

            var windowResult = loginWindow.resultLogin;
            if (!windowResult)
            {
                Environment.Exit(0);
            }

            LogManager.LogMessage(LogType.UserAction, LogLevel.ErrWarn, string.Format("User ''{0}'' logged in.", Globals.loggedInUser.Username));

            InitializeComponent();
            this.InitializePermissionsControlsMapping();
            this.InitializeNavLinks();

            this.tabOrder = new Dictionary<int, TextBox>();
            this.TextBox_MouseDown(this.navLogo, null);

            this.tabOrder.Add(0, this.navGlobal);
            this.tabOrder.Add(1, this.navEvents);
            this.tabOrder.Add(2, this.navConfiguration);
            this.tabOrder.Add(3, this.navCalibration);
            this.tabOrder.Add(4, this.navBodies);
            this.tabOrder.Add(5, this.navIRs);
            this.tabOrder.Add(6, this.navUsers);

            this.lastSelectedTab = 1;
            this.initialSelectedTab = 1;

            /*Refresh notifications in Background- each second*/
            var t = new DispatcherTimer(
                TimeSpan.FromSeconds(1),
                DispatcherPriority.Normal,
                (s, e) => { this.NotificationChecker(); },
            this.Dispatcher);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.RefreshPermissions();
            new Notification(NotificationType.Info, "Connected");

            /*Make DB Requests*/
            Globals.Database.QuerySelect(DatabaseQueries.SelectFromTable, new List<string>() { TableName.serverevents.ToString() });
            Globals.Database.QuerySelect(DatabaseQueries.SelectFromTable, new List<string>() { TableName.calibrations.ToString() });
            Globals.Database.QuerySelect(DatabaseQueries.SelectFromTable, new List<string>() { TableName.kinectbodies.ToString() });
            Globals.Database.QuerySelect(DatabaseQueries.SelectFromTable, new List<string>() { TableName.faces.ToString() });
            Globals.Database.QuerySelect(DatabaseQueries.SelectFromTable, new List<string>() { TableName.users.ToString() });
            
            this.SetAllTextBoxesToFocusable(true);
            this.isWindowLoaded = true;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
        }
        #endregion

        private void RefreshPermissions()
        {
            foreach (var permContrPair in this.mapPermissionControls)
            {
                if (!Globals.loggedInUser.CheckIfHasAccess(permContrPair.Key))
                {
                    foreach (var control in permContrPair.Value)
                    {
                        control.IsEnabled = false;
                        control.ToolTip = "Insufficient permmissions";

                        TextBox nav = control as TextBox;
                        if (nav != null)
                        {
                            nav.Background = Brushes.Gray;
                            nav.Foreground = Brushes.Black;
                        }
                    }
                }
            }
        }

        #region Menu
        private void MenuDBRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.Database.QuerySelect(DatabaseQueries.SelectFromTableLastN, new List<string>() { TableName.serverevents.ToString(), 10.ToString() }))
            {
                new Notification(NotificationType.Info, "Last 10 Rows selected from Table \"Logs\"");
            }
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
        #endregion

        #region Mouse_KB_Events
        /*Window*/
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.isWindowLoaded)
            {
                if (e.Key == Key.LeftAlt || e.Key == Key.System)
                {
                    this.SetAllTextBoxesToFocusable(true);
                }
                if (e.Key == Key.F4)
                {
                    /*Do something on F4 key press*/
                }
                else if (e.Key == Key.F5)
                {
                    /*Do something on F5 key press*/
                }
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (this.isWindowLoaded)
            {
                if (this.lastClickedNav != null && (e.Key == Key.LeftAlt || e.Key == Key.System))
                {
                    this.SetAllTextBoxesToFocusable(true);
                    this.lastClickedNav.Focusable = true;
                }
            }
        }

        /*Controls in Uniform grid*/
        private void TextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.SelectMenuItem(false);

            var textBox = sender as TextBox;
            if (textBox != null &&
                this.navigationsLinks.ContainsKey(textBox))
            {
                this.lastClickedNav = textBox;
                this.lastClickedNav.Focusable = true;

                if (this.navGlobal == this.lastClickedNav)
                {
                    this.navGlobal.Focusable = true;
                }
                else
                {
                    this.navGlobal.Focusable = false;
                }

                if (this.navEvents == this.lastClickedNav)
                {
                    this.navEvents.Focusable = true;
                }
                else
                {
                    this.navEvents.Focusable = false;
                }

                if (this.navCalibration == this.lastClickedNav)
                {
                    this.navCalibration.Focusable = true;
                }
                else
                {
                    this.navCalibration.Focusable = false;
                }

                if (this.navCalibration == this.lastClickedNav)
                {
                    this.navCalibration.Focusable = true;
                }
                else
                {
                    this.navCalibration.Focusable = false;
                }

                if (this.navBodies == this.lastClickedNav)
                {
                    this.navBodies.Focusable = true;
                }
                else
                {
                    this.navBodies.Focusable = false;
                }

                if (this.navIRs == this.lastClickedNav)
                {
                    this.navIRs.Focusable = true;
                }
                else
                {
                    this.navIRs.Focusable = false;
                }

                if (this.navUsers == this.lastClickedNav)
                {
                    this.navUsers.Focusable = true;
                }
                else
                {
                    this.navUsers.Focusable = false;
                }

                if (this.navLogo == this.lastClickedNav)
                {
                    this.navLogo.Focusable = true;
                }
                else
                {
                    this.navLogo.Focusable = false;
                }

                this.SelectMenuItem(true);
                this.lastSelectedTab = (UInt16)GetTabNumber(textBox);
                this.initialSelectedTab = (UInt16)GetTabNumber(textBox);

                this.framePresenter.Navigate(this.navigationsLinks[textBox]);
            }
        }

        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            var TextBox = sender as TextBox;
            if (TextBox != null)
            {
                TextBox.Background = Brushes.YellowGreen;
                TextBox.Foreground = Brushes.White;
            }
        }

        /*Uniform Grid*/
        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            var TextBox = sender as TextBox;
            if (TextBox != null &&
                TextBox != this.lastClickedNav)
            {
                TextBox.Background = this.Background;
                TextBox.Foreground = Brushes.Black;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            this.initialSelectedTab = this.lastSelectedTab;
            if ((this.navEvents.IsFocused == true ||
                this.navGlobal.IsFocused == true ||
                this.navConfiguration.IsFocused == true ||
                this.navCalibration.IsFocused == true ||
                this.navBodies.IsFocused == true ||
                this.navIRs.IsFocused == true ||
                this.navUsers.IsFocused == true ||
                this.navLogo.IsFocused == true))
            {
                this.SetAllTextBoxesToFocusable(true);
                if (e.Key == Key.Left)
                {
                    var nextTabToOpen = this.lastSelectedTab - 1;
                    if (nextTabToOpen < 0)
                    {
                        nextTabToOpen = this.tabOrder.Count - 1;
                    }

                    this.lastSelectedTab = (UInt16)nextTabToOpen;

                    this.tabOrder[nextTabToOpen].Focus();
                }

                if (e.Key == Key.Right)
                {
                    var nextTabToOpen = this.lastSelectedTab + 1;
                    if (nextTabToOpen >= this.tabOrder.Count)
                    {
                        nextTabToOpen = 0;
                    }

                    this.lastSelectedTab = (UInt16)nextTabToOpen;
                    this.tabOrder[nextTabToOpen].Focus();
                }
                this.SetAllTextBoxesToFocusable(false);
                this.lastClickedNav.Focusable = true;
            }
        }
        #endregion

        #region Helpers
        private void SetAllTextBoxesToFocusable(bool focusable)
        {
            this.navEvents.Focusable = focusable;
            this.navGlobal.Focusable = focusable;
            this.navConfiguration.Focusable = focusable;
            this.navCalibration.Focusable = focusable;
            this.navBodies.Focusable = focusable;
            this.navIRs.Focusable = focusable;
            this.navUsers.Focusable = focusable;
        }

        private void SelectMenuItem(bool isSelected)
        {
            if (this.lastClickedNav != null)
            {
                if (isSelected)
                {
                    this.lastClickedNav.Background = Brushes.YellowGreen;
                    this.lastClickedNav.Foreground = Brushes.White;
                }
                else
                {
                    this.lastClickedNav.Background = this.Background;
                    this.lastClickedNav.Foreground = Brushes.Black;
                }
            }
        }

        private void Nav_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var curNav = sender as TextBox;

            foreach (var tab in this.tabOrder.Values)
            {
                tab.FontSize = this.fontSizeInitialValueOnLostFocus;
                tab.BorderThickness = new Thickness(0);
            }

            curNav.FontSize = this.fontSizeIncreaseValueOnFocus;
            curNav.BorderThickness = new Thickness(this.borderSizeIncreaseValueOnFocus);

            if (this.lastClickedNav != curNav)
            {
                this.TextBox_MouseDown(curNav, null);
            }
        }

        private int GetTabNumber(TextBox tb)
        {
            int index = 0;
            if (this.tabOrder != null)
            {
                foreach (var keyPair in this.tabOrder)
                {
                    if (keyPair.Value == tb)
                    {
                        index = keyPair.Key;
                    }
                }
            }
            return index;
        }
        #endregion

        #region Interface_Implementation
        public void OnRequestQueueEmpty()
        {
            if (this.lastClickedNav != null)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.framePresenter.Navigate(this.navigationsLinks[this.lastClickedNav]);
                }));
            }
        }
        #endregion

        private void MenuRestartAllDevices_Click(object sender, RoutedEventArgs e)
        {
            var msgWindow = new MessageViewer("Warning", "Are you sure?");
            msgWindow.ShowDialog();

            if (msgWindow.result == MessageBoxResult.Yes)
            {
                var allDevices = Worker.GetConnectedDevices();

                foreach(var device in allDevices)
                {
                    var deviceID = device.deviceID;
                    if (deviceID == DeviceID.TU_SERVER) continue;

                    NetworkSettings.tcpClient.Send(new MessageRestartClientDevice(deviceID));
                }

                /*Send server request - as the last one*/
                NetworkSettings.tcpClient.Send(new MessageRestartServerDevice());
            }
        }

        private void MenuShutDownAllDevices_Click(object sender, RoutedEventArgs e)
        {
            var msgWindow = new MessageViewer("Warning", "Are you sure?");
            msgWindow.ShowDialog();

            if (msgWindow.result == MessageBoxResult.Yes)
            {
                var allDevices = Worker.GetConnectedDevices();

                foreach (var device in allDevices)
                {
                    var deviceID = device.deviceID;
                    if (deviceID == DeviceID.TU_SERVER) continue;

                    NetworkSettings.tcpClient.Send(new MessageShutdownDevice(deviceID));
                }

                /*Send server request - as the last one*/
                NetworkSettings.tcpClient.Send(new MessageShutdownDevice(DeviceID.TU_SERVER));
            }
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            var msgWindow = new MessageViewer("Warning", "Logout!\nAre you sure?");
            msgWindow.ShowDialog();

            if(msgWindow.result == MessageBoxResult.Yes)
            {
                LogManager.LogMessage(LogType.UserAction, LogLevel.ErrWarn, string.Format("User ''{0}'' logged out.", Globals.loggedInUser.Username));
                this.Hide();
                var loginWindow = new LoginWindow();
                loginWindow.ShowDialog();

                var windowResult = loginWindow.resultLogin;
                if (!windowResult)
                {
                    Environment.Exit(0);
                }
                this.Show();
                this.Window_Loaded(null, null);
            }
        }
    }
}
