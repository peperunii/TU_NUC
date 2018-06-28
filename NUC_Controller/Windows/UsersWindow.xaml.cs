using DB_Initialization;
using NUC_Controller.DB;
using NUC_Controller.Users;
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
using System.Windows.Shapes;

namespace NUC_Controller.Windows
{
    /// <summary>
    /// Interaction logic for UsersWindow.xaml
    /// </summary>
    public partial class UsersWindow : Window
    {
        public bool windowResult;
        private WindowAction windowAction;
        private User user;
        private bool isPasswordChangedOnEdit = false;
        private bool isPasswordLoaded = false;

        public UsersWindow(WindowAction action, User user = null)
        {
            this.windowAction = action;
            this.user = user;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var userTypes = new List<string>();
            foreach(UserType type in Enum.GetValues(typeof(UserType)))
            {
                userTypes.Add(type.ToString());
            }

            this.comboUserType.ItemsSource = userTypes;

            if (this.windowAction == WindowAction.Create)
            {
                this.Title = "Create User";
            }
            else
            {
                this.Title = "Edit User: " + this.user.Username;

                this.textUsername.IsEnabled = false;
                this.textUsername.Text = this.user.Username;
                this.textPassword.Password = this.user.Password;
                this.textRepeatPassword.Password = this.user.Password;

                this.comboUserType.SelectedItem = this.user.Type.ToString();

                this.isPasswordLoaded = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                this.buttonExit_Click(null, null);
            }
            else if(e.Key == Key.Enter)
            {
                this.buttonSave_Click(null, null);
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.ValidateSettings())
            {
                switch (this.windowAction)
                {
                    case WindowAction.Create:
                        var passhash = Crypto.GetHash(this.textPassword.Password);

                        var columnNamesString = DBUtils.GetColumnNamesAsString(TableName.users);
                        var valuesString = DBUtils.GetValuesString(new List<object>()
                        {
                            "NEXTVAL(pg_get_serial_sequence('users', 'id'))",
                            this.textUsername.Text,
                            passhash,
                            this.comboUserType.SelectedItem as string
                        });

                        /*DB Request for Insert*/
                        Globals.Database.QueryExecute(
                            DatabaseQueries.InsertIntoTable,
                            new List<string>()
                                { TableName.users.ToString(), columnNamesString, valuesString });

                        /*Add the new user to the Container*/
                        UsersContainer.AddUser(
                            new User(
                                this.textUsername.Text, 
                                this.textPassword.Password, 
                                (UserType)Enum.Parse(typeof(UserType), 
                                this.comboUserType.SelectedItem as string)));
                        break;

                    case WindowAction.Edit:
                        if (this.isPasswordChangedOnEdit)
                        {
                            this.user.Password = this.textPassword.Password;
                            this.user.Type = (UserType)Enum.Parse(typeof(UserType), this.comboUserType.SelectedItem as string);

                            /*DB Request for Update*/
                            var passwordHash = Crypto.GetHash(this.textPassword.Password);
                            Globals.Database.QueryExecute(
                                DatabaseQueries.UpdateRowInTable,
                                new List<string>()
                                    { TableName.users.ToString(), "password='" + passwordHash + "'" + ", usertype= '" + this.user.Type.ToString() + "'", "username='" + this.user.Username + "'"  });
                        }
                        else
                        {
                            this.user.Type = (UserType)Enum.Parse(typeof(UserType), this.comboUserType.SelectedItem as string);
                            Globals.Database.QueryExecute(
                                DatabaseQueries.UpdateRowInTable,
                                new List<string>()
                                    { TableName.users.ToString(), "usertype= '" + this.user.Type.ToString() + "'", "username='" + this.user.Username + "'"  });

                        }
                        break;
                }

                this.windowResult = true;
                this.Close();
            }
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            this.windowResult = false;
            this.Close();
        }

        private bool ValidateSettings()
        {
            var result = true;

            if (this.textUsername.Text == string.Empty ||
                this.textPassword.Password != this.textRepeatPassword.Password ||
                comboUserType.SelectedIndex == -1)
            {
                MessageBox.Show("Incorrect Settings. Can't save User !", "Error");
                result = false;
            }

            return result;
        }

        private void textPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(this.isPasswordLoaded)
                this.isPasswordChangedOnEdit = true;
        }
    }
}
