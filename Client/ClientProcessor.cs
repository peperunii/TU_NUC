using Client.Cameras;
using Network;
using Network.Discovery;
using Network.Logger;
using Network.Messages;
using Network.TCP;
using Network.TimeSync;
using System;
using System.Diagnostics;
using System.Threading;

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

            CameraInit();

            while (true)
            {
                /*do not let the program to exit*/
                Console.ReadKey();
                Thread.Sleep(1);
            }
        }
        
        /*Camera Events*/
        private static void CameraInit()
        {
            camera = new KinectCamera();
            camera.SetScaleFactor(CameraDataType.Color, Configuration.colorFrameScale);
            camera.SetScaleFactor(CameraDataType.Depth, Configuration.depthFrameScale);
            camera.SetScaleFactor(CameraDataType.IR, Configuration.irFrameScale);

            camera.SetDepthMinReliable(Configuration.minReliableDepth);
            camera.SetDepthMaxReliable(Configuration.maxReliableDepth);

            camera.OnColorFrameArrived += Camera_OnColorFrameArrived;
            camera.OnDepthFrameArrived += Camera_OnDepthFrameArrived;
            camera.OnIRFrameArrived += Camera_OnIRFrameArrived;
            camera.OnBodyFrameArrived += Camera_OnBodyFrameArrived;
        }

        private static int frameCounter = 0;
        private static void Camera_OnColorFrameArrived()
        {
            if (!Configuration.IsServerDisconnected)
            {
                LogManager.LogMessage(LogType.Info, "Sending color frame...");
                var colorData = camera.GetData(CameraDataType.Color);
                var messageColorFrame = new MessageColorFrame(1080, 1920, 3, false, (colorData as byte[]));
                tcpClient.Send(messageColorFrame);
                frameCounter++;
            }
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

            //tcpClient.Send(new MessageSkeleton);
        }
        

        private static void NetworkWorker_ServerFound(object source, Network.Events.ServerFoundEventArgs e)
        {
            connectionSettings = new ConnectionSettings(e.Address.ToString(), e.Port);

            tcpClient = new TcpNetworkClient(connectionSettings.Address, connectionSettings.Port, "nuc");
            tcpClient.OnMessage += TcpClient_OnMessage;
            tcpClient.OnServerDisconnected += TcpClient_OnServerDisconnected;
            tcpClient.Connect();

            /*Synchronize Time*/
            SendMessage(new MessageTimeSync());
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

        public static void SendMessage(Message message)
        {
            LogManager.LogMessage(LogType.Info, "Sending MESSAGE !");
            tcpClient.Send(message);
        }

        private static void TcpClient_OnMessage(Network.Messages.Message message)
        {
            LogManager.LogMessage(LogType.Info, "Received message: " + message.type);
            switch (message.type)
            {
                case MessageType.KeepAlive:
                    lastServerHeartbeat = DateTime.Now.ToFileTimeUtc();
                    break;

                case MessageType.TimeInfo:
                    ServerTime.Set(DateTime.FromFileTimeUtc((long)message.info));
                    LogManager.LogMessage(LogType.Info, "Sync Date/Time...");
                    break;

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
                    
                case MessageType.ReloadConfiguration:
                    Global.ReloadConfiguration();
                    break;

                case MessageType.GetConfigurationPerClient:
                    SendMessage(
                        new MessageSetConfigurationPerClient(
                            Configuration.DeviceID,
                            Configuration.GetConfigurationFile()));
                    break;

                case MessageType.SetConfigurationPerClient:
                    var config = message.info as string;
                    Configuration.ReplaceConfiguration(config);
                    break;

                case MessageType.CalibrationRequest:
                    /*Calculate Intrinsic params*/
                    //SendMessage(
                    //    new MessageCalibration());
                    break;

                case MessageType.ColorFrameRequest:
                    /*Send last colorFrame*/
                    SendMessage(new MessageColorFrame(camera.colorImageByteArr));
                    break;

                case MessageType.DepthFrameRequest:
                    /*TODO*/
                    /*Send last depthFrame*/
                    SendMessage(new MessageDepthFrame());
                    break;

                case MessageType.IRFrameRequest:
                    /*TODO*/
                    /*Send last colorFrame*/
                    SendMessage(new MessageIRFrame());
                    break;

                case MessageType.SkeletonRequest:
                    /*TODO*/
                    /*Send last colorFrame*/
                    SendMessage(new MessageSkeleton());
                    break;
            }
        }
    }
}
