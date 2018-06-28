using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUC_Controller.NetworkWorker
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
