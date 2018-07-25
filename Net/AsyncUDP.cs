using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CJF.Utility;

namespace CJF.Net
{
	/// <summary>
	/// 非同步 UDP 連線元件，使用xxxAsync<br/>
	/// 本類別有個潛在問題：<br/>
	/// 當同時兩個以上不同來源的封包進來時，第二個後面的封包會與跟著第一個封包，且RemoteEndPoint會是第一個封包位置<br />
	/// Updated:<br />
	/// [2013/06/12] 嘗試修正上述前在問題，但尚未進行壓力測試
	/// </summary>
	[Serializable]
	public class AsyncUDP : IDisposable
	{
		#region Variables
		LogManager _log = new LogManager(typeof(AsyncUDP));
		Socket m_UdpSocket;							// 伺服器 Socket 物件
		SocketAsyncEventArgs m_ReadEventArgs;
		byte[] m_ReceiveBuffer;
		int m_BufferSize = 0;						// 緩衝暫存區大小
		long m_ReceiveByteCount = 0;
		long m_SendByteCount = 0;
		long m_WaittingSend = 0;
		bool m_ServerStarted = false;
		bool m_IsExit = false;
		bool m_IsDisposed = false;
		bool m_EnableBroadcast = false;
		IntPtr m_Handle = IntPtr.Zero;
		EndPoint m_LocalEndPoint = null;
		SocketDebugType m_Debug = SocketDebugType.None;
		Timer _SecondCounter = null;
		/// <summary>效能監視器集合</summary>
		Dictionary<ServerCounterType, PerformanceCounter> m_Counters = null;
		#endregion

		#region Events
		/// <summary>開始監聽通訊埠時觸發</summary>
		public event EventHandler<AsyncUdpEventArgs> MonitorStarted;
		/// <summary>停止監聽通訊埠時觸發</summary>
		public event EventHandler<AsyncUdpEventArgs> MonitorStoped;
		/// <summary>當資料送出後觸發的事件</summary>
		public event EventHandler<AsyncUdpEventArgs> DataSended;
		/// <summary>當接收到資料時觸發的事件, 勿忘處理黏包的狀況</summary>
		public event EventHandler<AsyncUdpEventArgs> DataReceived;
		/// <summary>當連線發生錯誤時觸發</summary>
		public event EventHandler<AsyncUdpEventArgs> Exception;
		/// <summary>當每秒傳送計數器值變更時產生</summary>
		public event EventHandler<DataTransEventArgs> CounterChanged;
		#endregion

		#region Construct Method : AsyncUDP(int receiveBufferSize)
		/// <summary>建立新的 AsyncUDP 類別，並初始化相關屬性值</summary>
		/// <param name="bufferSize">接收緩衝暫存區大小</param>
		public AsyncUDP(int bufferSize)
		{
			m_IsExit = false;
			m_Counters = new Dictionary<ServerCounterType, PerformanceCounter>();
			m_BufferSize = bufferSize;
			m_ServerStarted = false;
			CommonSettings();
			SetCounterDictionary();
			m_UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			m_Handle = m_UdpSocket.Handle;
			m_UdpSocket.ReceiveBufferSize = m_BufferSize;
			m_UdpSocket.SendBufferSize = m_BufferSize;
			m_UdpSocket.EnableBroadcast = m_EnableBroadcast;
		}
		/// <summary>釋放 AsyncUDP 所使用的資源。 </summary>
		~AsyncUDP() { Dispose(false); }
		#endregion

		#region Private Method : void SetCounterDictionary()
		private void SetCounterDictionary()
		{
			m_Counters.Clear();
			m_Counters.Add(ServerCounterType.TotalReceivedBytes, null);
			m_Counters.Add(ServerCounterType.RateOfReceivedBytes, null);
			m_Counters.Add(ServerCounterType.TotalSendedBytes, null);
			m_Counters.Add(ServerCounterType.RateOfSendedBytes, null);
			m_Counters.Add(ServerCounterType.BytesOfSendQueue, null);
			m_Counters.Add(ServerCounterType.SendFail, null);
			m_Counters.Add(ServerCounterType.RateOfSendFail, null);
		}
		#endregion

		#region Private Method : void LoadCounterDictionary(string categoryName, string instanceName)
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

