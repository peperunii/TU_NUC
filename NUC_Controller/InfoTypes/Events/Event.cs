namespace NUC_Controller.InfoTypes.Events
{
    using System;

    public class Event
    {
        public UInt32 ID { get; set; }
        public DateTime Timestamp { get; set; }

        public string Source { get; set; }

        public string Message { get; set; }
        public string IP { get; set; }
        public string LogType { get; set; }

        public Event(
            UInt32 id, 
            long timestampUnixFormat, 
            string source, 
            string message, 
            string ip, 
            string logType)
        {
            this.Timestamp = DateTime.FromFileTimeUtc(timestampUnixFormat); 
            this.ID = id;

            this.Source = source;
            this.Message = message;
            this.IP = ip;
            this.LogType = logType;
        }

        
        public override string ToString()
        {
            var dateTimeFieldLength = 24;
            var sourceFieldLength = 13;

            var timestampText = this.Timestamp.ToString();
            if (timestampText.Length < dateTimeFieldLength)
                timestampText += (new String(' ', (dateTimeFieldLength - timestampText.Length)));

            var sourceText = this.Source.ToString() + ": ";
            if (sourceText.Length < sourceFieldLength)
                sourceText += (new String(' ', (sourceFieldLength - sourceText.Length)));

            return string.Format("{0} {1} {2}", timestampText, sourceText, this.Message);
        }
    }
}
