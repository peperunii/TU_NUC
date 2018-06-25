using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageSendInfoToServer : Message
    {
        public MessageSendInfoToServer(string message)
        {
            this.type = MessageType.Info;
            this.info = message;
        }

        public override byte[] Serialize()
        {
            var bytes = this.GetBytesForNumberShort((short)this.type);
            var bytesInfo = this.GetBytesOfInfo();
            var lenghtInfoBytes = this.GetBytesForNumberInt((int)bytesInfo.Length);

            return bytes.Concat(lenghtInfoBytes.Concat(bytesInfo)).ToArray();
        }

        private byte[] GetBytesOfInfo()
        {
            return Encoding.ASCII.GetBytes(this.info as string);
        }
    }
}
