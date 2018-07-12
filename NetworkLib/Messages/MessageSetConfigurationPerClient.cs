using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network.Utils;

namespace Network.Messages
{
    public class MessageSetConfigurationPerClient : Message
    {
        public DeviceID deviceId;

        public MessageSetConfigurationPerClient()
        {
            this.type = MessageType.ShowConfigurationPerClient;
            this.info = "";
        }

        public MessageSetConfigurationPerClient(DeviceID deviceId, string configuration)
        {
            this.type = MessageType.ShowConfigurationPerClient;
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
            return this.GetBytesForNumberShort((ushort)this.deviceId).ConcatenatingArrays( Encoding.ASCII.GetBytes(this.info as string));
        }
    }
}
