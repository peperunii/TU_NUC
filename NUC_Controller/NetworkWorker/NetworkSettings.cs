using Network.Discovery;
using Network.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUC_Controller.NetworkWorker
{
    public static class NetworkSettings
    {
        /*Network Settings*/
        public static DiscoverySender networkWorker;
        public static TcpNetworkClient tcpClient;
        public static ConnectionSettings connectionSettings;
    }
}
