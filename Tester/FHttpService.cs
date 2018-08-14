using CJF.Net.Http;
using CJF.Utility;
using CJF.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;

#pragma warning disable IDE1006
namespace Tester
{
    public partial class FHttpService : Form
    {
        HttpListener _HttpListener = null;
        string _SvcName = string.Empty;

        public FHttpService()
        {
            InitializeComponent();
            cbCertClient.SelectedIndex = 0;
            cbCertServer.SelectedIndex = 0;
        }

        #region Private Method : void btnStart_Click(object sender, EventArgs e)
        private void btnStart_Click(object sender, EventArgs e)
        {
            InitHttpListener();
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            gbForm.Enabled = true;
            _SvcName = txtSvcNames.Text;
        }
        #endregion

        #region Private Method : void btnStop_Click(object sender, EventArgs e)
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_HttpListener != null)
            {
                _HttpListener.Stop();
                _HttpListener.Close();
                _HttpListener = null;
            }
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            gbForm.Enabled = false;
        }
        #endregion

        #region Private Method : void btnFile_Click(object sender, EventArgs e)
        private void btnFile_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            TextBox txt = (TextBox)btn.Parent.Controls.Find("txtFile" + btn.Name.Substring(btn.Name.Length - 1), false)[0];
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "所有檔案(*.*)|*.*";
                ofd.RestoreDirectory = true;
                ofd.AutoUpgradeEnabled = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.Multiselect = false;
                ofd.DefaultExt = ".zip";
                ofd.Title = "請選擇檔案";
                if (string.IsNullOrEmpty(txtFile1.Text))
                    ofd.FileName = string.Empty;
                else
                    ofd.FileName = Path.GetFileName(txtFile1.Text);
                DialogResult dr = ofd.ShowDialog(this);
                if (dr != System.Windows.Forms.DialogResult.OK)
                    return;
                txt.Text = ofd.FileName;
            }
        }
        #endregion

        #region Private Method : void InitHttpListener()
        private void InitHttpListener()
        {
            string prefix = string.Empty;
            _HttpListener = new HttpListener();
            if (cbCertServer.Text.Equals("https"))
            {
                if (txtIP.Text.Equals("0.0.0.0"))
                    prefix = $"{cbCertServer.Text}://*:{txtSslPort.Text}/";
                else
                    prefix = $"{cbCertServer.Text}://{txtIP.Text}:{txtSslPort.Text}/";
                _HttpListener.Prefixes.Add(prefix);
            }
            else
            {
                if (txtIP.Text.Equals("0.0.0.0"))
                    prefix = $"http://*:{txtPort.Text}/";
                else
                    prefix = $"http://{txtIP.Text}:{txtPort.Text}/";
                _HttpListener.Prefixes.Add(prefix);
            }
            _HttpListener.Start();
            _HttpListener.BeginGetContext(new AsyncCallback(WebRequestCallback), _HttpListener);
        }
        #endregion

        #region Private Method : NameValueCollection GetNameValueCollection()
        private NameValueCollection GetNameValueCollection()
        {
            NameValueCollection nvc = new NameValueCollection();
            TextBox txt = null;
            foreach (Control c in gbForm.Controls)
            {
                if (!c.GetType().Equals(typeof(TextBox))) continue;
                if (!c.Name.StartsWith("txtKey")) continue;
                if (string.IsNullOrEmpty(c.Text)) continue;
                txt = (TextBox)gbForm.Controls.Find("txtVal" + c.Name.Substring(c.Name.Length - 1), false)[0];
                nvc.Add(c.Text, txt.Text);
            }
            return nvc;
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

                CustHttpSvc http = new CustHttpSvc(ctx, _SvcName);
                http.OnPopupMessage += new CustHttpSvc.PopupMessageHandler(delegate (object sender, string msg)
                    {
                        Task.Factory.StartNew(() => WriteLog(Color.Blue, msg));
                    });
                //http.OnReciveAPI += new HttpService.APIHandler(HttpService_OnReciveAPI);
                http.ProcessRequest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region WriteLog

        #region Internal Method : void WriteLog(Color c, string text)
        delegate void WriteLogCallback(Color c, string txt);
        void WriteLog(Color c, string text)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() => WriteLog(c, text)));
            //this.Invoke(new WriteLogCallback(WriteLog), new object[] { c, text });
            else
            {
                int origStart = this.rtbLog.SelectionStart;
                this.rtbLog.SelectionColor = this.rtbLog.ForeColor;
                int last = this.rtbLog.TextLength;
                this.rtbLog.SelectionLength = 0;
                this.rtbLog.SelectionStart = last;
                this.rtbLog.SelectionColor = Color.Black;
                this.rtbLog.AppendText(string.Format("{0:HH\\:mm\\:ss.fff} - ", DateTime.Now));
                last = this.rtbLog.TextLength;
                this.rtbLog.SelectionStart = last;
                this.rtbLog.SelectionColor = c;
                Regex reg = new Regex("{(.[^{]+)}");
                Match m = reg.Match(text);
                if (!m.Success)
                    this.rtbLog.AppendText(text + "\n");
                else
                {
                    string t = string.Empty;
                    while (m.Success)
                    {
                        t = text.Substring(0, m.Index);
                        text = text.Substring(m.Index + m.Length);
                        this.rtbLog.AppendText(t);
                        this.rtbLog.SelectionStart = this.rtbLog.TextLength;
                        this.rtbLog.SelectionColor = ColorTranslator.FromHtml(m.Groups[1].Value);
                        m = reg.Match(text);
                    }
                    if (text.Length != 0)
                    {
                        this.rtbLog.AppendText(text);
                        text = string.Empty;
                    }
                    this.rtbLog.AppendText("\n");
                }
                this.rtbLog.SelectionColor = this.rtbLog.ForeColor;
                this.rtbLog.SelectionLength = 0;
                this.rtbLog.ScrollToCaret();
                Application.DoEvents();
            }
        }
        #endregion

        #region Internal Method : void WriteLog(Color c, string format, params object[] args)
        void WriteLog(Color c, string format, params object[] args)
        {
            WriteLog(c, string.Format(format, args));
        }
        #endregion

        void WriteLog(string txt)
        {
            WriteLog(SystemColors.WindowText, txt);
        }
        void WriteLog(string format, params object[] args)
        {
            WriteLog(string.Format(format, args));
        }
        #endregion

        #region Private Method : void btnWebClientUpload_Click(object sender, EventArgs e)
        private void btnWebClientUpload_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri(txtUrl.Text);
            // 準備檔案
            bool done = false;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                if (!string.IsNullOrEmpty(txtUserAgent.Text))
                    wc.Headers.Add(HttpRequestHeader.UserAgent, txtUserAgent.Text);
                else
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "WebClient");
                wc.UploadFileCompleted += new UploadFileCompletedEventHandler(delegate (object s, UploadFileCompletedEventArgs se)
                {
                    if (se.Error != null)
                    {
                        WriteLog(Color.Red, "# 發生錯誤!");
                        WriteLog(Color.Red, se.Error.Message);
                    }
                    else if (se.Cancelled)
                    {
                        WriteLog(Color.Brown, "# 被遠端伺服器取消");
                    }
                    else
                    {
                        WriteLog(Color.Green, "# 上傳完成");
                        WriteLog(Color.Green, "# 自伺服器收到：{0}", Encoding.UTF8.GetString(se.Result));
                    }
                    done = true;
                });
                wc.UploadProgressChanged += new UploadProgressChangedEventHandler(delegate (object s, UploadProgressChangedEventArgs se)
                {
                    pbPercentage.Value = se.ProgressPercentage;
                    Application.DoEvents();
                });
                wc.UploadFileAsync(uri, txtFile1.Text);
                DateTime now = DateTime.Now;
                while (!done && DateTime.Now.Subtract(now).TotalSeconds <= 60)
                    Application.DoEvents();
                pbPercentage.Value = 100;
            }
        }
        #endregion

        #region Private Method : void btnWebClientPost_Click(object sender, EventArgs e)
        private void btnWebClientPost_Click(object sender, EventArgs e)
        {
            NameValueCollection nvc = GetNameValueCollection();
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    if (!string.IsNullOrEmpty(txtUserAgent.Text))
                        wc.Headers.Add(HttpRequestHeader.UserAgent, txtUserAgent.Text);
                    else
                        wc.Headers.Add(HttpRequestHeader.UserAgent, "WebClient");
                    byte[] res = wc.UploadValues(txtUrl.Text, "POST", nvc);
                    WriteLog("# 資料已使用 POST 送出，伺服器回覆：{0}", Encoding.UTF8.GetString(res));
                }
            }
            catch (WebException ex)
            {
                WriteLog(Color.Red, "# 資料無法使用 POST 送出，原因：");
                WriteLog(Color.Red, "# {0}", ex.Message);
            }
            catch (HttpException ex)
            {
                WriteLog(Color.Red, "# 資料無法使用 POST 送出，原因：");
                WriteLog(Color.Red, "# {0}", ex.Message);
            }
        }
        #endregion

        #region Private Method : void btnWebClientGet_Click(object sender, EventArgs e)
        private void btnWebClientGet_Click(object sender, EventArgs e)
        {
            NameValueCollection nvc = GetNameValueCollection();
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    if (!string.IsNullOrEmpty(txtUserAgent.Text))
                        wc.Headers.Add(HttpRequestHeader.UserAgent, txtUserAgent.Text);
                    else
                        wc.Headers.Add(HttpRequestHeader.UserAgent, "WebClient");
                    byte[] res = wc.DownloadData(txtUrl.Text + CustHttpSvc.ToQueryString(nvc));
                    WriteLog("# 資料已使用 GET 送出，伺服器回覆：{0}", Encoding.UTF8.GetString(res));
                }
            }
            catch (WebException ex)
            {
                WriteLog(Color.Red, "# 資料無法使用 GET 送出，原因：");
                WriteLog(Color.Red, "# {0}", ex.Message);
            }
            catch (HttpException ex)
            {
                WriteLog(Color.Red, "# 資料無法使用 GET 送出，原因：");
                WriteLog(Color.Red, "# {0}", ex.Message);
            }
        }
        #endregion

        #region Private Method : void btnExtWebClientUpload_Click(object sender, EventArgs e)
        private void btnExtWebClientUpload_Click(object sender, EventArgs e)
        {
            Uri uri = new Uri(txtUrl.Text);
            // 準備檔案
            bool done = false;
            using (ExtWebClient wc = new ExtWebClient())
            {
                wc.Encoding = Encoding.UTF8;
                if (!string.IsNullOrEmpty(txtUserAgent.Text))
                    wc.Headers.Add(HttpRequestHeader.UserAgent, txtUserAgent.Text);
                else
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "ExtWebClient");
                wc.UploadMultiFilesCompleted += new EventHandler<UploadMultiFilesCompletedEventArgs>(delegate (object s, UploadMultiFilesCompletedEventArgs se)
                {
                    if (se.Result == UploadMultiFilesCompletedResult.Failed)
                    {
                        WriteLog(Color.Red, "# 發生錯誤, Status Code:{0}/{1}", (int)se.StatusCode, se.StatusCode);
                        if (se.Response != null && se.Response.Length != 0)
                            WriteLog(Color.Red, Encoding.UTF8.GetString(se.Response));
                    }
                    else
                    {
                        WriteLog(Color.Green, "# 上傳完成");
                        WriteLog(Color.Green, "# 自伺服器收到：{0}", Encoding.UTF8.GetString(se.Response));
                    }
                    done = true;
                });
                wc.UploadMultiFilesProgressChanged += new EventHandler<UploadMultiFilesProgressChangedEventArgs>(delegate (object s, UploadMultiFilesProgressChangedEventArgs se)
                {
                    this.Invoke(new MethodInvoker(() => { pbPercentage.Value = se.ProgressPercentage; Application.DoEvents(); }));
                });
                NameValueCollection nvc = GetNameValueCollection();
                List<ExtWebClient.FileData> files = new List<ExtWebClient.FileData>();
                CJF.Utility.CRC.Crc16 crc = new CJF.Utility.CRC.Crc16();
                if (!string.IsNullOrEmpty(txtFile1.Text) && File.Exists(txtFile1.Text))
                {
                    files.Add(new ExtWebClient.FileData()
                    {
                        ContentType = ConvUtils.GetContentType(txtFile1.Text),
                        FileName = txtFile1.Text,
                        KeyName = "File1"
                    });
                    nvc.Add("File1CRC", crc.ComputeHash(File.ReadAllBytes(txtFile1.Text)).ToHexString(""));
                }
                if (!string.IsNullOrEmpty(txtFile2.Text) && File.Exists(txtFile2.Text))
                {
                    files.Add(new ExtWebClient.FileData()
                    {
                        ContentType = ConvUtils.GetContentType(txtFile2.Text),
                        FileName = txtFile2.Text,
                        KeyName = "File2"
                    });
                    nvc.Add("File2CRC", crc.ComputeHash(File.ReadAllBytes(txtFile2.Text)).ToHexString(""));
                }
                wc.UploadMultiFilesAsync(uri, nvc, files.ToArray(), null);
                DateTime now = DateTime.Now;
                while (!done && DateTime.Now.Subtract(now).TotalSeconds <= 60)
                    Application.DoEvents();
            }
        }
        #endregion

        private void FHttpService_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnStop_Click(null, null);
        }

        private void cbCertClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtUrl.Text = cbCertClient.Text + Regex.Replace(txtUrl.Text, "http[s]?", "");
        }

        private void cbCertServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSslPort.Enabled = label22.Enabled = (cbCertServer.SelectedIndex == 1);
        }
    }

    #region Private Struct : SoapStruct
    public struct SoapStruct
    {
        public string Path;
        public NameValueCollection QueryString;
        public XmlDocument XmlContext;
    }
    #endregion

    public interface IApiService
    {
        void DoProcess(NameValueCollection nvc);
    }

    #region Public Class : SvcApiBase
    public class SvcApiBase : WebAPIBase, IWebAPI
    {
        /// <summary>取得 API 呼叫隨機關鍵索引值(InvokeID)</summary>
        public int InvokeID { get; internal set; }

        protected SvcApiBase() { }
        public SvcApiBase(XmlNode xml, HttpListenerContext ctx)
        {
            this.HttpCtx = ctx;
            this.SourceXml = xml.OuterXml;

            try
            {
                // 必填欄位
                //XmlNode xn = null;
                //xn = xml.SelectSingleNode("MDSID");
                //if (xn == null || string.IsNullOrEmpty(xn.InnerText))
                //    throw new APIMissingFieldException("MDSID");
                //else
                //    this.MDS_ID = xn.InnerText;
            }
            catch (APIFormatException ex) { throw ex; }
            catch (APIMissingFieldException ex) { throw ex; }
            catch (NullReferenceException nre) { throw new APIFormatException("Missing some tag!!", nre); }
            catch (Exception ex) { throw new APIFormatException("API Error!!", ex); }
        }

        #region Public override Method : string GetResultXml(string retCode)
        /// <summary>產生回傳代碼的 XML 回傳給終端</summary>
        /// <param name="retCode">回傳代碼</param>
        /// <returns></returns>
        public override string GetResultXml(string retCode)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><" + RootTagRes + "/>");
            XmlNode root = xml.DocumentElement;
            // ReturnCode
            XmlNode rtrnCodeNode = xml.CreateElement("ReturnCode");
            rtrnCodeNode.InnerText = retCode;
            root.AppendChild(rtrnCodeNode);
            string exp = string.Empty;
            switch (retCode)
            {
                case "0001": exp = "Not such account."; break;
                case "0002": exp = "This account has expired or the account can not use the API."; break;
                case "0003": exp = "Not allow IP"; break;
                case "0004": exp = "Cycle too often."; break;
            }
            if (!string.IsNullOrEmpty(exp))
            {
                XmlNode xn = xml.CreateElement("Exception");
                xn.InnerText = exp;
                root.AppendChild(xn);
            }
            return xml.OuterXml;
        }
        #endregion
    }
    #endregion

    [ServiceContract()]
    public class CustHttpSvc : HttpBase
    {
        #region event & delegate
        public delegate void APIHandler(object sender, IWebAPI webApi);
        public event APIHandler OnReciveAPI;
        public delegate void SOAPHandler(object sender, SoapStruct soap);
        public event SOAPHandler OnReceiveSOAP;
        public delegate void PopupMessageHandler(object sender, string msg);
        public event PopupMessageHandler OnPopupMessage;
        #endregion

        private readonly string[] _AllowSvcNames = null;

        #region Construct Method : HttpService(HttpListenerContext context, string allowSvc)
        public CustHttpSvc(HttpListenerContext context, string allowSvc)
        {
            this.Context = context;
            _AllowSvcNames = allowSvc.Split(';');
        }
        ~CustHttpSvc() { Dispose(false); }
        #endregion

        #region Protected Override Method : void ReceivedAPI(string svc, NameValueCollection nvc)
        protected override void ReceivedAPI(string svc, NameValueCollection nvc)
        {
            string result = string.Empty;
            SvcApiBase api = null;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(nvc["xml"]);

                XmlNode root = doc.DocumentElement;
                switch (svc)
                {
                    default:
                        ResponseServiceNotSupport();
                        break;
                }
                if (api != null && this.OnReciveAPI != null)
                {
                    foreach (APIHandler del in this.OnReciveAPI.GetInvocationList())
                    {
                        del.BeginInvoke(this, api, new AsyncCallback(WebAPICallback), del);
                    }
                }
            }
            #region Exceptions
            catch (APIFormatException ex)
            {
                ResponseAPIError(ex.Message);
            }
            catch (APIMissingFieldException ex)
            {
                ResponseAPIError(ex.Message);
            }
            catch (XmlException ex)
            {
                ResponseAPIError(ex.Message);
            }
            catch (Exception ex)
            {
                ResponseException(ex);
                return;
            }
            #endregion
        }
        #endregion

        #region Protected Override Method : void HttpGetMethod(string path, NameValueCollection queryString)
        protected override void HttpGetMethod(string path, NameValueCollection queryString)
        {
            try
            {
                string pageName = path.Replace("/", "\\").TrimEnd('\\');
                string[] seg = this.Context.Request.Url.Segments;
                string urlFolder = string.Empty, urlFile = string.Empty, localFile = string.Empty;
                if (string.IsNullOrEmpty(pageName) && seg.Length == 1 && seg[0].Equals("/"))
                {
                    RedirectURL("index.htm");
                    return;
                }
                else if (seg.Length >= 2)
                    urlFolder = seg[1].TrimEnd('/').ToLower();
                urlFile = seg[seg.Length - 1];
                localFile = Path.Combine(RootPath, pageName);
                string ip = this.Context.Request.RemoteEndPoint.Address.ToString();
                //_log.Write(LogManager.LogLevel.Debug, "{0} GET /{1}{2}", this.Context.Request.RemoteEndPoint, path, ToQueryString(queryString));
                WriteLog("* 伺服器收到來自 {0} 的 GET 要求", this.Context.Request.RemoteEndPoint);
                WriteLog("* Request Path={0}", path);
                WriteLog("* Query String={0}", ToQueryString(queryString));

                switch (urlFolder)
                {
                    default:
                        base.HttpGetMethod(path, queryString);
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
                ResponseException(ex);
            }
        }
        #endregion

        #region Protected Override Method : void HttpPostMethod(string path, NameValueCollection queryString)
        protected override void HttpPostMethod(string path, NameValueCollection queryString)
        {
            try
            {
                string ip = this.Context.Request.RemoteEndPoint.Address.ToString();
                HttpListenerRequest request = this.Context.Request;
                NameValueCollection nvc = null;
                this.ReceivedFiles = null;
                WriteLog("* 伺服器收到來自 {0} 的 POST 要求", this.Context.Request.RemoteEndPoint);
                WriteLog("* UserAgent   ={0}", request.UserAgent);
                WriteLog("* Request Path={0}", path);
                WriteLog("* Query String={0}", ToQueryString(queryString));
                WriteLog("* Content Type={0}", request.ContentType);
                switch (request.ContentType.Split(';')[0].ToLower())
                {
                    case "multipart/form-data":
                        #region multipart/form-data
                        {
                            base.PopulatePostMultiPart(request, out nvc);
                            string pageName = path.Replace("/", "\\").ToLower().TrimEnd('\\');
                            string[] seg = this.Context.Request.Url.Segments;
                            string svcName = string.Empty, last = string.Empty, file = string.Empty, msg = string.Empty;
                            if (seg.Length >= 2)
                                svcName = seg[1].TrimEnd('/').ToLower();
                            ushort c1 = 0, c2 = 0;
                            if (this.ReceivedFiles != null && this.ReceivedFiles.Count != 0)
                            {
                                for (int i = 0; i < this.ReceivedFiles.Count; i++)
                                {
                                    msg = string.Format("* Received Files[{0}]={1}, Key={2}, ContentType={3}, {4}bytes", i, this.ReceivedFiles[i].FileName, this.ReceivedFiles[i].FieldKey, this.ReceivedFiles[i].ContentType, this.ReceivedFiles[i].Length);
                                    if (!string.IsNullOrEmpty(nvc[string.Format("File{0}CRC", i + 1)]))
                                    {
                                        c1 = CJF.Utility.CRC.Crc16.Compute(File.ReadAllBytes(this.ReceivedFiles[i].FullPath));
                                        c2 = Convert.ToUInt16(nvc[string.Format("File{0}CRC", i + 1)], 16);
                                        msg += ", CRC is " + (c1 == c2 ? "Success" : "Fail");
                                    }
                                    WriteLog(msg);
                                }
                            }
                            WriteLog("* SvcName={0}", svcName);
                            if (nvc != null)
                            {
                                foreach (string k in nvc.AllKeys)
                                    WriteLog("* Key={0}, Value={1}", k, nvc[k].Replace("\r", "<CR>").Replace("\n", "<LF>"));
                            }
                            if (Array.IndexOf(_AllowSvcNames, svcName) != -1)
                                ResponseString("Success");
                            else
                                ResponseServiceNotSupport();
                            if (this.ReceivedFiles != null && this.ReceivedFiles.Count != 0)
                            {
                                foreach (ReceivedFileInfo rfi in this.ReceivedFiles)
                                {
                                    if (File.Exists(rfi.FullPath))
                                        File.Delete(rfi.FullPath);
                                }
                            }
                            break;
                        }
                    #endregion
                    default:
                        HttpPOST(path, queryString);
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteLog(" * 發生錯誤\n" + ex.Message);
            }
        }
        #endregion

        #region Private Method : void HttpPOST(string path, NameValueCollection queryString)
        private void HttpPOST(string path, NameValueCollection queryString)
        {
            try
            {
                HttpListenerRequest request = this.Context.Request;
                StreamReader reader = new StreamReader(request.InputStream);
                string data = reader.ReadToEnd();
                reader.Close();
                string ip = this.Context.Request.RemoteEndPoint.Address.ToString();

                // 檢查是否為 WebService 的 SOAP 格式
                Regex reg = new Regex("<soap(12)?:Envelope[\\s\\S]*</soap(12)?:Envelope>", RegexOptions.Multiline | RegexOptions.Singleline);
                if (reg.IsMatch(data))
                {
                    #region SOAP
                    //_log.Write(LogManager.LogLevel.Debug, "{0} POST(SOAP) /{1}{2} {3}", this.Context.Request.RemoteEndPoint, path, ToQueryString(queryString), this.Context.Request.UserAgent);
                    //_log.Write(LogManager.LogLevel.Debug, "POST(SOAP) Received:{0}", data.TrimEnd('\r', '\n'));

                    if (this.OnReceiveSOAP != null)
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.LoadXml(data);
                        SoapStruct soap = new SoapStruct()
                        {
                            Path = path,
                            QueryString = queryString,
                            XmlContext = xml
                        };
                        foreach (SOAPHandler del in this.OnReceiveSOAP.GetInvocationList())
                            del.BeginInvoke(this, soap, new AsyncCallback(SOAPCallback), del);
                    }
                    #endregion
                }
                else
                {
                    #region General HTTP POST
                    string pageName = path.Replace("/", "\\").ToLower().TrimEnd('\\');
                    string[] seg = this.Context.Request.Url.Segments;
                    string svcName = string.Empty, dev = string.Empty;
                    if (seg.Length >= 2)
                        svcName = seg[1].TrimEnd('/').ToLower();
                    dev = seg[seg.Length - 1];
                    //_log.Write(LogManager.LogLevel.Debug, "{1} POST {0}/{2}{3} {4}", svcName, this.Context.Request.RemoteEndPoint, path, ToQueryString(queryString), this.Context.Request.UserAgent);
                    //_log.Write(LogManager.LogLevel.Debug, "Data:{0}", data.Replace("\r", "<CR>").Replace("\n", "<LF>"));
                    NameValueCollection nvc = HttpUtility.ParseQueryString(data);
                    IApiService webSvc = null;
                    switch (svcName)
                    {
                        default:
                            ResponseNotFound();
                            break;
                    }
                    try
                    {
                        if (webSvc != null)
                            webSvc.DoProcess(nvc);
                    }
                    catch (NotSupportedException)
                    {
                        // 需再客制化->輸出特製頁面
                        ResponseServiceNotSupport();
                    }
                    catch (FileNotFoundException)
                    {
                        ResponseNotFound();
                    }
                    catch (Exception ex)
                    {
                        ResponseException(ex);
                    }
                    #endregion
                }
            }
            catch (HttpListenerException) { }
            catch (Exception ex)
            {
                //_log.Write(LogManager.LogLevel.Error, "HttpPOST:{0} ", this.Context.Request.Url.OriginalString);
                //_log.WriteException(ex);
                ResponseException(ex);
            }
        }
        #endregion

        #region Write Log
        private void WriteLog(string format, params object[] args)
        {
            WriteLog(string.Format(format, args));
        }

        private void WriteLog(string txt)
        {
            if (this.OnPopupMessage != null)
                this.OnPopupMessage.Invoke(this, txt);
        }
        #endregion

        #region Delegate Callback Methods
        private void WebAPICallback(IAsyncResult result)
        {
            APIHandler handler = (APIHandler)result.AsyncState;
            handler.EndInvoke(result);
        }

        private void SOAPCallback(IAsyncResult result)
        {
            SOAPHandler handler = (SOAPHandler)result.AsyncState;
            handler.EndInvoke(result);
        }
        #endregion
    }


}
