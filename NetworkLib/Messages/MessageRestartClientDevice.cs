using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageRestartClientDevice : Message
    {
        public MessageRestartClientDevice()
        {
            this.type = MessageType.RestartClientDevice;
            this.info = null;
        }

        public override byte[] Serialize()
        {
            return this.GetBytesForNumberShort((ushort)this.type);
        }
    }
}
