using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Network.Events
{
    public class ServerFoundEventArgs : EventArgs
    {
        public IPAddress Address { get; set; }
        public int Port { get; set; }

        public ServerFoundEventArgs(IPAddress address, int port)
        {
            this.Address = address;
            this.Port = port;
        }
    }
}
