using Network.Utils;
using System;

namespace Network.Messages
{
    public class MessageCameraStop : Message
    {
        public DeviceID deviceId;

        public MessageCameraStop()
        {
            this.type = MessageType.CameraStop;
            this.info = new byte[] { };
        }

        public MessageCameraStop(DeviceID deviceID)
        {
            this.type = MessageType.CameraStop;
            this.deviceId = deviceID;
            this.info = BitConverter.GetBytes((ushort)deviceID);
        }

        public MessageCameraStop(byte[] infoBytes)
        {
            this.type = MessageType.CameraStop;
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
