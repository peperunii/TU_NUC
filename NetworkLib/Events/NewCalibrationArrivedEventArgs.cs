using Network;
using Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib.Events
{
    public class NewCalibrationArrivedEventArgs : EventArgs
    {
        public MessageCalibration messageCalibration { get; set; }

        public NewCalibrationArrivedEventArgs(MessageCalibration message)
        {
            this.messageCalibration = message;
        }
    }
}
