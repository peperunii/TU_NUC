using System;
using System.Collections.Generic;
using System.Net;

namespace Network.Events
{
    public class NucConnectedEventArgs : EventArgs
    {
        private Dictionary<DeviceID, IPEndPoint> connectedAddresses;
        private int deviceIndex;

        public NucConnectedEventArgs(Dictionary<DeviceID, IPEndPoint> connectedAddresses, int index)
        {
            this.ConnectedAddresses = connectedAddresses;
            this.deviceIndex = index;
        }

        public Dictionary<DeviceID, IPEndPoint> ConnectedAddresses
        {
            get
            {
                return connectedAddresses;
            }

            set
            {
                connectedAddresses = value;
            }
        }
    }
}
