namespace NUC_Controller.InfoTypes.Events
{
    using NUC_Controller.DB;
    using NUC_Controller.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class EventsContainer
    {
        private static string AlreadyExistsError = "Log with same ID already exist.";
        
        private static SortedDictionary<UInt32, Event> logsList;
        public static UNG UniqueNumberGenerator { get; set; }

        static EventsContainer()
        {
            logsList = new SortedDictionary<UInt32, Event>();
            UniqueNumberGenerator = new UNG(1, UInt32.MaxValue);
        }

        public static void AddEvent(DBRow dbRow)
        {
            /*This dbRow should be from Logs table - otherwise - an exception will be thrown*/
            if (dbRow != null)
            {
                var id = uint.Parse(dbRow.items["id"].ToString());
                var timestamp = long.Parse(dbRow.items["timestamp"].ToString());
                var source = dbRow.items["source"] as string;
                var ip = dbRow.items["ip"] as string;
                var eventType = dbRow.items["eventtype"] as string;
                var info = dbRow.items["info"] as string;

                if (logsList.ContainsKey(id))
                {
                    throw new ArgumentException(AlreadyExistsError);
                }
                UniqueNumberGenerator.ReserveNumber(id);

                logsList.Add(id, new Event(id, timestamp, source, info, ip, eventType));
            }
        }

        public static List<Event> GetAllEvents()
        {
            return (from t in logsList
                    select t.Value).ToList();
        }
    }
}
