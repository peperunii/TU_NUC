using Network;
using Network.Devices;
using Network.Logger;
using Network.Messages;
using Server.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            try
            {
                LogManager.LogMessage(LogType.Warning, "Server restart...");

                foreach (var tcpServer in ServerActions.tcpServers)
                {
                    tcpServer.Value.DisconnectAll();
                }
                ServerActions.tcpServers.Clear();

                LogManager.LogMessage(LogType.Warning, "TCP Connections closed...");

                if (ServerActions.discoveryServer != null)
                {
                    ServerActions.discoveryServer.CloseConnection();
                }
                ServerActions.discoveryServer = null;

                LogManager.LogMessage(LogType.Warning, "UDP server closed...");

                ServerActions.Devices.Clear();
                LogManager.LogMessage(LogType.Warning, "Clients removed...");
            }
            catch(Exception)
            {
                LogManager.LogMessage(LogType.Error, "Restart failed...");
            }

            /*The server App is hard to kill*/
            Thread.Sleep(3000);
            Application.Restart();
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        public static void ReloadConfiguration()
        {
            Configuration.LoadConfiguration();
        }
    }
}
