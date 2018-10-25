using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Collections.Concurrent;

namespace CJF.Net
{
	#region EventArgs For AsyncServer/TelnetServer
	/// <summary>SocketServerEventArgs 類別，供 AsyncServer/TelnetServer 產生事件用</summary>
	public class SocketServerEventArgs : EventArgs
	{
		private readonly AsyncClient _Client = null;
        private readonly byte[] _Data = null;
        private readonly Exception _Exception = null;
        private readonly EndPoint _RemoteEndPoint = null;
        private readonly IntPtr _Handle = IntPtr.Zero;
        private object _ExtraInfo = null;

		/// <summary>取得值，對應至遠端的連線物件</summary>
		public AsyncClient Client { get { return _Client; } }
		/// <summary>取得值，接收或傳送的位元組陣列資料</summary>
		public byte[] Data { get { return _Data; } }
		/// <summary>取得值，錯誤發生時，將利用此屬性回傳</summary>
		public Exception Exception { get { return _Exception; } }
		/// <summary>取得值，遠端端點資訊</summary>
		public EndPoint RemoteEndPoint { get { return _RemoteEndPoint; } }
		/// <summary>取得值，原始的控制代碼</summary>
		public IntPtr Handle { get { return _Handle; } }
		/// <summary>取得值，額外傳遞的資料</summary>
		public object ExtraInfo { get { return _ExtraInfo; } }
		/// <summary>取得值，是否為長時間未操作逾時而關閉連線</summary>
		public bool ClosedByIdle { get; internal set; }

		/// <summary>建立新的 AsyncServerEventArgs 類別</summary>
		/// <param name="ac">遠端的連線物件</param>
		/// <param name="data">接收或傳送的位元組</param>
		/// <param name="ex">錯誤類別</param>
		public SocketServerEventArgs(AsyncClient ac, byte[] data, Exception ex)
		{
			_Client = ac;
			if (ac != null)
			{
				_RemoteEndPoint = new IPEndPoint(ac.RemoteEndPoint.Address, ac.RemoteEndPoint.Port);
				_Handle = ac.Handle;
			}
			if (data != null)
			{
				_Data = new byte[data.Length];
				Array.Copy(data, _Data, data.Length);
			}
			else
				_Data = null;
			_Exception = ex;
		}

		/// <summary>建立新的 AsyncServerEventArgs 類別</summary>
		/// <param name="ac">遠端的連線物件</param>
		public SocketServerEventArgs(AsyncClient ac) : this(ac, null, null) { }

		/// <summary>建立新的 AsyncServerEventArgs 類別</summary>
		/// <param name="ac">遠端的連線物件</param>
		/// <param name="data">封包資料內容</param>
		public SocketServerEventArgs(AsyncClient ac, byte[] data) : this(ac, data, null) { }

		/// <summary>建立新的 AsyncServerEventArgs 類別</summary>
		/// <param name="handle">原始的控制代碼</param>
		/// <param name="ep">遠端端點資訊</param>
		public SocketServerEventArgs(IntPtr handle, EndPoint ep)
		{
			_Handle = handle;
			if (ep != null)
			{
				IPEndPoint ipp = (IPEndPoint)ep;
				_RemoteEndPoint = new IPEndPoint(ipp.Address, ipp.Port);
			}
		}
		/// <summary>設定額外傳遞的資料</summary>
		/// <param name="o">欲傳遞的資料</param>
		internal void SetExtraInfo(object o) { _ExtraInfo = o; }
	}
	#endregion

	#region EventArgs For Data Transfer Status
	/// <summary>DataTransEventArgs 類別，供 AsyncClient/AsyncUDP 產生事件用</summary>
	public class DataTransEventArgs : EventArgs
	{
        private readonly long _SendedBytes = 0;
		/// <summary>取得值，每秒發送的位元組數量，單位：位元組</summary>
		public long SendedBytes { get { return _SendedBytes; } }
        private readonly long _ReceivedBytes = 0;
		/// <summary>取得值，每秒接收的位元組數量，單位：位元組</summary>
		public long ReceivedBytes { get { return _ReceivedBytes; } }
        private readonly long _WaittingBytes = 0;
		/// <summary>取得值，等待發送的位元組數量，單位：位元組</summary>
		public long WaittingBytes { get { return _WaittingBytes; } }
		/// <summary>建立新的 AsyncClientEventArgs 類別</summary>
		/// <param name="received">每秒接收的位元組數量</param>
		/// <param name="sended">每秒發送的位元組數量</param>
		/// <param name="waitting">等待發送的位元組數量</param>
		public DataTransEventArgs(long sended, long received, long waitting)
		{
			_SendedBytes = sended;
			_ReceivedBytes = received;
			_WaittingBytes = waitting;
		}
	}
	#endregion

