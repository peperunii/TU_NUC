using Network;
using NUC_Controller.DB;
using NUC_Controller.Users;
using System.Threading;
using System.Windows.Forms;

namespace NUC_Controller
{
    public static class Globals
    {
        public static User loggedInUser;
        public static Database Database;
        public static bool IsConnectedToServer;

        static Globals()
        {
            Database = new Database();
            IsConnectedToServer = false;
        }

        public static void RestartApp()
        {
            Thread.Sleep(5000);
            Application.Restart();
        }

        public static void ReloadConfiguration()
        {
            Configuration.LoadConfiguration();
        }
    }
}
