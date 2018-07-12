using Network.Utils;
using System;

namespace Network.Messages
{
    public class MessageDepthFrameRequest : Message
    {
        public DeviceID deviceId;

        public MessageDepthFrameRequest()
        {
            this.type = MessageType.DepthFrameRequest;
            this.info = new byte[] { };
        }

        public MessageDepthFrameRequest(DeviceID deviceID)
        {
            this.type = MessageType.DepthFrameRequest;
            this.deviceId = deviceID;
            this.info = BitConverter.GetBytes((ushort)deviceID);
        }

        public MessageDepthFrameRequest(byte[] infoBytes)
        {
            this.type = MessageType.DepthFrameRequest;
            this.info = infoBytes;

            this.deviceId = (DeviceID)BitConverter.ToUInt16(infoBytes, 0);
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
            return this.info as byte[];
        }
    }
}
