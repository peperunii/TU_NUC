using System;
using System.Windows;

namespace NUC_Controller.Notifications
{
    public class Notification
    {
        public DateTime startTime { get; set; }
        public string message { get; set; }

        public Notification(NotificationType type, string message)
        {
            this.startTime = DateTime.Now;
            this.message = message;

            NotificationsContainer.AddNotification(this);

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var window = Application.Current.Windows[0] as MainWindow;
                window.NotificationSetter(type.ToString() + ": ", this.message);
            }));
        }
    }
}
