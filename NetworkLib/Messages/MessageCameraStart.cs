﻿using Network.Utils;
using System;

namespace Network.Messages
{
    public class MessageCameraStart : Message
    {
        public DeviceID deviceId;


        public MessageCameraStart()
        {
            this.type = MessageType.CameraStart;
            this.info = new byte[] { };
        }

        public MessageCameraStart(DeviceID deviceID)
        {
            this.type = MessageType.CameraStart;
            this.deviceId = deviceID;
            this.info = BitConverter.GetBytes((ushort)deviceID);
        }

        public MessageCameraStart(byte[] infoBytes)
        {
            this.type = MessageType.CameraStart;
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
