using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUC_Controller.DB
{
    public static class Tables
    {
        public static Dictionary<TableName, List<DBRow>> dbInfo;

        static Tables()
        {
            dbInfo = new Dictionary<TableName, List<DBRow>>();
        }

        public static void Init()
        {
            dbInfo.Clear();

            foreach (TableName tableName in Enum.GetValues(typeof(TableName)))
            {
                dbInfo.Add(tableName, new List<DBRow>());
            }
        }

        public static void ClearTables()
        {
            foreach(var item in dbInfo)
            {
                item.Value.Clear();
            }
        }

        public static void ClearTable(TableName tableName)
        {
            dbInfo[tableName].Clear();
        }
    }

    public class DBRow
    {
        public Dictionary<string, object> items;
        private List<String> schema;

        public DBRow(TableName tableName)
        {
            this.items = new Dictionary<string, object>();
            this.schema = Schemas.schemas[tableName];
        }

        public void SetRow(List<object> itemsInRow)
        {
            for(int itemIndex = 0; itemIndex < itemsInRow.Count; itemIndex++)
            {
                this.items.Add(this.schema[itemIndex], itemsInRow[itemIndex]);
            }
        }
    }

    public static class Schemas
    {
        public static Dictionary<TableName, List<string>> schemas;

        static Schemas()
        {
            schemas = new Dictionary<TableName, List<string>>();
        }

        public static void Init(Database dbInstance)
        {
            schemas.Clear();

            foreach (TableName tableName in Enum.GetValues(typeof(TableName)))
            {
                schemas.Add(tableName, dbInstance.QueryGetSchemaForTable(DatabaseQueries.GetSchemaForTable, new List<string>() { tableName.ToString() }));
            }
        }
    }
}
