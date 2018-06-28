using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NUC_Controller.Utils
{
    public static class Extensions
    {
        public static int CustomIndexOf(this string source, char toFind, int position)
        {
            int index = -1;
            for (int i = 0; i < position; i++)
            {
                index = source.IndexOf(toFind, index + 1);

                if (index == -1)
                    break;
            }

            return index;
        }

        public static IQueryable<T> OrderBy<T>(
            this IQueryable<T> source,
            string orderByProperty,
            bool asc) where T : class
        {

            string command = asc ? "OrderBy" : "OrderByDescending";

            var type = typeof(T);
            var property = type.GetProperty(orderByProperty);
            var parameter = System.Linq.Expressions.Expression.Parameter(type, "param");
            var propertyAccess = System.Linq.Expressions.Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = System.Linq.Expressions.Expression.Lambda(propertyAccess, parameter);
            var resultExpression =
                System.Linq.Expressions.Expression.Call(
                    typeof(Queryable),
                    command,
                    new Type[] { type, property.PropertyType },
                    source.Expression,
                    System.Linq.Expressions.Expression.Quote(orderByExpression));

            return source.Provider.CreateQuery<T>(resultExpression);
        }

        public static void Sort<TSource, TKey>(
            this ObservableCollection<TSource> source,
            Func<TSource, TKey> keySelector,
            bool asc)
        {
            List<TSource> sortedList = null;
            if (asc)
            {
                sortedList = source.OrderBy(keySelector).ToList();
            }
            else
            {
                sortedList = source.OrderByDescending(keySelector).ToList();
            }

            source.Clear();
            sortedList.ForEach(x => source.Add(x));
        }

        public static int RemoveAll<T>(this ObservableCollection<T> source, Predicate<T> predicate)
        {
            var elements = source.Where(entity => predicate(entity)).ToList();
            var count = elements.Count;

            elements.ForEach(entity => source.Remove(entity));

            return count;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumeration)
        {
            return new ObservableCollection<T>(enumeration);
        }

        
        public static T Clone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
