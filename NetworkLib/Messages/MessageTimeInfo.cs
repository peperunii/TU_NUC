using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageTimeInfo : Message
    {
        public MessageTimeInfo()
        {
            this.type = MessageType.TimeInfo;
            this.info = DateTime.Now.ToFileTimeUtc();
        }

        public MessageTimeInfo(byte [] dateTimeInfo)
        {
            this.type = MessageType.Info;
            this.info = BitConverter.ToInt64(dateTimeInfo, 0);
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
            return BitConverter.GetBytes((long)this.info);
        }
    }
}
