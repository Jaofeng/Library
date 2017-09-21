using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using CJF.Utility;

namespace CJF.Net
{
	#region Public Class : WebSocketClient
	/// <summary>
	/// 
	/// </summary>
	public class WebSocketClient : AsyncClient, IDisposable
	{
		LogManager _log = new LogManager(typeof(WebSocketClient));

		internal WebSocketClient(Socket sck) : this(sck, null, null) { }
		internal WebSocketClient(Socket sck, string acceptKey, string url) : base(sck)
		{
			this.AcceptKey = acceptKey;
			this.Url = url;
		}
		/// <summary>
		/// 
		/// </summary>
		~WebSocketClient() { Dispose(false); }

		/// <summary></summary>
		public string AcceptKey { get; private set; }
		/// <summary>取得連線的 URL 路徑</summary>
		public string Url { get; protected set; }
		/// <summary>取得文字編碼</summary>
		public new Encoding Encoding { get { return Encoding.UTF8; } }

		#region Public Override Method : void SendData(string data)
		/// <summary>傳送資料至遠端</summary>
		/// <param name="data">欲傳送的資料</param>
		public override void SendData(string data)
		{
			try
			{
				// 將資料字串轉成byte
				byte[] contentByte = Encoding.UTF8.GetBytes(data.ToString());
				List<byte> dataBytes = new List<byte>();

				if (contentByte.Length < 126)   // 資料長度小於126，Type1格式
				{
					// 未切割的Data Frame開頭
					dataBytes.Add((byte)0x81);
					dataBytes.Add((byte)contentByte.Length);
					dataBytes.AddRange(contentByte);
				}
				else if (contentByte.Length <= 65535)       // 長度介於126與65535(0xFFFF)之間，Type2格式
				{
					dataBytes.Add((byte)0x81);
					dataBytes.Add((byte)0x7E);              // 126
					// Extend Data 加長至2Byte
					dataBytes.Add((byte)((contentByte.Length >> 8) & 0xFF));
					dataBytes.Add((byte)((contentByte.Length) & 0xFF));
					dataBytes.AddRange(contentByte);
				}
				else                 // 長度大於65535，Type3格式
				{
					dataBytes.Add((byte)0x81);
					dataBytes.Add((byte)0x7F);              // 127
					// Extned Data 加長至8Byte
					dataBytes.Add((byte)((contentByte.Length >> 56) & 0xFF));
					dataBytes.Add((byte)((contentByte.Length >> 48) & 0xFF));
					dataBytes.Add((byte)((contentByte.Length >> 40) & 0xFF));
					dataBytes.Add((byte)((contentByte.Length >> 32) & 0xFF));
					dataBytes.Add((byte)((contentByte.Length >> 24) & 0xFF));
					dataBytes.Add((byte)((contentByte.Length >> 16) & 0xFF));
					dataBytes.Add((byte)((contentByte.Length >> 8) & 0xFF));
					dataBytes.Add((byte)((contentByte.Length) & 0xFF));
					dataBytes.AddRange(contentByte);
				}
				base.SendData(dataBytes.ToArray());
			}
			catch (Exception ex)
			{
				_log.WriteException(ex);
				this.Close();
			}
		}
		#endregion
	}
	#endregion

	#region Public Class : WebSocketServer
	/// <summary>WebSocket Server Class</summary>
	public class WebSocketServer : AsyncServer, IDisposable
	{
		const int BUFFER_SIZE = 1024;
		/// <summary>WebSocket 專用 GUID</summary>
		const string GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		LogManager _log = new LogManager(typeof(WebSocketServer));

		/// <summary>SHA1 加密類別</summary>
		SHA1 m_sha = null;


		List<string> m_Services = null;
		/// <summary>允許連接的服務名</summary>
		public string[] Services { get { return m_Services.ToArray(); } }
		/// <summary>取得所有遠端連線類別物件</summary>
		public new WebSocketClient[] Clients
		{
			get
			{
				WebSocketClient[] acs = new WebSocketClient[m_Clients.Values.Count];
				m_Clients.Values.CopyTo(acs, 0);
				return acs;
			}
		}


