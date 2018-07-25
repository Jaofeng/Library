using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CJF.Utility;

namespace CJF.Net.Multicast
{
    #region Class : CastReceiver
    /// <summary>UDP 多點傳輸接收元件，使用xxxAsync</summary>
    [Serializable]
    public class CastReceiver : IDisposable
    {
        #region Variables
        LogManager _log = new LogManager(typeof(CastReceiver));
        Socket m_Socket;                            // 伺服器 Socket 物件
        SocketAsyncEventArgs m_ReadEventArgs;
        byte[] m_ReceiveBuffer;
        int m_BufferSize = 1024;                        // 緩衝暫存區大小
        int m_ListenPort = 0;
        long m_ReceiveByteCount = 0;
        bool m_ServerStarted = false;
        bool m_IsExit = false;
        bool m_IsDisposed = false;
        IntPtr m_Handle = IntPtr.Zero;
        IPEndPoint m_LocalEndPoint = null;
        SocketDebugType m_Debug = SocketDebugType.None;
        Timer _SecondCounter = null;
        /// <summary>效能監視器集合</summary>
        Dictionary<ServerCounterType, PerformanceCounter> m_Counters = null;
        List<MulticastOption> m_JoinedGroups = null;
        #endregion

        #region Events
        /// <summary>當伺服器啟動時觸發</summary>
        public event EventHandler<AsyncUdpEventArgs> Started;
        /// <summary>當伺服器關閉時觸發</summary>
        public event EventHandler<AsyncUdpEventArgs> Shutdowned;
        /// <summary>當接收到資料時觸發的事件, 勿忘處理黏包的狀況</summary>
        public event EventHandler<AsyncUdpEventArgs> DataReceived;
        /// <summary>當連線發生錯誤時觸發</summary>
        public event EventHandler<AsyncUdpEventArgs> Exception;
        /// <summary>當每秒傳送計數器值變更時產生</summary>
        public event EventHandler<DataTransEventArgs> CounterChanged;
        #endregion

        #region Construct Method : CastReceiver(int listenPort)
        /// <summary>建立新的 CastReceiver 類別，並初始化相關屬性值</summary>
        /// <param name="listenPort">傾聽的通訊埠號</param>
        public CastReceiver(int listenPort) : this(new IPEndPoint(IPAddress.Any, listenPort)) { }
        #endregion

        #region Construct Method : CastReceiver(IPEndPoint localPort)
        /// <summary>建立新的 CastReceiver 類別，並初始化相關屬性值</summary>
        /// <param name="localPort">傾聽的通訊埠</param>
        public CastReceiver(IPEndPoint localPort)
        {
            m_IsExit = false;
            m_Counters = new Dictionary<ServerCounterType, PerformanceCounter>();
            m_ServerStarted = false;
            CommonSettings();
            SetCounterDictionary();
            m_ListenPort = localPort.Port;
            m_JoinedGroups = new List<MulticastOption>();
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_Handle = m_Socket.Handle;
            m_Socket.ReceiveBufferSize = m_BufferSize;
            m_Socket.SendBufferSize = m_BufferSize;
            m_LocalEndPoint = localPort;
            m_ReceiveBuffer = new byte[m_BufferSize];
            m_ReadEventArgs = new SocketAsyncEventArgs();
            m_ReadEventArgs.UserToken = new AsyncUserToken(m_Socket, m_BufferSize);
            m_ReadEventArgs.RemoteEndPoint = m_LocalEndPoint;
            m_ReadEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            m_ReadEventArgs.SetBuffer(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length);
        }
        /// <summary>釋放 CastReceiver 所使用的資源。 </summary>
        ~CastReceiver() { Dispose(false); }
        #endregion

        #region Private Method : void SetCounterDictionary()
        private void SetCounterDictionary()
        {
            m_Counters.Clear();
            m_Counters.Add(ServerCounterType.TotalReceivedBytes, null);
            m_Counters.Add(ServerCounterType.RateOfReceivedBytes, null);
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
        }
        #endregion

