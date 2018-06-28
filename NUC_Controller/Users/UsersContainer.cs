namespace NUC_Controller.Users
{
    using NUC_Controller.DB;
    using NUC_Controller.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class UsersContainer
    {
        private static string AlreadyExistsError = "User with same username already exist.";
        
        private static SortedDictionary<UInt32, User> usersList;
        public static UNG UniqueNumberGenerator { get; set; }

        static UsersContainer()
        {
            usersList = new SortedDictionary<UInt32, User>();
            UniqueNumberGenerator = new UNG(1, UInt32.MaxValue);
        }

        public static void AddUser(DBRow dbRow)
        {
            /*This dbRow should be from Users table - otherwise - an exception will be thrown*/
            if (dbRow != null)
            {
                try
                {
                    var id = uint.Parse(dbRow.items["id"].ToString());
                    var username = dbRow.items["username"] as string;
                    var password = dbRow.items["password"] as string;
                    var type = (UserType)Enum.Parse(typeof(UserType), dbRow.items["usertype"] as string);

                    if (usersList.ContainsKey(id))
                    {
                        return;
                        //throw new ArgumentException(AlreadyExistsError);
                    }
                    UniqueNumberGenerator.ReserveNumber(id);

                    usersList.Add(id, new User(username, password, type));
                }
                catch(Exception ex)
                {

                }
            }
        }

        public static void AddUser(User user)
        {
            /*This dbRow should be from Users table - otherwise - an exception will be thrown*/
            if (user != null)
            {
                var id = UniqueNumberGenerator.NextUInt32();
                
                if (usersList.ContainsKey(id))
                {
                    throw new ArgumentException(AlreadyExistsError);
                }
                UniqueNumberGenerator.ReserveNumber(id);

                usersList.Add(id, user);
            }
        }

        public static List<User> GetAllUsers()
        {
            return (from t in usersList
                    select t.Value).ToList();
        }

        public static User GetUser(string username)
        {
            return (from t in GetAllUsers()
                    where t.Username == username
                    select t).FirstOrDefault();
        }

        internal static void RemoveUser(string name)
        {
            var id = (from t in usersList
                      where t.Value.Username == name
                      select t.Key).FirstOrDefault();
            usersList.Remove(id);
        }
    }
}
