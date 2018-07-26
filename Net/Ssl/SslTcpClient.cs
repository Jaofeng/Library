using System;
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
    #region Public Class : SslTcpClient
    // From : Microsoft
    // https://msdn.microsoft.com/zh-tw/library/system.net.security.sslstream(v=vs.110).aspx
    /// <summary>SSL TCP Client</summary>
    public class SslTcpClient
    {
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
            else if (!string.IsNullOrEmpty(BypassIssuer) && certificate.Issuer.Equals(BypassIssuer))
                return true;
            return false;
        }
        #endregion

        TcpClient _Client = null;
        SslStream _Stream = null;
        bool _IsExit = false;
        X509CertificateCollection _X509Certs = null;

        #region Construct Methods : SslTcpClient(...)
        /// <summary>建立新的 CJF.Net.SslTcpClient 執行個體。</summary>
        /// <param name="ipp">遠端伺服器連線端點。</param>
        /// <param name="certificate">憑証檔所在位置，包含完整路徑。副檔名應為 *.cer。</param>
        /// <exception cref="ArgumentNullException">certificate 不得為 null 或空值。</exception>
        /// <exception cref="FileNotFoundException">找不到 certificate 憑證檔。</exception>
        public SslTcpClient(IPEndPoint ipp, string certificate)
        {
            if (string.IsNullOrEmpty(certificate))
                throw new ArgumentNullException();
            else if (!File.Exists(certificate))
                throw new FileNotFoundException();
            this.RemoteEndPoint = ipp;
            _X509Certs = new X509CertificateCollection();
            X509Certificate cert = X509Certificate.CreateFromCertFile(certificate);
            _X509Certs.Add(cert);
            this.CertIssuer = cert.Issuer;
        }
        /// <summary>建立新的 CJF.Net.SslTcpClient 執行個體。</summary>
        /// <param name="addr">遠端伺服器連線IP位址。</param>
        /// <param name="port">遠端伺服器連線通訊埠號。</param>
        /// <param name="certificate">憑証檔所在位置，包含完整路徑。副檔名應為 *.cer。</param>
        public SslTcpClient(IPAddress addr, int port, string certificate) : this(new IPEndPoint(addr, port), certificate) { }
        /// <summary>建立新的 CJF.Net.SslTcpClient 執行個體。</summary>
        /// <param name="ipp">遠端伺服器連線端點。</param>
        /// <param name="authData">位元組陣列，包含來自 X.509 憑證的資料。</param>
        private SslTcpClient(IPEndPoint ipp, byte[] authData) : this(ipp, authData, null) { }
        /// <summary>建立新的 CJF.Net.SslTcpClient 執行個體。</summary>
        /// <param name="ipp">遠端伺服器連線端點。</param>
        /// <param name="authData">位元組陣列，包含來自 X.509 憑證的資料。</param>
        /// <param name="password">存取 X.509 憑證資料所需的密碼。</param>
        private SslTcpClient(IPEndPoint ipp, byte[] authData, string password)
        {
            this.RemoteEndPoint = ipp;
            _X509Certs = new X509CertificateCollection();
            X509Certificate cert = null;
            if (string.IsNullOrEmpty(password))
                cert = new X509Certificate(authData);
            else
                cert = new X509Certificate(authData, password);
            _X509Certs.Add(cert);
            this.CertIssuer = cert.Issuer;
        }
        #endregion

        #region Public Prpperties
        /// <summary>取得執行個體開啟的遠端伺服器通訊位址端點資訊。</summary>
        public IPEndPoint RemoteEndPoint { get; private set; } = null;
        /// <summary>設定或取得憑證授權單位名稱。</summary>
        public string CertIssuer { get; private set; } = null;
        /// <summary>設定或取得忽略的憑證授權單位名稱。</summary>
        public static string BypassIssuer { get; set; } = null;
        #endregion

        #region Public Static Method : SslTcpClient ConnectTo(IPEndPoint ipp, string certificate)
        /// <summary>使用憑證檔連線至遠端伺服器。</summary>
        /// <param name="ipp">遠端伺服器端點資訊。</param>
        /// <param name="certificate">憑證檔案。</param>
        /// <returns>已連線的 CJF.Net.SslTcpClient 執行個體。</returns>
        public static SslTcpClient ConnectTo(IPEndPoint ipp, string certificate)
        {
            SslTcpClient client = new SslTcpClient(ipp, certificate);
            client.Connect();
            return client;
        }
        #endregion

        #region Public Method : void Connect()
        /// <summary>使用憑證連線至遠端伺服器。</summary>
        public void Connect()
        {
            if (_Client == null)
                _Client = new TcpClient();
            _IsExit = false;
            _Client.BeginConnect(this.RemoteEndPoint.Address, this.RemoteEndPoint.Port, new AsyncCallback((result) =>
                {
                    _Client.EndConnect(result);
                    _Stream = new SslStream(_Client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                    try
                    {
                        _Stream.AuthenticateAsClient(CertIssuer, _X509Certs, SslProtocols.Tls, true);
                    }
                    catch (Exception e)
                    {
                        switch (e)
                        {
                            case IOException e1:
                                Console.WriteLine("# Connection closed by peer.");
                                break;
                            case AuthenticationException e2:
                                Console.WriteLine("# Authentication failed - closing the connection.");
                                break;
                        }
                        if (e.InnerException != null)
                            Console.WriteLine("# Inner exception: {0}", e.InnerException.Message);
                        _Stream.Close();
                        _Client.Close();
                        _Client = null;
                        OnAuthenticateFail(this.RemoteEndPoint);
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
    #endregion
}
