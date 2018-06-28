using NUC_Controller.DB;
using NUC_Controller.Users;
using NUC_Controller.Utils;
using NUC_Controller.ViewModels;
using NUC_Controller.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for UsersPage.xaml
    /// </summary>
    /// 
    

    public partial class UsersPage : Page
    {
        private bool Ascending { get { return this.imgUp.IsVisible; } }

        private ObservableCollection<UserViewModel> usersList;

        private void InitializeComboboxOrderBy()
        {
            this.comboOrderBy.ItemsSource =
                UserOrderByMapper.GetOrderItems();
        }

        private void Refresh()
        {
            if (this.IsLoaded)
            {
                var selectedUsers = this.listboxUsers.SelectedItems
                        .Cast<UserViewModel>()
                        .Select(x => x.Username)
                        .ToList();

                this.RefreshViewModels();

                var orderByFilter = UserOrderByMapper.GetPropertyName(
                    this.comboOrderBy.SelectedItem.ToString());

                var sorted = this.usersList
                    .AsQueryable()
                    .OrderBy(orderByFilter, this.Ascending)
                    .ToList();

                this.usersList.Clear();
                foreach (var item in sorted)
                {
                    if (selectedUsers.Contains(item.Username))
                    {
                        item.IsSelected = true;
                    }

                    this.usersList.Add(item);
                }
            }
        }

        private void RefreshViewModels()
        {
            this.usersList.Clear();
            foreach (var item in UsersContainer.GetAllUsers())
            {
                this.usersList.Add(
                    new UserViewModel(item));
            }
            this.listboxUsers.ItemsSource = this.usersList;
        }

        private void RefreshButtons()
        {
            this.buttonAdd.ToolTip = null;
            this.buttonAdd.IsEnabled = false;
            this.buttonRemove.ToolTip = null;
            this.buttonRemove.IsEnabled = false;

            if (Globals.loggedInUser.CheckIfHasAccess(ActionType.CreateUser))
            {
                this.buttonAdd.IsEnabled = true;
                this.buttonAdd.ToolTip = null;
            }
            else
            {
                this.buttonAdd.IsEnabled = false;
                this.buttonAdd.ToolTip = "Insufficient permmissions";
            }

            if (this.listboxUsers.SelectedItems.Count == 0)
            {
                this.buttonRemove.IsEnabled = false;
                this.buttonEdit.IsEnabled = false;
            }
            else if (this.listboxUsers.SelectedItems.Count == 1)
            {
                var selUser = this.listboxUsers.SelectedItem as UserViewModel;
                if (selUser.Username == Globals.loggedInUser.Username)
                {
                    this.buttonRemove.IsEnabled = false;
                    this.buttonRemove.ToolTip = "You cannot remove yourself";
                }
                else if (!Globals.loggedInUser.CheckIfHasAccess(ActionType.RemoveUser))
                {
                    this.buttonRemove.IsEnabled = false;
                    this.buttonRemove.ToolTip = "Insufficient permmissions";
                }
                else
                {
                    this.buttonRemove.IsEnabled = true;
                }

                this.buttonEdit.IsEnabled = true;
            }
            else
            {
                var selItems = this.listboxUsers.SelectedItems.Cast<UserViewModel>();
                if (selItems.Any(u => u.Username == Globals.loggedInUser.Username))
                {
                    this.buttonRemove.IsEnabled = false;
                    this.buttonRemove.ToolTip = "You cannot remove yourself";
                }
                else if (!Globals.loggedInUser.CheckIfHasAccess(ActionType.RemoveUser))
                {
                    this.buttonRemove.ToolTip = "Insufficient permmissions";
                    this.buttonRemove.IsEnabled = false;
                }
                else
                {
                    this.buttonRemove.IsEnabled = true;
                }

                this.buttonEdit.IsEnabled = false;
            }
        }

        public UsersPage()
        {
            this.usersList = new ObservableCollection<UserViewModel>();

            this.InitializeComponent();
            this.InitializeComboboxOrderBy();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (UsersContainer.GetAllUsers().Count == 0)
            {
                foreach (var user in Tables.dbInfo[TableName.users])
                {
                    UsersContainer.AddUser(user);
                }
            }

            this.Refresh();
            this.RefreshButtons();
        }

        private void PageKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                this.ButtonRemoveClick(sender, e);
            }
        }

        //buttons
        private void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            var userWindow =
                new UsersWindow(WindowAction.Create);

            userWindow.ShowDialog();

            if (userWindow.windowResult == true)
            {
                this.Refresh();
                this.buttonAdd.Focus();
            }
        }

        private void ButtonRemoveClick(object sender, RoutedEventArgs e)
        {
            if (Globals.loggedInUser.CheckIfHasAccess(ActionType.RemoveUser))
            {
                    var userNames = this.listboxUsers.SelectedItems
                        .Cast<UserViewModel>()
                        .Select(x => x.Username)
                        .ToList();

                if (userNames != null)
                {
                    MessageBoxResult dialogResult;
                    if (userNames.Count() == 1)
                    {
                        dialogResult = MessageBox.Show(
                            App.Current.MainWindow,
                            string.Format(
                                "Do you really want to remove User with Name [{0}] ?",
                                string.Join(", ", userNames)),
                            App.ResourceAssembly.GetName().Name,
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Question,
                            MessageBoxResult.OK);
                    }
                    else
                    {
                        dialogResult = MessageBox.Show(
                            App.Current.MainWindow,
                            string.Format(
                                "Do you really want to remove Users with Names [{0}] ?",
                                string.Join(", ", userNames)),
                            App.ResourceAssembly.GetName().Name,
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Question,
                            MessageBoxResult.OK);
                    }

                    if (dialogResult == MessageBoxResult.OK)
                    {
                        foreach (var name in userNames)
                        {
                            if (name == Globals.loggedInUser.Username)
                            {
                                MessageBox.Show(
                                    App.Current.MainWindow,
                                    string.Format("You cannot remove yourself from the list"),
                                    App.ResourceAssembly.GetName().Name,
                                    MessageBoxButton.OK);
                            }
                            else
                            {
                                var userToBeRemoved = UsersContainer.GetUser(name);

                                if (! (userToBeRemoved.Username == "Admin"))
                                {
                                    /*DB Request to remove user*/
                                    Globals.Database.QueryExecute(DatabaseQueries.DeleteFromTable, new List<string>() { TableName.users.ToString(), "username='" + userToBeRemoved.Username + "'" });
                                    UsersContainer.RemoveUser(name);
                                }
                                else
                                {
                                    MessageBox.Show(
                                            string.Format("You can't remove user 'Admin'."), 
                                            "Error",
                                            MessageBoxButton.OK);
                                }
                            }
                        }

                        this.Refresh();
                        this.RefreshButtons();
                    }
                }
            }
            else
            {
                var dialogResult = MessageBox.Show(
                    App.Current.MainWindow,
                    string.Format("Insufficient permmissions to remove user(s)"),
                    App.ResourceAssembly.GetName().Name,
                    MessageBoxButton.OK);
            }
        }

        private void ButtonEditClick(object sender, RoutedEventArgs e)
        {
            var selItem = this.listboxUsers.SelectedItem as UserViewModel;
            if (selItem != null)
            {
                /*You can edin yourself or other users -which are not administrators */
                if (Globals.loggedInUser.Username == selItem.Username ||
                    (Globals.loggedInUser.Type == UserType.Admin &&
                    selItem.Type != UserType.Admin))
                {
                    var windows = new UsersWindow(
                        WindowAction.Edit,
                        UsersContainer.GetUser(selItem.Username));

                    if (windows.ShowDialog() == true)
                    {
                        this.Refresh();
                    }
                }
                else
                {
                    var dialogResult = MessageBox.Show(
                            App.Current.MainWindow,
                            string.Format("You cannot Edit this user."),
                            App.ResourceAssembly.GetName().Name,
                            MessageBoxButton.OK);
                }
            }
        }

        private void ButtonOrderByClick(object sender, RoutedEventArgs e)
        {
            if (this.imgUp.Visibility == Visibility.Visible)
            {
                this.imgDown.Visibility = Visibility.Visible;
                this.imgUp.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.imgDown.Visibility = Visibility.Collapsed;
                this.imgUp.Visibility = Visibility.Visible;
            }

            this.Refresh();
        }

        private void ButtonMouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void ButtonMouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }


        private void ComboOrderBySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Refresh();
        }

        private void LibtboxUsersMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.ButtonEditClick(sender, e);
        }

        private void LibtboxUsersSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.RefreshButtons();
        }

        private void ListboxUsersKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter &&
                this.listboxUsers.SelectedItems.Count == 1)
            {
                this.ButtonEditClick(sender, e);
            }
        }
    }


    
}
