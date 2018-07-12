using Network.Devices;
using Network.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Network.Messages
{
    public class MessageConnectedClients : Message
    {
        public MessageConnectedClients()
        {
            this.type = MessageType.ConnectedClients;
            this.info = new List<NUC>();
        }

        public MessageConnectedClients(byte [] byteArr)
        {
            this.type = MessageType.ConnectedClients;
            this.info = this.GetDevicesFromByteArr(byteArr);
        }

        public MessageConnectedClients(List<NUC> devices)
        {
            this.type = MessageType.ConnectedClients;
            this.info = devices.ToList();

            var portWithUI = (from t in devices
                              where t.deviceID == DeviceID.Controller
                              select t.ip.Port).FirstOrDefault();
            (this.info as List<NUC>).Add(new NUC(DeviceID.TU_SERVER, new IPEndPoint(Configuration.DeviceIP, portWithUI)));
        }

        public override byte[] Serialize()
        {
            var infoBytes = this.GetByteArrFromInfo(this.info as List<NUC>);
            var lengthInfoBytes = BitConverter.GetBytes((UInt32)infoBytes.Length);

            return this.GetBytesForNumberShort((ushort)this.type).ConcatenatingArrays(lengthInfoBytes.ConcatenatingArrays(infoBytes));
        }

        private List<NUC> GetDevicesFromByteArr(byte [] byteArr)
        {
            var devices = new List<NUC>();

            for (int i = 0; i < byteArr.Length;)
            {
                var deviceByteLength =
                    BitConverter.ToUInt32(
                        new byte[] { byteArr[i], byteArr[i + 1], byteArr[i + 2], byteArr[i + 3] }, 0);

                i += 4;

                var pointer = 0;

                var deviceID = (DeviceID)BitConverter.ToUInt16(new byte[] { byteArr[i + pointer], byteArr[i + pointer + 1] }, 0);
                pointer += 2;

                var socketLength = BitConverter.ToUInt16(new byte[] { byteArr[i + pointer], byteArr[i + pointer + 1] }, 0);
                pointer += 2;

                var socket = Encoding.ASCII.GetString(byteArr.SubArray(i + pointer, socketLength));
                var ipEndPoint = this.GetIpEndPointFromSocket(socket);
                pointer += socket.Length;

                i += (int)deviceByteLength;

                devices.Add(new NUC(deviceID, ipEndPoint));
            }

            return devices;
        }

        private IPEndPoint GetIpEndPointFromSocket(string socket)
        {
            var indexOfDots = socket.IndexOf(':');

            if(indexOfDots != -1)
            {
                var ipStr = socket.Substring(0, indexOfDots);
                var portStr = socket.Substring(indexOfDots + 1);

                var ipInfo = ipStr.Split('.');
                var ipAddress = new IPAddress(new byte[] { byte.Parse(ipInfo[0]), byte.Parse(ipInfo[1]), byte.Parse(ipInfo[2]), byte.Parse(ipInfo[3])});

                var port = int.Parse(portStr);

                return new IPEndPoint(ipAddress, port);
            }
            else
            {
                return new IPEndPoint(new IPAddress(new byte[] { 0,0,0,0 }), 0);
            }
        }

        private byte [] GetByteArrFromInfo(List<NUC> devices)
        {
            var result = new List<byte>();
            foreach(var device in devices)
            {
                result = result.Concat(device.Serialize()).ToList();
            }

            return result.ToArray();
        }
    }
}
