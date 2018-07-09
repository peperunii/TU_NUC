using Microsoft.Kinect;
using Newtonsoft.Json;
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
            var json = Encoding.ASCII.GetBytes(SerializationExtensions.Serialize(this.info as List<Body>));
            Console.WriteLine("Skeleton serialization");

            return json;
        }

        public List<Body> ConvertByteArrToBodyList(byte [] byteArr)
        {
            var reader = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(byteArr));
            var bodies = reader["Bodies"];
            Console.WriteLine(bodies);

            return new List<Body>();
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

    /// <summary>
    /// Provides some common functionality for serializing and deserializing body data to JSON.
    /// </summary>
    public static class SerializationExtensions
    {
        #region Serialization

        /// <summary>
        /// Serializes the current collection of <see cref="Body"/>.
        /// </summary>
        /// <param name="bodies">The body collection to serialize.</param>
        /// <returns>A JSON representation of the current body collection.</returns>
        public static string Serialize(this IEnumerable<Body> bodies)
        {
            StringBuilder json = new StringBuilder();

            json.Append("{");
            json.Append("\"Bodies\":");

            if (bodies != null)
            {
                json.Append("[");

                foreach (Body body in bodies)
                {
                    json.Append(body.Serialize() + ",");
                }

                json.Append("]");
                json.Append("}");
            }

            return json.ToString();
        }

        /// <summary>
        /// Serializes the current <see cref="Body"/>.
        /// </summary>
        /// <param name="body">The body to serialize.</param>
        /// <returns>A JSON representation of the current body.</returns>
        public static string Serialize(this Body body)
        {
            StringBuilder json = new StringBuilder();

            if (body != null)
            {
                json.Append("{");
                json.Append("\"TrackingId\":\"" + body.TrackingId + "\",");
                json.Append("\"IsTracked\":\"" + body.IsTracked + "\",");
                json.Append("\"LeanTrackingState\":\"" + body.LeanTrackingState + "\",");
                json.Append("\"HandLeftConfidence\":\"" + body.HandLeftConfidence + "\",");
                json.Append("\"HandLeftState\":\"" + body.HandLeftState + "\",");
                json.Append("\"HandRightConfidence\":\"" + body.HandRightConfidence + "\",");
                json.Append("\"HandRightState\":\"" + body.HandRightState + "\",");
                json.Append("\"Joints\":[");

                foreach (Joint joint in body.Joints.Values)
                {
                    json.Append(joint.Serialize() + ",");
                }

                json.Append("]");
                json.Append("}");
            }

            return json.ToString();
        }

        /// <summary>
        /// Serializes the current <see cref="Joint"/>.
        /// </summary>
        /// <param name="joint">The joint to serialize.</param>
        /// <returns>A JSON representation of the current joint.</returns>
        public static string Serialize(this Joint joint)
        {
            StringBuilder json = new StringBuilder();

            json.Append("{");
            json.Append("\"JointType\":\"" + joint.JointType + "\",");
            json.Append("\"TrackingState\":\"" + joint.TrackingState + "\",");
            json.Append("\"Position\":{");
            json.Append("\"X\":\"" + joint.Position.X + "\",");
            json.Append("\"Y\":\"" + joint.Position.Y + "\",");
            json.Append("\"Z\":\"" + joint.Position.Z + "\"");
            json.Append("}");
            json.Append("}");

            return json.ToString();
        }

        #endregion
    }
}
