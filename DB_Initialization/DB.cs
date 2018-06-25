using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

                /*Insert Admin User*/
                //if (Program.showInfo) Console.WriteLine("Creating Admin User ...");
                //this.Query(string.Format(QueryStrings.Insert_Into_Table_Users, "Admin", Crypto.GetHash("123")));

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
            
        }
    }
}
