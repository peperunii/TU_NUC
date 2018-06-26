using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageDepthFrameRequest : Message
    {
        public MessageDepthFrameRequest()
        {
            this.type = MessageType.DepthFrameRequest;
            this.info = null;
        }

        public override byte[] Serialize()
        {
            return this.GetBytesForNumberShort((ushort)this.type);
        }
    }
}
