/* 欲使用 SSL，請先確定以下幾件事：
 * 1. 是否已產生憑證
 *    在 Visual Studio Command Prompt 中使用 makecert 指令。
 *    如：makecert -r -pe -n "CN=SslSocket" -ss My -sky exchange
 *        -r  ：建立自動簽名的憑證。
 *        -pe ：將產生的私密金鑰標記為可匯出。如此可在憑證中加入私密金鑰。
 *        -n  ：指定主體的憑證名稱，使用雙引號包覆名稱開頭必須加CN=。
 *        -ss ：指定主體的憑證存放區名稱，其儲存輸出憑證，My為憑證存放區的個人存放區。
 *        -sky exchange ：指定收受者的金鑰類型，必須是下列之一：
 *                        signature（表示今要用於數位簽章）；
 *                        exchange（表示金鑰用於金鑰加密和金鑰交換），或一個代表提供程式類型的整數。
 *    詳細作法可參閱 https://dotblogs.com.tw/joysdw12/archive/2013/05/18/104476.aspx
 * 2. 是否已將憑證匯入「本機」存放區中
 *    開啟 MMC 將憑證(*.pfx)匯入「本機電腦」的「個人」中
 * 3. 綁定憑證到系統中
 *    Windows XP 以前作業系統，在 Visual Studio Command Prompt 中使用 httpcfg 指令
 *    如：httpcfg set ssl -i 0.0.0.0:8443 -h 0000000000003ed9cd0c315bbb6dc1c08da5e6
 *        -i 0.0.0.0:8443 -> 為註冊的位址與埠號
 *        -h 0000000000003ed9cd0c315bbb6dc1c08da5e6 -> 為憑證的指紋碼
 *        詳細用法請參閱 httpcfg
 *    Vista 以後版本的作業系統，改用 netsh 指令
 *    如：netsh http add sslcert ipport=0.0.0.0:8443 certhash=0000000000003ed9cd0c315bbb6dc1c08da5e6 appid={00112233-4455-6677-8899-AABBCCDDEEFF} certstorename=MY
 *        ipport=0.0.0.0:8443 -> 為註冊的位址與埠號
 *        certhash=0000000000003ed9cd0c315bbb6dc1c08da5e6 -> 為憑證的指紋碼
 *        appid={00112233-4455-6677-8899-AABBCCDDEEFF} -> 為應用程式的 GUID
 *        certstorename=MY -> 指定憑證的所在區域(個人)
 *        詳細用法請參閱 netsh
 * 4. 執行程式即可使用
 */

using CJF.Utility;
using CJF.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

#pragma warning disable IDE1006
namespace CJF.Net.Http
{
    #region Public Enum : HttpServiceActions
    /// <summary>Http 呼叫模式列舉清單。</summary>
    public enum HttpServiceActions
    {
        /// <summary>無</summary>
        None = 0,
        /// <summary>GET</summary>
        Get = 1,
        /// <summary>POST</summary>
        Post = 2,
        /// <summary>以 POST 方式傳輸檔案。</summary>
        File = 3,
        /// <summary>以 SOAP WebService 方式呼叫。</summary>
        Soap = 4,
        /// <summary>HEAD</summary>
        Head = 5,
        /// <summary>PUT，HTTP 1.1 以上才支援。</summary>
        Put = 6,
        /// <summary>DELETE，HTTP 1.1 以上才支援。</summary>
        Delete = 7,
        /// <summary>PATCH，HTTP 1.1 以上才支援。</summary>
        Patch = 8
    }
    #endregion

    #region Public Class : HttpServiceArgs(EventArgs)
    /// <summary>提供 CJF.Net.Http.HttpService 類別事件傳遞用。</summary>
    public class HttpServiceArgs : EventArgs
    {
        /// <summary>設定或取得收到事件後是否取消後續流程。</summary>
        public bool Cancel { get; set; } = false;
        /// <summary>取得 HTTP 行為模式列舉值。</summary>
        public HttpServiceActions Action { get; private set; } = HttpServiceActions.None;
        /// <summary>取得遠端呼叫的網頁；或設定返還遠端的網頁檔案位置。</summary>
        public string PageUrl { get; set; } = string.Empty;
        /// <summary>取得包含 QurtyString 以及表單內容的 NameValueCollection。</summary>
        public NameValueCollection KeyValues { get; private set; } = null;
        /// <summary>取得已接收的檔案清單資料結構陣列。</summary>
        public ReceivedFileInfo[] ReceivedFiles { get; private set; } = null;
        /// <summary>設定返還至遠端的內容。</summary>
        public object Result { get; set; } = null;
        /// <summary>初始化 CJF.Net.Http.HttpServiceArgs 類別的新執行個體。</summary>
        /// <param name="action">HTTP 呼叫行為列舉值。</param>
        /// <param name="page">使用者端呼叫的網頁網址。</param>
        internal HttpServiceArgs(HttpServiceActions action, string page) : this(action, page, null, null) { }
        /// <summary>初始化 CJF.Net.Http.HttpServiceArgs 類別的新執行個體。</summary>
        /// <param name="action">HTTP 呼叫行為列舉值。</param>
        /// <param name="page">使用者端呼叫的網頁網址。</param>
        /// <param name="keyValues">包含 QurtyString 與 Form 的資料內容。</param>
        internal HttpServiceArgs(HttpServiceActions action, string page, NameValueCollection keyValues) : this(action, page, keyValues, null) { }
        /// <summary>初始化 CJF.Net.Http.HttpServiceArgs 類別的新執行個體。</summary>
        /// <param name="action">HTTP 呼叫行為列舉值。</param>
        /// <param name="page">使用者端呼叫的網頁網址。</param>
        /// <param name="keyValues">包含 QurtyString 與 Form 的資料內容。</param>
        /// <param name="files">已接收的檔案清單。</param>
        internal HttpServiceArgs(HttpServiceActions action, string page, NameValueCollection keyValues, ReceivedFileInfo[] files)
        {
            Cancel = false;
            Action = action;
            PageUrl = page;
            KeyValues = keyValues;
            ReceivedFiles = files;
            Result = null;
        }
    }
    #endregion

