using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CJF.Utility;
using CJF.Utility.Extensions;

#pragma warning disable IDE0019
#pragma warning disable IDE0017
#pragma warning disable IDE1006
namespace CJF.Net
{
    /// <summary>非同步 TCP 連線伺服器，使用xxxAsync</summary>
    [Serializable]
    public class AsyncServer : IDisposable
    {
        #region Variables
        internal Mutex m_Mutex = null;
        internal Socket m_ListenSocket;                     // 伺服器 Socket 物件
        internal SocketAsyncEventArgsPool m_Pool;           // SocketAsyncEventArgs 預備接線池
        internal Semaphore m_MaxClients;
        internal IPEndPoint m_LocalEndPort;                 // 本地端通訊埠
        internal ConcurrentDictionary<string, AsyncClient> m_Clients;   // 已連線的客戶端
                                                                        /// <summary>效能監視器集合</summary>
        internal Dictionary<ServerCounterType, PerformanceCounter> m_Counters = null;
        internal ConcurrentDictionary<EndPoint, int> m_WaitToClean = null;

        internal bool m_IsShutdown = false;
        internal bool m_IsDisposed = false;
        internal Timer m_CleanClientTimer = null;

        private readonly int m_BufferSize;                           // 緩衝暫存區大小
        SocketAsyncEventArgs m_MainEventArg;
        uint m_AutoCloseTime = 0;
        #endregion

        #region Public Events
        /// <summary>當伺服器啟動時觸發</summary>
        public event EventHandler<SocketServerEventArgs> Started;
        /// <summary>當伺服器關閉時觸發</summary>
        public event EventHandler<SocketServerEventArgs> Shutdowned;
        /// <summary>當資料送出後觸發的事件</summary>
        public event EventHandler<SocketServerEventArgs> DataSended;
        /// <summary>當接收到資料時觸發的事件<br />勿忘處理黏包的狀況</summary>
        public event EventHandler<SocketServerEventArgs> DataReceived;
        /// <summary>當用戶端連線時觸發</summary>
        public event EventHandler<SocketServerEventArgs> ClientConnected;
        /// <summary>當用戶端請求關閉連線時觸發</summary>
        public event EventHandler<SocketServerEventArgs> ClientClosing;
        /// <summary>當用戶端以關閉連線時觸發</summary>
        public event EventHandler<SocketServerEventArgs> ClientClosed;
        /// <summary>當連線發生錯誤時觸發</summary>
        public event EventHandler<SocketServerEventArgs> Exception;
        /// <summary>當資料無法發送至遠端時產生</summary>
        public event EventHandler<SocketServerEventArgs> SendedFail;
        #endregion

