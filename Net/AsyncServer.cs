using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CJF.Utility;

namespace CJF.Net
{
	/// <summary>非同步 TCP 連線伺服器，使用xxxAsync</summary>
	[Serializable]
	public class AsyncServer : IDisposable
	{
		#region Variables
		LogManager _log = new LogManager(typeof(AsyncServer));
		Mutex m_Mutex = null;

		Socket m_ListenSocket;						// 伺服器 Socket 物件
		SocketAsyncEventArgs m_MainEventArg;
		SocketAsyncEventArgsPool m_ReadWritePool;	// SocketAsyncEventArgs 預備接線池
		Semaphore m_MaxNumberAcceptedClients;
		IPEndPoint m_LocalEndPort;					// 本地端通訊埠
		int m_MaxConnections;						// 同時可連接的最大連線數 
		int m_BufferSize;							// 緩衝暫存區大小
		bool m_ServerStarted = false;
		bool m_IsShutdown = false;
		bool m_IsDisposed = false;
		bool m_IsDisposing = false;
		uint m_AutoCloseTime = 0;
		SocketDebugType m_Debug = SocketDebugType.None;
		Timer m_CleanClientTimer = null;
		string m_CounterCategoryName = string.Empty;
		/// <summary>效能監視器集合</summary>
		Dictionary<ServerCounterType, PerformanceCounter> m_Counters = null;
		ConcurrentDictionary<EndPoint, int> m_WaitToClean = null;
		ConcurrentDictionary<string, AsyncClient> m_OnlineClients;	// 已連線的客戶端
		#endregion

		#region Public Events
		/// <summary>當伺服器啟動時觸發</summary>
		public event EventHandler<SocketServerEventArgs> OnStarted;
		/// <summary>當伺服器關閉時觸發</summary>
		public event EventHandler<SocketServerEventArgs> OnShutdown;
		/// <summary>當資料送出後觸發的事件</summary>
		public event EventHandler<SocketServerEventArgs> OnDataSended;
		/// <summary>當接收到資料時觸發的事件<br />勿忘處理黏包的狀況</summary>
		public event EventHandler<SocketServerEventArgs> OnDataReceived;
		/// <summary>當用戶端連線時觸發</summary>
		public event EventHandler<SocketServerEventArgs> OnClientConnected;
		/// <summary>當用戶端請求關閉連線時觸發</summary>
		public event EventHandler<SocketServerEventArgs> OnClientClosing;
		/// <summary>當用戶端以關閉連線時觸發</summary>
		public event EventHandler<SocketServerEventArgs> OnClientClosed;
		/// <summary>當連線發生錯誤時觸發</summary>
		public event EventHandler<SocketServerEventArgs> OnException;
		/// <summary>當資料無法發送至遠端時產生</summary>
		public event EventHandler<SocketServerEventArgs> OnSendedFail;
		#endregion

		#region Construct Method : AsyncServer(int numConnections, int receiveBufferSize)
		/// <summary>建立新的 AsyncServer 類別，並初始化相關屬性值</summary>
		/// <param name="numConnections">同時可連接的最大連線數</param>
		/// <param name="bufferSize">接收緩衝暫存區大小</param>
		public AsyncServer(int numConnections, int bufferSize)
		{
			m_Mutex = new Mutex();
			this.UseAsyncCallback = EventCallbackMode.BeginInvoke;
			m_Debug = SocketDebugType.None;
			m_IsDisposed = false;
			m_IsDisposing = false;
			m_IsShutdown = false;
			m_LocalEndPort = null;
			m_Counters = new Dictionary<ServerCounterType, PerformanceCounter>();
			m_MaxConnections = numConnections;
			m_BufferSize = bufferSize;
			m_ReadWritePool = new SocketAsyncEventArgsPool();
			// 預留兩條線程，用於過多的連線數檢查
			m_MaxNumberAcceptedClients = new Semaphore(numConnections + 2, numConnections + 2);
			m_OnlineClients = new ConcurrentDictionary<string, AsyncClient>();
			m_WaitToClean = new ConcurrentDictionary<EndPoint, int>();
			m_ServerStarted = false;
			for (int i = 0; i < m_MaxConnections; i++)
			{
				SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
				arg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
				arg.DisconnectReuseSocket = true;
				arg.SetBuffer(new Byte[m_BufferSize], 0, m_BufferSize);
				m_ReadWritePool.Push(arg);
			}
			SetCounterDictionary();
		}
		/// <summary>釋放 AsyncServer 所使用的資源。 </summary>
		~AsyncServer() { Dispose(false); }
		#endregion

		#region Private Method : void SetCounterDictionary()
		private void SetCounterDictionary()
		{
			m_Counters.Clear();
			m_Counters.Add(ServerCounterType.TotalRequest, null);
			m_Counters.Add(ServerCounterType.RateOfRequest, null);
			m_Counters.Add(ServerCounterType.Connections, null);
			m_Counters.Add(ServerCounterType.TotalReceivedBytes, null);
			m_Counters.Add(ServerCounterType.RateOfReceivedBytes, null);
			m_Counters.Add(ServerCounterType.TotalSendedBytes, null);
			m_Counters.Add(ServerCounterType.RateOfSendedBytes, null);
			m_Counters.Add(ServerCounterType.BytesOfSendQueue, null);
			m_Counters.Add(ServerCounterType.PoolUsed, null);
			m_Counters.Add(ServerCounterType.RateOfPoolUse, null);
			m_Counters.Add(ServerCounterType.SendFail, null);
			m_Counters.Add(ServerCounterType.RateOfSendFail, null);
		}
		#endregion

