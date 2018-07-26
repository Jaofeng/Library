using CJF.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace CJF.Net.Ssl
{
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
        bool _IsDisposed = false;
        bool _IsExit = false;
        TcpListener _Server = null;
        SslClientInfoCollection _Clients = null;
        readonly X509Certificate _ServerCertificate = null;
        #endregion

        #region Construct Method : SslTcpServer(...)
        /// <summary>[保護]建立新的 CJF.Net.SslTcpServer 執行個體。</summary>
        protected SslTcpServer() { }
        /// <summary>建立新的 CJF.Net.SslTcpServer 執行個體。</summary>
        /// <param name="ipAddr">欲建立的所在 IP 位址。</param>
        /// <param name="port">欲開啟的通訊埠號。</param>
        /// <param name="certificate">憑證檔名，包含完整路徑。副檔名應為 *.pxf。</param>
        /// <exception cref="FileNotFoundException">找不到 certificate 憑證檔案。</exception>
        /// <exception cref="NotImplementedException">伺服器端未安裝該憑證。</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">此憑證發生錯誤、無效或密碼不正確。</exception>
        public SslTcpServer(string ipAddr, int port, string certificate) : this(IPAddress.Parse(ipAddr), port, certificate) { }
        /// <summary>建立新的 CJF.Net.SslTcpServer 執行個體。</summary>
        /// <param name="ipAddr">欲建立的所在 IP 位址。</param>
        /// <param name="port">欲開啟的通訊埠號。</param>
        /// <param name="certificate">憑證檔名，包含完整路徑。副檔名應為 *.pxf。</param>
        /// <param name="password">此憑證的密碼。</param>
        /// <exception cref="FileNotFoundException">找不到 certificate 憑證檔案。</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">此憑證發生錯誤、無效或密碼不正確。</exception>
        public SslTcpServer(string ipAddr, int port, string certificate, string password) : this(IPAddress.Parse(ipAddr), port, certificate, password) { }

        /// <summary>建立新的 CJF.Net.SslTcpServer 執行個體。 </summary>
        /// <param name="addr">欲建立的所在 IP 位址資訊。</param>
        /// <param name="port">欲開啟的通訊埠號。</param>
        /// <param name="certificate">憑證檔名，包含完整路徑。副檔名應為 *.pxf。</param>
        /// <exception cref="FileNotFoundException">找不到 certificate 憑證檔案。</exception>
        /// <exception cref="System.Security.Cryptography.CryptographicException">此憑證發生錯誤、無效或密碼不正確。</exception>
        public SslTcpServer(IPAddress addr, int port, string certificate) : this(addr, port, certificate, null) { }

        /// <summary>建立新的 CJF.Net.SslTcpServer 執行個體。 </summary>
        /// <param name="addr">欲建立的所在 IP 位址資訊。</param>
        /// <param name="port">欲開啟的通訊埠號。</param>
        /// <param name="certificate">憑證檔名，包含完整路徑。副檔名應為 *.pxf。</param>
        /// <param name="password">此憑證的密碼。</param>
        /// <exception cref="FileNotFoundException">找不到 certificate 憑證檔案。</exception>
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
        public IPEndPoint LocalEndPoint { get; private set; } = null;
        /// <summary>取得用戶端連線數。</summary>
        public int Connection { get => _Clients.Count; }
        /// <summary>取得遠端使用者連線資訊。</summary>
        /// <param name="ep">遠端使用者端點資訊。</param>
        /// <returns>CJF.Net.SslClient 類別。</returns>
        public SslClientInfo this[EndPoint ep] { get => _Clients[ep]; }
        /// <summary>設定或取得連線閒置時間，如超過此時間將自動斷線。單位豪秒。</summary>
        public int IdleTime { get; set; } = 0;
        /// <summary>取得這個執行個體的使用者連線端點資訊。</summary>
        public IEnumerable<EndPoint> EndPoints { get => _Clients.EndPoints; }
        /// <summary>取得這個執行個體的使用者連線資訊。</summary>
        public IEnumerable<SslClientInfo> Clients { get => _Clients.Clients; }
        /// <summary>設定或取得是否僅檢查有效憑證。</summary>
        public bool CertificateValid { get; set; } = true;
        /// <summary>設定或取得憑證存放區域。預設為 "My"(個人)。</summary>
        public StoreName CertificateStoreName { get; set; } = StoreName.My;
        #endregion

        #region Public Method : void Start()
        /// <summary>啟動伺服器。</summary>
        /// <exception cref="NotImplementedException">伺服器端未安裝該憑證。</exception>
        /// <exception cref="NotSupportedException">此憑證驗證有問題。</exception>
        public void Start()
        {
            #region 檢查憑證是否已匯入本機，並檢查是否為信任憑證
            string subName = System.Text.RegularExpressions.Regex.Replace(_ServerCertificate.Subject, "CN=", "");

            X509Store store = new X509Store(CertificateStoreName, StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection x2c = store.Certificates.Find(X509FindType.FindBySubjectName, subName, false);
            if (x2c.Count == 0)
            {
                store.Close();
                store = new X509Store(CertificateStoreName, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                x2c = store.Certificates.Find(X509FindType.FindBySubjectName, subName, false);
                if (x2c.Count == 0)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    if (CertificateValid && !x2c[0].Verify())
                        throw new NotSupportedException();
                }
            }
            else
            {
                if (CertificateValid && !x2c[0].Verify())
                    throw new NotSupportedException();
            }
            store.Close();
            #endregion

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
            SetSocketOptions(client);
            EndPoint remote = client.Client.RemoteEndPoint;
            SslClientInfo sci = _Clients.Add(remote, client);
            SslStream sslStream = sci.Stream;

            try
            {
                OnClientConnected(remote);
                sslStream.AuthenticateAsServer(_ServerCertificate, false, SslProtocols.Tls, true);
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
                OnAuthenticateFail(remote);
                if (sslStream != null)
                    sslStream.Close();
                if (client != null && client.Connected)
                    client.Close();
                return;
            }
            catch (IOException) { }
            catch (Exception ex)
            {
                _log.WriteException(ex);
                Console.WriteLine($"{this.GetType()} - {ex.GetType()}\n{ex.Message}");
            }
            finally
            {
                if (_Clients != null)
                    _Clients.Remove(remote);
                if (sslStream != null)
                    sslStream.Close();
                if (client != null && client.Connected)
                    client.Close();
                if (client != null && !client.Connected)
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

        #region Public Virtual Method : IEnumerable<EndPoint> GetAllPoints()
        /// <summary>取得這個執行個體的使用者連線端點資訊。</summary>
        /// <returns>使用者連線端點資訊。</returns>
        public virtual IEnumerable<EndPoint> GetAllPoints()
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
                foreach (EndPoint ep in _Clients.EndPoints)
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

        #region Private Method : void SetSocketOptions(TcpClient client)
        private void SetSocketOptions(TcpClient client)
        {
            if (client == null)
                throw new ArgumentNullException();
            client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            byte[] inOptionValues = new byte[4 * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)60000).CopyTo(inOptionValues, 4);   // 空閒 60 秒
            BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, 8);    // 每 5 秒檢查一次
            client.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
        #endregion
    }
    #endregion
}
