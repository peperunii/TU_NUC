using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class NUC
    {
        public DeviceID deviceID;
        public IPEndPoint ip;
        
        public NUC(DeviceID deviceID, IPEndPoint ipEndPoint)
        {
            this.deviceID = deviceID;
            this.ip = ipEndPoint;
        }

        public override bool Equals(object obj)
        {
            var item = obj as NUC;

            if (item == null)
            {
                return false;
            }

            return this.deviceID == item.deviceID;
        }

        public override int GetHashCode()
        {
            return this.deviceID.GetHashCode();
        }
    }
}
