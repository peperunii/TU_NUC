using Network;
using Network.Discovery;
using Network.Logger;
using Network.Messages;
using NetworkLib.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public static class ServerActions
    {
        public static List<NUC> Devices { get; set; }
        private static DiscoveryReceiver discoveryServer;
        private static TcpNetworkListener tcpServer;

        public static void Listener()
        {
            Devices = new List<NUC>();

            StartDiscoveryServer();
            StartTcpServer();

            Global.EventDispatcher.AdminStartDiscovery += EventDispatcher_AdminStartDiscovery;

            while (true)
            {
                /*do not let the program to exit*/
                Console.ReadKey();
                Thread.Sleep(1);
            }
        }

        private static void StartTcpServer()
        {
            tcpServer = new TcpNetworkListener(Configuration.DeviceIP.ToString(), Configuration.CommunicationPort, "Server");
            tcpServer.OnMessage += Server_OnMessage;
            tcpServer.Connect();
        }

        private static void Server_OnMessage(object xSender, Message message)
        {
            
        }

        public static void StartDiscoveryServer()
        {
            discoveryServer = new DiscoveryReceiver(Configuration.DeviceIP.ToString(), Configuration.DiscoveryPort);
            discoveryServer.DeviceConnected += DiscoveryServer_DeviceConnected;
            discoveryServer.StartListening();
        }

        private static void DiscoveryServer_DeviceConnected(object source, Network.Events.NucConnectedEventArgs e)
        {
            //if (_wssv != null && _wssv.IsListening)
            //{
            //    var msg = new AdminEvent
            //    {
            //        messageType = AdminEvent.Type.RPI_CLIENTS_CHANGED,
            //        rpiClientChanged = new RpiClientsChanged(),
            //    };
            //
            //    foreach (var item in e.ConnectedAddresses)
            //    {
            //        msg.rpiClientChanged.rpiClients.Add(new RpiClient
            //        {
            //            piId = item.Key,
            //            ipAddress = item.Value.ToString()
            //        });
            //        var existingRpiID = (from t in this.devices
            //                             where t.piID == item.Key
            //                             select t).FirstOrDefault();
            //        if (existingRpiID == null)
            //        {
            //            this.devices.Add(new CameraInfo(item.Key, item.Value.ToString()));
            //        }
            //        else
            //        {
            //            existingRpiID.RaspberryAddress = item.Value.ToString();
            //        }
            //    }
            //
            //    byte[] bytes = ProtobufHelper.Serialize(msg);
            //
            //    _wssv.WebSocketServices[ServerConfig.Default.WebPathAdmin].Sessions.Broadcast(bytes);
            //}

            LogManager.LogMessage(LogType.Info, "Device Found");
            SendRpiStart();
        }


        private static void SendRpiStart()
        {
            LogManager.LogMessage(LogType.Info, "Device Start");
            discoveryServer.SendRpiStart();
        }

        public static void DiscoverConnectedDevices()
        {
            Devices.Clear();
            StartDiscoveryServer();
        }

        private static void EventDispatcher_AdminStartDiscovery(object sender, EventArgs e)
        {
            
        }
    }
}
