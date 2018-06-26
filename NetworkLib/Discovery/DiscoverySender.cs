using Network.Events;
using Network.Logger;
using Network.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Network.Discovery
{
    public class DiscoverySender
    {
        public UDPClient client { get; private set; }

        public delegate void MyEventHandler(object source, ServerFoundEventArgs e);
        public event MyEventHandler ServerFound;

        public void FindServer()
        {
            this.client = new UDPClient(new IPEndPoint(Configuration.DeviceIP, Configuration.DiscoveryPort));
            var messageDiscovery = new MessageDiscovery().Serialize();

            client.Send(
                messageDiscovery, 
                messageDiscovery.Length, 
                new IPEndPoint(IPAddress.Broadcast, Configuration.DiscoveryPort));
            
            StartListening();
        }

        public void StartListening()
        {
            this.client.BeginReceive(Receive, new object());
        }

        public void CloseConnection()
        {
            this.client.Close();
        }

        private void Receive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, Configuration.DiscoveryPort);
                byte[] bytes = client.EndReceive(ar, ref ip);
                var message = MessageParser.GetMessageFromBytArr(bytes);
                
                switch (message.type)
                {
                    case MessageType.DiscoveryResponse:
                
                        if (ServerFound != null)
                        {
                            ServerFound.Invoke(null, new ServerFoundEventArgs(ip.Address, ip.Port));
                        }

                        LogManager.LogMessage(
                            LogType.Info, 
                            String.Format("Server found {0}", ip.Address.ToString()));
                        break;
                    default:
                        break;
                }

                //again
                //CloseConnection();
                StartListening();
            }
            catch (Exception ex)
            {

            }
        }

        public static IEnumerable<IPAddress> GetLocalIPAddresses()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    yield return ip;
                }
            }
        }
    }
}
