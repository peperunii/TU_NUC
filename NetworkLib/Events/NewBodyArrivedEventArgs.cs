using Microsoft.Kinect;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib.Events
{
    public class NewBodyArrivedEventArgs : EventArgs
    {
        public DeviceID DeviceID { get; set; }
        public List<Body> BodiesList { get; set; }

        public NewBodyArrivedEventArgs(DeviceID deviceID, List<Body> bodiesList)
        {
            this.DeviceID = deviceID;
            this.BodiesList = bodiesList;
        }
    }
}
