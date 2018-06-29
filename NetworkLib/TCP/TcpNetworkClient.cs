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
using System.Reflection;

namespace Network.TCP
{
    public class TcpNetworkClient
    {
        private static List<MessageType> messagesWithHeaderOnly = null;

        public int Port { get; private set; }
        public string IpAddress { get; private set; }
        public string ThreadName { get; private set; }
        private NetworkStream _NetworkStream = null;
        private TcpClient _Client = null;
        private bool _ExitLoop = true;
        private BlockingCollection<Message> _Queue = new BlockingCollection<Message>();
        public delegate void dOnMessage(Message message);
        public event dOnMessage OnMessage;

        public delegate void dOnServerDisconnected();
        public event dOnServerDisconnected OnServerDisconnected;

        public TcpNetworkClient(string xIpAddress, int xPort, string xThreadName)
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
            }
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

            while (true)
            {
                try
                {
                    _Client = new TcpClient();
                    _Client.Connect(IpAddress, Port);
                    _NetworkStream = _Client.GetStream();

                    Configuration.IsServerDisconnected = false;

                    Thread lLoopWrite = new Thread(new ThreadStart(LoopWrite));
                    lLoopWrite.IsBackground = true;
                    lLoopWrite.Name = ThreadName + "Write";
                    lLoopWrite.Start();

                    Thread lLoopRead = new Thread(new ThreadStart(LoopRead));
                    lLoopRead.IsBackground = true;
                    lLoopRead.Name = ThreadName + "Read";
                    lLoopRead.Start();
                    break;
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void Disconnect()
        {
            _ExitLoop = true;
            _Queue.Add(null);
            if (_Client != null) _Client.Close();
        }

        public void Send(Message message)
        {
            if (message == null) return;
            _Queue.Add(message);

            if(_Queue.Count > Configuration.MAX_MESSAGES_IN_QUEUE)
            {
                for(int i = 0; i < _Queue.Count - Configuration.MAX_MESSAGES_IN_QUEUE; i++)
                    _Queue.Take();
            }
        }


        private void LoopWrite()
        {
            while (!_ExitLoop)
            {
                try
                {
                    Message lObject;
                    var result = _Queue.TryTake(out lObject);
                    if (result == false || lObject == null)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    LogManager.LogMessage(LogType.Info, "Sending... ");
                    var messageData = lObject.Serialize();
                    LogManager.LogMessage(LogType.Info, "Sending: " + messageData.Length + " bytes");
                    _NetworkStream.Write(messageData, 0, messageData.Length);
                    Thread.Sleep(1);
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
                    Thread.Sleep(1);
                }
                catch (System.IO.IOException ex)
                {
                    if (_ExitLoop) LogManager.LogMessage(LogType.Error, "User requested client shutdown.");
                    else LogManager.LogMessage(LogType.Error, "Disconnected");

                    this.RestartClient();
                }
                catch (Exception ex) { this.RestartClient(); LogManager.LogMessage(LogType.Error, ex.ToString()); }
            }
            LogManager.LogMessage(LogType.Error, "Reader is shutting down");
        }

        private void RestartClient()
        {
            Configuration.IsServerDisconnected = true;
            LogManager.LogMessage(LogType.Warning, "Restarting ...");

            Thread.Sleep(3000);

            this.Disconnect();

            //fire event
            dOnServerDisconnected lEvent = OnServerDisconnected;
            if (lEvent != null)
            {
                lEvent();
            }
        }

        private byte[] GetBytArrFromNetworkStream()
        {
            byte[] lHeader = new byte[2];
            if (_NetworkStream.Read(lHeader, 0, 2) != 2) return null;

            var messageType = (MessageType)BitConverter.ToInt16(lHeader, 0);

            if (messagesWithHeaderOnly.Contains(messageType))
            {
                return lHeader;
            }
            else
            {
                var dataLength = new byte[4];
                if (_NetworkStream.Read(dataLength, 0, 4) != 4) return null;
                var dataSize = BitConverter.ToInt32(dataLength, 0);
                var data = new byte[dataSize];

                if (_NetworkStream.Read(data, 0, dataSize) != dataSize) return null;

                return lHeader.Concat(dataLength.Concat(data)).ToArray();
            }
        }
    }
}
