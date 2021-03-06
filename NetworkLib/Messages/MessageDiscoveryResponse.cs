﻿using Network.Utils;
using System;

namespace Network.Messages
{
    public class MessageDiscoveryResponse : Message
    {
        public byte[] IP;
        public byte[] Port;

        public MessageDiscoveryResponse()
        {
            /*6 byte information - 4 for IP address and 2 for port*/
            this.type = MessageType.DiscoveryResponse;
            IP = Configuration.DeviceIP.GetAddressBytes();
            Port = this.GetBytesForNumberShort(Configuration.DiscoveryPort);

            this.info = new byte[] { IP[0], IP[1], IP[2], IP[3], Port[0], Port[1] };
        }

        public MessageDiscoveryResponse(byte[] infoBytes)
        {
            try
            {
                this.type = MessageType.DiscoveryResponse;
                this.info = infoBytes;

                this.IP = new byte[4] { infoBytes[0], infoBytes[1], infoBytes[2], infoBytes[3] };
                this.Port = new byte[2] { infoBytes[4], infoBytes[5] };
            }
            catch(Exception ex)
            {

            }
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
