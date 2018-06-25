using Network;
using Network.Logger;
using Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

                LogManager.LogMessage(LogType.Info, string.Format("Client {0}: Started", Configuration.DeviceID));

                ClientProcessor.Start();
            }
            catch (Exception ex)
            {
                /*Restart application on exception*/
                Global.RestartApp();
            }
        }
    }
}
