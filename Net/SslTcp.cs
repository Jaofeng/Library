using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using CJF.Utility;

namespace CJF.Net
{
	public sealed class SslClientInfo
	{
		TcpClient _Client = null;
		SslStream _Stream = null;

		public SslClientInfo(TcpClient client)
		{
			_Client = client;
			_Stream = new SslStream(client.GetStream(), false);
		}

		public TcpClient Client { get { return _Client; } }
		public SslStream Stream { get { return _Stream; } }
		public bool IsAuthenticated { get { return _Stream.IsAuthenticated; } }
		public bool Connected { get { return _Client.Connected; } }

		public void Close()
		{
			_Stream.Close();
			_Client.Close();
		}
	}

	public sealed class SslClientInfoCollection : ICollection
	{
		ConcurrentDictionary<EndPoint, SslClientInfo> _Clients = null;

		public SslClientInfoCollection()
		{
			_Clients = new ConcurrentDictionary<EndPoint, SslClientInfo>();
		}

		#region Public Properties
		/// <summary>依使用者端點資訊取得 SSL 連線資訊類別。</summary>
		/// <param name="ep">使用者端點資訊。</param>
		/// <returns>SSL 連線資訊類別</returns>
		public SslClientInfo this[EndPoint ep]
		{
			get
			{
				SslClientInfo ssl = null;
				if (_Clients.TryGetValue(ep, out ssl))
					return ssl;
				else
					return null;
			}
		}
		/// <summary>取得在這個執行個體中所包含的元素數。</summary>
		public int Count { get { return _Clients.Count; } }
		/// <summary>取得值，指出這個執行個體的存取是否為同步 (安全執行緒)。</summary>
		public bool IsSynchronized { get { return true; } }
		/// <summary>取得可用來同步存取這個執行個體的物件。</summary>
		public object SyncRoot { get { return true; } }
		/// <summary>取得這個執行個體的使用者連線端點資訊。</summary>
		public EndPoint[] EndPoints
		{
			get
			{
				EndPoint[] eps = new EndPoint[_Clients.Count];
				CopyTo(eps, 0);
				return eps;
			}
		}
		#endregion

		#region Public Method : SslStream Add(EndPoint ep, TcpClient client)
		/// <summary>新增使用者連線資訊。</summary>
		/// <param name="ep">使用者端點資訊。</param>
		/// <param name="client">用來連線的 System.Net.Sockets.TcpClient 類別。</param>
		/// <returns>回傳依附在 System.Net.Sockets.TcpClient 的 System.Net.Security.SslStream 資料加密串流類別。</returns>
		public SslStream Add(EndPoint ep, TcpClient client)
		{
			SslClientInfo ssl = new SslClientInfo(client);
			_Clients.TryAdd(ep, ssl);
			return ssl.Stream;
		}
		#endregion

		#region Public Method : void Remove(EndPoint ep)
		/// <summary>移除使用者端點資料。</summary>
		/// <param name="ep">欲移除的端點資訊。</param>
		public void Remove(EndPoint ep)
		{
			SslClientInfo client = null;
			_Clients.TryRemove(ep, out client);
		}
		#endregion

		#region Public Method : bool ContainsKey(EndPoint ep)
		/// <summary>判斷這個執行個體是否包含指定的索引鍵(使用者端點資訊)。</summary>
		/// <param name="ep">要在這個執行個體中尋找的索引鍵。</param>
		/// <returns>如果這個執行個體包含具有指定索引鍵的項目則為 true，否則為 false。</returns>
		public bool ContainsKey(EndPoint ep)
		{
			return _Clients.ContainsKey(ep);
		}
		#endregion

		#region Public Method : void CopyTo(Array array, int index)
		/// <summary>從特定的 System.Array 索引開始，複製這個執行個體的使用者端點資訊至 System.Array。</summary>
		/// <param name="array">一維 System.Array，是從這個執行個體複製過來的使用者端點資訊之目的端。System.Array 必須有以零起始的索引。</param>
		/// <param name="index">array 中以零起始的索引，是開始複製的位置。</param>
		/// <exception cref="System.ArgumentNullException">array 為 null。</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">index 小於零。</exception>
		/// <exception cref="System.ArgumentException">array 是多維的。-或-這個執行個體元素的數量大於從 index 到目的 array 結尾的可用空間。</exception>
		public void CopyTo(Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException();
			else if (index < 0)
				throw new ArgumentOutOfRangeException();
			else if (array.Rank != 1 || index + array.Length < _Clients.Count)
				throw new ArgumentException();
			EndPoint[] eps = new EndPoint[array.Length];
			_Clients.Keys.CopyTo(eps, index);
			eps.CopyTo(array, index);
		}
		#endregion

