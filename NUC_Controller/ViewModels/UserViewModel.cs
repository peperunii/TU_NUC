using NUC_Controller.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NUC_Controller.ViewModels
{
    public class UserViewModel
    {
        public bool IsSelected { get; set; }
        public bool IsEnabled { get; set; }

        public string Username { get; set; }
        public UserType Type { get; set; }
        public Brush BackgroundColor { get; set; }
        public string Tooltip { get; set; }

        public UserViewModel(User user)
        {
            this.IsEnabled = true;

            this.Username = user.Username;
            this.Type = user.Type;

            //all children
            if (Globals.loggedInUser.Username == user.Username)
            {
                /*Logged user*/
                this.BackgroundColor = Brushes.YellowGreen;
            }
            else
            {
                if (!Globals.loggedInUser.CheckIfHasAccess(ActionType.ChangeExistingUser))
                {
                    this.BackgroundColor = Brushes.DarkGray;
                    this.IsEnabled = false;
                    this.Tooltip = "Unable to edit";
                }
                else
                {
                    this.BackgroundColor = Brushes.LightGray;
                    this.IsEnabled = true;
                    this.Tooltip = "Modify user's settigns";
                }
            }
        }
    }
}
