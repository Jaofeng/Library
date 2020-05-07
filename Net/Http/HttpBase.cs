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
using System.Web;

#pragma warning disable IDE0019
namespace CJF.Net.Http
{
    /// <summary>HTTP 連線服務類別</summary>
    [Serializable]
    public class HttpBase : IDisposable
    {
        /// <summary>網頁預設根目錄</summary>
        const string ROOT_PATH = "Web";

        bool isDisposed = false;

        #region Public Properties
        /// <summary>取得HttpListenerContext類別</summary>
        public HttpServiceContext Context { get; protected set; }
        /// <summary>取得遠端IP資訊</summary>
        public IPEndPoint ClientPoint { get => Context.Request.RemoteEndPoint; }
        /// <summary>取得接收的檔案資訊</summary>
        public List<ReceivedFileInfo> ReceivedFiles { get; protected set; }
        #endregion

        #region Public Static Properties
        /// <summary>設定或取得網頁預設根目錄。絕對路徑或相對路徑皆可。
        /// <para>預設值：Web</para></summary>
        public static string RootPath { get; set; } = ROOT_PATH;
        /// <summary>設定或取得錯誤發生時，除記錄至事件檔外，是否發送Mail</summary>
        public static bool SendMailWhenException { get; set; } = false;
        #endregion

        #region Construct Method : HttpBase(...) +2
        /// <summary></summary>
        protected HttpBase() { }
        /// <summary></summary>
        /// <param name="context"></param>
        public HttpBase(HttpListenerContext context)
        {
            this.Context = new HttpServiceContext(context);
        }
        /// <summary></summary>
        /// <param name="context"></param>
        public HttpBase(HttpServiceContext context)
        {
            this.Context = context;
        }
        /// <summary></summary>
        ~HttpBase()
        {
            Dispose(false);
        }
        #endregion

        #region Public Virtual Method : void ProcessRequest()
        /// <summary>[覆寫] 接收到 Http Request 時的處理流程</summary>
        public virtual void ProcessRequest()
        {
            HttpListenerRequest request = this.Context.Request;
            string rawUrl = request.RawUrl;
            string pageUrl = rawUrl.Split('?')[0].TrimStart('/');
            // [Remote IP] - [Method] [RawUrl] [HTTP/Ver] - [User Agent]
            string format = "{0} - {1} {2} HTTP/{3}.{4} - {5}";
            Debug.Print(format, request.RemoteEndPoint, request.HttpMethod, rawUrl, request.ProtocolVersion.Major, request.ProtocolVersion.Minor, request.UserAgent);
            NameValueCollection queryString = request.QueryString;
            switch (request.HttpMethod.ToUpper())
            {
                case "HEAD":
                    HttpHeadMethod(pageUrl, queryString); break;
                case "GET":
                    HttpGetMethod(pageUrl, queryString); break;
                case "POST":
                    HttpPostMethod(pageUrl, queryString); break;
                case "PUT":
                    if (request.ProtocolVersion.Major >= 1 && request.ProtocolVersion.Minor >= 1)
                        HttpPutMethod(pageUrl, queryString);
                    else
                        ResponseMethodNotAllowed();
                    break;
                case "DELETE":
                    if (request.ProtocolVersion.Major >= 1 && request.ProtocolVersion.Minor >= 1)
                        HttpDeleteMethod(pageUrl, queryString);
                    else
                        ResponseMethodNotAllowed();
                    break;
                case "PATCH":
                    if (request.ProtocolVersion.Major >= 1 && request.ProtocolVersion.Minor >= 1)
                        HttpPatchMethod(pageUrl, queryString);
                    else
                        ResponseMethodNotAllowed();
                    break;
                default:
                    ResponseServiceNotSupport(); break;
            }
        }
        #endregion

