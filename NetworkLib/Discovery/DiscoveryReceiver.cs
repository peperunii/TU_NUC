using Network;
using Network.Events;
using Network.Logger;
using Network.Messages;
using NetworkLib.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
                Console.WriteLine("1: " + ex.ToString());
                LogManager.LogMessage(LogType.Error, ex.ToString());
            }
        }

        private void SendServerFound(TcpNetworkListener tcpServer, IPAddress deviceIP)
        {
            var ipAdd = Configuration.DeviceIP.GetAddressBytes();
            var port = BitConverter.GetBytes(tcpServer.Port);

            var infoBytes = new byte[] { ipAdd[0], ipAdd[1], ipAdd[2], ipAdd[3], port[0], port[1] };
            
            var msg = new MessageDiscoveryResponse()
            {
                IP = Configuration.DeviceIP.GetAddressBytes(),
                Port = port,
                info = infoBytes
            };
            Console.WriteLine("Type: " + msg.type);

            byte[] bytes = msg.Serialize();
            
            udp.Send(
                bytes, 
                bytes.Length, 
                new IPEndPoint(
                    deviceIP,//IPAddress.Parse(tcpServer.IpAddress), 
                    Configuration.DiscoveryPort));
        }

        public void SendDeviceStart(TcpNetworkListener tcpServer, IPAddress deviceIP)
        {
            SendServerFound(tcpServer, deviceIP);
        }
    }
}
