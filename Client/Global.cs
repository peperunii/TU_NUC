using Network;
using Network.Logger;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public static class Global
    {
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
            catch (Exception ex)
            {
                LogManager.LogMessage(LogType.Error, LogLevel.Errors, "Restarting device failed...");
            }
        }

        public static void RestartApp()
        {
            try
            {
                Thread.Sleep(3000);
                Application.Restart();
                Thread.Sleep(1000);
                Environment.Exit(0);
            }
            catch (Exception) { }
        }

        public static void ReloadConfiguration()
        {
            Configuration.LoadConfiguration();
        }
    }
}