        #region Protected Virtual Method : void HttpHeadMethod(string path, NameValueCollection queryString)
        /// <summary>[覆寫]HEAD 模式</summary>
        /// <param name="path">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        protected virtual void HttpHeadMethod(string path, NameValueCollection queryString)
        {
            string file = path.Replace("/", "\\").ToLower();
            file = Path.Combine(RootPath, file);
            ResponseHead(file);
        }
        #endregion

        #region Protected Virtual Method : void HttpGetMethod(string path, NameValueCollection queryString)
        /// <summary>[覆寫]GET 模式</summary>
        /// <param name="path">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        protected virtual void HttpGetMethod(string path, NameValueCollection queryString)
        {
            string file = path.Replace("/", "\\").ToLower();
            file = Path.Combine(RootPath, file);
            ResponseFile(file);
        }
        #endregion

        #region Protected Virtual Method : void HttpPostMethod(string path, NameValueCollection queryString)
        /// <summary>[覆寫]POST 模式</summary>
        /// <param name="path">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        protected virtual void HttpPostMethod(string path, NameValueCollection queryString)
        {
            try
            {
                //string svc = path.Replace("/", "\\").ToLower().TrimEnd('\\').Split('\\')[0];
                HttpListenerRequest request = this.Context.Request;
                NameValueCollection nvc = null;
                this.ReceivedFiles = null;
                if (request.ContentType == "application/x-www-form-urlencoded")
                {
                    StreamReader reader = new StreamReader(request.InputStream);
                    string data = reader.ReadToEnd();
                    //Debug.Print("Service:{0}:{1}", svc, data);
                    nvc = System.Web.HttpUtility.ParseQueryString(data);
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
                    ReceivedAPI(path, nvc);
                }
                else if (request.ContentType.StartsWith("multipart/form-data"))
                {
                    PopulatePostMultiPart(request, out nvc);
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
                    List<string> arr = this.ReceivedFiles.ConvertAll<string>(rfi => rfi.FileName);
                    //Debug.Print("Service:{0},Files:{1}", svc, string.Join(",", arr.ToArray()));
                    ReceivedAPI(path, nvc);
                }
            }
            catch (Exception ex) { Debug.Print(ex.Message); }
        }
        #endregion

        #region Protected Virtual Method : void HttpPutMethod(string path, NameValueCollection queryString)
        /// <summary>[覆寫]PUT 模式</summary>
        /// <param name="path">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        protected virtual void HttpPutMethod(string path, NameValueCollection queryString)
        {
            Debug.Print($"PUT {path}");
            string msg = "No Support PUT method!!";
            byte[] buffer = this.Context.Request.ContentEncoding.GetBytes(msg);
            ResponseBinary(buffer, (int)System.Net.HttpStatusCode.Forbidden, msg);
        }
        #endregion

        #region Protected Virtual Method : void HttpDeleteMethod(string path, NameValueCollection queryString)
        /// <summary>[覆寫]DELETE 模式</summary>
        /// <param name="path">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        protected virtual void HttpDeleteMethod(string path, NameValueCollection queryString)
        {
            Debug.Print($"DELETE {path}");
            string msg = "No Support DELETE method!!";
            byte[] buffer = this.Context.Request.ContentEncoding.GetBytes(msg);
            ResponseBinary(buffer, (int)System.Net.HttpStatusCode.Forbidden, msg);
        }
        #endregion

        #region Protected Virtual Method : void HttpPatchMethod(string path, NameValueCollection queryString)
        /// <summary>[覆寫]PATCH 模式</summary>
        /// <param name="path">網址路徑</param>
        /// <param name="queryString">要求中所包含的查詢字串。</param>
        protected virtual void HttpPatchMethod(string path, NameValueCollection queryString)
        {
            Debug.Print($"PATCH {path}");
            string msg = "No Support PATCH method!!";
            byte[] buffer = this.Context.Request.ContentEncoding.GetBytes(msg);
            ResponseBinary(buffer, (int)System.Net.HttpStatusCode.Forbidden, msg);
        }
        #endregion

