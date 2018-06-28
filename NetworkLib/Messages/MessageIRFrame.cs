using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageIRFrame : Message
    {
        public int Height;
        public int Width;
        public int Channels;
        public bool IsCompressed; // Jpeg
        public long Timestamp;

        public MessageIRFrame(int height, int width, int channels, bool isCompressed, byte[] bytes)
        {
            this.type = MessageType.ColorFrame;
            this.info = bytes;
            this.Timestamp = DateTime.Now.ToFileTimeUtc();

            this.Height = height;
            this.Width = width;
            this.Channels = channels;
            this.IsCompressed = isCompressed;

        }

        public MessageIRFrame(byte[] colorFrameInfo = null)
        {
            this.type = MessageType.ColorFrame;
            this.Height = BitConverter.ToInt32(colorFrameInfo.SubArray(0, 4), 0);
            this.Width = BitConverter.ToInt32(colorFrameInfo.SubArray(4, 4), 0);
            this.Channels = BitConverter.ToInt32(colorFrameInfo.SubArray(8, 4), 0);
            this.IsCompressed = BitConverter.ToBoolean(colorFrameInfo.SubArray(12, 1), 0);

            this.info = colorFrameInfo.SubArray(13);
        }

        public override byte[] Serialize()
        {
            var bytes = this.GetBytesForNumberShort((ushort)this.type);
            var bytesInfo = this.GetBytesOfInfo();
            var lenghtInfoBytes = this.GetBytesForNumberInt(bytesInfo.Length);

            var result = bytes.Concat(lenghtInfoBytes.Concat(bytesInfo)).ToArray();

            return result;
        }

        private byte[] GetBytesOfInfo()
        {
            var time = DateTime.Now;
            this.info = (BitConverter.GetBytes(this.Height).Concat(BitConverter.GetBytes(this.Width).Concat(BitConverter.GetBytes(this.Channels).Concat(BitConverter.GetBytes(this.IsCompressed).Concat(this.info as byte[]))))).ToArray();
            var timeSpan = (DateTime.Now - time).TotalMilliseconds;

            return this.info as byte[];
        }
    }
}
