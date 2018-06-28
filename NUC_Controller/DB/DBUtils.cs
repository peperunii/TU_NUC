using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUC_Controller.DB
{
    public static class DBUtils
    {
        public static int DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (int)((TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
        }

        public static string GetColumnNamesAsString(TableName tablename)
        {
            var resultStr = string.Empty;

            var schema = Schemas.schemas[tablename];

            var itemIndex = 0;
            foreach(var item in schema)
            {
                resultStr += item;
                if (itemIndex != schema.Count - 1)
                {
                    resultStr += ", ";
                }
                itemIndex++;
            }

            return resultStr;
        }

        public static string GetValuesString(List<object> objectsLilst)
        {
            var resultStr = string.Empty;

            var itemIndex = 0;
            foreach (var item in objectsLilst)
            {
                var isStr = item is string;
                
                if (isStr && !item.ToString().Contains("NEXTVAL")) resultStr += "'";
                resultStr += item.ToString();
                if (isStr && !item.ToString().Contains("NEXTVAL")) resultStr += "'";

                if (itemIndex != objectsLilst.Count - 1)
                {
                    resultStr += ", ";
                }
                itemIndex++;
            }

            return resultStr;
        }
    }
}
