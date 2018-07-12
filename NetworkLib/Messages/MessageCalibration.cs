using Network.Utils;

namespace Network.Messages
{
    public class MessageCalibration : Message
    {
        public MessageCalibration()
        {
            this.type = MessageType.Calibration;
            this.info = new byte[] { };
        }

        public override byte[] Serialize()
        {
            var bytes = this.GetBytesForNumberShort((ushort)this.type);
            var bytesInfo = this.GetBytesOfInfo();
            var lenghtInfoBytes = this.GetBytesForNumberInt((int)bytesInfo.Length);

            return bytes.ConcatenatingArrays(lenghtInfoBytes.ConcatenatingArrays(bytesInfo));
        }

        private byte[] GetBytesOfInfo()
        {
            return this.info as byte [];
        }
    }
}