    #region Public Class : HttpService
    /// <summary>HTTP 連線服務類別</summary>
    [Obsolete("未完整測試，建議盡量不要使用。", false)]
    public class HttpService : IDisposable
    {
        /// <summary>網頁預設根目錄</summary>
        const string ROOT_PATH = "Web";

        bool isDisposed = false;
        HttpListener _HttpListener = null;

        #region Events
        /// <summary>收到 HTTP GET 請求時發生。</summary>
        public event EventHandler<HttpServiceArgs> ReceivedGet;
        /// <summary>收到 HTTP POST 請求時發生。</summary>
        public event EventHandler<HttpServiceArgs> ReceivedPost;
        /// <summary>收到 HTTP HEAD 請求時發生。</summary>
        public event EventHandler<HttpServiceArgs> ReceivedHead;
        /// <summary>收到 HTTP PUT 請求時發生。</summary>
        public event EventHandler<HttpServiceArgs> ReceivedPut;
        /// <summary>收到 HTTP DELETE 請求時發生。</summary>
        public event EventHandler<HttpServiceArgs> ReceivedDelete;
        /// <summary>收到 HTTP PATCH 請求時發生。</summary>
        public event EventHandler<HttpServiceArgs> ReceivedPatch;
        /// <summary>收到使用者端以 POST 方式上傳檔案時發生。</summary>
        public event EventHandler<HttpServiceArgs> ReceivedFiles;
        /// <summary>收到 HTTP SOAP 請求時發生。</summary>
        private event EventHandler<HttpServiceArgs> ReceivedSoap;
        #endregion

        #region Public Properties
        /// <summary>設定或取得網頁預設根目錄。絕對路徑或相對路徑皆可。
        /// <para>預設值：Web</para></summary>
        public string RootPath { get; set; } = ROOT_PATH;
        /// <summary>設定或取得錯誤發生時，除記錄至事件檔外，是否發送Mail</summary>
        public bool SendMailWhenException { get; set; } = false;
        /// <summary>取得建立的網址。</summary>
        public string[] Prefixes { get; private set; } = new string[0];
        #endregion

        #region Construct Method : HttpService(string[] prefixes)
        /// <summary></summary>
        protected HttpService() { }
        /// <summary></summary>
        /// <param name="prefixes"></param>
        public HttpService(string[] prefixes)
        {
            Prefixes = new string[prefixes.Length];
            Array.Copy(prefixes, Prefixes, prefixes.Length);
        }
        /// <summary></summary>
        ~HttpService()
        {
            Dispose(false);
        }
        #endregion

        #region Public Method : void Start()
        /// <summary>啟動 HTTP 服務。</summary>
        public void Start()
        {
            string prefix = string.Empty;
            _HttpListener = new HttpListener();
            foreach (string p in Prefixes)
                _HttpListener.Prefixes.Add(p);
            _HttpListener.Start();
            _HttpListener.BeginGetContext(new AsyncCallback(WebRequestCallback), _HttpListener);
        }
        #endregion

        #region Public Method : void Stop()
        /// <summary>停止 HTTP 服務。</summary>
        public void Stop()
        {
            _HttpListener.Close();
        }
        #endregion

        #region Private Method : void WebRequestCallback(IAsyncResult ar)
        private void WebRequestCallback(IAsyncResult ar)
        {
            if (_HttpListener == null) return;
            try
            {
                HttpListenerContext ctx = null;
                ctx = _HttpListener.EndGetContext(ar);
                _HttpListener.BeginGetContext(new AsyncCallback(WebRequestCallback), _HttpListener);
                ProcessRequest(ctx);
            }
            catch (Exception ex) { Debug.Print(ex.Message); }
        }
        #endregion

        #region Public Virtual Method : void ProcessRequest(HttpListenerContext context)
        /// <summary>[覆寫] 接收到 Http Request 時的處理流程</summary>
        public virtual void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            string rawUrl = request.RawUrl;
            string pageUrl = rawUrl.Split('?')[0].TrimStart('/');
            // [Remote IP] - [Method] [RawUrl] [HTTP/Ver] - [User Agent]
            Debug.Print($"{request.RemoteEndPoint} - {request.HttpMethod} {rawUrl} HTTP/{request.ProtocolVersion.Major}.{request.ProtocolVersion.Minor} - {request.UserAgent}");
            NameValueCollection queryString = request.QueryString;
            switch (request.HttpMethod.ToUpper())
            {
                case "HEAD":
                    OnReceivedHead(context, pageUrl, queryString);
                    break;
                case "GET":
                    OnReceivedGet(context, pageUrl, queryString);
                    break;
                case "POST":
                    HttpPostMethod(context, pageUrl, queryString);
                    break;
                case "PUT":
                    if (request.ProtocolVersion.Major >= 1 && request.ProtocolVersion.Minor >= 1)
                        OnReceivedPut(context, pageUrl, queryString);
                    else
                        ResponseMethodNotAllowed(context);
                    break;
                case "DELETE":
                    if (request.ProtocolVersion.Major >= 1 && request.ProtocolVersion.Minor >= 1)
                        OnReceivedDelete(context, pageUrl, queryString);
                    else
                        ResponseMethodNotAllowed(context);
                    break;
                case "PATCH":
                    if (request.ProtocolVersion.Major >= 1 && request.ProtocolVersion.Minor >= 1)
                        OnReceivedPatch(context, pageUrl, queryString);
                    else
                        ResponseMethodNotAllowed(context);
                    break;
                default:
                    ResponseServiceNotSupport(context);
                    break;
            }
        }
        #endregion

