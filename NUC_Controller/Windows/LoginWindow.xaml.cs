using DB_Initialization;
using Network.Logger;
using NUC_Controller.DB;
using NUC_Controller.Users;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace NUC_Controller.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private bool enabledConnect = true;
        private bool isConnectedOnce = false;
        public bool resultLogin = false;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            if (this.enabledConnect)
            {
                var username = this.textUsername.Text;
                var password = this.textPassword.Password;

                var passwordHash = Crypto.GetHash(password);
                var selectResult = true;

                if (!this.isConnectedOnce)
                {
                    //Connect to DB and check for existing Record
                    var database = new Database();
                    selectResult =
                        database.QuerySelect(DatabaseQueries.SelectFromTable, new List<string>() { TableName.users.ToString() });
                }

                if (!selectResult)
                {
                    MessageBox.Show("Error connecting to DB");
                }
                else
                {
                    this.isConnectedOnce = true;
                    var isFound = false;

                    foreach (var user in Tables.dbInfo[TableName.users])
                    {
                        if (user.items["username"] as string == username &&
                            user.items["password"] as string == passwordHash)
                        {
                            Globals.loggedInUser =
                                new User(
                                    username,
                                    password,
                                    (UserType)Enum.Parse(typeof(UserType), user.items["usertype"] as string));
                            isFound = true;
                            break;
                        }
                    }

                    if (isFound)
                    {
                        this.resultLogin = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Incorrect credentials !");
                        LogManager.LogMessage(LogType.UserAction, LogLevel.Errors, string.Format("Unsuccessful Login with user: ''{0}''. Password: ''{1}''", username, password));
                    }
                }
            }
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                this.buttonConnect_Click(null, null);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textUsername.Focus();

            if(!Globals.Database.IsConnectiontoDBEstablieshed())
            {
                this.enabledConnect = false;
                this.buttonConnect.IsEnabled = false;
            }
        }

        private void buttonConnect_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.enabledConnect)
            {
                var position = e.GetPosition((IInputElement)sender);

                tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;

                tooltip.HorizontalOffset = position.X + 10;
                tooltip.VerticalOffset = position.Y + 10;
            }
            else
            {
                tooltip.Visibility = Visibility.Collapsed;
            }
        }
    }
}
