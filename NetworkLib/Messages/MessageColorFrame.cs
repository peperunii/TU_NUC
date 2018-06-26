using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMiracle.LibJpeg.Classic;
using BitMiracle.LibJpeg;
using Network.Logger;

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
            this.type = MessageType.ColorFrame;
            this.Height = BitConverter.ToInt32(colorFrameInfo.SubArray(0, 4), 0);
            this.Height = BitConverter.ToInt32(colorFrameInfo.SubArray(4, 4), 0);
            this.IsCompressed = BitConverter.ToBoolean(colorFrameInfo.SubArray(8, 1), 0);

            this.info = colorFrameInfo.SubArray(9);

            LogManager.LogMessage(LogType.Info, "Received ColorFrame");
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
            this.info = BitConverter.GetBytes(this.Height).Concat(BitConverter.GetBytes(this.Width).Concat(BitConverter.GetBytes(this.Channels).Concat(BitConverter.GetBytes(this.IsCompressed))));
            return this.info as byte [];
        }
    }
}
