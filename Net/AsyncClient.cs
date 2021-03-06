﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System.Management;
using CJF.Utility;
using CJF.Utility.Extensions;

#pragma warning disable IDE0019
#pragma warning disable IDE0044
namespace CJF.Net
{
    /// <summary>非同步 TCP 連線用戶端，使用xxxAsync</summary>
    [Serializable]
    public class AsyncClient : IDisposable
    {
        [System.Runtime.InteropServices.DllImport("Iphlpapi.dll", EntryPoint = "SendARP")]
        internal extern static int SendArp(int destIpAddress, int srcIpAddress, byte[] macAddress, ref int macAddressLength);

        private AutoResetEvent m_ResetEvent = null;

        bool _IsDisposed = false;
        SocketAsyncEventArgs m_AsyncArg = null;
        int m_SendBufferSize = 2048;
        int m_ReceiveBufferSize = 2048;
        [NonSerialized]
        Socket m_Socket = null;
        IntPtr m_Handle = IntPtr.Zero;
        bool m_Connected = false;
        [NonSerialized]
        Timer m_SecondCounter = null;
        bool m_ExecutingCounter = false;
        DateTime m_LastAction = DateTime.MinValue;

        internal long m_SendByteCount = 0;
        internal long m_ReceiveByteCount = 0;
        internal long m_WaittingSend = 0;

        #region Event Handler
        /// <summary>當資料送達至遠端時產生</summary>
        public event EventHandler<AsyncClientEventArgs> DataSended;
        /// <summary>當接收到遠端資料時產生<br />勿忘處理黏包、斷包的狀況</summary>
        public event EventHandler<AsyncClientEventArgs> DataReceived;
        /// <summary>當與遠端連線時產生</summary>
        public event EventHandler<AsyncClientEventArgs> Connected;
        /// <summary>當連線準備關閉時產生</summary>
        public event EventHandler<AsyncClientEventArgs> Closing;
        /// <summary>當連線完全關閉時產生</summary>
        public event EventHandler<AsyncClientEventArgs> Closed;
        /// <summary>當發生錯誤時產生</summary>
        public event EventHandler<AsyncClientEventArgs> Exception;
        /// <summary>當每秒傳送計數器值變更時產生</summary>
        public event EventHandler<DataTransEventArgs> CounterChanged;
        /// <summary>當資料發送至遠端前產生</summary>
        public event EventHandler<AsyncClientEventArgs> BeforeSend;
        /// <summary>當資料無法發送至遠端時產生</summary>
        public event EventHandler<AsyncClientEventArgs> SendFail;
        #endregion

        #region Construct Methods : AsyncClient(...)
        /// <summary>建立一個新的 AsyncClient</summary>
        public AsyncClient()
        {
            this.RemoteEndPoint = null;
            this.RemoteMacAddress = "00:00:00:00:00:00";
            this.LocalEndPoint = null;
            m_Socket = null;
            CommonSettings();
        }
        /// <summary>利用現有連線建立一個新的 AsyncClient</summary>
        internal AsyncClient(Socket socket)
        {
            m_Socket = socket ?? throw new ArgumentNullException("socket 參數不允許 null 值");
            m_Handle = socket.Handle;
            IPEndPoint ipp = null;
            ipp = (IPEndPoint)socket.RemoteEndPoint;
            this.RemoteEndPoint = new IPEndPoint(ipp.Address, ipp.Port);
            int count = 0;
            while (string.IsNullOrEmpty(this.RemoteMacAddress) && count < 3)
            {
                try { this.RemoteMacAddress = GetDestinationMacAddress(ipp.Address); }
                catch (ArgumentException) { this.RemoteMacAddress = null; }
                if (this.RemoteMacAddress == null)
                {
                    count++;
                    Thread.Sleep(100);
                }
            }
            ipp = (IPEndPoint)socket.LocalEndPoint;
            this.LocalEndPoint = new IPEndPoint(ipp.Address, ipp.Port);
            m_Connected = m_Socket.Connected;
            CommonSettings();
            m_Socket.SendBufferSize = m_SendBufferSize;
            m_Socket.ReceiveBufferSize = m_ReceiveBufferSize;
            m_AsyncArg = new SocketAsyncEventArgs();
            m_AsyncArg.UserToken = new AsyncUserToken(m_Socket, this.ReceiveBufferSize);
            m_AsyncArg.RemoteEndPoint = this.RemoteEndPoint;
            m_AsyncArg.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessConnect);
        }
        /// <summary>建立一個新的 AsyncClient</summary>
        /// <param name="ip">遠端伺服器 IP</param>
        /// <param name="port">連接的通訊埠號</param>
        public AsyncClient(string ip, int port)
        {
            this.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            this.LocalEndPoint = new IPEndPoint(GetHostIP(), 0);
            CommonSettings();
        }
        /// <summary>建立一個新的 SocketClient</summary>
        /// <param name="ipPort">遠端伺服器端點資訊</param>
        public AsyncClient(IPEndPoint ipPort)
        {
            this.RemoteEndPoint = new IPEndPoint(ipPort.Address, ipPort.Port);
            this.LocalEndPoint = new IPEndPoint(GetHostIP(), 0);
            CommonSettings();
        }
        /// <summary>釋放 AsyncClient 所使用的資源。 </summary>
        ~AsyncClient() { Dispose(false); }
        #endregion

