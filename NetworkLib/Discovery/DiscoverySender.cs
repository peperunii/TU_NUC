using Network.Events;
using Network.Logger;
using Network.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Network.Discovery
{
    public class DiscoverySender
    {
        public UDPClient client { get; private set; }

        public delegate void MyEventHandler(object source, ServerFoundEventArgs e);
        public event MyEventHandler ServerFound;

        private bool isServerFound = false;

        public void FindServer()
        {
            this.client = new UDPClient(new IPEndPoint(Configuration.DeviceIP, Configuration.DiscoveryPort));
            var messageDiscovery = new MessageDiscovery().Serialize();
            
            while(!isServerFound)
            {
                client.Send(
                    messageDiscovery,
                    messageDiscovery.Length,
                    new IPEndPoint(IPAddress.Broadcast, Configuration.DiscoveryPort));

                StartListening();

                Thread.Sleep(1000);
            }
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

                        isServerFound = true;
                        if (ServerFound != null)
                        {
                            var port = BitConverter.ToUInt16(new byte[] { (message.info as byte [])[4], (message.info as byte[])[5] }, 0);
                            ServerFound.Invoke(null, new ServerFoundEventArgs(ip.Address, port));

                            LogManager.LogMessage(
                                LogType.Info,
                                LogLevel.Communication,
                                "Communicating with server on Port: " + port);
                        }

                        LogManager.LogMessage(
                            LogType.Info,
                            LogLevel.Communication,
                            String.Format("Server found {0}", ip.Address.ToString()));
                        break;
                    default:
                        break;
                }

                Thread.Sleep(1);
                StartListening();
            }
            catch (Exception ex)
            {
                LogManager.LogMessage(
                    LogType.Error,
                    LogLevel.Errors,
                    ex.ToString());
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
