using System;
using System.Net;

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
