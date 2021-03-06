﻿using Network.Logger;
using Network.Messages;
using Network.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkLib.TCP
{
    public class TcpNetworkListener
    {
        private static List<MessageType> messagesWithHeaderOnly = null;

        public static byte[] endOfMessageByteSequence;
        private bool _ExitLoop = true;
        private TcpListener _Listener;
        private int BUFFER_SIZE = 33554432;

        public delegate void dOnMessage(object xSender, Message message);
        public event dOnMessage OnMessage;

        public delegate void dOnTCPMessageArived(TcpClient client, byte[] byteArr);
        public event dOnTCPMessageArived OnTCPMessageArrived;


        private List<TcpClient> _clients = new List<TcpClient>();

        public int Port { get; private set; }
        public string IpAddress { get; private set; }
        public string ThreadName { get; private set; }

        public List<TcpClient> Clients
        {
            get { return _clients; }
        }

        public TcpNetworkListener(string xIpAddress, int xPort, string xThreadName)
        {
            endOfMessageByteSequence = Encoding.ASCII.GetBytes("EndOfMessage");
            this.OnTCPMessageArrived += this.TcpNetworkClient_OnTCPMessageArrived;

            Port = xPort;
            IpAddress = xIpAddress;
            ThreadName = xThreadName;

            /*Reflection - find all Clild Messages classes and which of them are headerOnly*/
            if (messagesWithHeaderOnly == null)
            {
                messagesWithHeaderOnly = new List<MessageType>();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (!assembly.FullName.Contains("NetworkLib")) continue;
                    var types = assembly.GetTypes();

                    foreach (var type in types)
                    {
                        if (type.IsSubclassOf(typeof(Message)))
                        {
                            try
                            {
                                dynamic instance =
                                Convert.ChangeType(
                                    assembly.CreateInstance(type.ToString()),
                                    type);

                                if (instance.Serialize().Length == 2)
                                {
                                    messagesWithHeaderOnly.Add(instance.type);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }

                Console.WriteLine(string.Join(Environment.NewLine, messagesWithHeaderOnly));
            }
        }

        public TcpClient GetClient()
        {
            return this._clients[0];
        }

        public bool Connect()
        {
            if (!_ExitLoop)
            {
                LogManager.LogMessage(
                    LogType.Warning,
                    LogLevel.Communication,
                    "Listener running already");
                return false;
            }
            _ExitLoop = false;

            try
            {
                _Listener = new TcpListener(IPAddress.Parse(IpAddress), Port);
                _Listener.Server.ReceiveBufferSize = BUFFER_SIZE;
                _Listener.Server.SendBufferSize = BUFFER_SIZE;
                _Listener.Server.NoDelay = true;

                _Listener.Start();
                LogManager.LogMessage(
                    LogType.Info,
                    LogLevel.Communication,
                    "TCP connection in port: " + Port);
                Thread lThread = new Thread(new ThreadStart(LoopWaitingForClientsToConnect));
                lThread.IsBackground = true;
                lThread.Name = ThreadName + "WaitingForClients";
                lThread.Start();

                return true;
            }
            catch (Exception ex)
            {
                LogManager.LogMessage(
                    LogType.Error,
                    LogLevel.Errors,
                    ex.ToString()); }
            return false;
        }

        public void DisconnectAll()
        {
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    client.Close();
                    _clients.Remove(client);
                }
            }
        }

        public void Disconnect(TcpClient client)
        {
            _ExitLoop = true;
            lock (_clients)
            {
                client.Close();
                _clients.Remove(client);
            }
        }

        private void LoopWaitingForClientsToConnect()
        {
            try
            {
                while (!_ExitLoop)
                {
                    LogManager.LogMessage(
                        LogType.Info,
                        LogLevel.Communication,
                        "waiting for a client");
                    var lClient = _Listener.AcceptTcpClient();
                    string lClientIpAddress = lClient.Client.LocalEndPoint.ToString();
                    LogManager.LogMessage(
                        LogType.Info,
                        LogLevel.Communication,
                        "new client connecting: " + lClientIpAddress);
                    if (_ExitLoop) break;
                    lock (_clients) _clients.Add(lClient);

                    Thread lThread = new Thread(new ParameterizedThreadStart(LoopRead));
                    lThread.IsBackground = true;
                    lThread.Name = ThreadName + "CommunicatingWithClient";
                    lThread.Start(lClient);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogMessage(
                    LogType.Error,
                    LogLevel.Errors,
                    ex.ToString());
            }
            finally
            {
                _ExitLoop = true;
                if (_Listener != null) _Listener.Stop();
            }
        } // 

        private void TcpNetworkClient_OnTCPMessageArrived(TcpClient client, byte[] byteArr)
        {
            try
            {
                var msg = MessageParser.GetMessageFromBytArr(byteArr);
                //fire event
                dOnMessage lEvent = OnMessage;
                if (lEvent == null) return;
                lEvent(client, msg);
                Thread.Sleep(1);
            }
            catch (Exception) { }
        }

        private void LoopRead(object xClient)
        {
            TcpClient lClient = xClient as TcpClient;
            NetworkStream lNetworkStream = lClient.GetStream();
            
            while (!_ExitLoop && lClient.Connected)
            {
                try
                {
                    GetBytArrFromNetworkStream(lClient, lNetworkStream);
                }
                catch (System.IO.IOException ex)
                {
                    if (_ExitLoop) LogManager.LogMessage(LogType.Error, LogLevel.Errors, "User requested client shutdown");
                    else LogManager.LogMessage(LogType.Error, LogLevel.Communication, "Disconnected");
                    this.RestartClient(lClient);
                    break;
                }
                catch (Exception ex) {
                    this.RestartClient(lClient); LogManager.LogMessage(LogType.Error, LogLevel.Errors, ex.ToString());
                    break;
                }
            }
            LogManager.LogMessage(LogType.Error, LogLevel.Communication, "Listener is shutting down");
        }

        private void RestartClient(TcpClient client)
        {
            LogManager.LogMessage(LogType.Warning, LogLevel.Communication, "Restarting ...");
            this.Disconnect(client);
            //this.Connect();
        }
        
        private void GetBytArrFromNetworkStream(TcpClient tcpClient, NetworkStream _NetworkStream)
        {
            try
            {
                byte[] lHeader = new byte[2];
                
                if (_NetworkStream.Read(lHeader, 0, 2) != 2)
                {
                    this.TriggerEmptyMessage(tcpClient);
                }

                var messageType = (MessageType)BitConverter.ToInt16(lHeader, 0);

                if ((ushort)messageType > Enum.GetValues(typeof(MessageType)).Length)
                {
                    this.TriggerEmptyMessage(tcpClient);
                }
                if (messagesWithHeaderOnly.Contains(messageType))
                {
                    //fire event
                    dOnTCPMessageArived lEvent = OnTCPMessageArrived;
                    if (lEvent != null)
                    {
                        lEvent(tcpClient, lHeader);
                    }
                }
                else
                {
                    try
                    {
                        var dataLength = new byte[4];
                        
                        _NetworkStream.Read(dataLength, 0, 4);

                        var dataSize = BitConverter.ToInt32(dataLength, 0);
                        if (dataSize < 0)
                        {
                            this.TriggerEmptyMessage(tcpClient);
                        }
                        var data = new byte[dataSize];

                        var readBuffer = new byte[BUFFER_SIZE];
                        using (var memoryStream = new MemoryStream())
                        {
                            do
                            {
                                var numberOfBytesRead = _NetworkStream.ReadAsync(readBuffer, 0, readBuffer.Length);
                                memoryStream.Write(readBuffer, 0, numberOfBytesRead.Result);

                                if (!_NetworkStream.DataAvailable)
                                    System.Threading.Thread.Sleep(1);
                            }
                            while (_NetworkStream.DataAvailable);

                            data = memoryStream.ToArray();
                        }

                        //if (_NetworkStream.Read(data, 0, dataSize) != dataSize) return null;
                        var fullMessage = lHeader.ConcatenatingArrays(dataLength.ConcatenatingArrays(data));
                        
                        this.GetListOfArraysUsingSeparator(tcpClient, fullMessage);
                    }
                    catch (Exception)
                    {
                        this.TriggerEmptyMessage(tcpClient);
                    }
                }
            }
            catch(Exception ex)
            {
                this.TriggerEmptyMessage(tcpClient);
            }
        }

        private void TriggerEmptyMessage(TcpClient tcpClient)
        {
            //fire event
            dOnTCPMessageArived lEvent = OnTCPMessageArrived;
            if (lEvent != null)
            {
                lEvent(tcpClient, new byte[] { });
            }
        }

        private void GetListOfArraysUsingSeparator(TcpClient tcpClient, byte[] fullMessage)
        {
            var indexEnd = 0;
            while (indexEnd < fullMessage.Length)
            {
                var indexOfSeparator = SearchBytes(fullMessage, endOfMessageByteSequence);

                if (indexOfSeparator == -1) break;

                //fire event
                dOnTCPMessageArived lEvent = OnTCPMessageArrived;
                if (lEvent != null)
                {
                    lEvent(tcpClient, fullMessage.SubArray(indexEnd, indexOfSeparator));
                }

                indexEnd += (indexOfSeparator + endOfMessageByteSequence.Length);
            }
        }

        private int SearchBytes(byte[] array, byte[] subArr)
        {
            var len = subArr.Length;
            var limit = array.Length - len;
            for (var i = 0; i <= limit; i++)
            {
                var k = 0;
                for (; k < len; k++)
                {
                    if (subArr[k] != array[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }

        public void Send(TcpClient xClient, Message msg)
        {
            if (msg == null) return;
            if (!xClient.Connected) return;

            lock (xClient)
            {
                try
                {
                    NetworkStream lNetworkStream = xClient.GetStream();
                    var msgData = msg.Serialize().ConcatenatingArrays(endOfMessageByteSequence);
                    lNetworkStream.WriteAsync(msgData, 0, msgData.Length);
                    Thread.Sleep(1);
                }
                catch (Exception ex) { LogManager.LogMessage(LogType.Error, LogLevel.Errors, ex.ToString()); }
            }
        }

        public void Broadcast(Message msg)
        {
            lock (_clients)
            {
                foreach (var client in _clients) Send(client, msg);
            }
        }
    }
}
