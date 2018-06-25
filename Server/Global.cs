using Network;
using Network.Messages;
using Server.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public static class Global
    {
        public static List<NUC> ConnectedDevices { get; set; }
        public static EventDispatcher EventDispatcher { get; } = new EventDispatcher();

        static Global()
        {
            Global.EventDispatcher.AdminStartDiscovery += EventDispatcher_AdminStartDiscovery;
            Global.EventDispatcher.AdminStartCalibration += EventDispatcher_AdminStartCalibration;
            Global.EventDispatcher.AdminStartRecording += EventDispatcher_AdminStartRecording;
        }

        #region GlobalEvents_Handlers
        private static void EventDispatcher_AdminStartRecording(object sender, EventArgs e)
        {
            
        }

        private static void EventDispatcher_AdminStartCalibration(object sender, EventArgs e)
        {
            
        }

        private static void EventDispatcher_AdminStartDiscovery(object sender, EventArgs e)
        {

        }
        #endregion

        public static void RestartApp()
        {
            Application.Restart();
        }

        public static void ReloadConfiguration()
        {
            Configuration.LoadConfiguration();
        }
    }
}
