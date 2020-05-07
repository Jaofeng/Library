using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;

namespace CJF.Net.Http
{
    #region Public Interface : IHttpListenerSession
    /// <summary>Session 物件介面。</summary>
    public interface IHttpListenerSession : IDisposable
    {
        /// <summary>取得 Session ID。</summary>
        string ID { get; }

        /// <summary>取得 Session 內容物件。</summary>
        /// <param name="name">物件鍵名。</param>
        /// <returns></returns>
        object this[string name] { get; set; }

        /// <summary>最後請求時間。</summary>
        DateTime LastRequest { get; }

        /// <summary>是否已卸載。</summary>
        bool IsDisposed { get; }

        /// <summary>刪除指名的物件。</summary>
        /// <param name="name">物件鍵名。</param>
        void Remove(string name);
    }
    #endregion

    #region Public Class : HttpListenerSession
    /// <summary>用戶端連線狀態 Session 實體物件。</summary>
    public class HttpListenerSession : IHttpListenerSession
    {
        private ConcurrentDictionary<string, object> items;

        /// <summary>取得 Session ID。</summary>
        public string ID { get; private set; }

        /// <summary>設定或取得 Session 內容物件。</summary>
        /// <param name="name">物件鍵名。</param>
        /// <returns></returns>
        public object this[string name]
        {
            get { return items.ContainsKey(name) ? items[name] : null; }
            set { items[name] = value; }
        }

        /// <summary>取得最後請求時間。</summary>
        public DateTime LastRequest { get; internal set; }

        /// <summary>是否已卸載。</summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>建立新的用戶端連線狀態 CJF.Net.Http.Session 的實體物件。</summary>
        public HttpListenerSession()
        {
            items = new ConcurrentDictionary<string, object>();
            ID = GenerateSessionId();
            LastRequest = DateTime.Now;
        }

        /// <summary>刪除指名的物件。</summary>
        /// <param name="name">物件鍵名。</param>
        public void Remove(string name)
        {
            items.TryRemove(name, out _);
        }