        #region Response Methods
        #region Public Virtual Method : bool ResponseHead(string fileName)
        /// <summary>傳送檔案資訊至終端</summary>
        /// <param name="fileName">檔案路徑</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseHead(string fileName)
        {
            bool result = false;
            if (!File.Exists(fileName))
            {
                this.Context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                this.Context.Response.OutputStream.Close();
                result = true;
            }
            else
            {
                try
                {
                    using (FileStream fs = File.OpenRead(fileName))
                    {
                        this.Context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                        this.Context.Response.ContentLength64 = fs.Length;
                        string mime = ConvUtils.GetContentType(fileName);
                        this.Context.Response.ContentType = mime;
                        if (mime.Equals("application/octetstream", StringComparison.OrdinalIgnoreCase))
                            this.Context.Response.AddHeader("content-disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(fileName)));
                    }
                    this.Context.Response.OutputStream.Close();
                    result = true;
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 64)
                        Debug.Print("Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
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
                        if (this.Context.Response != null)
                            this.Context.Response.Close();
                    }
                    catch { }
                }
            }
            return result;
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(string fileName)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="fileName">檔案路徑</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(string fileName)
        {
            return ResponseFile(fileName, 0, 8192);
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(string fileName, int pause)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="fileName">檔案路徑</param>
        /// <param name="pause">傳輸暫停時間，單位豪秒。數值越小，傳輸速度越快，但負載也越大。最小值為 10ms。</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(string fileName, int pause)
        {
            return ResponseFile(fileName, pause, 8192);
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(string fileName, int pause, int packageSize)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="fileName">檔案路徑</param>
        /// <param name="pause">傳輸暫停時間，單位豪秒。數值越小，傳輸速度越快，但負載也越大。最小值為 10ms。</param>
        /// <param name="packageSize">傳輸的封包大小，單位Bytes</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(string fileName, int pause, int packageSize)
        {
            bool result = false;
            if (!File.Exists(fileName))
                result = ResponseNotFound();
            else
            {
                try
                {
                    using (FileStream fs = File.OpenRead(fileName))
                    {
                        this.Context.Response.StatusCode = (int)HttpStatusCode.OK;
                        this.Context.Response.ContentLength64 = fs.Length;
                        string mime = ConvUtils.GetContentType(fileName);
                        this.Context.Response.ContentType = mime;

                        if (mime.Equals("application/octetstream", StringComparison.OrdinalIgnoreCase))
                            this.Context.Response.AddHeader("content-disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(fileName)));
                        byte[] buffer = new byte[packageSize];
                        int read;
                        if (pause < 10)
                            pause = 10;
                        while ((read = fs.Read(buffer, 0, packageSize)) > 0)
                        {
                            this.Context.Response.OutputStream.Write(buffer, 0, read);
                            System.Threading.Thread.Sleep(pause);
                        }
                    }
                    this.Context.Response.OutputStream.Close();
                    result = true;
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 64)
                        Debug.Print("Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
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
                        if (this.Context.Response != null)
                            this.Context.Response.Close();
                    }
                    catch { }
                }
            }
            return result;
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(MemoryStream stream, string fileName)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="stream">檔案串流</param>
        /// <param name="fileName">檔案路徑</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(MemoryStream stream, string fileName)
        {
            return ResponseFile(stream, fileName, 10, 8192);
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(MemoryStream stream, string fileName, int speed)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="stream">檔案串流</param>
        /// <param name="fileName">檔案路徑</param>
        /// <param name="speed">傳輸暫停時間，單位豪秒。數值越小，傳輸速度越快，但負載也越大。最小值為 10ms。</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(MemoryStream stream, string fileName, int speed)
        {
            return ResponseFile(stream, fileName, speed, 8192);
        }
        #endregion

        #region Public Virtual Method : bool ResponseFile(MemoryStream stream, string fileName, int pause, int packageSize)
        /// <summary>傳送檔案至終端</summary>
        /// <param name="stream">檔案串流</param>
        /// <param name="fileName">檔案路徑</param>
        /// <param name="pause">傳輸暫停時間，單位豪秒。數值越小，傳輸速度越快，但負載也越大。最小值為 10ms。</param>
        /// <param name="packageSize">傳輸的封包大小，單位Bytes，預設為8192</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseFile(MemoryStream stream, string fileName, int pause, int packageSize)
        {
            bool result = false;
            if (stream == null || stream.Length == 0)
                result = ResponseNotFound();
            else
            {
                try
                {
                    this.Context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                    this.Context.Response.ContentLength64 = stream.Length;
                    string mime = ConvUtils.GetContentType(fileName);
                    this.Context.Response.ContentType = mime;
                    this.Context.Response.AddHeader("content-disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(fileName)));
                    byte[] buffer = new byte[packageSize];
                    int read;
                    if (pause < 10)
                        pause = 10;
                    stream.Position = 0;
                    while ((read = stream.Read(buffer, 0, packageSize)) > 0)
                    {
                        this.Context.Response.OutputStream.Write(buffer, 0, read);
                        System.Threading.Thread.Sleep(pause);
                    }
                    this.Context.Response.OutputStream.Close();
                    result = true;
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode == 64)
                        Debug.Print("Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
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
                        if (this.Context.Response != null)
                            this.Context.Response.Close();
                    }
                    catch { }
                }
            }
            return result;
        }
        #endregion

        #region Public Virtual Method : bool ResponseString(string msg, int statusCode)
        /// <summary>傳送文字訊息給終端</summary>
        /// <param name="msg">訊息內容</param>
        /// <param name="statusCode">HTTP 狀態碼</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseString(string msg, int statusCode)
        {
            this.Context.Response.ContentEncoding = Encoding.UTF8;
            return ResponseBinary(Encoding.UTF8.GetBytes(msg), statusCode);
        }
        #endregion

        #region Public Virtual Method : bool ResponseString(string msg)
        /// <summary>傳送文字訊息給終端</summary>
        /// <param name="msg">訊息內容</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseString(string msg)
        {
            this.Context.Response.ContentEncoding = Encoding.UTF8;
            return ResponseBinary(Encoding.UTF8.GetBytes(msg), (int)HttpStatusCode.OK);
        }
        #endregion

        #region Public Virtual Method : bool ResponseString(string msg, Encoding encoding)
        /// <summary>傳送文字訊息給終端</summary>
        /// <param name="msg">訊息內容</param>
        /// <param name="encoding">字元編碼方式</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseString(string msg, Encoding encoding)
        {
            this.Context.Response.ContentEncoding = encoding;
            return ResponseBinary(encoding.GetBytes(msg), (int)System.Net.HttpStatusCode.OK);
        }
        #endregion

        #region Public Virtual Method : bool ResponseBitmap(Bitmap bitmap)
        /// <summary>繪製 Bitmap 資料至終端</summary>
        /// <param name="bitmap">Bitmap 圖像資料</param>
        /// <returns></returns>
        public virtual bool ResponseBitmap(Bitmap bitmap)
        {
            return ResponseBitmap(bitmap, ImageFormat.Jpeg);
        }
        #endregion

        #region Public Virtual Method : bool ResponseBitmap(Bitmap bitmap, ImageFormat format)
        /// <summary>繪製 Bitmap 資料至終端</summary>
        /// <param name="bitmap">Bitmap 圖像資料</param>
        /// <param name="format">圖檔格式</param>
        /// <returns></returns>
        public virtual bool ResponseBitmap(Bitmap bitmap, ImageFormat format)
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
                        bitmap.Save(this.Context.Response.OutputStream, info, eps);
                        this.Context.Response.OutputStream.Close();
                    }
                    else
                        bitmap.Save(this.Context.Response.OutputStream, format);
                }
                else
                {
                    ResponseNotFound();
                }
                result = true;
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 64)
                {
                    Debug.Print("Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
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
                if (this.Context != null && this.Context.Response != null)
                    this.Context.Response.Close();
            }
            return result;
        }
        #endregion

        #region Public Virtual Method : bool ResponseBitmap(Bitmap bitmap, string fileName, ImageFormat format)
        /// <summary>繪製 Bitmap 資料至終端</summary>
        /// <param name="bitmap">Bitmap 圖像資料</param>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="format">圖檔格式</param>
        /// <returns></returns>
        public virtual bool ResponseBitmap(Bitmap bitmap, string fileName, ImageFormat format)
        {
            this.Context.Response.AddHeader("content-disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(fileName));
            return ResponseBitmap(bitmap, format);
        }
        #endregion

        #region Public Virtual Method : bool ResponseBinary(byte[] buffer, int statusCode)
        /// <summary>傳送位元組資料給終端</summary>
        /// <param name="buffer">位元組陣列資料內容</param>
        /// <param name="statusCode">HTTP 狀態碼</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseBinary(byte[] buffer, int statusCode)
        {
            return ResponseBinary(buffer, statusCode, null);
        }
        #endregion

        #region Public Virtual Method : bool ResponseBinary(byte[] buffer, int statusCode, string description)
        /// <summary>傳送位元組資料給終端</summary>
        /// <param name="buffer">位元組陣列資料內容</param>
        /// <param name="statusCode">HTTP 狀態碼</param>
        /// <param name="description">狀態碼說明</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseBinary(byte[] buffer, int statusCode, string description)
        {
            bool result = false;
            try
            {
                this.Context.Response.StatusCode = statusCode;
                if (!string.IsNullOrEmpty(description))
                    this.Context.Response.StatusDescription = description;
                if (buffer != null)
                {
                    this.Context.Response.ContentLength64 = buffer.Length;
                    this.Context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                    this.Context.Response.ContentLength64 = 0;
                this.Context.Response.OutputStream.Close();
                result = true;
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 64)
                {
                    Debug.Print("Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
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
                if (this.Context.Response != null)
                {
                    try { this.Context.Response.Close(); }
                    catch { }
                }
            }
            return result;
        }
        #endregion

        #region Public Virtual Method : bool ResponseException(Exception ex)
        /// <summary>將錯誤訊息送給終端</summary>
        /// <param name="ex">例外處理類別</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseException(Exception ex)
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
                return ResponseString(content);
            }
            else
            {
                sb.Insert(0, "<html><head><meta charset=\"utf-8\"></head><body>");
                sb.Append("</body></html>");
                return ResponseString(sb.ToString(), (int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Public Virtual Method : bool ResponseXML(string xml)
        /// <summary>傳送 XML 內容給終端，如ContentType未設定，則將會指定為application/xhtml+xml</summary>
        /// <param name="xml">XML 資料內容</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseXML(string xml)
        {
            try
            {
                // XML 強制使用 UTF-8 轉碼，不可再異動 By JF @ 2012/04/18
                this.Context.Response.ContentEncoding = Encoding.UTF8;
                if (this.Context.Response.ContentType == null)
                    this.Context.Response.ContentType = "application/xhtml+xml";
                return ResponseBinary(Encoding.UTF8.GetBytes(xml), (int)System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Debug.Print("From:ResponseXML");
                Debug.Print(ex.Message);
                return false;
            }
        }
        #endregion

        #region Public Virtual Method : bool ResponseNotFound()
        /// <summary>傳送「頁面不存在訊息」給終端, HTTP Error Code:404</summary>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseNotFound()
        {
            return ResponseString("File Not Found !", (int)System.Net.HttpStatusCode.NotFound);
        }
        #endregion

        #region Public Virtual Method : bool ResponseServiceNotSupport()
        /// <summary>傳送「不支援此服務」給終端, HTTP Error Code:501</summary>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseServiceNotSupport()
        {
            byte[] buffer = this.Context.Request.ContentEncoding.GetBytes("Service Not Support!!");
            return ResponseBinary(buffer, (int)System.Net.HttpStatusCode.NotImplemented, "Service Not Support!!");
        }
        #endregion

        #region Public Virtual Method : bool ResponseMethodNotAllowed()
        /// <summary>傳送「不支援此 Method」給終端, HTTP Error Code:405</summary>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseMethodNotAllowed()
        {
            return ResponseBinary(null, (int)System.Net.HttpStatusCode.MethodNotAllowed);
        }
        #endregion

        #region Public Virtual Method : bool ResponseUnauthorized()
        /// <summary>傳送「未獲得授權或未認證」給終端, HTTP Error Code:401</summary>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseUnauthorized()
        {
            byte[] buffer = this.Context.Request.ContentEncoding.GetBytes("Unauthorized!!");
            return ResponseBinary(buffer, (int)System.Net.HttpStatusCode.Unauthorized, "Unauthorized!!");
        }
        #endregion

        #region Public Virtual Method : bool ResponseNotAllowIP()
        /// <summary>傳送「不允許的 IP 位址連線」給終端, HTTP Error Code:403</summary>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseNotAllowIP()
        {
            byte[] buffer = this.Context.Request.ContentEncoding.GetBytes("Not Allow IP!!");
            return ResponseBinary(buffer, (int)System.Net.HttpStatusCode.Forbidden, "Not Allow IP!!");
        }
        #endregion

        #region Public Virtual Method : bool ResponseAPIError()
        /// <summary>傳送「API錯誤」給終端, HTTP Error Code:400</summary>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseAPIError()
        {
            return ResponseAPIError("API Error!!");
        }
        #endregion

        #region Public Virtual Method : bool ResponseAPIError(string msg)
        /// <summary>傳送「API錯誤」給終端, HTTP Error Code:400</summary>
        /// <param name="msg">錯誤訊息</param>
        /// <returns>是否正確傳送</returns>
        public virtual bool ResponseAPIError(string msg)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            return ResponseBinary(buffer, (int)System.Net.HttpStatusCode.BadRequest, "API Error!!");
        }
        #endregion
        #endregion

        #region Protected Virtual Method : void ReceivedAPI(string svc, NameValueCollection nvc)
        /// <summary>[覆寫]收到 API 時的處理函示</summary>
        /// <param name="svc">服務代碼</param>
        /// <param name="nvc">自 QueryString 取得的資料</param>
        protected internal virtual void ReceivedAPI(string svc, NameValueCollection nvc)
        {
            ResponseException(new NotImplementedException());
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
            if (isDisposed) return;
            if (disposing)
            {
                this.Context = null;
            }
            isDisposed = true;
        }
        #endregion

        #region Protected Virtual Method : void PopulatePostMultiPart(HttpListenerRequest request, out NameValueCollection nvc)
        /// <summary>[覆寫]拆解 Request 內容</summary>
        /// <param name="request">欲拆解的 HttpListenerRequest 類別</param>
        /// <param name="nvc">輸出成 NameValueCollection 類別</param>
        protected virtual void PopulatePostMultiPart(HttpListenerRequest request, out NameValueCollection nvc)
        {
            this.ReceivedFiles = new List<ReceivedFileInfo>();
            nvc = new NameValueCollection();
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
                                    this.ReceivedFiles.Add(new ReceivedFileInfo()
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
        }
        #endregion

        #region Public Virtual Method : void RedirectURL(string url)
        /// <summary>[覆寫]將回應設定為重新導向用戶端至指定的 URL。</summary>
        /// <param name="url">用戶端應用來尋找所要求之資源的 URL</param>
        public virtual void RedirectURL(string url)
        {
            try
            {
                this.Context.Response.Redirect(url);
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 64)
                {
                    Debug.Print("Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
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
                if (this.Context.Response != null)
                {
                    try { this.Context.Response.Close(); }
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
    }
}
