using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageCalibration : Message
    {
        public MessageCalibration()
        {
            this.type = MessageType.Calibration;
        }

        public override byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
