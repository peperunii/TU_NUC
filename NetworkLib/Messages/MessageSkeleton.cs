using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public class MessageSkeleton : Message
    {
        public DeviceID deviceID;

        public MessageSkeleton()
        {
            this.type = MessageType.Skeleton;
            this.info = new byte[] { };
        }

        public MessageSkeleton(DeviceID deviceID, List<Body> bodies)
        {
            this.type = MessageType.Skeleton;
            this.info = bodies;
            this.deviceID = deviceID;
        }

        public MessageSkeleton(byte[] bodiesByteArr)
        {
            this.type = MessageType.Skeleton;
            this.deviceID = (DeviceID)BitConverter.ToUInt16(bodiesByteArr, 0);
            this.info = this.ConvertByteArrToBodyList(bodiesByteArr.SubArray(2));
        }

        public byte [] ConvertBodiesListToByteArr()
        {
            Console.WriteLine("Skeleton serialization");
            var ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(ms, this.info as List<Body>);
            Console.WriteLine("Skeleton serialization 2");

            ms.Position = 0;

            var byteArr = ms.ToArray();
            Console.WriteLine("Skeleton byte length: " + byteArr.Length);
            return ms.ToArray();
        }

        public List<Body> ConvertByteArrToBodyList(byte [] byteArr)
        {
            var stream = new MemoryStream(byteArr);
            BinaryFormatter bf = new BinaryFormatter();

            stream.Position = 0;
            var listBodies = bf.Deserialize(stream) as List<Body>;
            stream.Close();

            return listBodies;
        }

        public override byte[] Serialize()
        {
            var bytes = this.GetBytesForNumberShort((ushort)this.type);
            var bytesDeviceID = BitConverter.GetBytes((ushort)this.deviceID);
            var bytesInfo = bytesDeviceID.Concat(this.ConvertBodiesListToByteArr()).ToArray();
            var lenghtInfoBytes = this.GetBytesForNumberInt(bytesInfo.Length);

            var result = bytes.Concat(lenghtInfoBytes.Concat(bytesInfo)).ToArray();

            return result;
        }
    }
}
