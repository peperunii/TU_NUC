namespace Network.Messages
{
    public class MessageKeepAlive : Message
    {
        public MessageKeepAlive()
        {
            this.type = MessageType.KeepAlive;
            this.info = null;
        }

        public override byte[] Serialize()
        {
            return this.GetBytesForNumberShort((ushort)this.type);
        }
    }
}