		#region Public Method : void CopyTo(EndPoint[] array, int index)
		/// <summary>從特定的索引開始，複製這個執行個體的使用者端點資訊至 System.Net.EndPoint[]。</summary>
		/// <param name="array">一維 System.Net.EndPoint[]，是從這個執行個體複製過來的使用者端點資訊之目的端。</param>
		/// <param name="index">array 中以零起始的索引，是開始複製的位置。</param>
		/// <exception cref="System.ArgumentNullException">array 為 null。</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">index 小於零。</exception>
		/// <exception cref="System.ArgumentException">這個執行個體元素的數量大於從 index 到目的 array 結尾的可用空間。</exception>
		public void CopyTo(EndPoint[] array, int index)
		{
			if (array == null)
				throw new ArgumentNullException();
			else if (index < 0)
				throw new ArgumentOutOfRangeException();
			else if (index + array.Length < _Clients.Count)
				throw new ArgumentException();
			_Clients.Keys.CopyTo(array, index);
		}
		#endregion

		#region Public Method : void Clean()
		/// <summary>將這個執行個體的所有元素移除。</summary>
		public void Clean()
		{
			_Clients.Clear();
		}
		#endregion

		#region Public Method : IEnumerator GetEnumerator()
		/// <summary>傳回會逐一查看集合的列舉程式。</summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return _Clients.GetEnumerator();
		}
		#endregion
	}


	#region Public Class : SslTcpServer
	// From : Microsoft
	// https://msdn.microsoft.com/zh-tw/library/system.net.security.sslstream(v=vs.110).aspx
	/// <summary>SSL TCP Server</summary>
	public class SslTcpServer : IDisposable
	{
		#region Public Events
		/// <summary>當伺服器啟動時觸發。</summary>
		public event EventHandler Started;
		/// <summary>當伺服器關閉時觸發。</summary>
		public event EventHandler Shutdowned;
		/// <summary>當用戶端連線時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> ClientConnected;
		/// <summary>當用戶端已關閉連線時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> ClientClosed;
		/// <summary>當收到用戶端傳送資料過來時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> DataReceived;
		/// <summary>當資料成功傳送給用戶端時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> DataSended;
		/// <summary>當資料傳送給用戶端失敗時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> DataSendFail;
		/// <summary>當用戶端認證失敗時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> AuthenticateFail;
		#endregion

		#region Private Variables
		LogManager _log = new LogManager(typeof(SslTcpServer));
		X509Certificate _ServerCertificate = null;
		bool _IsDisposed = false;
		bool _IsExit = false;
		TcpListener _Server = null;
		SslClientInfoCollection _Clients = null;
		#endregion

		#region Construct Method : SslTcpServer(...)
		/// <summary>[保護]建立新的 CJF.Net.SslTcpServer 執行個體。</summary>
		protected SslTcpServer() { }
		/// <summary>建立新的 CJF.Net.SslTcpServer 執行個體。</summary>
		/// <param name="ipAddr">欲建立的所在 IP 位址。</param>
		/// <param name="port">欲開啟的通訊埠號。</param>
		/// <param name="certificate">憑證檔名，包含完整路徑。</param>
		/// <exception cref="System.IO.FileNotFoundException">找不到 certificate 憑證檔案。</exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">此憑證發生錯誤、無效或密碼不正確。</exception>
		public SslTcpServer(string ipAddr, int port, string certificate) : this(IPAddress.Parse(ipAddr), port, certificate) { }
		/// <summary>建立新的 CJF.Net.SslTcpServer 執行個體。</summary>
		/// <param name="ipAddr">欲建立的所在 IP 位址。</param>
		/// <param name="port">欲開啟的通訊埠號。</param>
		/// <param name="certificate">憑證檔名，包含完整路徑。</param>
		/// <param name="password">此憑證的密碼。</param>
		/// <exception cref="System.IO.FileNotFoundException">找不到 certificate 憑證檔案。</exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">此憑證發生錯誤、無效或密碼不正確。</exception>
		public SslTcpServer(string ipAddr, int port, string certificate, string password) : this(IPAddress.Parse(ipAddr), port, certificate, password) { }

		/// <summary>建立新的 CJF.Net.SslTcpServer 執行個體。 </summary>
		/// <param name="addr">欲建立的所在 IP 位址資訊。</param>
		/// <param name="port">欲開啟的通訊埠號。</param>
		/// <param name="certificate">憑證檔名，包含完整路徑。</param>
		/// <exception cref="System.IO.FileNotFoundException">找不到 certificate 憑證檔案。</exception>
		/// <exception cref="System.IO.FileNotFoundException">找不到 certificate 憑證檔案。</exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">此憑證發生錯誤、無效或密碼不正確。</exception>
		public SslTcpServer(IPAddress addr, int port, string certificate) : this(addr, port, certificate, null) { }

		/// <summary>建立新的 CJF.Net.SslTcpServer 執行個體。 </summary>
		/// <param name="addr">欲建立的所在 IP 位址資訊。</param>
		/// <param name="port">欲開啟的通訊埠號。</param>
		/// <param name="certificate">憑證檔名，包含完整路徑。</param>
		/// <param name="password">此憑證的密碼。</param>
		/// <exception cref="System.IO.FileNotFoundException">找不到 certificate 憑證檔案。</exception>
		/// <exception cref="System.Security.Cryptography.CryptographicException">此憑證發生錯誤、無效或密碼不正確。</exception>
		public SslTcpServer(IPAddress addr, int port, string certificate, string password)
		{
			if (!File.Exists(certificate))
				throw new FileNotFoundException();
			if (string.IsNullOrEmpty(password))
				_ServerCertificate = X509Certificate.CreateFromCertFile(certificate);
			else
				_ServerCertificate = new X509Certificate(certificate, password);
			this.LocalEndPoint = new IPEndPoint(addr, port);
			_Server = new TcpListener(this.LocalEndPoint);
			_Clients = new SslClientInfoCollection();
			this.IdleTime = 5000;
		}
		/// <summary></summary>
		~SslTcpServer() { Dispose(false); }
		#endregion

		#region Public Properties
		/// <summary>取得執行個體開啟的本地通訊位址端點資訊。</summary>
		public IPEndPoint LocalEndPoint { get; private set; }
		/// <summary>取得用戶端連線數。</summary>
		public int Connection { get { return _Clients.Count; } }
		/// <summary>取得遠端使用者連線資訊。</summary>
		/// <param name="ep">遠端使用者端點資訊。</param>
		/// <returns>CJF.Net.SslClient 類別。</returns>
		public SslClientInfo this[EndPoint ep] { get { return _Clients[ep]; } }
		/// <summary>設定或取得連線閒置時間，如超過此時間將自動斷線。單位豪秒。</summary>
		public int IdleTime { get; set; }
		#endregion

		#region Public Method : void Start()
		/// <summary>啟動伺服器。</summary>
		public void Start()
		{
			// Create a TCP/IP (IPv4) socket and listen for incoming connections.
			_Server.Start();
			Task.Factory.StartNew(() =>
			{
				OnStarted(new EventArgs());
				while (!_IsExit)
				{
					Console.WriteLine("Waiting for a client to connect...");
					TcpClient client = null;
					try
					{
						client = _Server.AcceptTcpClient();
						if (client != null)
							Task.Factory.StartNew(() => ProcessClient(client));
					}
					catch (SocketException ex)
					{
						if (ex.ErrorCode == 10004)
							break;
					}
				}
				OnShutdowned(new EventArgs());
			});
		}
		#endregion

		#region Public Method : void Shutdown()
		/// <summary>關閉伺服器</summary>
		public void Shutdown()
		{
			_IsExit = true;
			_Server.Stop();
		}
		#endregion

		#region Public Method : void CloseClient(EndPoint ep)
		/// <summary>關閉遠端使用者連線。</summary>
		/// <param name="ep">欲關閉的使用者連線端點資訊。</param>
		public void CloseClient(EndPoint ep)
		{
			if (!_Clients.ContainsKey(ep))
				return;
			if (_Clients[ep].Client != null && _Clients[ep].Connected)
				_Clients[ep].Close();
			if (!_Clients[ep].Connected)
				OnClientClosed(ep);

		}
		#endregion

		#region Protected Virtual Method : void OnStarted(EventArgs e)
		/// <summary>產生 Started 事件。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		protected virtual void OnStarted(EventArgs e)
		{
			if (this.Started != null)
			{
				foreach (EventHandler del in this.Started.GetInvocationList())
				{
					try { del.BeginInvoke(this, e, null, null); }
					catch (Exception ex) { _log.WriteException(ex); }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnShutdowned(EventArgs e)
		/// <summary>產生 Shutdowned 事件。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		protected virtual void OnShutdowned(EventArgs e)
		{
			if (this.Shutdowned != null)
			{
				foreach (EventHandler del in this.Shutdowned.GetInvocationList())
				{
					try { del.BeginInvoke(this, e, null, null); }
					catch (Exception ex) { _log.WriteException(ex); }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnClientConnected(EndPoint ep)
		/// <summary>產生 ClientConnected 事件</summary>
		/// <param name="ep">已連線的 System.Net.EndPoint 類別。</param>
		protected virtual void OnClientConnected(EndPoint ep)
		{
			if (this.ClientConnected != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep);
				foreach (EventHandler<SslTcpEventArgs> del in this.ClientConnected.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch (Exception ex) { _log.WriteException(ex); }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnClientClosed(EndPoint ep)
		/// <summary>產生 ClientClosed 事件</summary>
		/// <param name="ep">已斷線的 System.Net.EndPoint 類別。</param>
		protected virtual void OnClientClosed(EndPoint ep)
		{
			if (this.ClientClosed != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep);
				foreach (EventHandler<SslTcpEventArgs> del in this.ClientClosed.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch (Exception ex) { _log.WriteException(ex); }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnDataReceived(EndPoint ep, byte[] data)
		/// <summary>產生 DataReceived 事件</summary>
		/// <param name="ep">遠端使用者的 System.Net.EndPoint 類別。</param>
		/// <param name="data">已接收的資料內容。</param>
		protected virtual void OnDataReceived(EndPoint ep, byte[] data)
		{
			if (this.DataReceived != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep, data);
				foreach (EventHandler<SslTcpEventArgs> del in this.DataReceived.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch (Exception ex) { _log.WriteException(ex); }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnDataSended(EndPoint ep, byte[] data)
		/// <summary>產生 DataSended 事件</summary>
		/// <param name="ep">遠端使用者的 System.Net.EndPoint 類別。</param>
		/// <param name="data">已發送的資料內容。</param>
		protected virtual void OnDataSended(EndPoint ep, byte[] data)
		{
			if (this.DataSended != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep, data);
				foreach (EventHandler<SslTcpEventArgs> del in this.DataSended.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch (Exception ex) { _log.WriteException(ex); }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnDataSendFail(EndPoint ep, byte[] data)
		/// <summary>產生 DataSendFail 事件</summary>
		/// <param name="ep">遠端使用者的 System.Net.EndPoint 類別。</param>
		/// <param name="data">發送失敗的資料內容。</param>
		protected virtual void OnDataSendFail(EndPoint ep, byte[] data)
		{
			if (this.DataSendFail != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep, data);
				foreach (EventHandler<SslTcpEventArgs> del in this.DataSendFail.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch (Exception ex) { _log.WriteException(ex); }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnAuthenticateFail(EndPoint ep)
		/// <summary>產生 AuthenticateFail 事件</summary>
		/// <param name="ep">已連線的 System.Net.EndPoint 類別。</param>
		protected virtual void OnAuthenticateFail(EndPoint ep)
		{
			if (this.AuthenticateFail != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep);
				foreach (EventHandler<SslTcpEventArgs> del in this.AuthenticateFail.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch (Exception ex) { _log.WriteException(ex); }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void ProcessClient(TcpClient client)
		/// <summary>處理使用者端連線程序。</summary>
		/// <param name="client">使用者連線類別(System.Net.Sockets.TcpClient)。</param>
		protected virtual void ProcessClient(TcpClient client)
		{
			EndPoint remote = client.Client.RemoteEndPoint;
			SslStream sslStream = _Clients.Add(remote, client);

			try
			{
				sslStream.AuthenticateAsServer(_ServerCertificate, false, SslProtocols.Tls, true);
				OnClientConnected(remote);
				if (this.IdleTime != 0)
				{
					sslStream.ReadTimeout = this.IdleTime;
					sslStream.WriteTimeout = this.IdleTime;
				}
				byte[] buffer = new byte[2048];
				int bytes = -1;
				DateTime dt = DateTime.Now;
				while (!_IsExit && client.Connected)
				{
					bytes = sslStream.Read(buffer, 0, buffer.Length);
					if (bytes != 0)
					{
						byte[] buf = new byte[bytes];
						Array.Copy(buffer, buf, bytes);
						dt = DateTime.Now;
						OnDataReceived(remote, buf);
					}
					else if (this.IdleTime != 0 && DateTime.Now.Subtract(dt).TotalMilliseconds >= this.IdleTime)
						break;
					Thread.Sleep(10);
				}
			}
			catch (AuthenticationException e)
			{
				_log.WriteException(e);
				if (sslStream != null)
					sslStream.Close();
				if (client != null && client.Connected)
					client.Close();
				OnAuthenticateFail(remote);
				return;
			}
			catch (IOException) { }
			catch (Exception ex)
			{
				_log.WriteException(ex);
			}
			finally
			{
				_Clients.Remove(remote);
				if (sslStream != null)
					sslStream.Close();
				if (client != null && client.Connected)
					client.Close();
				if (!client.Connected)
					OnClientClosed(remote);
			}
		}
		#endregion

		#region Public Virtual Method : void SendData(EndPoint ep, byte[] data)
		/// <summary>傳送資料給遠端使用者。</summary>
		/// <param name="ep">遠端使用者的連線端點資訊。</param>
		/// <param name="data">資料內容。</param>
		/// <exception cref="System.ArgumentException">找不到使用者端點，或使用者已經斷線。</exception>
		/// <exception cref="System.ArgumentNullException">資料內容不得為空陣列或 null。</exception>
		public virtual void SendData(EndPoint ep, byte[] data)
		{
			if (!_Clients.ContainsKey(ep))
				throw new ArgumentException("Client not found!");
			if (data == null || data.Length == 0)
				throw new ArgumentNullException("data is empty!");
			_Clients[ep].Stream.BeginWrite(data, 0, data.Length, new AsyncCallback((result) =>
				{
					_Clients[ep].Stream.EndWrite(result);
					if (result.IsCompleted)
						OnDataSended(ep, data);
					else
						OnDataSendFail(ep, data);

				}), null);
		}
		#endregion

		#region Public Virtual Method : EndPoint[] GetAllPoints()
		/// <summary>取得這個執行個體的使用者連線端點資訊。</summary>
		/// <returns>使用者連線端點資訊。</returns>
		public virtual EndPoint[] GetAllPoints()
		{
			return _Clients.EndPoints;
		}
		#endregion

		#region Delegate Callback Methods
		private void DataSendedEventCallback(IAsyncResult result)
		{
			object[] state = result.AsyncState as object[];
			EndPoint ep = state[0] as EndPoint;
			byte[] data = state[1] as byte[];
			bool completed = result.IsCompleted;
			_Clients[ep].Stream.EndWrite(result);
			if (result.IsCompleted)
				OnDataSended(ep, data);
			else
				OnDataSendFail(ep, data);
		}
		#endregion

		#region IDisposable 成員
		/// <summary>清除並釋放 HttpServiceBase 所使用的資源。</summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>[覆寫] 清除並釋放 HttpServiceBase 所使用的資源。</summary>
		/// <param name="disposing">是否確實清除</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_IsDisposed) return;
			if (disposing)
			{
				EndPoint[] eps = _Clients.EndPoints;
				foreach (EndPoint ep in eps)
				{
					_Clients[ep].Close();
					_Clients.Remove(ep);
				}
				_Clients = null;
				try { _Server.Stop(); }
				catch { }
				_Server = null;
			}
			_IsDisposed = true;
		}
		#endregion
	}
	#endregion



	// From : Microsoft
	// https://msdn.microsoft.com/zh-tw/library/system.net.security.sslstream(v=vs.110).aspx
	/// <summary>SSL TCP Client</summary>
	public class SslTcpClient
	{
		//private static Hashtable certificateErrors = new Hashtable();

		#region Pbublic Events
		/// <summary>當連線至伺服器時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> Connected;
		/// <summary>當已關閉與伺服器連線時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> Closed;
		/// <summary>當收到伺服器傳送資料過來時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> DataReceived;
		/// <summary>當資料成功傳送給伺服器時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> DataSended;
		/// <summary>當資料傳送給伺服器失敗時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> DataSendFail;
		/// <summary>當認證失敗時觸發。</summary>
		public event EventHandler<SslTcpEventArgs> AuthenticateFail;
		#endregion

		#region Public Static Method : bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		/// <summary>驗證伺服器的 SSL 憑證</summary>
		/// <param name="sender"></param>
		/// <param name="certificate"></param>
		/// <param name="chain"></param>
		/// <param name="sslPolicyErrors"></param>
		/// <returns></returns>
		public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors == SslPolicyErrors.None)
				return true;
			else if (!string.IsNullOrEmpty(BypassHostName) && certificate.Issuer.Equals(BypassHostName))
				return true;
			return false;
		}
		#endregion

		TcpClient _Client = null;
		SslStream _Stream = null;
		bool _IsExit = false;

		public SslTcpClient(IPEndPoint ipp)
		{
			this.RemoteEndPoint = ipp;
		}
		public SslTcpClient(IPAddress addr, int port)
		{
			this.RemoteEndPoint = new IPEndPoint(addr, port);
		}

		/// <summary>取得執行個體開啟的遠端伺服器通訊位址端點資訊。</summary>
		public IPEndPoint RemoteEndPoint { get; private set; }
		/// <summary>設定或取得憑證名稱。</summary>
		public string CertificateName { get; set; }

		public static string BypassHostName { get; set; }

		#region Public Static Method : SslTcpClient ConnectTo(IPEndPoint ipp, string certificate)
		/// <summary>使用憑證檔連線至遠端伺服器。</summary>
		/// <param name="ipp">遠端伺服器端點資訊。</param>
		/// <param name="certificate">憑證檔案。</param>
		/// <returns>已連線的 CJF.Net.SslTcpClient 執行個體。</returns>
		public static SslTcpClient ConnectTo(IPEndPoint ipp, string certificate)
		{
			SslTcpClient client = new SslTcpClient(ipp);
			client.Connect(certificate);
			return client;
		}
		#endregion

		#region Public Method : void Connect(string certificate)
		/// <summary>使用憑證連線至遠端伺服器。</summary>
		/// <param name="certificate">憑證檔案。</param>
		public void Connect(string certificate)
		{
			if (_Client == null)
				_Client = new TcpClient();
			_IsExit = false;
			_Client.BeginConnect(this.RemoteEndPoint.Address, this.RemoteEndPoint.Port, new AsyncCallback((result) =>
				{
					_Client.EndConnect(result);
					_Stream = new SslStream(_Client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

					X509CertificateCollection certs = new X509CertificateCollection();
					X509Certificate cert = X509Certificate.CreateFromCertFile(certificate);
					certs.Add(cert);

					try
					{
						_Stream.AuthenticateAsClient(cert.Issuer, certs, SslProtocols.Tls, true);
					}
					catch (AuthenticationException e)
					{
						if (e.InnerException != null)
							Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
						Console.WriteLine("Authentication failed - closing the connection.");
						_Stream.Close();
						_Client.Close();
						_Client = null;
						OnAuthenticateFail(this.RemoteEndPoint);
						return;
					}
					OnConnected(this.RemoteEndPoint);
					Task.Factory.StartNew(() =>
					{
						try
						{
							byte[] buffer = new byte[2048];
							int bytes = -1;
							DateTime dt = DateTime.Now;
							while (!_IsExit && _Client.Connected)
							{
								bytes = _Stream.Read(buffer, 0, buffer.Length);
								if (bytes != 0)
								{
									byte[] buf = new byte[bytes];
									Array.Copy(buffer, buf, bytes);
									dt = DateTime.Now;
									OnDataReceived(this.RemoteEndPoint, buf);
								}
								Thread.Sleep(10);
							}
						}
						catch { }
						if (!_Client.Connected)
							_Client.Close();
						OnClosed(this.RemoteEndPoint);
					});

				}), null);
		}
		#endregion

		#region Public Method : void Close()
		/// <summary>與遠端伺服器關閉連線。</summary>
		public void Close()
		{
			_IsExit = true;
			if (_Client != null && !_Client.Connected)
			{
				_Client.Close();
				_Client = null;
				OnClosed(this.RemoteEndPoint);
			}
		}
		#endregion

		#region Protected Virtual Method : void OnConnected(EndPoint ep)
		/// <summary>產生 Connected 事件</summary>
		/// <param name="ep">已連線的 System.Net.EndPoint 類別。</param>
		protected virtual void OnConnected(EndPoint ep)
		{
			if (this.Connected != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep);
				foreach (EventHandler<SslTcpEventArgs> del in this.Connected.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch { }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnClosed(EndPoint ep)
		/// <summary>產生 Closed 事件</summary>
		/// <param name="ep">已斷線的 System.Net.EndPoint 類別。</param>
		protected virtual void OnClosed(EndPoint ep)
		{
			if (this.Closed != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep);
				foreach (EventHandler<SslTcpEventArgs> del in this.Closed.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch { }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnDataReceived(EndPoint ep, byte[] data)
		/// <summary>產生 DataReceived 事件</summary>
		/// <param name="ep">遠端使用者的 System.Net.EndPoint 類別。</param>
		/// <param name="data">已接收的資料內容。</param>
		protected virtual void OnDataReceived(EndPoint ep, byte[] data)
		{
			if (this.DataReceived != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep, data);
				foreach (EventHandler<SslTcpEventArgs> del in this.DataReceived.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch { }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnDataSended(EndPoint ep, byte[] data)
		/// <summary>產生 DataSended 事件</summary>
		/// <param name="ep">遠端使用者的 System.Net.EndPoint 類別。</param>
		/// <param name="data">已發送的資料內容。</param>
		protected virtual void OnDataSended(EndPoint ep, byte[] data)
		{
			if (this.DataSended != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep, data);
				foreach (EventHandler<SslTcpEventArgs> del in this.DataSended.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch { }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnDataSendFail(EndPoint ep, byte[] data)
		/// <summary>產生 DataSendFail 事件</summary>
		/// <param name="ep">遠端使用者的 System.Net.EndPoint 類別。</param>
		/// <param name="data">發送失敗的資料內容。</param>
		protected virtual void OnDataSendFail(EndPoint ep, byte[] data)
		{
			if (this.DataSendFail != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep, data);
				foreach (EventHandler<SslTcpEventArgs> del in this.DataSendFail.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch { }
				}
			}
		}
		#endregion

		#region Protected Virtual Method : void OnAuthenticateFail(EndPoint ep)
		/// <summary>產生 AuthenticateFail 事件</summary>
		/// <param name="ep">已連線的 System.Net.EndPoint 類別。</param>
		protected virtual void OnAuthenticateFail(EndPoint ep)
		{
			if (this.AuthenticateFail != null)
			{
				SslTcpEventArgs ea = new SslTcpEventArgs(ep);
				foreach (EventHandler<SslTcpEventArgs> del in this.AuthenticateFail.GetInvocationList())
				{
					try { del.BeginInvoke(this, ea, null, null); }
					catch { }
				}
			}
		}
		#endregion

		#region Public Virtual Method : void SendData(byte[] data)
		/// <summary>傳送資料給遠端使用者。</summary>
		/// <param name="data">資料內容。</param>
		/// <exception cref="System.ArgumentException">找不到使用者端點，或使用者已經斷線。</exception>
		/// <exception cref="System.ArgumentNullException">資料內容不得為空陣列或 null。</exception>
		public virtual void SendData(byte[] data)
		{
			if (!_Client.Connected)
				throw new SocketException();
			if (data == null || data.Length == 0)
				throw new ArgumentNullException("data is empty!");
			_Stream.BeginWrite(data, 0, data.Length, new AsyncCallback((result) =>
				{
					byte[] tmp = result.AsyncState as byte[];
					_Stream.EndWrite(result);
					if (result.IsCompleted)
						OnDataSended(this.RemoteEndPoint, tmp);
					else
						OnDataSendFail(this.RemoteEndPoint, tmp);
				}), data);
		}
		#endregion
	}
}
