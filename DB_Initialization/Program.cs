using System;

namespace DB_Initialization
{
    class Program
    {
        public static bool showInfo;
        public static bool fillInfo;

        static void Main(string[] args)
        {
            try
            {
                showInfo = false;
                fillInfo = false;
                var dropDB = false;

                foreach (var arg in args)
                {
                    if (arg == "drop")
                    {
                        dropDB = true;
                    }
                    else if (arg == "debug")
                    {
                        showInfo = true;
                    }
                    else if (arg == "fill")
                    {
                        fillInfo = true;
                    }
                }

                if (showInfo) Console.WriteLine("Connecting to DB ...");
                new DB(dropDB);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Startup Error: " + ex.ToString());
            }
        }
    }
}
