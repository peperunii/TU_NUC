using Network.Utils;
using System;

namespace Network.Messages
{
    public class MessageCalibration : Message
    {
        public DeviceID deviceId;
        public MessageColorFrame colorFrame;
        public MessageDepthFrame depthFrame;

        public MessageCalibration()
        {
            this.type = MessageType.Calibration;
            this.info = new byte[] { };
        }

        public MessageCalibration(
            DeviceID deviceId, 
            MessageColorFrame colorFrame,
            MessageDepthFrame depthFrame)
        {
            this.type = MessageType.Calibration;

            this.deviceId = deviceId;
            this.info = BitConverter.GetBytes((ushort)this.deviceId).
                ConcatenatingArrays(colorFrame.Serialize()).
                ConcatenatingArrays(depthFrame.Serialize());
        }

        public MessageCalibration(byte [] byteArr)
        {
            this.type = MessageType.Calibration;
            this.info = byteArr;

            this.deviceId = (DeviceID)BitConverter.ToUInt16(byteArr, 0);
            var colorMsgLength = BitConverter.ToUInt32(byteArr, 4);
            this.colorFrame = new MessageColorFrame(byteArr.SubArray(8, (int)colorMsgLength));
            this.depthFrame = new MessageDepthFrame(byteArr.SubArray(8 + (int)colorMsgLength));
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
            return this.info as byte [];
        }
    }
}
