using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageDiscovery : Message
    {
        public MessageDiscovery()
        {
            this.type = MessageType.Discovery;
            this.info = Configuration.DeviceID;
        }

        public MessageDiscovery(short infoBytes)
        {
            this.type = MessageType.Discovery;
            this.info = infoBytes;
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
            return this.GetBytesForNumberShort((ushort)this.info);
        }
    }
}
