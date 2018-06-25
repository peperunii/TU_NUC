using Network.Logger;
using Network.Messages;
using Network.TCP;
using System;
using System.Collections.Generic;
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
        
        private List<TcpNetworkClient> _clients = new List<TcpNetworkClient>();
        
        public int Port { get; private set; }
        public string IpAddress { get; private set; }
        public string ThreadName { get; private set; }
        
        public List<TcpNetworkClient> Clients
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
                foreach (var lClient in _clients) lClient.Disconnect();
                _clients.Clear();
            }
        }
        
        private void LoopWaitingForClientsToConnect()
        {
            try
            {
                while (!_ExitLoop)
                {
                    LogManager.LogMessage(LogType.Info, "Waiting for a client");
                    TcpClient tcpClient = _Listener.AcceptTcpClient();
                    var clientEndPoint = tcpClient.Client.LocalEndPoint;
                    string lClientIpAddress = clientEndPoint.ToString();
                    LogManager.LogMessage(LogType.Info, "New client connecting: " + lClientIpAddress);
                    if (_ExitLoop) break;
        
                    string clientIPAddress = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
                    var clientPort = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port;
                    var threadName = string.Format("TcpCLient {0}", _clients.Count);
        
                    lock (_clients)
                    {
                        var wrapper = new TcpNetworkClient(clientIPAddress, clientPort, threadName);
                        wrapper.OnMessage += Wrapper_OnMessage;
                        _clients.Add(wrapper);
                        wrapper.Connect(tcpClient);
                    }
        
                    //Thread lThread = new Thread(new ParameterizedThreadStart(LoopRead));
                    //lThread.IsBackground = true;
                    //lThread.Name = ThreadName + "CommunicatingWithClient";
                    //lThread.Start(tcpClient);
        
                    //Thread lLoopWrite = new Thread(new ThreadStart(LoopWrite));
                    //lLoopWrite.IsBackground = true;
                    //lLoopWrite.Name = ThreadName + "Write";
                    //lLoopWrite.Start(tcpClient);
                }
            }
            catch (Exception ex) { LogManager.LogMessage(LogType.Error, ex.ToString()); }
            finally
            {
                _ExitLoop = true;
                if (_Listener != null) _Listener.Stop();
            }
        } // 
        
        private void Wrapper_OnMessage(Message message)
        {
            // raise an event
            dOnMessage lEvent = OnMessage;
            lEvent?.Invoke(null, message);
        }

        private void LoopWrite(object xClient)
        {
            TcpClient lClient = xClient as TcpClient;
            NetworkStream lNetworkStream = lClient.GetStream();
            while (!_ExitLoop)
            {
                //try
                //{
                //    var lObject = _Queue.Take();
                //    if (lObject == null) break;
                //
                //    lNetworkStream.Write(BitConverter.GetBytes((Int16)lObject.messageType), 0, 2);
                //    ProtoBuf.Serializer.SerializeWithLengthPrefix<TCPMessage>(lNetworkStream, lObject, ProtoBuf.PrefixStyle.Fixed32);
                //}
                //catch (System.IO.IOException)
                //{
                //    if (_ExitLoop) LogManager.LogMessage(LogType.Error, "User requested client shutdown.");
                //    else GLogManager.LogMessage(LogType.Error, "Disconnected");
                //}
                //catch (Exception ex) { LogManager.LogMessage(LogType.Error, ex.ToString()); }
            }
            _ExitLoop = true;
            LogManager.LogMessage(LogType.Error, "Writer is shutting down");
        }

        private void LoopRead(object xClient)
        {
            TcpClient lClient = xClient as TcpClient;
            NetworkStream lNetworkStream = lClient.GetStream();

            while (!_ExitLoop && lClient.Connected)
            {
                try
                {
                    var msg = MessageParser.GetMessageFromBytArr(GetBytArrFromNetworkStream(lNetworkStream));
                    //fire event
                    dOnMessage lEvent = OnMessage;
                    if (lEvent == null) continue;
                    lEvent(null, msg);
                }
                catch (System.IO.IOException)
                {
                    if (_ExitLoop) LogManager.LogMessage(LogType.Error, "User requested client shutdown");
                    else LogManager.LogMessage(LogType.Error, "Disconnected");
                }
                catch (Exception ex) { LogManager.LogMessage(LogType.Error, ex.ToString()); }
            }
            LogManager.LogMessage(LogType.Error, "Listener is shutting down");
        }

        private byte[] GetBytArrFromNetworkStream(NetworkStream _NetworkStream)
        {
            byte[] lHeader = new byte[2];
            if (_NetworkStream.Read(lHeader, 0, 2) != 2) return null;

            var messageType = (MessageType)BitConverter.ToInt16(lHeader, 0);

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
                    var dataLength = new byte[4];
                    if (_NetworkStream.Read(dataLength, 0, 4) != 4) return null;
                    var dataSize = BitConverter.ToInt32(dataLength, 0);
                    var data = new byte[dataSize];

                    if (_NetworkStream.Read(data, 0, dataSize) != dataSize) return null;

                    return lHeader.Concat(dataLength.Concat(data)).ToArray();
            }
        }

        public void Send(TcpNetworkClient xClient, Message msg)
        {
            xClient.Send(msg);
        }
        
        public void Broadcast(Message msg)
        {
            foreach (var client in _clients) Send(client, msg);
        }
    }
}
