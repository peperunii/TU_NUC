namespace Network.Messages
{
    public class MessageTimeSync : Message
    {
        public MessageTimeSync()
        {
            this.type = MessageType.TimeSyncRequest;
            this.info = null;
        }

        public override byte [] Serialize()
        {
            return this.GetBytesForNumberShort((ushort)this.type);
        }
    }
}
