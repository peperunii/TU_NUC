using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageCalibrationRequest : Message
    {
        public MessageCalibrationRequest()
        {
            this.type = MessageType.RestartServerDevice;
            this.info = null;
        }

        public override byte[] Serialize()
        {
            return this.GetBytesForNumberShort((ushort)this.type);
        }
    }
}