        #region Private Method : void CommonSettings()
        private void CommonSettings()
        {
            this.UseAsyncCallback = EventCallbackMode.BeginInvoke;
            _SecondCounter = new Timer(SecondCounterCallback, null, 1000, 1000);
        }
        #endregion

        #region Public Method : void JoinMulticastGroup(string ipAddr)
        /// <summary>加入欲傾聽的群組</summary>
        /// <param name="ipAddr">群組IP</param>
        public void JoinMulticastGroup(string ipAddr)
        {
            JoinMulticastGroup(IPAddress.Parse(ipAddr));
        }
        #endregion

        #region Public Method : void JoinMulticastGroup(IPAddress ipAddr)
        /// <summary>加入欲傾聽的群組</summary>
        /// <param name="ipAddr">群組IP</param>
        public void JoinMulticastGroup(IPAddress ipAddr)
        {
            if (!m_JoinedGroups.Exists(mo => mo.Group.Equals(ipAddr)))
                m_JoinedGroups.Add(new MulticastOption(ipAddr, m_LocalEndPoint.Address));
        }
        #endregion

        #region Public Method : void Start()
        /// <summary>開始伺服器並等待連線請求</summary>
        public void Start()
        {
            // 設定 Multicast 相關參數
            m_Socket.Bind(m_LocalEndPoint);
            if (m_JoinedGroups.Count == 0)
                throw new ArgumentOutOfRangeException("JoinedGroup", "請先設定欲加入的群組");
            foreach (MulticastOption mo in m_JoinedGroups)
                m_Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mo);
            m_IsExit = false;
            m_ServerStarted = true;

            #region 產生事件 - Started
            OnStarted(m_Handle, m_LocalEndPoint);
            #endregion

            if (!m_Socket.ReceiveFromAsync(m_ReadEventArgs))
                this.ProcessReceive(m_ReadEventArgs);
        }
        #endregion

