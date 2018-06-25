using Network.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Network.Logger;

namespace Network.TCP
{
    public class TcpNetworkClient
    {
        public int Port { get; private set; }
        public string IpAddress { get; private set; }
        public string ThreadName { get; private set; }
        private NetworkStream _NetworkStream = null;
        private TcpClient _Client = null;
        private bool _ExitLoop = true;
        private BlockingCollection<Message> _Queue = new BlockingCollection<Message>();
        public delegate void dOnMessage(Message message);
        public event dOnMessage OnMessage;


        public TcpNetworkClient(string xIpAddress, int xPort, string xThreadName)
        {
            Port = xPort;
            IpAddress = xIpAddress;
            ThreadName = xThreadName;
        } //

        public void Connect(TcpClient client)
        {
            if (!_ExitLoop) return; // running already
            _ExitLoop = false;
            _Client = client;
            _NetworkStream = _Client.GetStream();

            Thread lLoopWrite = new Thread(new ThreadStart(LoopWrite));
            lLoopWrite.IsBackground = true;
            lLoopWrite.Name = ThreadName + "Write";
            lLoopWrite.Start();

            Thread lLoopRead = new Thread(new ThreadStart(LoopRead));
            lLoopRead.IsBackground = true;
            lLoopRead.Name = ThreadName + "Read";
            lLoopRead.Start();
        }

        public void Connect()
        {
            if (!_ExitLoop) return; // running already
            _ExitLoop = false;

            _Client = new TcpClient();
            _Client.Connect(IpAddress, Port);
            _NetworkStream = _Client.GetStream();

            Thread lLoopWrite = new Thread(new ThreadStart(LoopWrite));
            lLoopWrite.IsBackground = true;
            lLoopWrite.Name = ThreadName + "Write";
            lLoopWrite.Start();

            Thread lLoopRead = new Thread(new ThreadStart(LoopRead));
            lLoopRead.IsBackground = true;
            lLoopRead.Name = ThreadName + "Read";
            lLoopRead.Start();
        } //

        public void Disconnect()
        {
            _ExitLoop = true;
            _Queue.Add(null);
            if (_Client != null) _Client.Close();
        }

        public void Send(Message xHeader)
        {
            if (xHeader == null) return;
            _Queue.Add(xHeader);
        }


        private void LoopWrite()
        {
            while (!_ExitLoop)
            {
                try
                {
                    Message lObject = _Queue.Take();
                    if (lObject == null) break;

                    var messageData = lObject.Serialize();
                    _NetworkStream.Write(messageData, 0, messageData.Length);
                }
                catch (System.IO.IOException)
                {
                    if (_ExitLoop) LogManager.LogMessage(LogType.Error, "User requested client shutdown.");
                    else LogManager.LogMessage(LogType.Error, "Disconnected");
                }
                catch (Exception ex) { LogManager.LogMessage(LogType.Error, ex.ToString()); }
            }
            _ExitLoop = true;
            LogManager.LogMessage(LogType.Error, "Writer is shutting down");
        }


        private void LoopRead()
        {
            while (!_ExitLoop)
            {
                try
                {
                    var msg = MessageParser.GetMessageFromBytArr(GetBytArrFromNetworkStream());
                    //fire event
                    dOnMessage lEvent = OnMessage;
                    if (lEvent == null) continue;
                    lEvent(msg);
                }
                catch (System.IO.IOException)
                {
                    if (_ExitLoop) LogManager.LogMessage(LogType.Error, "User requested client shutdown.");
                    else LogManager.LogMessage(LogType.Error, "Disconnected");
                }
                catch (Exception ex) { LogManager.LogMessage(LogType.Error, ex.ToString()); }
            }
            LogManager.LogMessage(LogType.Error, "Reader is shutting down");
        }

        private byte[] GetBytArrFromNetworkStream()
        {
            byte[] lHeader = new byte[2];
            if (_NetworkStream.Read(lHeader, 0, 2) != 2) return null;

            var messageType = (MessageType)BitConverter.ToInt16(lHeader, 0);

            switch(messageType)
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
                    if(_NetworkStream.Read(dataLength, 0, 4) != 4) return null;
                    var dataSize = BitConverter.ToInt32(dataLength, 0);
                    var data = new byte[dataSize];

                    if (_NetworkStream.Read(data, 0, dataSize) != dataSize) return null;

                    return lHeader.Concat(dataLength.Concat(data)).ToArray();
            }
        }
    }
}
