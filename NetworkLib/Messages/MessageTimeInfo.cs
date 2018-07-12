using Network.Utils;
using System;

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
            this.type = MessageType.TimeInfo;
            this.info = BitConverter.ToInt64(dateTimeInfo, 0);
        }

        public override byte[] Serialize()
        {
            var bytes = this.GetBytesForNumberShort((ushort)this.type);
            var bytesInfo = this.GetBytesOfInfo();
            var lenghtInfoBytes = this.GetBytesForNumberInt((int)bytesInfo.Length);

            return bytes.ConcatenatingArrays(lenghtInfoBytes.ConcatenatingArrays(bytesInfo));
        }

        private byte[] GetBytesOfInfo()
        {
            return BitConverter.GetBytes((long)this.info);
        }
    }
}
