using Network;
using Network.Devices;
using Network.Discovery;
using Network.Logger;
using Network.Messages;
using Network.Utils;
using NetworkLib.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public static class ServerActions
    {
        public static List<NUC> Devices { get; set; }
        public static Dictionary<DeviceID, TcpNetworkListener> tcpServers;
        
        public static DiscoveryReceiver discoveryServer;
        
        public static void Listener()
        {
            Devices = new List<NUC>();
            tcpServers = new Dictionary<DeviceID, TcpNetworkListener>();
            
            StartDiscoveryServer();

            Global.EventDispatcher.AdminStartDiscovery += EventDispatcher_AdminStartDiscovery;
            
            while (true)
            {
                /*do not let the program to exit*/
                Console.ReadKey();
                Thread.Sleep(1);
            }
        }

        public static TcpNetworkListener StartTcpServer()
        {
            var tcpServer = new TcpNetworkListener(Configuration.DeviceIP.ToString(), Configuration.GetAvailablePort(), "Server");
            tcpServer.OnMessage += Server_OnMessage;
            tcpServer.Connect();

            SendToController(new MessageSendInfoToServer("Client connected."));
            return tcpServer;
        }

        private static void SendToController(Message message)
        {
            try
            {
                if (tcpServers.ContainsKey(DeviceID.Controller))
                {
                    tcpServers[DeviceID.Controller].Send(
                        tcpServers[DeviceID.Controller].GetClient(), message);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static Dictionary<DeviceID, List<DateTime>> deviceFps = new Dictionary<DeviceID, List<DateTime>>();
        private static void FPSCalculator(MessageIRFrame message)
        {
            var deviceId = message.deviceID;
            if(!deviceFps.ContainsKey(deviceId))
            {
                deviceFps.Add(deviceId, new List<DateTime>());
            }
            deviceFps[deviceId].Add(DateTime.Now);
            for (int i = deviceFps[deviceId].Count - 1; i >= 0; i--)
            {
                if((deviceFps[deviceId][(deviceFps[deviceId].Count - 1)] - deviceFps[deviceId][i]).TotalSeconds > 1)
                {
                    var fps = deviceFps[deviceId].Count - i;
                    Console.WriteLine("FPS_" + deviceId + ": " + fps);
                    break;
                }
            }
        }

        private static void Server_OnMessage(object xSender, Message message)
        {
            var bytesEnding = TcpNetworkListener.endOfMessageByteSequence;
            try
            {
                if (message != null)
                {
                    var tcpStream = (xSender as TcpClient).GetStream();

                    switch (message.type)
                    {
                        case MessageType.Info:
                            /*Re-Send info to the Controller - If connected*/
                            SendToController(message);
                            break;

                        case MessageType.KeepAlive:
                            break;

                        case MessageType.TimeSyncRequest:
                            var messageTimeInfo = (new MessageTimeInfo().Serialize()).ConcatenatingArrays(bytesEnding);
                            tcpStream.WriteAsync(messageTimeInfo, 0, messageTimeInfo.Length);
                            break;


                        case MessageType.RestartClientApp:
                            break;
                            try
                            {
                                var msg = message as MessageRestartClientApp;
                                var deviceID = msg.deviceId;

                                tcpServers[deviceID].Send(
                                        tcpServers[deviceID].GetClient(),
                                        message);
                            }
                            catch (Exception)
                            {

                            }
                            break;

                        case MessageType.RestartClientDevice:
                            break;
                            try
                            {
                                var msg = message as MessageRestartClientDevice;
                                var deviceID = msg.deviceId;

                                tcpServers[deviceID].Send(
                                        tcpServers[deviceID].GetClient(),
                                        message);
                            }
                            catch (Exception)
                            {

                            }
                            break;


                        case MessageType.RestartServerApp:
                            //Global.RestartApp();
                            break;

                        case MessageType.RestartServerDevice:
                            //Global.RestartDevice();
                            break;

                        case MessageType.ShutdownDevice:
                            try
                            {
                                var msg = message as MessageShutdownDevice;
                                var deviceID = msg.deviceId;

                                if (deviceID != DeviceID.TU_SERVER)
                                {
                                    tcpServers[deviceID].Send(
                                            tcpServers[deviceID].GetClient(),
                                            message);
                                }
                                else
                                {
                                    Global.ShutDownDevice();
                                }
                            }
                            catch (Exception)
                            {

                            }
                            break;


                        case MessageType.GetConnectedClients:
                            LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Sending Clients info to UI Controller");
                            LogManager.LogMessage(LogType.Info, LogLevel.Communication, "Number of connected Devices: " + Devices.Count);
                            var messageClients = (new MessageConnectedClients(ServerActions.Devices).Serialize()).ConcatenatingArrays(bytesEnding);
                            tcpStream.WriteAsync(messageClients, 0, messageClients.Length);
                            break;


                        case MessageType.ReloadConfiguration:
                            break;

                        case MessageType.GetConfigurationPerClient:
                            {
                                var deviceID_GetConfig = (message as MessageGetConfigurationPerClient).deviceId;

                                if (deviceID_GetConfig == DeviceID.TU_SERVER)
                                {
                                    var getConfMessage = ArrayUtils.ConcatenatingArrays(new MessageSetConfigurationPerClient(DeviceID.TU_SERVER, Configuration.GetConfigurationFile()).Serialize(), bytesEnding);
                                    tcpStream.WriteAsync(getConfMessage, 0, getConfMessage.Length);
                                }
                                else
                                {
                                    var getConfMessage = (new MessageGetConfigurationPerClient(deviceID_GetConfig).Serialize()).ConcatenatingArrays(bytesEnding);
                                    tcpStream.Write(getConfMessage, 0, getConfMessage.Length);
                                }
                            }
                            break;

                        case MessageType.StoreConfigurationPerClient:
                            {
                                var deviceID_StoreConfig = (message as MessageStoreConfigurationPerClient).deviceId;
                                var configuration = (message as MessageStoreConfigurationPerClient).info as string;

                                LogManager.LogMessage(
                                    LogType.Warning,
                                    LogLevel.Everything,
                                    "Saving Configuration for client: " + deviceID_StoreConfig);

                                if (deviceID_StoreConfig == DeviceID.TU_SERVER)
                                {
                                    Configuration.ReplaceConfiguration(configuration);
                                }
                                else
                                {
                                    tcpServers[deviceID_StoreConfig].Send(
                                        tcpServers[deviceID_StoreConfig].GetClient(),
                                        new MessageStoreConfigurationPerClient(deviceID_StoreConfig, configuration));
                                }
                            }
                            break;

                        case MessageType.ShowConfigurationPerClient:
                            {
                                /*Send configuration to the Controller - If connected*/
                                SendToController(new MessageSetConfigurationPerClient(
                                            (message as MessageSetConfigurationPerClient).deviceId,
                                            (message as MessageSetConfigurationPerClient).info as string));
                            }
                            break;


                        case MessageType.CameraStatusRequest:
                            {
                                try
                                {
                                    var msg = message as MessageCameraStatusRequest;
                                    var deviceID = msg.deviceId;

                                    tcpServers[deviceID].Send(
                                            tcpServers[deviceID].GetClient(),
                                            message);
                                }
                                catch(Exception)
                                {

                                }
                            }
                            break;

                        case MessageType.CameraStatus:
                            {
                                try
                                {
                                    SendToController(message);
                                }
                                catch(Exception)
                                {

                                }
                            }
                            break;

                        case MessageType.CameraStart:
                            {
                                try
                                {
                                    var deviceID = (message as MessageCameraStart).deviceId;

                                    tcpServers[deviceID].Send(
                                            tcpServers[deviceID].GetClient(),
                                            new MessageCameraStart(deviceID));
                                }
                                catch (Exception) { }
                            }
                            break;

                        case MessageType.CameraStop:
                            {
                                try
                                {
                                    var deviceID = (message as MessageCameraStop).deviceId;

                                    tcpServers[deviceID].Send(
                                            tcpServers[deviceID].GetClient(),
                                            new MessageCameraStop(deviceID));
                                }
                                catch (Exception) { }
                            }
                            break;

                        case MessageType.ColorFrame:
                            SendToController(message);
                            break;

                        case MessageType.DepthFrame:
                            SendToController(message);
                            break;

                        case MessageType.IRFrame:
                            FPSCalculator(message as MessageIRFrame);
                            //Console.WriteLine(
                            //    (message as MessageIRFrame).deviceID +
                            //    ": " +
                            //    DateTime.FromFileTimeUtc((message as MessageIRFrame).Timestamp).ToString();
                            SendToController(message);
                            break;

                        case MessageType.Skeleton:
                            /*Resend Skeleton*/
                            Console.WriteLine("Sending Skeleton Message..");
                            SendToController(message);
                            break;

                        case MessageType.ColorFrameRequest:
                            {
                                try
                                {
                                    var deviceID = (message as MessageColorFrameRequest).deviceId;

                                    tcpServers[deviceID].Send(
                                            tcpServers[deviceID].GetClient(),
                                            message);
                                    Console.WriteLine("Resend Color Request");
                                }
                                catch (Exception) { }
                            }
                            break;

                        case MessageType.DepthFrameRequest:
                            {
                                try
                                {
                                    var deviceID = (message as MessageDepthFrameRequest).deviceId;

                                    tcpServers[deviceID].Send(
                                            tcpServers[deviceID].GetClient(),
                                            message);
                                    Console.WriteLine("Resend Depth Request");
                                }
                                catch (Exception) { }
                            }
                            break;

                        case MessageType.IRFrameRequest:
                            {
                                try
                                {
                                    var deviceID = (message as MessageIRFrameRequest).deviceId;

                                    tcpServers[deviceID].Send(
                                            tcpServers[deviceID].GetClient(),
                                            message);
                                    Console.WriteLine("Resend IR Request");
                                }
                                catch (Exception) { }
                            }
                            break;

                        case MessageType.SkeletonRequest:
                            {
                                try
                                {
                                    var deviceID = (message as MessageSkeletonRequest).deviceId;

                                    tcpServers[deviceID].Send(
                                            tcpServers[deviceID].GetClient(),
                                            message);
                                    Console.WriteLine("Resend Skeleton Request");
                                }
                                catch (Exception) { }
                            }
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                LogManager.LogMessage(LogType.Error, LogLevel.Communication, "Error receiving message.");
            }
        }

        public static void StartDiscoveryServer()
        {
            discoveryServer = new DiscoveryReceiver(Configuration.DeviceIP.ToString(), Configuration.DiscoveryPort);
            discoveryServer.DeviceConnected += DiscoveryServer_DeviceConnected;
            discoveryServer.StartListening();
        }

        private static void DiscoveryServer_DeviceConnected(object source, Network.Events.NucConnectedEventArgs e)
        {
            var connectedDevices = e.ConnectedAddresses;
            
            foreach (var device in connectedDevices)
            {
                LogManager.LogMessage(LogType.Info, LogLevel.Communication, "Device Found: " + device.Key);

                var deviceID = device.Key;
                var address = device.Value;

                var exist = (from t in Devices
                             where t.deviceID == deviceID
                             select t).FirstOrDefault();
                if (exist != null)
                {
                    Devices.Remove(exist);
                    tcpServers.Remove(deviceID);
                }

                Devices.Add(new NUC(deviceID, address));
                var tcpServer = StartTcpServer();
                tcpServers.Add(deviceID, tcpServer);

                SendDeviceStart(tcpServer, deviceID);
            }
        }


        private static void SendDeviceStart(TcpNetworkListener tcpServer, DeviceID deviceID)
        {
            LogManager.LogMessage(LogType.Info, LogLevel.Communication, "Device Start");
            var deviceIP = (from t in Devices
                        where t.deviceID == deviceID
                        select t.ip).FirstOrDefault();

            discoveryServer.SendDeviceStart(tcpServer, deviceIP.Address);
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
