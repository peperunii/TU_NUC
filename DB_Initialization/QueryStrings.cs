using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Initialization
{
    public static class QueryStrings
    {
        public static string kill_Connections =
            "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE pid <> pg_backend_pid() AND datname = '{0}'";

        public static string delete_Database =
            "DROP DATABASE {0}";

        public static string create_Database =
            "CREATE DATABASE {0}";

        public static string create_Table_ServerEvents =
            "CREATE TABLE ServerEvents " +
            "(ID SERIAL PRIMARY KEY, " +
            "timestamp bigint NOT NULL, " +
            "source varchar(16), " +
            "eventType varchar(32), " +
            "info varchar(512))";
        
        public static string Insert_Into_Table_ServerEvents =
            "INSERT INTO ServerEvents(id, timestamp, source, eventType, info) VALUES (NEXTVAL(pg_get_serial_sequence('ServerEvents', 'id')), {0}, '{1}', '{2}', {3});";
    }
}
