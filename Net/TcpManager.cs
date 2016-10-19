using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;


namespace CJF.Net
{
	#region Static Class : TcpManager
	/// <summary>TCP 管理器</summary>
	public static class TcpManager
	{
		#region PInvoke define
		private const int TCP_TABLE_OWNER_PID_ALL = 5;

		[DllImport("iphlpapi.dll", SetLastError = true)]
		private static extern uint GetExtendedTcpTable(
			IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, int tblClass, int reserved);

		[DllImport("iphlpapi.dll")]
		private static extern int SetTcpEntry(ref MIB_TCPROW pTcpRow);

		[StructLayout(LayoutKind.Sequential)]
		private struct MIB_TCPROW
		{
			public TcpState dwState;
			public int dwLocalAddr;
			public int dwLocalPort;
			public int dwRemoteAddr;
			public int dwRemotePort;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MIB_TCPROW_OWNER_PID
		{
			public TcpState dwState;
			public uint dwLocalAddr;
			public int dwLocalPort;
			public uint dwRemoteAddr;
			public int dwRemotePort;
			public int dwOwningPid;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MIB_TCPTABLE_OWNER_PID
		{
			public uint dwNumEntries;
			private MIB_TCPROW_OWNER_PID table;
		}
		#endregion

		#region Private Method : MIB_TCPROW_OWNER_PID[] GetAllTcpConnections()
		private static MIB_TCPROW_OWNER_PID[] GetAllTcpConnections()
		{
			const int NO_ERROR = 0;
			const int IP_v4 = 2;
			MIB_TCPROW_OWNER_PID[] tTable = null;
			int buffSize = 0;
			GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, IP_v4, TCP_TABLE_OWNER_PID_ALL, 0);
			IntPtr buffTable = Marshal.AllocHGlobal(buffSize);
			try
			{
				if (NO_ERROR != GetExtendedTcpTable(buffTable, ref buffSize, true, IP_v4, TCP_TABLE_OWNER_PID_ALL, 0)) return null;
				MIB_TCPTABLE_OWNER_PID tab =
					(MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(buffTable, typeof(MIB_TCPTABLE_OWNER_PID));
				IntPtr rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.dwNumEntries));
				tTable = new MIB_TCPROW_OWNER_PID[tab.dwNumEntries];

				int rowSize = Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID));
				for (int i = 0; i < tab.dwNumEntries; i++)
				{
					MIB_TCPROW_OWNER_PID tcpRow =
						(MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(MIB_TCPROW_OWNER_PID));
					tTable[i] = tcpRow;
					rowPtr = (IntPtr)((int)rowPtr + rowSize);
				}
			}
			catch { }
			finally
			{
				Marshal.FreeHGlobal(buffTable);
			}
			return tTable;
		}
		#endregion

		#region Private Method : int TranslatePort(int port)
		private static int TranslatePort(int port)
		{
			return ((port & 0xFF) << 8 | (port & 0xFF00) >> 8);
		}
		#endregion

		#region Public Static Method : bool Kill(SocketInfo conn)
		/// <summary>刪除連線並清除該連線的所有資源</summary>
		/// <param name="conn">連線資訊</param>
		/// <returns>是否刪除成功</returns>
		public static bool Kill(SocketInfo conn)
		{
			if (conn == null) throw new ArgumentNullException("conn");
			MIB_TCPROW row = new MIB_TCPROW();
			row.dwState = TcpState.DeleteTcb;
#pragma warning disable 612,618
			row.dwLocalAddr = (int)conn.LocalEndPoint.Address.Address;
#pragma warning restore 612,618
			row.dwLocalPort = TranslatePort(conn.LocalEndPoint.Port);
#pragma warning disable 612,618
			row.dwRemoteAddr = (int)conn.RemoteEndPoint.Address.Address;
#pragma warning restore 612,618
			row.dwRemotePort = TranslatePort(conn.RemoteEndPoint.Port);
			return SetTcpEntry(ref row) == 0;
		}
		#endregion

		#region Public Static Method : bool Kill(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
		/// <summary>刪除連線並清除該連線的所有資源</summary>
		/// <param name="localEndPoint">本地端通訊埠</param>
		/// <param name="remoteEndPoint">遠端通訊埠</param>
		/// <returns>是否刪除成功</returns>
		public static bool Kill(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
		{
			if (localEndPoint == null) throw new ArgumentNullException("localEndPoint");
			if (remoteEndPoint == null) throw new ArgumentNullException("remoteEndPoint");
			MIB_TCPROW row = new MIB_TCPROW();
			row.dwState = TcpState.DeleteTcb;
#pragma warning disable 612,618
			row.dwLocalAddr = (int)localEndPoint.Address.Address;
#pragma warning restore 612,618
			row.dwLocalPort = TranslatePort(localEndPoint.Port);
#pragma warning disable 612,618
			row.dwRemoteAddr = (int)remoteEndPoint.Address.Address;
#pragma warning restore 612,618
			row.dwRemotePort = TranslatePort(remoteEndPoint.Port);
			return SetTcpEntry(ref row) == 0;
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetTable()
		/// <summary>取得本機所有連線的資訊表</summary>
		/// <returns></returns>
		public static SocketInfo[] GetTable()
		{
			MIB_TCPROW_OWNER_PID[] tcpRows = GetAllTcpConnections();
			if (tcpRows == null) return null;
			List<SocketInfo> list = new List<SocketInfo>();
			SocketInfo conn = null;
			foreach (MIB_TCPROW_OWNER_PID row in tcpRows)
			{
				int localPort = TranslatePort(row.dwLocalPort);
				int remotePort = TranslatePort(row.dwRemotePort);
				conn =
					new SocketInfo(
						new IPEndPoint(row.dwLocalAddr, localPort),
						new IPEndPoint(row.dwRemoteAddr, remotePort),
						row.dwState,
						row.dwOwningPid
						);
				list.Add(conn);
			}
			return list.ToArray();
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetTableByProcess(int pid)
		/// <summary>依應用程式行程代碼取得連線資訊表</summary>
		/// <param name="pid">應用程式行程代碼</param>
		/// <returns></returns>
		public static SocketInfo[] GetTableByProcess(int pid)
		{
			MIB_TCPROW_OWNER_PID[] tcpRows = GetAllTcpConnections();
			if (tcpRows == null) return null;
			List<SocketInfo> list = new List<SocketInfo>();
			SocketInfo conn = null;
			foreach (MIB_TCPROW_OWNER_PID row in tcpRows)
			{
				if (row.dwOwningPid == pid)
				{
					int localPort = TranslatePort(row.dwLocalPort);
					int remotePort = TranslatePort(row.dwRemotePort);
					conn =
						new SocketInfo(
							new IPEndPoint(row.dwLocalAddr, localPort),
							new IPEndPoint(row.dwRemoteAddr, remotePort),
							row.dwState,
							pid);
					list.Add(conn);
				}
			}
			return list.ToArray();
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetTableByCurrentProcess()
		/// <summary>取得應用程式本身的連線資訊表</summary>
		/// <returns></returns>
		public static SocketInfo[] GetTableByCurrentProcess()
		{
			return GetTableByProcess(Process.GetCurrentProcess().Id);
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetServerClients(IPEndPoint serverPoint, int pid)
		/// <summary>利用應用程式行程代碼取得本端伺服器所有連線資訊</summary>
		/// <param name="serverPoint">本端伺服器傾聽的通訊埠資訊</param>
		/// <param name="pid">應用程式行程代碼</param>
		/// <returns></returns>
		public static SocketInfo[] GetServerClients(IPEndPoint serverPoint, int pid)
		{
			SocketInfo[] sis = GetTableByProcess(pid);
			return Array.FindAll<SocketInfo>(sis, si => si.LocalEndPoint.Equals(serverPoint));
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetServerClients(IPEndPoint serverPoint)
		/// <summary>取得應用程式本身的本端伺服器所有連線資訊</summary>
		/// <param name="serverPoint">本端伺服器傾聽的通訊埠資訊</param>
		/// <returns></returns>
		public static SocketInfo[] GetServerClients(IPEndPoint serverPoint)
		{
			SocketInfo[] sis = GetTableByCurrentProcess();
			return Array.FindAll<SocketInfo>(sis, si => si.LocalEndPoint.Equals(serverPoint));
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetRemoteServerTable(IPEndPoint serverPoint, int pid)
		/// <summary>利用應用程式行程代碼取得遠端伺服器所有連線資訊</summary>
		/// <param name="serverPoint">遠端伺服器傾聽的通訊埠資訊</param>
		/// <param name="pid">應用程式行程代碼</param>
		/// <returns></returns>
		public static SocketInfo[] GetRemoteServerTable(IPEndPoint serverPoint, int pid)
		{
			SocketInfo[] sis = GetTableByProcess(pid);
			return Array.FindAll<SocketInfo>(sis, si => si.RemoteEndPoint.Equals(serverPoint));
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetRemoteServerTable(IPEndPoint serverPoint)
		/// <summary>取得應用程式本身的遠端伺服器所有連線資訊</summary>
		/// <param name="serverPoint">遠端伺服器傾聽的通訊埠資訊</param>
		/// <returns></returns>
		public static SocketInfo[] GetRemoteServerTable(IPEndPoint serverPoint)
		{
			SocketInfo[] sis = GetTableByCurrentProcess();
			return Array.FindAll<SocketInfo>(sis, si => si.RemoteEndPoint.Equals(serverPoint));
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetLocalCloseWaitTable(IPEndPoint ep, int pid)
		/// <summary>利用應用程式行程代碼取得本端連線狀態為 CLOSE_WAIT 的連線</summary>
		/// <param name="ep">本端伺服器傾聽的通訊埠資訊</param>
		/// <param name="pid">應用程式行程代碼</param>
		/// <returns></returns>
		public static SocketInfo[] GetLocalCloseWaitTable(IPEndPoint ep, int pid)
		{
			SocketInfo[] sis = GetTableByProcess(pid);
			if (sis == null)
				return null;
			else
				return Array.FindAll<SocketInfo>(sis, si => si != null && si.LocalEndPoint.Equals(ep) && si.State == TcpState.CloseWait);
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetLocalCloseWaitTable(IPEndPoint ep)
		/// <summary>取得應用程式本身的本端連線狀態為 CLOSE_WAIT 的連線</summary>
		/// <param name="ep">本端伺服器傾聽的通訊埠資訊</param>
		/// <returns></returns>
		public static SocketInfo[] GetLocalCloseWaitTable(IPEndPoint ep)
		{
			SocketInfo[] sis = GetTableByCurrentProcess();
			if (sis == null)
				return null;
			else
				return Array.FindAll<SocketInfo>(sis, si => si != null && si.LocalEndPoint.Equals(ep) && si.State == TcpState.CloseWait);
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetRemoteCloseWaitTable(IPEndPoint ep, int pid)
		/// <summary>利用應用程式行程代碼取得遠端連線狀態為 CLOSW_WAIT 的連線</summary>
		/// <param name="ep">遠端伺服器傾聽的通訊埠資訊</param>
		/// <param name="pid">應用程式行程代碼</param>
		/// <returns></returns>
		public static SocketInfo[] GetRemoteCloseWaitTable(IPEndPoint ep, int pid)
		{
			SocketInfo[] sis = GetTableByProcess(pid);
			return Array.FindAll<SocketInfo>(sis, si => si.RemoteEndPoint.Equals(ep) && si.State == TcpState.CloseWait);
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetRemoteCloseWaitTable(IPEndPoint ep)
		/// <summary>取得應用程式本身的遠端連線狀態為 CLOSW_WAIT 的連線</summary>
		/// <param name="ep">遠端伺服器傾聽的通訊埠資訊</param>
		/// <returns></returns>
		public static SocketInfo[] GetRemoteCloseWaitTable(IPEndPoint ep)
		{
			SocketInfo[] sis = GetTableByCurrentProcess();
			return Array.FindAll<SocketInfo>(sis, si => si.RemoteEndPoint.Equals(ep) && si.State == TcpState.CloseWait);
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetLocalTable(IPEndPoint ep, TcpState status, int pid)
		/// <summary>利用應用程式行程代碼與通訊埠狀態取得本端連線表</summary>
		/// <param name="ep">本端伺服器傾聽的通訊埠資訊</param>
		/// <param name="status">通訊埠狀態</param>
		/// <param name="pid">應用程式行程代碼</param>
		/// <returns></returns>
		public static SocketInfo[] GetLocalTable(IPEndPoint ep, TcpState status, int pid)
		{
			SocketInfo[] sis = GetTableByProcess(pid);
			if (sis == null)
				return null;
			else
				return Array.FindAll<SocketInfo>(sis, si => si != null && si.LocalEndPoint.Equals(ep) && si.State == status);
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetLocalTable(IPEndPoint ep, TcpState status)
		/// <summary>依通訊埠狀態取得應用程式本身的本端連線表</summary>
		/// <param name="ep">本端伺服器傾聽的通訊埠資訊</param>
		/// <param name="status">通訊埠狀態</param>
		/// <returns></returns>
		public static SocketInfo[] GetLocalTable(IPEndPoint ep, TcpState status)
		{
			SocketInfo[] sis = GetTableByCurrentProcess();
			if (sis == null)
				return null;
			else
				return Array.FindAll<SocketInfo>(sis, si => si != null && si.LocalEndPoint.Equals(ep) && si.State == status);
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetRemoteTable(IPEndPoint ep, TcpState status, int pid)
		/// <summary>利用應用程式行程代碼與通訊埠狀態取得遠端連線表</summary>
		/// <param name="ep">遠端伺服器傾聽的通訊埠資訊</param>
		/// <param name="status">通訊埠狀態</param>
		/// <param name="pid">應用程式行程代碼</param>
		/// <returns></returns>
		public static SocketInfo[] GetRemoteTable(IPEndPoint ep, TcpState status, int pid)
		{
			SocketInfo[] sis = GetTableByProcess(pid);
			if (sis == null)
				return sis;
			else
				return Array.FindAll<SocketInfo>(sis, si => si != null && si.RemoteEndPoint.Equals(ep) && si.State == status);
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetRemoteTable(IPEndPoint ep, TcpState status)
		/// <summary>依通訊埠狀態取得應用程式本身的遠端連線表</summary>
		/// <param name="ep">遠端伺服器傾聽的通訊埠資訊</param>
		/// <param name="status">通訊埠狀態</param>
		/// <returns></returns>
		public static SocketInfo[] GetRemoteTable(IPEndPoint ep, TcpState status)
		{
			SocketInfo[] sis = GetTableByCurrentProcess();
			if (sis == null)
				return sis;
			else
				return Array.FindAll<SocketInfo>(sis, si => si != null && si.RemoteEndPoint.Equals(ep) && si.State == status);
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetStatusTable(TcpState status, int pid)
		/// <summary>利用應用程式行程代碼與通訊埠狀態取得連線表</summary>
		/// <param name="status">通訊埠狀態</param>
		/// <param name="pid">應用程式行程代碼</param>
		/// <returns></returns>
		public static SocketInfo[] GetStatusTable(TcpState status, int pid)
		{
			SocketInfo[] sis = GetTableByProcess(pid);
			if (sis == null)
				return sis;
			else
				return Array.FindAll<SocketInfo>(sis, si => si != null && si.State == status);
		}
		#endregion

		#region Public Static Method : SocketInfo[] GetStatusTable(TcpState status)
		/// <summary>依通訊埠狀態取得應用程式本身的連線表</summary>
		/// <param name="status">通訊埠狀態</param>
		/// <returns></returns>
		public static SocketInfo[] GetStatusTable(TcpState status)
		{
			SocketInfo[] sis = GetTableByCurrentProcess();
			if (sis == null)
				return sis;
			else
				return Array.FindAll<SocketInfo>(sis, si => si != null && si.State == status);
		}
		#endregion

	}
	#endregion

	#region Public Sealed Class : SocketInfo
	/// <summary>TCP 連線資訊</summary>
	public sealed class SocketInfo : IEquatable<SocketInfo>, IEqualityComparer<SocketInfo>
	{
		private readonly IPEndPoint _LocalEndPoint;
		private readonly IPEndPoint _RemoteEndPoint;
		private readonly TcpState _State;
		private readonly Process _Process;

		#region Construct Methods : SocketInfo(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, TcpState state)
		/// <summary>使用指定的本端與遠端位址、通訊埠編號與連線狀態來初始化 CJF.Net.SocketInfo 類別的新執行個體。</summary>
		/// <param name="localEndPoint">本地端位址和通訊埠編號的 System.Net.IPEndPoint 類別</param>
		/// <param name="remoteEndPoint">本地端位址和通訊埠編號的 System.Net.IPEndPoint 類別</param>
		/// <param name="state">連線狀態</param>
		/// <param name="ownerProcessId">擁有的應用程式行程代碼</param>
		public SocketInfo(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, TcpState state, int ownerProcessId)
		{
			if (localEndPoint == null) throw new ArgumentNullException("localEndPoint");
			if (remoteEndPoint == null) throw new ArgumentNullException("remoteEndPoint");
			_LocalEndPoint = localEndPoint;
			_RemoteEndPoint = remoteEndPoint;
			_State = state;
			_Process = Process.GetProcessById(ownerProcessId);
		}
		#endregion

		#region Public Properties
		/// <summary>取得值，本地端通訊埠資訊</summary>
		public IPEndPoint LocalEndPoint { get { return _LocalEndPoint; } }
		/// <summary>取得值，遠端通訊埠資訊</summary>
		public IPEndPoint RemoteEndPoint { get { return _RemoteEndPoint; } }
		/// <summary>取得值，連線狀態</summary>
		public TcpState State { get { return _State; } }
		/// <summary>取得值，擁有的應用程式資訊</summary>
		public Process OwnerProcess { get { return _Process; } }
		#endregion

		#region Public Method : bool Equals(SocketInfo si1, SocketInfo si2)
		/// <summary>判斷指定的兩個 SocketInfo 是否相等。</summary>
		/// <param name="si1">欲比較的 CJF.Net.SocketInfo。</param>
		/// <param name="si2">被比較的 CJF.Net.SocketInfo。</param>
		/// <returns>如果兩個 CJF.Net.SocketInfo 相等，則為 true，否則為 false。</returns>
		public bool Equals(SocketInfo si1, SocketInfo si2)
		{
			return (si1.LocalEndPoint.Equals(si2.LocalEndPoint) && si1.RemoteEndPoint.Equals(si2.RemoteEndPoint));
		}
		#endregion

		#region Public Override Method : int GetHashCode()
		/// <summary>做為 SocketInfo 的雜湊函式。</summary>
		/// <returns>目前 CJF.Net.SocketInfo 的雜湊程式碼。</returns>
		public override int GetHashCode()
		{
			return this.GetHashCode(this);
		}
		#endregion

		#region Public Method : int GetHashCode(SocketInfo obj)
		/// <summary>做為 SocketInfo 的雜湊函式。</summary>
		/// <param name="obj">CJF.Net.SocketInfo類別物件</param>
		/// <returns>目前 CJF.Net.SocketInfo 的雜湊程式碼。</returns>
		public int GetHashCode(SocketInfo obj)
		{
			return obj.LocalEndPoint.GetHashCode() ^ obj.RemoteEndPoint.GetHashCode();
		}
		#endregion

		#region Public Method : bool Equals(SocketInfo other)
		/// <summary>判斷指定的 SocketInfo 和目前的 SocketInfo 是否相等。</summary>
		/// <param name="other">CJF.Net.SocketInfo，要與目前的 SocketInfo 比較。</param>
		/// <returns>如果指定的 CJF.Net.SocketInfo 和目前的 CJF.Net.SocketInfo 相等，則為 true，否則為 false。</returns>
		public bool Equals(SocketInfo other)
		{
			return Equals(this, other);
		}
		#endregion

		#region Public Override Method : bool Equals(object obj)
		/// <summary>判斷指定的 SocketInfo 和目前的 SocketInfo 是否相等。</summary>
		/// <param name="obj">CJF.Net.SocketInfo，要與目前的 SocketInfo 比較。</param>
		/// <returns>如果指定的 CJF.Net.SocketInfo 和目前的 CJF.Net.SocketInfo 相等，則為 true，否則為 false。</returns>
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is SocketInfo))
				return false;
			return Equals(this, (SocketInfo)obj);
		}
		#endregion
	}
	#endregion
}
