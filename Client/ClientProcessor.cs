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
            try
            {
                Console.WriteLine("Camera Init");
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

                Console.WriteLine("Camera Init - Done");
                camera.Start();
                Thread.Sleep(2000);
                Console.WriteLine("Camera started");
                camera.Stop();
                Thread.Sleep(2000);
                Console.WriteLine("Camera stopped");
                camera.Start();
                Thread.Sleep(2000);
                Console.WriteLine("Camera started");
                camera.Stop();
            }
            catch(Exception ex)
            {
                Console.Write("Error: " + ex.ToString());
            }
        }

        private static int frameCounter = 0;
        private static void Camera_OnColorFrameArrived()
        {
            if (!Configuration.IsServerDisconnected)
            {
                LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Sending color frame...");
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
            SendMessage(new MessageSendInfoToServer("Device " + Configuration.DeviceID + ": Connected"));
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
            tcpClient.Send(message);
        }

        private static void TcpClient_OnMessage(Network.Messages.Message message)
        {
            try
            {
                LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Received message: " + message.type);
                switch (message.type)
                {
                    case MessageType.TimeInfo:
                        ServerTime.Set(DateTime.FromFileTimeUtc((long)message.info));
                        LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Sync Date/Time...");
                        break;

                    case MessageType.KeepAlive:
                        lastServerHeartbeat = DateTime.Now.ToFileTimeUtc();
                        break;


                    case MessageType.RestartClientApp:
                        Global.RestartApp();
                        break;

                    case MessageType.RestartClientDevice:
                        Global.RestartDevice();
                        break;


                    case MessageType.ReloadConfiguration:
                        Global.ReloadConfiguration();
                        break;

                    case MessageType.StoreConfigurationPerClient:
                        try
                        {
                            var config_store = message.info as string;
                            Configuration.ReplaceConfiguration(config_store);
                            LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Configuration replaced");
                        }
                        catch (Exception ex)
                        {
                            var info = Configuration.DeviceID + ": There was a problem saving the configuration";
                            SendMessage(new MessageSendInfoToServer(info));
                            LogManager.LogMessage(LogType.Error, LogLevel.ErrWarnInfo, info + ": " + ex.ToString());
                        }
                        break;

                    case MessageType.GetConfigurationPerClient:
                        SendMessage(
                            new MessageSetConfigurationPerClient(
                                Configuration.DeviceID,
                                Configuration.GetConfigurationFile()));
                        break;

                    case MessageType.ShowConfigurationPerClient:
                        var config_show = message.info as string;
                        Configuration.ReplaceConfiguration(config_show);
                        break;


                    case MessageType.CameraStart:
                        {
                            try
                            {
                                camera.Start();
                                LogManager.LogMessage(LogType.Info, LogLevel.ErrWarnInfo, "Camera Started.");
                            }
                            catch (Exception ex)
                            {
                                var info = Configuration.DeviceID + ": There was a problem starting the camera";
                                SendMessage(new MessageSendInfoToServer(info));
                                LogManager.LogMessage(LogType.Error, LogLevel.ErrWarnInfo, info + ": " + ex.ToString());
                            }
                        }
                        break;

                    case MessageType.CameraStop:
                        {
                            try
                            {
                                camera.Stop();
                                LogManager.LogMessage(LogType.Info, LogLevel.ErrWarnInfo, "Camera Stopped.");
                            }
                            catch (Exception ex)
                            {
                                var info = Configuration.DeviceID + ": There was a problem stopping the camera";
                                SendMessage(new MessageSendInfoToServer(info));
                                LogManager.LogMessage(LogType.Error, LogLevel.ErrWarnInfo, info + ": " + ex.ToString());
                            }
                        }
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
            catch (Exception ex)
            {
                LogManager.LogMessage(LogType.Error, LogLevel.Communication, "Error receiving message.");
            }
        }
    }
}
