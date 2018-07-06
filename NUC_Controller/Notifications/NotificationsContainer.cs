using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUC_Controller.Notifications
{
    public static class NotificationsContainer
    {
        private static List<Notification> notifications;

        static NotificationsContainer()
        {
            notifications = new List<Notification>();
        }

        public static void AddNotification(Notification notification)
        {
            notifications.Add(notification);
        }

        public static Notification GetLastNotification()
        {
            if(notifications.Count > 0)
            {
                return notifications[notifications.Count - 1];
            }
            else
            {
                return null;
            }
        }

        public static List<Notification> GetAllNotifications()
        {
            return notifications;
        }
    }
}
