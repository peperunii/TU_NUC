using DB_Initialization;
using Microsoft.Kinect;
using Network;
using Network.Devices;
using Network.Discovery;
using Network.Logger;
using Network.Messages;
using Network.TCP;
using Network.TimeSync;
using NetworkLib.Events;
using NUC_Controller.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NUC_Controller.NetworkWorker
{
    static class Worker
    {
        private static List<NUC> connectedDevices;

        public delegate void MyEventHandlerColor(object source, NewColorArrivedEventArgs e);
        public delegate void MyEventHandlerDepth(object source, NewDepthArrivedEventArgs e);
        public delegate void MyEventHandlerIR(object source, NewIRArrivedEventArgs e);
        public delegate void MyEventHandlerBody(object source, NewBodyArrivedEventArgs e);

        public static event MyEventHandlerColor NewColorArrived;
        public static event MyEventHandlerDepth NewDepthArrived;
        public static event MyEventHandlerIR NewIRArrived;
        public static event MyEventHandlerBody NewBodyArrived;

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
            if (message != null)
            {
                try
                {
                    LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Received message: " + message.type);
                    switch (message.type)
                    {
                        case MessageType.Info:
                            new Notification(NotificationType.Info, (message as MessageSendInfoToServer).info as string);
                            break;

                        case MessageType.TimeInfo:
                            {
                                ServerTime.Set(DateTime.FromFileTimeUtc((long)message.info));
                                LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Sync Date/Time...");
                            }
                            break;

                        case MessageType.ConnectedClients:
                            {
                                Worker.connectedDevices = message.info as List<NUC>;

                                /*Read configurations if user is allowed*/
                                if (Globals.loggedInUser.CheckIfHasAccess(Users.ActionType.ReadConfig))
                                {
                                    foreach (var client in Worker.connectedDevices)
                                    {
                                        SendMessage(new MessageGetConfigurationPerClient(client.deviceID));
                                        if (client.deviceID != DeviceID.TU_SERVER)
                                        {
                                            SendMessage(new MessageCameraStatusRequest(client.deviceID));
                                        }
                                    }
                                }
                            }
                            break;

                        case MessageType.ShowConfigurationPerClient:
                            {
                                var deviceId = (message as MessageSetConfigurationPerClient).deviceId;
                                var device = (from t in connectedDevices
                                              where t.deviceID == deviceId
                                              select t).FirstOrDefault();
                                device.SetConfiguration((message as MessageSetConfigurationPerClient).info as string);
                            }
                            break;

                        case MessageType.CameraStatus:
                            {
                                var msg = (message as MessageCameraStatus);
                                var deviceId = msg.deviceID;
                                var device = (from t in connectedDevices
                                              where t.deviceID == deviceId
                                              select t).FirstOrDefault();
                                device.isCameraStarted = msg.isCameraStarted;
                                device.isColorStreamEnabled = msg.isColorStreamEnabled;
                                device.isDepthStreamEnabled = msg.isDepthStreamEnabled;
                                device.isIRStreamEnabled = msg.isIRStreamEnabled;
                                device.isBodyStreamEnabled = msg.isBodyStreamEnabled;
                            }
                            break;

                        case MessageType.ColorFrame:
                            NewColorArrived.Invoke(null, new NewColorArrivedEventArgs(message as MessageColorFrame));
                            break;

                        case MessageType.DepthFrame:
                            {
                                NewDepthArrived.Invoke(null, new NewDepthArrivedEventArgs(message as MessageDepthFrame));
                            }
                            break;

                        case MessageType.IRFrame:
                            {
                                Console.WriteLine("Received IR frame");
                                NewIRArrived.Invoke(null, new NewIRArrivedEventArgs(message as MessageIRFrame));
                            }
                            break;

                        case MessageType.Skeleton:
                            {
                                var msgSkeleton = message as MessageSkeleton;
                                var deviceId = msgSkeleton.deviceID;
                                var bodiesList = msgSkeleton.info as List<Skeleton>;

                                NewBodyArrived.Invoke(null, new NewBodyArrivedEventArgs(deviceId, bodiesList));
                            }
                            break;

                            // The controller is not sending configuration
                            //case MessageType.GetConfigurationPerClient:
                            //    SendMessage(new MessageSetConfigurationPerClient(Network.Configuration.DeviceID, Network.Configuration.GetConfigurationFile()));
                            //    break;

                    }
                }
                catch (Exception ex)
                {
                    Network.Logger.LogManager.LogMessage(LogType.Error, LogLevel.Communication, "Error in receiving message");
                }
            }
        }

        public static List<NUC> GetConnectedDevices()
        {
            return Worker.connectedDevices;
        }
    }
}
