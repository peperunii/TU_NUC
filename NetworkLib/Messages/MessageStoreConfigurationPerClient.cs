using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network.Utils;

namespace Network.Messages
{
    public class MessageStoreConfigurationPerClient : Message
    {
        public DeviceID deviceId;

        public MessageStoreConfigurationPerClient()
        {
            this.type = MessageType.StoreConfigurationPerClient;
            this.info = "";
        }

        public MessageStoreConfigurationPerClient(DeviceID deviceId, string configuration)
        {
            this.type = MessageType.StoreConfigurationPerClient;
            this.info = configuration;
            this.deviceId = deviceId;
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
            return this.GetBytesForNumberShort((ushort)this.deviceId).ConcatenatingArrays(Encoding.ASCII.GetBytes(this.info as string));
        }
    }
}
