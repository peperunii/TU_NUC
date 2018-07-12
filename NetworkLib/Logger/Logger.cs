using DB_Initialization;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Network.Logger
{
    public static class LogManager
    {
        private static bool showConsole = false;
        private static bool saveInFile = false;
        private static bool saveinDB = false;

        private static LogLevel logLevel = LogLevel.Errors;
        private static DB database = null;
        
        private static string logFilename;

        static LogManager()
        {

        }

        public static void SetConsole(bool show = true)
        {
            LogManager.showConsole = show;
        }

        public static void SetFileOutput(string filename, bool saveInFile = true)
        {
            LogManager.logFilename = filename;
            LogManager.saveInFile = saveInFile;
        }

        public static void SetDB(bool saveInDB = true)
        {
            LogManager.saveinDB = saveInDB;
            LogManager.database = new DB(false, true);
        }

        public static void SetLogLevel(LogLevel logLevel = LogLevel.Errors)
        {
            LogManager.logLevel = logLevel;
        }

        //private static int iterator = 0;
        //public static void LogError(string sender, int line)
        //{
        //    Console.WriteLine(DateTime.Now + ": " + "Step " + iterator + ". func: " + sender + ", line: " + line);
        //}

        public static async void LogMessage(LogType logType, LogLevel priority, string message)
        {
            if (LogManager.logLevel >= priority)
            {
                await Task.Run(() =>
                {
                    var time = DateTime.Now;
                    var messageLog = string.Format("{0},{1}: [{2}] {3}", time, time.Millisecond, logType, message);

                    if (LogManager.showConsole)
                    {
                        Console.WriteLine(messageLog);
                    }
                    if (LogManager.saveInFile)
                    {
                        File.AppendAllText(LogManager.logFilename, messageLog + "\n");
                    }
                    if (LogManager.saveinDB)
                    {
                        LogManager.database.Query(
                            string.Format(
                                QueryStrings.Insert_Into_Table_ServerEvents,
                                DateTime.Now.ToFileTimeUtc(),
                                Configuration.DeviceID,
                                Configuration.DeviceIP,
                                logType,
                                message));
                    }
                });
            }
        }
    }
}
