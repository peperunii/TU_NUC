using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public static class MessageParser
    {
        public static Message GetMessageFromBytArr(byte [] messageArr)
        {
            if (messageArr != null && messageArr.Length >= 2)
            {
                var messageType = BitConverter.ToUInt16(new byte[] { messageArr[0], messageArr[1] }, 0);
                switch((MessageType)messageType)
                {
                    /*Discovery messages*/
                    case MessageType.Discovery:
                        return new MessageDiscovery(BitConverter.ToInt16(messageArr, 6));

                    case MessageType.DiscoveryResponse:
                        return new MessageDiscoveryResponse(messageArr.SubArray(6));

                    /*Main Group of Messages*/
                    case MessageType.Info:
                        return new MessageSendInfoToServer(Encoding.ASCII.GetString(messageArr.SubArray(6)));

                    case MessageType.KeepAlive:
                        return new MessageKeepAlive();

                    case MessageType.TimeSyncRequest:
                        return new MessageTimeSync();

                    case MessageType.TimeInfo:
                        return new MessageTimeInfo(messageArr.SubArray(6));

                    
                        
                    /*Restart Requests*/
                    case MessageType.RestartClientApp:
                        return new MessageRestartClientApp(messageArr.SubArray(6));

                    case MessageType.RestartClientDevice:
                        return new MessageRestartClientDevice(messageArr.SubArray(6));

                    case MessageType.RestartServerApp:
                        return new MessageRestartServerApp();

                    case MessageType.RestartServerDevice:
                        return new MessageRestartServerDevice();
                        
                    case MessageType.ShutdownDevice:
                        return new MessageShutdownDevice(messageArr.SubArray(6));


                    case MessageType.CameraStart:
                        return new MessageCameraStart(messageArr.SubArray(6));

                    case MessageType.CameraStop:
                        return new MessageCameraStop(messageArr.SubArray(6));


                    case MessageType.ConnectedClients:
                        return new MessageConnectedClients(messageArr.SubArray(6));

                    case MessageType.GetConnectedClients:
                        return new MessageGetConnectedClients();


                    case MessageType.ReloadConfiguration:
                        return new MessageReloadConfiguration();

                    case MessageType.GetConfigurationPerClient:
                        return new MessageGetConfigurationPerClient(messageArr.SubArray(6));
                        
                    case MessageType.ShowConfigurationPerClient:
                        return
                            new MessageSetConfigurationPerClient(
                                (DeviceID)BitConverter.ToUInt16(
                                    new byte[] { messageArr[6], messageArr[7] }, 0),
                                Encoding.ASCII.GetString(messageArr.SubArray(8)));

                    case MessageType.StoreConfigurationPerClient:
                        return new MessageStoreConfigurationPerClient(
                                (DeviceID)BitConverter.ToUInt16(
                                    new byte[] { messageArr[6], messageArr[7] }, 0),
                                Encoding.ASCII.GetString(messageArr.SubArray(8)));


                    /*Frames and calibration*/
                    case MessageType.Calibration:
                        return null;

                    case MessageType.CalibrationRequest:
                        return new MessageCalibrationRequest();


                    case MessageType.ColorFrame:
                        return new MessageColorFrame(messageArr.SubArray(6));

                    case MessageType.DepthFrame:
                        return null;

                    case MessageType.IRFrame:
                        return null;

                    case MessageType.Skeleton:
                        return null;


                    case MessageType.ColorFrameRequest:
                        return null;

                    case MessageType.DepthFrameRequest:
                        return null;

                    case MessageType.IRFrameRequest:
                        return null;

                    case MessageType.SkeletonRequest:
                        return null;
                        

                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static T[] SubArray<T>(this T[] data, int index)
        {
            var length = data.Length - index;
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