        #region Properties
        /// <summary>取得目前是否連線中</summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public bool IsConnected
        {
            get
            {
                if (_IsDisposed)
                    return false;
                //throw new ObjectDisposedException(this.GetType().ToString(), "物件已被 Dispose");
                try
                {
                    if (m_Socket != null && !m_Socket.Connected)
                        m_Connected = IsSocketConnected();
                    return m_Connected;
                }
                catch (ObjectDisposedException) { return false; }
            }
        }
        /// <summary>取得或設定連線逾時時間, 單位豪秒(ms)</summary>
        public int ConnectTimeout { get; set; }
        /// <summary>取得 Socket 的控制代碼</summary>
        public IntPtr Handle
        {
            get
            {
                IntPtr handle = m_Handle;
                try
                {
                    if (m_Socket != null)
                    {
                        handle = m_Socket.Handle;
                        if (!handle.Equals(m_Handle))
                            m_Handle = handle;
                    }
                }
                catch (Exception) { }
                finally { }
                return handle;
            }
        }
        /// <summary>取得通訊端介面</summary>
        public Socket Socket { get { return m_Socket; } }
        /// <summary>遠端端點位址資訊</summary>
        public IPEndPoint RemoteEndPoint { get; private set; }
        /// <summary>本地端端點位址資訊</summary>
        public IPEndPoint LocalEndPoint { get; private set; }
        /// <summary>遠端端點位址資訊</summary>
        public string RemoteMacAddress { get; private set; }
        /// <summary>取得或設定額外專屬的自訂值</summary>
        public object ExtraInfo { get; set; }
        /// <summary>取得或設定額外專屬的自訂值</summary>
        public object TagInfo { get; set; }
        /// <summary>設定或取得接收緩衝區大小，單位:位元組，預設值:2048</summary>
        /// <exception cref="ArgumentOutOfRangeException">值必須大於512Bytes</exception>
        public int ReceiveBufferSize
        {
            get { return m_ReceiveBufferSize; }
            set
            {
                if (value < 512)
                    throw new ArgumentOutOfRangeException("ReceiveBufferSize", value, "值必須大於512Bytes");
                m_ReceiveBufferSize = value;
                if (m_Socket != null)
                    m_Socket.ReceiveBufferSize = m_ReceiveBufferSize;
            }
        }
        /// <summary>設定或取得發送緩衝區大小，單位:位元組，預設值:2048</summary>
        /// <exception cref="ArgumentOutOfRangeException">值必須大於512Bytes</exception>
        public int SendBufferSize
        {
            get { return m_SendBufferSize; }
            set
            {
                if (value < 512)
                    throw new ArgumentOutOfRangeException("SendBufferSize", value, "值必須大於512Bytes");
                m_SendBufferSize = value;
                if (m_Socket != null)
                    m_Socket.SendBufferSize = m_SendBufferSize;
            }
        }
        /// <summary>設定或取得使用的字元編碼</summary>
        public Encoding Encoding { get; set; }
        /// <summary>取得每秒的接收量，單位:位元組</summary>
        public long ReceiveSpeed { get { return Interlocked.Read(ref m_ReceiveByteCount); } }
        /// <summary>取得每秒發送量，單位:位元組</summary>
        public long SendSpeed { get { return Interlocked.Read(ref m_SendByteCount); } }
        /// <summary>取得現在等待發送的資料量，單位:位元組</summary>
        public long WaittingSend { get { return Interlocked.Read(ref m_WaittingSend); } }
        /// <summary>取得值，是否已Disposed</summary>
        public bool IsDisposed { get { return _IsDisposed; } }
        /// <summary>取得與設定，是否使用非同步方式產生回呼事件</summary>
        public EventCallbackMode UseAsyncCallback { get; set; }
        /// <summary>設定或取得無動作自動斷線的時間，單位秒，0表示不檢查，預設為0</summary>
        public uint AutoClose { get; set; }
        /// <summary>取得距離前次操作的時間，單位秒</summary>
        public long IdleTime { get { return (this.AutoClose == 0) ? 0 : (long)DateTime.Now.Subtract(m_LastAction).TotalSeconds; } }
        #endregion