		#region Public Method : void LoadCounterDictionary(string categoryName, string instanceName)
		/// <summary>載入效能計數器</summary>
		/// <param name="categoryName"></param>
		/// <param name="instanceName"></param>
		/// <exception cref="System.InvalidOperationException">
		/// categoryName為空字串 ("")。 -或- 
		/// instanceName為空字串 ("")。 -或- 
		/// 指定的分類不存在。 -或- 
		/// 指定的分類會標記為多執行個體，而且需要以執行個體名稱建立效能計數器。 -或- 
		/// categoryName 和 counterName 已經當地語系化成不同的語言。</exception>
		/// <exception cref="System.ArgumentNullException">categoryName 或 counterName 為 null。</exception>
		/// <exception cref="System.ComponentModel.Win32Exception">在存取系統 API 時發生錯誤。</exception>
		/// <exception cref="System.PlatformNotSupportedException">平台是 Windows 98 或 Windows Millennium Edition (Me)，這兩個平台都不支援效能計數器。</exception>
		/// <exception cref="System.UnauthorizedAccessException">以不具有系統管理員權限執行的程式碼嘗試讀取效能計數器。</exception>
		public void LoadCounterDictionary(string categoryName, string instanceName)
		{
			if (categoryName == null || instanceName == null)
				throw new ArgumentNullException("categoryName 或 instanceName 不可為 null。");
			if (string.IsNullOrEmpty(categoryName) || string.IsNullOrEmpty(instanceName))
				throw new ArgumentException("categoryName 或 instanceName 不可為空字串。");
			if (!PerformanceCounterCategory.Exists(categoryName))
				throw new ArgumentException(string.Format("指定的分類不存在：\"{0}\"", categoryName));

			m_Counters[ServerCounterType.TotalRequest] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_TOTAL_REQUEST, instanceName, false);
			m_Counters[ServerCounterType.RateOfRequest] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_RATE_OF_REQUEST, instanceName, false);
			m_Counters[ServerCounterType.Connections] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_CONNECTIONS, instanceName, false);
			m_Counters[ServerCounterType.TotalReceivedBytes] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_TOTAL_RECEIVED, instanceName, false);
			m_Counters[ServerCounterType.RateOfReceivedBytes] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_RATE_OF_RECEIVED, instanceName, false);
			m_Counters[ServerCounterType.TotalSendedBytes] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_TOTAL_SENDED, instanceName, false);
			m_Counters[ServerCounterType.RateOfSendedBytes] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_RATE_OF_SENDED, instanceName, false);
			m_Counters[ServerCounterType.BytesOfSendQueue] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_BYTES_QUEUE, instanceName, false);
			m_Counters[ServerCounterType.PoolUsed] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_POOL_USED, instanceName, false);
			m_Counters[ServerCounterType.RateOfPoolUse] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_RATE_OF_POOL_USE, instanceName, false);
			m_Counters[ServerCounterType.SendFail] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_SAND_FAIL, instanceName, false);
			m_Counters[ServerCounterType.RateOfSendFail] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_RATE_OF_SAND_FAIL, instanceName, false);
		}
		#endregion

		#region Public Method : void Start(IPEndPoint localEndPoint)
		/// <summary>開始伺服器並等待連線請求, 如需引入效能監視器(PerformanceCounter)，請先執行LoadCounterDictionary函示</summary>
		/// <param name="localEndPoint">本地傾聽通訊埠</param>
		public void Start(IPEndPoint localEndPoint)
		{
			m_LocalEndPort = localEndPoint;
			m_ListenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			m_ListenSocket.ReceiveBufferSize = m_BufferSize;
			m_ListenSocket.SendBufferSize = m_BufferSize;
			m_ListenSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
			m_ListenSocket.NoDelay = true;
			m_ListenSocket.Bind(localEndPoint);
			m_ListenSocket.Listen(m_MaxConnections);
			m_ServerStarted = true;
			m_IsShutdown = false;
			m_CleanClientTimer = new Timer(CleanInvalidClients, null, 10000, 10000);

			#region 產生事件 - OnStarted
			if (this.OnStarted != null)
			{
				switch (this.UseAsyncCallback)
				{
					case EventCallbackMode.BeginInvoke:
						#region 非同步呼叫 - BeginInvoke
						{
							foreach (EventHandler<SocketServerEventArgs> del in this.OnStarted.GetInvocationList())
							{
								try { del.BeginInvoke(this, null, new AsyncCallback(AsyncServerEventCallback), del); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Invoke:
						#region 同步呼叫 - DynamicInvoke
						{
							foreach (Delegate del in this.OnStarted.GetInvocationList())
							{
								try { del.DynamicInvoke(this, null); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Thread:
						#region 建立執行緒 - Thread
						{
							foreach (Delegate del in this.OnStarted.GetInvocationList())
							{
								try
								{
									EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, null } };
									ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
								}
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
				}
			}
			#endregion

			this.StartAccept(null);
			m_Mutex.WaitOne();
		}
		#endregion

		#region Public Method : void Shutdown()
		/// <summary>關閉伺服器</summary>
		public void Shutdown()
		{
			if (m_IsDisposed || m_IsShutdown) return;
			m_IsShutdown = true;

			#region 關閉用戶端連線
			int counter = 0;
			AsyncClient[] acs = null;
			DateTime now;
			while (m_OnlineClients.Values.Count > 0 && counter < 3)
			{
				counter++;
				acs = new AsyncClient[m_OnlineClients.Count];
				m_OnlineClients.Values.CopyTo(acs, 0);
				foreach (AsyncClient ac in acs)
				{
					if (ac.Connected) ac.Close();
					now = DateTime.Now;
					while (ac.Connected && DateTime.Now.Subtract(now).TotalMilliseconds <= 500)
						Thread.Sleep(100);
				}
				now = DateTime.Now;
				while (m_OnlineClients.Count > 0 && DateTime.Now.Subtract(now).TotalMilliseconds <= 1000)
					Thread.Sleep(100);
				if (m_OnlineClients.Count == 0)
					break;
			}
			#endregion

			if (m_CleanClientTimer != null)
			{
				m_CleanClientTimer.Dispose();
				m_CleanClientTimer = null;
			}

			#region Shutdown Listener
			if (m_ListenSocket != null && m_ListenSocket.Connected)
			{
				try
				{
					if (m_Debug.HasFlag(SocketDebugType.Shutdown))
					{
						Debug.Print("[{0}]Socket : Before Shutdown In AsyncServer.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Before Shutdown");
					}
					m_ListenSocket.Shutdown(SocketShutdown.Both);
					if (m_Debug.HasFlag(SocketDebugType.Shutdown))
					{
						Debug.Print("[{0}]Socket : After Shutdown In AsyncServer.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "After Shutdown");
					}
				}
				catch (Exception ex)
				{
					Debug.Print("[LIB]EX:Shutdown:{0}", ex.Message);
					_log.WriteException(ex);
				}
				finally
				{
					if (m_Debug.HasFlag(SocketDebugType.Close))
					{
						Debug.Print("[{0}]Socket : Before Close In AsyncServer.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Before Close");
					}
					m_ListenSocket.Close();
					Thread.Sleep(500);
					if (m_Debug.HasFlag(SocketDebugType.Close))
					{
						Debug.Print("[{0}]Socket : After Close In AsyncServer.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "After Close");
					}
				}
			}
			#endregion

			#region Removing Counter
			ServerCounterType[] cts = new ServerCounterType[m_Counters.Keys.Count];
			m_Counters.Keys.CopyTo(cts, 0);
			foreach (ServerCounterType ct in cts)
			{
				if (m_Counters[ct] != null)
				{
					m_Counters[ct].Close();
					m_Counters[ct].Dispose();
				}
				m_Counters.Remove(ct);
			}
			m_Counters.Clear();
			m_WaitToClean.Clear();
			m_ServerStarted = false;
			#endregion

			try
			{
				m_Mutex.ReleaseMutex();
			}
			catch { }
			finally { m_Mutex.Close(); }

			#region 產生事件 - OnShutdown
			if (this.OnShutdown != null)
			{
				switch (this.UseAsyncCallback)
				{
					case EventCallbackMode.BeginInvoke:
						#region 非同步呼叫 - BeginInvoke
						{
							foreach (EventHandler<SocketServerEventArgs> del in this.OnShutdown.GetInvocationList())
							{
								try { del.BeginInvoke(this, null, new AsyncCallback(AsyncServerEventCallback), del); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Invoke:
						#region 同步呼叫 - DynamicInvoke
						{
							foreach (Delegate del in this.OnShutdown.GetInvocationList())
							{
								try { del.DynamicInvoke(this, null); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Thread:
						#region 建立執行緒 - Thread
						{
							foreach (Delegate del in this.OnShutdown.GetInvocationList())
							{
								try
								{
									EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, null } };
									ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
								}
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
				}
			}
			#endregion
		}
		#endregion

		#region Public Method : SendData(AsyncClient ac, byte[] data)
		/// <summary>傳送資料到用戶端</summary>
		/// <param name="ac"></param>
		/// <param name="data"></param>
		public void SendData(AsyncClient ac, byte[] data)
		{
			if (m_IsShutdown) return;
			if (ac.Connected)
			{
				try
				{
					Interlocked.Add(ref ac.m_WaittingSend, data.Length);
					if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.BytesOfSendQueue] != null)
						m_Counters[ServerCounterType.BytesOfSendQueue].IncrementBy(data.Length);
					SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
					arg.SetBuffer(data, 0, data.Length);
					arg.RemoteEndPoint = ac.RemoteEndPoint;
					arg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
					arg.UserToken = new AsyncUserToken(ac.Socket, data.Length);
					if (m_Debug.HasFlag(SocketDebugType.Send))
					{
						Debug.Print("[{0}]Socket : Before SendAsync In AsyncServer.SendData", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Before SendAsync");
					}
					try
					{
						if (!ac.Socket.SendAsync(arg))
							this.ProcessSend(arg);
					}
					catch
					{
						Interlocked.Add(ref ac.m_WaittingSend, -data.Length);
						if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.BytesOfSendQueue] != null)
							m_Counters[ServerCounterType.BytesOfSendQueue].IncrementBy(-data.Length);
						if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.SendFail] != null)
							m_Counters[ServerCounterType.SendFail].Increment();
						if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.RateOfSendFail] != null)
							m_Counters[ServerCounterType.RateOfSendFail].Increment();
					}
					if (m_Debug.HasFlag(SocketDebugType.Send))
					{
						Debug.Print("[{0}]Socket : After SendAsync In AsyncServer.SendData", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "After SendAsync");
					}
				}
				catch (Exception ex)
				{
					_log.WriteException(ex);
				}
			}
			else
				throw new SocketException((int)SocketError.NotConnected);
		}
		#endregion

		#region Public Method : AsyncClient FindClient(EndPoint ep)
		/// <summary>以遠端節點資訊尋找客戶端連線</summary>
		/// <param name="ep">遠端節點資訊</param>
		/// <returns>客戶端連線類別, 如查無連線將回傳 null</returns>
		public AsyncClient FindClient(EndPoint ep)
		{
			IPEndPoint cipp = (IPEndPoint)ep;
			string[] ipps = new string[m_OnlineClients.Count];
			m_OnlineClients.Keys.CopyTo(ipps, 0);
			foreach (string ipp in ipps)
			{
				if (ipp.Equals(cipp.ToString()))
					return m_OnlineClients[ipp];
			}
			return null;
		}
		#endregion

		#region Properties
		/// <summary>取得值，目前連線數</summary>
		public int Connections { get { return m_OnlineClients.Count; } }
		/// <summary>取得值，伺服器連線物件</summary>
		public Socket Socket { get { return m_ListenSocket; } }
		/// <summary>取得值，緩衝區最大值</summary>
		public int BufferSize { get { return m_BufferSize; } }
		/// <summary>取得值，最大連線數</summary>
		public int MaxConnections { get { return m_MaxConnections; } }
		/// <summary>取得值，本地端通訊埠</summary>
		public IPEndPoint LocalEndPort { get { return m_LocalEndPort; } }
		/// <summary>取得值，目前伺服器否啟動中</summary>
		public bool IsStarted { get { return m_ServerStarted; } }
		/// <summary>取得或設定是否為除錯模式</summary>
		public SocketDebugType DebugMode
		{
			get { return m_Debug; }
			set
			{
				if (m_Debug == value) return;
				m_Debug = value;
				string[] eps = new string[m_OnlineClients.Keys.Count];
				m_OnlineClients.Keys.CopyTo(eps, 0);
				foreach (string ep in eps)
				{
					if (m_OnlineClients[ep] != null)
						m_OnlineClients[ep].DebugMode = m_Debug;
				}
			}
		}
		/// <summary>取得值，已接受連線的次數，此數值由自訂之效能計數器中取出。</summary>
		public long AcceptCounter { get { return (long)m_Counters[ServerCounterType.TotalRequest].NextValue(); } }
		/// <summary>取得值，接線池剩餘數量。</summary>
		public int PoolSurplus { get { return m_ReadWritePool.Count; } }
		/// <summary>取得伺服器所有效能監視器</summary>
		public Dictionary<ServerCounterType, PerformanceCounter> PerformanceCounters { get { return m_Counters; } }
		/// <summary>取得與設定，是否使用非同步方式產生回呼事件</summary>
		public EventCallbackMode UseAsyncCallback { get; set; }
		/// <summary>取得所有遠端連線類別物件</summary>
		public AsyncClient[] Clients
		{
			get
			{
				AsyncClient[] acs = new AsyncClient[m_OnlineClients.Values.Count];
				m_OnlineClients.Values.CopyTo(acs, 0);
				return acs;
			}
		}
		/// <summary>設定或取得長時間未操作自動將用戶端斷線的設定時間，單位秒，0表不作動，預設值為0</summary>
		public uint AutoCloseTime
		{
			get { return m_AutoCloseTime; }
			set
			{
				m_AutoCloseTime = value;
				if (m_OnlineClients.Values.Count != 0)
				{
					AsyncClient[] acs = new AsyncClient[m_OnlineClients.Values.Count];
					m_OnlineClients.Values.CopyTo(acs, 0);
					foreach (AsyncClient ac in acs)
						ac.AutoClose = m_AutoCloseTime;
				}
			}
		}
		#endregion

		#region Private Method : void StartAccept(SocketAsyncEventArgs acceptEventArg)
		/// <summary>開始接收連線請求</summary>
		/// <param name="e">已接受的 SocketAsyncEventArgs 物件</param>
		private void StartAccept(SocketAsyncEventArgs e)
		{
			if (m_IsDisposed || m_IsShutdown) return;
			int index = Thread.CurrentThread.ManagedThreadId;
			string rep = "Unknow";
			try
			{
				if (e == null)
				{
					e = new SocketAsyncEventArgs();
					e.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
				}
				else
				{
					if (e.RemoteEndPoint != null)
						rep = e.RemoteEndPoint.ToString();
					e.AcceptSocket = null;
				}
				if (m_Debug.HasFlag(SocketDebugType.Connect))
				{
					Debug.Print("[{0}]Socket : Before AcceptAsync In AsyncServer.StartAccept", DateTime.Now.ToString("HH:mm:ss.fff"));
					_log.Write(LogManager.LogLevel.Debug, "Before AcceptAsync:{0}", rep);
				}
				m_MaxNumberAcceptedClients.WaitOne();
				m_MainEventArg = e;
				if (!m_ListenSocket.AcceptAsync(e))
					ProcessAccept(e);
				if (e.SocketError != SocketError.Success)
				{
					Debug.Print("[{0}]Socket : AsyncServer.StartAccept Fail:{1}", DateTime.Now.ToString("HH:mm:ss.fff"), e.SocketError);
					_log.Write(LogManager.LogLevel.Debug, "AsyncServer.StartAccept Fail:{0}:{1}", rep, e.SocketError);
				}
				if (m_Debug.HasFlag(SocketDebugType.Connect))
				{
					Debug.Print("[{0}]Socket : After AcceptAsync In AsyncServer.StartAccept", DateTime.Now.ToString("HH:mm:ss.fff"));
					_log.Write(LogManager.LogLevel.Debug, "After AcceptAsync:{0}", rep);
				}
			}
			catch (Exception ex)
			{
				_log.WriteException(ex);
			}
		}
		#endregion

		#region Private Method : void IO_Completed(object sender, SocketAsyncEventArgs e)
		/// <summary>當完成動作時，則呼叫此回呼函示。完成的動作由 SocketAsyncEventArg.LastOperation 屬性取得</summary>
		/// <param name="sender">AsyncServer 物件</param>
		/// <param name="e">完成動作的 SocketAsyncEventArg 物件</param>
		private void IO_Completed(object sender, SocketAsyncEventArgs e)
		{
			if (m_Debug.HasFlag(SocketDebugType.IO_Completed))
			{
				Debug.Print("[{0}]Socket : Before IO_Completed:{1}", DateTime.Now.ToString("HH:mm:ss.fff"), e.LastOperation);
				_log.Write(LogManager.LogLevel.Debug, "Before IO_Completed:{0}:{1}", Thread.CurrentThread.ManagedThreadId, e.LastOperation);
			}
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Receive:
					this.ProcessReceive(e);
					break;
				case SocketAsyncOperation.Send:
					this.ProcessSend(e);
					break;
				case SocketAsyncOperation.Accept:
					this.ProcessAccept(e);
					break;
				case SocketAsyncOperation.Disconnect:
					if (m_Debug.HasFlag(SocketDebugType.IO_Completed))
						_log.Write(LogManager.LogLevel.Debug, "Receive Client Disconnect Request!");
					this.CloseClientSocket(e);
					break;
				default:
					break;
			}
			if (m_Debug.HasFlag(SocketDebugType.IO_Completed))
			{
				Debug.Print("[{0}]Socket : After IO_Completed:{1}", DateTime.Now.ToString("HH:mm:ss.fff"), e.LastOperation);
				_log.Write(LogManager.LogLevel.Debug, "After IO_Completed:{0}:{1}", Thread.CurrentThread.ManagedThreadId, e.LastOperation);
			}
		}
		#endregion

		#region Private Method : void ProcessAccept(SocketAsyncEventArgs e)
		/// <summary>處理接受連線</summary>
		/// <param name="e">完成連線的 SocketAsyncEventArg 物件</param>
		private void ProcessAccept(SocketAsyncEventArgs e)
		{
			if (m_IsShutdown || m_IsDisposed || m_IsDisposing) return;
			Socket s = e.AcceptSocket;
			if (s == null)
			{
				this.StartAccept(e);
				return;
			}
			int index = Thread.CurrentThread.ManagedThreadId;
			string rep = "Unknow";
			if (e.RemoteEndPoint != null)
				rep = e.RemoteEndPoint.ToString();
			if (s.Connected)
			{
				try
				{
					SocketAsyncEventArgs readEventArgs = null;
					if (m_Debug.HasFlag(SocketDebugType.PopSocketArg))
					{
						Debug.Print("[{0}]Socket : Before Pop In AsyncServer.ProcessAccept", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Before Pop In ProcessAccept:{0}", rep);
					}
					readEventArgs = m_ReadWritePool.Pop();
					if (m_Debug.HasFlag(SocketDebugType.PopSocketArg))
					{
						Debug.Print("[{0}]Socket : After Pop In AsyncServer.ProcessAccept", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "After Pop In ProcessAccept:{0}:{1}:{2}", rep,
							(m_Counters[ServerCounterType.PoolUsed] == null) ? "Unknow" : m_Counters[ServerCounterType.PoolUsed].RawValue.ToString(), this.Connections);
					}
					if (readEventArgs != null)
					{
						AsyncClient ac = new AsyncClient(s);
						ac.ResetIdleTime();
						ac.AutoClose = m_AutoCloseTime;
						ac.DebugMode = this.DebugMode;
						ac.OnBeforeSended += new EventHandler<AsyncClientEventArgs>(ac_OnBeforeSended);
						ac.OnDataSended += new EventHandler<AsyncClientEventArgs>(ac_OnDataSended);
						ac.OnSendedFail += new EventHandler<AsyncClientEventArgs>(ac_OnSendedFail);
						ac.OnClosed += new EventHandler<AsyncClientEventArgs>(ac_OnClosed);
						ac.OnClosing += new EventHandler<AsyncClientEventArgs>(ac_OnClosing);
						m_OnlineClients.AddOrUpdate(s.RemoteEndPoint.ToString(), ac, (k, v) =>
							{
								v.Dispose();
								v = null;
								v = ac;
								return v;
							});
						readEventArgs.UserToken = new AsyncUserToken(s, m_BufferSize);
						if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.PoolUsed] != null)
							m_Counters[ServerCounterType.PoolUsed].Increment();
						if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.RateOfPoolUse] != null)
							m_Counters[ServerCounterType.RateOfPoolUse].Increment();
						if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.TotalRequest] != null)
							m_Counters[ServerCounterType.TotalRequest].Increment();
						if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.RateOfRequest] != null)
							m_Counters[ServerCounterType.RateOfRequest].Increment();
						if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.Connections] != null)
							m_Counters[ServerCounterType.Connections].Increment();

						#region 產生事件 - OnClientConnected
						if (this.OnClientConnected != null)
						{
							SocketServerEventArgs asea = new SocketServerEventArgs(ac);
							switch (this.UseAsyncCallback)
							{
								case EventCallbackMode.BeginInvoke:
									#region 非同步呼叫 - BeginInvoke
									{
										foreach (EventHandler<SocketServerEventArgs> del in this.OnClientConnected.GetInvocationList())
										{
											try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
											catch (Exception ex) { _log.WriteException(ex); }
										}
										break;
									}
									#endregion
								case EventCallbackMode.Invoke:
									#region 同步呼叫 - DynamicInvoke
									{
										foreach (Delegate del in this.OnClientConnected.GetInvocationList())
										{
											try { del.DynamicInvoke(this, asea); }
											catch (Exception ex) { _log.WriteException(ex); }
										}
										break;
									}
									#endregion
								case EventCallbackMode.Thread:
									#region 建立執行緒 - Thread
									{
										foreach (Delegate del in this.OnClientConnected.GetInvocationList())
										{
											try
											{
												EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
												ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
											}
											catch (Exception ex) { _log.WriteException(ex); }
										}
										break;
									}
									#endregion
							}
						}
						#endregion

						if (m_Debug.HasFlag(SocketDebugType.Receive))
						{
							Debug.Print("[{0}]Socket : Before ReceiveAsync In AsyncServer.ProcessAccept", DateTime.Now.ToString("HH:mm:ss.fff"));
							_log.Write(LogManager.LogLevel.Debug, "Before ReceiveAsync In ProcessAccept:{0}", rep);
						}
						try
						{
							if (!s.ReceiveAsync(readEventArgs))
								ProcessReceive(readEventArgs);
						}
						catch (ObjectDisposedException) { }
						if (m_Debug.HasFlag(SocketDebugType.Receive))
						{
							Debug.Print("[{0}]Socket : After ReceiveAsync In AsyncServer.ProcessAccept", DateTime.Now.ToString("HH:mm:ss.fff"));
							_log.Write(LogManager.LogLevel.Debug, "After ReceiveAsync In ProcessAccept:{0}", rep);
						}
					}
					else
					{
						_log.Write(LogManager.LogLevel.Warn, "連線數不足，請擴充連線數!");
						throw new SocketException(10024);
					}
				}
				catch (SocketException ex)
				{
					AsyncUserToken token = (AsyncUserToken)e.UserToken;
					if (m_Debug.HasFlag(SocketDebugType.Connect))
					{
						Debug.Print("[{0}]Socket : SocketException:{1} In AsyncServer.ProcessAccept", DateTime.Now.ToString("HH:mm:ss.fff"), ex.SocketErrorCode);
						Debug.Print(ex.Message);
						_log.Write(LogManager.LogLevel.Debug, "SocketException In ProcessAccept:{0}", rep);
					}
					if (s != null && token != null && token.Client != null)
						_log.Write(LogManager.LogLevel.Debug, "無法與 {0} 建立連線", token.Client.RemoteEndPoint);
					_log.WriteException(ex);

					AsyncClient ac = new AsyncClient(s);

					#region 產生事件 - OnException
					if (this.OnException != null)
					{
						SocketServerEventArgs asea = new SocketServerEventArgs(ac, null, ex);
						switch (this.UseAsyncCallback)
						{
							case EventCallbackMode.BeginInvoke:
								#region 非同步呼叫 - BeginInvoke
								{
									foreach (EventHandler<SocketServerEventArgs> del in this.OnException.GetInvocationList())
									{
										try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
										catch (Exception exx) { _log.WriteException(exx); }
									}
									break;
								}
								#endregion
							case EventCallbackMode.Invoke:
								#region 同步呼叫 - DynamicInvoke
								{
									foreach (Delegate del in this.OnException.GetInvocationList())
									{
										try { del.DynamicInvoke(this, asea); }
										catch (Exception exx) { _log.WriteException(exx); }
									}
									break;
								}
								#endregion
							case EventCallbackMode.Thread:
								#region 建立執行緒 - Thread
								{
									foreach (Delegate del in this.OnException.GetInvocationList())
									{
										try
										{
											EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
											ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
										}
										catch (Exception exx) { _log.WriteException(exx); }
									}
									break;
								}
								#endregion
						}
					}
					#endregion

					#region 三秒後強制斷線
					System.Threading.Tasks.Task.Factory.StartNew(o =>
					{
						try
						{
							AsyncClient acc = (AsyncClient)o;
							Thread.Sleep(3000);
							if (acc != null)
							{
								acc.Close();
								acc.Dispose();
								acc = null;
							}
						}
						catch { }
					}, ac);
					#endregion
				}
				catch (Exception ex)
				{
					if (m_Debug.HasFlag(SocketDebugType.Connect))
					{
						Debug.Print("[{0}]Socket : Exception In AsyncServer.ProcessAccept", DateTime.Now.ToString("HH:mm:ss.fff"));
						Debug.Print(ex.Message);
						_log.Write(LogManager.LogLevel.Debug, "Exception In ProcessAccept:{0}", rep);
					}
					_log.WriteException(ex);
				}
				finally
				{
					// 等待下一個連線請求
					this.StartAccept(e);
				}
			}
		}
		#endregion

		#region Private Method : void ProcessReceive(SocketAsyncEventArgs e)
		/// <summary>
		/// 當完成接收資料時，將呼叫此函示
		/// 如果客戶端關閉連接，將會一併關閉此連線(Socket)
		/// 如果收到數據接著將數據返回到客戶端
		/// </summary>
		/// <param name="e">已完成接收的 SocketAsyncEventArg 物件</param>
		private void ProcessReceive(SocketAsyncEventArgs e)
		{
			int index = Thread.CurrentThread.ManagedThreadId;
			AsyncUserToken token = e.UserToken as AsyncUserToken;
			if (token == null || token.IsDisposed || m_IsShutdown || m_IsDisposed)
				return;
			IntPtr origHandle = IntPtr.Zero;
			Socket s = token.Client;
			AsyncClient ac = null;
			EndPoint remote = null;
			IPEndPoint remote4Callback = null;
			string rep = "Unknow";
			if (e.RemoteEndPoint != null)
				rep = e.RemoteEndPoint.ToString();
			try
			{
				origHandle = new IntPtr(s.Handle.ToInt32());
				remote = s.RemoteEndPoint;
				IPEndPoint ipp = (IPEndPoint)remote;
				remote4Callback = new IPEndPoint(ipp.Address, ipp.Port);
			}
			catch (ObjectDisposedException) { }
			try
			{
				if (e.BytesTransferred > 0)
				{
					if (m_Debug.HasFlag(SocketDebugType.Receive))
					{
						Debug.Print("[{0}]Socket : Exec ReceiveAsync In AsyncServer.ProcessReceive", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Exec ReceiveAsync In AsyncServer.ProcessReceive:{0}", rep);
					}
					if (e.SocketError == SocketError.Success)
					{
						if (m_Debug.HasFlag(SocketDebugType.Receive))
							_log.Write(LogManager.LogLevel.Debug, "Exec ReceiveAsync:{0}", s.RemoteEndPoint);
						ac = m_OnlineClients[s.RemoteEndPoint.ToString()];
						int count = e.BytesTransferred;
						if (m_Debug.HasFlag(SocketDebugType.Receive))
							_log.Write(LogManager.LogLevel.Debug, "Received Data:{0}:{1}", s.RemoteEndPoint, count);
						List<byte> rec = new List<byte>();
						if ((token.CurrentIndex + count) > token.BufferSize)
						{
							rec.AddRange(token.ReceivedData);
							token.ClearBuffer();
						}
						token.SetData(e);
						if (s.Available == 0)
						{
							rec.AddRange(token.ReceivedData);
							token.ClearBuffer();
						}

						if (ac == null)
						{
							Debug.Print("Unknow Socket Connect!!");
							_log.Write(LogManager.LogLevel.Debug, "Unknow Socket:{0}", rep);
							_log.Write(LogManager.LogLevel.Debug, "Data:{0}", ConvUtils.Byte2HexString(rec.ToArray()));
							this.CloseClientSocket(e);
							return;
						}

						if (ac != null)
							Interlocked.Add(ref ac.m_ReceiveByteCount, count);
						if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.TotalReceivedBytes] != null)
							m_Counters[ServerCounterType.TotalReceivedBytes].IncrementBy(count);
						if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.RateOfReceivedBytes] != null)
							m_Counters[ServerCounterType.RateOfReceivedBytes].IncrementBy(count);

						#region 產生事件 - OnDataReceived
						if (rec.Count != 0 && this.OnDataReceived != null)
						{
							SocketServerEventArgs asea = new SocketServerEventArgs(ac, rec.ToArray());
							switch (this.UseAsyncCallback)
							{
								case EventCallbackMode.BeginInvoke:
									#region 非同步呼叫 -BeginInvoke
									{
										foreach (EventHandler<SocketServerEventArgs> del in this.OnDataReceived.GetInvocationList())
										{
											try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
											catch (Exception ex) { _log.WriteException(ex); }
										}
										break;
									}
									#endregion
								case EventCallbackMode.Invoke:
									#region 同步呼叫 -DynamicInvoke
									{
										foreach (Delegate del in this.OnDataReceived.GetInvocationList())
										{
											try { del.DynamicInvoke(this, asea); }
											catch (Exception ex) { _log.WriteException(ex); }
										}
										break;
									}
									#endregion
								case EventCallbackMode.Thread:
									#region 建立執行緒 - Thread
									{
										foreach (Delegate del in this.OnDataReceived.GetInvocationList())
										{
											try
											{
												EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
												ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
											}
											catch (Exception ex) { _log.WriteException(ex); }
										}
										break;
									}
									#endregion
							}
						}
						#endregion

						if (m_Debug.HasFlag(SocketDebugType.Receive))
						{
							Debug.Print("[{0}]Socket : Before ReceiveAsync In AsyncServer.ProcessReceive", DateTime.Now.ToString("HH:mm:ss.fff"));
							_log.Write(LogManager.LogLevel.Debug, "Before ReceiveAsync:{0}", rep);
						}
						if (!ac.Connected)
						{
							RecoverSocket(origHandle, remote4Callback, e);
							return;
						}
						try
						{
							if (s != null && !s.ReceiveAsync(e))	// 讀取下一個由客戶端傳送的封包
								this.ProcessReceive(e);
						}
						catch (ObjectDisposedException) { }
						catch (Exception ex)
						{
							if (!m_IsShutdown && !m_IsDisposed)
							{
								_log.Write(LogManager.LogLevel.Debug, "AsyncServer.ProcessReceive:{0}", rep);
								_log.WriteException(ex);
							}
						}
						if (m_Debug.HasFlag(SocketDebugType.Receive))
						{
							Debug.Print("[{0}]Socket : After ReceiveAsync In AsyncServer.ProcessReceive", DateTime.Now.ToString("HH:mm:ss.fff"));
							_log.Write(LogManager.LogLevel.Debug, "After ReceiveAsync:{0}", rep);
						}
					}
					else
						this.ProcessError(e);
				}
				else
				{
					RecoverSocket(origHandle, remote4Callback, e);
				}
			}
			catch (KeyNotFoundException)
			{
				_log.Write(LogManager.LogLevel.Debug, "RemoteEndPoint Not Found:{0}", rep);
				this.CloseClientSocket(e);
			}
			catch (ObjectDisposedException ex) { _log.Write(LogManager.LogLevel.Debug, "Object({0}) Disposed", ex.ObjectName); }
			catch (Exception ex)
			{
				_log.Write(LogManager.LogLevel.Debug, "ProcessReceive Error!");
				_log.WriteException(ex);
				this.CloseClientSocket(e);
			}
		}
		#endregion

		#region Private Method : void ProcessSend(SocketAsyncEventArgs e)
		/// <summary>當完成傳送資料時，將呼叫此函示</summary>
		/// <param name="e">SocketAsyncEventArg associated with the completed send operation.</param>
		private void ProcessSend(SocketAsyncEventArgs e)
		{
			int index = Thread.CurrentThread.ManagedThreadId;
			if (e.BytesTransferred > 0)
			{
				if (m_Debug.HasFlag(SocketDebugType.Send))
				{
					Debug.Print("[{0}]Socket : Exec SendAsync In AsyncServer.ProcessSend", DateTime.Now.ToString("HH:mm:ss.fff"));
					_log.Write(LogManager.LogLevel.Debug, "Exec SendAsync In AsyncServer.ProcessSend");
				}
				if (e.SocketError == SocketError.Success)
				{
					Socket s = null;
					AsyncClient ac = null;
					if (e.UserToken.GetType().Equals(typeof(Socket)))
					{
						s = e.UserToken as Socket;
						ac = m_OnlineClients[s.RemoteEndPoint.ToString()];
					}
					else if (e.UserToken.GetType().Equals(typeof(AsyncClient)))
					{
						ac = (AsyncClient)e.UserToken;
						s = ac.Socket;
					}
					if (ac == null)
					{
						Debug.Print("Unknow Socket Connect!!");
						if (e.RemoteEndPoint != null)
							_log.Write(LogManager.LogLevel.Debug, "Unknow Socket:{0}", e.RemoteEndPoint);
						this.CloseClientSocket(e);
						return;
					}
					int count = e.BytesTransferred;
					if (m_Debug.HasFlag(SocketDebugType.Send))
						_log.Write(LogManager.LogLevel.Debug, "Exec SendAsync:{0}", count);
					Interlocked.Add(ref ac.m_SendByteCount, count);
					Interlocked.Add(ref ac.m_WaittingSend, -count);
					if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.TotalSendedBytes] != null)
						m_Counters[ServerCounterType.TotalSendedBytes].IncrementBy(count);
					if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.RateOfSendedBytes] != null)
						m_Counters[ServerCounterType.RateOfSendedBytes].IncrementBy(count);
					if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.BytesOfSendQueue] != null)
						m_Counters[ServerCounterType.BytesOfSendQueue].IncrementBy(-count);

					byte[] buffer = new byte[count];
					Array.Copy(e.Buffer, buffer, count);

					#region 產生事件 - OnDataSended
					if (this.OnDataSended != null)
					{
						SocketServerEventArgs asea = new SocketServerEventArgs(ac, buffer);
						switch (this.UseAsyncCallback)
						{
							case EventCallbackMode.BeginInvoke:
								#region 非同步呼叫 - BeginInvoke
								{
									foreach (EventHandler<SocketServerEventArgs> del in this.OnDataSended.GetInvocationList())
									{
										try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
										catch (Exception ex) { _log.WriteException(ex); }
									}
									break;
								}
								#endregion
							case EventCallbackMode.Invoke:
								#region 同步呼叫 - DynamicInvoke
								{
									foreach (Delegate del in this.OnDataSended.GetInvocationList())
									{
										try { del.DynamicInvoke(this, asea); }
										catch (Exception ex) { _log.WriteException(ex); }
									}
									break;
								}
								#endregion
							case EventCallbackMode.Thread:
								#region 建立執行緒 - Thread
								{
									foreach (Delegate del in this.OnDataSended.GetInvocationList())
									{
										try
										{
											EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
											ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
										}
										catch (Exception ex) { _log.WriteException(ex); }
									}
									break;
								}
								#endregion
						}
					}
					#endregion
				}
				else
					this.ProcessError(e);
			}
			else
			{
				_log.Write(LogManager.LogLevel.Debug, "Send Zero Byte Data To Remote");
				this.CloseClientSocket(e);
			}
		}
		#endregion

		#region Private Method : void ProcessError(SocketAsyncEventArgs e)
		/// <summary>當發生錯誤時，將呼叫此函示，並關閉客戶端</summary>
		/// <param name="e">發生錯誤的 SocketAsyncEventArgs 物件</param>
		private void ProcessError(SocketAsyncEventArgs e)
		{
			AsyncUserToken token = (AsyncUserToken)e.UserToken;
			Socket s = token.Client;
			AsyncClient ac = null;
			int index = Thread.CurrentThread.ManagedThreadId;
			if (s != null)
			{
				try
				{
					ac = m_OnlineClients[s.RemoteEndPoint.ToString()];

					#region 產生事件 - OnException
					if (this.OnException != null)
					{
						IPEndPoint localEp = (IPEndPoint)s.LocalEndPoint;
						SocketException se = new SocketException((Int32)e.SocketError);
						Exception ex = new Exception(string.Format("客戶端連線({1})發生錯誤:{0},狀態:{2}", (int)e.SocketError, localEp, e.LastOperation), se);
						SocketServerEventArgs asea = new SocketServerEventArgs(ac, null, ex);
						switch (this.UseAsyncCallback)
						{
							case EventCallbackMode.BeginInvoke:
								#region 非同步呼叫 - BeginInvoke
								{
									foreach (EventHandler<SocketServerEventArgs> del in this.OnException.GetInvocationList())
									{
										try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
										catch (Exception exx) { _log.WriteException(exx); }
									}
									break;
								}
								#endregion
							case EventCallbackMode.Invoke:
								#region 同步呼叫 - DynamicInvoke
								{
									foreach (Delegate del in this.OnException.GetInvocationList())
									{
										try { del.DynamicInvoke(this, asea); }
										catch (Exception exx) { _log.WriteException(exx); }
									}
									break;
								}
								#endregion
							case EventCallbackMode.Thread:
								#region 建立執行緒 - Thread
								{
									foreach (Delegate del in this.OnException.GetInvocationList())
									{
										try
										{
											EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
											ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
										}
										catch (Exception exx) { _log.WriteException(exx); }
									}
									break;
								}
								#endregion
						}
					}
					#endregion

					if (m_Debug.HasFlag(SocketDebugType.Shutdown))
					{
						Debug.Print("[{0}]Socket : Before CloseClientSocket In AsyncServer.ProcessError", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Before CloseClientSocket In ProcessError");
					}
					this.CloseClientSocket(e);
					if (m_Debug.HasFlag(SocketDebugType.Shutdown))
					{
						Debug.Print("[{0}]Socket : After CloseClientSocket In AsyncServer.ProcessError", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "After CloseClientSocket In ProcessError");
					}
				}
				catch (ObjectDisposedException) { }
				catch (Exception) { }	// 如果客戶端已關閉則不處理
			}
		}
		#endregion

		#region Private Method : void CloseClientSocket(SocketAsyncEventArgs e)
		/// <summary>關閉客戶端連線</summary>
		/// <param name="e">需處理的 SocketAsyncEventArg 物件</param>
		private void CloseClientSocket(SocketAsyncEventArgs e)
		{
			AsyncUserToken token = e.UserToken as AsyncUserToken;
			int index = Thread.CurrentThread.ManagedThreadId;
			if (token != null && token.Client != null && !token.IsDisposed)
			{
				try
				{
					IntPtr origHandle = IntPtr.Zero;
					Socket s = token.Client;
					origHandle = new IntPtr(s.Handle.ToInt32());
					if (m_IsShutdown || m_IsDisposed) return;
					AsyncClient ac = null;
					EndPoint remote = null;
					IPEndPoint remote4Callback = null;
					bool exists = false;
					try
					{
						IPEndPoint ipp = (IPEndPoint)s.RemoteEndPoint;
						remote4Callback = new IPEndPoint(ipp.Address, ipp.Port);
						remote = s.RemoteEndPoint;
						exists = m_OnlineClients.TryRemove(remote.ToString(), out ac);
					}
					catch (ObjectDisposedException) { }

					try
					{
						#region Try Shutdown
						if (m_Debug.HasFlag(SocketDebugType.Close))
						{
							Debug.Print("[{0}]Socket : Before Shutdown In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
							_log.Write(LogManager.LogLevel.Debug, "Before Shutdown(CloseClient)");
						}
						s.Shutdown(SocketShutdown.Both);
						if (m_Debug.HasFlag(SocketDebugType.Close))
						{
							Debug.Print("[{0}]Socket : After Shutdown In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
							_log.Write(LogManager.LogLevel.Debug, "After Shutdown(CloseClient)");
						}
						#endregion
					}
					catch { }	// 如果客戶端已關閉，則不需要將錯誤丟出
					finally
					{
						#region Close Socket
						try
						{
							if (m_Debug.HasFlag(SocketDebugType.Close))
							{
								Debug.Print("[{0}]Socket : Before Close In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
								_log.Write(LogManager.LogLevel.Debug, "Before Close(CloseClient)");
							}
							s.Close();
							if (m_Debug.HasFlag(SocketDebugType.Close))
							{
								Debug.Print("[{0}]Socket : After Close In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
								_log.Write(LogManager.LogLevel.Debug, "After Close(CloseClient)");
							}
						}
						catch { }
						finally { s = null; }
						#endregion
					}
					if (ac != null && remote != null)
					{
						if (m_WaitToClean.ContainsKey(remote))
						{
							int v;
							m_WaitToClean.TryRemove(remote, out v);
						}
					}
					if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.Connections] != null)
						m_Counters[ServerCounterType.Connections].Decrement();
					try
					{
						m_MaxNumberAcceptedClients.Release();
					}
					catch (SemaphoreFullException) { }
					catch (NullReferenceException) { }
					try
					{
						if (ac != null)
							ac.Dispose();
						token.Dispose();
					}
					catch (Exception ex) { _log.WriteException(ex); }
					e.UserToken = null;

					#region 回收 SocketAsyncEventArgs 物件
					if (!m_IsShutdown)
					{
						if ((m_Debug & SocketDebugType.PushSocketArg) != 0)
						{
							Debug.Print("[{0}]Socket : Before Push In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
							_log.Write(LogManager.LogLevel.Debug, "Before Push:{0}", m_ReadWritePool.Count);
						}
						if (!m_IsDisposed && m_Counters[ServerCounterType.PoolUsed] != null)
							m_Counters[ServerCounterType.PoolUsed].Decrement();
						m_ReadWritePool.Push(e);
						if ((m_Debug & SocketDebugType.PushSocketArg) != 0)
						{
							Debug.Print("[{0}]Socket : After Push In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
							_log.Write(LogManager.LogLevel.Debug, "After Push:{0}", m_ReadWritePool.Count);
						}
					}
					#endregion
				}
				catch (Exception ex)
				{
					_log.WriteException(ex);
				}
			}
		}
		#endregion

		#region Private Method : void RecoverSocket(IntPtr origHandle, EndPoint remote, SocketAsyncEventArgs e)
		/// <summary>回收客戶端連線</summary>
		/// <param name="origHandle">原始控制代碼</param>
		/// <param name="remote">原始遠端資訊</param>
		/// <param name="e">需處理的 SocketAsyncEventArg 物件</param>
		private void RecoverSocket(IntPtr origHandle, EndPoint remote, SocketAsyncEventArgs e)
		{
			int index = Thread.CurrentThread.ManagedThreadId;
			if (m_IsShutdown || m_IsDisposed) return;
			try
			{
				IPEndPoint remote4Callback = null;
				if (remote != null)
				{
					IPEndPoint ipp = (IPEndPoint)remote;
					remote4Callback = new IPEndPoint(ipp.Address, ipp.Port);
				}
				AsyncClient ac = null;
				bool exists = false;
				if (remote != null)
				{
					#region 產生事件 - OnClientClosing
					if (false && exists && this.OnClientClosing != null)
					{
						SocketServerEventArgs asea = new SocketServerEventArgs(ac);
						switch (this.UseAsyncCallback)
						{
							case EventCallbackMode.BeginInvoke:
								#region 非同步呼叫 - BeginInvoke
								{
									foreach (EventHandler<SocketServerEventArgs> del in this.OnClientClosing.GetInvocationList())
									{
										try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
										catch (Exception ex) { _log.WriteException(ex); }
									}
									break;
								}
								#endregion
							case EventCallbackMode.Invoke:
								#region 同步呼叫 - DynamicInvoke
								{
									foreach (Delegate del in this.OnClientClosing.GetInvocationList())
									{
										try { del.DynamicInvoke(this, asea); }
										catch (Exception ex) { _log.WriteException(ex); }
									}
									break;
								}
								#endregion
							case EventCallbackMode.Thread:
								#region 建立執行緒 - Thread
								{
									foreach (Delegate del in this.OnClientClosing.GetInvocationList())
									{
										try
										{
											EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
											ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
										}
										catch (Exception ex) { _log.WriteException(ex); }
									}
									break;
								}
								#endregion
						}
					}
					#endregion

					#region 如連線還存在，則關閉連線
					if (ac != null && ac.Socket != null)
					{
						Socket s = ac.Socket;
						try
						{
							#region Try Shutdown
							if (m_Debug.HasFlag(SocketDebugType.Close))
							{
								Debug.Print("[{0}]Socket : Before Shutdown In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
								_log.Write(LogManager.LogLevel.Debug, "Before Shutdown(CloseClient)");
							}
							s.Shutdown(SocketShutdown.Both);
							if (m_Debug.HasFlag(SocketDebugType.Close))
							{
								Debug.Print("[{0}]Socket : After Shutdown In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
								_log.Write(LogManager.LogLevel.Debug, "After Shutdown(CloseClient)");
							}
							#endregion
						}
						catch { }	// 如果客戶端已關閉，則不需要將錯誤丟出
						finally
						{
							#region Close Socket
							try
							{
								if (m_Debug.HasFlag(SocketDebugType.Close))
								{
									Debug.Print("[{0}]Socket : Before Close In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
									_log.Write(LogManager.LogLevel.Debug, "Before Close(CloseClient)");
								}
								s.Close();
								if (m_Debug.HasFlag(SocketDebugType.Close))
								{
									Debug.Print("[{0}]Socket : After Close In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
									_log.Write(LogManager.LogLevel.Debug, "After Close(CloseClient)");
								}
							}
							catch { }
							#endregion
						}
					}
					#endregion

					if (m_WaitToClean.ContainsKey(remote))
					{
						int v;
						m_WaitToClean.TryRemove(remote, out v);
					}
				}
				if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.Connections] != null)
					m_Counters[ServerCounterType.Connections].Decrement();
				try
				{
					m_MaxNumberAcceptedClients.Release();
				}
				catch (SemaphoreFullException) { }
				catch (NullReferenceException) { }

				#region 回收 SocketAsyncEventArgs 物件
				if (!m_IsShutdown)
				{
					if ((m_Debug & SocketDebugType.PushSocketArg) != 0)
					{
						Debug.Print("[{0}]Socket : Before Push In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Before Push:{0}", m_ReadWritePool.Count);
					}
					if (!m_IsDisposed && m_Counters[ServerCounterType.PoolUsed] != null)
						m_Counters[ServerCounterType.PoolUsed].Decrement();
					m_ReadWritePool.Push(e);
					if ((m_Debug & SocketDebugType.PushSocketArg) != 0)
					{
						Debug.Print("[{0}]Socket : After Push In AsyncServer.CloseClientSocket", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "After Push:{0}", m_ReadWritePool.Count);
					}
				}
				#endregion

				#region 產生事件 - OnClientClosed
				if (false && this.OnClientClosed != null)
				{
					SocketServerEventArgs asea = new SocketServerEventArgs(origHandle, remote4Callback);
					switch (this.UseAsyncCallback)
					{
						case EventCallbackMode.BeginInvoke:
							#region 非同步呼叫 - BeginInvoke
							{
								foreach (EventHandler<SocketServerEventArgs> del in this.OnClientClosed.GetInvocationList())
								{
									try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
									catch (Exception ex) { _log.WriteException(ex); }
								}
								break;
							}
							#endregion
						case EventCallbackMode.Invoke:
							#region 同步呼叫 - DynamicInvoke
							{
								foreach (Delegate del in this.OnClientClosed.GetInvocationList())
								{
									try { del.DynamicInvoke(this, asea); }
									catch (Exception ex) { _log.WriteException(ex); }
								}
								break;
							}
							#endregion
						case EventCallbackMode.Thread:
							#region 建立執行緒 - Thread
							{
								foreach (Delegate del in this.OnClientClosed.GetInvocationList())
								{
									try
									{
										EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
										ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
									}
									catch (Exception ex) { _log.WriteException(ex); }
								}
								break;
							}
							#endregion
					}
				}
				#endregion
			}
			catch (Exception ex)
			{
				_log.WriteException(ex);
			}
		}
		#endregion

		#region Delegate Callback Methods
		private void AsyncServerEventCallback(IAsyncResult result)
		{
			try
			{
				EventHandler<SocketServerEventArgs> del = result.AsyncState as EventHandler<SocketServerEventArgs>;
				del.EndInvoke(result);
			}
			catch (ObjectDisposedException) { }
			catch (NullReferenceException) { }
			catch (Exception ex) { _log.WriteException(ex); }
		}
		#endregion

		#region IDisposable
		/// <summary>清除並釋放 AsyncServer 所使用的資源。</summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void Dispose(bool disposing)
		{
			if (m_IsDisposed) return;
			if (disposing)
			{
				m_IsDisposing = true;
				try
				{
					SocketAsyncEventArgs arg = null;
					if (m_ReadWritePool != null)
					{
						if (m_Debug.HasFlag(SocketDebugType.PopSocketArg))
						{
							Debug.Print("[{0}]Socket : Before Pop In Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
							_log.Write(LogManager.LogLevel.Debug, "Before Push:{0}", m_ReadWritePool.Count);
						}
						arg = m_ReadWritePool.Pop();
						if (m_Debug.HasFlag(SocketDebugType.PopSocketArg))
						{
							Debug.Print("[{0}]Socket : After Pop In Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
							_log.Write(LogManager.LogLevel.Debug, "Before Push:{0}", m_ReadWritePool.Count);
						}
						arg.Dispose();
						arg = null;
						m_ReadWritePool.Clear();
					}
					m_Counters.Clear();
					m_Counters = null;
					m_ReadWritePool = null;
					if (m_OnlineClients != null)
						m_OnlineClients.Clear();
					m_OnlineClients = null;
					if (m_WaitToClean != null)
						m_WaitToClean.Clear();
					m_WaitToClean = null;
					m_MaxNumberAcceptedClients.Close();
					m_MaxNumberAcceptedClients = null;
					m_Mutex.Dispose();
					m_Mutex = null;
					if (m_ListenSocket != null)
					{
						try
						{
							if (m_Debug.HasFlag(SocketDebugType.Shutdown))
							{
								Debug.Print("[{0}]Socket : Before Shutdown In AsyncServer.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
								_log.Write(LogManager.LogLevel.Debug, "Before Shutdown In Dispose");
							}
							m_ListenSocket.Shutdown(SocketShutdown.Both);
							if (m_Debug.HasFlag(SocketDebugType.Shutdown))
							{
								Debug.Print("[{0}]Socket : After Shutdown In AsyncServer.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
								_log.Write(LogManager.LogLevel.Debug, "After Shutdown In Dispose");
							}
						}
						catch { }
						finally
						{
							try
							{
								if (m_Debug.HasFlag(SocketDebugType.Close))
								{
									Debug.Print("[{0}]Socket : Before Close In AsyncServer.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
									_log.Write(LogManager.LogLevel.Debug, "Before Close In Dispose");
								}
								m_ListenSocket.Close();
								if (m_Debug.HasFlag(SocketDebugType.Close))
								{
									Debug.Print("[{0}]Socket : After Close In AsyncServer.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
									_log.Write(LogManager.LogLevel.Debug, "After Close In Dispose");
								}
							}
							catch (Exception ex) { _log.WriteException(ex); }
							finally { m_ListenSocket = null; }
						}
					}
					m_LocalEndPort = null;
				}
				catch { }
			}
			m_IsDisposing = false;
			m_IsDisposed = true;
		}
		#endregion

		#region Private Method : void CleanInvalidClients(object o)
		private void CleanInvalidClients(object o)
		{
			if (m_IsDisposed || m_IsShutdown) return;
			try
			{
				if (m_MainEventArg == null || m_ListenSocket == null)
					this.StartAccept(null);
				SocketInfo[] sis = TcpManager.GetLocalCloseWaitTable(this.LocalEndPort);
				if (sis != null)
				{
					int v = 0;
					foreach (SocketInfo si in sis)
					{
						if (m_WaitToClean.ContainsKey(si.RemoteEndPoint))
						{
							m_WaitToClean[si.RemoteEndPoint]++;
							if (m_WaitToClean[si.RemoteEndPoint] >= 3)
							{
								TcpManager.Kill(si);
								m_WaitToClean.TryRemove(si.RemoteEndPoint, out v);
							}
						}
						else
							m_WaitToClean.TryAdd(si.RemoteEndPoint, 0);
					}
				}
				if (m_OnlineClients != null)
				{
					List<AsyncClient> lac = new List<AsyncClient>();
					string[] eps = new string[m_OnlineClients.Keys.Count];
					m_OnlineClients.Keys.CopyTo(eps, 0);
					AsyncClient ac = null;
					foreach (string ep in eps)
					{
						if (m_OnlineClients[ep] == null || m_OnlineClients[ep].Socket == null
							 || !m_OnlineClients[ep].Connected)
						{
							m_OnlineClients.TryRemove(ep, out ac);
							if (ac != null)
								ac.Dispose();
						}
					}
				}
			}
			catch { }
		}
		#endregion

		#region AsyncClient Events
		#region Private Method : void ac_OnBeforeSended(object sender, AsyncClientEventArgs e)
		private void ac_OnBeforeSended(object sender, AsyncClientEventArgs e)
		{
			try
			{
				if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.BytesOfSendQueue] != null)
					m_Counters[ServerCounterType.BytesOfSendQueue].IncrementBy(e.Data.Length);
			}
			catch (Exception ex) { _log.WriteException(ex); }
		}
		#endregion

		#region Private Method : void ac_OnDataSended(object sender, AsyncClientEventArgs e)
		private void ac_OnDataSended(object sender, AsyncClientEventArgs e)
		{
			try
			{
				int count = e.Data.Length;
				if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.TotalSendedBytes] != null)
					m_Counters[ServerCounterType.TotalSendedBytes].IncrementBy(count);
				if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.RateOfSendedBytes] != null)
					m_Counters[ServerCounterType.RateOfSendedBytes].IncrementBy(count);
				if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.BytesOfSendQueue] != null)
					m_Counters[ServerCounterType.BytesOfSendQueue].IncrementBy(-count);
			}
			catch (Exception ex) { _log.WriteException(ex); }

			AsyncClient ac = (AsyncClient)sender;

			#region 產生事件 - OnDataSended
			if (this.OnDataSended != null)
			{
				SocketServerEventArgs asea = new SocketServerEventArgs(ac, e.Data);
				asea.SetExtraInfo(e.ExtraInfo);
				switch (this.UseAsyncCallback)
				{
					case EventCallbackMode.BeginInvoke:
						#region 非同步呼叫 - BeginInvoke
						{
							foreach (EventHandler<SocketServerEventArgs> del in this.OnDataSended.GetInvocationList())
							{
								try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Invoke:
						#region 同步呼叫 - DynamicInvoke
						{
							foreach (Delegate del in this.OnDataSended.GetInvocationList())
							{
								try { del.DynamicInvoke(this, asea); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Thread:
						#region 建立執行緒 - Thread
						{
							foreach (Delegate del in this.OnDataSended.GetInvocationList())
							{
								try
								{
									EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
									ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
								}
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
				}
			}
			#endregion

		}
		#endregion

		#region Private Method : void ac_OnSendedFail(object sender, AsyncClientEventArgs e)
		private void ac_OnSendedFail(object sender, AsyncClientEventArgs e)
		{
			try
			{
				if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.SendFail] != null)
					m_Counters[ServerCounterType.SendFail].Increment();
				if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.RateOfSendFail] != null)
					m_Counters[ServerCounterType.RateOfSendFail].Increment();
				if (!m_IsShutdown && !m_IsDisposed && m_Counters[ServerCounterType.BytesOfSendQueue] != null)
					m_Counters[ServerCounterType.BytesOfSendQueue].IncrementBy(-e.Data.Length);
			}
			catch (Exception ex) { _log.WriteException(ex); }

			AsyncClient ac = (AsyncClient)sender;

			#region 產生事件 - OnSendedFail
			if (this.OnSendedFail != null)
			{
				SocketServerEventArgs asea = new SocketServerEventArgs(ac, e.Data);
				asea.SetExtraInfo(e.ExtraInfo);
				switch (this.UseAsyncCallback)
				{
					case EventCallbackMode.BeginInvoke:
						#region 非同步呼叫 - BeginInvoke
						{
							foreach (EventHandler<SocketServerEventArgs> del in this.OnSendedFail.GetInvocationList())
							{
								try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Invoke:
						#region 同步呼叫 - DynamicInvoke
						{
							foreach (Delegate del in this.OnSendedFail.GetInvocationList())
							{
								try { del.DynamicInvoke(this, asea); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Thread:
						#region 建立執行緒 - Thread
						{
							foreach (Delegate del in this.OnSendedFail.GetInvocationList())
							{
								try
								{
									EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
									ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
								}
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
				}
			}
			#endregion

		}
		#endregion

		#region Private Method : void ac_OnClosing(object sender, AsyncClientEventArgs e)
		private void ac_OnClosing(object sender, AsyncClientEventArgs e)
		{
			AsyncClient ac = (AsyncClient)sender;
			if (ac != null)
			{
				AsyncClient acc = null;
				m_OnlineClients.TryRemove(ac.RemoteEndPoint.ToString(), out acc);
			}

			#region 產生事件 - OnClientClosing
			if (this.OnClientClosing != null)
			{
				SocketServerEventArgs asea = new SocketServerEventArgs(ac);
				asea.SetExtraInfo(e.ExtraInfo);
				switch (this.UseAsyncCallback)
				{
					case EventCallbackMode.BeginInvoke:
						#region 非同步呼叫 - BeginInvoke
						{
							foreach (EventHandler<SocketServerEventArgs> del in this.OnClientClosing.GetInvocationList())
							{
								try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Invoke:
						#region 同步呼叫 - DynamicInvoke
						{
							foreach (Delegate del in this.OnClientClosing.GetInvocationList())
							{
								try { del.DynamicInvoke(this, asea); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Thread:
						#region 建立執行緒 - Thread
						{
							foreach (Delegate del in this.OnClientClosing.GetInvocationList())
							{
								try
								{
									EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
									ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
								}
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
				}
			}
			#endregion
		}
		#endregion

		#region Private Method : void ac_OnClosed(object sender, AsyncClientEventArgs e)
		private void ac_OnClosed(object sender, AsyncClientEventArgs e)
		{
			AsyncClient ac = (AsyncClient)sender;

			if (ac != null)
			{
				AsyncClient acc = null;
				m_OnlineClients.TryRemove(ac.RemoteEndPoint.ToString(), out acc);
			}

			#region 產生事件 - OnClientClosed
			if (this.OnClientClosed != null)
			{
				SocketServerEventArgs asea = new SocketServerEventArgs(ac);
				asea.ClosedByIdle = e.ClosedByIdle;
				asea.SetExtraInfo(e.ExtraInfo);
				switch (this.UseAsyncCallback)
				{
					case EventCallbackMode.BeginInvoke:
						#region 非同步呼叫 - BeginInvoke
						{
							foreach (EventHandler<SocketServerEventArgs> del in this.OnClientClosed.GetInvocationList())
							{
								try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Invoke:
						#region 同步呼叫 - DynamicInvoke
						{
							foreach (Delegate del in this.OnClientClosed.GetInvocationList())
							{
								try { del.DynamicInvoke(this, asea); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Thread:
						#region 建立執行緒 - Thread
						{
							foreach (Delegate del in this.OnClientClosed.GetInvocationList())
							{
								try
								{
									EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
									ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
								}
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
				}
			}
			#endregion
		}
		#endregion
		#endregion

		#region Private Method : void EventThreadWorker(object o)
		private void EventThreadWorker(object o)
		{
			try
			{
				EventThreadVariables etv = (EventThreadVariables)o;
				etv.InvokeMethod.DynamicInvoke(etv.Args);
			}
			catch (Exception ex) { _log.WriteException(ex); }
		}
		#endregion
	}
}


