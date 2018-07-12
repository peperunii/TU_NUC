using Network;
using Network.Devices;
using Network.Logger;
using Server.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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

        private static void StartShutDown(string param)
        {
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.FileName = "cmd";
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Arguments = "/C shutdown " + param;
            Process.Start(proc);
        }

        public static void ShutDownDevice()
        {
            try
            {
                LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Shutting down Device...");
                StartShutDown("-f -s -t 5");
            }
            catch (Exception ex)
            {
                LogManager.LogMessage(LogType.Error, LogLevel.Errors, "Shutting down device failed...");
            }
        }

        public static void RestartDevice()
        {
            try
            {
                LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Restarting Device...");
                StartShutDown("-f -r -t 5");
            }
            catch(Exception ex)
            {
                LogManager.LogMessage(LogType.Error, LogLevel.Errors, "Restarting device failed...");
            }
        }

        public static void RestartApp()
        {
            try
            {
                LogManager.LogMessage(LogType.Warning, LogLevel.ErrWarnInfo, "Server restart...");

                foreach (var tcpServer in ServerActions.tcpServers)
                {
                    tcpServer.Value.DisconnectAll();
                }
                ServerActions.tcpServers.Clear();

                LogManager.LogMessage(LogType.Warning, LogLevel.Communication, "TCP Connections closed...");

                if (ServerActions.discoveryServer != null)
                {
                    ServerActions.discoveryServer.CloseConnection();
                }
                ServerActions.discoveryServer = null;

                LogManager.LogMessage(LogType.Warning, LogLevel.Communication, "UDP server closed...");

                ServerActions.Devices.Clear();
                LogManager.LogMessage(LogType.Warning, LogLevel.Communication, "Clients removed...");
            }
            catch(Exception)
            {
                LogManager.LogMessage(LogType.Error, LogLevel.Errors, "Restart failed...");
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
