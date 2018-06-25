using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Network.Discovery
{
    public class UDPClient : UdpClient
    {
        public UDPClient() : base()
        {
            //Calls the protected Client property belonging to the UdpClient base class.
            Socket s = this.Client;
            //Uses the Socket returned by Client to set an option that is not available using UdpClient.
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
        }

        public UDPClient(IPEndPoint ipLocalEndPoint) : base(ipLocalEndPoint)
        {
            //Calls the protected Client property belonging to the UdpClient base class.
            Socket s = this.Client;
            //Uses the Socket returned by Client to set an option that is not available using UdpClient.
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
        }
    }
}
