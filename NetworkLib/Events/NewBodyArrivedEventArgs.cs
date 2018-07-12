using Network;
using Network.Messages;
using System;
using System.Collections.Generic;

namespace NetworkLib.Events
{
    public class NewBodyArrivedEventArgs : EventArgs
    {
        public DeviceID DeviceID { get; set; }
        public List<Skeleton> BodiesList { get; set; }

        public NewBodyArrivedEventArgs(DeviceID deviceID, List<Skeleton> bodiesList)
        {
            this.DeviceID = deviceID;
            this.BodiesList = bodiesList;
        }
    }
}
