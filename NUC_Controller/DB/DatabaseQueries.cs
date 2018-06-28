using DB_Initialization;

namespace NUC_Controller.DB
{
    public static class DatabaseQueries
    {
        public static string SelectFromTable;

        public static string SelectFromTableLastN;

        public static string SelectFromTableWhere;

        public static string GetSchemaForTable;

        public static string InsertIntoTable;

        public static string UpdateRowInTable;

        public static string DeleteFromTable;

        static DatabaseQueries()
        {
            SelectFromTable = Configuration.dbQuery_SelectFromTable;
            SelectFromTableLastN = Configuration.dbQuery_Select_FromTable_lastN;
            SelectFromTableWhere = Configuration.dbQuery_Select_FromTable_Where;
            GetSchemaForTable = Configuration.dbQuery_GetSchema;
            InsertIntoTable = Configuration.dbQuery_Insert_IntoTable;
            UpdateRowInTable = Configuration.dbQuery_Update_RowInTable;
            DeleteFromTable = Configuration.dbQuery_DeleteFromTable;
        }
    }
}
