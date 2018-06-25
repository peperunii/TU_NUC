using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Messages
{
    public abstract class Message
    {
        public MessageType type;
        public object info;

        public abstract byte[] Serialize();
        
        public byte[] GetBytesForNumberShort(short value)
        {
            return BitConverter.GetBytes(value);
        }

        public byte[] GetBytesForNumberInt(int value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}
