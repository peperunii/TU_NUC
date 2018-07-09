using Network.Logger;
using Network.Messages;
using Network.TCP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkLib.TCP
{
    public class TcpNetworkListener
    {
        private static List<MessageType> messagesWithHeaderOnly = null;

        private bool _ExitLoop = true;
        private TcpListener _Listener;
        public delegate void dOnMessage(object xSender, Message message);
        public event dOnMessage OnMessage;

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

   
        private void LoopRead(object xClient)
        {
            TcpClient lClient = xClient as TcpClient;
            NetworkStream lNetworkStream = lClient.GetStream();

            while (!_ExitLoop && lClient.Connected)
            {
                try
                {
                    var msg = MessageParser.GetMessageFromBytArr(GetBytArrFromNetworkStream(lNetworkStream));

                    if (msg != null)
                    {
                        //fire event
                        dOnMessage lEvent = OnMessage;
                        if (lEvent == null) continue;
                        lEvent(lClient, msg);
                    }
                    Thread.Sleep(1);
                }
                catch (System.IO.IOException ex)
                {
                    if (_ExitLoop) LogManager.LogMessage(LogType.Error, LogLevel.Errors, "User requested client shutdown");
                    else LogManager.LogMessage(LogType.Error, LogLevel.Communication, "Disconnected");
                    this.RestartClient(lClient);
                }
                catch (Exception ex) {
                    this.RestartClient(lClient); LogManager.LogMessage(LogType.Error, LogLevel.Errors, ex.ToString()); }
            }
            LogManager.LogMessage(LogType.Error, LogLevel.Communication, "Listener is shutting down");
        }

        private void RestartClient(TcpClient client)
        {
            LogManager.LogMessage(LogType.Warning, LogLevel.Communication, "Restarting ...");
            this.Disconnect(client);
            this.Connect();
        }
        
        private byte[] GetBytArrFromNetworkStream(NetworkStream _NetworkStream)
        {
            try
            {
                byte[] lHeader = new byte[2];

                Console.WriteLine("reading 2 bytes...");
                if (_NetworkStream.Read(lHeader, 0, 2) != 2)
                {
                    return null;
                }

                var messageType = (MessageType)BitConverter.ToInt16(lHeader, 0);
                Console.WriteLine("Message type: " + messageType);
                if ((ushort)messageType > Enum.GetValues(typeof(MessageType)).Length)
                {
                    return null;
                }
                if (messagesWithHeaderOnly.Contains(messageType))
                {
                    return lHeader;
                }
                else
                {
                    try
                    {
                        var dataLength = new byte[4];

                        Console.WriteLine("reading 4 bytes...");
                        _NetworkStream.Read(dataLength, 0, 4);

                        var dataSize = BitConverter.ToInt32(dataLength, 0);
                        if (dataSize < 0)
                        {
                            return null;
                        }
                        var data = new byte[dataSize];
                        
                        Console.WriteLine("reading " + dataSize + " bytes...");
                        if (_NetworkStream.Read(data, 0, dataSize) != dataSize) return null;
                        var fullMessage = lHeader.Concat(dataLength.Concat(data)).ToArray();
                        return fullMessage;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR in byte func!");
                Console.WriteLine(ex.ToString());
                return null;
            }
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
                    var msgData = msg.Serialize();
                    lNetworkStream.Write(msgData, 0, msgData.Length);
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
