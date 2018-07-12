using System.Collections.Generic;

namespace NUC_Controller.Utils
{
    public static class UserOrderByMapper
    {
        private static Dictionary<string, string> orderPropList;

        static UserOrderByMapper()
        {
            orderPropList = new Dictionary<string, string>()
            {
                { "Username", "Username" },
                { "Type", "Type" }
            };
        }

        public static string GetPropertyName(string orderBy)
        {
            if (orderPropList.ContainsKey(orderBy))
            {
                return orderPropList[orderBy];
            }

            return string.Empty;
        }

        public static IEnumerable<string> GetOrderItems()
        {
            return orderPropList.Keys;
        }
    }
}
