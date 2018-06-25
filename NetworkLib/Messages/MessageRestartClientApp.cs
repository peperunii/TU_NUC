using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageRestartClientApp : Message
    {
        public MessageRestartClientApp()
        {
            this.type = MessageType.RestartClientApp;
            this.info = null;
        }

        public override byte[] Serialize()
        {
            return this.GetBytesForNumberShort((short)this.type);
        }
    }
}
