using Network;
using Network.Events;
using Network.Logger;
using Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network.Discovery
{
    public class DiscoveryReceiver
    {
        private readonly UdpClient udp = new UdpClient(Configuration.DiscoveryPort);
        public Dictionary<DeviceID, IPEndPoint> connectedAddresses;
        private string tcpServerIP;
        private int tcpServerPort;

        public delegate void MyEventHandler(object source, NucConnectedEventArgs e);
        public event MyEventHandler DeviceConnected;

        public DiscoveryReceiver(string tcpServerIP, int tcpServerPort)
        {
            this.tcpServerIP = tcpServerIP;
            this.tcpServerPort = tcpServerPort;
            this.connectedAddresses = new Dictionary<DeviceID, IPEndPoint>();
        }

        public void StartListening()
        {
            this.udp.BeginReceive(Receive, new object());
        }

        public void CloseConnection()
        {
            this.udp.Close();
        }
        private void Receive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, Configuration.DiscoveryPort);
                byte[] bytes = udp.EndReceive(ar, ref ip);
                var message = MessageParser.GetMessageFromBytArr(bytes);

                Console.WriteLine("Received message: " + message.type);
                switch (message.type)
                {
                    case MessageType.Discovery:
                        var deviceId = (DeviceID)(short)message.info;
                        if (this.connectedAddresses.ContainsKey(deviceId))
                        {
                            LogManager.LogMessage(
                                LogType.Warning, 
                                string.Format(
                                    "Device {0}: Reconnecting ...",
                                    deviceId));

                            this.connectedAddresses.Remove(deviceId);
                        }

                        this.connectedAddresses.Add((DeviceID)(short)message.info, ip);

                        SendServerFound(ip);

                        if (DeviceConnected != null)
                        {
                            DeviceConnected.Invoke(null, new NucConnectedEventArgs(connectedAddresses, connectedAddresses.Count - 1));
                        }

                        LogManager.LogMessage(LogType.Info, String.Format("Server found by client {0}", ip.Address.ToString()));
                        
                        break;
                    default:
                        break;
                }

                //again
                StartListening();
            }
            catch (Exception ex)
            {

            }
        }

        private void SendServerFound(IPEndPoint ip)
        {
            var ipAdd = Configuration.DeviceIP.GetAddressBytes();
            var port = BitConverter.GetBytes(Configuration.DiscoveryPort);

            var infoBytes = new byte[] { ipAdd[0], ipAdd[1], ipAdd[2], ipAdd[3], port[0], port[1] };

            Console.WriteLine("Sending Discovery Response");
            var msg = new MessageDiscoveryResponse()
            {
                IP = Configuration.DeviceIP.GetAddressBytes(),
                Port = port,
                info = infoBytes
            };
            Console.WriteLine("Type: " + msg.type);

            byte[] bytes = msg.Serialize();

            udp.Send(bytes, bytes.Length, ip);
        }

        public void SendRpiStart()
        {
            //foreach (var rPi in connectedAddresses)
            //{
            //    UdpDiscovery msg = new UdpDiscovery
            //    {
            //        messageType = UdpDiscovery.Type.RPI_START,
            //        sessionId = ProtobufHelper.generateUUID(),
            //    };
            //
            //    byte[] bytes = ProtobufHelper.Serialize(msg);
            //
            //    udp.Send(bytes, bytes.Length, rPi.Value);
            //
            //    Console.WriteLine("Rpi Start {0}", rPi.Value);
            //}
            ////UdpDiscovery msg = new UdpDiscovery
            ////{
            ////    messageType = UdpDiscovery.Type.RPI_START,
            ////    sessionId = ProtobufHelper.generateUUID(),
            ////};
            //
            ////byte[] bytes = ProtobufHelper.Serialize(msg);
            //
            ////IPEndPoint targetEndPoint = new IPEndPoint(IPAddress.Broadcast, Global.DISCOVERY_PORT);
            //
            ////udp.Send(bytes, bytes.Length, targetEndPoint);
        }
    }
}
