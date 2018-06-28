using Network.Devices;
using Network.Discovery;
using Network.Logger;
using Network.Messages;
using Network.TCP;
using Network.TimeSync;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NUC_Controller.NetworkWorker
{
    static class Worker
    {
        private static List<NUC> connectedDevices;

        public static void StartNetworkClient()
        {
            NetworkSettings.networkWorker = new DiscoverySender();
            NetworkSettings.networkWorker.ServerFound += NetworkWorker_ServerFound;
            NetworkSettings.networkWorker.FindServer();
        }

        private static void NetworkWorker_ServerFound(object source, Network.Events.ServerFoundEventArgs e)
        {
            Globals.IsConnectedToServer = true;
            NetworkSettings.connectionSettings = new ConnectionSettings(e.Address.ToString(), e.Port);

            NetworkSettings.tcpClient = new TcpNetworkClient(NetworkSettings.connectionSettings.Address, NetworkSettings.connectionSettings.Port, "controller");
            NetworkSettings.tcpClient.OnMessage += TcpClient_OnMessage;
            NetworkSettings.tcpClient.OnServerDisconnected += TcpClient_OnServerDisconnected;
            NetworkSettings.tcpClient.Connect();

            /*Synchronize Time*/
            NetworkSettings.tcpClient.Send(new MessageTimeSync());
            NetworkSettings.tcpClient.Send(new MessageGetConnectedClients());
        }

        private static void TcpClient_OnServerDisconnected()
        {
            Globals.IsConnectedToServer = false;
            if (NetworkSettings.networkWorker != null)
            {
                NetworkSettings.networkWorker.CloseConnection();
            }

            NetworkSettings.networkWorker = new DiscoverySender();
            NetworkSettings.networkWorker.ServerFound += NetworkWorker_ServerFound;
            NetworkSettings.networkWorker.FindServer();
        }

        public static void SendMessage(Message message)
        {
            NetworkSettings.tcpClient.Send(message);
        }

        private static void TcpClient_OnMessage(Network.Messages.Message message)
        {
            LogManager.LogMessage(LogType.Info, "Received message: " + message.type);
            switch (message.type)
            {
                case MessageType.RestartClientApp:
                    Globals.RestartApp();
                    break;

                case MessageType.RestartClientDevice:
                    ProcessStartInfo proc = new ProcessStartInfo();
                    proc.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.FileName = "cmd";
                    proc.Arguments = "/C shutdown -f -r";
                    Process.Start(proc);
                    break;

                case MessageType.TimeInfo:
                    ServerTime.Set(DateTime.FromFileTimeUtc((long)message.info));
                    LogManager.LogMessage(LogType.Info, "Sync Date/Time...");
                    break;

                case MessageType.ReloadConfiguration:
                    Globals.ReloadConfiguration();
                    break;

                case MessageType.KeepAlive:
                    break;

                case MessageType.ConnectedClients:
                    Worker.connectedDevices = message.info as List<NUC>;
                    break;

            }
        }

        public static List<NUC> GetConnectedDevices()
        {
            return Worker.connectedDevices;
        }
    }
}
