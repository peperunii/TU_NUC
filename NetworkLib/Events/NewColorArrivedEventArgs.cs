using Network.Messages;
using System;

namespace NetworkLib.Events
{
    public class NewColorArrivedEventArgs : EventArgs
    {
        public MessageColorFrame Message { get; set; }

        public NewColorArrivedEventArgs(MessageColorFrame message)
        {
            this.Message = message;
        }
    }
}
