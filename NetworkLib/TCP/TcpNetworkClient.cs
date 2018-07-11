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
using System.IO;

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
        private byte[] endOfMessageByteSequence;

        private int MAX_MESSAGE_SIZE = (1920 * 1080 * 4) + 1500;

        private BlockingCollection<Message> _Queue = new BlockingCollection<Message>();
        public delegate void dOnMessage(Message message);
        public event dOnMessage OnMessage;

        public delegate void dOnServerDisconnected();
        public event dOnServerDisconnected OnServerDisconnected;

        public TcpNetworkClient(string xIpAddress, int xPort, string xThreadName)
        {
            endOfMessageByteSequence = Encoding.ASCII.GetBytes("EndOfMessage");

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
                    //_Client.ReceiveBufferSize = MAX_MESSAGE_SIZE;
                    //_Client.SendBufferSize = MAX_MESSAGE_SIZE;
                    //_Client.SendTimeout = 1500;
                    //_Client.ReceiveTimeout = 1500;

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
                    
                    var messageData = lObject.Serialize().Concat(this.endOfMessageByteSequence).ToArray();
                    _NetworkStream.Write(messageData, 0, messageData.Length);
                    Thread.Sleep(1);
                }
                catch (System.IO.IOException ex)
                {
                    Console.WriteLine("Fail sending.." + ex.ToString());
                    if (_ExitLoop) LogManager.LogMessage(LogType.Error, LogLevel.Communication, "User requested client shutdown.");
                    else LogManager.LogMessage(LogType.Error, LogLevel.Errors, "Disconnected");
                }
                catch (Exception ex) { Console.WriteLine("Fail sending.." + ex.ToString()); LogManager.LogMessage(LogType.Error, LogLevel.Errors, ex.ToString()); }
            }
            _ExitLoop = true;
            LogManager.LogMessage(LogType.Error, LogLevel.Errors, "Writer is shutting down");
        }
        
        private void LoopRead()
        {
            while (!_ExitLoop)
            {
                try
                {
                    var listArrays = GetBytArrFromNetworkStream();
                    foreach (var byteArr in listArrays)
                    {
                        var msg = MessageParser.GetMessageFromBytArr(byteArr);
                        //fire event
                        dOnMessage lEvent = OnMessage;
                        if (lEvent == null) continue;
                        lEvent(msg);
                        Thread.Sleep(1);
                    }
                }
                catch (System.IO.IOException ex)
                {
                    if (_ExitLoop) LogManager.LogMessage(LogType.Error, LogLevel.Communication, "User requested client shutdown.");
                    else LogManager.LogMessage(LogType.Error, LogLevel.Errors, "Disconnected");

                    this.RestartClient();
                }
                catch (Exception ex) { this.RestartClient(); LogManager.LogMessage(LogType.Error, LogLevel.Errors, ex.ToString()); }
            }
            LogManager.LogMessage(LogType.Error, LogLevel.Errors, "Reader is shutting down");
        }

        private void RestartClient()
        {
            Configuration.IsServerDisconnected = true;
            LogManager.LogMessage(LogType.Warning, LogLevel.Communication, "Restarting ...");

            Thread.Sleep(3000);

            this.Disconnect();

            //fire event
            dOnServerDisconnected lEvent = OnServerDisconnected;
            if (lEvent != null)
            {
                lEvent();
            }
        }

        private List<byte[]> GetBytArrFromNetworkStream()
        {
            try
            {
                byte[] lHeader = new byte[2];

                if (_NetworkStream.Read(lHeader, 0, 2) != 2)
                {
                    return new List<byte[]>();
                }

                var messageType = (MessageType)BitConverter.ToInt16(lHeader, 0);

                if ((ushort)messageType > Enum.GetValues(typeof(MessageType)).Length)
                {
                    return new List<byte[]>();
                }
                if (messagesWithHeaderOnly.Contains(messageType))
                {
                    return new List<byte[]>() { lHeader };
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
                            return new List<byte[]>();
                        }
                        var data = new byte[dataSize];

                        var readBuffer = new byte[1024];
                        using (var memoryStream = new MemoryStream())
                        {
                            do
                            {
                                int numberOfBytesRead = _NetworkStream.Read(readBuffer, 0, readBuffer.Length);
                                memoryStream.Write(readBuffer, 0, numberOfBytesRead);

                                if (!_NetworkStream.DataAvailable)
                                    System.Threading.Thread.Sleep(1);
                            }
                            while (_NetworkStream.DataAvailable);

                            data = memoryStream.ToArray();
                        }

                        //if (_NetworkStream.Read(data, 0, dataSize) != dataSize) return null;
                        var fullMessage = lHeader.Concat(dataLength.Concat(data)).ToArray();

                        return this.GetListOfArraysUsingSeparator(fullMessage);
                    }
                    catch (Exception ex)
                    {
                        return new List<byte[]>();
                    }
                }
            }
            catch (Exception ex)
            {
                return new List<byte[]>();
            }
        }

        private List<byte[]> GetListOfArraysUsingSeparator(byte[] fullMessage)
        {
            var listArrays = new List<byte[]>();

            var indexEnd = 0;
            while (indexEnd < fullMessage.Length)
            {
                var indexOfSeparator = SearchBytes(fullMessage, this.endOfMessageByteSequence);

                if (indexOfSeparator == -1) break;

                listArrays.Add(fullMessage.SubArray(indexEnd, indexOfSeparator));
                indexEnd += (indexOfSeparator + this.endOfMessageByteSequence.Length);
            }

            return listArrays;
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
    }
}
