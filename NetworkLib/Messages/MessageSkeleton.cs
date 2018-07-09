using Microsoft.Kinect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
         
            return json;
        }

        public List<Skeleton> ConvertByteArrToBodyList(byte [] byteArr)
        {
            var listBodies = new List<Skeleton>();
            var jObj = JObject.Parse(Encoding.ASCII.GetString(byteArr))["Bodies"];

            var children = jObj.Children();

            foreach(var body in jObj)
            {
                var skeleton = new Skeleton();
                var joints = body["Joints"];
                
                foreach(var joint in joints)
                {
                    var jointType = joint["JointType"];
                    var trackState = joint["TrackingState"];
                    var position = joint["Position"];

                    var jointObj = new Joint();
                    jointObj.JointType = (JointType)Enum.Parse(typeof(JointType), (string)jointType);
                    jointObj.TrackingState = (TrackingState)Enum.Parse(typeof(TrackingState), (string)trackState);
                    jointObj.Position = new CameraSpacePoint();
                    jointObj.Position.X = float.Parse((string)position["X"]);
                    jointObj.Position.Y = float.Parse((string)position["Y"]);
                    jointObj.Position.Z = float.Parse((string)position["Z"]);

                    skeleton.AddJoint(jointObj);
                }

                listBodies.Add(skeleton);
            }
            
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

            Console.WriteLine(".............");
            Console.WriteLine("json body: " + json.ToString());
            Console.WriteLine(".............");
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

    public class Skeleton
    {
        public List<Joint> Joints;
        public bool IsTracked;

        public Skeleton()
        {
            this.Joints = new List<Joint>();
        }

        public void AddJoint(Joint joint)
        {
            this.Joints.Add(joint);
        }
    }
}
