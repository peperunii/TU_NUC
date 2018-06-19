using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Initialization
{
    public static class Configuration
    {
        public static int dbPort = 0;
        public static string dbIP = "0.0.0.0";
        public static string dbUsername = "";
        public static string dbPassword = "";
        public static string dbName = "";

        public static string dbQuery_SelectFromTable = "";
        public static string dbQuery_Select_FromTable_lastN = "";
        public static string dbQuery_Select_FromTable_Where = "";
        public static string dbQuery_GetSchema = "";
        public static string dbQuery_Insert_IntoTable = "";
        public static string dbQuery_Update_RowInTable = "";
        public static string dbQuery_DeleteFromTable = "";
        
        private static string confIdentifierFor_dbIP = "dbIP";
        private static string confIdentifierFor_dbPort = "dbPort";
        private static string confIdentifierFor_dbUsername = "dbUsername";
        private static string confIdentifierFor_dbPassword = "dbPassword";
        private static string confIdentifierFor_dbName = "dbName";

        private static string confIdentifierFor_dbQuery_Select = "DB_QUERY_SELECT_FROM_TABLE";
        private static string confIdentifierFor_dbQuery_Select_N = "DB_QUERY_SELECT_FROM_TABLE_LAST_N";
        private static string confIdentifierFor_dbQuery_Select_where = "DB_QUERY_SELECT_FROM_TABLE_WHERE";
        private static string confIdentifierFor_dbQuery_GetSchema = "DB_QUERY_GET_SCHEMA";
        private static string confIdentifierFor_dbQuery_Insert = "DB_QUERY_INSERT_INTO_TABLE";
        private static string confIdentifierFor_dbQuery_Update = "DB_QUERY_UPDATE_ROW_IN_TABLE";
        private static string confIdentifierFor_dbQuery_Delete = "DB_QUERY_DELETE_FROM_TABLE";

        private static string configFile = @"..\..\..\config\conf.txt";

        static Configuration()
        {
            /*Read configuration file*/
            ParseConfigiration(File.ReadAllLines(configFile));
        }

        private static void ParseConfigiration(string[] configLines)
        {
            foreach (var line in configLines)
            {
                /*DB Settings*/
                if (line.Contains(confIdentifierFor_dbIP))
                {
                    Configuration.dbIP = GetValueFromLine(line);
                }
                else if (line.Contains(confIdentifierFor_dbPort))
                {
                    Configuration.dbPort = int.Parse(GetValueFromLine(line));
                }
                else if (line.Contains(confIdentifierFor_dbUsername))
                {
                    Configuration.dbUsername = GetValueFromLine(line);
                }
                else if (line.Contains(confIdentifierFor_dbPassword))
                {
                    Configuration.dbPassword = GetValueFromLine(line);
                }
                else if (line.Contains(confIdentifierFor_dbName))
                {
                    Configuration.dbName = GetValueFromLine(line);
                }
                /*DB Queries*/
                else if (line.Contains(confIdentifierFor_dbQuery_Select_N))
                {
                    Configuration.dbQuery_Select_FromTable_lastN = GetValueFromLine(line);
                }
                else if (line.Contains(confIdentifierFor_dbQuery_Select_where))
                {
                    Configuration.dbQuery_Select_FromTable_Where = GetValueFromLine(line);
                }
                else if (line.Contains(confIdentifierFor_dbQuery_Select))
                {
                    Configuration.dbQuery_SelectFromTable = GetValueFromLine(line);
                }
                else if (line.Contains(confIdentifierFor_dbQuery_GetSchema))
                {
                    Configuration.dbQuery_GetSchema = GetValueFromLine(line);
                }
                else if (line.Contains(confIdentifierFor_dbQuery_Update))
                {
                    Configuration.dbQuery_Update_RowInTable = GetValueFromLine(line);
                }
                else if (line.Contains(confIdentifierFor_dbQuery_Insert))
                {
                    Configuration.dbQuery_Insert_IntoTable = GetValueFromLine(line);
                }
                else if (line.Contains(confIdentifierFor_dbQuery_Delete))
                {
                    Configuration.dbQuery_DeleteFromTable = GetValueFromLine(line);
                }
            }
        }

        private static string GetValueFromLine(string line)
        {
            var index = line.IndexOf(':');
            var value = line.Substring(index + 1);
            value = value.Trim();

            if (value[0] == '\"' &&
                value[value.Length - 1] == '\"')
            {
                value = value.Substring(1, value.Length - 2);
            }
            return value;
        }
    }
}
