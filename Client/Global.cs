using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public static class Global
    {
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