	#region EventArgs For AsyncClient
	/// <summary>AsyncClientEventArgs 類別，供 AsyncClient 產生事件用</summary>
	public class AsyncClientEventArgs : EventArgs
	{
        private readonly byte[] _Data = null;
        private readonly Exception _Exception = null;
        private readonly EndPoint _RemoteEndPoint = null;
        private readonly EndPoint _LocalEndPoint = null;
        private readonly IntPtr _Handle = IntPtr.Zero;
		private object _ExtraInfo = null;

		/// <summary>取得值，接收或傳送的位元組陣列資料</summary>
		public byte[] Data { get { return _Data; } }
		/// <summary>取得值，錯誤發生時，將利用此屬性回傳</summary>
		public Exception Exception { get { return _Exception; } }
		/// <summary>取得值，遠端端點資訊</summary>
		public EndPoint RemoteEndPoint { get { return _RemoteEndPoint; } }
		/// <summary>取得值，本地端點資訊</summary>
		public EndPoint LocalEndPoint { get { return _LocalEndPoint; } }
		/// <summary>取得值，原始的控制代碼</summary>
		public IntPtr Handle { get { return _Handle; } }
		/// <summary>取得值，額外傳遞的資料</summary>
		public object ExtraInfo { get { return _ExtraInfo; } }
		/// <summary>取得值，是否為長時間未操作逾時而關閉連線</summary>
		public bool ClosedByIdle { get; internal set; }

		/// <summary>建立新的 AsyncClientEventArgs 類別</summary>
		/// <param name="handle">原始的控制代碼</param>
		/// <param name="lp">本地端點資訊</param>
		/// <param name="ep">遠端端點資訊</param>
		/// <param name="data">接收或傳送的位元組</param>
		/// <param name="ex">錯誤類別</param>
		public AsyncClientEventArgs(IntPtr handle, EndPoint lp, EndPoint ep, byte[] data, Exception ex)
		{
			_Handle = handle;
			IPEndPoint ipp = null;
			this.ClosedByIdle = false;
			if (lp != null)
			{
				ipp = (IPEndPoint)lp;
				_LocalEndPoint = new IPEndPoint(ipp.Address, ipp.Port);
			}
			if (ep != null)
			{
				ipp = (IPEndPoint)ep;
				_RemoteEndPoint = new IPEndPoint(ipp.Address, ipp.Port);
			}
			if (data != null)
			{
				_Data = new byte[data.Length];
				Array.Copy(data, _Data, data.Length);
			}
			else
				_Data = null;
			_Exception = ex;
		}
		/// <summary>建立新的 AsyncClientEventArgs 類別</summary>
		/// <param name="handle">原始的控制代碼</param>
		/// <param name="lp">本地端點資訊</param>
		/// <param name="ep">遠端端點資訊</param>
		public AsyncClientEventArgs(IntPtr handle, EndPoint lp, EndPoint ep) : this(handle, lp, ep, null, null) { }
		/// <summary>設定額外傳遞的資料</summary>
		/// <param name="o">欲傳遞的資料</param>
		internal void SetExtraInfo(object o) { _ExtraInfo = o; }
	}
	#endregion

	#region EventArgs For AsyncUdp
	/// <summary>AsyncUdpEventArgs 類別，供 AsyncUdp 產生事件用</summary>
	public class AsyncUdpEventArgs : EventArgs
	{
        private readonly byte[] _Data = null;
        private readonly Exception _Exception = null;
        private readonly EndPoint _RemoteEndPoint = null;
        private readonly EndPoint _LocalEndPoint = null;
        private readonly IntPtr _Handle = IntPtr.Zero;
		private object _ExtraInfo = null;

