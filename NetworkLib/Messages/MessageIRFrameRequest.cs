using Network.Utils;
using System;

namespace Network.Messages
{
    public class MessageIRFrameRequest : Message
    {
        public DeviceID deviceId;

        public MessageIRFrameRequest()
        {
            this.type = MessageType.IRFrameRequest;
            this.info = new byte[] { };
        }

        public MessageIRFrameRequest(DeviceID deviceID)
        {
            this.type = MessageType.IRFrameRequest;
            this.deviceId = deviceID;
            this.info = BitConverter.GetBytes((ushort)deviceID);
        }

        public MessageIRFrameRequest(byte[] infoBytes)
        {
            this.type = MessageType.IRFrameRequest;
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
