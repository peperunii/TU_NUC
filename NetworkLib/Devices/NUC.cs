using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Network.Devices
{
    public class ConfigParam
    {
        public string Param { get; set; }
        public string Value { get; set; }

        public ConfigParam(string param, string value)
        {
            this.Param = param;
            this.Value = value;
        }
    }

    public class NUC
    {
        public DeviceID deviceID;
        public IPEndPoint ip;
        public string config;
        public List<ConfigParam> configDict;

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

        public void SetConfiguration(string config)
        {
            this.configDict = new List<ConfigParam>();
            this.config = config;
            var lines = this.config.Split('\n');

            foreach (var line in lines)
            {
                try
                {
                    var parts = line.Split(':');
                    var part1 = parts[0].Trim();
                    var part2 = parts[1].Trim();

                    this.configDict.Add(new ConfigParam(part1, part2));
                }
                catch(Exception)
                {
                    this.configDict.Add(new ConfigParam(string.Empty, string.Empty));
                }
            }
        }
    }
}