		/// <summary>取得值，接收或傳送的位元組陣列資料</summary>
		public byte[] Data { get { return _Data; } }
		/// <summary>取得值，錯誤發生時，將利用此屬性回傳</summary>
		public Exception Exception { get { return _Exception; } }
		/// <summary>取得值，遠端端點資訊</summary>
		public EndPoint RemoteEndPoint { get { return _RemoteEndPoint; } }
		/// <summary>取得值，本地端點資訊</summary>
		public EndPoint LocalEndPoint { get { return _LocalEndPoint; } }
		/// <summary>取得值，原始的控制代碼</summary>
		public IntPtr Handle { get { return _Handle; } }
		/// <summary>取得值，額外傳遞的資料</summary>
		public object ExtraInfo { get { return _ExtraInfo; } }

		/// <summary>建立新的 AsyncUdpEventArgs 類別</summary>
		/// <param name="handle">原始的控制代碼</param>
		/// <param name="local">本地端點資訊</param>
		/// <param name="remote">遠端端點資訊</param>
		/// <param name="data">接收或傳送的位元組</param>
		/// <param name="ex">錯誤類別</param>
		public AsyncUdpEventArgs(IntPtr handle, EndPoint local, EndPoint remote, byte[] data, Exception ex)
		{
			_Handle = new IntPtr(handle.ToInt32());
			IPEndPoint ipp = null;
			if (remote != null)
			{
				ipp = (IPEndPoint)remote;
				_RemoteEndPoint = new IPEndPoint(ipp.Address, ipp.Port);
			}
			if (local != null)
			{
				ipp = (IPEndPoint)local;
				_LocalEndPoint = new IPEndPoint(ipp.Address, ipp.Port);
			}
			if (data != null)
			{
				_Data = new byte[data.Length];
				Array.Copy(data, _Data, data.Length);
			}
			else
				_Data = null;
			_Exception = ex;
		}
		/// <summary>建立新的 AsyncUdpEventArgs 類別</summary>
		/// <param name="handle">原始的控制代碼</param>
		/// <param name="local">本地端點資訊</param>
		/// <param name="remote">遠端端點資訊</param>
		public AsyncUdpEventArgs(IntPtr handle, EndPoint local, EndPoint remote) : this(handle, local, remote, null, null) { }
		/// <summary>建立新的 AsyncUdpEventArgs 類別</summary>
		/// <param name="handle">原始的控制代碼</param>
		/// <param name="local">本地端點資訊</param>
		/// <param name="remote">遠端端點資訊</param>
		/// <param name="data">接收或傳送的位元組</param>
		public AsyncUdpEventArgs(IntPtr handle, EndPoint local, EndPoint remote, byte[] data) : this(handle, local, remote, data, null) { }
		/// <summary>設定額外傳遞的資料</summary>
		/// <param name="o">欲傳遞的資料</param>
		internal void SetExtraInfo(object o) { _ExtraInfo = o; }
	}
	#endregion

	#region EventArgs For PingTester Result
	/// <summary>PingResultEventArgs 類別，供 PingTester 產生事件用</summary>
	public class PingResultEventArgs : EventArgs
	{
		/// <summary>建立新的 PingResultEventArgs 類別。</summary>
		/// <param name="ex">執行期間所發生的錯誤</param>
		public PingResultEventArgs(Exception ex) { this.Exception = ex; }
		/// <summary>建立新的 PingResultEventArgs 類別。</summary>
		public PingResultEventArgs() { }
		internal PingResultEventArgs(System.Net.NetworkInformation.PingReply reply)
		{
			this.Status = reply.Status;
			this.RoundtripTime = reply.RoundtripTime;
			if (this.Status == System.Net.NetworkInformation.IPStatus.Success)
			{
				this.RemoteIP = reply.Address.ToString();
				this.Ttl = reply.Options.Ttl;
				this.DataLength = reply.Buffer.Length;
			}
		}
		/// <summary>取得主機位址，該主機傳送網際網路控制訊息通訊協定 (ICMP) 回應回覆。</summary>
		public string RemoteIP { get; internal set; }
		/// <summary>取得用於傳送網際網路控制訊息通訊協定 (ICMP) 回應要求及接收對應的 ICMP 回應回覆訊息的毫秒數。</summary>
		public long RoundtripTime { get; internal set; }
		/// <summary>取得路由節點數目，這些節點可在丟棄 System.Net.NetworkInformation.Ping 資料前轉送該資料。</summary>
		public int Ttl { get; internal set; }
		/// <summary>取得在網際網路控制訊息通訊協定 (ICMP) 回應回覆訊息中所收到的資料緩衝區長度。</summary>
		public int DataLength { get; internal set; }
		/// <summary>取得嘗試傳送網際網路控制訊息通訊協定 (ICMP) 回應要求及接收對應的 ICMP 回應回覆訊息的狀態。</summary>
		public System.Net.NetworkInformation.IPStatus Status { get; internal set; }
		/// <summary>取得值，錯誤發生時，將利用此屬性回傳。</summary>
		public Exception Exception { get; private set; }
	}
	#endregion

