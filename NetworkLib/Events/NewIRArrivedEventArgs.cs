using Network.Messages;
using System;

namespace NetworkLib.Events
{
    public class NewIRArrivedEventArgs : EventArgs
    {
        public MessageIRFrame Message { get; set; }

        public NewIRArrivedEventArgs(MessageIRFrame message)
        {
            this.Message = message;
        }
    }
}