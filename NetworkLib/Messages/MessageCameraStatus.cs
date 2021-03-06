﻿using Network.Utils;
using System;

namespace Network.Messages
{
    public class MessageCameraStatus : Message
    {
        public DeviceID deviceID;

        public bool isCameraStarted;
        public bool isColorStreamEnabled;
        public bool isDepthStreamEnabled;
        public bool isIRStreamEnabled;
        public bool isBodyStreamEnabled;

        public MessageCameraStatus()
        {
            this.type = MessageType.CameraStatus;
            this.info = "";
        }

        public MessageCameraStatus(
            DeviceID deviceID,
            bool isCameraStarted,
            bool isColorStreamEnabled,
            bool isDepthStreamEnabled,
            bool isIRStreamEnabled,
            bool isBodyStreamEnabled)
        {
            this.type = MessageType.CameraStatus;

            this.deviceID = deviceID;
            this.isCameraStarted = isCameraStarted;
            this.isColorStreamEnabled = isColorStreamEnabled;
            this.isDepthStreamEnabled = isDepthStreamEnabled;
            this.isIRStreamEnabled = isIRStreamEnabled;
            this.isBodyStreamEnabled = isBodyStreamEnabled;

            this.info = this.GetBytesOfInfo();
        }

        public MessageCameraStatus(byte [] byteArr)
        {
            this.type = MessageType.CameraStatus;
            this.info = byteArr;
            this.ParseByteArr();
        }

        private void ParseByteArr()
        {
            var byteArr = this.info as byte[];
            this.deviceID = (DeviceID)BitConverter.ToUInt16(byteArr, 0);
            this.isCameraStarted = BitConverter.ToBoolean(byteArr, 2);
            this.isColorStreamEnabled = BitConverter.ToBoolean(byteArr, 3);
            this.isDepthStreamEnabled = BitConverter.ToBoolean(byteArr, 4);
            this.isIRStreamEnabled = BitConverter.ToBoolean(byteArr, 5);
            this.isBodyStreamEnabled = BitConverter.ToBoolean(byteArr, 6);
        }

        public override byte[] Serialize()
        {
            var bytes = this.GetBytesForNumberShort((ushort)this.type);
            var lenghtInfoBytes = this.GetBytesForNumberInt((int)(this.info as byte []).Length);

            return bytes.ConcatenatingArrays(lenghtInfoBytes.ConcatenatingArrays(this.info as byte []));
        }

        private byte[] GetBytesOfInfo()
        {
            var array = this.GetBytesForNumberShort((ushort)this.deviceID);
            array = array.ConcatenatingArrays(BitConverter.GetBytes(this.isCameraStarted));
            array = array.ConcatenatingArrays(BitConverter.GetBytes(this.isColorStreamEnabled));
            array = array.ConcatenatingArrays(BitConverter.GetBytes(this.isDepthStreamEnabled));
            array = array.ConcatenatingArrays(BitConverter.GetBytes(this.isIRStreamEnabled));
            array = array.ConcatenatingArrays(BitConverter.GetBytes(this.isBodyStreamEnabled));

            return array;
        }
    }
}
