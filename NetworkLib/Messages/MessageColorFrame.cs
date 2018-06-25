using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMiracle.LibJpeg.Classic;
using BitMiracle.LibJpeg;

namespace Network.Messages
{
    public class MessageColorFrame : Message
    {
        public int Height;
        public int Width;
        public int Channels;
        public bool IsCompressed; // Jpeg
        public long Timestamp;

        public MessageColorFrame(int height, int width, int channels, bool isCompressed, byte [] bytes)
        {
            this.type = MessageType.ColorFrame;
            this.info = bytes;
            this.Timestamp = DateTime.Now.ToFileTimeUtc();

            this.Height = height;
            this.Width = width;
            this.Channels = channels;
            this.IsCompressed = isCompressed;
        }

        public MessageColorFrame(byte[] colorFrameInfo)
        {
            this.type = MessageType.Info;
            //this.info = new JpegImage(new SampleRow())
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