        #region Public Method : void Connect()
        /// <summary>連線至遠端伺服器</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Net.Sockets.Socket.#ConnectAsync(System.Net.Sockets.SocketAsyncEventArgs)")]
        public void Connect()
        {
            if (m_Connected)
                throw new SocketException((int)SocketError.IsConnected);
            if (m_ResetEvent == null)
                m_ResetEvent = new AutoResetEvent(false);
            if (m_Socket == null)
            {
                m_Socket = new Socket(this.RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                SetSocketOptions();
                m_Handle = m_Socket.Handle;
                m_Socket.SendBufferSize = m_SendBufferSize;
                m_Socket.ReceiveBufferSize = m_ReceiveBufferSize;
            }

            m_SendByteCount = 0;
            m_ReceiveByteCount = 0;
            m_AsyncArg = new SocketAsyncEventArgs();
            m_AsyncArg.UserToken = new AsyncUserToken(m_Socket, this.ReceiveBufferSize, this.ExtraInfo);
            m_AsyncArg.RemoteEndPoint = this.RemoteEndPoint;
            m_AsyncArg.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessConnect);
            m_Socket.ConnectAsync(m_AsyncArg);
            bool back = m_ResetEvent.WaitOne(this.ConnectTimeout, true);
            if (!back)
            {
                SocketError se = SocketError.Success;
                try
                {
                    m_Socket.Close();
                    se = m_AsyncArg.SocketError;
                    m_Socket.Shutdown(SocketShutdown.Both);
                }
                catch (ObjectDisposedException) { }
                if (se == SocketError.Success && m_AsyncArg.ConnectSocket == null)
                    throw new SocketException((int)SocketError.TimedOut);
                else
                    throw new SocketException((int)m_AsyncArg.SocketError);
            }
            else if (m_AsyncArg.SocketError != SocketError.Success && m_AsyncArg.ConnectSocket == null)
                throw new SocketException((int)m_AsyncArg.SocketError);
        }
        #endregion

        #region Public Method : void Connect(string ip, int port)
        /// <summary>連線至遠端伺服器</summary>
        /// <param name="ip">遠端伺服器 IP</param>
        /// <param name="port">連接的通訊埠號</param>
        public void Connect(string ip, int port)
        {
            this.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Connect();
        }
        #endregion

        #region Public Method : void Connect(IPEndPoint epServer)
        /// <summary>連線至遠端伺服器</summary>
        /// <param name="epServer">遠端伺服器的端點資料</param>
        public void Connect(IPEndPoint epServer)
        {
            this.RemoteEndPoint = epServer;
            this.Connect();
        }
        #endregion

        #region Public Method :  void Close()
        /// <summary>關閉與遠端伺服器的連線</summary>
        public void Close()
        {
            Close(false);
        }
        #endregion

        #region Private Method : void Close(bool byIdle)
        /// <summary>關閉與遠端伺服器的連線</summary>
        /// <param name="byIdle">是否為Idle所關閉的</param>
        private void Close(bool byIdle)
        {
            if (_IsDisposed) return;
            m_LastAction = DateTime.Now;
            if (m_ResetEvent != null)
                m_ResetEvent.Close();
            bool isNull = false;
            if (m_Socket == null)
                return;
            this.OnClosing(this.ExtraInfo);
            try
            {
                m_Socket.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException) { isNull = true; }
            catch (NullReferenceException) { isNull = true; }
            catch (SocketException) { isNull = true; }
            if (!isNull)
            {
                try
                {
                    m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                    m_Socket.Close();
                }
                catch (ObjectDisposedException) { isNull = true; }
                catch (NullReferenceException) { isNull = true; }
                catch (SocketException) { isNull = true; }
            }
            m_Connected = false;
            SecondCounterCallback(null);
            try
            {
                if (m_SecondCounter != null)
                    m_SecondCounter.Dispose();
            }
            catch { }
            finally { m_SecondCounter = null; }
            m_ReceiveByteCount = 0;
            m_SendByteCount = 0;
            try
            {
                if (m_AsyncArg != null)
                    m_AsyncArg.Dispose();
            }
            catch { }
            finally { m_AsyncArg = null; }

            if (!isNull)
                this.OnClosed(byIdle, this.ExtraInfo);
        }
        #endregion

