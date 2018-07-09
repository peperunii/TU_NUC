﻿using Network;
using Network.Devices;
using Network.Discovery;
using Network.Logger;
using Network.Messages;
using NetworkLib.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        private static void Server_OnMessage(object xSender, Message message)
        {
            try
            {
                if (message != null)
                {
                    SendToController(new MessageSendInfoToServer("Message received: " + message.type));

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
                            var messageTimeInfo = new MessageTimeInfo().Serialize();
                            tcpStream.Write(messageTimeInfo, 0, messageTimeInfo.Length);
                            break;


                        case MessageType.RestartClientApp:
                            break;

                        case MessageType.RestartClientDevice:
                            break;


                        case MessageType.RestartServerApp:
                            Global.RestartApp();
                            break;

                        case MessageType.RestartServerDevice:
                            Global.RestartDevice();
                            break;

                        case MessageType.ShutdownDevice:
                            break;


                        case MessageType.GetConnectedClients:
                            LogManager.LogMessage(LogType.Info, LogLevel.Everything, "Sending Clients info to UI Controller");
                            LogManager.LogMessage(LogType.Info, LogLevel.Communication, "Number of connected Devices: " + Devices.Count);
                            var messageClients = new MessageConnectedClients(ServerActions.Devices).Serialize();
                            tcpStream.Write(messageClients, 0, messageClients.Length);
                            break;


                        case MessageType.ReloadConfiguration:
                            break;

                        case MessageType.GetConfigurationPerClient:
                            {
                                var deviceID_GetConfig = (message as MessageGetConfigurationPerClient).deviceId;

                                if (deviceID_GetConfig == DeviceID.TU_SERVER)
                                {
                                    var getConfMessage = new MessageSetConfigurationPerClient(DeviceID.TU_SERVER, Configuration.GetConfigurationFile()).Serialize();
                                    tcpStream.Write(getConfMessage, 0, getConfMessage.Length);
                                }
                                else
                                {
                                    var getConfMessage = new MessageGetConfigurationPerClient(deviceID_GetConfig).Serialize();
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
                            LogManager.LogMessage(
                                LogType.Warning,
                                LogLevel.Everything,
                                "Received Color Frame: ("
                                + (message as MessageColorFrame).Height
                                + ", "
                                + (message as MessageColorFrame).Width
                                + ")");
                            break;

                        case MessageType.Skeleton:
                            SendToController(message);
                            break;

                        case MessageType.SkeletonRequest:
                            {
                                try
                                {
                                    var deviceID = (message as MessageSkeletonRequest).deviceId;

                                    tcpServers[deviceID].Send(
                                            tcpServers[deviceID].GetClient(),
                                            new MessageSkeletonRequest(deviceID));
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
