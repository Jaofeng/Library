/* Copy From : http://blog.chdz1.com/NETjs/254.html */
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace CJF.Net.Routing
{
    #region Public Static Class : RouteTableManager
    /// <summary>路由表管理工具類別。</summary>
    public static class RouteTableManager
    {
        #region Public Enum : MIB_IPFORWARD_TYPE(uint)
        public enum MIB_IPFORWARD_TYPE : uint
        {
            /// <summary>Some other type not specified in RFC 1354.</summary>
            MIB_IPROUTE_TYPE_OTHER = 1,
            /// <summary>An invalid route. This value can result from a route added by an ICMP redirect.</summary>
            MIB_IPROUTE_TYPE_INVALID = 2,
            /// <summary>A local route where the next hop is the final destination (a local interface).</summary>
            MIB_IPROUTE_TYPE_DIRECT = 3,
            /// <summary>The remote route where the next hop is not the final destination (a remote destination).</summary>
            MIB_IPROUTE_TYPE_INDIRECT = 4
        }
        #endregion

        #region Public Enum : MIB_IPPROTO(uint)
        public enum MIB_IPPROTO : uint
        {
            /// <summary>Some other protocol not specified in RFC 1354.</summary>
            MIB_IPPROTO_OTHER = 1,
            /// <summary>A local interface.</summary>
            MIB_IPPROTO_LOCAL = 2,
            /// <summary>
            /// A static route. 
            /// This value is used to identify route information for IP routing
            /// set through network management such as the Dynamic Host Configuration
            /// Protocol (DCHP), the Simple Network Management Protocol (SNMP),
            /// or by calls to the CreateIpForwardEntry, DeleteIpForwardEntry,
            /// or SetIpForwardEntry functions.
            /// </summary>
            MIB_IPPROTO_NETMGMT = 3,
            /// <summary>The result of ICMP redirect.</summary>
            MIB_IPPROTO_ICMP = 4,
            /// <summary>The Exterior Gateway Protocol (EGP), a dynamic routing protocol.</summary>
            MIB_IPPROTO_EGP = 5,
            /// <summary>The Gateway-to-Gateway Protocol (GGP), a dynamic routing protocol.</summary>
            MIB_IPPROTO_GGP = 6,
            /// <summary>
            /// The Hellospeak protocol, a dynamic routing protocol. This is a
            /// historical entry no longer in use and was an early routing protocol
            /// used by the original ARPANET routers that ran special software
            /// called the Fuzzball routing protocol, sometimes called Hellospeak,
            /// as described in RFC 891 and RFC 1305. For more information,
            /// see http://www.ietf.org/rfc/rfc891.txt and http://www.ietf.org/rfc/rfc1305.txt.
            /// </summary>
            MIB_IPPROTO_HELLO = 7,
            /// <summary>The Berkeley Routing Information Protocol (RIP) or RIP-II, a dynamic routing protocol.</summary>
            MIB_IPPROTO_RIP = 8,
            /// <summary>
            /// The Intermediate System-to-Intermediate System (IS-IS) protocol,
            /// a dynamic routing protocol. The IS-IS protocol was developed for
            /// use in the Open Systems Interconnection (OSI) protocol suite.
            /// </summary>
            MIB_IPPROTO_IS_IS = 9,
            /// <summary>
            /// The End System-to-Intermediate System (ES-IS) protocol, a dynamic
            /// routing protocol. The ES-IS protocol was developed for use in the
            /// Open Systems Interconnection (OSI) protocol suite.
            /// </summary>
            MIB_IPPROTO_ES_IS = 10,
            /// <summary>
            /// The Cisco Interior Gateway Routing Protocol (IGRP), a dynamic routing protocol.
            /// </summary>
            MIB_IPPROTO_CISCO = 11,
            /// <summary>
            /// The Bolt, Beranek, and Newman (BBN) Interior Gateway Protocol
            /// (IGP) that used the Shortest Path First (SPF) algorithm. This
            /// was an early dynamic routing protocol.
            /// </summary>
            MIB_IPPROTO_BBN = 12,
            /// <summary>The Open Shortest Path First (OSPF) protocol, a dynamic routing protocol.</summary>
            MIB_IPPROTO_OSPF = 13,
            /// <summary>
            /// The Border Gateway Protocol (BGP), a dynamic routing protocol.
            /// </summary>
            MIB_IPPROTO_BGP = 14,
            /// <summary>
            /// A Windows specific entry added originally by a routing protocol, but which is now static.
            /// </summary>
            MIB_IPPROTO_NT_AUTOSTATIC = 10002,
            /// <summary>
            /// A Windows specific entry added as a static route from the routing user interface or a routing command.
            /// </summary>
            MIB_IPPROTO_NT_STATIC = 10006,
            /// <summary>
            /// A Windows specific entry added as a static route from the routing
            /// user interface or a routing command, except these routes do not cause Dial On Demand (DOD).
            /// </summary>
            MIB_IPPROTO_NT_STATIC_NON_DOD = 10007
        }
        #endregion

        #region Public Struct : MIB_IPINTERFACE_ROW
        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_IPINTERFACE_ROW
        {
            public uint Family;
            public ulong InterfaceLuid;
            public uint InterfaceIndex;
            public uint MaxReassemblySize;
            public ulong InterfaceIdentifier;
            public uint MinRouterAdvertisementInterval;
            public uint MaxRouterAdvertisementInterval;
            public byte AdvertisingEnabled;
            public byte ForwardingEnabled;
            public byte WeakHostSend;
            public byte WeakHostReceive;
            public byte UseAutomaticMetric;
            public byte UseNeighborUnreachabilityDetection;
            public byte ManagedAddressConfigurationSupported;
            public byte OtherStatefulConfigurationSupported;
            public byte AdvertiseDefaultRoute;
            public uint RouterDiscoveryBehavior;
            public uint DadTransmits;
            public uint BaseReachableTime;
            public uint RetransmitTime;
            public uint PathMtuDiscoveryTimeout;
            public uint LinkLocalAddressBehavior;
            public uint LinkLocalAddressTimeout;
            public uint ZoneIndice0, ZoneIndice1, ZoneIndice2, ZoneIndice3, ZoneIndice4, ZoneIndice5, ZoneIndice6, ZoneIndice7,
             ZoneIndice8, ZoneIndice9, ZoneIndice10, ZoneIndice11, ZoneIndice12, ZoneIndice13, ZoneIndice14, ZoneIndice15;
            public uint SitePrefixLength;
            public uint Metric;
            public uint NlMtu;
            public byte Connected;
            public byte SupportsWakeUpPatterns;
            public byte SupportsNeighborDiscovery;
            public byte SupportsRouterDiscovery;
            public uint ReachableTime;
            public byte TransmitOffload;
            public byte ReceiveOffload;
            public byte DisableDefaultRoutes;
        }
        #endregion

        #region Internal Struct : IPFORWARDROW
        [ComVisible(false), StructLayout(LayoutKind.Sequential)]
        internal struct IPFORWARDROW
        {
            public uint dwForwardDest;        //destination IP address.
            public uint dwForwardMask;        //Subnet mask
            public uint dwForwardPolicy;      //conditions for multi-path route. Unused, specify 0.
            public uint dwForwardNextHop;     //IP address of the next hop. Own address?
            public uint dwForwardIfIndex;     //index of interface
            public MIB_IPFORWARD_TYPE dwForwardType;        //route type
            public MIB_IPPROTO dwForwardProto;       //routing protocol.
            public uint dwForwardAge;         //age of route.
            public uint dwForwardNextHopAS;   //autonomous system number. 0 if not relevant
            public int dwForwardMetric1;     //-1 if not used (goes for all metrics)
            public int dwForwardMetric2;
            public int dwForwardMetric3;
            public int dwForwardMetric4;
            public int dwForwardMetric5;
        }
        #endregion

        #region Private Struct : IPForwardTable
        [ComVisible(false), StructLayout(LayoutKind.Sequential)]
        private struct IPForwardTable
        {
            public uint Size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public IPFORWARDROW[] Table;
        };
        #endregion

        #region DLL Import
        [DllImport("Iphlpapi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        private static extern int CreateIpForwardEntry(IPFORWARDROW pRoute);

        [DllImport("Iphlpapi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        private static extern int DeleteIpForwardEntry(IPFORWARDROW pRoute);

        [DllImport("Iphlpapi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        private static extern int SetIpForwardEntry(IPFORWARDROW pRoute);

        [DllImport("Iphlpapi.dll")]
        private static extern int GetIpInterfaceEntry(ref MIB_IPINTERFACE_ROW row);

        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto)]
        private static extern int GetBestInterface(uint DestAddr, out uint BestIfIndex);

        [DllImport("Iphlpapi.dll")]
        private static extern int GetBestRoute(uint dwDestAddr, uint dwSourceAddr, out IPFORWARDROW pBestRoute);

        [DllImport("Iphlpapi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        private static extern int GetIpForwardTable(IntPtr pIpForwardTable, ref int pdwSize, bool bOrder);

        [DllImport("Kernel32.dll")]
        private extern static int FormatMessage(int flag, ref IntPtr source, int msgid, int langid, ref string buf, int size, ref IntPtr args);
        #endregion

        #region Public Static Method : string GetErrMsg(int errCode)
        /// <summary>取得錯誤訊息。</summary>
        /// <param name="errCode">錯誤代碼。</param>
        /// <returns></returns>
        public static string GetErrMsg(int errCode)
        {
            IntPtr tempptr = IntPtr.Zero;
            string msg = null;
            FormatMessage(0x1300, ref tempptr, errCode, 0, ref msg, 255, ref tempptr);
            return msg;
        }
        #endregion

        #region Public Static Method : int GetIpForwardTable(out IPForwardRow[] ipForwardTable)
        /// <summary>取得路由表。</summary>
        /// <param name="ipForwardTable">[OUT]傳回的路由表。</param>
        /// <returns></returns>
        public static int GetIpForwardTable(out IPForwardRow[] ipForwardTable)
        {
            var fwdTable = IntPtr.Zero;
            int size = 0;
            var result = GetIpForwardTable(fwdTable, ref size, true);
            fwdTable = Marshal.AllocHGlobal(size);
            result = GetIpForwardTable(fwdTable, ref size, true);
            if (result == 0)
            {
                var res = (IPForwardTable)Marshal.PtrToStructure(fwdTable, typeof(IPForwardTable));
                ipForwardTable = new IPForwardRow[res.Size];
                IntPtr p = new IntPtr(fwdTable.ToInt64() + Marshal.SizeOf(res.Size));
                for (int i = 0; i <res.Size;i++)
                {
                    ipForwardTable[i] = new IPForwardRow((IPFORWARDROW)Marshal.PtrToStructure(p, typeof(IPFORWARDROW)));
                    p = new IntPtr(p.ToInt64() + Marshal.SizeOf(typeof(IPFORWARDROW)));
                }
            }
            else
                ipForwardTable = null;

            Marshal.FreeHGlobal(fwdTable);

            return result;
        }
        #endregion

        #region Public Static Method : int GetBestRoute(IPAddress destAddr, IPAddress sourceAddr, out IPForwardRow bestRoute)
        /// <summary>取得基礎路由。</summary>
        /// <param name="destAddr"></param>
        /// <param name="sourceAddr"></param>
        /// <param name="bestRoute"></param>
        /// <returns></returns>
        public static int GetBestRoute(IPAddress destAddr, IPAddress sourceAddr, out IPForwardRow bestRoute)
        {
            var res = GetBestRoute(IpToUint(destAddr), IpToUint(sourceAddr), out IPFORWARDROW pBestRoute);
            bestRoute = new IPForwardRow(pBestRoute);
            return res;
        }
        #endregion

        #region Public Static Method : int GetBestInterface(IPAddress destAddr, out uint bestIfIndex)
        /// <summary>取得基礎接口。</summary>
        /// <param name="destAddr"></param>
        /// <param name="bestIfIndex"></param>
        /// <returns></returns>
        public static int GetBestInterface(IPAddress destAddr, out uint bestIfIndex)
        {
            return GetBestInterface(IpToUint(destAddr), out bestIfIndex);
        }
        #endregion

        #region Public Static Method : int GetIpInterfaceEntry(uint interfaceIndex, out MIB_IPINTERFACE_ROW row)
        /// <summary>取得 IP 接口資訊。</summary>
        /// <param name="interfaceIndex"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static int GetIpInterfaceEntry(uint interfaceIndex, out MIB_IPINTERFACE_ROW row)
        {
            row = new MIB_IPINTERFACE_ROW()
            {
                Family = 2
            };
            //row.InterfaceLuid = 0;
            row.InterfaceIndex = interfaceIndex;
            return GetIpInterfaceEntry(ref row);
        }
        #endregion

        #region Public Unsae Static Method : int GetIpForwardEntry(IPAddress destAddr, IPAddress nextHop, out IPForwardRow route) - Remarked
        ///// <summary>取得單條路由資訊。</summary>
        ///// <param name="destAddr"></param>
        ///// <param name="nextHop"></param>
        ///// <param name="route"></param>
        ///// <returns></returns>
        //public unsafe static int GetIpForwardEntry(IPAddress destAddr, IPAddress nextHop, out IPForwardRow route)
        //{
        //    route = null;

        //    var res = GetIpForwardTable(out IPForwardRow[] ipForwardTable);

        //    if (res == 0)
        //    {
        //        for (int i = 0; i < ipForwardTable.Length; i++)
        //        {
        //            if (ipForwardTable[i].Dest.Equals(destAddr) && ipForwardTable[i].NextHop.Equals(nextHop))
        //            {
        //                route = ipForwardTable[i];
        //                break;
        //            }
        //        }
        //    }

        //    return res;
        //}
        #endregion

        #region Public Unsafe Static Method : int GetIpForwardEntry(IPAddress destAddr, out IPForwardRow route) - Remarked
        ///// <summary>取得單條路由資訊。</summary>
        ///// <param name="destAddr"></param>
        ///// <param name="route"></param>
        ///// <returns></returns>
        //public unsafe static int GetIpForwardEntry(IPAddress destAddr, out IPForwardRow route)
        //{
        //    route = null;
        //    var res = GetIpForwardTable(out IPForwardRow[] ipForwardTable);

        //    if (res == 0)
        //    {
        //        for (int i = 0; i < ipForwardTable.Length; i++)
        //        {
        //            if (ipForwardTable[i].Dest.Equals(destAddr))
        //            {
        //                route = ipForwardTable[i];
        //                break;
        //            }
        //        }
        //    }
        //    return res;
        //}
        #endregion

        #region Public Static Method : int CreateIpForwardEntry(IPForwardRow route)
        /// <summary>建立路由。</summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static int CreateIpForwardEntry(IPForwardRow route)
        {
            return CreateIpForwardEntry(route.GetBaseStruct());
        }
        #endregion

        #region Public Static Method : int CreateIpForwardEntry(IPAddress dest, IPAddress mask, IPAddress nextHop, uint ifIndex, int metric = 1)
        /// <summary>建立路由。</summary>
        /// <param name="dest"></param>
        /// <param name="mask"></param>
        /// <param name="nextHop"></param>
        /// <param name="ifIndex"></param>
        /// <param name="metric"></param>
        /// <returns></returns>
        public static int CreateIpForwardEntry(IPAddress dest, IPAddress mask, IPAddress nextHop, uint ifIndex, int metric = 1)
        {
            IPForwardRow route = new IPForwardRow()
            {
                Dest = dest,
                Mask = mask,
                NextHop = nextHop,
                IfIndex = ifIndex,
                Metric = metric,
                Policy = 0,
                Type = MIB_IPFORWARD_TYPE.MIB_IPROUTE_TYPE_DIRECT,
                Proto = MIB_IPPROTO.MIB_IPPROTO_NETMGMT,
                Age = 0,
                NextHopAS = 0
            };

            OperatingSystem os = Environment.OSVersion;
            if (os.Platform == PlatformID.Win32NT && os.Version.Major >= 6)
            {
                int res = GetIpInterfaceEntry(ifIndex, out MIB_IPINTERFACE_ROW row);
                if (res != 0)
                    return res;
                route.Metric = (int)row.Metric;
            }

            return CreateIpForwardEntry(route);
        }
        #endregion

        #region Public Static Method : int CreateIpForwardEntry(IPAddress dest, IPAddress mask, IPAddress nextHop, int metric = 1)
        /// <summary>建立路由。</summary>
        /// <param name="dest"></param>
        /// <param name="mask"></param>
        /// <param name="nextHop"></param>
        /// <param name="metric"></param>
        /// <returns></returns>
        public static int CreateIpForwardEntry(IPAddress dest, IPAddress mask, IPAddress nextHop, int metric = 1)
        {
            int res = GetBestInterface(nextHop, out uint bestIfIndex);
            if (res != 0)
                return res;

            return CreateIpForwardEntry(dest, mask, nextHop, bestIfIndex, metric);
        }
        #endregion

        #region Public Static Method : int CreateIpForwardEntry(IPAddress dest, IPAddress nextHop, int metric = 1)
        /// <summary>建立路由。</summary>
        /// <param name="dest"></param>
        /// <param name="nextHop"></param>
        /// <param name="metric"></param>
        /// <returns></returns>
        public static int CreateIpForwardEntry(IPAddress dest, IPAddress nextHop, int metric = 1)
        {
            return CreateIpForwardEntry(dest, IPAddress.Parse("255.255.255.255"), nextHop, metric);
        }
        #endregion

        #region Public Static Method : int SetIpForwardEntry(IPForwardRow route)
        /// <summary>[不推薦使用]修改路由。僅用於修改網觀和躍點數(？)。</summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static int SetIpForwardEntry(IPForwardRow route)
        {
            return SetIpForwardEntry(route.GetBaseStruct());
        }
        #endregion

        #region Public Static Method : int DeleteIpForwardEntry(IPForwardRow route)
        /// <summary>刪除路由</summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static int DeleteIpForwardEntry(IPForwardRow route)
        {
            return DeleteIpForwardEntry(route.GetBaseStruct());
        }
        #endregion

        #region Public Static Method : int DeleteIpForwardEntry(IPAddress destAddr, IPAddress nextHop) - Remarked
        ///// <summary>刪除路由</summary>
        ///// <param name="destAddr"></param>
        ///// <param name="nextHop"></param>
        ///// <returns></returns>
        //public static int DeleteIpForwardEntry(IPAddress destAddr, IPAddress nextHop)
        //{
        //    int res = GetIpForwardEntry(destAddr, nextHop, out IPForwardRow route);
        //    if (res != 0)
        //        return res;
        //    return DeleteIpForwardEntry(route);
        //}
        #endregion

        #region Public Static Method : int DeleteIpForwardEntry(IPAddress destAddr) - Remarked
        ///// <summary>刪除路由</summary>
        ///// <param name="destAddr"></param>
        ///// <returns></returns>
        //public static int DeleteIpForwardEntry(IPAddress destAddr)
        //{
        //    int res = GetIpForwardEntry(destAddr, out IPForwardRow route);
        //    if (res != 0)
        //        return res;
        //    return DeleteIpForwardEntry(route);
        //}
        #endregion

        #region Public Static Method : uint IpToUint(IPAddress ipAddress)
        /// <summary>將 IPAddress 轉換成 UInt 型別。</summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static uint IpToUint(IPAddress ipAddress)
        {
            string[] startIP = ipAddress.ToString().Split('.');
            uint U = uint.Parse(startIP[3]) << 24;
            U += uint.Parse(startIP[2]) << 16;
            U += uint.Parse(startIP[1]) << 8;
            U += uint.Parse(startIP[0]);
            return U;
        }
        #endregion

        #region Public Static Method : IPAddress UintToIp(uint ip)
        /// <summary>將 UInt 型別的數值轉換成 IPAddress 類別。</summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static IPAddress UintToIp(uint ip)
        {
            string ipStr = $"{ip & 0xff}.{(ip >> 8) & 0xff}.{(ip >> 16) & 0xff}.{(ip >> 24) & 0xff}";
            return IPAddress.Parse(ipStr);
        }
        #endregion
    }
    #endregion

    #region Public Class : IPForwardRow
    /// <summary>路由點資訊類別。</summary>
    public class IPForwardRow
    {
        #region Public Properties
        /// <summary>設定或取得目標 IP 位址。</summary>
        public IPAddress Dest { get; set; }
        /// <summary>設定或取得網路遮罩。</summary>
        public IPAddress Mask { get; set; }
        /// <summary>多徑路徑的條件。 未使用，請指定0。</summary>
        public uint Policy { get; set; } = 0;
        /// <summary>下一個節點的 IP 位址。</summary>
        public IPAddress NextHop { get; set; }
        /// <summary>設定或取得介面中的索引值。</summary>
        public uint IfIndex { get; set; }
        /// <summary>設定或取得路由類型。</summary>
        public RouteTableManager.MIB_IPFORWARD_TYPE Type { get; set; }
        /// <summary>設定或取得路由的協定。</summary>
        public RouteTableManager.MIB_IPPROTO Proto { get; set; }
        /// <summary>Age of route.</summary>
        public uint Age { get; set; }
        /// <summary>autonomous system number. 0 if not relevant</summary>
        public uint NextHopAS { get; set; }
        /// <summary>設定或取得測量值(？)。如果不使用，請設定 -1（適用於所有指標）</summary>
        public int Metric { get; set; }
        #endregion

        #region Public Construct Method : IPForwardRow(...)
        /// <summary></summary>
        internal IPForwardRow() { }
        /// <summary></summary>
        /// <param name="baseStruct"></param>
        internal IPForwardRow(RouteTableManager.IPFORWARDROW baseStruct)
        {
            Dest = RouteTableManager.UintToIp(baseStruct.dwForwardDest);
            Mask = RouteTableManager.UintToIp(baseStruct.dwForwardMask);
            Policy = baseStruct.dwForwardPolicy;
            NextHop = RouteTableManager.UintToIp(baseStruct.dwForwardNextHop);
            IfIndex = baseStruct.dwForwardIfIndex;
            Type = baseStruct.dwForwardType;
            Proto = baseStruct.dwForwardProto;
            Age = baseStruct.dwForwardAge;
            NextHopAS = baseStruct.dwForwardNextHopAS;
            Metric = baseStruct.dwForwardMetric1;
        }
        #endregion

        #region Public Method : RouteTableManager.MIB_IPFORWARDROW GetBaseStruct()
        /// <summary></summary>
        /// <returns></returns>
        internal RouteTableManager.IPFORWARDROW GetBaseStruct()
        {
            return new RouteTableManager.IPFORWARDROW()
            {
                dwForwardDest = RouteTableManager.IpToUint(Dest),
                dwForwardMask = RouteTableManager.IpToUint(Mask),
                dwForwardPolicy = Policy,
                dwForwardNextHop = RouteTableManager.IpToUint(NextHop),
                dwForwardIfIndex = IfIndex,
                dwForwardType = Type,
                dwForwardProto = Proto,
                dwForwardAge = Age,
                dwForwardNextHopAS = NextHopAS,
                dwForwardMetric1 = Metric,
                dwForwardMetric2 = -1,
                dwForwardMetric3 = -1,
                dwForwardMetric4 = -1,
                dwForwardMetric5 = -1
            };
        }
        #endregion
    }
    #endregion
}