        #region Protected Virtual Method : void OnReceivedHead(HttpListenerContext context, string page, NameValueCollection queryString)
        /// <summary>[覆寫]產生 ReceivedHead 事件。</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="page">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        protected virtual void OnReceivedHead(HttpListenerContext context, string page, NameValueCollection queryString)
        {
            if (ReceivedHead == null)
            {
                string msg = "No Support HEAD method!!";
                byte[] buffer = context.Request.ContentEncoding.GetBytes(msg);
                ResponseBinary(context, buffer, (int)HttpStatusCode.Forbidden, msg);
            }
            else
            {
                HttpServiceArgs args = new HttpServiceArgs(HttpServiceActions.Head, page, queryString);
                try
                {
                    ReceivedHead?.BeginInvoke(this, args, null, null);
                }
                catch { throw; }
                if (!args.Cancel)
                {
                    ResponseHead(context, Path.Combine(RootPath, args.PageUrl.Replace("/", "\\").ToLower()));
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnReceivedGet(HttpListenerContext context, string page, NameValueCollection queryString)
        /// <summary>[覆寫]產生 ReceivedGet 事件。</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="page">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        protected virtual void OnReceivedGet(HttpListenerContext context, string page, NameValueCollection queryString)
        {
            HttpServiceArgs args = new HttpServiceArgs(HttpServiceActions.Get, page, queryString);
            try
            {
                ReceivedGet?.BeginInvoke(this, args, null, null);
            }
            catch { throw; }
            if (!args.Cancel)
            {
                ResponseFile(context, Path.Combine(RootPath, args.PageUrl.Replace("/", "\\").ToLower()));
            }
            else
            {
                ResponseCallbackResult(context, args.Result);
            }
        }
        #endregion

        #region Protected Virtual Method : void OnReceivedPost(HttpListenerContext context, string page, NameValueCollection keyValues)
        /// <summary>[覆寫]產生 ReceivedPost事件。</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="page">網址路徑</param>
        /// <param name="keyValues">網址中的查詢字串與表單傳遞過來的資料內容。</param>
        protected virtual void OnReceivedPost(HttpListenerContext context, string page, NameValueCollection keyValues)
        {
            HttpServiceArgs args = new HttpServiceArgs(HttpServiceActions.Post, page, keyValues);
            try
            {
                ReceivedPost?.BeginInvoke(this, args, null, null);
            }
            catch { throw; }
            if (!args.Cancel)
            {
                ResponseFile(context, Path.Combine(RootPath, args.PageUrl.Replace("/", "\\").ToLower()));
            }
            else
            {
                ResponseCallbackResult(context, args.Result);
            }
        }
        #endregion

        #region Protected Virtual Method : void OnReceivedFiles(HttpListenerContext context, string page, NameValueCollection keyValues, ReceivedFileInfo[] files)
        /// <summary>[複寫]產生 ReceivedFiles 事件。</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="page">網址路徑</param>
        /// <param name="keyValues">網址中的查詢字串與表單傳遞過來的資料內容。</param>
        /// <param name="files">已接收的檔案清單。</param>
        protected virtual void OnReceivedFiles(HttpListenerContext context, string page, NameValueCollection keyValues, ReceivedFileInfo[] files)
        {
            HttpServiceArgs args = new HttpServiceArgs(HttpServiceActions.File, page, keyValues, files);
            try
            {
                ReceivedFiles?.BeginInvoke(this, args, null, null);
            }
            catch { throw; }
            if (!args.Cancel)
            {
                ResponseFile(context, Path.Combine(RootPath, args.PageUrl.Replace("/", "\\").ToLower()));
            }
            else
            {
                ResponseCallbackResult(context, args.Result);
            }
            foreach (ReceivedFileInfo rfi in files)
            {
                try
                {
                    if (File.Exists(rfi.FullPath))
                        File.Delete(rfi.FullPath);
                }
                catch { }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnReceivedSoap(HttpListenerContext context, string page, NameValueCollection keyValues)
        /// <summary>[覆寫]產生 ReceivedSoap 事件。</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="page">網址路徑</param>
        /// <param name="keyValues">網址中的查詢字串與表單傳遞過來的資料內容。</param>
        protected virtual void OnReceivedSoap(HttpListenerContext context, string page, NameValueCollection keyValues)
        {
            if (ReceivedSoap == null)
            {
                string msg = "No Support SOAP method!!";
                byte[] buffer = context.Request.ContentEncoding.GetBytes(msg);
                ResponseBinary(context, buffer, (int)HttpStatusCode.Forbidden, msg);
            }
            else
            {
                HttpServiceArgs args = new HttpServiceArgs(HttpServiceActions.Soap, page, keyValues, null);
                try
                {
                    ReceivedSoap.BeginInvoke(this, args, null, null);
                }
                catch { throw; }
                if (args.Cancel)
                {
                    ResponseCallbackResult(context, args.Result);
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnReceivedPut(HttpListenerContext context, string page, NameValueCollection queryString)
        /// <summary>[覆寫]產生 ReceivedPut 事件。</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="page">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        protected virtual void OnReceivedPut(HttpListenerContext context, string page, NameValueCollection queryString)
        {
            if (ReceivedPut == null)
            {
                string msg = "No Support PUT method!!";
                byte[] buffer = context.Request.ContentEncoding.GetBytes(msg);
                ResponseBinary(context, buffer, (int)HttpStatusCode.Forbidden, msg);
            }
            else
            {
                HttpServiceArgs args = new HttpServiceArgs(HttpServiceActions.Put, page, queryString);
                try
                {
                    ReceivedPut.BeginInvoke(this, args, null, null);
                }
                catch { throw; }
                if (!args.Cancel)
                {
                    ResponseFile(context, Path.Combine(RootPath, args.PageUrl.Replace("/", "\\").ToLower()));
                }
                else
                {
                    ResponseCallbackResult(context, args.Result);
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnReceivedDelete(HttpListenerContext context, string page, NameValueCollection queryString)
        /// <summary>[覆寫]產生 ReceivedDelete 事件。</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="page">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        protected virtual void OnReceivedDelete(HttpListenerContext context, string page, NameValueCollection queryString)
        {
            if (ReceivedPut == null)
            {
                string msg = "No Support DELETE method!!";
                byte[] buffer = context.Request.ContentEncoding.GetBytes(msg);
                ResponseBinary(context, buffer, (int)HttpStatusCode.Forbidden, msg);
            }
            else
            {
                HttpServiceArgs args = new HttpServiceArgs(HttpServiceActions.Delete, page, queryString);
                try
                {
                    ReceivedDelete.BeginInvoke(this, args, null, null);
                }
                catch { throw; }
                if (!args.Cancel)
                {
                    ResponseFile(context, Path.Combine(RootPath, args.PageUrl.Replace("/", "\\").ToLower()));
                }
                else
                {
                    ResponseCallbackResult(context, args.Result);
                }
            }
        }
        #endregion

        #region Protected Virtual Method : void OnReceivedPatch(HttpListenerContext context, string page, NameValueCollection queryString)
        /// <summary>[覆寫]產生 ReceivedPatch 事件。</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="page">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        protected virtual void OnReceivedPatch(HttpListenerContext context, string page, NameValueCollection queryString)
        {
            if (ReceivedPut == null)
            {
                string msg = "No Support PATCH method!!";
                byte[] buffer = context.Request.ContentEncoding.GetBytes(msg);
                ResponseBinary(context, buffer, (int)HttpStatusCode.Forbidden, msg);
            }
            else
            {
                HttpServiceArgs args = new HttpServiceArgs(HttpServiceActions.Patch, page, queryString);
                try
                {
                    ReceivedPatch.BeginInvoke(this, args, null, null);
                }
                catch { throw; }
                if (!args.Cancel)
                {
                    ResponseFile(context, Path.Combine(RootPath, args.PageUrl.Replace("/", "\\").ToLower()));
                }
            }
        }
        #endregion

        #region Response Methods
        #region Public Virtual Method : bool ResponseHead(HttpListenerContext context, string fileName)
        /// <summary>傳送檔案資訊至終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="fileName">檔案路徑</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseHead(HttpListenerContext context, string fileName)
        {
            bool result = false;
            if (!File.Exists(fileName))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.OutputStream.Close();
                result = true;
            }
            else
            {
                try
                {
                    using (FileStream fs = File.OpenRead(fileName))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.ContentLength64 = fs.Length;
                        string mime = ConvUtils.GetContentType(fileName);
                        context.Response.ContentType = mime;
                        if (mime.Equals("application/octetstream", StringComparison.OrdinalIgnoreCase))
                            context.Response.AddHeader("content-disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(fileName)));
                    }
                    context.Response.OutputStream.Close();
                    result = true;
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 64)
                        Debug.Print("Remote Disconnected:{0}", context.Request.RemoteEndPoint);
                    else
                    {
                        Debug.Print("From:ResponseFile");
                        Debug.Print(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("From:ResponseFile");
                    Debug.Print(ex.Message);
                }
                finally
                {
                    try
                    {
                        if (context.Response != null)
                            context.Response.Close();
                    }
                    catch { }
                }
            }
            return result;
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(HttpListenerContext context, string fileName)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="fileName">檔案路徑</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(HttpListenerContext context, string fileName)
        {
            return ResponseFile(context, fileName, 0, 8192);
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(HttpListenerContext context, string fileName, int pause)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="fileName">檔案路徑</param>
        /// <param name="pause">傳輸暫停時間，單位豪秒。數值越小，傳輸速度越快，但負載也越大。最小值為 10ms。</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(HttpListenerContext context, string fileName, int pause)
        {
            return ResponseFile(context, fileName, pause, 8192);
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(HttpListenerContext context, string fileName, int pause, int packageSize)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="fileName">檔案路徑</param>
        /// <param name="pause">傳輸暫停時間，單位豪秒。數值越小，傳輸速度越快，但負載也越大。最小值為 10ms。</param>
        /// <param name="packageSize">傳輸的封包大小，單位Bytes</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(HttpListenerContext context, string fileName, int pause, int packageSize)
        {
            bool result = false;
            if (!File.Exists(fileName))
                result = ResponseNotFound(context);
            else
            {
                try
                {
                    using (FileStream fs = File.OpenRead(fileName))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.ContentLength64 = fs.Length;
                        string mime = ConvUtils.GetContentType(fileName);
                        context.Response.ContentType = mime;

                        if (mime.Equals("application/octetstream", StringComparison.OrdinalIgnoreCase))
                            context.Response.AddHeader("content-disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(fileName)));
                        byte[] buffer = new byte[packageSize];
                        int read;
                        if (pause < 10)
                            pause = 10;
                        while ((read = fs.Read(buffer, 0, packageSize)) > 0)
                        {
                            context.Response.OutputStream.Write(buffer, 0, read);
                            System.Threading.Thread.Sleep(pause);
                        }
                    }
                    context.Response.OutputStream.Close();
                    result = true;
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 64)
                        Debug.Print("Remote Disconnected:{0}", context.Request.RemoteEndPoint);
                    else
                    {
                        Debug.Print("From:ResponseFile");
                        Debug.Print(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("From:ResponseFile");
                    Debug.Print(ex.Message);
                }
                finally
                {
                    try
                    {
                        if (context.Response != null)
                            context.Response.Close();
                    }
                    catch { }
                }
            }
            return result;
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(HttpListenerContext context, MemoryStream stream, string fileName)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="stream">檔案串流</param>
        /// <param name="fileName">檔案路徑</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(HttpListenerContext context, MemoryStream stream, string fileName)
        {
            return ResponseFile(context, stream, fileName, 10, 8192);
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(HttpListenerContext context, MemoryStream stream, string fileName, int speed)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="stream">檔案串流</param>
        /// <param name="fileName">檔案路徑</param>
        /// <param name="speed">傳輸暫停時間，單位豪秒。數值越小，傳輸速度越快，但負載也越大。最小值為 10ms。</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(HttpListenerContext context, MemoryStream stream, string fileName, int speed)
        {
            return ResponseFile(context, stream, fileName, speed, 8192);
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(HttpListenerContext context, MemoryStream stream, string fileName, int pause, int packageSize)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="stream">檔案串流</param>
        /// <param name="fileName">檔案路徑</param>
        /// <param name="pause">傳輸暫停時間，單位豪秒。數值越小，傳輸速度越快，但負載也越大。最小值為 10ms。</param>
        /// <param name="packageSize">傳輸的封包大小，單位Bytes，預設為8192</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(HttpListenerContext context, MemoryStream stream, string fileName, int pause, int packageSize)
        {
            bool result = false;
            if (stream == null || stream.Length == 0)
                result = ResponseNotFound(context);
            else
            {
                try
                {
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                    context.Response.ContentLength64 = stream.Length;
                    string mime = ConvUtils.GetContentType(fileName);
                    context.Response.ContentType = mime;
                    if (mime.Equals("application/octetstream", StringComparison.OrdinalIgnoreCase))
                        context.Response.AddHeader("content-disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(fileName)));
                    byte[] buffer = new byte[packageSize];
                    int read;
                    if (pause < 10)
                        pause = 10;
                    while ((read = stream.Read(buffer, 0, packageSize)) > 0)
                    {
                        context.Response.OutputStream.Write(buffer, 0, read);
                        System.Threading.Thread.Sleep(pause);
                    }
                    context.Response.OutputStream.Close();
                    result = true;
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 64)
                        Debug.Print("Remote Disconnected:{0}", context.Request.RemoteEndPoint);
                    else
                    {
                        Debug.Print("From:ResponseFile");
                        Debug.Print(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("From:ResponseFile");
                    Debug.Print(ex.Message);
                }
                finally
                {
                    try
                    {
                        if (context.Response != null)
                            context.Response.Close();
                    }
                    catch { }
                }
            }
            return result;
        }
        #endregion

        #region Public Virtual Method : bool ResponseString(HttpListenerContext context, string msg, int statusCode)
        /// <summary>傳送文字訊息給終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="msg">訊息內容</param>
        /// <param name="statusCode">HTTP 狀態碼</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseString(HttpListenerContext context, string msg, int statusCode)
        {
            context.Response.ContentEncoding = Encoding.UTF8;
            return ResponseBinary(context, Encoding.UTF8.GetBytes(msg), statusCode);
        }
        #endregion

        #region Public Virtual Method : bool ResponseString(HttpListenerContext context, string msg)
        /// <summary>傳送文字訊息給終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="msg">訊息內容</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseString(HttpListenerContext context, string msg)
        {
            context.Response.ContentEncoding = Encoding.UTF8;
            return ResponseBinary(context, Encoding.UTF8.GetBytes(msg), (int)System.Net.HttpStatusCode.OK);
        }
        #endregion

        #region Public Virtual Method : bool ResponseString(HttpListenerContext context, string msg, Encoding encoding)
        /// <summary>傳送文字訊息給終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="msg">訊息內容</param>
        /// <param name="encoding">字元編碼方式</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseString(HttpListenerContext context, string msg, Encoding encoding)
        {
            context.Response.ContentEncoding = encoding;
            return ResponseBinary(context, encoding.GetBytes(msg), (int)System.Net.HttpStatusCode.OK);
        }
        #endregion

        #region Public Virtual Method : bool ResponseBitmap(HttpListenerContext context, Bitmap bitmap)
        /// <summary>繪製 Bitmap 資料至終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="bitmap">Bitmap 圖像資料</param>
        /// <returns></returns>
        public virtual bool ResponseBitmap(HttpListenerContext context, Bitmap bitmap)
        {
            return ResponseBitmap(context, bitmap, ImageFormat.Jpeg);
        }
        #endregion

        #region Public Virtual Method : bool ResponseBitmap(HttpListenerContext context, Bitmap bitmap, ImageFormat format)
        /// <summary>繪製 Bitmap 資料至終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="bitmap">Bitmap 圖像資料</param>
        /// <param name="format">圖檔格式</param>
        /// <returns></returns>
        public virtual bool ResponseBitmap(HttpListenerContext context, Bitmap bitmap, ImageFormat format)
        {
            bool result = false;
            try
            {
                if (bitmap != null)
                {
                    if (format == ImageFormat.Jpeg)
                    {
                        ImageCodecInfo info = GetImageEncoder(format);
                        EncoderParameters eps = new EncoderParameters(1);
                        EncoderParameter ep = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                        eps.Param[0] = ep;
                        bitmap.Save(context.Response.OutputStream, info, eps);
                        context.Response.OutputStream.Close();
                    }
                    else
                        bitmap.Save(context.Response.OutputStream, format);
                }
                else
                {
                    ResponseNotFound(context);
                }
                result = true;
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 64)
                {
                    Debug.Print("Remote Disconnected:{0}", context.Request.RemoteEndPoint);
                    result = false;
                }
                else
                {
                    Debug.Print("From:ResponseBitmap");
                    Debug.Print(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Debug.Print("From:ResponseBitmap");
                Debug.Print(ex.Message);
            }
            finally
            {
                if (context != null && context.Response != null)
                    context.Response.Close();
            }
            return result;
        }
        #endregion

        #region Public Virtual Method : bool ResponseBitmap(HttpListenerContext context, Bitmap bitmap, string fileName, ImageFormat format)
        /// <summary>繪製 Bitmap 資料至終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="bitmap">Bitmap 圖像資料</param>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="format">圖檔格式</param>
        /// <returns></returns>
        public virtual bool ResponseBitmap(HttpListenerContext context, Bitmap bitmap, string fileName, ImageFormat format)
        {
            context.Response.AddHeader("content-disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(fileName));
            return ResponseBitmap(context, bitmap, format);
        }
        #endregion

        #region Public Virtual Method : bool ResponseBinary(HttpListenerContext context, byte[] buffer, int statusCode)
        /// <summary>傳送位元組資料給終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="buffer">位元組陣列資料內容</param>
        /// <param name="statusCode">HTTP 狀態碼</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseBinary(HttpListenerContext context, byte[] buffer, int statusCode)
        {
            return ResponseBinary(context, buffer, statusCode, null);
        }
        #endregion

        #region Public Virtual Method : bool ResponseBinary(HttpListenerContext context, byte[] buffer, int statusCode, string description)
        /// <summary>傳送位元組資料給終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="buffer">位元組陣列資料內容</param>
        /// <param name="statusCode">HTTP 狀態碼</param>
        /// <param name="description">狀態碼說明</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseBinary(HttpListenerContext context, byte[] buffer, int statusCode, string description)
        {
            bool result = false;
            try
            {
                context.Response.StatusCode = statusCode;
                if (!string.IsNullOrEmpty(description))
                    context.Response.StatusDescription = description;
                if (buffer != null)
                {
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                    context.Response.ContentLength64 = 0;
                context.Response.OutputStream.Close();
                result = true;
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 64)
                {
                    Debug.Print("Remote Disconnected:{0}", context.Request.RemoteEndPoint);
                    result = false;
                }
                else
                {
                    Debug.Print("From:ResponseBinary");
                    Debug.Print(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Debug.Print("From:ResponseBinary");
                Debug.Print(ex.Message);
            }
            finally
            {
                if (context.Response != null)
                {
                    try { context.Response.Close(); }
                    catch { }
                }
            }
            return result;
        }
        #endregion

        #region Public Virtual Method : bool ResponseException(HttpListenerContext context, Exception ex)
        /// <summary>將錯誤訊息送給終端</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="ex">例外處理類別</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseException(HttpListenerContext context, Exception ex)
        {
            StringBuilder sb = new StringBuilder(ex.Message.Replace("\r", "").Replace("\n", "<br />"));
            sb.AppendFormat("<br />{0}<br />", "=".PadLeft(40, '='));
            if (ex.StackTrace != null)
                sb.AppendFormat("{0}<br />", ex.StackTrace.Replace("\r", "").Replace("\n", "<br />"));
            if (ex.InnerException != null)
            {
                sb.AppendFormat("{0}<br />", "-".PadLeft(40, '-'));
                sb.AppendFormat("InnerException<br />");
                sb.AppendFormat("{0}<br />", ex.InnerException.Message.Replace("\r", "").Replace("\n", "<br />"));
                if (ex.InnerException.StackTrace != null)
                {
                    sb.AppendFormat("StackTrace<br />");
                    sb.AppendFormat("{0}<br />", ex.InnerException.StackTrace.Replace("\r", "").Replace("\n", "<br />"));
                }
            }
            if (File.Exists("Web\\Exception.htm"))
            {
                string content = ReadHtmlFile("Web\\Exception.htm");
                content = content.Replace("<!-- #ERROR_MSG# -->", sb.ToString());
                return ResponseString(context, content);
            }
            else
                return ResponseString(context, sb.ToString(), (int)System.Net.HttpStatusCode.InternalServerError);
        }
        #endregion

        #region Public Virtual Method : bool ResponseXML(HttpListenerContext context, XmlDocument xml)
        /// <summary>傳送 XML 內容給終端，如ContentType未設定，則將會指定為application/xhtml+xml</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="xml">XML 資料內容</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseXML(HttpListenerContext context, XmlDocument xml)
        {
            try
            {
                // XML 強制使用 UTF-8 轉碼，不可再異動 By JF @ 2012/04/18
                context.Response.ContentEncoding = Encoding.UTF8;
                if (context.Response.ContentType == null)
                    context.Response.ContentType = "application/xhtml+xml";
                return ResponseBinary(context, Encoding.UTF8.GetBytes(xml.OuterXml), (int)System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Debug.Print("From:ResponseXML");
                Debug.Print(ex.Message);
                return false;
            }
        }
        #endregion

        #region Public Virtual Method : bool ResponseNotFound(HttpListenerContext context)
        /// <summary>傳送「頁面不存在訊息」給終端, HTTP Error Code:404</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseNotFound(HttpListenerContext context)
        {
            return ResponseString(context, "File Not Found !", (int)System.Net.HttpStatusCode.NotFound);
        }
        #endregion

        #region Public Virtual Method : bool ResponseServiceNotSupport(HttpListenerContext context)
        /// <summary>傳送「不支援此服務」給終端, HTTP Error Code:501</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseServiceNotSupport(HttpListenerContext context)
        {
            byte[] buffer = context.Request.ContentEncoding.GetBytes("Service Not Support!!");
            return ResponseBinary(context, buffer, (int)System.Net.HttpStatusCode.NotImplemented, "Service Not Support!!");
        }
        #endregion

        #region Public Virtual Method : bool ResponseMethodNotAllowed(HttpListenerContext context)
        /// <summary>傳送「不支援此 Method」給終端, HTTP Error Code:405</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseMethodNotAllowed(HttpListenerContext context)
        {
            return ResponseBinary(context, null, (int)HttpStatusCode.MethodNotAllowed);
        }
        #endregion

        #region Public Virtual Method : bool ResponseUnauthorized(HttpListenerContext context)
        /// <summary>傳送「未獲得授權或未認證」給終端, HTTP Error Code:401</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseUnauthorized(HttpListenerContext context)
        {
            byte[] buffer = context.Request.ContentEncoding.GetBytes("Unauthorized!!");
            return ResponseBinary(context, buffer, (int)System.Net.HttpStatusCode.Unauthorized, "Unauthorized!!");
        }
        #endregion

        #region Public Virtual Method : bool ResponseNotAllowIP(HttpListenerContext context)
        /// <summary>傳送「不允許的 IP 位址連線」給終端, HTTP Error Code:403</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseNotAllowIP(HttpListenerContext context)
        {
            byte[] buffer = context.Request.ContentEncoding.GetBytes("Not Allow IP!!");
            return ResponseBinary(context, buffer, (int)System.Net.HttpStatusCode.Forbidden, "Not Allow IP!!");
        }
        #endregion

        #region Public Virtual Method : bool ResponseAPIError(HttpListenerContext context)
        /// <summary>傳送「API錯誤」給終端, HTTP Error Code:400</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseAPIError(HttpListenerContext context)
        {
            return ResponseAPIError(context, "API Error!!");
        }
        #endregion

        #region Public Virtual Method : bool ResponseAPIError(HttpListenerContext context, string msg)
        /// <summary>傳送「API錯誤」給終端, HTTP Error Code:400</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="msg">錯誤訊息</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseAPIError(HttpListenerContext context, string msg)
        {
            return ResponseBinary(context, Encoding.UTF8.GetBytes(msg), (int)System.Net.HttpStatusCode.BadRequest, "API Error!!");
        }
        #endregion
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
            if (isDisposed) return;
            if (disposing)
            {
                Stop();
                _HttpListener = null;
            }
            isDisposed = true;
        }
        #endregion

        #region Public Virtual Method : void RedirectURL(HttpListenerContext context, string url)
        /// <summary>[覆寫]將回應設定為重新導向用戶端至指定的 URL。</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="url">用戶端應用來尋找所要求之資源的 URL</param>
        public virtual void RedirectURL(HttpListenerContext context, string url)
        {
            try
            {
                context.Response.Redirect(url);
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 64)
                {
                    Debug.Print("Remote Disconnected:{0}", context.Request.RemoteEndPoint);
                }
                else
                {
                    Debug.Print("From:RedirectURL");
                    Debug.Print(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Debug.Print("From:RedirectURL");
                Debug.Print(ex.Message);
            }
            finally
            {
                if (context.Response != null)
                {
                    try { context.Response.Close(); }
                    catch { }
                }
            }
        }
        #endregion

        #region Public Static Method : NameValueCollection ViewStateToNameValueCollection(string viewState)
        /// <summary>將頁面中的VIEWSTATE資料轉為鍵值索引集合</summary>
        /// <param name="viewState">VIEWSTATE字串</param>
        /// <returns>鍵值索引集合</returns>
        public static NameValueCollection ViewStateToNameValueCollection(string viewState)
        {
            if (viewState == "##VIEWSTATE##")
                return new NameValueCollection();
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            byte[] buffer = Convert.FromBase64String(viewState);
            System.IO.MemoryStream _memory = new System.IO.MemoryStream(buffer);
            return (NameValueCollection)formatter.Deserialize(_memory);
        }
        #endregion

        #region Public Static Method : string NameValueCollectionToViewState(NameValueCollection nvc)
        /// <summary>
        /// 將鍵值索引集合轉為VIEWSTATE資料
        /// </summary>
        /// <param name="nvc">鍵值索引集合</param>
        /// <returns>VIEWSTATE字串資料</returns>
        public static string NameValueCollectionToViewState(NameValueCollection nvc)
        {
            System.IO.MemoryStream _memory = new System.IO.MemoryStream();
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(_memory, nvc);
            _memory.Position = 0;
            byte[] read = new byte[_memory.Length];
            _memory.Read(read, 0, read.Length);
            _memory.Close();
            return Convert.ToBase64String(read);
        }
        #endregion

        #region Public Static Method : string ReadHtmlFile(string fileName)
        /// <summary>依檔案編碼方式讀取 Html 檔</summary>
        /// <param name="fileName">檔案名稱</param>
        /// <returns></returns>
        public static string ReadHtmlFile(string fileName)
        {
            System.Text.Encoding enc = ConvUtils.GetFileEncoding(fileName);
            if (enc != null)
                return File.ReadAllText(fileName, enc);
            else
                return File.ReadAllText(fileName);
        }
        #endregion

        #region Public Static Method : string ToQueryString(NameValueCollection nvc)
        /// <summary>將 NameValueCollection 類別中的值轉成 QueryString 格式</summary>
        /// <param name="nvc">Key-Value對應的類別</param>
        /// <returns></returns>
        public static string ToQueryString(NameValueCollection nvc)
        {
            if (nvc == null || nvc.AllKeys.Length == 0)
                return string.Empty;
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            return "?" + string.Join("&", array);
        }
        #endregion

        #region Private Method : ImageCodecInfo GetImageEncoder(ImageFormat format)
        private ImageCodecInfo GetImageEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codes = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo ici in codes)
                if (ici.FormatID == format.Guid)
                    return ici;
            return null;
        }
        #endregion

        #region Private Method : NameValueCollection PopulatePostMultiPart(HttpListenerRequest request, out ReceivedFileInfo[] files)
        /// <summary>拆解 Request 內容。</summary>
        /// <param name="request">欲拆解的 HttpListenerRequest 類別。</param>
        /// <param name="files">接收的檔案。</param>
        /// <returns>QueryString NameValueCollection</returns>
        private NameValueCollection PopulatePostMultiPart(HttpListenerRequest request, out ReceivedFileInfo[] files)
        {
            List<ReceivedFileInfo> receivedFiles = new List<ReceivedFileInfo>();
            NameValueCollection nvc = new NameValueCollection();
            using (MyMemoryStream ms = new MyMemoryStream())
            {
                int boundary_index = request.ContentType.IndexOf("boundary=") + 9;
                string boundary = request.ContentType.Substring(boundary_index, request.ContentType.Length - boundary_index);
                byte[] boundaryBytes = Encoding.UTF8.GetBytes(boundary);
                string line = string.Empty;
                int no = 0;
                string[] arr1 = null, arr2 = null;
                byte[] buff = new byte[64 * 1024];
                int read = 0;
                while ((read = request.InputStream.Read(buff, 0, buff.Length)) > 0)
                    ms.Write(buff, 0, read);
                ms.Position = 0;
                while (ms.Position < ms.Length)
                {
                    line = ms.ReadLine();
                    if (line.IndexOf(boundary) != -1)
                    {
                        no = 0;
                        continue;
                    }
                    if (no == 0)
                    {
                        if (line.StartsWith("Content-Disposition"))
                        {
                            arr1 = line.Split(';');
                            if (arr1[0].Split(':')[1].Trim() != "form-data")
                                continue;
                            arr2 = arr1[1].Trim().Split('=');
                            if (arr2[0].ToLower() != "name")
                                continue;
                            string key = arr2[1].Trim('"');
                            if (line.IndexOf("filename=", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                line = ms.ReadLine();   // ContentType
                                if (!line.StartsWith("Content-Type"))
                                    continue;
                                string ct = line.Split(':')[1].Trim();
                                string fn = arr1[2].Trim().Split('=')[1].Trim('"');
                                if (nvc.AllKeys.Contains<string>(key))
                                    nvc[key] += ';' + fn;
                                else
                                    nvc.Add(key, fn);
                                line = ms.ReadLine();   // 捨棄空行

                                int idx = ms.ToArray().IndexOfBytes(boundaryBytes, (int)ms.Position);
                                if (idx != -1)
                                {
                                    string tmp = Path.GetTempFileName();
                                    using (FileStream fs = File.Create(tmp))
                                    {
                                        long total = idx - ms.Position - 4;
                                        int readLen = buff.Length;
                                        if (total < readLen)
                                            readLen = (int)total;
                                        while (total > 0 && (read = ms.Read(buff, 0, readLen)) > 0)
                                        {
                                            fs.Write(buff, 0, read);
                                            total -= read;
                                            if (total < readLen)
                                                readLen = (int)total;
                                        }
                                        fs.Close();
                                    }
                                    receivedFiles.Add(new ReceivedFileInfo()
                                    {
                                        FieldKey = key,
                                        FileName = fn,
                                        ContentType = ct,
                                        Length = new FileInfo(tmp).Length,
                                        FullPath = tmp
                                    });
                                    ms.Position += boundaryBytes.Length + 6;
                                    no = 0;
                                    continue;
                                }
                            }
                            else
                            {
                                StringBuilder sb = new StringBuilder();
                                line = ms.ReadLine();   // 捨棄空行
                                line = ms.ReadLine();
                                while (ms.Position < ms.Length && line.IndexOf(boundary) == -1)
                                {
                                    sb.Append(line);
                                    line = ms.ReadLine();
                                }
                                if (Array.IndexOf<string>(nvc.AllKeys, key) == -1)
                                    nvc.Add(key, sb.ToString());
                                else
                                    nvc[key] += ';' + sb.ToString();
                                if (line.IndexOf(boundary) != -1)
                                {
                                    no = 0;
                                    continue;
                                }
                            }
                        }
                    }
                    no++;
                }
                ms.Close();
                buff = null;
            }
            GC.Collect();
            files = receivedFiles.ToArray();
            return nvc;
        }
        #endregion

        #region Private Method : void HttpPostMethod(HttpListenerContext context, string path, NameValueCollection queryString)
        /// <summary>[覆寫]POST 模式</summary>
        /// <param name="context">要求存取的遠端連線內容。</param>
        /// <param name="page">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        private void HttpPostMethod(HttpListenerContext context, string page, NameValueCollection queryString)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                switch (request.ContentType)
                {
                    case string s when s.StartsWith("multipart/form-data"):
                        #region multipart/form-data
                        {
                            var nvc = PopulatePostMultiPart(request, out ReceivedFileInfo[] rfs);
                            if (queryString != null && queryString.Count != 0)
                            {
                                foreach (KeyValuePair<string, string> kv in queryString)
                                {
                                    if (nvc.AllKeys.Contains<string>(kv.Key))
                                        nvc[kv.Key] += ";" + kv.Value;
                                    else
                                        nvc.Add(kv.Key, kv.Value);
                                }
                            }
                            if (rfs.Length == 0)
                            {
                                OnReceivedPost(context, page, nvc);
                            }
                            else
                            {
                                OnReceivedFiles(context, page, nvc, rfs);
                            }
                            break;
                        }
                    #endregion
                    default:
                        #region Normal POST
                        {
                            StreamReader reader = new StreamReader(request.InputStream);
                            string data = reader.ReadToEnd();
                            reader.Close();
                            string ip = context.Request.RemoteEndPoint.Address.ToString();

                            // 檢查是否為 WebService 的 SOAP 格式
                            Regex reg = new Regex("<soap(12)?:Envelope[\\s\\S]*</soap(12)?:Envelope>", RegexOptions.Multiline | RegexOptions.Singleline);
                            if (reg.IsMatch(data))
                            {
                                ResponseServiceNotSupport(context);
                                #region SOAP
                                //if (this.ReceivedSOAP != null)
                                //{
                                //    XmlDocument xml = new XmlDocument();
                                //    xml.LoadXml(data);
                                //    SoapStruct soap = new SoapStruct()
                                //    {
                                //        Path = page,
                                //        QueryString = queryString,
                                //        XmlContext = xml
                                //    };
                                //    foreach (SOAPHandler del in this.OnReceiveSOAP.GetInvocationList())
                                //        del.BeginInvoke(this, soap, new AsyncCallback(SOAPCallback), del);
                                //}
                                #endregion
                            }
                            else
                            {
                                #region General HTTP POST
                                NameValueCollection nvc = HttpUtility.ParseQueryString(data);
                                if (queryString != null && queryString.Count != 0)
                                {
                                    foreach (KeyValuePair<string, string> kv in queryString)
                                    {
                                        if (nvc.AllKeys.Contains<string>(kv.Key))
                                            nvc[kv.Key] += ";" + kv.Value;
                                        else
                                            nvc.Add(kv.Key, kv.Value);
                                    }
                                }
                                OnReceivedPost(context, page, nvc);
                                #endregion
                            }
                            break;
                        }
                        #endregion
                }
            }
            catch (Exception ex)
            {
                Debug.Print("From:HttpPostMethod:{0}", page);
                Debug.Print(ex.Message);
                ResponseException(context, ex);
            }
        }
        #endregion

        #region Private Method : void ResponseCallbackResult(HttpListenerContext context, object result)
        private void ResponseCallbackResult(HttpListenerContext context, object result)
        {
            switch (result)
            {
                case string s:
                    if (File.Exists(s))
                        ResponseFile(context, s);
                    else
                        ResponseString(context, s);
                    break;
                case XmlDocument xml:
                    ResponseXML(context, xml);
                    break;
                case null:
                default:
                    break;
            }
        }
        #endregion
    }
    #endregion
}
