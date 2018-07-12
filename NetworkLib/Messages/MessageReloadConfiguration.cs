namespace Network.Messages
{
    public class MessageReloadConfiguration : Message
    {
        public MessageReloadConfiguration()
        {
            this.type = MessageType.ReloadConfiguration;
            this.info = null;
        }

        public override byte[] Serialize()
        {
            return this.GetBytesForNumberShort((ushort)this.type);
        }
    } 
}
