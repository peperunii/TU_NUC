using System.Windows;
using System.Windows.Input;

namespace NUC_Controller.Windows
{
    /// <summary>
    /// Interaction logic for MessageViewer.xaml
    /// </summary>
    public partial class MessageViewer : Window
    {
        public MessageBoxResult result = MessageBoxResult.No;

        public MessageViewer(string title, string message)
        {
            this.InitializeComponent();
            this.Title = title;
            this.textMessage.Text = message;

            this.Owner = Application.Current.MainWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void buttonYes_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            this.Close();
        }

        private void buttonNO_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            this.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                result = MessageBoxResult.No;
                this.Close();
            }
        }
    }
}
