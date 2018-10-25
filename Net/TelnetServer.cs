using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CJF.Utility;
using CJF.Utility.Extensions;

namespace CJF.Net.Telnet
{
    /// <summary>非同步 Telnet 連線伺服器，使用xxxAsync</summary>
    [Serializable]
    public class TelnetServer : IDisposable
    {
        private readonly byte[] CLIENT_CONNECTED_COMMANDS = new byte[] { 0xFF, 0xFD, 0x25, 0xFF, 0xFD, 0x01, 0xFF, 0xFD, 0x03, 0xFF, 0xFD, 0x27, 0xFF, 0xFD, 0x1F, 0xFF, 0xFD, 0x00, 0xFF, 0xFB, 0x00 };

        #region Public Enum : CommandEndCharType
        /// <summary>接收資料後，後續產生事件的方式</summary>
        public enum CommandEndCharType
        {
            /// <summary>一般模式，即收即傳</summary>
            None = 0,
            /// <summary>字串模式，收到 NewLine 字串後傳回</summary>
            NewLine = 1
        }
        #endregion

        #region Public Enum : LoginStatus
        /// <summary>登入狀態列舉</summary>
        public enum LoginStatus
        {
            /// <summary>未登入</summary>
            NotLogged = 0,
            /// <summary>登入中</summary>
            Logging = 1,
            /// <summary>已登入</summary>
            LoggedIn = 2
        }
        #endregion

        #region Variables
        AsyncServer m_Server;                       // 伺服器 Socket 物件
        IPEndPoint m_LocalEndPort;                  // 本地端通訊埠
        bool m_IsShutdown = false;
        bool m_IsDisposed = false;
        uint m_AutoCloseTime = 0;
        /// <summary>記錄每個用戶端的接收緩衝區</summary>
        ConcurrentDictionary<EndPoint, byte[]> m_ReceivedBuffer = null;
        /// <summary>記錄每個用戶端的命令結束字元</summary>
        ConcurrentDictionary<EndPoint, CommandEndCharType> m_CommandEndChar = null;
        /// <summary>記錄每個用戶端的 Telnet 狀態</summary>
        ConcurrentDictionary<EndPoint, TelnetClientSetting> m_ClientSettings = null;
        ///// <summary>等待下次傳送給用戶端的封包內容</summary>
        //ConcurrentDictionary<EndPoint, byte[]> m_WaitForSend = null;
        /// <summary>記錄登入帳號與密碼。Index=0:帳號1,Index=1:密碼1,Index=2:帳號2,Index=2:密碼2,...</summary>
        string[] m_IDPWD = null;
        #endregion

        #region Public Events
        /// <summary>當伺服器啟動時觸發。</summary>
        public event EventHandler Started;
        /// <summary>當伺服器關閉時觸發。</summary>
        public event EventHandler Shutdowned;
        /// <summary>當資料送出後觸發的事件。</summary>
        public event EventHandler<SocketServerEventArgs> DataSended;
        /// <summary>當接收到資料時觸發的事件。勿忘處理黏包的狀況。</summary>
        public event EventHandler<SocketServerEventArgs> DataReceived;
        /// <summary>當用戶端連線時觸發。</summary>
        public event EventHandler<SocketServerEventArgs> ClientConnected;
        /// <summary>當用戶端請求關閉連線時觸發。</summary>
        public event EventHandler<SocketServerEventArgs> ClientClosing;
        /// <summary>當用戶端以關閉連線時觸發。</summary>
        public event EventHandler<SocketServerEventArgs> ClientClosed;
        /// <summary>當連線發生錯誤時觸發。</summary>
        public event EventHandler<SocketServerEventArgs> Exception;
        /// <summary>當資料無法發送至遠端時產生。</summary>
        public event EventHandler<SocketServerEventArgs> SendedFail;
        /// <summary>當用戶端設定環境設定變更時產生。</summary>
        public event EventHandler<TelnetSettingChangedEventArgs> SettingsChanged;
        #endregion

        #region Construct Method : TelnetServer(int numConnections, Encoding enc)
        /// <summary>建立新的 TelnetServer 類別，並初始化相關屬性值</summary>
        /// <param name="numConnections">同時可連接的最大連線數</param>
        /// <param name="enc">使用的字元編碼類別</param>
        public TelnetServer(int numConnections, Encoding enc)
        {
            m_IsDisposed = false;
            m_IsShutdown = false;
            m_LocalEndPort = null;
            MaxConnections = numConnections;
            m_ReceivedBuffer = new ConcurrentDictionary<EndPoint, byte[]>();
            m_CommandEndChar = new ConcurrentDictionary<EndPoint, CommandEndCharType>();
            m_ClientSettings = new ConcurrentDictionary<EndPoint, TelnetClientSetting>();
            //m_WaitForSend = new ConcurrentDictionary<EndPoint, byte[]>();
            IsStarted = false;
            m_Server = new AsyncServer(MaxConnections, 1024);
            m_Server.ClientClosed += new EventHandler<SocketServerEventArgs>(Server_OnClientClosed);
            m_Server.ClientClosing += new EventHandler<SocketServerEventArgs>(Server_OnClientClosing);
            m_Server.ClientConnected += new EventHandler<SocketServerEventArgs>(Server_OnClientConnected);
            m_Server.DataReceived += new EventHandler<SocketServerEventArgs>(Server_OnDataReceived);
            m_Server.DataSended += new EventHandler<SocketServerEventArgs>(Server_OnDataSended);
            m_Server.Exception += new EventHandler<SocketServerEventArgs>(Server_OnException);
            m_Server.SendedFail += new EventHandler<SocketServerEventArgs>(Server_OnSendedFail);
            m_Server.Started += new EventHandler<SocketServerEventArgs>(Server_OnStarted);
            this.DefaultEndChar = TelnetServer.CommandEndCharType.None;
            this.FilterCommands = true;
            this.PopupCommands = false;
            this.Encoding = enc;
        }
        /// <summary>釋放 TelnetServer 所使用的資源。 </summary>
        ~TelnetServer() { Dispose(false); }
        #endregion

