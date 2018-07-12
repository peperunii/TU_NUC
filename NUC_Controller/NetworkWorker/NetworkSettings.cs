using Network.Discovery;
using Network.TCP;

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