	#region EventArgs For SslTcpServer
	/// <summary>SslTcpEventArgs 類別，供 SslTcpServer 產生事件用</summary>
	public class SslTcpEventArgs : EventArgs
	{
        private readonly byte[] _Data = null;
        private readonly Exception _Exception = null;
        private readonly EndPoint _RemoteEndPoint = null;

		/// <summary>取得值，接收或傳送的位元組陣列資料</summary>
		public byte[] Data { get { return _Data; } }
		/// <summary>取得值，錯誤發生時，將利用此屬性回傳</summary>
		public Exception Exception { get { return _Exception; } }
		/// <summary>取得值，遠端端點資訊</summary>
		public EndPoint RemoteEndPoint { get { return _RemoteEndPoint; } }

		/// <summary>建立新的 SslTcpEventArgs 類別</summary>
		/// <param name="ep">遠端使用者連線端點。</param>
		/// <param name="data">封包資料內容。</param>
		/// <param name="ex">錯誤類別。</param>
		public SslTcpEventArgs(EndPoint ep, byte[] data, Exception ex)
		{
			_RemoteEndPoint = ep;
			if (data != null)
			{
				_Data = new byte[data.Length];
				Array.Copy(data, _Data, data.Length);
			}
			else
				_Data = null;
			_Exception = ex;
		}

		/// <summary>建立新的 SslTcpEventArgs 類別</summary>
		/// <param name="ep">遠端使用者連線端點。</param>
		public SslTcpEventArgs(EndPoint ep) : this(ep, null, null) { }

		/// <summary>建立新的 SslTcpEventArgs 類別。</summary>
		/// <param name="ep">遠端使用者連線端點。</param>
		/// <param name="data">封包資料內容。</param>
		public SslTcpEventArgs(EndPoint ep, byte[] data) : this(ep, data, null) { }
	}
    #endregion

    #region Public Class : AsyncServerPoolBeenEmptyException
    /// <summary>自定錯誤類型，當預備接線池中的所有預備線使用完畢時叫用</summary>
    [Serializable]
	public class AsyncServerPoolBeenEmptyException : Exception
	{
		/// <summary>錯誤訊息</summary>
		new public string Message = "所有的頻道都已占滿，請先釋放其他的頻道";
	}
	#endregion

	#region Class : SocketAsyncEventArgsPool
	/// <summary>SocketAsyncEventArg 接線池類別物件</summary>
	class SocketAsyncEventArgsPool : IDisposable
	{
		ConcurrentStack<SocketAsyncEventArgs> m_pool;
		bool m_IsDisposed = false;

		/// <summary>初始化接線池</summary>
		public SocketAsyncEventArgsPool()
		{
			m_pool = new ConcurrentStack<SocketAsyncEventArgs>();
		}
		~SocketAsyncEventArgsPool() { Dispose(false); }

		/// <summary>新增 SocketAsyncEventArg 物件到接線池中</summary>
		/// <param name="item">SocketAsyncEventArg 物件</param>
		public void Push(SocketAsyncEventArgs item)
		{
			if (item == null)
				throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
			m_pool.Push(item);
		}

		/// <summary>自接線池中取出 SocketAsyncEventArg 物件，並移除他</summary>
		public SocketAsyncEventArgs Pop()
		{
			try
			{
				if (m_pool.TryPop(out SocketAsyncEventArgs arg))
					return arg;
				else
					return null;
			}
			catch (InvalidOperationException)
			{
				return null;
			}
		}
		/// <summary>清除接線池中內容</summary>
		public void Clear()
		{
			if (m_pool != null)
				m_pool.Clear();
		}