        #region Public Method : void Start(IPEndPoint localEndPoint)
        /// <summary>開始伺服器並等待連線請求, 如需引入效能監視器(PerformanceCounter)，請先執行LoadCounterDictionary函示</summary>
        /// <param name="localEndPoint">本地傾聽通訊埠</param>
        public void Start(IPEndPoint localEndPoint)
        {
            m_LocalEndPort = localEndPoint;
            m_Server.AutoCloseTime = m_AutoCloseTime;
            m_Server.Start(localEndPoint);
        }
        #endregion

        #region Public Method : void Shutdown()
        /// <summary>關閉伺服器</summary>
        public void Shutdown()
        {
            if (m_IsDisposed || m_IsShutdown) return;
            m_IsShutdown = true;
            try
            {
                m_ReceivedBuffer?.Clear();
                m_CommandEndChar?.Clear();
                m_ClientSettings?.Clear();
                m_Server?.Shutdown();
            }
            catch { }
            OnShutdowned();
        }
        #endregion

        #region Public Method : void SetCommandEndChar(EndPoint ep, CommandEndCharType ct)
        /// <summary>設定遠端命令結束字元檢查方式</summary>
        /// <param name="ep">遠端端點資訊</param>
        /// <param name="ct">字元檢查方式列舉值</param>
        public void SetCommandEndChar(EndPoint ep, CommandEndCharType ct)
        {
            if (m_CommandEndChar.ContainsKey(ep))
                m_CommandEndChar.AddOrUpdate(ep, ct, (k, v) => v = ct);
        }
        #endregion

        #region Public Method : CommandEndCharType? GetCommandEndChar(EndPoint ep)
        /// <summary>取得遠端命令結束字元檢查方式</summary>
        /// <param name="ep">欲查詢遠端端點資訊</param>
        /// <returns>查詢的遠端命令結束字元檢查方式</returns>
        public CommandEndCharType? GetCommandEndChar(EndPoint ep)
        {
            if (m_CommandEndChar.TryGetValue(ep, out CommandEndCharType ct))
                return ct;
            else
                return null;
        }
        #endregion

        #region Public Method : TelnetClientSetting? GetTelnetClientSetting(EndPoint ep)
        /// <summary>取得用戶端的參數設定結構類別</summary>
        /// <param name="ep">欲查詢遠端端點資訊</param>
        /// <returns></returns>
        public TelnetClientSetting? GetTelnetClientSetting(EndPoint ep)
        {
            if (m_ClientSettings.TryGetValue(ep, out TelnetClientSetting tcs))
                return tcs;
            else
                return null;
        }
        #endregion

        #region Public Properties
        /// <summary>取得值，目前連線數</summary>
        public int Connections { get { return m_Server != null ? m_Server.Connections : 0; } }
        /// <summary>取得值，伺服器連線物件</summary>
        public AsyncServer SocketServer { get { return m_Server; } }
        /// <summary>取得值，最大連線數</summary>
        public int MaxConnections { get; private set; }
        /// <summary>取得值，本地端通訊埠</summary>
        public IPEndPoint LocalEndPort { get => m_LocalEndPort; }
        /// <summary>取得值，目前伺服器否啟動中</summary>
        public bool IsStarted { get; private set; }
        /// <summary>設定或取得編碼類別</summary>
        public Encoding Encoding { get; set; }
        /// <summary>設定或取得是否過濾IAC指令。true:過濾指令; false:不過濾事件，將會當成是資料。預設為true</summary>
        public bool FilterCommands { get; set; }
        /// <summary>設定或取得是否將IAC指令往上層傳遞。true:傳遞指令，且會當成是資料並傳遞事件; false:不傳遞事件。預設為false。</summary>
        public bool PopupCommands { get; set; }
        /// <summary>設定或取得命令結束字元檢查方式</summary>
        public CommandEndCharType DefaultEndChar { get; set; } = CommandEndCharType.None;
        /// <summary>設定或取得新行結束字元，預設 CrLf。</summary>
        public string NewLineChars { get; set; } = Environment.NewLine;
        /// <summary>設定或取得長時間未操作自動將用戶端斷線的設定時間，單位秒，0表不作動，預設值為0</summary>
        public uint AutoCloseTime
        {
            get { return m_AutoCloseTime; }
            set
            {
                m_AutoCloseTime = value;
                if (m_Server != null)
                    m_Server.AutoCloseTime = m_AutoCloseTime;
            }
        }
        /// <summary>設定或取得是否使用登入帳號/密碼。空值為不使用；不為空值時，則使用帳密功能，且格式需為：帳號1,密碼1,帳號2,密碼2,...</summary>
        /// <exception cref="FormatException">格式錯誤</exception>
        /// <exception cref="ArgumentNullException">帳號或密碼不得為空值</exception>
        public string Authentication
        {
            get
            {
                if (m_IDPWD == null || m_IDPWD.Length == 0)
                    return string.Empty;
                else
                    return string.Join(",", m_IDPWD);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    m_IDPWD = null;
                else
                {
                    string[] tmp = value.Split(',');
                    if (tmp.Length % 2 != 0)
                        throw new FormatException("格式錯誤");
                    else
                    {
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (string.IsNullOrEmpty(tmp[i]))
                                throw new ArgumentNullException("帳號或密碼不得為空值");
                        }
                        m_IDPWD = tmp;
                    }
                }
            }
        }
        #endregion

