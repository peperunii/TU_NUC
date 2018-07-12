using Network.Utils;
using System;

namespace Network.Messages
{
    public class MessageCameraStatusRequest : Message
    {
        public DeviceID deviceId;

        public MessageCameraStatusRequest()
        {
            this.type = MessageType.CameraStatusRequest;
            this.info = new byte[] { };
        }

        public MessageCameraStatusRequest(DeviceID deviceID)
        {
            this.type = MessageType.CameraStatusRequest;
            this.deviceId = deviceID;
            this.info = BitConverter.GetBytes((ushort)deviceID);
        }

        public MessageCameraStatusRequest(byte[] infoBytes)
        {
            this.type = MessageType.CameraStatusRequest;
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
