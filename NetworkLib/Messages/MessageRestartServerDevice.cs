using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageRestartServerDevice : Message
    {
        public MessageRestartServerDevice()
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