        #region Protected Virtual Method : void OnDataSended(byte[] buffer, object extraInfo = null)
        /// <summary>產生 DataSended 事件</summary>
        /// <param name="buffer">已發送的資料內容</param>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnDataSended(byte[] buffer, object extraInfo = null)
        {
            if (buffer.Length != 0 && this.DataSended != null)
            {
                AsyncClientEventArgs acea = new AsyncClientEventArgs(this.Handle, this.LocalEndPoint, this.RemoteEndPoint, buffer, null);
                acea.SetExtraInfo(extraInfo);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncClientEventArgs> del in this.DataSended.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, acea, new AsyncCallback(AsyncClientEventCallback), del); }
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
                                try { del.DynamicInvoke(this, acea); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, acea } };
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

        #region Protected Virtual Method : void OnDataReceived(byte[] buffer, object extraInfo = null)
        /// <summary>產生 DataReceived 事件</summary>
        /// <param name="buffer">已接收的資料</param>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnDataReceived(byte[] buffer, object extraInfo = null)
        {
            if (buffer.Length != 0 && this.DataReceived != null)
            {
                AsyncClientEventArgs acea = new AsyncClientEventArgs(this.Handle, this.LocalEndPoint, this.RemoteEndPoint, buffer, null);
                acea.SetExtraInfo(extraInfo);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncClientEventArgs> del in this.DataReceived.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, acea, new AsyncCallback(AsyncClientEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.DataReceived.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, acea); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, acea } };
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

        #region Protected Virtual Method : void OnConnected(IntPtr handle, object extraInfo = null)
        /// <summary>產生 Connected 事件</summary>
        /// <param name="handle">Socket 控制代碼</param>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnConnected(IntPtr handle, object extraInfo = null)
        {
            if (this.Connected != null)
            {
                AsyncClientEventArgs acea = new AsyncClientEventArgs(handle, this.LocalEndPoint, this.RemoteEndPoint);
                acea.SetExtraInfo(extraInfo);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncClientEventArgs> del in this.Connected.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, acea, new AsyncCallback(AsyncClientEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.Connected.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, acea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Thread:
                        #region 建立執行緒 - Thread
                        {
                            foreach (Delegate del in this.Connected.GetInvocationList())
                            {
                                try
                                {
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, acea } };
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

        #region Protected Virtual Method : void OnClosing(object extraInfo = null)
        /// <summary>產生 Closing 事件</summary>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnClosing(object extraInfo = null)
        {
            if (this.Closing != null)
            {
                AsyncClientEventArgs acea = new AsyncClientEventArgs(this.Handle, this.LocalEndPoint, this.RemoteEndPoint);
                acea.SetExtraInfo(extraInfo);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncClientEventArgs> del in this.Closing.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, acea, new AsyncCallback(AsyncClientEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.Closing.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, acea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Thread:
                        #region 建立執行緒 - Thread
                        {
                            foreach (Delegate del in this.Closing.GetInvocationList())
                            {
                                try
                                {
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, acea } };
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

        #region Protected Virtual Method : void OnClosed(bool byIdle = false, object extraInfo = null)
        /// <summary>產生 Closed 事件</summary>
        /// <param name="byIdle">是否為閒置關閉</param>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnClosed(bool byIdle = false, object extraInfo = null)
        {
            if (this.Closed != null)
            {
                AsyncClientEventArgs acea = new AsyncClientEventArgs(this.Handle, this.LocalEndPoint, this.RemoteEndPoint);
                acea.ClosedByIdle = byIdle;
                acea.SetExtraInfo(extraInfo);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncClientEventArgs> del in this.Closed.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, acea, new AsyncCallback(AsyncClientEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.Closed.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, acea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Thread:
                        #region 建立執行緒 - Thread
                        {
                            foreach (Delegate del in this.Closed.GetInvocationList())
                            {
                                try
                                {
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, acea } };
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

        #region Protected Virtual Method : void OnException(Exception e, object extraInfo = null)
        /// <summary>產生 Exception 事件</summary>
        /// <param name="e">錯誤資訊類別</param>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnException(Exception e, object extraInfo = null)
        {
            if (!_IsDisposed && this.Exception != null)
            {
                AsyncClientEventArgs acea = new AsyncClientEventArgs(this.Handle, this.LocalEndPoint, this.RemoteEndPoint, null, e);
                acea.SetExtraInfo(extraInfo);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步回呼 - DynamicInvoke
                        {
                            foreach (EventHandler<AsyncClientEventArgs> del in this.Exception.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, acea, new AsyncCallback(AsyncClientEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步回呼 - DynamicInvoke
                        {
                            foreach (Delegate del in this.Exception.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, acea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
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
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, acea } };
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

        #region Protected Virtual Method : void OnCounterChanged(long sBytes, long rBytes, long wBytes)
        /// <summary>產生系統效能監視器 CounterChanged 事件</summary>
        /// <param name="sBytes">每秒接收的位元組數量</param>
        /// <param name="rBytes">每秒發送的位元組數量</param>
        /// <param name="wBytes">等待發送的位元組數量</param>
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
                                catch (Exception ex) { Debug.Print(ex.Message); }
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
                                catch (Exception ex) { Debug.Print(ex.Message); }
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
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                        #endregion
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnBeforeSend(byte[] data, object extraInfo = null)
        /// <summary>產生 BeforeSend 事件</summary>
        /// <param name="data">原始資料</param>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnBeforeSend(byte[] data, object extraInfo = null)
        {
            if (this.BeforeSend != null)
            {
                try
                {
                    AsyncClientEventArgs acea = new AsyncClientEventArgs(this.Handle, this.LocalEndPoint, this.RemoteEndPoint, data, null);
                    acea.SetExtraInfo(extraInfo);
                    switch (this.UseAsyncCallback)
                    {
                        case EventCallbackMode.BeginInvoke:
                            #region 非同步作法 - BeginInvoke
                            {
                                foreach (EventHandler<AsyncClientEventArgs> del in this.BeforeSend.GetInvocationList())
                                {
                                    try { del.BeginInvoke(this, acea, new AsyncCallback(AsyncClientEventCallback), del); }
                                    catch (Exception ex) { Debug.Print(ex.Message); }
                                }
                                break;
                            }
                        #endregion
                        case EventCallbackMode.Invoke:
                            #region 同步作法 - DynamicInvoke
                            {
                                foreach (Delegate del in this.BeforeSend.GetInvocationList())
                                {
                                    try { del.DynamicInvoke(this, acea); }
                                    catch (Exception ex) { Debug.Print(ex.Message); }
                                }
                                break;
                            }
                        #endregion
                        case EventCallbackMode.Thread:
                            #region 建立執行緒 - Thread
                            {
                                foreach (Delegate del in this.BeforeSend.GetInvocationList())
                                {
                                    try
                                    {
                                        EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, acea } };
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(EventThreadWorker), etv);
                                    }
                                    catch (Exception ex) { Debug.Print(ex.Message); }
                                }
                                break;
                            }
                            #endregion
                    }
                }
                catch (Exception ex) { Debug.Print(ex.Message); }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnSendFail(byte[] data, object extraInfo = null)
        /// <summary>產生 SendFail 事件</summary>
        /// <param name="data">原始資料</param>
        /// <param name="extraInfo">額外資訊</param>
        protected virtual void OnSendFail(byte[] data, object extraInfo = null)
        {
            if (this.SendFail != null)
            {
                AsyncClientEventArgs acea = new AsyncClientEventArgs(this.Handle, this.LocalEndPoint, this.RemoteEndPoint, data, null);
                acea.SetExtraInfo(extraInfo);
                switch (this.UseAsyncCallback)
                {
                    case EventCallbackMode.BeginInvoke:
                        #region 非同步呼叫 - BeginInvoke
                        {
                            foreach (EventHandler<AsyncClientEventArgs> del in this.SendFail.GetInvocationList())
                            {
                                try { del.BeginInvoke(this, acea, new AsyncCallback(AsyncClientEventCallback), del); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Invoke:
                        #region 同步呼叫 - DynamicInvoke
                        {
                            foreach (Delegate del in this.SendFail.GetInvocationList())
                            {
                                try { del.DynamicInvoke(this, acea); }
                                catch (Exception ex) { Debug.Print(ex.Message); }
                            }
                            break;
                        }
                    #endregion
                    case EventCallbackMode.Thread:
                        #region 建立執行緒 - Thread
                        {
                            foreach (Delegate del in this.SendFail.GetInvocationList())
                            {
                                try
                                {
                                    EventThreadVariables etv = new EventThreadVariables() { InvokeMethod = del, Args = new object[] { this, acea } };
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

        #region Public Virtual Method : void SendData(string data, Encoding encode)
        /// <summary>傳送資料至遠端伺服器</summary>
        /// <param name="data">欲傳送的字串資料</param>
        /// <param name="encode">編碼原則</param>
        public virtual void SendData(string data, Encoding encode)
        {
            SendData(encode.GetBytes(data));
        }
        #endregion

        #region Public Virtual Method : void SendData(string data)
        /// <summary>傳送資料至遠端伺服器</summary>
        /// <param name="data">欲傳送的字串資料</param>
        public virtual void SendData(string data)
        {
            this.SendData(this.Encoding.GetBytes(data));
        }
        #endregion

        #region Public Virtual Method : void SendData(byte[] data, object extraInfo = null)
        /// <summary>傳送資料至遠端伺服器</summary>
        /// <param name="data">欲傳送的位元組資料</param>
        /// <param name="extraInfo">需額外傳遞的資訊</param>
        public virtual void SendData(byte[] data, object extraInfo = null)
        {
            if (this.IsConnected && m_Socket.Connected)
            {
                m_LastAction = DateTime.Now;
                bool success = false;
                Interlocked.Add(ref m_WaittingSend, data.Length);
                this.OnBeforeSend(data, extraInfo ?? (this.ExtraInfo ?? null));

                SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
                arg.SetBuffer(data, 0, data.Length);
                arg.UserToken = new AsyncUserToken(m_Socket, this.SendBufferSize, extraInfo);
                arg.RemoteEndPoint = this.RemoteEndPoint;
                arg.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessSend);
                SocketException sex = null;
                try
                {
                    if (!m_Socket.SendAsync(arg))
                        ProcessSend(this, arg);
                    success = true;
                }
                catch (ObjectDisposedException) { }
                catch (SocketException se) { sex = se; }
                catch (Exception ex) { Debug.Print(ex.Message); }
                if (!success)
                {
                    Interlocked.Add(ref m_WaittingSend, -data.Length);
                    this.OnSendFail(data, extraInfo ?? (this.ExtraInfo ?? null));
                }
                if (sex != null)
                    this.OnException(sex, extraInfo ?? (this.ExtraInfo ?? null));
            }
            else
            {
                // throw new SocketException((Int32)SocketError.NotConnected);
                this.OnSendFail(data, extraInfo ?? (this.ExtraInfo ?? null));
            }
        }
        #endregion

        #region Public Method : void ResetIdleTime()
        /// <summary>重新設置未操作逾時時間</summary>
        public void ResetIdleTime()
        {
            m_LastAction = DateTime.Now;
        }
        #endregion

        #region Delegate Callback Methods
        private void AsyncClientEventCallback(IAsyncResult result)
        {
            try
            {
                EventHandler<AsyncClientEventArgs> del = result.AsyncState as EventHandler<AsyncClientEventArgs>;
                del.EndInvoke(result);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex) { Debug.Print(ex.Message); }
        }
        private void TransferCounterCallback(IAsyncResult result)
        {
            try
            {
                EventHandler<DataTransEventArgs> del = result.AsyncState as EventHandler<DataTransEventArgs>;
                del.EndInvoke(result);
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex) { Debug.Print(ex.Message); }
        }
        #endregion

        #region Private Method : void CommonSettings()
        private void CommonSettings()
        {
            this.UseAsyncCallback = EventCallbackMode.Invoke;
            this.ConnectTimeout = 5000;
            this.Encoding = Encoding.Default;
            this.AutoClose = 0;
            ResetIdleTime();
            SetSocketOptions();
        }
        #endregion

        #region Public Method : void Dispose()
        /// <summary>釋放 AsyncClient 所使用的資源。 </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Protected Virtual Method : void Dispose(bool disposing)
        /// <summary>釋放 AsyncClient 所使用的資源。 </summary>
        /// <param name="disposing">是否完全釋放</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_IsDisposed) return;
            if (disposing)
            {
                if (m_SecondCounter != null)
                {
                    m_SecondCounter.Dispose();
                    m_SecondCounter = null;
                }
                if (m_AsyncArg != null)
                {
                    m_AsyncArg.Dispose();
                    m_AsyncArg = null;
                }
                try
                {
                    m_Socket.Shutdown(SocketShutdown.Both);
                }
                catch (ObjectDisposedException) { }
                catch (Exception) { } // throws if client process has already closed
                finally
                {
                    try
                    {
                        if (m_Socket != null)
                        {
                            m_Socket.Close();
                            m_Socket.Dispose();
                            m_Socket = null;
                        }
                    }
                    catch { }
                    try { m_ResetEvent.Dispose(); }
                    finally { }
                }
                if (m_ResetEvent != null)
                    m_ResetEvent.Close();
                m_ResetEvent = null;
                m_Handle = IntPtr.Zero;
            }
            _IsDisposed = true;
        }
        #endregion

        #region Private Method : void SecondCounterCallback(object o)
        private void SecondCounterCallback(object o)
        {
            if (_IsDisposed || m_ExecutingCounter) return;
            m_ExecutingCounter = true;
            long sBytes = Interlocked.Read(ref m_SendByteCount);
            Interlocked.Exchange(ref m_SendByteCount, 0);
            long rBytes = Interlocked.Read(ref m_ReceiveByteCount);
            Interlocked.Exchange(ref m_ReceiveByteCount, 0);
            long wBytes = Interlocked.Read(ref m_WaittingSend);

            this.OnCounterChanged(sBytes, rBytes, wBytes);

            if (this.AutoClose != 0 && this.IdleTime >= this.AutoClose)
                this.Close(true);

            m_ExecutingCounter = false;
        }
        #endregion

        #region Private Method : void ProcessError(SocketAsyncEventArgs e)
        private void ProcessError(SocketAsyncEventArgs e)
        {
            if (e == null || _IsDisposed) return;
            Socket s = null;
            AsyncUserToken token = null;
            if (e.UserToken.GetType().Equals(typeof(AsyncUserToken)))
            {
                token = e.UserToken as AsyncUserToken;
                s = token.Client;
            }
            else if (e.UserToken.GetType().Equals(typeof(Socket)))
                s = e.UserToken as Socket;
            else
            {
                this.OnException(new Exception("未知的類別物件：" + e.UserToken.GetType().ToString()), token.ExtraInfo ?? (this.ExtraInfo ?? null));
            }

            #region 關閉連線
            if (!_IsDisposed && s != null && s.Connected)
            {
                try { s.Shutdown(SocketShutdown.Both); }
                catch (Exception) { }   // 如果遠端伺服器已關閉則不處理
                finally
                {
                    try
                    {
                        if (s != null)
                            s.Close();
                        s = null;
                    }
                    catch (Exception ex) { Debug.Print(ex.Message); }
                }
            }
            #endregion

            this.OnException(new SocketException((Int32)e.SocketError), token.ExtraInfo ?? (this.ExtraInfo ?? null));
        }
        #endregion

        #region Private Method : void ProcessConnect(object sender, SocketAsyncEventArgs e)
        private void ProcessConnect(object sender, SocketAsyncEventArgs e)
        {
            if (_IsDisposed) return;
            if (e.LastOperation != SocketAsyncOperation.Connect)
                return;
            m_LastAction = DateTime.Now;
            try
            {
                m_ResetEvent.Set();
                if (e.SocketError == SocketError.Success)
                {
                    AsyncUserToken token = e.UserToken as AsyncUserToken;
                    Socket s = token.Client;
                    this.LocalEndPoint = (IPEndPoint)s.LocalEndPoint;
                    int count = 0;
                    while (string.IsNullOrEmpty(this.RemoteMacAddress) && count < 3)
                    {
                        try { this.RemoteMacAddress = GetDestinationMacAddress(((IPEndPoint)e.RemoteEndPoint).Address); }
                        catch (ArgumentException) { this.RemoteMacAddress = null; }
                        if (this.RemoteMacAddress == null)
                        {
                            count++;
                            Thread.Sleep(100);
                        }
                    }
                    m_Connected = true;
                    this.OnConnected(s.Handle, this.ExtraInfo ?? (token?.ExtraInfo));

                    if (!m_Connected || _IsDisposed) return;

                    byte[] receiveBuffer = new byte[this.ReceiveBufferSize];
                    e.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
                    e.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessReceive);
                    try
                    {
                        if (!s.ReceiveAsync(e))
                            ProcessReceive(this, e);
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message);
                    }
                }
                else
                {
                    m_Connected = false;
                    ProcessError(e);
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                if (!_IsDisposed)
                    Debug.Print(ex.Message);
            }
        }
        #endregion

        #region Private Method : void ProcessReceive(object sender, SocketAsyncEventArgs e)
        private void ProcessReceive(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Receive || _IsDisposed || !m_Connected) return;
            m_LastAction = DateTime.Now;
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                {
                    try
                    {
                        int count = e.BytesTransferred;
                        Interlocked.Add(ref m_ReceiveByteCount, count);
                        AsyncUserToken token = e.UserToken as AsyncUserToken;
                        Socket s = token.Client;
                        List<byte> rec = new List<byte>();
                        if ((token.CurrentIndex + count) > token.Capacity)
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
                        this.OnDataReceived(rec.ToArray(), ExtraInfo ?? (token ?? null));
                        try
                        {
                            if (!s.ReceiveAsync(e))
                                ProcessReceive(this, e);
                        }
                        catch (ObjectDisposedException) { }
                        catch (Exception ex) { Debug.Print(ex.Message); }
                    }
                    catch (Exception ex) { Debug.Print(ex.Message); }
                }
                else
                    this.ProcessError(e);
            }
            else
            {
                this.Close();
            }
        }
        #endregion

        #region Private Method : void ProcessSend(object sender, SocketAsyncEventArgs e)
        private void ProcessSend(object sender, SocketAsyncEventArgs e)
        {
            m_LastAction = DateTime.Now;
            try
            {
                if (_IsDisposed || e.LastOperation != SocketAsyncOperation.Send || !this.IsConnected)
                {
                    Interlocked.Add(ref m_WaittingSend, -e.Buffer.Length);
                    AsyncUserToken token = (AsyncUserToken)e.UserToken;
                    this.OnSendFail(e.Buffer, (token != null && token.ExtraInfo != null) ? token.ExtraInfo : ExtraInfo ?? null);
                    return;
                }
            }
            catch (ObjectDisposedException) { return; }
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                {
                    int count = e.BytesTransferred;
                    Interlocked.Add(ref m_SendByteCount, count);
                    Interlocked.Add(ref m_WaittingSend, -count);
                    try
                    {
                        if (!_IsDisposed)
                        {
                            byte[] buffer = new byte[count];
                            Array.Copy(e.Buffer, buffer, count);
                            AsyncUserToken token = (AsyncUserToken)e.UserToken;
                            this.OnDataSended(buffer, ExtraInfo ?? (token ?? null));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message);
                        AsyncUserToken token = (AsyncUserToken)e.UserToken;
                        this.OnSendFail(e.Buffer, (token != null && token.ExtraInfo != null) ? token.ExtraInfo : ExtraInfo ?? null);
                    }
                }
                else
                    ProcessError(e);
            }
            e.Dispose();
            e = null;
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
            catch (Exception ex) { Debug.Print(ex.Message); }
        }
        #endregion

        #region Private Method : void SetSocketOptions()
        private void SetSocketOptions()
        {
            if (m_Socket != null)
            {
                m_Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                byte[] inOptionValues = new byte[4 * 3];
                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes((uint)60000).CopyTo(inOptionValues, 4);   // 空閒 60 秒
                BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, 8);    // 每 5 秒檢查一次
                m_Socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
                if (m_SecondCounter == null)
                    m_SecondCounter = new Timer(SecondCounterCallback, null, 0, 1000);
                else
                    m_SecondCounter.Change(0, 1000);
            }
        }
        #endregion

        #region Private Method : bool IsSocketConnected()
        /// <summary>當 Socket.Connected為false時，進一步確認當下狀態</summary>
        /// <returns></returns>
        private bool IsSocketConnected()
        {
            /********************************************************************************************
			 * Connected  屬性的值僅會反映最近一次作業的連接狀態。 
			 * 如果您需要判斷連接的目前狀態，請執行非封鎖、零位元組的 Send 呼叫。 
			 * 如果該呼叫成功傳回或擲回 WAEWOULDBLOCK 錯誤碼 (10035)，則表示通訊端仍在連接中，否則，就表示通訊端已不再連接。 
			 * Depending on http://msdn.microsoft.com/zh-tw/library/system.net.sockets.socket.connected.aspx?cs-save-lang=1&cs-lang=csharp
			********************************************************************************************/
            if (_IsDisposed) throw new ObjectDisposedException(this.GetType().Name);
            bool connectState = true;
            bool blockingState = m_Socket.Blocking;
            try
            {
                byte[] tmp = new byte[1];
                m_Socket.Blocking = false;
                m_Socket.Send(tmp, 0, SocketFlags.None);    // 若發送錯誤，將跳至 Catch 區段
                connectState = true;
            }
            catch (ObjectDisposedException)
            {
                connectState = false;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                    connectState = true;
                else
                    connectState = false;
            }
            finally
            {
                try
                {
                    if (m_Socket != null)
                        m_Socket.Blocking = blockingState;
                }
                catch { connectState = false; }
            }
            return connectState;
        }
        #endregion

        #region Public Static Method : IPAddress GetHostIP()
        /// <summary>取得本機IP</summary>
        /// <returns></returns>
        public static IPAddress GetHostIP()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addrs = ipEntry.AddressList;
            IPAddress ipAddr = null;
            foreach (IPAddress ipa in addrs)
            {
                if (ipa.IsIPv6LinkLocal || ipa.AddressFamily.HasFlag(AddressFamily.InterNetworkV6))
                    continue;
                ipAddr = ipa;
            }
            return ipAddr;
        }
        #endregion

        #region Public Static Method : string GetDestinationMacAddress(IPAddress address)
        /// <summary>取得目標網路介面的 MAC 位址</summary>
        /// <param name="address">遠端 IP 位址</param>
        /// <exception cref="ArgumentException">Only supports IPv4 Addresses.</exception>
        /// <returns></returns>
        public static string GetDestinationMacAddress(IPAddress address)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("Only supports IPv4 Addresses.");
            try
            {
                int addrInt = IpAddressAsInt32(address);
                int srcAddrInt = IpAddressAsInt32(IPAddress.Any);
                const int MacAddressLength = 6;// 48bits
                byte[] macAddress = new byte[MacAddressLength];
                int macAddrLen = macAddress.Length;
                int ret = SendArp(addrInt, srcAddrInt, macAddress, ref macAddrLen);
                if (ret != 0)
                    return null;
                string mac = string.Empty;
                for (int i = 0; i < macAddress.Length; i++)
                    mac += macAddress[i].ToString("X2") + ":";
                if (mac.Length != 0)
                    mac = mac.TrimEnd(':');
                return mac;
            }
            catch { return null; }
        }
        #endregion

        #region Private Static Method : int IpAddressAsInt32(IPAddress address)
        private static int IpAddressAsInt32(IPAddress address)
        {
            byte[] ipAddrBytes = address.GetAddressBytes();
            int addrInt = BitConverter.ToInt32(ipAddrBytes, 0);
            return addrInt;
        }
        #endregion
    }
}