        #region Construct Method : AsyncServer(...)
        /// <summary>[保護]建立新的 AsyncServer 類別</summary>
        protected AsyncServer() { }
        /// <summary>建立新的 AsyncServer 類別，並初始化相關屬性值</summary>
        /// <param name="numConnections">同時可連接的最大連線數</param>
        public AsyncServer(int numConnections) : this(numConnections, 1024) { }
        /// <summary>建立新的 AsyncServer 類別，並初始化相關屬性值</summary>
        /// <param name="numConnections">同時可連接的最大連線數</param>
        /// <param name="bufferSize">接收緩衝暫存區大小</param>
        public AsyncServer(int numConnections, int bufferSize)
        {
            m_Mutex = new Mutex();
            this.UseAsyncCallback = EventCallbackMode.BeginInvoke;
            m_IsDisposed = false;
            m_LocalEndPort = null;
            m_Counters = new Dictionary<ServerCounterType, PerformanceCounter>();
            //this.MaxConnections = numConnections;
            m_BufferSize = bufferSize;
            m_Pool = new SocketAsyncEventArgsPool();
            // 預留兩條線程，用於過多的連線數檢查
            m_MaxClients = new Semaphore(numConnections + 2, numConnections + 2);
            m_Clients = new ConcurrentDictionary<string, AsyncClient>();
            m_WaitToClean = new ConcurrentDictionary<EndPoint, int>();
            m_IsShutdown = false;
            this.IsStarted = false;
            for (int i = 0; i < numConnections; i++)
            {
                SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
                arg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                arg.DisconnectReuseSocket = true;
                arg.SetBuffer(new Byte[m_BufferSize], 0, m_BufferSize);
                m_Pool.Push(arg);
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

        #region Public Virtual Method : void LoadCounterDictionary(string categoryName, string instanceName)
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
        public virtual void LoadCounterDictionary(string categoryName, string instanceName)
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

        #region Public Virtual Method : void Start(IPEndPoint localEndPoint)
        /// <summary>開始伺服器並等待連線請求, 如需引入效能監視器(PerformanceCounter)，請先執行LoadCounterDictionary函示</summary>
        /// <param name="localEndPoint">本地傾聽通訊埠</param>
        public virtual void Start(IPEndPoint localEndPoint)
        {
            m_LocalEndPort = localEndPoint;
            m_ListenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_ListenSocket.ReceiveBufferSize = m_BufferSize;
            m_ListenSocket.SendBufferSize = m_BufferSize;
            m_ListenSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            m_ListenSocket.NoDelay = true;
            m_ListenSocket.Bind(localEndPoint);
            m_ListenSocket.Listen(m_Pool.Count);
            this.IsStarted = true;
            m_IsShutdown = false;
            m_CleanClientTimer = new Timer(CleanInvalidClients, null, 10000, 10000);

            this.OnStarted();

            this.StartAccept(null);
            m_Mutex.WaitOne();
        }
        #endregion

        #region Public Virtual Method : void Shutdown()
        /// <summary>關閉伺服器</summary>
        public virtual void Shutdown()
        {
            if (m_IsDisposed || m_IsShutdown) return;
            m_IsShutdown = true;

            #region 關閉用戶端連線
            int counter = 0;
            AsyncClient[] acs = null;
            DateTime now;
            while (m_Clients.Values.Count > 0 && counter < 3)
            {
                counter++;
                acs = new AsyncClient[m_Clients.Count];
                m_Clients.Values.CopyTo(acs, 0);
                foreach (AsyncClient ac in acs)
                {
                    if (ac.IsConnected) ac.Close();
                    now = DateTime.Now;
                    while (ac.IsConnected && DateTime.Now.Subtract(now).TotalMilliseconds <= 500)
                        Thread.Sleep(100);
                }
                now = DateTime.Now;
                while (m_Clients.Count > 0 && DateTime.Now.Subtract(now).TotalMilliseconds <= 1000)
                    Thread.Sleep(100);
                if (m_Clients.Count == 0)
                    break;
            }
            #endregion

            m_CleanClientTimer?.Dispose();
            m_CleanClientTimer = null;

            #region Shutdown Listener
            if (m_ListenSocket != null && m_ListenSocket.Connected)
            {
                try { m_ListenSocket.Shutdown(SocketShutdown.Both); }
                catch (Exception ex)
                {
                    Debug.Print("[LIB]EX:Shutdown:{0}", ex.Message);
                }
            }
            if (m_ListenSocket != null && m_ListenSocket.IsBound)
            {
                m_ListenSocket.Close();
                Thread.Sleep(500);
            }
            #endregion

            #region Removing Counter
            foreach (var ct in m_Counters.Keys)
            {
                m_Counters[ct]?.Close();
                m_Counters[ct]?.Dispose();
            }
            m_Counters.Clear();
            m_WaitToClean.Clear();
            this.IsStarted = false;
            #endregion

            try { m_Mutex.Close(); }
            catch { }

            this.OnShutdowned();
        }
        #endregion

        #region Public Method : SendData(AsyncClient ac, byte[] data)
        /// <summary>傳送資料到用戶端</summary>
        /// <param name="ac"></param>
        /// <param name="data"></param>
        public void SendData(AsyncClient ac, byte[] data)
        {
            if (m_IsShutdown) return;
            if (ac.IsConnected)
            {
                try
                {
                    Interlocked.Add(ref ac.m_WaittingSend, data.Length);
                    if (!m_IsShutdown && !m_IsDisposed)
                        m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(data.Length);
                    SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
                    arg.SetBuffer(data, 0, data.Length);
                    arg.RemoteEndPoint = ac.RemoteEndPoint;
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    arg.UserToken = new AsyncUserToken(ac.Socket, data.Length);
                    try
                    {
                        if (!ac.Socket.SendAsync(arg))
                            this.ProcessSend(arg);
                    }
                    catch
                    {
                        Interlocked.Add(ref ac.m_WaittingSend, -data.Length);
                        if (!m_IsShutdown && !m_IsDisposed)
                        {
                            m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(-data.Length);
                            m_Counters[ServerCounterType.SendFail]?.Increment();
                            m_Counters[ServerCounterType.RateOfSendFail]?.Increment();
                        }
                    }
                }
                catch (Exception ex) { Debug.Print(ex.Message); }
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
            string[] ipps = new string[m_Clients.Count];
            m_Clients.Keys.CopyTo(ipps, 0);
            foreach (string ipp in ipps)
            {
                if (ipp.Equals(cipp.ToString()))
                    return m_Clients[ipp];
            }
            return null;
        }
        #endregion

        #region Protected Virtual Method : void OnStarted()
        /// <summary>產生 Started 事件</summary>
        protected virtual void OnStarted()
        {
            if (this.Started != null)
            {
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<SocketServerEventArgs> del in this.Started.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, null, new AsyncCallback(AsyncServerEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.Started.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, null); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, null } };
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
                                }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnShutdowned()
        /// <summary>產生 Shutdowned 事件</summary>
        protected virtual void OnShutdowned()
        {
            if (this.Shutdowned != null)
            {
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<SocketServerEventArgs> del in this.Shutdowned.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, null, new AsyncCallback(AsyncServerEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.Shutdowned.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, null); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, null } };
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
                                }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnDataSended(AsyncClient ac, byte[] buffer, object extraInfo = null)
        /// <summary>產生 DataSended 事件</summary>
        /// <param name="ac">欲發送資料對象的 AsyncClient 類別</param>
        /// <param name="buffer">資料內容</param>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnDataSended(AsyncClient ac, byte[] buffer, object extraInfo = null)
        {
            if (ac != null) ac.ResetIdleTime();
            if (this.DataSended != null)
            {
                SocketServerEventArgs asea = new SocketServerEventArgs(ac, buffer);
                asea.SetExtraInfo(extraInfo);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<SocketServerEventArgs> del in this.DataSended.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.DataSended.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, asea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
                                }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnDataReceived(AsyncClient ac, byte[] buffer)
        /// <summary>產生 DataReceived 事件</summary>
        /// <param name="ac">傳送資料的 AsyncClient 類別</param>
        /// <param name="buffer">資料內容</param>
        protected virtual void OnDataReceived(AsyncClient ac, byte[] buffer)
        {
            if (ac != null) ac.ResetIdleTime();
            if (buffer.Length != 0 && this.DataReceived != null)
            {
                SocketServerEventArgs asea = new SocketServerEventArgs(ac, buffer);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 -BeginInvoke
                        {
                            foreach (EventHandler<SocketServerEventArgs> del in this.DataReceived.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 -DynamicInvoke
                        {
                            foreach (Delegate del in this.DataReceived.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, asea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
                                }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnClientConnected(AsyncClient ac)
        /// <summary>產生 ClientConnected 事件</summary>
        /// <param name="ac">已連線的 AsyncClient 類別</param>
        protected virtual void OnClientConnected(AsyncClient ac)
        {
            if (ac != null) ac.ResetIdleTime();
            if (this.ClientConnected != null)
            {
                SocketServerEventArgs asea = new SocketServerEventArgs(ac);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<SocketServerEventArgs> del in this.ClientConnected.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.ClientConnected.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, asea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Thread:
                        #region 建立執行緒 - Thread
                        {
                            foreach (Delegate del in this.ClientConnected.GetInvocationList())
                            {
                                try
                                {
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
                                }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnClientClosing(AsyncClient ac, object extraInfo = null)
        /// <summary>產生 ClientClosing 事件</summary>
        /// <param name="ac">斷線的 AsyncClient 類別</param>
        /// <param name="byIdle">是否因閒置而斷線</param>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnClientClosing(AsyncClient ac, bool byIdle = false, object extraInfo = null)
        {
            if (ac != null) ac.ResetIdleTime();
            if (this.ClientClosing != null)
            {
                SocketServerEventArgs asea = new SocketServerEventArgs(ac);
                asea.ClosedByIdle = byIdle;
                asea.SetExtraInfo(extraInfo);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<SocketServerEventArgs> del in this.ClientClosing.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.ClientClosing.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, asea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Thread:
                        #region 建立執行緒 - Thread
                        {
                            foreach (Delegate del in this.ClientClosing.GetInvocationList())
                            {
                                try
                                {
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
                                }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnClientClosed(AsyncClient ac, bool byIdle = false, object extraInfo = null)
        /// <summary>產生 ClientClosed 事件</summary>
        /// <param name="ac">斷線的 AsyncClient 類別</param>
        /// <param name="byIdle">是否因閒置而斷線</param>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnClientClosed(AsyncClient ac, bool byIdle = false, object extraInfo = null)
        {
            if (this.ClientClosed != null)
            {
                SocketServerEventArgs asea = new SocketServerEventArgs(ac);
                asea.ClosedByIdle = byIdle;
                asea.SetExtraInfo(extraInfo);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<SocketServerEventArgs> del in this.ClientClosed.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.ClientClosed.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, asea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Thread:
                        #region 建立執行緒 - Thread
                        {
                            foreach (Delegate del in this.ClientClosed.GetInvocationList())
                            {
                                try
                                {
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
                                }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnClientClosed(IntPtr handle, EndPoint ep)
        /// <summary>產生 ClientClosed 事件</summary>
        /// <param name="handle">原 AsyncClient 連線類別的控制代碼</param>
        /// <param name="ep">遠端節點資訊</param>
        protected virtual void OnClientClosed(IntPtr handle, EndPoint ep)
        {
            if (this.ClientClosed != null)
            {
                SocketServerEventArgs asea = new SocketServerEventArgs(handle, ep);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<SocketServerEventArgs> del in this.ClientClosed.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.ClientClosed.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, asea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Thread:
                        #region 建立執行緒 - Thread
                        {
                            foreach (Delegate del in this.ClientClosed.GetInvocationList())
                            {
                                try
                                {
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
                                }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnException(AsyncClient ac, Exception ex)
        /// <summary>產生 Exception 事件</summary>
        /// <param name="ac">發生錯誤 AsyncClient 的類別</param>
        /// <param name="ex">錯誤原因</param>
        protected virtual void OnException(AsyncClient ac, Exception ex)
        {
            if (this.Exception != null)
            {
                SocketServerEventArgs asea = new SocketServerEventArgs(ac, null, ex);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<SocketServerEventArgs> del in this.Exception.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
                                catch (Exception exx) { Debug.Print(exx.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.Exception.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, asea); }
                                catch (Exception exx) { Debug.Print(exx.Message); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
                                }
                                catch (Exception exx) { Debug.Print(exx.Message); }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnSendedFail(AsyncClient ac, byte[] buffer, object extraInfo = null)
        /// <summary>產生 SendedFail 事件</summary>
        /// <param name="ac">發送失敗的 AsyncClient 類別</param>
        /// <param name="buffer">資料內容</param>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnSendedFail(AsyncClient ac, byte[] buffer, object extraInfo = null)
        {
            if (this.SendedFail != null)
            {
                SocketServerEventArgs asea = new SocketServerEventArgs(ac, buffer);
                asea.SetExtraInfo(extraInfo);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<SocketServerEventArgs> del in this.SendedFail.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, asea, new AsyncCallback(AsyncServerEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.SendedFail.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, asea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Thread:
                        #region 建立執行緒 - Thread
                        {
                            foreach (Delegate del in this.SendedFail.GetInvocationList())
                            {
                                try
                                {
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, asea } };
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
                                }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>取得值，目前連線數</summary>
        public int Connections { get => m_Clients.Count; }
        /// <summary>取得值，伺服器連線物件</summary>
        public Socket Socket { get => m_ListenSocket; }
        /// <summary>取得值，緩衝區最大值</summary>
        public int BufferSize { get => m_BufferSize; }
        ///// <summary>取得值，最大連線數</summary>
        //public int MaxConnections { get; private set; }
        /// <summary>取得值，本地端通訊埠</summary>
        public IPEndPoint LocalEndPort { get => m_LocalEndPort; }
        /// <summary>取得值，目前伺服器否啟動中</summary>
        public bool IsStarted { get; protected set; } = false;
        /// <summary>取得值，已接受連線的次數，此數值由自訂之效能計數器中取出。</summary>
        public long AcceptCounter { get => (long)m_Counters[ServerCounterType.TotalRequest]?.NextValue(); }
        /// <summary>取得值，接線池剩餘數量。</summary>
        public int PoolSurplus { get => m_Pool.Count; }
        /// <summary>取得伺服器所有效能監視器</summary>
        public Dictionary<ServerCounterType, PerformanceCounter> PerformanceCounters { get => m_Counters; }
        /// <summary>取得與設定，是否使用非同步方式產生回呼事件</summary>
        public EventCallbackMode UseAsyncCallback { get; set; } = EventCallbackMode.BeginInvoke;
        /// <summary>取得所有遠端連線類別物件</summary>
        public AsyncClient[] Clients
        {
            get
            {
                AsyncClient[] acs = new AsyncClient[m_Clients.Values.Count];
                m_Clients.Values.CopyTo(acs, 0);
                return acs;
            }
        }
        /// <summary>設定或取得長時間未操作自動將用戶端斷線的設定時間，單位秒，0表不作動，預設值為0</summary>
        public uint AutoCloseTime
        {
            get => m_AutoCloseTime;
            set
            {
                m_AutoCloseTime = value;
                if (m_Clients.Values.Count != 0)
                {
                    AsyncClient[] acs = new AsyncClient[m_Clients.Values.Count];
                    m_Clients.Values.CopyTo(acs, 0);
                    foreach (AsyncClient ac in acs)
                        ac.AutoClose = m_AutoCloseTime;
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void StartAccept(SocketAsyncEventArgs acceptEventArg)
        /// <summary>開始接收連線請求</summary>
        /// <param name="e">已接受的 SocketAsyncEventArgs 物件</param>
        protected virtual void StartAccept(SocketAsyncEventArgs e)
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
                m_MaxClients.WaitOne();
                m_MainEventArg = e;
                if (!m_ListenSocket.AcceptAsync(e))
                    ProcessAccept(e);
                if (e.SocketError != SocketError.Success)
                {
                    Debug.Print("[{0}]Socket : AsyncServer.StartAccept Fail:{1}", DateTime.Now.ToString("HH:mm:ss.fff"), e.SocketError);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }
        #endregion

        #region Protected Virtual Method : void IO_Completed(object sender, SocketAsyncEventArgs e)
        /// <summary>當完成動作時，則呼叫此回呼函示。完成的動作由 SocketAsyncEventArg.LastOperation 屬性取得</summary>
        /// <param name="sender">AsyncServer 物件</param>
        /// <param name="e">完成動作的 SocketAsyncEventArg 物件</param>
        protected virtual void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
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
                    this.CloseClientSocket(e);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Protected Virtual Method : void ProcessAccept(SocketAsyncEventArgs e)
        /// <summary>處理接受連線</summary>
        /// <param name="e">完成連線的 SocketAsyncEventArg 物件</param>
        protected virtual void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (m_IsShutdown || m_IsDisposed) return;
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
                    readEventArgs = m_Pool.Pop();
                    if (readEventArgs != null)
                    {
                        AsyncClient ac = new AsyncClient(s);
                        ac.ResetIdleTime();
                        ac.AutoClose = m_AutoCloseTime;
                        ac.BeforeSend += new EventHandler<AsyncClientEventArgs>(ac_OnBeforeSended);
                        ac.DataSended += new EventHandler<AsyncClientEventArgs>(ac_OnDataSended);
                        ac.SendFail += new EventHandler<AsyncClientEventArgs>(ac_OnSendedFail);
                        ac.Closed += new EventHandler<AsyncClientEventArgs>(ac_OnClosed);
                        ac.Closing += new EventHandler<AsyncClientEventArgs>(ac_OnClosing);
                        m_Clients.AddOrUpdate(s.RemoteEndPoint.ToString(), ac, (k, v) =>
                            {
                                v.Dispose();
                                v = null;
                                v = ac;
                                return v;
                            });
                        readEventArgs.UserToken = new AsyncUserToken(s, m_BufferSize);

                        if (!m_IsShutdown && !m_IsDisposed)
                        {
                            // 下一行是 Syntax Sugar，等同於
                            // if (m_Counters[ServerCounterType.PoolUsed] != null)
                            //     m_Counters[ServerCounterType.PoolUsed].Increment();
                            m_Counters[ServerCounterType.PoolUsed]?.Increment();
                            m_Counters[ServerCounterType.RateOfPoolUse]?.Increment();
                            m_Counters[ServerCounterType.TotalRequest]?.Increment();
                            m_Counters[ServerCounterType.RateOfRequest]?.Increment();
                            m_Counters[ServerCounterType.Connections]?.Increment();
                        }

                        this.OnClientConnected(ac);

                        try
                        {
                            if (!s.ReceiveAsync(readEventArgs))
                                ProcessReceive(readEventArgs);
                        }
                        catch (ObjectDisposedException) { }
                    }
                    else
                    {
                        throw new SocketException(10024);
                    }
                }
                catch (SocketException ex)
                {
                    AsyncUserToken token = (AsyncUserToken)e.UserToken;
                    Debug.Print(ex.Message);

                    AsyncClient ac = new AsyncClient(s);
                    this.OnException(ac, ex);

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
                catch (Exception ex) { Debug.Print(ex.Message); }
                finally
                {
                    // 等待下一個連線請求
                    this.StartAccept(e);
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void ProcessReceive(SocketAsyncEventArgs e)
        /// <summary>當完成接收資料時，將呼叫此函示
        /// <para>如果客戶端關閉連接，將會一併關閉此連線(Socket)</para>
        /// <para>如果收到數據接著將數據返回到客戶端</para></summary>
        /// <param name="e">已完成接收的 SocketAsyncEventArg 物件</param>
        protected virtual void ProcessReceive(SocketAsyncEventArgs e)
        {
            int index = Thread.CurrentThread.ManagedThreadId;
            AsyncUserToken token = e.UserToken as AsyncUserToken;
            if (token == null || token.IsDisposed || m_IsShutdown || m_IsDisposed)
                return;
            IntPtr origHandle = IntPtr.Zero;
            Socket s = token.Client;
            AsyncClient ac;
            EndPoint remote;
            IPEndPoint remote4Callback = null;
            //string rep;
            //if (e.RemoteEndPoint != null)
            //    rep = e.RemoteEndPoint.ToString();
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
                        ac = m_Clients[s.RemoteEndPoint.ToString()];
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
                            this.CloseClientSocket(e);
                            return;
                        }

                        if (ac != null)
                            Interlocked.Add(ref ac.m_ReceiveByteCount, count);
                        if (!m_IsShutdown && !m_IsDisposed)
                        {
                            m_Counters[ServerCounterType.TotalReceivedBytes]?.IncrementBy(count);
                            m_Counters[ServerCounterType.RateOfReceivedBytes]?.IncrementBy(count);
                        }
                        this.OnDataReceived(ac, rec.ToArray());
                        if (!ac.IsConnected)
                        {
                            RecyclingSocket(origHandle, remote4Callback, e);
                            return;
                        }
                        try
                        {
                            if (s != null && !s.ReceiveAsync(e))    // 讀取下一個由客戶端傳送的封包
                                this.ProcessReceive(e);
                        }
                        catch (ObjectDisposedException) { }
                        catch (Exception ex)
                        {
                            if (!m_IsShutdown && !m_IsDisposed)
                            {
                                Debug.Print(ex.Message);
                            }
                        }
                    }
                    else
                        this.ProcessError(e);
                }
                else
                {
                    RecyclingSocket(origHandle, remote4Callback, e);
                }
            }
            catch (KeyNotFoundException)
            {
                this.CloseClientSocket(e);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                this.CloseClientSocket(e);
            }
        }
        #endregion

        #region Protected Virtual Method : void ProcessSend(SocketAsyncEventArgs e)
        /// <summary>當完成傳送資料時，將呼叫此函示</summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send operation.</param>
        protected virtual void ProcessSend(SocketAsyncEventArgs e)
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
                        this.CloseClientSocket(e);
                        return;
                    }
                    int count = e.BytesTransferred;
                    Interlocked.Add(ref ac.m_SendByteCount, count);
                    Interlocked.Add(ref ac.m_WaittingSend, -count);
                    if (!m_IsShutdown && !m_IsDisposed)
                    {
                        m_Counters[ServerCounterType.TotalSendedBytes]?.IncrementBy(count);
                        m_Counters[ServerCounterType.RateOfSendedBytes]?.IncrementBy(count);
                        m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(-count);
                    }

                    byte[] buffer = new byte[count];
                    Array.Copy(e.Buffer, buffer, count);

                    this.OnDataSended(ac, buffer);
                }
                else
                    this.ProcessError(e);
            }
            else
            {
                this.CloseClientSocket(e);
            }
        }
        #endregion

        #region Protected Virtual Method : void ProcessError(SocketAsyncEventArgs e)
        /// <summary>當發生錯誤時，將呼叫此函示，並關閉客戶端</summary>
        /// <param name="e">發生錯誤的 SocketAsyncEventArgs 物件</param>
        protected virtual void ProcessError(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            Socket s = token.Client;
            AsyncClient ac = null;
            int index = Thread.CurrentThread.ManagedThreadId;
            if (s != null)
            {
                try
                {
                    ac = m_Clients[s.RemoteEndPoint.ToString()];

                    #region 產生事件 - OnException
                    IPEndPoint localEp = (IPEndPoint)s.LocalEndPoint;
                    SocketException se = new SocketException((Int32)e.SocketError);
                    Exception ex = new Exception(string.Format("客戶端連線({1})發生錯誤:{0},狀態:{2}", (int)e.SocketError, localEp, e.LastOperation), se);
                    this.OnException(ac, ex);
                    #endregion

                    this.CloseClientSocket(e);
                }
                catch (ObjectDisposedException) { }
                catch (Exception) { }   // 如果客戶端已關閉則不處理
            }
        }
        #endregion

        #region Private Method : void CloseClientSocket(SocketAsyncEventArgs e)
        /// <summary>關閉客戶端連線</summary>
        /// <param name="e">需處理的 SocketAsyncEventArg 物件</param>
        protected void CloseClientSocket(SocketAsyncEventArgs e)
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
                        exists = m_Clients.TryRemove(remote.ToString(), out ac);
                    }
                    catch (ObjectDisposedException) { }

                    try { s.Shutdown(SocketShutdown.Both); }
                    catch { }   // 如果客戶端已關閉，則不需要將錯誤丟出
                    finally
                    {
                        try { s.Close(); }
                        catch { }
                        finally { s = null; }
                    }
                    if (ac != null && remote != null)
                    {
                        if (m_WaitToClean.ContainsKey(remote))
                        {
                            m_WaitToClean.TryRemove(remote, out int v);
                        }
                    }
                    if (!m_IsShutdown && !m_IsDisposed)
                        m_Counters[ServerCounterType.Connections]?.Decrement();
                    try
                    {
                        m_MaxClients.Release();
                    }
                    catch (SemaphoreFullException) { }
                    catch (NullReferenceException) { }
                    try
                    {
                        if (ac != null)
                            ac.Dispose();
                        token.Dispose();
                    }
                    catch (Exception ex) { Debug.Print(ex.Message); }
                    e.UserToken = null;

                    #region 回收 SocketAsyncEventArgs 物件
                    if (!m_IsShutdown)
                    {
                        if (!m_IsDisposed)
                            m_Counters[ServerCounterType.PoolUsed]?.Decrement();
                        m_Pool.Push(e);
                    }
                    #endregion
                }
                catch (Exception ex) { Debug.Print(ex.Message); }
            }
        }
        #endregion

        #region Protected Virtual Method : void RecyclingSocket(IntPtr origHandle, EndPoint remote, SocketAsyncEventArgs e)
        /// <summary>回收客戶端連線</summary>
        /// <param name="origHandle">原始控制代碼</param>
        /// <param name="remote">原始遠端資訊</param>
        /// <param name="e">需處理的 SocketAsyncEventArg 物件</param>
        protected virtual void RecyclingSocket(IntPtr origHandle, EndPoint remote, SocketAsyncEventArgs e)
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
                if (remote != null)
                {
                    #region 如連線還存在，則關閉連線
                    if (ac != null && ac.Socket != null)
                    {
                        Socket s = ac.Socket;
                        try { s.Shutdown(SocketShutdown.Both); }
                        catch { }
                        try { s.Close(); }
                        catch { }
                    }
                    #endregion

                    if (m_WaitToClean.ContainsKey(remote))
                    {
                        m_WaitToClean.TryRemove(remote, out int v);
                    }
                }
                if (!m_IsShutdown && !m_IsDisposed)
                    m_Counters[ServerCounterType.Connections]?.Decrement();
                try
                {
                    m_MaxClients.Release();
                }
                catch (SemaphoreFullException) { }
                catch (NullReferenceException) { }

                #region 回收 SocketAsyncEventArgs 物件
                if (!m_IsShutdown)
                {
                    if (!m_IsDisposed)
                        m_Counters[ServerCounterType.PoolUsed]?.Decrement();
                    m_Pool.Push(e);
                }
                #endregion

                this.OnClientClosed(origHandle, remote4Callback);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
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
            catch (Exception ex) { Debug.Print(ex.Message); }
        }
        #endregion

        #region IDisposable
        /// <summary>清除並釋放 AsyncServer 所使用的資源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>清除並釋放 AsyncServer 所使用的資源。</summary>
        /// <param name="disposing">是否確實清除與釋放。</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual void Dispose(bool disposing)
        {
            if (m_IsDisposed) return;
            if (disposing)
            {
                try
                {
                    SocketAsyncEventArgs arg = null;
                    if (m_Pool != null)
                    {
                        arg = m_Pool.Pop();
                        arg.Dispose();
                        arg = null;
                        m_Pool.Clear();
                    }
                    m_Counters.Clear();
                    m_Counters = null;
                    m_Pool = null;
                    if (m_Clients != null)
                        m_Clients.Clear();
                    m_Clients = null;
                    if (m_WaitToClean != null)
                        m_WaitToClean.Clear();
                    m_WaitToClean = null;
                    m_MaxClients.Close();
                    m_MaxClients = null;
                    m_Mutex.Dispose();
                    m_Mutex = null;
                    if (m_ListenSocket != null)
                    {
                        try { m_ListenSocket.Shutdown(SocketShutdown.Both); }
                        catch { }
                        finally
                        {
                            try { m_ListenSocket.Close(); }
                            catch (Exception ex) { Debug.Print(ex.Message); }
                            finally { m_ListenSocket = null; }
                        }
                    }
                    m_LocalEndPort = null;
                }
                catch { }
            }
            m_IsDisposed = true;
        }
        #endregion

        #region Internal Method : void CleanInvalidClients(object o)
        internal void CleanInvalidClients(object o)
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
                if (m_Clients != null)
                {
                    List<AsyncClient> lac = new List<AsyncClient>();
                    string[] eps = new string[m_Clients.Keys.Count];
                    m_Clients.Keys.CopyTo(eps, 0);
                    AsyncClient ac = null;
                    foreach (string ep in eps)
                    {
                        if (m_Clients[ep] == null || m_Clients[ep].Socket == null
                             || !m_Clients[ep].IsConnected)
                        {
                            m_Clients.TryRemove(ep, out ac);
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
                if (!m_IsShutdown && !m_IsDisposed)
                    m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(e.Data.Length);
            }
            catch (Exception ex) { Debug.Print(ex.Message); }
        }
        #endregion

        #region Private Method : void ac_OnDataSended(object sender, AsyncClientEventArgs e)
        private void ac_OnDataSended(object sender, AsyncClientEventArgs e)
        {
            try
            {
                int count = e.Data.Length;
                if (!m_IsShutdown && !m_IsDisposed)
                {
                    m_Counters[ServerCounterType.TotalSendedBytes]?.IncrementBy(count);
                    m_Counters[ServerCounterType.RateOfSendedBytes]?.IncrementBy(count);
                    m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(-count);
                }
            }
            catch (Exception ex) { Debug.Print(ex.Message); }

            AsyncClient ac = (AsyncClient)sender;
            this.OnDataSended(ac, e.Data, e.ExtraInfo);
        }
        #endregion

        #region Private Method : void ac_OnSendedFail(object sender, AsyncClientEventArgs e)
        private void ac_OnSendedFail(object sender, AsyncClientEventArgs e)
        {
            try
            {
                if (!m_IsShutdown && !m_IsDisposed)
                {
                    m_Counters[ServerCounterType.SendFail]?.Increment();
                    m_Counters[ServerCounterType.RateOfSendFail]?.Increment();
                    m_Counters[ServerCounterType.BytesOfSendQueue]?.IncrementBy(-e.Data.Length);
                }
            }
            catch (Exception ex) { Debug.Print(ex.Message); }

            AsyncClient ac = (AsyncClient)sender;
            this.OnSendedFail(ac, e.Data, e.ExtraInfo);
        }
        #endregion

        #region Private Method : void ac_OnClosing(object sender, AsyncClientEventArgs e)
        private void ac_OnClosing(object sender, AsyncClientEventArgs e)
        {
            AsyncClient ac = (AsyncClient)sender;
            if (ac != null)
                m_Clients.TryRemove(ac.RemoteEndPoint.ToString(), out _);
            this.OnClientClosing(ac, e.ClosedByIdle, e.ExtraInfo);
        }
        #endregion

        #region Private Method : void ac_OnClosed(object sender, AsyncClientEventArgs e)
        private void ac_OnClosed(object sender, AsyncClientEventArgs e)
        {
            AsyncClient ac = (AsyncClient)sender;
            if (ac != null)
                m_Clients.TryRemove(ac.RemoteEndPoint.ToString(), out _);
            this.OnClientClosed(ac, e.ClosedByIdle, e.ExtraInfo);
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
            catch (Exception ex) { Debug.Print(ex.Message); }
        }
        #endregion
    }
}