        #region Public Method : void Shutdown()
        /// <summary>關閉伺服器</summary>
        public void Shutdown()
        {
            if (m_IsDisposed || m_IsExit) return;
            m_IsExit = true;

            if (m_Socket != null)
            {
                try
                {
                    foreach (MulticastOption mo in m_JoinedGroups)
                        m_Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, mo);
                }
                catch { }
                try
                {
                    if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                    {
                        Console.WriteLine("[{0}]Socket : Before Shutdown In CastReceiver.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
                        _log.Write(LogManager.LogLevel.Debug, "Before Shutdown In CastReceiver.Shutdown");
                    }
                    m_Socket.Shutdown(SocketShutdown.Both);
                    if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                    {
                        Console.WriteLine("[{0}]Socket : After Shutdown In CastReceiver.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
                        _log.Write(LogManager.LogLevel.Debug, "After Shutdown In CastReceiver.Shutdown");
                    }
                }
                catch { }
                finally
                {
                    if (m_Debug.HasFlag(SocketDebugType.Close))
                    {
                        Console.WriteLine("[{0}]Socket : Before Close In CastReceiver.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
                        _log.Write(LogManager.LogLevel.Debug, "Before Close In CastReceiver.Shutdown");
                    }
                    m_Socket.Close();
                    if (m_Debug.HasFlag(SocketDebugType.Close))
                    {
                        Console.WriteLine("[{0}]Socket : After Close In CastReceiver.Shutdown", DateTime.Now.ToString("HH:mm:ss.fff"));
                        _log.Write(LogManager.LogLevel.Debug, "After Close In CastReceiver.Shutdown");
                    }
                }
            }
            m_Socket = null;
            m_ServerStarted = false;

            #region 產生事件 - Shutdown
            OnShutdowned(m_Handle, this.LocalEndPort);
            #endregion
        }
        #endregion

        #region Properties
        /// <summary>取得伺服器連線物件</summary>
        public Socket Socket { get { return m_Socket; } }
        /// <summary>取得傾聽的通訊埠號</summary>
        public int ListenPort { get { return m_ListenPort; } }
        /// <summary>取得已加入的群組</summary>
        public MulticastOption[] JoinedGroups { get { return m_JoinedGroups.ToArray(); } }
        /// <summary>取得緩衝區最大值</summary>
        public int BufferSize
        {
            get { return m_BufferSize; }
            set
            {
                m_BufferSize = value;
                if (m_Socket != null)
                {
                    m_Socket.ReceiveBufferSize = m_BufferSize;
                    m_Socket.SendBufferSize = m_BufferSize;
                }
            }
        }
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
        public long ReceiveSpeed { get { return Interlocked.Read(ref m_ReceiveByteCount); } }
        /// <summary>取得值，是否已Disposed</summary>
        public bool IsDisposed { get { return m_IsDisposed; } }
        /// <summary>取得與設定，是否使用非同步方式產生回呼事件</summary>
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
            long rBytes = Interlocked.Read(ref m_ReceiveByteCount);
            Interlocked.Exchange(ref m_ReceiveByteCount, 0);

            #region 產生事件 - CounterChanged
            OnCounterChanged(0, rBytes, 0);
            #endregion
        }
        #endregion

        #region Private Method : void IO_Completed(object sender, SocketAsyncEventArgs e)
        /// <summary>當完成動作時，則呼叫此回呼函示。完成的動作由 SocketAsyncEventArg.LastOperation 屬性取得</summary>
        /// <param name="sender">CastReceiver 物件</param>
        /// <param name="e">完成動作的 SocketAsyncEventArg 物件</param>
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom:
                    this.ProcessReceive(e);
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
            if (e.BytesTransferred > 0)
            {
                if (m_Debug.HasFlag(SocketDebugType.Receive))
                {
                    Console.WriteLine("[{0}]Socket : Exec ReceiveAsync In CastReceiver.ProcessReceive", DateTime.Now.ToString("HH:mm:ss.fff"));
                    _log.Write(LogManager.LogLevel.Debug, "Exec ReceiveAsync In CastReceiver.ProcessReceive");
                }
                if (e.SocketError == SocketError.Success)
                {
                    if (e.UserToken == null) return;
                    AsyncUserToken token = e.UserToken as AsyncUserToken;
                    if (token.IsDisposed) return;
                    Socket s = token.Client;
                    int count = e.BytesTransferred;
                    Interlocked.Add(ref m_ReceiveByteCount, count);
                    m_Counters[ServerCounterType.TotalReceivedBytes]?.IncrementBy(count);
                    m_Counters[ServerCounterType.RateOfReceivedBytes]?.IncrementBy(count);
                    List<byte> rec = new List<byte>();
                    if ((token.CurrentIndex + count) > token.BufferSize)
                    {
                        rec.AddRange(token.ReceivedData);
                        token.ClearBuffer();
                    }
                    token.SetData(e);
                    if (s != null && s.Available == 0)
                    {
                        rec.AddRange(token.ReceivedData);
                        token.ClearBuffer();
                    }

                    #region 產生事件 - DataReceived
                    if (rec.Count != 0)
                    {
                        AsyncUdpEventArgs auea = new AsyncUdpEventArgs(s.Handle, s.LocalEndPoint, e.RemoteEndPoint, rec.ToArray());
                        OnDataReceived(auea);
                    }
                    #endregion

                    if (m_Debug.HasFlag(SocketDebugType.Receive))
                    {
                        Console.WriteLine("[{0}]Socket : Before ReceiveAsync In CastReceiver.ProcessReceive", DateTime.Now.ToString("HH:mm:ss.fff"));
                        _log.Write(LogManager.LogLevel.Debug, "Before ReceiveAsync In CastReceiver.ProcessReceive");
                    }
                    try
                    {
                        if (!s.ReceiveFromAsync(e)) // 讀取下一個由客戶端傳送的封包
                            this.ProcessReceive(e);
                    }
                    catch (ObjectDisposedException) { }
                    catch (Exception ex)
                    {
                        _log.Write(LogManager.LogLevel.Debug, "From:CastReceiver.ProcessReceive");
                        _log.WriteException(ex);
                    }
                    if (m_Debug.HasFlag(SocketDebugType.Receive))
                    {
                        Console.WriteLine("[{0}]Socket : After ReceiveAsync In CastReceiver.ProcessReceive", DateTime.Now.ToString("HH:mm:ss.fff"));
                        _log.Write(LogManager.LogLevel.Debug, "After ReceiveAsync In CastReceiver.ProcessReceive");
                    }
                }
                else
                    this.ProcessError(e);
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
            if (s != null)
            {
                try
                {
                    localEp = s.LocalEndPoint;
                    remoteEp = s.RemoteEndPoint;
                    handle = s.Handle;
                    if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                    {
                        Console.WriteLine("[{0}]Socket : Before Shutdown In CastReceiver.ProcessError", DateTime.Now.ToString("HH:mm:ss.fff"));
                        _log.Write(LogManager.LogLevel.Debug, "Before Shutdown In CastReceiver.ProcessError");
                    }
                    s.Shutdown(SocketShutdown.Both);
                    if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                    {
                        Console.WriteLine("[{0}]Socket : After Shutdown In CastReceiver.ProcessError", DateTime.Now.ToString("HH:mm:ss.fff"));
                        _log.Write(LogManager.LogLevel.Debug, "After Shutdown In CastReceiver.ProcessError");
                    }
                }
                catch (Exception) { }   // 如果客戶端已關閉則不處理
                finally { }
            }
            else
            {
                localEp = m_Socket.LocalEndPoint;
                remoteEp = m_Socket.RemoteEndPoint;
                handle = m_Socket.Handle;
            }

            #region 產生事件 - OnException
            SocketException se = new SocketException((Int32)e.SocketError);
            Exception ex = new Exception(string.Format("客戶端連線({1})發生錯誤:{0},狀態:{2}", (int)e.SocketError, localEp, e.LastOperation), se);
            AsyncUdpEventArgs auea = new AsyncUdpEventArgs(handle, localEp, remoteEp, null, ex);
            OnException(auea);
            #endregion
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
        /// <summary>清除並釋放 CastReceiver 所使用的資源。</summary>
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
                    if (m_Socket == null)
                    {
                        if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                        {
                            Console.WriteLine("[{0}]Socket Is Null In CastReceiver.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
                            _log.Write(LogManager.LogLevel.Debug, "Socket Is Null In CastReceiver.Dispose");
                        }
                    }
                    else
                    {
                        try
                        {
                            if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                            {
                                Console.WriteLine("[{0}]Socket : Before Shutdown In CastReceiver.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
                                _log.Write(LogManager.LogLevel.Debug, "Before Shutdown In CastReceiver.Dispose");
                            }
                            m_Socket.Shutdown(SocketShutdown.Both);
                            if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                            {
                                Console.WriteLine("[{0}]Socket : After Shutdown In CastReceiver.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
                                _log.Write(LogManager.LogLevel.Debug, "After Shutdown In CastReceiver.Dispose");
                            }
                        }
                        catch { }
                        finally
                        {
                            if (m_Debug.HasFlag(SocketDebugType.Close))
                            {
                                Console.WriteLine("[{0}]Socket : Before Close In CastReceiver.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
                                _log.Write(LogManager.LogLevel.Debug, "Before Close In CastReceiver.Dispose");
                            }
                            m_Socket.Close();
                            if (m_Debug.HasFlag(SocketDebugType.Close))
                            {
                                Console.WriteLine("[{0}]Socket : After Close In CastReceiver.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
                                _log.Write(LogManager.LogLevel.Debug, "After Close In CastReceiver.Dispose");
                            }
                            m_Socket = null;
                        }
                    }
                }
                catch { }
            }
            m_IsDisposed = true;
        }
        #endregion

        #region Public Static Method : IPAddress[] GetHostIP()
        /// <summary>取得本機IP</summary>
        /// <returns></returns>
        public static IPAddress[] GetHostIP()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addrs = ipEntry.AddressList;
            List<IPAddress> ipAddr = new List<IPAddress>();
            foreach (IPAddress ipa in addrs)
            {
                if (ipa.IsIPv6LinkLocal || ipa.AddressFamily.HasFlag(System.Net.Sockets.AddressFamily.InterNetworkV6))
                    continue;
                ipAddr.Add(ipa);
            }
            return ipAddr.ToArray();
        }
        #endregion

        #region Protected Virtual Method : void OnStarted(IntPtr handle, EndPoint endPoint)
        /// <summary>產生 Started 事件。</summary>
        /// <param name="handle">原始的控制代碼。</param>
        /// <param name="endPoint">本地端點資訊。</param>
        protected virtual void OnStarted(IntPtr handle, EndPoint endPoint)
        {
            if (this.Started != null)
            {
                AsyncUdpEventArgs auea = new AsyncUdpEventArgs(handle, endPoint, null);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步作法 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncUdpEventArgs> del in this.Started.GetInvocationList())
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
                            foreach (Delegate del in this.Started.GetInvocationList())
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
                            foreach (Delegate del in this.Started.GetInvocationList())
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

        #region Protected Virtual Method : void OnShutdowned(IntPtr handle, EndPoint endPoint)
        /// <summary>產生 Started 事件。</summary>
        /// <param name="handle">原始的控制代碼。</param>
        /// <param name="endPoint">本地端點資訊。</param>
        protected virtual void OnShutdowned(IntPtr handle, EndPoint endPoint)
        {
            if (this.Shutdowned != null)
            {
                AsyncUdpEventArgs auea = new AsyncUdpEventArgs(handle, endPoint, null);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步作法 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncUdpEventArgs> del in this.Shutdowned.GetInvocationList())
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
                            foreach (Delegate del in this.Shutdowned.GetInvocationList())
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
                            foreach (Delegate del in this.Shutdowned.GetInvocationList())
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

        #region Protected Virtual Method : void OnDataReceived(AsyncUdpEventArgs e)
        /// <summary>產生 DataReceived 事件。</summary>
        /// <param name="e">發生錯誤的 AsyncUdpEventArgs 物件</param>
        protected virtual void OnDataReceived(AsyncUdpEventArgs e)
        {
            if (this.DataReceived != null)
            {
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncUdpEventArgs> del in this.DataReceived.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, e, new AsyncCallback(AsyncUdpEventCallback), del); }
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
                                try { del.DynamicInvoke(this, e); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, e } };
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

        #region Protected Virtual Method : void OnException(AsyncUdpEventArgs e)
        /// <summary>產生 Exception 事件。</summary>
        /// <param name="e">發生錯誤的 AsyncUdpEventArgs 物件</param>
        protected virtual void OnException(AsyncUdpEventArgs e)
        {
            if (this.Exception != null)
            {
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncUdpEventArgs> del in this.Exception.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, e, new AsyncCallback(AsyncUdpEventCallback), del); }
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
                                try { del.DynamicInvoke(this, e); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, e } };
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

        #region Protected Virtual Method : void OnCounterChanged(long sended, long received, long waitting)
        /// <summary>產生 CounterChanged 事件。</summary>
        /// <param name="sended">每秒發送的位元組數量</param>
        /// <param name="received">每秒接收的位元組數量</param>
        /// <param name="waitting">等待發送的位元組數量</param>
        protected virtual void OnCounterChanged(long sended, long received, long waitting)
        {
            if (this.CounterChanged != null)
            {
                DataTransEventArgs dtea = new DataTransEventArgs(sended, received, waitting);
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
    #endregion

    #region Class : CastSender
    /// <summary>UDP 多點傳輸發送元件，使用xxxAsync</summary>
    [Serializable]
    public class CastSender : IDisposable
    {
        #region Variables
        LogManager _log = new LogManager(typeof(CastSender));
        Socket m_Socket;                            // 伺服器 Socket 物件
        int m_BufferSize = 1024;                        // 緩衝暫存區大小
        long m_SendByteCount = 0;
        long m_WaittingSend = 0;
        bool m_IsDisposed = false;
        IntPtr m_Handle = IntPtr.Zero;
        EndPoint m_LocalEndPoint = null;
        EndPoint m_RemoteEndPoint = null;
        SocketDebugType m_Debug = SocketDebugType.None;
        Timer _SecondCounter = null;
        /// <summary>效能監視器集合</summary>
        Dictionary<ServerCounterType, PerformanceCounter> m_Counters = null;
        #endregion

        #region Events
        /// <summary>當資料送出後觸發的事件</summary>
        public event EventHandler<AsyncUdpEventArgs> DataSended;
        /// <summary>當連線發生錯誤時觸發</summary>
        public event EventHandler<AsyncUdpEventArgs> Exception;
        /// <summary>當每秒傳送計數器值變更時產生</summary>
        public event EventHandler<DataTransEventArgs> CounterChanged;
        #endregion

        #region Construct Method : CastSender(IPEndPoint ep, int ttl)
        /// <summary>建立新的 CastSender 類別，並初始化相關屬性值</summary>
        /// <param name="ep">接收端的群組端點資訊</param>
        /// <param name="ttl">多點傳輸存留時間，單位豪秒</param>
        public CastSender(IPEndPoint ep, int ttl)
        {
            m_Counters = new Dictionary<ServerCounterType, PerformanceCounter>();
            CommonSettings();
            SetCounterDictionary();
            m_RemoteEndPoint = ep;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_Handle = m_Socket.Handle;
            m_Socket.SendBufferSize = m_BufferSize;
            m_Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ep.Address));
            m_Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);
            m_LocalEndPoint = m_Socket.LocalEndPoint;
            m_Socket.Connect(m_RemoteEndPoint);
        }
        #endregion

        #region Construct Method : CastSender(string groupIP, int port, int ttl)
        /// <summary>建立新的 CastSender 類別，並初始化相關屬性值</summary>
        /// <param name="gIP">接收端的群組 IP, 224.0.0.1 ~ 239.255.255.255 </param>
        /// <param name="port">接收端的通訊埠號</param>
        /// <param name="ttl">多點傳輸存留時間，單位豪秒</param>
        public CastSender(string gIP, int port, int ttl) : this(new IPEndPoint(IPAddress.Parse(gIP), port), ttl) { }
        /// <summary>釋放 CastSender 所使用的資源。 </summary>
        ~CastSender() { Dispose(false); }
        #endregion

        #region Private Method : void SetCounterDictionary()
        private void SetCounterDictionary()
        {
            m_Counters.Clear();
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
            this.UseAsyncCallback = EventCallbackMode.Thread;
            _SecondCounter = new Timer(SecondCounterCallback, null, 1000, 1000);
        }
        #endregion

        #region Public Method : SendData(byte[] data, object extraInfo = null)
        /// <summary>傳送資料到用戶端</summary>
        /// <param name="data">欲發送的資料</param>
        /// <param name="extraInfo">額外傳遞的資料</param>
        public void SendData(byte[] data, object extraInfo = null)
        {
            SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
            arg.RemoteEndPoint = m_RemoteEndPoint;
            arg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            Interlocked.Add(ref m_WaittingSend, data.Length);
            m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(data.Length);
            arg.SetBuffer(data, 0, data.Length);
            arg.UserToken = extraInfo;
            if (m_Debug.HasFlag(SocketDebugType.Send))
            {
                Console.WriteLine("[{0}]Socket : Before SendAsync In CastSender.SendData", DateTime.Now.ToString("HH:mm:ss.fff"));
                _log.Write(LogManager.LogLevel.Debug, "Before SendAsync In CastSender.SendData");
            }
            try
            {
                if (!m_Socket.SendToAsync(arg))
                    this.ProcessSend(arg);
            }
            catch (SocketException)
            {
                Interlocked.Add(ref m_WaittingSend, -data.Length);
                m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(-data.Length);
                m_Counters[ServerCounterType.SendFail]?.Increment();
                m_Counters[ServerCounterType.RateOfSendFail]?.Increment();
                this.ProcessError(arg);
            }
            catch (Exception) { }
            if (m_Debug.HasFlag(SocketDebugType.Send))
            {
                Console.WriteLine("[{0}]Socket : After SendAsync In CastSender.SendData", DateTime.Now.ToString("HH:mm:ss.fff"));
                _log.Write(LogManager.LogLevel.Debug, "After SendAsync In CastSender.SendData");
            }
        }
        #endregion

        #region Properties
        /// <summary>取得伺服器連線物件</summary>
        public Socket Socket { get { return m_Socket; } }
        /// <summary>取得緩衝區最大值</summary>
        public int BufferSize
        {
            get { return m_BufferSize; }
            set
            {
                m_BufferSize = value;
                if (m_Socket != null)
                {
                    m_Socket.ReceiveBufferSize = m_BufferSize;
                    m_Socket.SendBufferSize = m_BufferSize;
                }
            }
        }
        /// <summary>取得 Socket 的控制代碼</summary>
        public IntPtr Handle { get { return m_Handle; } }
        /// <summary>取得本地端通訊埠</summary>
        public EndPoint LocalEndPort { get { return m_LocalEndPoint; } }
        /// <summary>取得本地端通訊埠</summary>
        public EndPoint RemoteEndPoint { get { return m_RemoteEndPoint; } }
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
        /// <summary>取得每一秒發送量，單位:位元組</summary>
        public long SendSpeed { get { return m_SendByteCount; } }
        /// <summary>取得現在等待發送的資料量，單位:位元組</summary>
        public long WaittingSend { get { return m_WaittingSend; } }
        /// <summary>取得值，是否已Disposed</summary>
        public bool IsDisposed { get { return m_IsDisposed; } }
        /// <summary>取得與設定，是否使用非同步方式產生回呼事件</summary>
        public EventCallbackMode UseAsyncCallback { get; set; }
        /// <summary>取得或設定額外專屬的自訂值</summary>
        public object ExtraInfo { get; set; }
        /// <summary>取得或設定額外專屬的自訂值</summary>
        public object TagInfo { get; set; }
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
            long wBytes = Interlocked.Read(ref m_WaittingSend);
            Interlocked.Exchange(ref m_WaittingSend, 0);

            #region 產生事件 - CounterChanged
            OnCounterChanged(sBytes, 0, wBytes);
            #endregion
        }
        #endregion

        #region Private Method : void IO_Completed(object sender, SocketAsyncEventArgs e)
        /// <summary>當完成動作時，則呼叫此回呼函示。完成的動作由 SocketAsyncEventArg.LastOperation 屬性取得</summary>
        /// <param name="sender">CastSender 物件</param>
        /// <param name="e">完成動作的 SocketAsyncEventArg 物件</param>
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.SendTo:
                    this.ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
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
                if (m_Debug.HasFlag(SocketDebugType.Send))
                {
                    Console.WriteLine("[{0}]Socket : Exec SendAsync In CastSender.ProcessSend", DateTime.Now.ToString("HH:mm:ss.fff"));
                    _log.Write(LogManager.LogLevel.Debug, "Exec SendAsync In CastSender.ProcessSend");
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
                    object extraInfo = e.UserToken;

                    #region 產生事件 - DataSended
                    AsyncUdpEventArgs auea = new AsyncUdpEventArgs(m_Socket.Handle, m_Socket.LocalEndPoint, e.RemoteEndPoint, buffer);
                    auea.SetExtraInfo(extraInfo);
                    OnDataSended(auea);
                    #endregion

                    e.Dispose();
                    e = null;
                }
                else
                    this.ProcessError(e);
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
            if (s != null)
            {
                try
                {
                    localEp = s.LocalEndPoint;
                    remoteEp = s.RemoteEndPoint;
                    handle = s.Handle;
                    if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                    {
                        Console.WriteLine("[{0}]Socket : Before Shutdown In CastSender.ProcessError", DateTime.Now.ToString("HH:mm:ss.fff"));
                        _log.Write(LogManager.LogLevel.Debug, "Before Shutdown In CastSender.ProcessError");
                    }
                    s.Shutdown(SocketShutdown.Both);
                    if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                    {
                        Console.WriteLine("[{0}]Socket : After Shutdown In CastSender.ProcessError", DateTime.Now.ToString("HH:mm:ss.fff"));
                        _log.Write(LogManager.LogLevel.Debug, "After Shutdown In CastSender.ProcessError");
                    }
                }
                catch (Exception) { }   // 如果客戶端已關閉則不處理
                finally { }
            }
            else
            {
                localEp = m_Socket.LocalEndPoint;
                remoteEp = m_Socket.RemoteEndPoint;
                handle = m_Socket.Handle;
            }

            #region 產生事件 - Exception
            SocketException se = new SocketException((Int32)e.SocketError);
            Exception ex = new Exception(string.Format("客戶端連線({1})發生錯誤:{0},狀態:{2}", (int)e.SocketError, localEp, e.LastOperation), se);
            AsyncUdpEventArgs auea = new AsyncUdpEventArgs(handle, localEp, remoteEp, null, ex);
            OnException(auea);
            #endregion
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
        /// <summary>清除並釋放 CastSender 所使用的資源。</summary>
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
                    if (m_Socket == null)
                    {
                        if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                        {
                            Console.WriteLine("[{0}]Socket Is Null In CastSender.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
                            _log.Write(LogManager.LogLevel.Debug, "Socket Is Null In CastSender.Dispose");
                        }
                    }
                    else
                    {
                        try
                        {
                            if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                            {
                                Console.WriteLine("[{0}]Socket : Before Shutdown In CastSender.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
                                _log.Write(LogManager.LogLevel.Debug, "Before Shutdown In CastSender.Dispose");
                            }
                            m_Socket.Shutdown(SocketShutdown.Both);
                            if (m_Debug.HasFlag(SocketDebugType.Shutdown))
                            {
                                Console.WriteLine("[{0}]Socket : After Shutdown In CastSender.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
                                _log.Write(LogManager.LogLevel.Debug, "After Shutdown In CastSender.Dispose");
                            }
                        }
                        catch { }
                        finally
                        {
                            if (m_Debug.HasFlag(SocketDebugType.Close))
                            {
                                Console.WriteLine("[{0}]Socket : Before Close In CastSender.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
                                _log.Write(LogManager.LogLevel.Debug, "Before Close In CastSender.Dispose");
                            }
                            m_Socket.Close();
                            if (m_Debug.HasFlag(SocketDebugType.Close))
                            {
                                Console.WriteLine("[{0}]Socket : After Close In CastSender.Dispose", DateTime.Now.ToString("HH:mm:ss.fff"));
                                _log.Write(LogManager.LogLevel.Debug, "After Close In CastSender.Dispose");
                            }
                            m_Socket = null;
                        }
                    }
                }
                catch { }
            }
            m_IsDisposed = true;
        }
        #endregion

        #region Protected Virtual Method : void OnDataSended(AsyncUdpEventArgs e)
        /// <summary>產生 DataSended 事件。</summary>
        /// <param name="e">發生錯誤的 AsyncUdpEventArgs 物件</param>
        protected virtual void OnDataSended(AsyncUdpEventArgs e)
        {
            if (this.DataSended != null)
            {
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncUdpEventArgs> del in this.DataSended.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, e, new AsyncCallback(AsyncUdpEventCallback), del); }
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
                                try { del.DynamicInvoke(this, e); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, e } };
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

        #region Protected Virtual Method : void OnException(AsyncUdpEventArgs e)
        /// <summary>產生 Exception 事件。</summary>
        /// <param name="e">發生錯誤的 AsyncUdpEventArgs 物件</param>
        protected virtual void OnException(AsyncUdpEventArgs e)
        {
            if (this.Exception != null)
            {
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncUdpEventArgs> del in this.Exception.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, e, new AsyncCallback(AsyncUdpEventCallback), del); }
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
                                try { del.DynamicInvoke(this, e); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, e } };
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

        #region Protected Virtual Method : void OnCounterChanged(long sended, long received, long waitting)
        /// <summary>產生 CounterChanged 事件。</summary>
        /// <param name="sended">每秒發送的位元組數量</param>
        /// <param name="received">每秒接收的位元組數量</param>
        /// <param name="waitting">等待發送的位元組數量</param>
        protected virtual void OnCounterChanged(long sended, long received, long waitting)
        {
            if (this.CounterChanged != null)
            {
                DataTransEventArgs dtea = new DataTransEventArgs(sended, received, waitting);
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
    #endregion
}


