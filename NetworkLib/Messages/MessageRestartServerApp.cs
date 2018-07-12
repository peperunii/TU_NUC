namespace Network.Messages
{
    public class MessageRestartServerApp : Message
    {
        public MessageRestartServerApp()
        {
            this.type = MessageType.RestartServerApp;
            this.info = null;
        }

        public override byte[] Serialize()
        {
            return this.GetBytesForNumberShort((ushort)this.type);
        }
    }
}