		#region Construct Method : WebSocketServer(int numConnections)
		/// <summary>建立新的 WebSocketServer 類別，並初始化相關屬性值</summary>
		/// <param name="maxClients">同時可連接的最大連線數</param>
		public WebSocketServer(int maxClients)
		{
			base.DebugMode = SocketDebugType.None;
			this.UseAsyncCallback = EventCallbackMode.BeginInvoke;
			m_Mutex = new Mutex();
			m_Services = new List<string>();
			m_sha = SHA1CryptoServiceProvider.Create();
			m_IsDisposed = false;
			m_LocalEndPort = null;
			m_Counters = new Dictionary<ServerCounterType, PerformanceCounter>();
			m_Pool = new SocketAsyncEventArgsPool();
			// 預留兩條線程，用於過多的連線數檢查
			m_MaxClients = new Semaphore(maxClients + 2, maxClients + 2);
			m_Clients = new ConcurrentDictionary<string, AsyncClient>();
			m_WaitToClean = new ConcurrentDictionary<EndPoint, int>();
			m_IsShutdown = false;
			this.IsStarted = false;
			for (int i = 0; i < maxClients; i++)
			{
				SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
				arg.Completed += new EventHandler<SocketAsyncEventArgs>(this.IO_Completed);
				arg.DisconnectReuseSocket = true;
				arg.SetBuffer(new Byte[BUFFER_SIZE], 0, BUFFER_SIZE);
				m_Pool.Push(arg);
			}
			SetCounterDictionary();
		}
		/// <summary></summary>
		~WebSocketServer() { Dispose(false); }
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

		#region Public Override Method : void Start(IPEndPoint localEndPoint)
		/// <summary>開始伺服器並等待連線請求, 如需引入效能監視器(PerformanceCounter)，請先執行LoadCounterDictionary函示</summary>
		/// <param name="localEndPoint">本地傾聽通訊埠</param>
		public override void Start(IPEndPoint localEndPoint)
		{
			m_LocalEndPort = localEndPoint;
			m_ListenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.IP);
			m_ListenSocket.ReceiveBufferSize = BUFFER_SIZE;
			m_ListenSocket.SendBufferSize = BUFFER_SIZE;
			m_ListenSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
			m_ListenSocket.NoDelay = true;
			m_ListenSocket.Bind(localEndPoint);
			m_ListenSocket.Listen(m_Pool.Count);
			m_IsShutdown = false;
			m_CleanClientTimer = new Timer(CleanInvalidClients, null, 10000, 10000);

			base.OnStarted();

			this.StartAccept(null);
			m_Mutex.WaitOne();
		}
		#endregion

		#region Public Method : void AppendService(string svc)
		/// <summary>新增服務</summary>
		/// <param name="svc">服務代碼</param>
		public void AppendService(string svc)
		{
			if (!m_Services.Exists(s => s.Equals(svc, StringComparison.OrdinalIgnoreCase)))
				m_Services.Add(svc);
		}
		#endregion

