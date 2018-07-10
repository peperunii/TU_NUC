using Network.Messages;
using System;

namespace NetworkLib.Events
{
    public class NewDepthArrivedEventArgs : EventArgs
    {
        public MessageDepthFrame Message { get; set; }

        public NewDepthArrivedEventArgs(MessageDepthFrame message)
        {
            this.Message = message;
        }
    }
}
