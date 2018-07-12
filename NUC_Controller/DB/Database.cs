namespace NUC_Controller.DB
{
    using DB_Initialization;
    using Npgsql;
    using NUC_Controller.Notifications;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class Database
    {
        private string dbIP;
        private int dbPort;
        private string dbUsername;
        private string dbPassword;
        private string dbName;

        private string databaseConnectionString;
        private string serverConnectionString;

        public Database()
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

            Tables.Init();
            Schemas.Init(this);
        }

        public void QueryExecute(string query, List<string> dbParams = null)
        {
            var result = string.Empty;
            var conn = new NpgsqlConnection(this.databaseConnectionString);

            try
            {
                conn.Open();
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                query = this.InsertParamsInQuery(query, dbParams);
                var voidQuery = new NpgsqlCommand(query, conn);

                voidQuery.ExecuteNonQuery();
                Thread.Sleep(10);
            }
            catch (Exception)
            {
                
            }
            finally
            {
                conn.Close();
            }
        }

        public bool QuerySelect(string query, List<string> dbParams = null)
        {
            var success = true;
            var conn = new NpgsqlConnection(this.databaseConnectionString);

            try
            {
                conn.Open();
            }
            catch(Exception ex)
            {
                this.SetDBExceptionNotification(ex);
                return false;
            }

            try
            {
                query = this.InsertParamsInQuery(query, dbParams);
                var selectionQuery = new NpgsqlCommand(query, conn);

                NpgsqlDataReader dataReader = selectionQuery.ExecuteReader();

                var tableName = (TableName)Enum.Parse(typeof(TableName), dbParams[0]);

                while (dataReader.Read())
                {
                    var dataRow = new DBRow(tableName);
                    var rowItems = new List<object>();

                    for (int i = 0; i < Schemas.schemas[tableName].Count; i++)
                    {
                        rowItems.Add(dataReader[i]);
                    }
                    
                    dataRow.SetRow(rowItems);
                    Tables.dbInfo[tableName].Add(dataRow);
                }
                dataReader.Close();

                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                this.SetDBExceptionNotification(ex);
                success = false;
            }
            finally
            {
                conn.Close();
            }

            return success;
        }

        public List<string> QueryGetSchemaForTable(string query, List<string> dbParams = null)
        {
            var schema = new List<string>();

            var conn = new NpgsqlConnection(this.databaseConnectionString);

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                this.SetDBExceptionNotification(ex);
                return schema;
            }

            try
            {
                query = this.InsertParamsInQuery(query, dbParams);
                var selectionQuery = new NpgsqlCommand(query, conn);

                NpgsqlDataReader dataReader = selectionQuery.ExecuteReader();

                var tableName = (TableName)Enum.Parse(typeof(TableName), dbParams[0]);

                while (dataReader.Read())
                {
                    schema.Add(dataReader[3].ToString());
                }
                dataReader.Close();

                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                this.SetDBExceptionNotification(ex);
            }
            finally
            {
                conn.Close();
            }

            return schema;
        }

        public bool IsConnectiontoDBEstablieshed()
        {
            return this.QuerySelect(DatabaseQueries.SelectFromTable, new List<string>() { TableName.serverevents.ToString() });
        }

        private void SetDBExceptionNotification(Exception ex)
        {
            var exceptionLength = ex.ToString().Length;
            var substringLength = exceptionLength > MonitoringConfiguration.MAX_LENGTH_FOR_NOTIFICATION_ERROR ? MonitoringConfiguration.MAX_LENGTH_FOR_NOTIFICATION_ERROR : exceptionLength;
            new Notification(NotificationType.Error, ex.ToString().Substring(0, substringLength) + "...");
        }

        private string InsertParamsInQuery(string query, List<string> dbParams)
        {
            if(dbParams != null)
            {
                query = string.Format(query, dbParams.ToArray());
            }

            return query;
        }
    }
}
