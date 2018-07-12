using System;

namespace Network.Messages
{
    public abstract class Message
    {
        public MessageType type;
        public object info;

        public abstract byte[] Serialize();
        
        public byte[] GetBytesForNumberShort(ushort value)
        {
            return BitConverter.GetBytes(value);
        }

        public byte[] GetBytesForNumberInt(int value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}
