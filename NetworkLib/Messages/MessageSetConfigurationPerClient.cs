using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageSetConfigurationPerClient : Message
    {
        public MessageSetConfigurationPerClient(string configuration)
        {
            this.type = MessageType.SetConfigurationPerClient;
            this.info = configuration;
        }

        public override byte[] Serialize()
        {
            var bytes = this.GetBytesForNumberShort((ushort)this.type);
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
