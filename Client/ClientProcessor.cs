using Client.Cameras;
using Network;
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
using System.Windows.Forms;

namespace Client
{
    public static class ClientProcessor
    {
        private static DiscoverySender networkWorker;
        private static TcpNetworkClient tcpClient;
        private static ConnectionSettings connectionSettings;
        private static long lastServerHeartbeat;
        private static KinectCamera camera;

        public static void Start()
        {
            networkWorker = new DiscoverySender();
            networkWorker.ServerFound += NetworkWorker_ServerFound;
            networkWorker.FindServer();

            camera = new KinectCamera();
            camera.OnColorFrameArrived += Camera_OnColorFrameArrived;
            camera.OnDepthFrameArrived += Camera_OnDepthFrameArrived;
            camera.OnIRFrameArrived += Camera_OnIRFrameArrived;
            camera.OnBodyFrameArrived += Camera_OnBodyFrameArrived;

            while (true)
            {
                /*do not let the program to exit*/
                Console.ReadKey();
                Thread.Sleep(1);
            }
        }
        
        /*Camera Events*/

        private static void Camera_OnColorFrameArrived()
        {
            var colorData = camera.GetData(CameraDataType.Color);
        }

        private static void Camera_OnDepthFrameArrived()
        {
            var depthData = camera.GetData(CameraDataType.Depth);
        }
        
        private static void Camera_OnIRFrameArrived()
        {
            var irData = camera.GetData(CameraDataType.IR);
        }
        private static void Camera_OnBodyFrameArrived()
        {
            var bodyData = camera.GetData(CameraDataType.Body);
        }
        

        private static void NetworkWorker_ServerFound(object source, Network.Events.ServerFoundEventArgs e)
        {
            connectionSettings = new ConnectionSettings(e.Address.ToString(), e.Port);

            tcpClient = new TcpNetworkClient(connectionSettings.Address, connectionSettings.Port, "nuc");
            tcpClient.OnMessage += TcpClient_OnMessage;
            tcpClient.OnServerDisconnected += TcpClient_OnServerDisconnected;
            tcpClient.Connect();

            /*Synchronize Time*/
            tcpClient.Send(new MessageTimeSync());
        }

        private static void TcpClient_OnServerDisconnected()
        {
            if (networkWorker != null)
            {
                networkWorker.CloseConnection();
            }

            networkWorker = new DiscoverySender();
            networkWorker.ServerFound += NetworkWorker_ServerFound;
            networkWorker.FindServer();
        }

        private static void TcpClient_OnMessage(Network.Messages.Message message)
        {
            LogManager.LogMessage(LogType.Info, "Received message: " + message.type);
            switch (message.type)
            {
                case MessageType.RestartClientApp:
                    Global.RestartApp();
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
                    Global.ReloadConfiguration();
                    break;

                case MessageType.KeepAlive:
                    lastServerHeartbeat = DateTime.Now.ToFileTimeUtc();
                    break;
            }
        }
    }
}
