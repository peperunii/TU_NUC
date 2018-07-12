using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network.Utils;

namespace Network.Messages
{
    public class MessageColorFrameRequest : Message
    {
        public DeviceID deviceId;

        public MessageColorFrameRequest()
        {
            this.type = MessageType.ColorFrameRequest;
            this.info = new byte[] { };
        }

        public MessageColorFrameRequest(DeviceID deviceID)
        {
            this.type = MessageType.ColorFrameRequest;
            this.deviceId = deviceID;
            this.info = BitConverter.GetBytes((ushort)deviceID);
        }

        public MessageColorFrameRequest(byte[] infoBytes)
        {
            this.type = MessageType.ColorFrameRequest;
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
