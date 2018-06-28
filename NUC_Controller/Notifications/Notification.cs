using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NUC_Controller.Notifications
{
    public class Notification
    {
        public DateTime startTime;

        public Notification(NotificationType type, string message)
        {
            this.startTime = DateTime.Now;
            NotificationsContainer.AddNotification(this);
            var window = Application.Current.Windows[0] as MainWindow;
            window.NotificationSetter(type.ToString() + ": ", message);
        }
    }
}