		/// <summary>取得接線池中剩餘的 SocketAsyncEventArg 物件</summary>
		public int Count { get { return m_pool.Count; } }

		/// <summary>釋放接線池所使用的資源。 </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing)
		{
			if (m_IsDisposed) return;
			if (disposing)
			{
				m_pool.Clear();
				m_pool = null;
			}
			m_IsDisposed = true;
		}
	}
	#endregion

	#region Class : AsyncUserToken
	class AsyncUserToken : IDisposable
	{
		List<byte> m_Received = null;

		internal AsyncUserToken(Socket s, int bufferSize, object extraInfo)
		{
			this.IsDisposed = false;
			this.Client = s;
			this.BufferSize = bufferSize;
			this.TokenKey = Guid.NewGuid();
			this.ExtraInfo = extraInfo;
			m_Received = new List<byte>(bufferSize);
		}
		public AsyncUserToken(Socket s, int bufferSize) : this(s, bufferSize, null) { }
		~AsyncUserToken() { Dispose(false); }

		public Socket Client { get; private set; }
		public int BufferSize { get; private set; }
		/// <summary>取得索引值</summary>
		public int CurrentIndex { get; private set; }
		/// <summary>取得自遠端收到的資料</summary>
		public byte[] ReceivedData { get { return m_Received.ToArray(); } }
		/// <summary></summary>
		public Guid TokenKey { get; private set; }
		public int Capacity { get { return m_Received.Capacity; } }
		public bool IsDisposed { get; private set; }
		public object ExtraInfo { get; private set; }

		#region Public Method : void SetData(SocketAsyncEventArgs args)
		/// <summary>將接收到的資料儲存製暫存區中</summary>
		/// <param name="args">需處理的 SocketAsyncEventArg 物件</param>
		public void SetData(SocketAsyncEventArgs args)
		{
			Int32 count = args.BytesTransferred;
			if ((this.CurrentIndex + count) > this.BufferSize)
			{
				throw new ArgumentOutOfRangeException("count",
					String.Format(CultureInfo.CurrentCulture, "Adding {0} bytes on buffer which has {1} bytes, the listener buffer will overflow.", count, this.CurrentIndex));
			}
			byte[] buffer = new byte[count];
			Array.Copy(args.Buffer, args.Offset, buffer, 0, count);
			m_Received.AddRange(buffer);
			this.CurrentIndex += count;
		}
		#endregion

