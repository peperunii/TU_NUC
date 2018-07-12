namespace Network.Messages
{
    public class MessageGetConnectedClients : Message
    {
        public MessageGetConnectedClients()
        {
            this.type = MessageType.GetConnectedClients;
            this.info = null;
        }

        public override byte[] Serialize()
        {
            return this.GetBytesForNumberShort((ushort)this.type);
        }
    }
}
