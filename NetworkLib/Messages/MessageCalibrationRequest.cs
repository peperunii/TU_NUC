﻿using Network.Utils;
using System;

namespace Network.Messages
{
    public class MessageCalibrationRequest : Message
    {
        public DeviceID deviceId;

        public MessageCalibrationRequest()
        {
            this.type = MessageType.CalibrationRequest;
            this.info = new byte[] { };
        }

        public MessageCalibrationRequest(DeviceID deviceID)
        {
            this.type = MessageType.CalibrationRequest;
            this.deviceId = deviceID;
            this.info = BitConverter.GetBytes((ushort)deviceID);
        }

        public MessageCalibrationRequest(byte[] infoBytes)
        {
            this.type = MessageType.CalibrationRequest;
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
