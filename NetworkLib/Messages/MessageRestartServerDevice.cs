namespace Network.Messages
{
    public class MessageRestartServerDevice : Message
    {
        public MessageRestartServerDevice()
        {
            this.type = MessageType.RestartServerDevice;
            this.info = null;
        }

        public override byte[] Serialize()
        {
            return this.GetBytesForNumberShort((ushort)this.type);
        }
    }
}
