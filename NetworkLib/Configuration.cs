using Network.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Network
{
    public static class Configuration
    {
        public static IPAddress DeviceIP;
        public static DeviceID DeviceID;
        public static ushort DiscoveryPort;
        public static ushort CommunicationPort;

        public static bool logInConsole;
        public static bool logInDB;
        public static bool logInFile;
        public static string logFilename;

        private static string configFile = @"..\..\..\config\conf.txt";
        private static string identifierForDeviceID = "deviceID";
        private static string identifierForDiscoveryPort = "Discovery Port";
        private static string identifierForCommunicationPort = "Communication Port";

        private static string identifierForLogInConsole = "Log in Console";
        private static string identifierForLogInDB = "Log in DB";
        private static string identifierForLogInFile = "Log in File";
        private static string identifierForLogFilename = "Log Filename";

        static Configuration()
        {
            LoadConfiguration();
        }

        public static void LoadConfiguration()
        {
            Configuration.ParseConfigiration(File.ReadAllLines(configFile));
            var localIPs = GetLocalIPAddresses();

            foreach (var localIP in localIPs)
            {
                Configuration.DeviceIP = localIP;
                break;
            }
        }

        private static void ParseConfigiration(string[] configLines)
        {
            foreach (var line in configLines)
            {
                if (line.Contains(identifierForDeviceID))
                {
                    try
                    {
                        Configuration.DeviceID = (DeviceID)(int.Parse(GetValueFromLine(line)) - 1);
                    }
                    catch (Exception ex)
                    {
                        Configuration.DeviceID = (DeviceID)Enum.Parse(typeof(DeviceID), GetValueFromLine(line));
                    }
                }
                else if (line.Contains(identifierForDiscoveryPort))
                {
                    Configuration.DiscoveryPort = ushort.Parse(GetValueFromLine(line));
                }
                else if (line.Contains(identifierForCommunicationPort))
                {
                    Configuration.CommunicationPort = ushort.Parse(GetValueFromLine(line));
                }

                /*Logging configuration*/
                else if (line.Contains(identifierForLogInConsole))
                {
                    Configuration.logInConsole = bool.Parse(GetValueFromLine(line));
                }
                else if (line.Contains(identifierForLogInDB))
                {
                    Configuration.logInDB = bool.Parse(GetValueFromLine(line));
                }
                else if (line.Contains(identifierForLogInFile))
                {
                    Configuration.logInFile = bool.Parse(GetValueFromLine(line));
                }
                else if (line.Contains(identifierForLogFilename))
                {
                    Configuration.logFilename = GetValueFromLine(line).Trim();
                }
            }
        }

        private static string GetValueFromLine(string line)
        {
            var index = line.IndexOf(':');
            var value = line.Substring(index + 1);
            value = value.Trim();

            if (value[0] == '\"' &&
                value[value.Length - 1] == '\"')
            {
                value = value.Substring(1, value.Length - 2);
            }
            return value;
        }

        public static IEnumerable<IPAddress> GetLocalIPAddresses()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    yield return ip;
                }
            }
        }

        public static ushort GetAvailablePort()
        {
            ushort startingPort = 1000;

            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            //getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            //getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            //getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (ushort i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }
    }
}
