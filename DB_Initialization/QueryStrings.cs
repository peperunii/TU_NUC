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
            "ip varchar(32)," + 
            "eventType varchar(32), " +
            "info varchar(512))";

        public static string create_Table_Users =
            "CREATE TABLE Users " +
            "(ID SERIAL PRIMARY KEY, " +
            "username varchar(32) NOT NULL, " +
            "password varchar(64) NOT NULL, " +
            "usertype varchar(32) NOT NULL)";

        public static string create_Table_KinectBodies =
            "CREATE TABLE KinectBodies " +
            "(ID SERIAL PRIMARY KEY, " +
            "timestamp bigint NOT NULL, " +
            "source varchar(16), " +
            "istracked varchar(8)," +
            "joints varchar(1500))";

        public static string create_Table_Faces =
            "CREATE TABLE Faces " +
            "(ID SERIAL PRIMARY KEY, " +
            "timestamp bigint NOT NULL, " +
            "source varchar(16), " +
            "description varchar(10500))";

        public static string create_Table_Calibrations =
            "CREATE TABLE Calibrations " +
            "(ID SERIAL PRIMARY KEY, " +
            "timestamp bigint NOT NULL, " +
            "source varchar(16), " +
            "intrinsic varchar(1000)," +
            "extrinsic varchar(1000))";


        public static string Insert_Into_Table_ServerEvents =
            "INSERT INTO ServerEvents(id, timestamp, source, ip, eventType, info) VALUES (NEXTVAL(pg_get_serial_sequence('ServerEvents', 'id')), {0}, '{1}', '{2}', '{3}', '{4}');";

        public static string Insert_Into_Table_Users =
            "INSERT INTO Users(id, username, password, usertype) VALUES (NEXTVAL(pg_get_serial_sequence('Users', 'id')), '{0}', '{1}', '{2}');";

        public static string Insert_Into_Table_KinectBodies =
            "INSERT INTO KinectBodies(id, timestamp, source, istracked, joints) VALUES (NEXTVAL(pg_get_serial_sequence('KinectBodies', 'id')), {0}, '{1}', '{2}', '{3}');";

        public static string Insert_Into_Table_Faces =
            "INSERT INTO Faces(id, timestamp, source, description) VALUES (NEXTVAL(pg_get_serial_sequence('Faces', 'id')), {0}, '{1}', '{2}');";
    }
}
