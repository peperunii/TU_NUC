using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageIRFrameRequest : Message
    {
        public MessageIRFrameRequest()
        {
            this.type = MessageType.IRFrameRequest;
            this.info = null;
        }

        public override byte[] Serialize()
        {
            return this.GetBytesForNumberShort((short)this.type);
        }
    }
}
