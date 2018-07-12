using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DB_Initialization
{
    public class DB
    {
        private string dbIP;
        private int dbPort;
        private string dbUsername;
        private string dbPassword;
        private string dbName;

        private string databaseConnectionString;
        private string serverConnectionString;

        public DB(bool dropOnCreation, bool donotCreateAnything = false)
        {
            this.dbIP = Configuration.dbIP;
            this.dbPort = Configuration.dbPort;
            this.dbUsername = Configuration.dbUsername;
            this.dbPassword = Configuration.dbPassword;
            this.dbName = Configuration.dbName;

            this.serverConnectionString =
                string.Format("Server={0};Port={1};User Id={2};Password={3}",
                    this.dbIP,
                    this.dbPort,
                    this.dbUsername,
                    this.dbPassword);

            this.databaseConnectionString =
                    this.serverConnectionString + ";Database=" + this.dbName;

            if (!donotCreateAnything)
            {
                if (dropOnCreation)
                {
                    if (Program.showInfo) Console.WriteLine("Closing exisitng connections ...");
                    /*Kill all connections if any*/
                    this.Query(string.Format(QueryStrings.kill_Connections, this.dbName), QueryType.QueryToServer);

                    if (Program.showInfo) Console.WriteLine("Dropping database \"" + this.dbName + "\" ...");
                    /*Drop DB if exist*/
                    this.Query(string.Format(QueryStrings.delete_Database, this.dbName), QueryType.QueryToServer);
                }

                /*Create DB*/
                this.Query(string.Format(QueryStrings.create_Database, this.dbName), QueryType.QueryToServer);

                if (Program.showInfo) Console.WriteLine("Creating database \"" + this.dbName + "\" ...");

                /*Create Tables*/
                if (Program.showInfo) Console.WriteLine("Creating table \"ServerEvents\" ...");
                this.Query(QueryStrings.create_Table_ServerEvents);

                if (Program.showInfo) Console.WriteLine("Creating table \"KinectBodies\" ...");
                this.Query(QueryStrings.create_Table_KinectBodies);

                if (Program.showInfo) Console.WriteLine("Creating table \"Faces\" ...");
                this.Query(QueryStrings.create_Table_Faces);

                if (Program.showInfo) Console.WriteLine("Creating table \"Calibrations\" ...");
                this.Query(QueryStrings.create_Table_Calibrations);

                if (Program.showInfo) Console.WriteLine("Creating table \"Users\" ...");
                this.Query(QueryStrings.create_Table_Users);

                /*Insert Admin User*/
                if (Program.showInfo) Console.WriteLine("Creating Admin User ...");
                this.Query(string.Format(QueryStrings.Insert_Into_Table_Users, "admin", Crypto.GetHash("123"), "Admin"));

                if (Program.fillInfo)
                {
                    if (Program.showInfo) Console.WriteLine("Filling tables ...");
                    this.FillInfo();
                }
            }
        }

        public void Query(string query, QueryType queryType = QueryType.QueryToDatabase)
        {
            var conn = new NpgsqlConnection(queryType == QueryType.QueryToServer ? this.serverConnectionString : this.databaseConnectionString);

            try
            {
                conn.Open();
                /*attempt to create DB. Throw exception on existing, since "IF NOT EXIST" - function is not working*/
                var createTable = new NpgsqlCommand(query, conn);

                createTable.ExecuteNonQuery();
                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                if (Program.showInfo) Console.WriteLine(string.Format("--Error: Unable to complete this query."));
            }
            finally
            {
                conn.Close();
            }
        }

        private void FillInfo()
        {
            var sources = new List<string> { "Server", "NUC_1", "NUC_2", "NUC_3" };
            var eventTypes = new List<string> { "Warning", "Info", "Error" };
            var messages = new List<string> { "Fake message 1", "Fake message 2", "Fake message 3", "Fake message 4"};
            var ip = "127.0.0.1";
            var random = new Random();
            
            for (int i = 0; i < 100000000; i += 100000)
            {
                var source = sources[random.Next(0, sources.Count)];
                var eventType = eventTypes[random.Next(0, eventTypes.Count)];
                var message = messages[random.Next(0, messages.Count)];
                this.Query(string.Format(QueryStrings.Insert_Into_Table_ServerEvents, DateTime.Now.ToFileTimeUtc() + i, source, ip, eventType, message));
            }
        }
    }
}
