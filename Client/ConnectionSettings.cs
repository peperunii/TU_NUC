namespace Client
{
    public class ConnectionSettings
    {
        public string Address { get; }
        public int Port { get; }

        public ConnectionSettings(string address, int port)
        {
            this.Address = address;
            this.Port = port;
        }
    }
}
