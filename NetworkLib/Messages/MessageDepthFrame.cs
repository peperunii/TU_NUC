using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageDepthFrame : Message
    {
        public DeviceID deviceID;

        public int Height;
        public int Width;
        public int Channels;
        public bool IsCompressed; // Jpeg
        public long Timestamp;

        public MessageDepthFrame()
        {
            this.type = MessageType.DepthFrame;
            this.info = new byte[] { };
        }

        public MessageDepthFrame(DeviceID deviceID, int height, int width, int channels, bool isCompressed, byte[] bytes)
        {
            this.type = MessageType.DepthFrame;
            this.info = bytes;
            this.Timestamp = DateTime.Now.ToFileTimeUtc();

            this.deviceID = deviceID;

            this.Height = height;
            this.Width = width;
            this.Channels = channels;
            this.IsCompressed = isCompressed;

        }

        public MessageDepthFrame(byte[] colorFrameInfo = null)
        {
            this.type = MessageType.DepthFrame;
            this.deviceID = (DeviceID)BitConverter.ToUInt16(colorFrameInfo, 0);
            this.Height = BitConverter.ToInt32(colorFrameInfo, 2);
            this.Width = BitConverter.ToInt32(colorFrameInfo, 6);
            this.Channels = BitConverter.ToInt32(colorFrameInfo, 10);
            this.IsCompressed = BitConverter.ToBoolean(colorFrameInfo, 14);
            this.Timestamp = BitConverter.ToInt64(colorFrameInfo, 15);

            this.info = colorFrameInfo.SubArray(23);
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
            this.info =
                 this.GetBytesForNumberShort((ushort)this.deviceID).
                 Concat((BitConverter.GetBytes(this.Height).
                 Concat(BitConverter.GetBytes(this.Width).
                 Concat(BitConverter.GetBytes(this.Channels).
                 Concat(BitConverter.GetBytes(this.IsCompressed).
                 Concat(BitConverter.GetBytes(this.Timestamp).
                 Concat(this.info as byte[]))))))).ToArray();

            return this.info as byte[];
        }
    }
}