			m_Counters[ServerCounterType.TotalReceivedBytes] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_TOTAL_RECEIVED, instanceName, false);
			m_Counters[ServerCounterType.RateOfReceivedBytes] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_RATE_OF_RECEIVED, instanceName, false);
			m_Counters[ServerCounterType.TotalSendedBytes] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_TOTAL_SENDED, instanceName, false);
			m_Counters[ServerCounterType.RateOfSendedBytes] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_RATE_OF_SENDED, instanceName, false);
			m_Counters[ServerCounterType.BytesOfSendQueue] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_BYTES_QUEUE, instanceName, false);
			m_Counters[ServerCounterType.SendFail] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_SAND_FAIL, instanceName, false);
			m_Counters[ServerCounterType.RateOfSendFail] = new PerformanceCounter(categoryName, AsyncServerConsts.SOCKET_COUNTER_RATE_OF_SAND_FAIL, instanceName, false);
		}
		#endregion

		#region Private Method : void CommonSettings()
		private void CommonSettings()
		{
			// 預設皆使用 DynamicInvoke
			this.UseAsyncCallback = EventCallbackMode.Invoke;
			_SecondCounter = new Timer(SecondCounterCallback, null, 1000, 1000);
		}
		#endregion

		#region Public Method : void Start(IPEndPoint localEndPoint)
		/// <summary>開始監聽資料傳入</summary>
		/// <param name="localEndPoint">欲監聽的本地傾聽通訊埠</param>
		public void Start(EndPoint localEndPoint)
		{
			if (m_UdpSocket == null)
			{
				m_UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				m_Handle = m_UdpSocket.Handle;
				m_UdpSocket.ReceiveBufferSize = m_BufferSize;
				m_UdpSocket.SendBufferSize = m_BufferSize;
				m_UdpSocket.EnableBroadcast = m_EnableBroadcast;
			}
			m_IsExit = false;
			try
			{
				m_UdpSocket.Bind(localEndPoint);
			}
			catch (Exception ex)
			{
				this.OnException(ex, localEndPoint);
				return;
			}
			m_LocalEndPoint = localEndPoint;
			m_ReceiveBuffer = new byte[m_BufferSize];
			m_ReadEventArgs = new SocketAsyncEventArgs();
			m_ReadEventArgs.UserToken = new AsyncUserToken(m_UdpSocket, m_BufferSize);
			m_ReadEventArgs.RemoteEndPoint = localEndPoint;
			m_ReadEventArgs.DisconnectReuseSocket = true;
			m_ReadEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
			m_ReadEventArgs.SetBuffer(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length);

			m_ServerStarted = true;

			this.OnMonitorStarted(m_Handle, this.LocalEndPort);

			if (!m_UdpSocket.ReceiveFromAsync(m_ReadEventArgs))
				this.ProcessReceive(m_ReadEventArgs);
		}
		#endregion

		#region Public Method : void Shutdown()
		/// <summary>關閉伺服器</summary>
		public void Shutdown()
		{
			if (m_IsDisposed || m_IsExit) return;
			m_IsExit = true;

			if (m_UdpSocket != null)
			{
				try
				{
					if (m_Debug.HasFlag(SocketDebugType.Shutdown))
					{
						Console.WriteLine("[{0}]Socket : Before Shutdown In AsyncUDP.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Before Shutdown In AsyncUDP.Shutdown");
					}
					m_UdpSocket.Shutdown(SocketShutdown.Both);
					if (m_Debug.HasFlag(SocketDebugType.Shutdown))
					{
						Console.WriteLine("[{0}]Socket : After Shutdown In AsyncUDP.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "After Shutdown In AsyncUDP.Shutdown");
					}
				}
				catch { }
				finally
				{
					if (m_Debug.HasFlag(SocketDebugType.Close))
					{
						Console.WriteLine("[{0}]Socket : Before Close In AsyncUDP.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Before Close In AsyncUDP.Shutdown");
					}
					m_UdpSocket.Close();
					if (m_Debug.HasFlag(SocketDebugType.Close))
					{
						Console.WriteLine("[{0}]Socket : After Close In AsyncUDP.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "After Close In AsyncUDP.Shutdown");
					}
				}
			}
			m_UdpSocket = null;
			m_ServerStarted = false;
			this.OnMonitorStoped(m_Handle, this.LocalEndPort);
		}
		#endregion

		#region Public Method : SendData(EndPoint remoteEndPoint, byte[] data, object extraInfo)
		/// <summary>使用傳送資料到用戶端</summary>
		/// <param name="remoteEndPoint">接收對象的通訊埠位置</param>
		/// <param name="data">欲發送的資料</param>
		/// <param name="extraInfo">欲傳遞的額外資訊</param>
		public void SendData(EndPoint remoteEndPoint, byte[] data, object extraInfo = null)
		{
			SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
			arg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
			arg.RemoteEndPoint = remoteEndPoint;
			arg.UserToken = extraInfo;
			Interlocked.Add(ref m_WaittingSend, data.Length);
			m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(data.Length);
			arg.SetBuffer(data, 0, data.Length);
			if (m_Debug.HasFlag(SocketDebugType.Send))
			{
				Console.WriteLine("[{0}]Socket : Before SendAsync In AsyncUDP.SendData", DateTime.Now.ToString("HH:mm:ss.fff"));
				_log.Write(LogManager.LogLevel.Debug, "Before SendAsync In AsyncUDP.SendData");
			}
			try
			{
				if (!m_UdpSocket.SendToAsync(arg))
					this.ProcessSend(arg);
			}
			catch (Exception)
			{
				Interlocked.Add(ref m_WaittingSend, -data.Length);
				m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(-data.Length);
				m_Counters[ServerCounterType.SendFail]?.Increment();
				m_Counters[ServerCounterType.RateOfSendFail]?.Increment();
				this.ProcessError(arg);
			}
			if (m_Debug.HasFlag(SocketDebugType.Send))
			{
				Console.WriteLine("[{0}]Socket : After SendAsync In AsyncUDP.SendData", DateTime.Now.ToString("HH:mm:ss.fff"));
				_log.Write(LogManager.LogLevel.Debug, "After SendAsync In AsyncUDP.SendData");
			}
		}
		#endregion

		#region Public Method : void BroadcastData(int port, byte[] data)
		/// <summary>廣播資料到網路上</summary>
		/// <param name="port">接收端的通訊埠號</param>
		/// <param name="data">欲發送的資料</param>
		/// <exception cref="System.Net.Sockets.SocketException">ErrorCode回傳10013時，請先設定EnableBroadcast屬性為true</exception>
		public void BroadcastData(int port, byte[] data)
		{
			if (!m_EnableBroadcast)
				throw new SocketException(10013);
			SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
			arg.RemoteEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
			Interlocked.Add(ref m_WaittingSend, data.Length);
			m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(data.Length);
			arg.SetBuffer(data, 0, data.Length);
			if (m_Debug.HasFlag(SocketDebugType.Send))
			{
				Console.WriteLine("[{0}]Socket : Before SendAsync In AsyncUDP.BroadcastData", DateTime.Now.ToString("HH:mm:ss.fff"));
				_log.Write(LogManager.LogLevel.Debug, "Before SendAsync In AsyncUDP.BroadcastData");
			}
			try
			{
				if (!m_UdpSocket.SendToAsync(arg))
					this.ProcessSend(arg);
			}
			catch (Exception)
			{
				Interlocked.Add(ref m_WaittingSend, -data.Length);
				m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(-data.Length);
				m_Counters[ServerCounterType.SendFail]?.Increment();
				m_Counters[ServerCounterType.RateOfSendFail]?.Increment();
				this.ProcessError(arg);
			}
			if (m_Debug.HasFlag(SocketDebugType.Send))
			{
				Console.WriteLine("[{0}]Socket : After SendAsync In AsyncUDP.BroadcastData", DateTime.Now.ToString("HH:mm:ss.fff"));
				_log.Write(LogManager.LogLevel.Debug, "After SendAsync In AsyncUDP.BroadcastData");
			}
		}
		#endregion

		#region Public Static Method : bool Broadcast(int port, byte[] data)
		/// <summary>使用非同步方式廣播資料到網路上</summary>
		/// <param name="port">接收端的通訊埠號</param>
		/// <param name="data">欲發送的資料</param>
		/// <returns>發送成功或失敗</returns>
		/// <exception cref="System.InvalidOperationException">通訊埠正在使用中。</exception>
		public static bool Broadcast(int port, byte[] data)
		{
			const int BUFFER_SIZE = 512;
			Exception e = null;
			try
			{
				using (Socket udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
				{
					udp.ReceiveBufferSize = BUFFER_SIZE;
					udp.SendBufferSize = BUFFER_SIZE;
					udp.EnableBroadcast = true;
					udp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
					udp.DontFragment = true;
					udp.MulticastLoopback = false;
					using (SocketAsyncEventArgs arg = new SocketAsyncEventArgs())
					{
						arg.RemoteEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
						arg.UserToken = new AsyncUserToken(udp, BUFFER_SIZE);
						arg.DisconnectReuseSocket = true;
						//arg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object sender, SocketAsyncEventArgs e)
						//    {
						//    });
						arg.SetBuffer(data, 0, data.Length);
						try { udp.SendToAsync(arg); }
						catch (InvalidOperationException ex) { e = ex; }
					}
				}
			}
			catch (Exception) { return false; }
			if (e != null)
				throw e;
			else
				return true;
		}
		#endregion

		#region Properties
		/// <summary>取得伺服器連線物件</summary>
		public Socket Socket { get { return m_UdpSocket; } }
		/// <summary>取得緩衝區最大值</summary>
		public int BufferSize { get { return m_BufferSize; } }
		/// <summary>取得 Socket 的控制代碼</summary>
		public IntPtr Handle { get { return m_Handle; } }
		/// <summary>取得本地端通訊埠</summary>
		public EndPoint LocalEndPort { get { return m_LocalEndPoint; } }
		/// <summary>取得值，目前伺服器否啟動中</summary>
		public bool IsStarted { get { return m_ServerStarted; } }
		/// <summary>取得或設定是否為除錯模式</summary>
		public SocketDebugType Debug
		{
			get { return m_Debug; }
			set
			{
				if (m_Debug == value) return;
				m_Debug = value;
			}
		}
		/// <summary>取得每一秒的接收量，單位:位元組</summary>
		public long ReceiveSpeed { get { return m_ReceiveByteCount; } }
		/// <summary>取得每一秒發送量，單位:位元組</summary>
		public long SendSpeed { get { return m_SendByteCount; } }
		/// <summary>取得現在等待發送的資料量，單位:位元組</summary>
		public long WaittingSend { get { return m_WaittingSend; } }
		/// <summary>取得值，是否已Disposed</summary>
		public bool IsDisposed { get { return m_IsDisposed; } }
		/// <summary>設定或取得，是否可以用廣播的方式發送或接收資料</summary>
		public bool EnableBroadcast
		{
			get { return m_EnableBroadcast; }
			set
			{
				m_EnableBroadcast = value;
				m_UdpSocket.EnableBroadcast = m_EnableBroadcast;
				m_UdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, m_UdpSocket.EnableBroadcast);
				m_UdpSocket.DontFragment = m_EnableBroadcast;
				m_UdpSocket.MulticastLoopback = false;
			}
		}
		/// <summary>取得與設定，是否使用非同步方式產生回呼事件，預設使用同步呼叫(Invoke)</summary>
		public EventCallbackMode UseAsyncCallback { get; set; }
		#endregion

		#region Delegate Callback Methods
		private void TransferCounterCallback(IAsyncResult result)
		{
			EventHandler<DataTransEventArgs> del = result.AsyncState as EventHandler<DataTransEventArgs>;
			del.EndInvoke(result);
		}
		private void AsyncUdpEventCallback(IAsyncResult result)
		{
			EventHandler<AsyncUdpEventArgs> del = result.AsyncState as EventHandler<AsyncUdpEventArgs>;
			del.EndInvoke(result);
		}
		#endregion

		#region Private Method : void SecondCounterCallback(object o)
		private void SecondCounterCallback(object o)
		{
			if (m_IsDisposed) return;
			long sBytes = Interlocked.Read(ref m_SendByteCount);
			Interlocked.Exchange(ref m_SendByteCount, 0);
			long rBytes = Interlocked.Read(ref m_ReceiveByteCount);
			Interlocked.Exchange(ref m_ReceiveByteCount, 0);
			long wBytes = Interlocked.Read(ref m_WaittingSend);
			Interlocked.Exchange(ref m_WaittingSend, 0);

			this.OnCounterChanged(sBytes, rBytes, wBytes);
		}
		#endregion

		#region Private Method : void IO_Completed(object sender, SocketAsyncEventArgs e)
		/// <summary>當完成動作時，則呼叫此回呼函示。完成的動作由 SocketAsyncEventArg.LastOperation 屬性取得</summary>
		/// <param name="sender">AsyncUDP 物件</param>
		/// <param name="e">完成動作的 SocketAsyncEventArg 物件</param>
		private void IO_Completed(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.ReceiveFrom:
					this.ProcessReceive(e);
					break;
				case SocketAsyncOperation.SendTo:
					this.ProcessSend(e);
					break;
				default:
					throw new ArgumentException("The last operation completed on the socket was not a receive or send");
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
			if (m_IsExit) return;
			if (e.BytesTransferred > 0)
			{
				if (m_Debug.HasFlag(SocketDebugType.Receive))
				{
					Console.WriteLine("[{0}]Socket : Exec ReceiveAsync In AsyncUDP.ProcessReceive", DateTime.Now.ToString("HH:mm:ss.fff"));
					_log.Write(LogManager.LogLevel.Debug, "Exec ReceiveAsync In AsyncUDP.ProcessReceive");
				}
				if (e.SocketError == SocketError.Success)
				{
					AsyncUserToken token = e.UserToken as AsyncUserToken;
					if (token == null || token.IsDisposed)
						return;
					Socket s = token.Client;
					int count = e.BytesTransferred;
					Interlocked.Add(ref m_ReceiveByteCount, count);
					m_Counters[ServerCounterType.TotalReceivedBytes]?.IncrementBy(count);
					m_Counters[ServerCounterType.RateOfReceivedBytes]?.IncrementBy(count);
					List<byte> rec = new List<byte>();
					bool isSameRemote = true;
					string rip = null;
					if (e.RemoteEndPoint != null)
						rip = ((IPEndPoint)e.RemoteEndPoint).ToString();
					if ((token.CurrentIndex + count) > token.BufferSize)
					{
						rec.AddRange(token.ReceivedData);
						token.ClearBuffer();
					}
					token.SetData(e);
					if (rip != null && s.Available == 0)
					{
						// 檢查前一封包與本封包來源是否相同 By Jaofeng Chen @ 2013/06/12
						isSameRemote = rip.CompareTo(((IPEndPoint)e.RemoteEndPoint).ToString()) == 0;
						if (isSameRemote)
						{
							// 相同，則加到前一封包後面 By Jaofeng Chen @ 2013/06/12
							rec.AddRange(token.ReceivedData);
							token.ClearBuffer();
						}
					}

					this.OnDataReceived(rec.ToArray(), s.Handle, s.LocalEndPoint, e.RemoteEndPoint);

					if (m_Debug.HasFlag(SocketDebugType.Receive))
					{
						Console.WriteLine("[{0}]Socket : Before ReceiveAsync In AsyncUDP.ProcessReceive", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Before ReceiveAsync In AsyncUDP.ProcessReceive");
					}
					try
					{
						if (!s.ReceiveFromAsync(e))	// 讀取下一個由客戶端傳送的封包
							this.ProcessReceive(e);
					}
					catch (ObjectDisposedException) { }
					catch (Exception ex)
					{
						_log.Write(LogManager.LogLevel.Debug, "AsyncUDP.ProcessReceive");
						_log.WriteException(ex);
					}
					if (m_Debug.HasFlag(SocketDebugType.Receive))
					{
						Console.WriteLine("[{0}]Socket : After ReceiveAsync In AsyncUDP.ProcessReceive", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "After ReceiveAsync In AsyncUDP.ProcessReceive");
					}
					if (!isSameRemote)
					{
						// 如果有不同來源的封包資料，則再做一次 By Jaofeng Chen @ 2013/06/12
						this.ProcessReceive(e);
					}
				}
				else
					this.ProcessError(e);
			}
		}
		#endregion

		#region Private Method : void ProcessSend(SocketAsyncEventArgs e)
		/// <summary>當完成傳送資料時，將呼叫此函示</summary>
		/// <param name="e">SocketAsyncEventArg associated with the completed send operation.</param>
		private void ProcessSend(SocketAsyncEventArgs e)
		{
			if (e.BytesTransferred > 0)
			{
				try
				{
					if (m_Debug.HasFlag(SocketDebugType.Send))
					{
						Console.WriteLine("[{0}]Socket : Exec SendAsync In AsyncUDP.ProcessSend", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Exec SendAsync In AsyncUDP.ProcessSend");
					}
					if (e.SocketError == SocketError.Success)
					{
						int count = e.BytesTransferred;
						Interlocked.Add(ref m_SendByteCount, count);
						Interlocked.Add(ref m_WaittingSend, -count);
						m_Counters[ServerCounterType.TotalSendedBytes]?.IncrementBy(count);
						m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(-count);
						byte[] buffer = new byte[count];
						Array.Copy(e.Buffer, buffer, count);
						this.OnDataSended(buffer, e.RemoteEndPoint, e.UserToken);
						e.Dispose();
						e = null;
					}
					else
						this.ProcessError(e);
				}
				catch
				{
					this.ProcessError(e);
				}
			}
		}
		#endregion

		#region Private Method : void ProcessError(SocketAsyncEventArgs e)
		/// <summary>當發生錯誤時，將呼叫此函示，並關閉客戶端</summary>
		/// <param name="e">發生錯誤的 SocketAsyncEventArgs 物件</param>
		private void ProcessError(SocketAsyncEventArgs e)
		{
			AsyncUserToken token = e.UserToken as AsyncUserToken;
			Socket s = token.Client;
			EndPoint localEp = null;
			EndPoint remoteEp = null;
			IntPtr handle = IntPtr.Zero;
			try
			{
				if (s != null)
				{
					localEp = s.LocalEndPoint;
					remoteEp = s.RemoteEndPoint;
					handle = s.Handle;
				}
				else
				{
					localEp = m_UdpSocket.LocalEndPoint;
					remoteEp = m_UdpSocket.RemoteEndPoint;
					handle = m_UdpSocket.Handle;
				}

				SocketException se = new SocketException((Int32)e.SocketError);
				Exception ex = new Exception(string.Format("客戶端連線({1})發生錯誤:{0},狀態:{2}", (int)e.SocketError, localEp, e.LastOperation), se);
				this.OnException(ex, localEp, remoteEp);

				try
				{
					if (m_Debug.HasFlag(SocketDebugType.Shutdown))
					{
						Console.WriteLine("[{0}]Socket : Before Shutdown In AsyncUDP.ProcessError", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "Before Shutdown In AsyncUDP.ProcessError");
					}
					this.Shutdown();
					if (m_Debug.HasFlag(SocketDebugType.Shutdown))
					{
						Console.WriteLine("[{0}]Socket : After Shutdown In AsyncUDP.ProcessError", DateTime.Now.ToString("HH:mm:ss.fff"));
						_log.Write(LogManager.LogLevel.Debug, "After Shutdown In AsyncUDP.ProcessError");
					}
				}
				catch (Exception) { }	// 如果客戶端已關閉則不處理
			}
			catch { }
		}
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

		#region IDisposable
		/// <summary>清除並釋放 AsyncUDP 所使用的資源。</summary>
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
				try
				{
					m_Counters.Clear();
					m_Counters = null;
					if (m_UdpSocket == null)
					{
						if (m_Debug.HasFlag(SocketDebugType.Shutdown))
						{
							Console.WriteLine("[{0}]Socket Is Null In AsyncUDP.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
							_log.Write(LogManager.LogLevel.Debug, "Socket Is Null In AsyncUDP.Dispose");
						}
					}
					else
					{
						try
						{
							if (m_Debug.HasFlag(SocketDebugType.Shutdown))
							{
								Console.WriteLine("[{0}]Socket : Before Shutdown In AsyncUDP.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
								_log.Write(LogManager.LogLevel.Debug, "Before Shutdown In AsyncUDP.Dispose");
							}
							m_UdpSocket.Shutdown(SocketShutdown.Both);
							if (m_Debug.HasFlag(SocketDebugType.Shutdown))
							{
								Console.WriteLine("[{0}]Socket : After Shutdown In AsyncUDP.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
								_log.Write(LogManager.LogLevel.Debug, "After Shutdown In AsyncUDP.Dispose");
							}
						}
						catch { }
						finally
						{
							if (m_Debug.HasFlag(SocketDebugType.Close))
							{
								Console.WriteLine("[{0}]Socket : Before Close In AsyncUDP.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
								_log.Write(LogManager.LogLevel.Debug, "Before Close In AsyncUDP.Dispose");
							}
							m_UdpSocket.Close();
							if (m_Debug.HasFlag(SocketDebugType.Close))
							{
								Console.WriteLine("[{0}]Socket : After Close In AsyncUDP.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
								_log.Write(LogManager.LogLevel.Debug, "After Close In AsyncUDP.Dispose");
							}
							m_UdpSocket = null;
						}
					}
				}
				catch { }
			}
			m_IsDisposed = true;
		}
		#endregion

		#region Protected Virtual Method : void OnMonitorStarted(IntPtr handle, EndPoint local, object extraInfo = null)
		/// <summary>產生 MonitorStarted 事件</summary>
		/// <param name="handle">產生事件的 Socket 控制碼</param>
		/// <param name="local">本地端點資訊</param>
		/// <param name="extraInfo">額外資訊</param>
		protected virtual void OnMonitorStarted(IntPtr handle, EndPoint local, object extraInfo = null)
		{
			if (this.MonitorStarted != null)
			{
				AsyncUdpEventArgs auea = new AsyncUdpEventArgs(handle, local, null);
				auea.SetExtraInfo(extraInfo);
				switch (this.UseAsyncCallback)
				{
					case EventCallbackMode.BeginInvoke:
						#region 非同步作法 - BeginInvoke
						{
							foreach (EventHandler<AsyncUdpEventArgs> del in this.MonitorStarted.GetInvocationList())
							{
								try { del.BeginInvoke(this, auea, new AsyncCallback(AsyncUdpEventCallback), del); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Invoke:
						#region 同步作法 - DynamicInvoke
						{
							foreach (Delegate del in this.MonitorStarted.GetInvocationList())
							{
								try { del.DynamicInvoke(this, auea); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Thread:
						#region 建立執行緒 - Thread
						{
							foreach (Delegate del in this.MonitorStarted.GetInvocationList())
							{
								try
								{
									EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, auea } };
									ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
								}
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnMonitorStoped(IntPtr handle, EndPoint local, object extraInfo = null)
		/// <summary>產生 MonitorStoped 事件</summary>
		/// <param name="handle">產生事件的 Socket 控制碼</param>
		/// <param name="local">本地端點資訊</param>
		/// <param name="extraInfo">額外資訊</param>
		protected virtual void OnMonitorStoped(IntPtr handle, EndPoint local, object extraInfo = null)
		{
			if (this.MonitorStoped != null)
			{
				AsyncUdpEventArgs auea = new AsyncUdpEventArgs(handle, local, null);
				auea.SetExtraInfo(extraInfo);
				switch (this.UseAsyncCallback)
				{
					case EventCallbackMode.BeginInvoke:
						#region 非同步作法 - BeginInvoke
						{
							foreach (EventHandler<AsyncUdpEventArgs> del in this.MonitorStoped.GetInvocationList())
							{
								try { del.BeginInvoke(this, auea, new AsyncCallback(AsyncUdpEventCallback), del); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Invoke:
						#region 同步作法 - DynamicInvoke
						{
							foreach (Delegate del in this.MonitorStoped.GetInvocationList())
							{
								try { del.DynamicInvoke(this, auea); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Thread:
						#region 建立執行緒 - Thread
						{
							foreach (Delegate del in this.MonitorStoped.GetInvocationList())
							{
								try
								{
									EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, auea } };
									ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
								}
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnDataReceived(byte[] buffer, IntPtr handle, EndPoint local, EndPoint remote, object extraInfo = null)
		/// <summary>產生 DataReceived 事件</summary>
		/// <param name="buffer">已收到的資料內容</param>
		/// <param name="handle">本端 Socket 控制碼</param>
		/// <param name="local">本地位址資訊</param>
		/// <param name="remote">遠端位址資訊</param>
		/// <param name="extraInfo">額外資訊</param>
		protected virtual void OnDataReceived(byte[] buffer, IntPtr handle, EndPoint local, EndPoint remote, object extraInfo = null)
		{
			if (buffer.Length > 0 && this.DataReceived != null)
			{
				AsyncUdpEventArgs auea = new AsyncUdpEventArgs(handle, local, remote, buffer);
				switch (this.UseAsyncCallback)
				{
					case EventCallbackMode.BeginInvoke:
						#region 非同步呼叫 - BeginInvoke
						{
							foreach (EventHandler<AsyncUdpEventArgs> del in this.DataReceived.GetInvocationList())
							{
								try { del.BeginInvoke(this, auea, new AsyncCallback(AsyncUdpEventCallback), del); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Invoke:
						#region 同步呼叫 - DynamicInvoke
						{
							foreach (Delegate del in this.DataReceived.GetInvocationList())
							{
								try { del.DynamicInvoke(this, auea); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Thread:
						#region 建立執行緒 - Thread
						{
							foreach (Delegate del in this.DataReceived.GetInvocationList())
							{
								try
								{
									EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, auea } };
									ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
								}
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnDataSended(byte[] buffer, EndPoint remote, object extraInfo = null)
		/// <summary>產生 DataSended 事件</summary>
		/// <param name="buffer">已傳送的資料內容</param>
		/// <param name="remote">遠端端點資訊</param>
		/// <param name="extraInfo">額外資訊</param>
		protected virtual void OnDataSended(byte[] buffer, EndPoint remote, object extraInfo = null)
		{
			if (this.DataSended != null)
			{
				try
				{
					AsyncUdpEventArgs auea = new AsyncUdpEventArgs(m_UdpSocket.Handle, m_UdpSocket.LocalEndPoint, remote, buffer);
					auea.SetExtraInfo(extraInfo);
					switch (this.UseAsyncCallback)
					{
						case EventCallbackMode.BeginInvoke:
							#region 非同步呼叫 - BeginInvoke
							{
								foreach (EventHandler<AsyncUdpEventArgs> del in this.DataSended.GetInvocationList())
								{
									try { del.BeginInvoke(this, auea, new AsyncCallback(AsyncUdpEventCallback), del); }
									catch (Exception ex) { _log.WriteException(ex); }
								}
								break;
							}
							#endregion
						case EventCallbackMode.Invoke:
							#region 同步呼叫 - DynamicInvoke
							{
								foreach (Delegate del in this.DataSended.GetInvocationList())
								{
									try { del.DynamicInvoke(this, auea); }
									catch (Exception ex) { _log.WriteException(ex); }
								}
								break;
							}
							#endregion
						case EventCallbackMode.Thread:
							#region 建立執行緒 - Thread
							{
								foreach (Delegate del in this.DataSended.GetInvocationList())
								{
									try
									{
										EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, auea } };
										ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
									}
									catch (Exception ex) { _log.WriteException(ex); }
								}
								break;
							}
							#endregion
					}
				}
				catch (Exception ex) { _log.WriteException(ex); }
			}
		}
		#endregion

		#region Protected Virtual Method : void OnException(Exception ex, EndPoint local = null, EndPoint remote = null, object extraInfo = null)
		/// <summary>產生 Exception 事件</summary>
		/// <param name="ex">錯誤原因</param>
		/// <param name="local">本地端點資訊</param>
		/// <param name="remote">遠端端點資訊</param>
		/// <param name="extraInfo">額外資訊</param>
		protected virtual void OnException(Exception ex, EndPoint local = null, EndPoint remote = null, object extraInfo = null)
		{
			if (this.Exception != null)
			{
				AsyncUdpEventArgs auea = new AsyncUdpEventArgs(m_UdpSocket.Handle, local, remote, null, ex);
				auea.SetExtraInfo(extraInfo);
				switch (this.UseAsyncCallback)
				{
					case EventCallbackMode.BeginInvoke:
						#region 非同步呼叫 - BeginInvoke
						{
							foreach (EventHandler<AsyncUdpEventArgs> del in this.Exception.GetInvocationList())
							{
								try { del.BeginInvoke(this, auea, new AsyncCallback(AsyncUdpEventCallback), del); }
								catch (Exception exx) { _log.WriteException(exx); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Invoke:
						#region 同步呼叫 - DynamicInvoke
						{
							foreach (Delegate del in this.Exception.GetInvocationList())
							{
								try { del.DynamicInvoke(this, auea); }
								catch (Exception exx) { _log.WriteException(exx); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Thread:
						#region 建立執行緒 - Thread
						{
							foreach (Delegate del in this.Exception.GetInvocationList())
							{
								try
								{
									EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, auea } };
									ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
								}
								catch (Exception exx) { _log.WriteException(exx); }
							}
							break;
						}
						#endregion
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnCounterChanged(long sBytes, long rBytes, long wBytes)
		/// <summary>產生 CounterChanged 事件</summary>
		/// <param name="sBytes">已發送的 Bytes</param>
		/// <param name="rBytes">已接收的 Bytes</param>
		/// <param name="wBytes">等待發送的 Bytes</param>
		protected virtual void OnCounterChanged(long sBytes, long rBytes, long wBytes)
		{
			if (this.CounterChanged != null)
			{
				DataTransEventArgs dtea = new DataTransEventArgs(sBytes, rBytes, wBytes);
				switch (this.UseAsyncCallback)
				{
					case EventCallbackMode.BeginInvoke:
						#region 非同步呼叫 - BeginInvoke
						{
							foreach (EventHandler<DataTransEventArgs> del in this.CounterChanged.GetInvocationList())
							{
								try { del.BeginInvoke(this, dtea, new AsyncCallback(TransferCounterCallback), del); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Invoke:
						#region 同步呼叫 - DynamicInvoke
						{
							foreach (Delegate del in this.CounterChanged.GetInvocationList())
							{
								try { del.DynamicInvoke(this, dtea); }
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
					case EventCallbackMode.Thread:
						#region 建立執行緒 - Thread
						{
							foreach (Delegate del in this.CounterChanged.GetInvocationList())
							{
								try
								{
									EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, dtea } };
									ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
								}
								catch (Exception ex) { _log.WriteException(ex); }
							}
							break;
						}
						#endregion
				}
			}
		}
		#endregion
	}
}


