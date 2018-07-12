using Client.Cameras;
using Network;
using Network.Discovery;
using Network.Logger;
using Network.Messages;
using Network.TCP;
using Network.TimeSync;
using System;
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

        private static bool IsCameraStarted = false;
        private static bool IsColorFrameRequested = false;
        private static bool IsDepthFrameRequested = false;
        private static bool IsIRFrameRequested = false;
        private static bool IsBodyFrameRequested = false;

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
            }
            catch(Exception ex)
            {
                LogManager.LogMessage(LogType.Error, LogLevel.Errors, "Problem with camera Init: " + ex.ToString());
            }
        }
        
        private static void Camera_OnColorFrameArrived()
        {
            if (!Configuration.IsServerDisconnected)
            {
                if (IsCameraStarted && IsColorFrameRequested)
                {
                    Console.WriteLine("Sending Color frame");

                    LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Sending color frame...");
                    var colorData = camera.GetData(CameraDataType.Color);

                    tcpClient.Send(
                    new MessageColorFrame(
                        Configuration.DeviceID,
                        camera.colorFrameDescription.Height,
                        camera.colorFrameDescription.Width,
                        3,
                        false,
                        colorData as byte[]));
                }
            }
        }

        private static void Camera_OnDepthFrameArrived()
        {
            if (IsCameraStarted && IsDepthFrameRequested)
            {
                Console.WriteLine("Sending Depth frame");

                var depthData = camera.GetData(CameraDataType.Depth);

                tcpClient.Send(
                    new MessageDepthFrame(
                        Configuration.DeviceID,
                        camera.depthFrameDescription.Height,
                        camera.depthFrameDescription.Width,
                        1,
                        false,
                        depthData as byte[]));
            }
        }

        private static void Camera_OnIRFrameArrived()
        {
            if (IsCameraStarted && IsIRFrameRequested)
            {
                var irData = camera.GetData(CameraDataType.IR);
                tcpClient.Send(
                    new MessageIRFrame(
                        Configuration.DeviceID, 
                        camera.irFrameDescription.Height, 
                        camera.irFrameDescription.Width, 
                        1, 
                        false, 
                        irData as byte []));
            }
        }

        private static void Camera_OnBodyFrameArrived()
        {
            if (IsCameraStarted && IsBodyFrameRequested)
            {
                if (camera.IsTrackedBodyFound())
                {
                    tcpClient.Send(new MessageSkeleton(Configuration.DeviceID, camera.GetConvertedBodyArr()));
                }
            }
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

                    case MessageType.ShutdownDevice:
                        Global.ShutDownDevice();
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
                                IsCameraStarted = true;
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
                                IsCameraStarted = false;
                                IsColorFrameRequested = false;
                                IsDepthFrameRequested = false;
                                IsIRFrameRequested = false;
                                IsBodyFrameRequested = false;
                            }
                            catch (Exception ex)
                            {
                                var info = Configuration.DeviceID + ": There was a problem stopping the camera";
                                SendMessage(new MessageSendInfoToServer(info));
                                LogManager.LogMessage(LogType.Error, LogLevel.ErrWarnInfo, info + ": " + ex.ToString());
                            }
                        }
                        break;

                    case MessageType.CameraStatusRequest:
                        var msgCameraStatus =
                            new MessageCameraStatus(
                                Configuration.DeviceID,
                                IsCameraStarted, 
                                IsColorFrameRequested,
                                IsDepthFrameRequested,
                                IsIRFrameRequested, 
                                IsBodyFrameRequested);
                        SendMessage(msgCameraStatus);
                        break;
                    
                    case MessageType.CalibrationRequest:
                        /*Calculate Intrinsic params*/
                        //SendMessage(
                        //    new MessageCalibration());
                        break;

                    case MessageType.ColorFrameRequest:
                        Console.WriteLine("Request: Color - " + (!IsColorFrameRequested).ToString());
                        IsColorFrameRequested = !IsColorFrameRequested;
                        break;

                    case MessageType.DepthFrameRequest:
                        Console.WriteLine("Request: Depth - " + (!IsDepthFrameRequested).ToString());
                        IsDepthFrameRequested = !IsDepthFrameRequested;
                        break;

                    case MessageType.IRFrameRequest:
                        Console.WriteLine("Request: IR - " + (!IsIRFrameRequested).ToString());
                        IsIRFrameRequested = !IsIRFrameRequested;
                        break;

                    case MessageType.SkeletonRequest:
                        Console.WriteLine("Request: Skeleton - " + (!IsBodyFrameRequested).ToString());
                        IsBodyFrameRequested = !IsBodyFrameRequested;
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