        #region Private Method : string GenerateSessionId()
        /// <summary>建立新的 Session ID。</summary>
        /// <returns></returns>
        private string GenerateSessionId()
        {
            return Guid.NewGuid().ToString();
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false;
        /// <summary></summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (string key in items.Keys)
                    {
                        items.TryRemove(key, out _);
                    }
                    items.Clear();
                    items = null;
                }
                disposedValue = true;
                IsDisposed = true;
            }
        }

        /// <summary></summary>
        ~HttpListenerSession() { Dispose(false); }

        /// <summary>卸載 CJF.Net.Http.Session 物件。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
    #endregion

    #region Public Class : HttpServiceContext
    /// <summary>包裝著 System.Net.HttpListenerContext 的殼。</summary>
    public class HttpServiceContext
    {
        private readonly HttpListenerContext _innerContext;
        /// <summary>建立一個包裝著 System.Net.HttpListenerContext 的殼。</summary>
        /// <param name="context">原始 System.Net.HttpListenerContext 物件。</param>
        public HttpServiceContext(HttpListenerContext context)
        {
            _innerContext = context;
        }

        /// <summary> 取得 System.Net.HttpListenerRequest，表示用戶端的資源要求。</summary>
        public HttpListenerRequest Request => _innerContext.Request;

        /// <summary>取得將傳送至用戶端的 System.Net.HttpListenerResponse 物件，以回應用戶端的要求。</summary>
        public HttpListenerResponse Response => _innerContext.Response;

        /// <summary>取得用來取得用戶端識別、驗證資訊和安全性角色之物件，由這個 System.Net.HttpListenerContext 物件表示用戶端的要求。</summary>
        public IPrincipal User { get; internal set; }

        /// <summary>取得用來取得用戶端連線狀態的物件，由這個 CJF.Net.Http.Session 物件表示用戶端的連線狀態。</summary>
        public IHttpListenerSession Session { get; internal set; }
    }
    #endregion

    #region Public Class : HttpSession
    /// <summary></summary>
    public class HttpSession : IDisposable
    {
        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="context"></param>
        public delegate void SessionStartHandler(object sender, HttpServiceContext context);
        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="sessionId"></param>
        public delegate void SessionEndHandler(object sender, string sessionId);
        /// <summary></summary>
        public event SessionStartHandler SessionStart;
        /// <summary></summary>
        public event SessionEndHandler SessionEnd;

        private const string _cookieName = "__sessionid__";
        private ConcurrentDictionary<string, HttpListenerSession> _sessions;
        private Thread _workerThread;
        private WaitHandle[] _waitHandles = new WaitHandle[1];
        private bool _running = false;

        /// <summary>設定或取得全域連線狀態逾時時間。單位分鐘。</summary>
        public static int Timeout { get; set; } = 20;

        /// <summary>取得物件是否已卸載。</summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>建立新的 CJF.Net.Http.HttpSession 物件，用以管理所有 CJF.Net.Http.Session 物件。</summary>
        public HttpSession()
        {
            _sessions = new ConcurrentDictionary<string, HttpListenerSession>();
            _workerThread = new Thread(new ParameterizedThreadStart(WorkerThreadProc));
            _workerThread.Start(this);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        /// <summary></summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (string sid in _sessions.Keys)
                    {
                        TryRemove(sid, out HttpListenerSession s);
                        s.Dispose();
                        s = null;
                    }
                    _sessions.Clear();
                    _sessions = null;
                }
                disposedValue = true;
                IsDisposed = true;
            }
        }
        /// <summary></summary>
        ~HttpSession() { Dispose(false); }
        /// <summary></summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Public Method : bool GetOrCreate(HttpServiceContext context, out Session session)
        /// <summary>取得舊有或建立新的 CJF.Net.Http.Session 執行個體。</summary>
        /// <param name="context">傳入包裝著 System.Net.HttpListenerContext 殼的執行個體。</param>
        /// <param name="session">傳回已存在的 CJF.Net.Http.Session 執行個體。</param>
        /// <returns>true: 找到對應的 CJF.Net.Http.Session 執行個體; false: 未找到，但已建立新的 CJF.Net.Http.Session 執行個體。</returns>
        public bool GetOrCreate(HttpServiceContext context, out HttpListenerSession session)
        {
            var cookie = context.Request.Cookies[_cookieName];
            session = null;
            if (cookie != null && !cookie.Expired)
            {
                _sessions.TryGetValue(cookie.Value, out session);
                if (session != null)
                    session.LastRequest = DateTime.Now;
            }
            if (session == null)
            {
                HttpListenerSession sess = new HttpListenerSession();
                _sessions.AddOrUpdate(sess.ID, sess, (k, v) => v = sess);
                cookie = new Cookie(_cookieName, sess.ID, "/");
                context.Response.SetCookie(cookie);
                context.Session = sess;
                session = sess;
                SessionStart?.BeginInvoke(this, context, null, null);
                return false;
            }
            else
            {
                context.Session = session;
                return true;
            }
        }
        #endregion

        #region Public Method : bool TryRemove(string sessionId, out Session session)
        /// <summary>嘗試移除 CJF.Net.Http.Session 物件。</summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="session">傳回 CJF.Net.Http.Session 物件</param>
        /// <returns>true: 已移除; false: sessionId 不存在。</returns>
        public bool TryRemove(string sessionId, out HttpListenerSession session)
        {
            if (!_sessions.TryRemove(sessionId, out session))
                return false;
            SessionEnd?.BeginInvoke(this, sessionId, null, null);
            return true;
        }
        #endregion

        #region Public Method : void Stop()
        /// <summary>停止執行 CJF.Net.Http.HttpSession 執行元件。</summary>
        public void Stop()
        {
            if (_workerThread != null)
            {
                (_waitHandles[0] as ManualResetEvent).Set();
                _workerThread.Join(500);
                _workerThread = null;
            }
            foreach (string k in _sessions.Keys)
            {
                if (TryRemove(k, out HttpListenerSession s))
                    s.Dispose();
            }
        }
        #endregion

        #region Private Method : void WorkerThreadProc(object obj)
        /// <summary></summary>
        /// <param name="obj"></param>
        private void WorkerThreadProc(object obj)
        {
            HttpSession self = (HttpSession)obj;
            self._running = true;
            _waitHandles[0] = new ManualResetEvent(false);  // For Stop()
            while (self._running)
            {
                int waitres = WaitHandle.WaitAny(_waitHandles, 500);
                switch (waitres)
                {
                    case 0:
                        self._running = true;
                        break;
                    case WaitHandle.WaitTimeout:
                        CheckExpired();
                        break;
                }
            }
        }
        #endregion

        #region Private Method : void CheckExpired()
        /// <summary></summary>
        private void CheckExpired()
        {
            List<HttpListenerSession> ls = _sessions.Values.ToList().FindAll(s => s.LastRequest.AddMinutes(Timeout) <= DateTime.Now);
            if (ls == null || ls.Count == 0)
                return;
            foreach (HttpListenerSession s in ls)
            {
                TryRemove(s.ID, out HttpListenerSession rs);
                rs.Dispose();
                rs = null;
            }
        }
        #endregion
    }
    #endregion
}
