using Network.Utils;
using System;

namespace Network.Messages
{
    public class MessageRestartClientApp : Message
    {
        public DeviceID deviceId;

        public MessageRestartClientApp()
        {
            this.type = MessageType.RestartClientApp;
            this.info = new byte[] { };
        }

        public MessageRestartClientApp(DeviceID deviceID)
        {
            this.type = MessageType.RestartClientApp;
            this.deviceId = deviceID;
            this.info = BitConverter.GetBytes((ushort)deviceID);
        }

        public MessageRestartClientApp(byte[] infoBytes)
        {
            this.type = MessageType.RestartClientApp;
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
