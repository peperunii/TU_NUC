﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageRestartClientDevice : Message
    {
        private DeviceID deviceId;

        public MessageRestartClientDevice()
        {
            this.type = MessageType.RestartClientDevice;
            this.info = new byte[] { };
        }

        public MessageRestartClientDevice(DeviceID deviceID)
        {
            this.type = MessageType.RestartClientDevice;
            this.deviceId = deviceID;
            this.info = BitConverter.GetBytes((ushort)deviceID);
        }

        public MessageRestartClientDevice(byte[] infoBytes)
        {
            this.type = MessageType.RestartClientDevice;
            this.info = infoBytes;

            this.deviceId = (DeviceID)BitConverter.ToUInt16(infoBytes, 0);
        }

        public override byte[] Serialize()
        {
            var bytes = this.GetBytesForNumberShort((ushort)this.type);
            var bytesInfo = this.GetBytesOfInfo();
            var lenghtInfoBytes = this.GetBytesForNumberInt((int)bytesInfo.Length);

            return bytes.Concat(lenghtInfoBytes.Concat(bytesInfo)).ToArray();
        }

        private byte[] GetBytesOfInfo()
        {
            return this.info as byte[];
        }
    }
}
