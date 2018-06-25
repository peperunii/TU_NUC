using Network.Discovery;
using Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Network.TimeSync
{
    public struct SystemTime
    {
        public ushort Year;
        public ushort Month;
        public ushort DayOfWeek;
        public ushort Day;
        public ushort Hour;
        public ushort Minute;
        public ushort Second;
        public ushort Millisecond;
    };


    public static class ServerTime
    {
        [DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        public extern static bool Win32SetSystemTime(ref SystemTime sysTime);


        public static void Get(IPEndPoint localEndPoint, IPEndPoint targetEndPoint)
        {
            var client = new UDPClient(localEndPoint);
            var message = new MessageTimeSync().Serialize();

            client.Send(message, message.Length, targetEndPoint);
        }

        public static void Set(DateTime dateTime)
        {
            var st = new SystemTime();
            st.Year = (ushort)dateTime.Year;
            st.Month = (ushort)dateTime.Month;
            st.Day = (ushort)dateTime.Day;
            st.Hour = (ushort)dateTime.Hour;
            st.Minute = (ushort)dateTime.Minute;
            st.Second = (ushort)dateTime.Second;
            Win32SetSystemTime(ref st);
        }
    }
}
