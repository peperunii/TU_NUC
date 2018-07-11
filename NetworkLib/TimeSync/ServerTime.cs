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

        [PreserveSig]
        [DllImport("advapi32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        public static extern int AdjustTokenPrivileges(int tokenhandle, int disableprivs,
                                                       [MarshalAs(UnmanagedType.Struct)] ref TOKEN_PRIVILEGES Newstate,
                                                       int bufferlength, int PreivousState, int Returnlength);

        [PreserveSig]
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        public static extern int GetCurrentProcess();

        [PreserveSig]
        [DllImport("advapi32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        public static extern int LookupPrivilegeValue(string lpsystemname, string lpname,
                                                      [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);

        [PreserveSig]
        [DllImport("advapi32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        public static extern int OpenProcessToken(int ProcessHandle, int DesiredAccess, ref int tokenhandle);

        [PreserveSig]
        [DllImport("advapi32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int RegLoadKey(uint hKey, string lpSubKey, string lpFile);

        [PreserveSig]
        [DllImport("advapi32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int RegUnLoadKey(uint hKey, string lpSubKey);

        #region Nested type: LUID

        public struct LUID
        {
            public int HighPart;
            public int LowPart;
        }

        #endregion

        #region Nested type: TOKEN_PRIVILEGES

        public struct TOKEN_PRIVILEGES
        {
            public int Attributes;
            public LUID Luid;
            public int PrivilegeCount;
        }

        #endregion
        private static void SetPrivilege(string privilege)
        {
            int i1 = 0;
            var luid = new LUID();
            var token_PRIVILEGES = new TOKEN_PRIVILEGES();
            int i2 = OpenProcessToken(GetCurrentProcess(), 40, ref i1);
            if (i2 == 0)
                throw new Exception("OpenProcessToken For Privilege <" + privilege + "> Failed");
            i2 = LookupPrivilegeValue(null, privilege, ref luid);
            if (i2 == 0)
                throw new Exception("LookupPrivilegeValue For Privilege <" + privilege + "> Failed");
            token_PRIVILEGES.PrivilegeCount = 1;
            token_PRIVILEGES.Attributes = 2;
            token_PRIVILEGES.Luid = luid;
            i2 = AdjustTokenPrivileges(i1, 0, ref token_PRIVILEGES, 1024, 0, 0);
            if (i2 == 0)
                throw new Exception("AdjustTokenPrivileges For Privilege <" + privilege + "> Failed");
        }

        public static void Get(IPEndPoint localEndPoint, IPEndPoint targetEndPoint)
        {
            var client = new UDPClient(localEndPoint);
            var message = new MessageTimeSync().Serialize();

            client.Send(message, message.Length, targetEndPoint);
        }

        public static void Set(DateTime dateTime)
        {
            SetPrivilege("SeSystemtimePrivilege");
            var st = new SystemTime();
            st.Year = (ushort)dateTime.Year;
            st.Month = (ushort)dateTime.Month;
            st.Day = (ushort)dateTime.Day;
            st.Hour = (ushort)dateTime.Hour;
            st.Minute = (ushort)dateTime.Minute;
            st.Second = (ushort)dateTime.Second;
            Win32SetSystemTime(ref st);

            Console.WriteLine("Time sync: " + dateTime);
        }
    }
}