        #region IDisposable
        /// <summary>清除並釋放 TelnetServer 所使用的資源。</summary>
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
                try
                {
                    if (m_Server != null)
                    {
                        if (m_Server.IsStarted)
                            m_Server.Shutdown();
                        m_Server.Dispose();
                        m_Server = null;
                    }
                    m_ReceivedBuffer?.Clear();
                    m_ReceivedBuffer = null;
                    m_CommandEndChar?.Clear();
                    m_CommandEndChar = null;
                    m_ClientSettings?.Clear();
                    m_ClientSettings = null;
                }
                catch { }
            }
            m_IsDisposed = true;
        }
        #endregion

        #region Protected Virtual Method : void OnStarted()
        /// <summary>產生 Started 事件。</summary>
        protected virtual void OnStarted()
        {
            if (Started == null)
                return;
            EventArgs arg = new EventArgs();
            foreach (EventHandler del in Started.GetInvocationList())
            {
                try { del.BeginInvoke(this, arg, null, del); }
                catch (Exception ex) { Debug.Print(ex.Message); }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnShutdowned()
        /// <summary>產生 Shutdowned 事件。</summary>
        protected virtual void OnShutdowned()
        {
            if (Shutdowned == null)
                return;
            EventArgs arg = new EventArgs();
            foreach (EventHandler del in Shutdowned.GetInvocationList())
            {
                try { del.BeginInvoke(this, arg, null, del); }
                catch (Exception ex) { Debug.Print(ex.Message); }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnClientConnected(SocketServerEventArgs e)
        /// <summary>產生 ClientConnected 事件。</summary>
        /// <param name="e">要傳遞的內容。</param>
        protected virtual void OnClientConnected(SocketServerEventArgs e)
        {
            if (ClientConnected == null)
                return;
            foreach (EventHandler<SocketServerEventArgs> del in ClientConnected.GetInvocationList())
            {
                try { del.BeginInvoke(this, e, null, del); }
                catch (Exception ex) { Debug.Print(ex.Message); }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnClientClosing(SocketServerEventArgs e)
        /// <summary>產生 ClientClosing 事件。</summary>
        /// <param name="e">要傳遞的內容。</param>
        protected virtual void OnClientClosing(SocketServerEventArgs e)
        {
            if (ClientClosing != null)
            {
                foreach (EventHandler<SocketServerEventArgs> del in ClientClosing.GetInvocationList())
                {
                    try { del.BeginInvoke(this, e, null, del); }
                    catch (Exception ex) { Debug.Print(ex.Message); }
                }
            }
            if (e.RemoteEndPoint != null)
            {
                m_ReceivedBuffer?.TryRemove(e.RemoteEndPoint, out byte[] tmp);
                m_CommandEndChar?.TryRemove(e.RemoteEndPoint, out CommandEndCharType ct);
                m_ClientSettings?.TryRemove(e.RemoteEndPoint, out TelnetClientSetting tcs);
            }
        }
        #endregion

        #region Protected Virtual Method : void OnClientClosed(SocketServerEventArgs e)
        /// <summary>產生 ClientClosed 事件。</summary>
        /// <param name="e">要傳遞的內容。</param>
        protected virtual void OnClientClosed(SocketServerEventArgs e)
        {
            if (ClientClosed != null)
            {
                foreach (EventHandler<SocketServerEventArgs> del in ClientClosed.GetInvocationList())
                {
                    try { del.BeginInvoke(this, e, null, del); }
                    catch (Exception ex) { Debug.Print(ex.Message); }
                }
            }
            if (e.RemoteEndPoint != null)
            {
                m_ReceivedBuffer?.TryRemove(e.RemoteEndPoint, out byte[] tmp);
                m_CommandEndChar?.TryRemove(e.RemoteEndPoint, out CommandEndCharType ct);
                m_ClientSettings?.TryRemove(e.RemoteEndPoint, out TelnetClientSetting tcs);
            }
        }
        #endregion

        #region Protected Virtual Method : void OnDataSended(SocketServerEventArgs e)
        /// <summary>產生 DataSended 事件。</summary>
        /// <param name="e">要傳遞的內容。</param>
        protected virtual void OnDataSended(SocketServerEventArgs e)
        {
            if (DataSended == null)
                return;
            SocketServerEventArgs ssea = e;
            if (this.FilterCommands && !this.PopupCommands)
            {
                byte[] snd = FilterSendedCommands(e.Data);
                if (snd.Length == 0)
                    return;
                ssea = new SocketServerEventArgs(e.Client, snd);
            }
            foreach (EventHandler<SocketServerEventArgs> del in DataSended.GetInvocationList())
            {
                try { del.BeginInvoke(this, ssea, null, del); }
                catch (Exception ex) { Debug.Print(ex.Message); }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnDataReceived(SocketServerEventArgs e)
        /// <summary>產生 DataReceived 事件。</summary>
        /// <param name="e">要傳遞的內容。</param>
        protected virtual void OnDataReceived(SocketServerEventArgs e)
        {
            if (DataReceived == null)
                return;
            foreach (EventHandler<SocketServerEventArgs> del in DataReceived.GetInvocationList())
            {
                try { del.BeginInvoke(this, e, null, del); }
                catch (Exception ex) { Debug.Print(ex.Message); }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnException(SocketServerEventArgs e)
        /// <summary>產生 Exception 事件。</summary>
        /// <param name="e">要傳遞的內容。</param>
        protected virtual void OnException(SocketServerEventArgs e)
        {
            if (Exception == null)
                return;
            foreach (EventHandler<SocketServerEventArgs> del in Exception.GetInvocationList())
            {
                try { del.BeginInvoke(this, e, null, del); }
                catch (Exception ex) { Debug.Print(ex.Message); }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnSendedFail(SocketServerEventArgs e)
        /// <summary>產生 SendedFail 事件。</summary>
        /// <param name="e">要傳遞的內容。</param>
        protected virtual void OnSendedFail(SocketServerEventArgs e)
        {
            if (SendedFail == null)
                return;
            foreach (EventHandler<SocketServerEventArgs> del in SendedFail.GetInvocationList())
            {
                try { del.BeginInvoke(this, e, null, del); }
                catch (Exception ex) { Debug.Print(ex.Message); }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnSettingsChanged(EndPoint rp, TelnetClientSetting orig, TelnetClientSetting newSetting)
        /// <summary>產生 SettingsChanged 事件。</summary>
        /// <param name="rp">遠端端點資訊。</param>
        /// <param name="orig">原始設定。</param>
        /// <param name="newSetting">新設定。</param>
        protected virtual void OnSettingsChanged(EndPoint rp, TelnetClientSetting orig, TelnetClientSetting newSetting)
        {
            if (SettingsChanged == null)
                return;
            TelnetSettingChangedEventArgs arg = new TelnetSettingChangedEventArgs(rp, orig, newSetting);
            foreach (EventHandler<TelnetSettingChangedEventArgs> del in SettingsChanged.GetInvocationList())
            {
                try { del.BeginInvoke(this, arg, null, del); }
                catch (Exception ex) { Debug.Print(ex.Message); }
            }
        }
        #endregion


        #region Private Method : void Server_OnStarted(object sender, AsyncServerEventArgs e)
        private void Server_OnStarted(object sender, SocketServerEventArgs e)
        {
            IsStarted = m_Server.IsStarted;
            m_IsShutdown = false;
            OnStarted();
        }
        #endregion

        #region Private Method : void Server_OnSendedFail(object sender, AsyncServerEventArgs e)
        private void Server_OnSendedFail(object sender, SocketServerEventArgs e)
        {
            OnSendedFail(e);
        }
        #endregion

        #region Private Method : void Server_OnException(object sender, AsyncServerEventArgs e)
        private void Server_OnException(object sender, SocketServerEventArgs e)
        {
            OnException(e);
        }
        #endregion

        #region Private Method : void Server_OnDataSended(object sender, AsyncServerEventArgs e)
        private void Server_OnDataSended(object sender, SocketServerEventArgs e)
        {
            OnDataSended(e);
        }
        #endregion

        #region Private Method : void Server_OnDataReceived(object sender, AsyncServerEventArgs e)
        private void Server_OnDataReceived(object sender, SocketServerEventArgs e)
        {
            string data = string.Empty;
            if (!m_CommandEndChar.TryGetValue(e.RemoteEndPoint, out CommandEndCharType ct))
            {
                ct = this.DefaultEndChar;
                m_CommandEndChar.AddOrUpdate(e.RemoteEndPoint, ct, (k, v) => v = ct);
            }
            if (!m_ClientSettings.TryGetValue(e.RemoteEndPoint, out TelnetClientSetting tcs))
            {
                tcs = TelnetClientSetting.Default;
                m_ClientSettings.AddOrUpdate(e.RemoteEndPoint, tcs, (k, v) => v = tcs);
            }
            byte[] rec = e.Data;
            if (this.FilterCommands)
                rec = FilterReceivedCommands(e.Client, e.Data);
            if (rec.Length == 0 && e.Data.Length == 0)
                return;
            else if (rec.Length == 0 && e.Data.Length != 0 && !this.PopupCommands)
                return;
            if (tcs.Echo && rec.Length != 0)
                e.Client.SendData(rec);
            if (this.PopupCommands)
            {
                OnDataReceived(new SocketServerEventArgs(e.Client, e.Data));
            }
            else
            {
                if (ct == CommandEndCharType.NewLine)
                {
                    // 行輸入模式
                    data = this.Encoding.GetString(rec);
                    if (data.Equals(NewLineChars))
                    {
                        // 收到 Enter 按鍵
                        byte[] buf = null;

                        #region 處理登入流程
                        if (m_IDPWD != null && m_IDPWD.Length != 0 && tcs.LoginStatus != LoginStatus.LoggedIn)
                        {
                            buf = new byte[m_ReceivedBuffer[e.RemoteEndPoint].Length];
                            Array.Copy(m_ReceivedBuffer[e.RemoteEndPoint], buf, m_ReceivedBuffer[e.RemoteEndPoint].Length);
                            m_ReceivedBuffer.AddOrUpdate(e.RemoteEndPoint, new byte[] { }, (k, v) => v = new byte[] { });
                            data = Encoding.Default.GetString(buf);

                            switch (tcs.LoginStatus)
                            {
                                case LoginStatus.NotLogged:
                                    #region 尚未登入，要求輸入密碼
                                    for (int i = 0; i < m_IDPWD.Length; i += 2)
                                    {
                                        if (data.Equals(m_IDPWD[i]))
                                        {
                                            e.Client.SendData(new byte[] { 0xFF, 0xFE, 0x01, 0xFF, 0xF1, 0xFF, 0xF1 });
                                            Thread.Sleep(500);
                                            e.Client.SendData(tcs.NewLine + "Password:\x1B[0m");
                                            tcs.LoginStatus = LoginStatus.Logging;
                                            tcs.UserID = data;
                                            m_ClientSettings.AddOrUpdate(e.RemoteEndPoint, tcs, (k, v) => v = tcs);
                                            break;
                                        }
                                        else
                                        {
                                            e.Client.SendData(new byte[] { 0xFF, 0xFB, 0x01, 0xFF, 0xF1, 0xFF, 0xF1 });
                                            Thread.Sleep(200);
                                            e.Client.SendData(tcs.NewLine + "\x1B[31;1mUnknow account!!!\x1B[0m" + tcs.NewLine + "Login:");
                                        }
                                    }
                                    break;
                                #endregion
                                case LoginStatus.Logging:
                                    #region 登入中，收到輸入的密碼
                                    // 檢查密碼
                                    for (int i = 0; i < m_IDPWD.Length; i += 2)
                                    {
                                        if (tcs.UserID.Equals(m_IDPWD[i]))
                                        {
                                            if (data.Equals(m_IDPWD[i + 1]))
                                            {
                                                tcs.LoginStatus = LoginStatus.LoggedIn;
                                                m_ClientSettings.AddOrUpdate(e.RemoteEndPoint, tcs, (k, v) => v = tcs);
                                                break;
                                            }
                                        }
                                    }
                                    if (tcs.LoginStatus == LoginStatus.LoggedIn)
                                    {
                                        // 密碼正確
                                        e.Client.SendData(tcs.NewLine + "\x1B[32;1mSuccess!!!\x1B[0m");
                                        SetCommandEndChar(e.RemoteEndPoint, TelnetServer.CommandEndCharType.None);
                                        e.Client.SendData(new byte[] { 0xFF, 0xFB, 0x01, 0xFF, 0xF1, 0xFF, 0xF1 });
                                        Thread.Sleep(1000);
                                        OnClientConnected(e);
                                    }
                                    else
                                    {
                                        // 密碼錯誤
                                        e.Client.SendData(tcs.NewLine + "\x1B[31;1mAccess denied!!!\x1B[0m" + tcs.NewLine);
                                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                                        {
                                            Thread.Sleep(2000);
                                            e.Client.Close();
                                        });
                                    }
                                    break;
                                    #endregion
                            }
                            return;
                        }
                        #endregion

                        buf = new byte[m_ReceivedBuffer[e.RemoteEndPoint].Length];
                        Array.Copy(m_ReceivedBuffer[e.RemoteEndPoint], buf, m_ReceivedBuffer[e.RemoteEndPoint].Length);
                        m_ReceivedBuffer.AddOrUpdate(e.RemoteEndPoint, new byte[] { }, (k, v) => v = new byte[] { });
                        OnDataReceived(new SocketServerEventArgs(e.Client, buf));
                    }
                    else
                    {
                        #region 收到一般按鍵, 過濾控制鍵, 並存入暫存區
                        foreach (byte b in rec)
                        {
                            if (b < 0x20)
                            {
                                switch (b)
                                {
                                    case 0x08:
                                        RemoveBufferLastByte(e.RemoteEndPoint);
                                        break;
                                    case 0x1B:
                                        m_ReceivedBuffer.AddOrUpdate(e.RemoteEndPoint, new byte[] { }, (k, v) => v = new byte[] { });
                                        break;
                                    default:
                                        // Ingore
                                        break;
                                }
                            }
                            else
                                AppendBuffer(e.RemoteEndPoint, b);
                        }
                        #endregion
                    }
                }
                else if (ct == CommandEndCharType.None)
                {
                    // 一般按鍵輸入模式
                    if (m_ReceivedBuffer[e.RemoteEndPoint].Length != 0)
                    {
                        byte[] buf = new byte[m_ReceivedBuffer[e.RemoteEndPoint].Length + rec.Length];
                        Array.Copy(m_ReceivedBuffer[e.RemoteEndPoint], buf, m_ReceivedBuffer[e.RemoteEndPoint].Length);
                        m_ReceivedBuffer.AddOrUpdate(e.RemoteEndPoint, new byte[] { }, (k, v) => v = new byte[] { });
                        Array.Copy(rec, 0, buf, buf.Length - rec.Length, rec.Length);
                        OnDataReceived(new SocketServerEventArgs(e.Client, buf));
                    }
                    else
                        OnDataReceived(new SocketServerEventArgs(e.Client, rec));
                }
            }
        }
        #endregion

        #region Private Method : void Server_OnClientConnected(object sender, AsyncServerEventArgs e)
        private void Server_OnClientConnected(object sender, SocketServerEventArgs e)
        {
            RemoveInvalidClient();
            m_ReceivedBuffer.AddOrUpdate(e.RemoteEndPoint, new byte[] { }, (k, v) => v = new byte[] { });
            m_CommandEndChar.AddOrUpdate(e.RemoteEndPoint, this.DefaultEndChar, (k, v) => v = this.DefaultEndChar);
            m_ClientSettings.AddOrUpdate(e.RemoteEndPoint, TelnetClientSetting.Default, (k, v) => v = TelnetClientSetting.Default);
            if (m_IDPWD != null && m_IDPWD.Length != 0)
            {
                //System.Threading.Tasks.Task.Factory.StartNew(() =>
                //{
                //    Thread.Sleep(500);
                    SetCommandEndChar(e.RemoteEndPoint, TelnetServer.CommandEndCharType.NewLine);
                    e.Client.SendData("\x1B[2J\x1B[1;1Hlogin:");
                //});
            }
            else
            {
                OnClientConnected(e);
            }
        }
        #endregion

        #region Private Method : void Server_OnClientClosing(object sender, AsyncServerEventArgs e)
        private void Server_OnClientClosing(object sender, SocketServerEventArgs e)
        {
            OnClientClosing(e);
        }
        #endregion

        #region Private Method : void Server_OnClientClosed(object sender, AsyncServerEventArgs e)
        private void Server_OnClientClosed(object sender, SocketServerEventArgs e)
        {
            OnClientClosed(e);
        }
        #endregion

        #region Private Method : void AppendBuffer(EndPoint ep, byte data)
        private void AppendBuffer(EndPoint ep, byte data)
        {
            int origLen = 0;
            if (m_ReceivedBuffer.ContainsKey(ep))
                origLen = m_ReceivedBuffer[ep].Length;
            byte[] buf = new byte[origLen + 1];
            if (origLen == 0)
                buf[0] = data;
            else
            {
                Array.Copy(m_ReceivedBuffer[ep], buf, origLen);
                buf[buf.Length - 1] = data;
            }
            m_ReceivedBuffer.AddOrUpdate(ep, buf, (k, v) => v = buf);
        }
        #endregion

        #region Private Method : void AppendBuffer(EndPoint ep, byte[] data)
        private void AppendBuffer(EndPoint ep, byte[] data)
        {
            int origLen = 0;
            if (m_ReceivedBuffer.ContainsKey(ep))
                origLen = m_ReceivedBuffer[ep].Length;
            byte[] buf = new byte[origLen + data.Length];
            if (origLen == 0)
                Array.Copy(data, buf, data.Length);
            else
            {
                Array.Copy(m_ReceivedBuffer[ep], buf, origLen);
                Array.Copy(data, 0, buf, origLen, data.Length);
            }
            m_ReceivedBuffer.AddOrUpdate(ep, buf, (k, v) => v = buf);
        }
        #endregion

        #region Private Method : void RemoveBufferLastByte(EndPoint ep)
        private void RemoveBufferLastByte(EndPoint ep)
        {
            int origLen = 0;
            if (m_ReceivedBuffer.ContainsKey(ep))
                origLen = m_ReceivedBuffer[ep].Length;
            if (origLen == 0)
                return;
            else if (origLen == 1)
                m_ReceivedBuffer.AddOrUpdate(ep, new byte[] { }, (k, v) => v = new byte[] { });
            else
            {
                byte[] buf = new byte[origLen - 1];
                Array.Copy(m_ReceivedBuffer[ep], buf, buf.Length);
                m_ReceivedBuffer.AddOrUpdate(ep, buf, (k, v) => v = buf);
            }
        }
        #endregion

        #region Private Method : void RemoveInvalidClient()
        private void RemoveInvalidClient()
        {
            EndPoint[] eps = new EndPoint[m_ReceivedBuffer.Keys.Count];
            m_ReceivedBuffer.Keys.CopyTo(eps, 0);
            foreach (EndPoint ep in eps)
            {
                if (m_Server.FindClient(ep) == null)
                {
                    m_ReceivedBuffer.TryRemove(ep, out byte[] tmp);
                    m_CommandEndChar.TryRemove(ep, out CommandEndCharType ct);
                    m_ClientSettings.TryRemove(ep, out TelnetClientSetting tcs);
                }
            }
        }
        #endregion

        #region Private Method : void FilterReceivedCommands(AsyncClient ac, byte[] data)
        /// <summary>過濾傳入的 Telnet 指令</summary>
        /// <param name="ac">使用者端</param>
        /// <param name="data">傳入的資料</param>
        /// <returns></returns>
        private byte[] FilterReceivedCommands(AsyncClient ac, byte[] data)
        {
            List<byte> cmd = new List<byte>();
            List<byte> back = new List<byte>();
            if (!m_ClientSettings.TryGetValue(ac.RemoteEndPoint, out TelnetClientSetting tcs))
                tcs = TelnetClientSetting.Default;
            TelnetClientSetting orig = tcs.Clone();
            int idx = 0;
            while (idx < data.Length)
            {
                if (data[idx] == 0xFF)
                {
                    // from : http://www.tsnien.idv.tw/Internet_WebBook/chap11/11-4%20Telnet%20通訊協定.html
                    // Telnet 命令
                    // Interpret as Command(IAC)
                    switch (data[idx + 1])
                    {
                        case 0xF0:  // 240 : SE					-> 子谈判参数的结束
                        case 0xF1:  // 241 : NOP				-> 空操作
                        case 0xF2:  // 242 : Data Mark			-> 一个同步信号的数据流部分。该命令的後面经常跟着一个TCP紧急通知
                        case 0xF3:  // 243 : Break				-> NVT的BRK字符
                        case 0xF4:  // 244 : Interrupt Process	-> IP 功能.
                        case 0xF5:  // 245 : Abort output		-> AO 功能.
                        case 0xF6:  // 246 : Are You There		-> AYT 功能.
                        case 0xF7:  // 247 : Erase character	-> 刪除字元.
                        case 0xF8:  // 248 : Erase Line			-> 刪除行.
                        case 0xF9:  // 249 : Go ahead			-> 
                            idx += 2;
                            break;
                        case 0xFA:  // 250 : SB					-> 表示後面所跟的是對需要的選項的子谈判
                            #region SB(250)
                            {
                                idx += 2;
                                switch (data[idx])
                                {
                                    case 0x1F:  // Set Windows Size
                                        tcs.WindowSize = new WindowSize(data.ToInt16(idx + 1, true), data.ToInt16(idx + 3, true));
                                        idx += 5;
                                        break;
                                    case 0x18:  // Terminal Type(24)
                                        #region Terminal Type(24)
                                        {
                                            idx++;
                                            int len = 0;
                                            while (data[idx + len] != 0xFF && data[idx + len + 1] != 0xF0 && idx + len < data.Length)
                                                len++;
                                            if (len != 0)
                                                tcs.TerminalType = Encoding.ASCII.GetString(data, idx + 1, len - 1);
                                            idx += len + 2;
                                            break;
                                        }
                                    #endregion
                                    case 0x20:  // Terminal Speen(32)
                                        #region Terminal Speed(32)
                                        {
                                            idx++;
                                            int len = 0;
                                            while (data[idx + len] != 0xFF && data[idx + len + 1] != 0xF0 && idx + len < data.Length)
                                                len++;
                                            if (len != 0)
                                            {
                                                string tmp = Encoding.ASCII.GetString(data, idx + 1, len - 1);
                                                if (tmp.IndexOf(',') != -1)
                                                    tcs.TerminalSpeed = Convert.ToInt32(tmp.Split(',')[0]);
                                                else
                                                    tcs.TerminalSpeed = Convert.ToInt32(tmp);
                                            }
                                            idx += len + 2;
                                            break;
                                        }
                                    #endregion
                                    case 0x27:  // New Environment(39)
                                    default:
                                        {
                                            // By pass
                                            idx++;
                                            while (data[idx] != 0xFF && data[idx + 1] != 0xF0 && idx < data.Length)
                                                idx++;
                                            break;
                                        }
                                }
                                break;
                            }
                        #endregion
                        case 0xFB:  // 251 : WILL(0xFB) -> 發送者想激活某選項，接受者接收(DO,0xFD)或拒絕(DONT,0xFE)該選項請求
                            #region WILL(251)
                            {
                                idx += 2;
                                if (data[idx] < 0x2F)
                                {
                                    back.AddRange(new byte[] { 0xFF, 0xFD, data[idx] });
                                    switch (data[idx])
                                    {
                                        case 0x00:  // Binary Transmission
                                            if (!tcs.BinaryTransmission)
                                            {
                                                tcs.BinaryTransmission = true;
                                                back.AddRange(new byte[] { 0xFF, 0xFD, 0x00 });
                                            }
                                            break;
                                        case 0x01:  // ECHO(1)
                                            if (!tcs.Echo)
                                            {
                                                tcs.Echo = true;
                                                back.AddRange(new byte[] { 0xFF, 0xFD, 0x01 });
                                            }
                                            break;
                                        case 0x03:  // Will Suppress Go Ahead(3)
                                        case 0x12:  // LOGOUT(18)
                                        case 0x1F:  // Negotiate About Window Size(31)
                                            break;
                                        case 0x18:  // Terminal Type(24)
                                                    // Return -> DO Terminal Type & SB Terminal Type
                                            back.AddRange(new byte[] { 0xFF, 0xFA, 0x18, 0x01, 0xFF, 0xF0 });
                                            break;
                                        case 0x20:  // Terminal Speed(32)
                                                    // Return -> DO NOT Terminal Speed
                                            back.AddRange(new byte[] { 0xFF, 0xFA, 0x20, 0x01, 0xFF, 0xF0 });
                                            break;
                                        case 0x25:  // Authentication Option
                                            back.AddRange(new byte[] { 0xFF, 0xFA, 0x25, 0x01, 0x0F, 0x00, 0xFF, 0xF0 });
                                            break;
                                        case 0x27:  // New Environment Option(39)
                                                    // Return -> DO New Environment Option
                                            back.AddRange(new byte[] { 0xFF, 0xFA, 0x27, 0x01, 0xFF, 0xF0 });
                                            break;
                                    }
                                    idx++;
                                }
                                break;
                            }
                        #endregion
                        case 0xFC:  // 252 : WONT(0xFC) -> 發送者希望使某選項無效，接受者必須接受(DONT,0xFE)該請求
                            #region WONT(252)
                            {
                                idx += 2;
                                if (data[idx] < 0x2F)
                                {
                                    switch (data[idx])
                                    {
                                        case 0x00:  // Binary Transmission
                                            if (tcs.BinaryTransmission)
                                            {
                                                tcs.BinaryTransmission = false;
                                                back.AddRange(new byte[] { 0xFF, 0xFE, 0x00 });
                                            }
                                            break;
                                        case 0x01:  // ECHO(1)
                                            if (tcs.Echo)
                                            {
                                                tcs.Echo = false;
                                                back.AddRange(new byte[] { 0xFF, 0xFE, 0x01 });
                                            }
                                            break;
                                        case 0x12:  // LOGOUT(18)
                                            break;
                                    }
                                    idx++;
                                }
                                break;
                            }
                        #endregion
                        case 0xFD:  // 253 : DO  (0xFD) -> 發送者希望接收者激活某選項，接受者接受(WILL,0xFB)或拒絕(DONT,0xFE)該請求
                            #region DO(253)
                            {
                                idx += 2;
                                if (data[idx] < 0x2F)
                                {
                                    switch (data[idx])
                                    {
                                        case 0x00:  // Binary Transmission
                                            if (!tcs.BinaryTransmission)
                                            {
                                                tcs.BinaryTransmission = true;
                                                back.AddRange(new byte[] { 0xFF, 0xFB, 0x00 });
                                            }
                                            break;
                                        case 0x01:  // ECHO(1)
                                            if (!tcs.Echo)
                                            {
                                                tcs.Echo = true;
                                                back.AddRange(new byte[] { 0xFF, 0xFB, 0x01 });
                                            }
                                            break;
                                    }
                                    idx++;
                                }
                                break;
                            }
                        #endregion
                        case 0xFE:  // 254 : DONT(0xFE) -> 發送者希望對方使某選項無效，接受者必須接受(WONT,0xFC)該請求
                            #region DONT(254)
                            {
                                // DO NOT(0xFE) -> Return WILL NOT(0xFC)
                                idx += 2;
                                if (data[idx] < 0x2F)
                                {
                                    switch (data[idx])
                                    {
                                        case 0x00:  // Binary Transmission
                                            if (tcs.BinaryTransmission)
                                            {
                                                tcs.BinaryTransmission = false;
                                                back.AddRange(new byte[] { 0xFF, 0xFC, 0x00 });
                                            }
                                            break;
                                        case 0x01:  // ECHO(1)
                                            if (tcs.Echo)
                                            {
                                                tcs.Echo = false;
                                                back.AddRange(new byte[] { 0xFF, 0xFC, 0x01 });
                                            }
                                            break;
                                        case 0x18:  // TERMTYPE(24)
                                            back.AddRange(new byte[] { 0xFF, 0xFC, 0x18 });
                                            break;
                                    }
                                    idx++;
                                }
                                break;
                            }
                        #endregion
                        default:
                            idx++;
                            break;
                    }
                }
                else
                {
                    cmd.Add(data[idx]);
                    idx++;
                }
            }
            if (back.Count != 0)
                ac.SendData(back.ToArray());
            if (!tcs.Equals(orig))
            {
                OnSettingsChanged(ac.RemoteEndPoint, orig, tcs);
                m_ClientSettings.AddOrUpdate(ac.RemoteEndPoint, tcs, (k, v) => v = tcs);
            }
            return cmd.ToArray();
        }
        #endregion

        #region Private Method : void FilterSendedCommands(byte[] data)
        /// <summary>過濾輸出的 Telnet 指令</summary>
        /// <param name="data">傳入的資料</param>
        /// <returns></returns>
        private byte[] FilterSendedCommands(byte[] data)
        {
            List<byte> cmd = new List<byte>();
            int idx = 0;
            while (idx < data.Length)
            {
                if (data[idx] == 0xFF)
                {
                    // from : https://hk.saowen.com/a/044160fd7765db9d42d3a6e2bb32b085e52188957a5c99e6d6dd8a82d1a7f75f
                    // Telnet 命令
                    // Interpret as Command(IAC)
                    switch (data[idx + 1])
                    {
                        case 0xF0:  // 240 : SE					-> 子谈判参数的结束
                        case 0xF1:  // 241 : NOP				-> 空操作
                        case 0xF2:  // 242 : Data Mark			-> 一個同步信號的數據流部分。該命令的後面經常跟著一個TCP緊急通知
                        case 0xF3:  // 243 : Break				-> NVT的BRK字符
                        case 0xF4:  // 244 : Interrupt Process	-> IP 功能.
                        case 0xF5:  // 245 : Abort output		-> AO 功能.
                            idx += 2;
                            break;
                        case 0xF6:  // 246 : Are You There		-> AYT 功能.
                            idx += 2;
                            break;
                        case 0xF7:  // 247 : Erase character	-> 刪除字元.
                        case 0xF8:  // 248 : Erase Line			-> 刪除行.
                        case 0xF9:  // 249 : Go ahead			-> 
                            idx += 2;
                            break;
                        case 0xFA:  // 250 : SB					-> 表示後面所跟的是對需要的選項的子谈判
                            {
                                idx += 2;
                                while (data[idx] != 0xFF && data[idx + 1] != 0xF0 && idx < data.Length)
                                    idx++;
                                break;
                            }
                        case 0xFB:  // 251 : WILL				-> 表示希望開始使用或者確認所使用的是指定的選項。
                        case 0xFC:  // 252 : WILL NOT			-> 表示希望開始不使用或者確認所不使用的是指定的選項。
                        case 0xFD:  // 253 : DO					-> 表示一方要求另一方使用，或者確認你希望另一方使用指定的選項。
                        case 0xFE:  // 254 : DO	NOT				-> 表示一方要求另一方不使用，或者確認你希望另一方不使用指定的選項。
                            {
                                idx += 2;
                                if (data[idx] < 0x2F)
                                    idx++;
                                break;
                            }
                        default:
                            idx++;
                            break;
                    }
                }
                else
                {
                    cmd.Add(data[idx]);
                    idx++;
                }
            }
            return cmd.ToArray();
        }
        #endregion
    }

    #region Public Struct : TelnetClientSetting
    /// <summary>自定義 Telnet 參數狀態結構</summary>
    public struct TelnetClientSetting : ICloneable, IEquatable<TelnetClientSetting>
    {
        /// <summary>使用者登入狀態</summary>
        public TelnetServer.LoginStatus LoginStatus { get; set; }
        /// <summary>登入者帳號</summary>
        public string UserID { get; set; }
        /// <summary>伺服器是否回應(ECHO)</summary>
        public bool Echo { get; set; }
        /// <summary>終端種類</summary>
        public string TerminalType { get; set; }
        /// <summary>終端速度</summary>
        public int TerminalSpeed { get; set; }
        /// <summary>終端視窗大小</summary>
        public WindowSize WindowSize { get; set; }
        /// <summary>新行字符</summary>
        public string NewLine { get; set; }
        /// <summary>是否使用二進制傳輸</summary>
        public bool BinaryTransmission { get; set; }
        /// <summary>定義預設的Telnet參數狀態</summary>
        public static TelnetClientSetting Default
        {
            get
            {
                return new TelnetClientSetting()
                {
                    LoginStatus = TelnetServer.LoginStatus.NotLogged,
                    UserID = string.Empty,
                    Echo = false,
                    TerminalType = string.Empty,
                    TerminalSpeed = 0,
                    NewLine = Environment.NewLine,
                    BinaryTransmission = true,
                    WindowSize = new WindowSize(80, 24)
                };
            }
        }

        /// <summary>取得容易檢視屬性值內容的字串。</summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{{LoginStatus:'{LoginStatus}', UserID:'{UserID}', Echo:'{Echo}', " +
                $"TerminalType:'{TerminalType}', TerminalSpeed:'{TerminalSpeed}', " +
                $"NewLine:'{Encoding.Default.GetBytes(NewLine).ToHexString()}', " +
                $"BinaryTransmission:'{BinaryTransmission}', " +
                $"WindowSize:{WindowSize}}}";
        }
        object ICloneable.Clone()
        {
            return this.Clone();
        }
        /// <summary>複製結構。</summary>
        /// <returns></returns>
        public TelnetClientSetting Clone()
        {
            return new TelnetClientSetting()
            {
                LoginStatus = LoginStatus,
                UserID = UserID,
                Echo = Echo,
                TerminalType = TerminalType,
                TerminalSpeed = TerminalSpeed,
                NewLine = NewLine,
                BinaryTransmission = BinaryTransmission,
                WindowSize = new WindowSize(WindowSize.Width, WindowSize.Height)
            };
        }
        /// <summary>檢查兩 TelnetClientSetting 結構是否相同。</summary>
        /// <param name="other">另一個要檢查的 TelnetClientSetting 結構。</param>
        /// <returns></returns>
        public bool Equals(TelnetClientSetting other)
        {
            if (ReferenceEquals(this, other))
                return true;
            else
                return
                    other.LoginStatus.Equals(LoginStatus) &&
                    other.UserID.Equals(UserID) &&
                    other.Echo.Equals(Echo) &&
                    other.TerminalType.Equals(TerminalType) &&
                    other.TerminalSpeed.Equals(TerminalSpeed) &&
                    other.NewLine.Equals(NewLine) &&
                    other.BinaryTransmission.Equals(BinaryTransmission) &&
                    other.WindowSize.Equals(WindowSize);
        }
    }
    #endregion

    #region Public Struct : WindowSize
    /// <summary>Console 視窗大小資料結構</summary>
    public struct WindowSize : IEquatable<WindowSize>
    {
        /// <summary>寬度</summary>
        public int Width { get; set; }
        /// <summary>高度</summary>
        public int Height { get; set; }
        /// <summary>建立一個 WindowSize 資料結構</summary>
        /// <param name="w">寬度</param>
        /// <param name="h">高度</param>
        public WindowSize(int w, int h)
        {
            this.Width = w;
            this.Height = h;
        }
        /// <summary>取得容易檢視屬性值內容的字串。</summary>
        public override string ToString()
        {
            return $"{{Width:'{Width}', Height:'{Height}'}}";
        }
        /// <summary>檢查兩 WindowSize 結構是否相同。</summary>
        /// <param name="other">另一個要檢查的 WindowSize 結構。</param>
        /// <returns></returns>
        public bool Equals(WindowSize other)
        {
            if (ReferenceEquals(this, other))
                return true;
            else
                return other.Width.Equals(Width) && other.Height.Equals(Height);
        }
    }
    #endregion

    #region EventArgs For TelnetSettingChangedEventArgs
    /// <summary>TelnetSettingChangedEventArgs 類別，供 TelnetServer 產生事件用。</summary>
    public class TelnetSettingChangedEventArgs : EventArgs
    {
        private readonly TelnetClientSetting _NewSetting;
        private readonly TelnetClientSetting _Original;
        private readonly EndPoint _RemoteEndPoint = null;

        /// <summary>取得值，用戶端新的設定值。</summary>
        public TelnetClientSetting NewSettint { get => _NewSetting; }
        /// <summary>取得值，用戶端原始設定值。</summary>
        public TelnetClientSetting Original { get => _Original; }
        /// <summary>取得值，遠端端點資訊。</summary>
        public EndPoint RemoteEndPoint { get => _RemoteEndPoint; }
        /// <summary>建立新的 SettingChangedEventArgs 類別</summary>
        /// <param name="ep">遠端端點資訊。</param>
        /// <param name="orig">用戶端原始設定值。</param>
        /// <param name="newSetting">用戶端新的設定值。</param>
        public TelnetSettingChangedEventArgs(EndPoint ep, TelnetClientSetting orig, TelnetClientSetting newSetting)
        {
            IPEndPoint ipp = null;
            if (ep != null)
            {
                ipp = (IPEndPoint)ep;
                _RemoteEndPoint = new IPEndPoint(ipp.Address, ipp.Port);
            }
            _Original = orig;
            _NewSetting = newSetting;
        }
    }
    #endregion

}


