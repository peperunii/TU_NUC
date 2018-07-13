using Network.Logger;
using Network.Messages;
using Network.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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

        private int BUFFER_SIZE = 655360;

        private BlockingCollection<Message> _Queue = new BlockingCollection<Message>();
        public delegate void dOnMessage(Message message);
        public event dOnMessage OnMessage;

        public delegate void dOnServerDisconnected();
        public event dOnServerDisconnected OnServerDisconnected;

        public delegate void dOnTCPMessageArived(byte [] byteArr);
        public event dOnTCPMessageArived OnTCPMessageArrived;

        public TcpNetworkClient(string xIpAddress, int xPort, string xThreadName)
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
            }
        } //

        public void Connect(TcpClient client)
        {
            client.ReceiveBufferSize = BUFFER_SIZE;
            client.SendBufferSize = BUFFER_SIZE;

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

        private void TcpNetworkClient_OnTCPMessageArrived(byte[] byteArr)
        {
            try
            {
                var msg = MessageParser.GetMessageFromBytArr(byteArr);
                //fire event
                dOnMessage lEvent = OnMessage;
                if (lEvent == null) return;
                lEvent(msg);
                Thread.Sleep(1);
            }
            catch(Exception) { }
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
                    _Client.ReceiveBufferSize = BUFFER_SIZE;
                    _Client.SendBufferSize = BUFFER_SIZE;
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
                    
                    var messageData = lObject.Serialize().ConcatenatingArrays(this.endOfMessageByteSequence);
                    _NetworkStream.WriteAsync(messageData, 0, messageData.Length);
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
                    GetBytArrFromNetworkStream();
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

        private void GetBytArrFromNetworkStream()
        {
            try
            {
                byte[] lHeader = new byte[2];

                if (_NetworkStream.Read(lHeader, 0, 2) != 2)
                {
                    this.TriggerEmptyMessage();
                }

                var messageType = (MessageType)BitConverter.ToInt16(lHeader, 0);

                if ((ushort)messageType > Enum.GetValues(typeof(MessageType)).Length)
                {
                    this.TriggerEmptyMessage();
                }
                if (messagesWithHeaderOnly.Contains(messageType))
                {
                    //fire event
                    dOnTCPMessageArived lEvent = OnTCPMessageArrived;
                    if (lEvent != null)
                    {
                        lEvent(lHeader);
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
                            this.TriggerEmptyMessage();
                        }
                        var data = new byte[dataSize];

                        var readBuffer = new byte[16536];
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
                        
                        var fullMessage = lHeader.ConcatenatingArrays(dataLength.ConcatenatingArrays(data));

                        this.GetListOfArraysUsingSeparator(fullMessage);
                    }
                    catch (Exception ex)
                    {
                        this.TriggerEmptyMessage();
                    }
                }
            }
            catch (Exception ex)
            {
                this.TriggerEmptyMessage();
            }
        }

        private void TriggerEmptyMessage()
        {
            //fire event
            dOnTCPMessageArived lEvent = OnTCPMessageArrived;
            if (lEvent != null)
            {
                lEvent(new byte[] { });
            }
        }

        private void GetListOfArraysUsingSeparator(byte[] fullMessage)
        {
            var indexEnd = 0;
            while (indexEnd < fullMessage.Length)
            {
                var indexOfSeparator = SearchBytes(fullMessage, this.endOfMessageByteSequence);

                if (indexOfSeparator == -1) break;
                
                //fire event
                dOnTCPMessageArived lEvent = OnTCPMessageArrived;
                if (lEvent != null)
                {
                    lEvent(fullMessage.SubArray(indexEnd, indexOfSeparator));
                }

                indexEnd += (indexOfSeparator + this.endOfMessageByteSequence.Length);
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
    }
}