		#region Protected Override Method : void ProcessAccept(SocketAsyncEventArgs e)
		/// <summary>處理接受連線</summary>
		/// <param name="e">完成連線的 SocketAsyncEventArg 物件</param>
		protected override void ProcessAccept(SocketAsyncEventArgs e)
		{
			if (m_IsShutdown || m_IsDisposed) return;
			Socket s = e.AcceptSocket;
			if (s == null)
			{
				this.StartAccept(e);
				return;
			}
			int index = Thread.CurrentThread.ManagedThreadId;
			if (s.Connected)
			{
				try
				{
					SocketAsyncEventArgs readEventArgs = null;
					readEventArgs = m_Pool.Pop();
					if (readEventArgs != null)
					{
						WebSocketClient ac = ShakeHands(s);
						if (ac == null) return;
						readEventArgs.UserToken = new AsyncUserToken(s, BUFFER_SIZE);
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

						base.OnClientConnected(ac);
						try
						{
							if (!s.ReceiveAsync(readEventArgs))
								ProcessReceive(readEventArgs);
						}
						catch (ObjectDisposedException) { }
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
					if (s != null && token != null && token.Client != null)
						_log.Write(LogManager.LogLevel.Debug, "無法與 {0} 建立連線", token.Client.RemoteEndPoint);
					_log.WriteException(ex);

					WebSocketClient ac = new WebSocketClient(s);
					base.OnException(ac, ex);

					#region 三秒後強制斷線
					System.Threading.Tasks.Task.Factory.StartNew(o =>
					{
						try
						{
							WebSocketClient acc = (WebSocketClient)o;
							Thread.Sleep(3000);
							if (acc != null)
							{
								acc.Close();
								acc.Dispose();
								acc = null;
							}
						}
						catch (Exception exx) { Debug.Print(exx.Message); }
					}, ac);
					#endregion
				}
				catch (Exception ex)
				{
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

		#region Protected Override Method : void ProcessReceive(SocketAsyncEventArgs e)
		/// <summary>
		/// 當完成接收資料時，將呼叫此函示
		/// 如果客戶端關閉連接，將會一併關閉此連線(Socket)
		/// 如果收到數據接著將數據返回到客戶端
		/// </summary>
		/// <param name="e">已完成接收的 SocketAsyncEventArg 物件</param>
		protected override void ProcessReceive(SocketAsyncEventArgs e)
		{
			int index = Thread.CurrentThread.ManagedThreadId;
			AsyncUserToken token = e.UserToken as AsyncUserToken;
			if (token == null || token.IsDisposed || m_IsShutdown || m_IsDisposed)
				return;
			IntPtr origHandle = IntPtr.Zero;
			Socket s = token.Client;
			WebSocketClient ac = null;
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
					if (e.SocketError == SocketError.Success)
					{
						ac = (WebSocketClient)m_Clients[s.RemoteEndPoint.ToString()];
						int count = e.BytesTransferred;
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

						#region 解析封包內容
						byte[] data = rec.ToArray();
						if (!((data[0] & 0x80) == 0x80))
						{
							_log.Write("Exceed 1 Frame. Not Handle");
							return;
						}
						// 是否包含Mask(第一個bit為1代表有Mask)，沒有Mask則不處理
						if (!((data[1] & 0x80) == 0x80))
						{
							_log.Write("Exception: No Mask");
							this.CloseClientSocket(e);
							return;
						}
						// 資料長度 = dataBuffer[1] - 127
						int payloadLen = data[1] & 0x7F;
						byte[] masks = new byte[4];
						byte[] payloadData = null;
						switch (payloadLen)
						{
							case 126:
								#region 包含16 bit Extend Payload Length
								{
									Array.Copy(data, 4, masks, 0, 4);
									payloadLen = (UInt16)(data[2] << 8 | data[3]);
									payloadData = new Byte[payloadLen];
									Array.Copy(data, 8, payloadData, 0, payloadLen);
									break;
								}
								#endregion
							case 127:
								#region 包含 64 bit Extend Payload Length
								{
									Array.Copy(data, 10, masks, 0, 4);
									var uInt64Bytes = new Byte[8];
									for (int i = 0; i < 8; i++)
									{
										uInt64Bytes[i] = data[9 - i];
									}
									UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);

									payloadData = new Byte[len];
									for (UInt64 i = 0; i < len; i++)
										payloadData[i] = data[i + 14];
									break;
								}
								#endregion
							default:
								#region 沒有 Extend Payload Length
								{
									Array.Copy(data, 2, masks, 0, 4);
									payloadData = new Byte[payloadLen];
									Array.Copy(data, 6, payloadData, 0, payloadLen);
									break;
								}
								#endregion
						}
						// 使用 WebSocket Protocol 中的公式解析資料
						for (var i = 0; i < payloadLen; i++)
							payloadData[i] = (Byte)(payloadData[i] ^ masks[i % 4]);

						// 解析出的資料
						_log.Write("Data:{0}", ConvUtils.Byte2HexString(payloadData));
						#endregion

						base.OnDataReceived(ac, payloadData);

						if (!ac.IsConnected)
						{
							RecyclingSocket(origHandle, remote4Callback, e);
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
					}
					else
						base.ProcessError(e);
				}
				else
				{
					RecyclingSocket(origHandle, remote4Callback, e);
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

		#region Protected Override Method : void ProcessSend(SocketAsyncEventArgs e)
		/// <summary>當完成傳送資料時，將呼叫此函示</summary>
		/// <param name="e">SocketAsyncEventArg associated with the completed send operation.</param>
		protected override void ProcessSend(SocketAsyncEventArgs e)
		{
			int index = Thread.CurrentThread.ManagedThreadId;
			if (e.BytesTransferred > 0)
			{
				if (e.SocketError == SocketError.Success)
				{
					Socket s = null;
					AsyncClient ac = null;
					if (e.UserToken.GetType().Equals(typeof(Socket)))
					{
						s = e.UserToken as Socket;
						ac = m_Clients[s.RemoteEndPoint.ToString()];
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

					base.OnDataSended(ac, buffer);
				}
				else
					base.ProcessError(e);
			}
			else
			{
				_log.Write(LogManager.LogLevel.Debug, "Send Zero Byte Data To Remote");
				base.CloseClientSocket(e);
			}
		}
		#endregion

		#region WebSocketClient Events
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

			base.OnDataSended((WebSocketClient)sender, e.Data, e.ExtraInfo);
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

			base.OnSendedFail((WebSocketClient)sender, e.Data, e.ExtraInfo);
		}
		#endregion

		#region Private Method : void ac_OnClosing(object sender, AsyncClientEventArgs e)
		private void ac_OnClosing(object sender, AsyncClientEventArgs e)
		{
			WebSocketClient ac = (WebSocketClient)sender;
			if (ac != null)
			{
				AsyncClient acc = null;
				m_Clients.TryRemove(ac.RemoteEndPoint.ToString(), out acc);
			}
			base.OnClientClosing(ac, e.ExtraInfo);
		}
		#endregion

		#region Private Method : void ac_OnClosed(object sender, AsyncClientEventArgs e)
		private void ac_OnClosed(object sender, AsyncClientEventArgs e)
		{
			WebSocketClient ac = (WebSocketClient)sender;

			if (ac != null)
			{
				AsyncClient acc = null;
				m_Clients.TryRemove(ac.RemoteEndPoint.ToString(), out acc);
			}
			base.OnClientClosed(ac, e.ClosedByIdle, e.ExtraInfo);
		}
		#endregion
		#endregion

		#region Private Method : WebSocketClient ShakeHands(Socket socket)
		private WebSocketClient ShakeHands(Socket socket)
		{
			// 存放Request資料的Buffer
			byte[] buffer = new byte[BUFFER_SIZE];
			// 接收的Request長度
			int length = socket.Receive(buffer);
			// 將buffer中的資料解碼成字串
			// GET /chat HTTP/1.1
			// Host: server.example.com
			// Upgrade: websocket
			// Connection: Upgrade
			// Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==
			// Origin: http://example.com
			// Sec-WebSocket-Protocol: chat, superchat
			// Sec-WebSocket-Version: 13
			string data = Encoding.UTF8.GetString(buffer, 0, length);
			Debug.Print(data);

			// 將資料字串中的空白位元移除
			string[] dataArray = data.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			string url = dataArray.Where(s => s.StartsWith("GET") && s.EndsWith("HTTP/1.1")).Single().Split(' ')[1];
			if (!m_Services.Exists(s => s.Equals(url, StringComparison.OrdinalIgnoreCase)))
			{
				Debug.Print("Not Allow Service!");
				// Send Back "Bad Request" : Status Code : 400
				socket.Send(Encoding.UTF8.GetBytes("HTTP/1.1 400 Bad Request\r\n\r\n"));

				#region 三秒後強制斷線
				System.Threading.Tasks.Task.Factory.StartNew(o =>
				{
					try
					{
						Socket acc = (Socket)o;
						Thread.Sleep(3000);
						if (acc != null)
						{
							acc.Close();
							acc.Dispose();
							acc = null;
						}
					}
					catch { }
				}, socket);
				#endregion

				return null;
			}
			// 從Client傳來的Request Header訊息中取
			string key = dataArray.Where(s => s.Contains("Sec-WebSocket-Key: ")).Single().Replace("Sec-WebSocket-Key: ", String.Empty).Trim();
			string acceptKey = CreateAcceptKey(key);
			// WebSocket Protocol定義的ShakeHand訊息
			string handShakeMsg =
		 "HTTP/1.1 101 Switching Protocols\r\n" +
		 "Upgrade: websocket\r\n" +
		 "Connection: Upgrade\r\n" +
		 "Sec-WebSocket-Accept: " + acceptKey + "\r\n\r\n";
			socket.Send(Encoding.UTF8.GetBytes(handShakeMsg));

			Debug.Print(handShakeMsg);

			WebSocketClient ac = new WebSocketClient(socket, acceptKey, url);
			ac.AutoClose = base.AutoCloseTime;
			ac.BeforeSend += new EventHandler<AsyncClientEventArgs>(ac_OnBeforeSended);
			ac.DataSended += new EventHandler<AsyncClientEventArgs>(ac_OnDataSended);
			ac.SendFail += new EventHandler<AsyncClientEventArgs>(ac_OnSendedFail);
			ac.Closed += new EventHandler<AsyncClientEventArgs>(ac_OnClosed);
			ac.Closing += new EventHandler<AsyncClientEventArgs>(ac_OnClosing);
			m_Clients.AddOrUpdate(socket.RemoteEndPoint.ToString(), ac, (k, v) =>
			{
				v.Dispose();
				v = null;
				v = ac;
				return v;
			});

			return ac;
		}
		#endregion

		#region Private Method : string CreateAcceptKey(String key)
		private string CreateAcceptKey(String key)
		{
			string keyStr = key + GUID;
			byte[] hashBytes = ComputeHash(keyStr);
			return Convert.ToBase64String(hashBytes);
		}
		#endregion

		#region Private Method : byte[] ComputeHash(String str)
		private byte[] ComputeHash(String str)
		{
			return m_sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(str));
		}
		#endregion
	}
	#endregion
}
