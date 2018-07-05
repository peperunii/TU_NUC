using Network;
using Network.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                /* Setting up the Log manager */
                LogManager.SetLogLevel(Configuration.loglevel);

                LogManager.SetConsole(Configuration.logInConsole);
                LogManager.SetDB(Configuration.logInDB);
                LogManager.SetFileOutput(Configuration.logFilename, Configuration.logInFile);
                
                LogManager.LogMessage(LogType.Info, LogLevel.ErrWarnInfo, string.Format("Server: Started", Configuration.DeviceID));

                ServerActions.Listener();
            }
            catch (Exception ex)
            {
                Console.WriteLine("3: " + ex.ToString());
                /*Restart application on exception*/
                Global.RestartApp();
            }
        }
    }
}
