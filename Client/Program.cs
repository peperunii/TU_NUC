using Network;
using Network.Logger;
using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                /* Setting up the Log manager */
                LogManager.SetConsole(Configuration.logInConsole);
                LogManager.SetDB(Configuration.logInDB);
                LogManager.SetFileOutput(Configuration.logFilename, Configuration.logInFile);

                LogManager.LogMessage(LogType.Info, LogLevel.ErrWarnInfo, string.Format("Client {0}: Started", Configuration.DeviceID));

                ClientProcessor.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("4: " + ex.ToString());
                /*Restart application on exception*/
                LogManager.LogMessage(LogType.Error, LogLevel.Errors, ex.ToString());
                Global.RestartApp();
            }
        }
    }
}