		#region Public Method : void ClearBuffer()
		/// <summary>清除暫存區資料</summary>
		public void ClearBuffer()
		{
			m_Received.Clear();
			this.CurrentIndex = 0;
		}
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this.IsDisposed) return;
			if (disposing)
			{
				this.Client = null;
				if (m_Received != null)
					m_Received = null;
			}
			this.IsDisposed = true;
		}
		#endregion
	}
	#endregion

	#region Public Enum : ServerPerformanceCounterType
	/// <summary>定義效能監視器種類</summary>
	public enum ServerCounterType
	{
		/// <summary>總連線數</summary>
		TotalRequest,
		/// <summary>每秒請求的連線數</summary>
		RateOfRequest,
		/// <summary>目前連線數</summary>
		Connections,
		/// <summary>接收總位元組數</summary>
		TotalReceivedBytes,
		/// <summary>每秒接收位元組數</summary>
		RateOfReceivedBytes,
		/// <summary>發送總位元組數</summary>
		TotalSendedBytes,
		/// <summary>每秒發送位元組數</summary>
		RateOfSendedBytes,
		/// <summary>目前等待發送的位元組數</summary>
		BytesOfSendQueue,
		/// <summary>已使用的緩衝區</summary>
		PoolUsed,
		/// <summary>每秒使用的緩衝區數量</summary>
		RateOfPoolUse,
		/// <summary>發送失敗的總次數</summary>
		SendFail,
		/// <summary>每秒發送失敗的次數</summary>
		RateOfSendFail
	}
	#endregion

	#region PerformanceCounter AsyncServerConsts
	/// <summary>定義 AsyncServer PerformanceCounter 名稱</summary>
	public static class AsyncServerConsts
	{
		/// <summary>目前連線數</summary>
		public const string SOCKET_COUNTER_CONNECTIONS = "# Connections";
		/// <summary>總連線數</summary>
		public const string SOCKET_COUNTER_TOTAL_REQUEST = "# Total Request";
		/// <summary>接收總位元組數</summary>
		public const string SOCKET_COUNTER_TOTAL_RECEIVED = "# Total Received Bytes";
		/// <summary>發送總位元組數</summary>
		public const string SOCKET_COUNTER_TOTAL_SENDED = "# Total Sended Bytes";
		/// <summary>已使用的緩衝區</summary>
		public const string SOCKET_COUNTER_POOL_USED = "# Pool Used";
		/// <summary>發送失敗的總次數</summary>
		public const string SOCKET_COUNTER_SAND_FAIL = "# Sand Fail";
		/// <summary>每秒請求的連線數</summary>
		public const string SOCKET_COUNTER_RATE_OF_REQUEST = "Request/Sec.";
		/// <summary>每秒接收位元組數</summary>
		public const string SOCKET_COUNTER_RATE_OF_RECEIVED = "Received Bytes/Sec.";
		/// <summary>每秒發送位元組數</summary>
		public const string SOCKET_COUNTER_RATE_OF_SENDED = "Sended Bytes/Sec.";
		/// <summary>每秒使用的緩衝區數量</summary>
		public const string SOCKET_COUNTER_RATE_OF_POOL_USE = "Pool Used/Sec.";
		/// <summary>每秒發送失敗的次數</summary>
		public const string SOCKET_COUNTER_RATE_OF_SAND_FAIL = "Send Fail/Sec";
		/// <summary>等待發送的位元組數</summary>
		public const string SOCKET_COUNTER_BYTES_QUEUE = "Send Queue Bytes";

		/// <summary>目前連線數</summary>
		public const string SOCKET_COUNTER_CONNECTIONS_HELP = "目前連線數";
		/// <summary>總連線數</summary>
		public const string SOCKET_COUNTER_TOTAL_REQUEST_HELP = "請求連線總數";
		/// <summary>每秒請求的連線數</summary>
		public const string SOCKET_COUNTER_TOTAL_RECEIVED_HELP = "已接收的位元組數";
		/// <summary>發送總位元組數</summary>
		public const string SOCKET_COUNTER_TOTAL_SENDED_HELP = "已發送的位元組數";
		/// <summary>已使用的緩衝區</summary>
		public const string SOCKET_COUNTER_POOL_USED_HELP = "已使用的緩衝區數量";
		/// <summary>發送失敗的總次數</summary>
		public const string SOCKET_COUNTER_SAND_FAIL_HELP = "發送失敗的次數";
		/// <summary>每秒請求的連線數</summary>
		public const string SOCKET_COUNTER_RATE_OF_REQUEST_HELP = "每秒請求的連線數";
		/// <summary>每秒接收位元組數</summary>
		public const string SOCKET_COUNTER_RATE_OF_RECEIVED_HELP = "每秒接收的位元組數";
		/// <summary>每秒發送位元組數</summary>
		public const string SOCKET_COUNTER_RATE_OF_SENDED_HELP = "每秒發送的位元組數";
		/// <summary>每秒使用的緩衝區數量</summary>
		public const string SOCKET_COUNTER_RATE_OF_POOL_USE_HELP = "等待發送的位元組數";
		/// <summary>每秒發送失敗的次數</summary>
		public const string SOCKET_COUNTER_RATE_OF_SAND_FAIL_HELP = "每秒發送失敗的次數";
		/// <summary>等待發送的位元組數</summary>
		public const string SOCKET_COUNTER_BYTES_QUEUE_HELP = "等待發送的位元組數";
	}
	#endregion

	#region Public Enum : EventCallbackMode
	/// <summary>事件產生方式</summary>
	public enum EventCallbackMode
	{
		/// <summary>同步呼叫(DynamicInvoke)</summary>
		Invoke,
		/// <summary>非同步呼叫(BeginInvoke)</summary>
		BeginInvoke,
		/// <summary>使用執行緒集區序列產生執行緒呼叫(ThreadPool.QueueUserWorkItem(DynamicInvoke))</summary>
		Thread
	}
	#endregion

	#region Internal Struct : EventThreadVariables
	internal struct EventThreadVariables
	{
		public Delegate InvokeMethod;
		public object[] Args;
	}
	#endregion
}
