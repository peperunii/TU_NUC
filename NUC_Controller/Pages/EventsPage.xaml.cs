namespace NUC_Controller.Pages
{
    using Network.Logger;
    using NUC_Controller.DB;
    using NUC_Controller.InfoTypes.Events;
    using NUC_Controller.Notifications;
    using NUC_Controller.Utils;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for LogsPage.xaml
    /// </summary>
    public partial class EventsPage : Page
    {
        private int lastSearchedIndexAllLogs = 0;
        private int lastSearchedIndexInformation = 0;
        private int lastSearchedIndexNotifications = 0;

        private bool isAltKeyPressed = false;
        private bool isAllowedSingleLogCopy = true;
        private bool isFirstTimeLoad = true;

        private static int lastSelectedLogTab = 0;
        private static int comboFilterTypeIndex = 0;
        private static string textFilterAllLogs = string.Empty;
        private static string textFilterInformation = string.Empty;
        private static string textFilterCharts = string.Empty;
        private static string textFilterErrorLogs = string.Empty;

        private static List<string> allinformation = new List<string>();

        public Func<double, string> ChartAxisFormat { get; set; }
        
        public EventsPage()
        {
            this.InitializeComponent();
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBoxLogFilter.IsEnabled = false;
            this.tabLogControl.SelectedIndex = lastSelectedLogTab;

            /*Init Logs - if Logs page loaded for the first time*/
            

            if (EventsContainer.GetAllEvents().Count == 0)
            {
                foreach (var log in Tables.dbInfo[TableName.serverevents])
                {
                    EventsContainer.AddEvent(log);
                }
            }

            var allLogs = EventsContainer.GetAllEvents();

            var sourcesCount = (from t in allLogs
                                select t.Source).Distinct().Count();

            var eventTypes = (from t in allLogs
                             select t.LogType).Distinct();

            var logsCount = allLogs.Count;

            allinformation.Clear();
            allinformation.Add(string.Format("Devices: {0}", sourcesCount));
            if(eventTypes.Count() != 0)
            {
                foreach(var eventType in eventTypes)
                {
                    var eventCount = (from t in allLogs
                                  where t.LogType == eventType
                                  select t).Count();
                    allinformation.Add(string.Format("Number of Events for type '{0}': {1}", eventType, eventCount));
                }
            }
            allinformation.Add(string.Format("Total Number of Events: {0}", logsCount));

          
            if (comboFilterTypeIndex != -1)
            {
                switch (lastSelectedLogTab)
                {
                    case 0:
                        this.textBoxLogFilter.Text = textFilterAllLogs;
                        break;

                    case 1:
                        this.textBoxLogFilter.Text = textFilterInformation;
                        break;

                    case 2:
                        this.textBoxLogFilter.Text = textFilterCharts;
                        break;
                }

                this.textBoxLogFilter.IsEnabled = true;
                if (this.comboType != null)
                {
                    this.comboType.SelectedIndex = comboFilterTypeIndex;
                }
                RefreshLogs();
            }

            if (!Globals.loggedInUser.CheckIfHasAccess(Users.ActionType.ExportEventsToFile))
            {
                this.buttonExportToFile.IsEnabled = false;
            }

            if(this.isFirstTimeLoad)
            {
                this.SortDataGrid(this.listBoxAllLogs, 0, ListSortDirection.Descending);
                this.SortDataGrid(this.listBoxAllNotifications, 0, ListSortDirection.Descending);
                this.isFirstTimeLoad = false;
            }
        }

        private void RefreshLogs()
        {
            switch (this.comboType.SelectedIndex)
            {
                case 0: //Search
                        //Do nothing -> only when clicked on button Next
                    break;

                case 1: //Filter
                    switch (this.tabLogControl.SelectedIndex)
                    {
                        case 0: // All Logs
                            {
                                var filteredLogs = FilterLogsBasedOnString(EventsContainer.GetAllEvents());

                                this.listBoxAllLogs.ItemsSource = null;
                                this.listBoxAllLogs.ItemsSource = filteredLogs;
                            }
                            break;
                            
                        case 1: // Notifications
                            {
                                var filteredNotifications = FilterNotificationsBasedOnString(NotificationsContainer.GetAllNotifications());

                                this.listBoxAllNotifications.ItemsSource = null;
                                this.listBoxAllNotifications.ItemsSource = filteredNotifications;
                            }
                            break;

                        case 2: // Information
                            {
                                var filteredLogs = FilterInformationBasedOnString(allinformation);

                                this.listBoxInformation.ItemsSource = null;
                                this.listBoxInformation.ItemsSource = filteredLogs;
                            }
                            break;
                    }
                    break;
            }
        }


        private void ButtonExportToFile_Click(object sender, RoutedEventArgs e)
        {
            switch (this.tabLogControl.SelectedIndex)
            {
                case 0:
                    {
                        this.MenuExportAllLogs_Click();
                    }
                    break;

                case 1:
                    {
                        //this.MenuExportInformation_Click();
                    }
                    break;

                case 2:
                    {
                        //this.MenuExportCharts_Click();
                    }
                    break;
            }
        }

        private void MenuExportAllLogs_Click()
        {
            Microsoft.Win32.SaveFileDialog dlg =
                new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "encrypted log files (.log.bin)|*.log.bin"
                };

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                //LogsWriter writer = new LogsWriter();
                //byte[] exportedLogs = writer.WriteLogs(LogsWriter.LogsType.AllEvent);
                //
                //FileEncryptor.Encrypt(exportedLogs, dlg.FileName);
            }
        }

        private void ButtonNextResult_Click(object sender, RoutedEventArgs e)
        {
            if (this.comboType.SelectedIndex == 0)
            {
                /*Get current text from textBoxLogFilter*/
                var textFilter = this.textBoxLogFilter.Text;
                if (textFilter != string.Empty)
                {
                    var SelectedIndex = SearchUsingTextCriteria(textFilter);
                    if (SelectedIndex != -1)
                    {
                        switch (this.tabLogControl.SelectedIndex)
                        {
                            case 0:
                                {
                                    //this.listBoxAllLogs.ScrollIntoView(this.listBoxAllLogs.SelectedItem);
                                    this.listBoxAllLogs.SelectedIndex = SelectedIndex;

                                    if (this.listBoxAllLogs.ItemContainerGenerator.ContainerFromIndex(SelectedIndex) is ListViewItem lbi)
                                    {
                                        lbi.IsSelected = true;
                                        lbi.Focus();
                                    }
                                    var sw = FindScrollViewer(this.listBoxAllLogs);

                                    if (sw != null) sw.ScrollToVerticalOffset(SelectedIndex);

                                    this.lastSearchedIndexAllLogs = SelectedIndex;
                                    this.listBoxAllLogs.UpdateLayout();
                                }
                                break;

                            case 1:
                                {
                                    //this.listBoxAllLogs.ScrollIntoView(this.listBoxAllLogs.SelectedItem);
                                    this.listBoxAllNotifications.SelectedIndex = SelectedIndex;

                                    if (this.listBoxAllNotifications.ItemContainerGenerator.ContainerFromIndex(SelectedIndex) is ListViewItem lbi)
                                    {
                                        lbi.IsSelected = true;
                                        lbi.Focus();
                                    }
                                    var sw = FindScrollViewer(this.listBoxAllNotifications);

                                    if (sw != null) sw.ScrollToVerticalOffset(SelectedIndex);

                                    this.lastSearchedIndexNotifications = SelectedIndex;
                                    this.listBoxAllNotifications.UpdateLayout();
                                }
                                break;

                            case 2:
                                {
                                    this.listBoxInformation.SelectedIndex = SelectedIndex;
                                    if (this.listBoxInformation.ItemContainerGenerator.ContainerFromIndex(SelectedIndex) is ListViewItem lbi)
                                    {
                                        lbi.IsSelected = true;
                                        lbi.Focus();
                                    }
                                    var sw = FindScrollViewer(this.listBoxInformation);
                                    if (sw != null) sw.ScrollToVerticalOffset(SelectedIndex);

                                    //this.listBoxChangeLogs.ScrollIntoView(this.listBoxChangeLogs.SelectedItem);
                                    this.lastSearchedIndexInformation = SelectedIndex;
                                    this.listBoxInformation.UpdateLayout();
                                }
                                break;
                        }
                    }
                }
            }
        }


        private void TabLogControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lastSelectedLogTab = this.tabLogControl.SelectedIndex;
            switch (this.tabLogControl.SelectedIndex)
            {
                case 0:
                    this.textBoxLogFilter.Text = textFilterAllLogs;
                    break;

                case 1:
                    break;

                case 2:
                    break;
            }
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.textBoxLogFilter.IsEnabled = true;
            comboFilterTypeIndex = this.comboType.SelectedIndex;

            this.listBoxAllLogs.ItemsSource = EventsContainer.GetAllEvents();
            this.listBoxAllNotifications.ItemsSource = NotificationsContainer.GetAllNotifications();
            this.listBoxInformation.ItemsSource = allinformation;


            if (this.comboType.SelectedIndex == 0)
            {
                this.buttonNextResult.Visibility = Visibility.Visible;
            }
            else
            {
                this.buttonNextResult.Visibility = Visibility.Collapsed;
            }

            /*Trigger filterring base on current text in textBoxFilter*/
            this.TextBoxLogFilter_TextChanged(null, null);
        }

        private void TextBoxLogFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            switch (this.tabLogControl.SelectedIndex)
            {
                case 0:
                    textFilterAllLogs = this.textBoxLogFilter.Text;
                    break;

                case 1:
                    textFilterInformation = this.textBoxLogFilter.Text;
                    break;

                case 2:
                    textFilterCharts = this.textBoxLogFilter.Text;
                    break;
            }

            this.lastSearchedIndexAllLogs = 0;
            this.lastSearchedIndexInformation = 0;
            this.lastSearchedIndexNotifications = 0;

            this.RefreshLogs();
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                this.isAltKeyPressed = true;
            }
            else if (e.Key == Key.Enter)
            {
                this.ButtonNextResult_Click(null, null);
            }

            if (Keyboard.Modifiers == ModifierKeys.Control &&
                e.Key == Key.C)
            {
                this.RightClickCopyCmdExecuted(sender, null);
            }
        }

        private void Page_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                this.isAltKeyPressed = false;
            }
        }

        private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                this.isAltKeyPressed = true;
            }

            if (this.isAltKeyPressed == true)
            {
                if (e.Key == Key.E)
                {
                    this.tabLogControl.SelectedIndex = 0;
                }
                else if (e.Key == Key.I)
                {
                    this.tabLogControl.SelectedIndex = 1;
                }
                else if (e.Key == Key.C)
                {
                    this.tabLogControl.SelectedIndex = 2;
                }
            }
        }


        private void RightClickCopyCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string collectedText = "";

            if (this.tabLogControl.SelectedIndex == 0)
            {
                foreach (string str in this.listBoxAllLogs.SelectedItems)
                {
                    collectedText += str + "\r\n";
                }

                if (this.listBoxAllLogs.SelectedItems != null)
                {
                    Clipboard.SetText(collectedText);
                }
            }
        }

        private void RightClickCopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.isAllowedSingleLogCopy;
        }


        private int SearchUsingTextCriteria(string textCriteria)
        {
            var returnedIndex = -1;

            switch(this.tabLogControl.SelectedIndex)
            {
                case 0:
                    {
                        var currentLogs = EventsContainer.GetAllEvents();
                        var lastSearchedIndex = this.lastSearchedIndexAllLogs;

                        string[] keywords = new string[] { };
                        if (textCriteria[0] == '\"' && textCriteria[textCriteria.Length - 1] == '\"')
                        {
                            if (textCriteria.Length > 1)
                            {
                                keywords = new string[] { textCriteria.Substring(1, textCriteria.Length - 2) };
                            }
                        }
                        else
                        {
                            keywords = textCriteria.Split(' ');
                        }

                        var numberOfKeywords = keywords.Count();

                        if (currentLogs != null)
                        {
                            bool isTextFound = false;

                            if (lastSearchedIndex < currentLogs.Count - 1)
                            {
                                for (int i = lastSearchedIndex + 1; i < currentLogs.Count; i++)
                                {
                                    var numberOFRecognizedWords = 0;

                                    for (int k = 0; k < numberOfKeywords; k++)
                                    {
                                        if (currentLogs[i].Message.Contains(keywords[k], StringComparison.OrdinalIgnoreCase))
                                        {
                                            numberOFRecognizedWords++;
                                        }
                                    }
                                    if (numberOfKeywords == numberOFRecognizedWords)
                                    {
                                        isTextFound = true;
                                        returnedIndex = i;
                                        break;
                                    }
                                }
                            }
                            if (isTextFound == false)
                            {
                                for (int i = 0; i < lastSearchedIndex; i++)
                                {
                                    var numberOFRecognizedWords = 0;

                                    for (int k = 0; k < numberOfKeywords; k++)
                                    {
                                        if (currentLogs[i].Message.Contains(keywords[k], StringComparison.OrdinalIgnoreCase))
                                        {
                                            numberOFRecognizedWords++;
                                        }
                                    }
                                    if (numberOfKeywords == numberOFRecognizedWords)
                                    {
                                        isTextFound = true;
                                        returnedIndex = i;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case 1:
                    {
                        var currentLogs = NotificationsContainer.GetAllNotifications();
                        var lastSearchedIndex = this.lastSearchedIndexAllLogs;

                        string[] keywords = new string[] { };
                        if (textCriteria[0] == '\"' && textCriteria[textCriteria.Length - 1] == '\"')
                        {
                            if (textCriteria.Length > 1)
                            {
                                keywords = new string[] { textCriteria.Substring(1, textCriteria.Length - 2) };
                            }
                        }
                        else
                        {
                            keywords = textCriteria.Split(' ');
                        }

                        var numberOfKeywords = keywords.Count();

                        if (currentLogs != null)
                        {
                            bool isTextFound = false;

                            if (lastSearchedIndex < currentLogs.Count - 1)
                            {
                                for (int i = lastSearchedIndex + 1; i < currentLogs.Count; i++)
                                {
                                    var numberOFRecognizedWords = 0;

                                    for (int k = 0; k < numberOfKeywords; k++)
                                    {
                                        if (currentLogs[i].message.Contains(keywords[k], StringComparison.OrdinalIgnoreCase))
                                        {
                                            numberOFRecognizedWords++;
                                        }
                                    }
                                    if (numberOfKeywords == numberOFRecognizedWords)
                                    {
                                        isTextFound = true;
                                        returnedIndex = i;
                                        break;
                                    }
                                }
                            }
                            if (isTextFound == false)
                            {
                                for (int i = 0; i < lastSearchedIndex; i++)
                                {
                                    var numberOFRecognizedWords = 0;

                                    for (int k = 0; k < numberOfKeywords; k++)
                                    {
                                        if (currentLogs[i].message.Contains(keywords[k], StringComparison.OrdinalIgnoreCase))
                                        {
                                            numberOFRecognizedWords++;
                                        }
                                    }
                                    if (numberOfKeywords == numberOFRecognizedWords)
                                    {
                                        isTextFound = true;
                                        returnedIndex = i;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case 2:
                    var currentInfo = allinformation;
                    //lastSearchedIndex = this.lastSearchedIndexInformation;
                    break;
            }
            
            return returnedIndex;
        }

        private ScrollViewer FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer)
                return d as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var sw = FindScrollViewer(VisualTreeHelper.GetChild(d, i));
                if (sw != null) return sw;
            }
            return null;
        }

        private List<Notification> FilterNotificationsBasedOnString(List<Notification> currentNotifications)
        {
            var filteredList = new List<Notification>();

            var textCriteria = this.textBoxLogFilter.Text;

            if (textCriteria != string.Empty)
            {
                var param = FilterEventType.None;
                var indexOfDots = textCriteria.IndexOf(':');
                if (indexOfDots > 0) // the result should not be the first char.
                {
                    var subString = textCriteria.Substring(0, indexOfDots);

                    if (
                        subString == "Date" ||
                        subString == "date" ||
                        subString == "Time" ||
                        subString == "time" ||
                        subString == "Timestamp" ||
                        subString == "timestamp")
                    {
                        param = FilterEventType.Date;
                    }
                    else if (
                        subString == "Message" ||
                        subString == "message")
                    {
                        param = FilterEventType.Message;
                    }
                }

                /*Set dafault values*/
                if (param == FilterEventType.None)
                {
                    indexOfDots = -1;
                    param = FilterEventType.Message;
                }

                string[] keywords = new string[] { };
                textCriteria = textCriteria.Substring(indexOfDots + 1);

                if (textCriteria.Length > 0 && textCriteria[0] == '\"' && textCriteria[textCriteria.Length - 1] == '\"')
                {
                    if (textCriteria.Length > 1)
                    {
                        keywords = new string[] { textCriteria.Substring(1, textCriteria.Length - 2) };
                    }
                }
                else
                {
                    keywords = textCriteria.Split(' ');
                }

                var numberOfKeywords = keywords.Count();


                foreach (var notification in currentNotifications)
                {
                    var numberOFRecognizedWords = 0;

                    for (int k = 0; k < numberOfKeywords; k++)
                    {
                        switch (param)
                        {
                            case FilterEventType.Date:
                                if (notification.startTime.ToString().Contains(keywords[k], StringComparison.OrdinalIgnoreCase))
                                {
                                    numberOFRecognizedWords++;
                                }
                                break;

                            case FilterEventType.Message:
                                if (notification.message.Contains(keywords[k], StringComparison.OrdinalIgnoreCase))
                                {
                                    numberOFRecognizedWords++;
                                }
                                break;
                        }
                    }
                    if (numberOfKeywords == numberOFRecognizedWords)
                    {
                        filteredList.Add(notification);
                    }
                }
            }
            else
            {
                return currentNotifications;
            }

            return filteredList;
        }

        private List<Event> FilterLogsBasedOnString(List<Event> currentLogs)
        {
            var filteredList = new List<Event>();

            var textCriteria = this.textBoxLogFilter.Text;

            if (textCriteria != string.Empty)
            {
                var param = FilterEventType.None;
                var indexOfDots = textCriteria.IndexOf(':');
                if(indexOfDots > 0) // the result should not be the first char.
                {
                    var subString = textCriteria.Substring(0, indexOfDots);

                    if (subString == "Source" ||
                        subString == "source")
                    {
                        param = FilterEventType.Source;
                    }
                    else if (
                        subString == "Date" ||
                        subString == "date" ||
                        subString == "Time" ||
                        subString == "time" ||
                        subString == "Timestamp" ||
                        subString == "timestamp")
                    {
                        param = FilterEventType.Date;
                    }
                    else if (
                        subString == "Type" ||
                        subString == "type")
                    {
                        param = FilterEventType.Type;
                    }
                    else if (
                        subString == "Message" ||
                        subString == "message")
                    {
                        param = FilterEventType.Message;
                    }
                }

                /*Set dafault values*/
                if (param == FilterEventType.None)
                {
                    indexOfDots = -1;
                    param = FilterEventType.Message;
                }
                
                string[] keywords = new string[] { };
                textCriteria = textCriteria.Substring(indexOfDots + 1);

                if (textCriteria.Length > 0 && textCriteria[0] == '\"' && textCriteria[textCriteria.Length - 1] == '\"') 
                {
                    if (textCriteria.Length > 1)
                    {
                        keywords = new string[] { textCriteria.Substring(1, textCriteria.Length - 2) };
                    }
                }
                else
                {
                    keywords = textCriteria.Split(' ');
                }

                var numberOfKeywords = keywords.Count();


                foreach (var log in currentLogs)
                {
                    var numberOFRecognizedWords = 0;

                    for (int k = 0; k < numberOfKeywords; k++)
                    {
                        switch (param)
                        {
                            case FilterEventType.Source:
                                if (log.Source.Contains(keywords[k], StringComparison.OrdinalIgnoreCase))
                                {
                                    numberOFRecognizedWords++;
                                }
                                break;

                            case FilterEventType.Date:
                                if (log.Timestamp.ToString().Contains(keywords[k], StringComparison.OrdinalIgnoreCase))
                                {
                                    numberOFRecognizedWords++;
                                }
                                break;

                            case FilterEventType.Type:
                                if (log.LogType.Contains(keywords[k], StringComparison.OrdinalIgnoreCase))
                                {
                                    numberOFRecognizedWords++;
                                }
                                break;

                            case FilterEventType.Message:
                                if (log.Message.Contains(keywords[k], StringComparison.OrdinalIgnoreCase))
                                {
                                    numberOFRecognizedWords++;
                                }
                                break;
                        }
                    }
                    if (numberOfKeywords == numberOFRecognizedWords)
                    {
                        filteredList.Add(log);
                    }
                }
            }
            else
            {
                return currentLogs;
            }

            return filteredList;
        }

        private List<string> FilterInformationBasedOnString(List<string> currentLogs)
        {
            var filteredList = new List<string>();

            var textCriteria = this.textBoxLogFilter.Text;

            if (textCriteria != string.Empty)
            {
                string[] keywords = new string[] { };
                if (textCriteria[0] == '\"' && textCriteria[textCriteria.Length - 1] == '\"')
                {
                    if (textCriteria.Length > 1)
                    {
                        keywords = new string[] { textCriteria.Substring(1, textCriteria.Length - 2) };
                    }
                }
                else
                {
                    keywords = textCriteria.Split(' ');
                }

                var numberOfKeywords = keywords.Count();


                foreach (var log in currentLogs)
                {
                    var numberOFRecognizedWords = 0;

                    for (int k = 0; k < numberOfKeywords; k++)
                    {
                        if (log.Contains(keywords[k], StringComparison.OrdinalIgnoreCase))
                        {
                            numberOFRecognizedWords++;
                        }
                    }
                    if (numberOfKeywords == numberOFRecognizedWords)
                    {
                        filteredList.Add(log);
                    }
                }
            }
            else
            {
                return currentLogs;
            }

            return filteredList;
        }

        private void ListBoxAllLogs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void ListBoxAllLogs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            var columnInfo = this.GetColumnInfo(this.listBoxAllLogs);
            var sortInfo = this.GetSortInfo(this.listBoxAllLogs);

            this.buttonRefresh.IsEnabled = false;
            LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Getting event logs");

            Globals.Database.QuerySelect(DatabaseQueries.SelectFromTable, new List<string>() { TableName.serverevents.ToString() });
            EventsContainer.Clear();

            this.Page_Loaded(null, null);

            var filteredLogs = FilterLogsBasedOnString(EventsContainer.GetAllEvents());
            this.listBoxAllLogs.ItemsSource = null;
            this.listBoxAllLogs.ItemsSource = filteredLogs;

            var filteredInfo = FilterInformationBasedOnString(allinformation);
            this.listBoxInformation.ItemsSource = null;
            this.listBoxInformation.ItemsSource = filteredInfo;

            this.RefreshLogs();
            this.SetColumnInfo(this.listBoxAllLogs, columnInfo);
            this.SetSortInfo(this.listBoxAllLogs, sortInfo);

            this.buttonRefresh.IsEnabled = true;
        }


        public void SortDataGrid(DataGrid dataGrid, int columnIndex = 0, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            var column = dataGrid.Columns[columnIndex];

            // Clear current sort descriptions
            dataGrid.Items.SortDescriptions.Clear();

            // Add the new sort description
            dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, sortDirection));

            // Apply sort
            foreach (var col in dataGrid.Columns)
            {
                col.SortDirection = null;
            }
            column.SortDirection = sortDirection;

            // Refresh items to display sort
            dataGrid.Items.Refresh();
        }

        List<DataGridColumn> GetColumnInfo(DataGrid dg)
        {
            List<DataGridColumn> columnInfos = new List<DataGridColumn>();
            foreach (var column in dg.Columns)
            {
                columnInfos.Add(column);
            }
            return columnInfos;
        }

        List<SortDescription> GetSortInfo(DataGrid dg)
        {
            List<SortDescription> sortInfos = new List<SortDescription>();
            foreach (var sortDescription in dg.Items.SortDescriptions)
            {
                sortInfos.Add(sortDescription);
            }
            return sortInfos;
        }

        void SetColumnInfo(DataGrid dg, List<DataGridColumn> columnInfos)
        {
            columnInfos.Sort((c1, c2) => { return c1.DisplayIndex - c2.DisplayIndex; });
            foreach (var columnInfo in columnInfos)
            {
                var column = dg.Columns.FirstOrDefault(col => col.Header == columnInfo.Header);
                if (column != null)
                {
                    column.SortDirection = columnInfo.SortDirection;
                    column.DisplayIndex = columnInfo.DisplayIndex;
                    column.Visibility = columnInfo.Visibility;
                }
            }
        }

        void SetSortInfo(DataGrid dg, List<SortDescription> sortInfos)
        {
            dg.Items.SortDescriptions.Clear();
            foreach (var sortInfo in sortInfos)
            {
                dg.Items.SortDescriptions.Add(sortInfo);
            }
        }
    }
}
