using Network.Utils;
using System;

namespace Network.Messages
{
    public class MessageGetConfigurationPerClient : Message
    {
        public DeviceID deviceId;

        public MessageGetConfigurationPerClient()
        {
            this.type = MessageType.GetConfigurationPerClient;
            this.info = new byte[] { };
        }

        public MessageGetConfigurationPerClient(DeviceID deviceID)
        {
            this.type = MessageType.GetConfigurationPerClient;
            this.deviceId = deviceID;
            this.info = BitConverter.GetBytes((ushort)deviceID);
        }

        public MessageGetConfigurationPerClient(byte[] infoBytes)
        {
            this.type = MessageType.GetConfigurationPerClient;
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
