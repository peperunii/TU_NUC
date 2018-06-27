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
        }
        
        public bool Connect()
        {
            if (!_ExitLoop)
            {
                LogManager.LogMessage(LogType.Warning, "Listener running already");
                return false;
            }
            _ExitLoop = false;
        
            try
            {
                _Listener = new TcpListener(IPAddress.Parse(IpAddress), Port);
                _Listener.Start();

                LogManager.LogMessage(LogType.Info, "TCP connection in port: " + Port);
                Thread lThread = new Thread(new ThreadStart(LoopWaitingForClientsToConnect));
                lThread.IsBackground = true;
                lThread.Name = ThreadName + "WaitingForClients";
                lThread.Start();
        
                return true;
            }
            catch (Exception ex) { LogManager.LogMessage(LogType.Error, ex.ToString()); }
            return false;
        }

        public void Disconnect()
        {
            _ExitLoop = true;
            lock (_clients)
            {
                foreach (TcpClient lClient in _clients) lClient.Close();
                _clients.Clear();
            }
        }

        private void LoopWaitingForClientsToConnect()
        {
            try
            {
                while (!_ExitLoop)
                {
                    LogManager.LogMessage(LogType.Info, "waiting for a client");
                    var lClient = _Listener.AcceptTcpClient();
                    string lClientIpAddress = lClient.Client.LocalEndPoint.ToString();
                    LogManager.LogMessage(LogType.Info, "new client connecting: " + lClientIpAddress);
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
                LogManager.LogMessage(LogType.Error, ex.ToString());
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
                }
                catch (System.IO.IOException)
                {
                    if (_ExitLoop) LogManager.LogMessage(LogType.Error, "User requested client shutdown");
                    else LogManager.LogMessage(LogType.Error, "Disconnected");
                    this.RestartClient();
                }
                catch (Exception ex) { this.RestartClient(); LogManager.LogMessage(LogType.Error, ex.ToString()); }
            }
            LogManager.LogMessage(LogType.Error, "Listener is shutting down");
        }

        private void RestartClient()
        {
            Console.WriteLine("Restarting ...");
            this.Disconnect();
            this.Connect();
        }
        
        private byte[] GetBytArrFromNetworkStream(NetworkStream _NetworkStream)
        {
            byte[] lHeader = new byte[2];

            if (_NetworkStream.Read(lHeader, 0, 2) != 2)
                return null;

            var messageType = (MessageType)BitConverter.ToInt16(lHeader, 0);
            
            if((ushort)messageType > Enum.GetValues(typeof(MessageType)).Length)
            {
                return null;
            }
            switch (messageType)
            {
                case MessageType.KeepAlive:
                case MessageType.ReloadConfiguration:
                case MessageType.RestartClientApp:
                case MessageType.RestartClientDevice:
                case MessageType.RestartServerApp:
                case MessageType.RestartServerDevice:
                case MessageType.SkeletonRequest:
                case MessageType.ColorFrameRequest:
                case MessageType.DepthFrameRequest:
                case MessageType.IRFrameRequest:
                case MessageType.TimeSyncRequest:
                    return lHeader;

                default:
                    try
                    {
                        var dataLength = new byte[4];
                        _NetworkStream.Read(dataLength, 0, 4);
                        
                        var dataSize = BitConverter.ToInt32(dataLength, 0);
                        if (dataSize < 0)
                        {
                            return null;
                        }
                        var data = new byte[dataSize];
                        
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
                }
                catch (Exception ex) { LogManager.LogMessage(LogType.Error, ex.ToString()); }
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
