using Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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