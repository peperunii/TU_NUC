using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUC_Controller.Users
{
    public class User
    {
        private UserType type;
        public UserType Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        private string username;
        public string Username
        {
            get { return this.username; }
        }

        private string password;
        public string Password
        {
            get { return this.password; }
            set { this.password = value; }
        }

        public User(string uName, string uPass, UserType uType)
        {
            this.username = uName;
            this.password = uPass;
            this.type = uType;
        }

        public bool CheckIfHasAccess(ActionType actionType)
        {
            return
                AccessTable.accessChecker[actionType].Contains(this.Type);
        }
    }
}
