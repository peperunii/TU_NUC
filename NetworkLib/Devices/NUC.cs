using Network;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Network.Devices
{
    public class NUC
    {
        public DeviceID deviceID;
        public IPEndPoint ip;
        public string config;

        public NUC(DeviceID deviceID, IPEndPoint ipEndPoint)
        {
            this.deviceID = deviceID;
            this.ip = ipEndPoint;

            this.config = string.Empty;
        }

        public override bool Equals(object obj)
        {
            var item = obj as NUC;

            if (item == null)
            {
                return false;
            }

            return this.deviceID == item.deviceID;
        }

        public override int GetHashCode()
        {
            return this.deviceID.GetHashCode();
        }

        public byte [] Serialize()
        {
            var deviceID_byteArr = BitConverter.GetBytes((ushort)this.deviceID);
            var ip_byteArr = Encoding.ASCII.GetBytes(this.ip.ToString());
            var lengthIP = ip_byteArr.Length;
            var lengthIP_byteArr = BitConverter.GetBytes((UInt16)lengthIP);
            var length_byteArr = BitConverter.GetBytes((uint)(deviceID_byteArr.Length + lengthIP_byteArr.Length + ip_byteArr.Length));
            
            return length_byteArr.Concat(deviceID_byteArr.Concat(lengthIP_byteArr.Concat(ip_byteArr))).ToArray(); 
        }
    }
}
